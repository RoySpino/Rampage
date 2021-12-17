﻿using rpgc.Syntax;
using rpgc.Text;
using rpgc.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    internal sealed class Lexer
    {
        bool doFreeLex = false;

        SourceText source;
        string tmpVal;
        int pos, lineNum, sSize, peekPos;
        char curChar;
        DiagnosticBag diagnostics = new DiagnosticBag();
        bool onEvalLine = true;
        List<SyntaxToken> strucLexLine = new List<SyntaxToken>();
        int parenCnt = 0, linePos;
        string lineType = "";
        bool doDecmiation;
        List<string> sourceLines = new List<string>();
        bool isProcSection = false, inDBlock = false;
        private string specChkStr;
        List<StructCard> lineFeeder = new List<StructCard>();
        string currentSub;
        int symStart;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int start;
        TokenKind kind;
        object Value;

        public Lexer(SourceText s)
        {
            source = s;
            pos = -1;
            linePos = 0;
            lineNum = 1;
            sSize = s.Length;
            doDecmiation = true;

            nextChar();
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private bool isGoodSpec(string spec, int line)
        {
            Dictionary<string, int> specVal2 = new Dictionary<string, int>() { { "CTL-OPT", 1 }, { "DCL-F", 2 }, { "DCL-S", 3 }, { "DCL-C", 3 }, { "DCL-DS", 3 },{ "END-DS", 3 }, { "DCL-PR", 3 }, { "END-PR", 3 },{ "DCL-PI", 3 }, { "END-PI", 3 },{ "C",4}, { "DCL-PROC", 5 },{ "END-PROC",5} };
            Dictionary<string, int> procSpec = new Dictionary<string, int>() { { "DCL-S", 3 }, { "DCL-C", 3 }, { "DCL-DS", 3 }, { "END-DS", 3 }, { "DCL-PR", 3 }, { "END-PR", 3 }, { "DCL-PI", 3 }, { "END-PI", 3 }, { "C", 4 }, { "DCL-PROC", 5 }, { "END-PROC", 5 } };
            Dictionary<string, int> mainDic = null;
            int tmpVal;
            string curSpec;

            curSpec = spec;

            // standardize dictionary
            if (isProcSection == false)
                mainDic = specVal2;
            else
                mainDic = procSpec;

            // invalid specification
            if (mainDic.ContainsKey(curSpec) == false)
            {
                if (inDBlock == true)
                    curSpec = "DCL-S";
                else
                    curSpec = "C";
            }

            // spec is the same 
            if (curSpec == specChkStr)
                return true;

            // within the main procedure AND spec are not the same 
            if (isProcSection == false)
            {
                if (mainDic[curSpec] >= mainDic[specChkStr])
                {
                    // start of procedure section reset to D and return true
                    if (curSpec == "DCL-PROC")
                    {
                        isProcSection = true;
                        specChkStr = "DCL-S";
                    }
                    else
                        specChkStr = curSpec;

                    return true;
                }
                return false;
            }
            else
            {
                // in procedure section AND spec are not the same
                if (mainDic[curSpec] >= mainDic[specChkStr])
                {
                    // end of procedure reset spec
                    if (curSpec == "DCL-PROC")
                        specChkStr = "DCL-S";
                    else
                        specChkStr = curSpec;

                    return true;
                }
                return false;
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void nextChar()
        {
            pos += 1;

            if (pos >= sSize)
                curChar = '\0';
            else
            {
                curChar = source[pos];

                if (curChar == 10)
                {
                    pos += 1;
                    lineNum += 1;
                    linePos = 0;
                    curChar = source[pos];
                }

                if (curChar > 31)
                    linePos += 1;
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private char peek(int offset)
        {
            int index;

            index = pos + offset;

            if (index >= sSize)
                return '\0';
            else
                return source[index];
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private TokenKind getAssignmentOrComparisonToken()
        {
            TokenKind ret;

            // if the [=] is inside a parethisies then its a comparison
            onEvalLine = (parenCnt == 0);

            // if the line started with a comparison keyword then its a comparison
            switch (lineType)
            {
                case "DOU":
                case "DOW":
                case "IF":
                case "WHEN":
                    onEvalLine = false;
                    break;
            }

            // check if the current line is a comparison or assignment
            if (onEvalLine == true)
            {
                // first = is an assignment all others are comparisons
                ret = TokenKind.TK_ASSIGN;
                onEvalLine = false;
            }
            else
                ret = TokenKind.TK_EQ;

            return ret;
        }
        // ////////////////////////////////////////////////////////////////////////////////////
        private TokenKind getLessGreaterThanOperator(char first, char op)
        {
            TokenKind ret;

            // check if the fisrt char is a Less than symbol
            if (first == '<')
                switch (op)
                {
                    case '>':
                        ret = TokenKind.TK_NE;
                        tmpVal = ("" + first + op);
                        nextChar();
                        break;
                    case '=':
                        ret = TokenKind.TK_LE;
                        tmpVal = ("" + first + op);
                        nextChar();
                        break;
                    default:
                        ret = TokenKind.TK_LT;
                        tmpVal = ("" + first);
                        break;
                }
            else
                // for greater than symbols
                switch (op)
                {
                    case '=':
                        ret = TokenKind.TK_GE;
                        tmpVal = ("" + first + op);
                        nextChar();
                        break;
                    default:
                        ret = TokenKind.TK_GT;
                        tmpVal = ("" + first);
                        break;
                }

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private bool chkOnBooleanLine(string symbol)
        {
            switch (symbol)
            {
                case "IF":
                case "WHEN":
                case "DOW":
                case "DOU":
                case "AND":
                case "OR":
                case "NOT":
                    return true;
                default:
                    return false;
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        public SyntaxToken doStructLex()
        {
            string line = "", sorc, tmp;
            int lineLength;
            string[] arr;
            MatchCollection mth;
            SyntaxToken tmpTok;
            bool Ft, Hv, Em;
            bool getAnotherCard;

            Ft = (doDecmiation == true);
            Hv = (strucLexLine.Count > 0);
            Em = (sourceLines.Count == 0);

            // If block only executes when
            // Ft: On the first time run or 
            // Hv: the list strucLexLine has elements (Has a value) or 
            // Em: sourceLines is not empty
            if (Ft == true || (Hv == false && Em == false))
            {
                // do this only once 
                // create a list of lines
                if (doDecmiation == true)
                {
                    doDecmiation = false;

                    sorc = Regex.Replace(source.ToString(), @"(\r\n|\n|\0)", "¶");
                    //sorc = sorc.Substring(0, sorc.Length - 1);
                    arr = sorc.Split('¶');

                    // save array as list
                    sourceLines = new List<string>(arr);

                    strucLexLine = Decimator.performCSpecVarDeclar(arr);
                }

                
                lineFeeder = new List<StructCard>();

                for (int i = 0; i < sourceLines.Count(); i++)
                {
                    // get a line and capatilize all letters but not the strings
                    line = SyntaxFacts.normalizeLine(sourceLines[i]);

                    // remove comments and add line to list
                    line = SyntaxFacts.normalizeComments(line);
                    lineFeeder.Add(new StructCard(line, (i + 1)));
                }

                // decimate line into factor strings
                strucLexLine = Decimator.doDecimation3(lineFeeder, ref diagnostics);
            }

            // pop the first element from the token list and return it
            tmpTok = strucLexLine[0];
            strucLexLine.RemoveAt(0);

            return tmpTok;
        }


        // ////////////////////////////////////////////////////////////////////////////////////
        public SyntaxToken doLex()
        {
            string symbol = "";

            kind = TokenKind.TK_BADTOKEN;
            Value = null;


            // -------------------------------------------------------------------------------------------------
            // check if using free format
            if (pos == 0 && curChar == '*' && peek(1) == '*')
            {
                start = pos;
                symStart = linePos;
                while (char.IsWhiteSpace(curChar) == false)
                {
                    symbol += curChar;
                    nextChar();
                }
                symbol = symbol.Trim().ToUpper();
                if (symbol == "**FREE")
                    doFreeLex = true;

                kind = TokenKind.TK_SPACE;
                Value = "";
            }

            // compile a traditinal RPG Program
            if (doFreeLex == false)
                return doStructLex();

            // -------------------------------------------------------------------------------------------------
            // c++ style comment line
            if (curChar == '/' && peek(1) == '/')
                ignoreCommentLine();

            // -------------------------------------------------------------------------------------------------
            switch (curChar)
            {
                case '\0':
                    start += 1;
                    symbol = "_";
                    kind = TokenKind.TK_EOI;
                    Value = "_";
                    break;
                case ';':
                    start += 1;
                    kind = TokenKind.TK_SEMI;
                    Value = ";";
                    onEvalLine = true;
                    nextChar();
                    break;
                case ':':
                    start += 1;
                    kind = TokenKind.TK_COLON;
                    Value = ":";
                    nextChar();
                    break;
                case '+':
                    start += 1;
                    kind = TokenKind.TK_ADD;
                    Value = "+";
                    nextChar();
                    break;
                case '-':
                    start += 1;
                    kind = TokenKind.TK_SUB;
                    Value = "-";
                    nextChar();
                    break;
                case '*':
                    start += 1;
                    symbol = readCompilerConstantsOrMult();
                    break;
                case '/':
                    start += 1;
                    kind = TokenKind.TK_DIV;
                    Value = "/";
                    nextChar();
                    break;
                case '(':
                    start += 1;
                    kind = TokenKind.TK_PARENOPEN;
                    Value = "(";
                    parenCnt += 1;
                    nextChar();
                    break;
                case ')':
                    start += 1;
                    kind = TokenKind.TK_PARENCLOSE;
                    Value = ")";
                    parenCnt -= 1;
                    nextChar();
                    break;
                case '=':
                    start += 1;
                    kind = getAssignmentOrComparisonToken();
                    Value = "=";
                    nextChar();
                    break;
                case '<':
                    start += 1;
                    kind = getLessGreaterThanOperator('<', peek(1));
                    Value = tmpVal;
                    nextChar();
                    break;
                case '>':
                    start += 1;
                    kind = getLessGreaterThanOperator('>', peek(1));
                    Value = tmpVal;
                    nextChar();
                    break;
                case '%':
                    symbol = readBuiltInFunctions();
                    break;
                case '@':
                case '#':
                case '$':
                    symbol = readIdentifierOrKeyword();
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    symbol = readNumberToken();
                    break;
                case ' ':
                case '\n':
                case '\r':
                    readWiteSpace();
                    break;
                case '\t':
                    readWiteSpace();
                    break;
                case '\'':
                    readString();
                    break;
                default:
                    if (char.IsLetter(curChar) == true)
                        Value = readIdentifierOrKeyword();
                    else
                    {
                        if (char.IsWhiteSpace(curChar) == true)
                        {
                            Value = "";
                            readWiteSpace();
                        }
                        else
                        {
                            diagnostics.reportBadCharacter(curChar, 1);
                            Value = "";
                            symbol = curChar.ToString();
                        }
                    }
                    break;
            }

            return new SyntaxToken(kind, lineNum, start, Value, symStart);
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void readString()
        {
            bool isInString;
            int charCnt;
            string text;

            // record current position and skip first single quoat
            start = pos;
            symStart = linePos;

            nextChar();

            isInString = true;
            charCnt = 0;
            text = "";

            while (isInString)
            {
                charCnt += 1;
                if (curChar == '\'' && peek(1) != '\'')
                    break;

                switch (curChar)
                {
                    case '\0':
                    case '\n':
                    case '\r':
                        diagnostics.reportBadString(new TextSpan(start, charCnt, lineNum, pos));
                        isInString = false;
                        break;
                    case '\'':
                        text += curChar;
                        nextChar();
                        break;
                    default:
                        text += curChar;
                        break;
                }

                nextChar();
            }

            nextChar();
            kind = TokenKind.TK_STRING;
            Value = text;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private string readNumberToken()
        {
            string symbol = "";
            int intDummy;

            start = pos;
            symStart = linePos;

            while (char.IsDigit(curChar) == true)
            {
                symbol += curChar;
                nextChar();
            }

            if (int.TryParse(symbol, out intDummy) == false)
                diagnostics.reportInvalidNumber(symbol, TypeSymbol.Integer, start, symbol.Length);

            kind = TokenKind.TK_INTEGER;
            Value = intDummy;

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void readWiteSpace()
        {
            string symbol = "";

            start = pos;
            symStart = linePos;

            // at end of line check for end/start of a block
            // otherwise set kind to space token
            if (lineType.Length > 0 && (curChar == 10 || curChar == 13))
            {
                // get and reset linetype
                getLineType(lineType);
                lineType = "";
            }
            else
                kind = TokenKind.TK_SPACE;

            // skip whitespace
            while (char.IsWhiteSpace(curChar) == true)
            {
                symbol += curChar;
                nextChar();
            }

            Value = "";
            start += 1;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private string readIdentifierOrKeyword()
        {
            string symbol = "";

            start = pos;
            symStart = linePos;
            while (char.IsLetterOrDigit(curChar) || curChar == '#' || curChar == '@' || curChar == '$' || curChar == '_')
            {
                sb.Append( curChar);
                nextChar();
            }

            symbol = sb.ToString();
            sb.Clear();
            symbol = symbol.ToUpper();

            // check if symbol is a type
            kind = SyntaxFacts.getRPGTypeFree(symbol);
            if (kind != TokenKind.TK_BADTOKEN)
                return symbol;

            // get any declaration keywords
            if ((symbol == "DCL" || symbol == "END") && peek(0) == '-')
                symbol = getDeclaration(symbol);

            // check if the symbol is a start/end of a block
            setLineType(symbol);

            // assign keyword token
            kind = SyntaxFacts.getKeywordKind(symbol);

            // check if symbol is a function name or identifier
            if (kind == TokenKind.TK_IDENTIFIER)
            {
                // set subroutine name
                subroutinesHandler(symbol);

                // check built in Functions
                if (SyntaxFacts.isValidFunction(symbol) == false)
                {
                    // chech if symbol is a valid variabel name
                    if (SyntaxFacts.isValidVariable(symbol))
                        Value = symbol;
                    else
                        kind = TokenKind.TK_BADTOKEN;
                }
            }
            else
            {
                // RPG does not support Subroutine recursion
                // report it as an error
                switch (kind)
                {
                    case TokenKind.TK_BEGSR:
                        subroutinesHandler();
                        break;
                    case TokenKind.TK_ENDSR:
                        currentSub = "";
                        break;
                    case TokenKind.TK_EXSR:
                        subroutinesHandler();
                        break;
                }

                // this prevents [=] being treeted as assignment when only the 
                // first is an assignment all others are comparisons
                onEvalLine = chkOnBooleanLine(symbol);
            }

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private string readBuiltInFunctions()
        {
            string symbol = "";

            start = pos;
            symStart = linePos;
            symbol = "%";

            nextChar();
            while (char.IsLetter(curChar) == true)
            {
                symbol += curChar;
                nextChar();
            }

            Value = symbol.Trim().ToUpper();
            kind = SyntaxFacts.getBuiltInFunction(Value.ToString());

            return symbol;
        }


        // ////////////////////////////////////////////////////////////////////////////////////
        private string getDeclaration(string declar)
        {
            string ret;
            char peekchar;
            int pidx = 0;

            // get declaration symbol
            ret = declar;
            peekchar = peek(pidx);
            while (char.IsLetter(peekchar) || peekchar == 45)
            {
                ret += peekchar;
                pidx += 1;
                peekchar = peek(pidx);
            }

            ret = ret.ToUpper();

            // check if symbol is a block/block terminator
            switch (ret)
            {
                case "DCL-PROC":
                case "BEGSR":
                    kind = TokenKind.TK_PROCDCL;
                    break;
                case "DCL-PR":
                    kind = TokenKind.TK_VARDDATAS;
                    break;
                case "DCL-PI":
                    kind = TokenKind.TK_PROCINFC;
                    break;
                case "DCL-DS":
                    kind = TokenKind.TK_VARDDATAS;
                    break;
                case "DCL-S":
                    kind = TokenKind.TK_VARDECLR;
                    break;
                case "DCL-C":
                    kind = TokenKind.TK_VARDCONST;
                    break;
                case "END-PROC":
                case "ENDSR":
                    kind = TokenKind.TK_PROCEND;
                    break;
                case "END-PI":
                    kind = TokenKind.TK_ENDPI;
                    break;
            }

            // if the symbol is not a declaration return the original symbol
            // otherwise return the declaration symbol
            if (kind == TokenKind.TK_IDENTIFIER)
                return declar;
            else
                for (int i = 0; i < pidx; i++)
                    nextChar();

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        // special indicators and constants defalts to mult if nothing is found
        private string readCompilerConstantsOrMult()
        {
            string symbol, peekStr;
            char peekChar;

            start = pos;
            symStart = linePos;
            peekPos = 0;
            symbol = "*";
            peekStr = "";

            while (true)
            {
                peekChar = peek(peekPos);
                peekPos += 1;

                if (SyntaxFacts.isCharLiteralOrControl(peekChar) == false  && peekChar != '*')
                    break;

                peekStr += peekChar;
            }

            peekStr = peekStr.ToUpper();
            kind = SyntaxFacts.getBuiltInIndicator(peekStr);


            if (kind != TokenKind.TK_BADTOKEN)
            {
                for (int i=0; i< peekStr.Length; i++)
                {
                    nextChar();
                    start += 1;
                }

                Value = peekStr;
                return peekStr;
            }
            else
            {
                nextChar();
                kind = TokenKind.TK_MULT;
                Value = "*";
                start += 1;
            }

            kind = TokenKind.TK_MULT;
            return peekStr;
        }
        // ////////////////////////////////////////////////////////////////////////////////////
        private void ignoreCommentLine()
        {
            start = pos;
            symStart = linePos;
            while (true)
            {
                nextChar();
                if (curChar == '\n' || curChar == '\0')
                    break;
            }

            kind = TokenKind.TK_SPACE;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void subroutinesHandler(string identifier = "")
        {
            // first pass BEGSR token received
            if (String.IsNullOrEmpty(currentSub) == true && identifier == "")
            {
                currentSub = "^^";
                return;
            }

            // second pass IDENTIFIER received this is the name of the subroutine
            if(currentSub == "^^" && identifier != "")
            {
                currentSub = identifier;
                return;
            }

            // third pass EXECSR recived
            if (String.IsNullOrEmpty(currentSub) == false && currentSub == identifier)
                kind = TokenKind.TK_BADTOKEN;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void getLineType(string symbol)
        {
            // if there is a lineType set a
            switch (symbol)
            {
                //case "DCL-PROC":
                //case "BEGSR":
                case "DOU":
                case "DOW":
                case "ELSE":
                case "FOR":
                case "IF":
                case "MON":
                    kind = TokenKind.TK_BLOCKSTART;
                    return;
                case "ENDDO":
                case "ENDFOR":
                case "ENDIF":
                case "ENDMON":
                    kind = TokenKind.TK_BLOCKEND;
                    return;
            }

            kind = TokenKind.TK_SPACE;
            return;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void setLineType(string symbol)
        {
            if (lineType.Length == 0)
            {
                switch (symbol)
                {
                    case "BEGSR":
                    case "DCL-PROC":
                    case "DOU":
                    case "DOW":
                    case "FOR":
                    case "IF":
                    case "MON":
                    case "MONITOR":
                    case "ELSE":
                        lineType = symbol;
                        break;
                }
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        public DiagnosticBag getDiagnostics()
        {
            return diagnostics;
        }
    }
}
