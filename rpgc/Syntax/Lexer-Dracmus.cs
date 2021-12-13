using rpgc.Syntax;
using rpgc.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    internal sealed class Lexer
    {
        SourceText source;
        string tmpVal;
        int pos, linePos, lineNum, sSize, peekPos;
        char curChar, curSpec;
        DiagnosticBag diagnostics = new DiagnosticBag();
        bool onEvalLine = true, doFreeLex = true;
        List<StructNode> lineElem = null;
        List<SyntaxToken> strucLexLine = new List<SyntaxToken>();
        int parenCnt = 0;
        string lineType = "";

        int start;
        TokenKind kind;
        object Value;

        public Lexer(SourceText s)
        {
            source = s;
            pos = -1;
            linePos = 0;
            curSpec = 'H';
            lineNum = 1;
            sSize = s.Length;

            nextChar();
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

                if (curChar == '\n')
                {
                    pos += 1;
                    lineNum += 1;
                    linePos = 1;
                    curChar = source[pos];
                }

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
                case "FOR":
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
            string line = "", symbol;
            bool onElipsis = false, atEOF = false;
            int column = 1;
            SyntaxToken tmpTok = null;

            if (linePos == 1 && strucLexLine.Count == 0)
            {
                while (true)
                {
                    line += curChar;
                    atEOF = (curChar == 0);

                    // skip traditinal RPG comment line and exit function
                    if (column == 2 && curChar == '*')
                    {
                        while (curChar > 15)
                            nextChar();

                        nextChar();
                        return new SyntaxToken(TokenKind.TK_SPACE, lineNum, (pos + 1), "");
                    }

                    // at end of line, slice string into factor componets
                    // then exit loop
                    if (curChar <= 15 && line.Length > 2)
                    {
                        line = line.ToUpper().TrimEnd();
                        line = line.Replace("\0", "");
                        line = line.Replace("\r", "");
                        line = line.Replace("\n", "");

                        // remove all chars on c style comments
                        if (line.Contains("//") == true)
                            line = line.Remove(line.IndexOf("//"));

                        // nothing entered return nothing
                        if (line == "")
                            return new SyntaxToken(TokenKind.TK_SPACE, lineNum, (pos + 1), "");

                        // get current spec character for this line
                        curSpec = line[0];

                        // decimate line into factor strings
                        lineElem = Decimator.doDecimation(lineNum, line);

                        // reset line and column counts
                        line = "";
                        column = 1;
                        break;
                    }

                    nextChar();
                    column += 1;
                }

                // bad specification found
                if (lineElem == null)
                {
                    diagnostics.reportBadSpec(curChar, 1);
                    return new SyntaxToken(TokenKind.TK_BADTOKEN, lineNum, 1, line);
                }

                // nothing was entered on the line
                if (lineElem.Count == 0)
                    return new SyntaxToken(TokenKind.TK_SPACE, lineNum, 1, line);

                // get keyword symbol and check if on a evaluation line
                if (lineElem.Count >= 5)
                {
                    symbol = lineElem[4].symbol;
                    onEvalLine = (symbol.Contains("EVAL") || symbol == "IF" || symbol == "FOR" ||
                                  symbol == "WHEN" || symbol == "CALLP" || symbol == "DOW" || symbol == "DOU");
                }
                else
                    onEvalLine = false;

                // -------------------------------------------------------------------------------------------------
                // lex structured line
                if (lineElem != null)
                {
                    switch (curSpec)
                    {
                        case 'D':
                            strucLexLine.Add(SyntaxFacts.getIdentifierToken(lineElem[0]));
                            break;
                        case 'C':
                            for (int i = 0; i < lineElem.Count; i++)
                            {
                                if (onEvalLine == false)
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            strucLexLine.Add(SyntaxFacts.getColum2Kyes(lineElem[0]));
                                            break;
                                        case 1:
                                            strucLexLine.Add(SyntaxFacts.getColum3Kyes(lineElem[1]));
                                            break;
                                        case 4:
                                            strucLexLine.AddRange(Decimator.cSpecRectifier(lineElem, ref diagnostics));
                                            break;
                                        case 7:
                                            break;
                                        case 8:
                                            break;
                                        case 2:
                                        case 9:
                                        case 10:
                                        case 11:
                                            strucLexLine.Add(SyntaxFacts.twoDigitIndicators(lineElem[i]));
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (i)
                                    {
                                        case 0:
                                            strucLexLine.Add(SyntaxFacts.getColum2Kyes(lineElem[0]));
                                            break;
                                        case 1:
                                            strucLexLine.Add(SyntaxFacts.getColum3Kyes(lineElem[1]));
                                            break;
                                        case 2:
                                            strucLexLine.Add(SyntaxFacts.twoDigitIndicators(lineElem[i]));
                                            break;
                                        case 3:
                                            strucLexLine.Add(SyntaxFacts.getKeywordToken(lineElem[4]));
                                            break;
                                        case 5:
                                            strucLexLine.AddRange(Decimator.doLex(lineElem[5], lineElem[5].symbol));
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            // add end of file token
            if (atEOF == true)
                strucLexLine.Add(new SyntaxToken(TokenKind.TK_EOI, lineNum, pos, "_"));

            if (linePos != 1 && strucLexLine.Count == 0)
                strucLexLine.Add(new SyntaxToken(TokenKind.TK_EOI, lineNum, pos, ""));

            // pop the first element from the token list and return it
            tmpTok = strucLexLine[0];
            strucLexLine.RemoveAt(0);
            return tmpTok;
        }


        // ////////////////////////////////////////////////////////////////////////////////////
        public SyntaxToken doLex()
        {
            string symbol = "", text;
            int intDummy;
            string peekStr;
            char peekChar;
            SyntaxToken tmpTok = null;

            kind = TokenKind.TK_BADTOKEN;
            Value = null;

            // compile a traditinal RPG Program
            if (doFreeLex == false)
                return doStructLex();

            // -------------------------------------------------------------------------------------------------
            // check if using free format
            if (pos == 0 && curChar == '*' && peek(1) == '*')
            {
                start = pos;
                while (char.IsWhiteSpace(curChar) == false)
                {
                    symbol += curChar;
                    nextChar();
                }
                symbol = symbol.ToUpper();
                if (symbol != "**FREE")
                {
                    doStructLex();
                    doFreeLex = false;
                }
                kind = TokenKind.TK_SPACE;
                Value = "";
            }

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
                case '\t':
                case '\r':
                    readWiteSpace();
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

            //text = SyntaxFacts.getText(kind);
            if (symbol == null)
                symbol = "";

            return new SyntaxToken(kind, lineNum, start, Value);
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private string readNumberToken()
        {
            string symbol = "";
            int intDummy;

            start = pos;

            while (char.IsDigit(curChar) == true)
            {
                symbol += curChar;
                nextChar();
            }

            if (int.TryParse(symbol, out intDummy) == false)
                diagnostics.reportInvalidNumber(symbol, typeof(int), start, symbol.Length);

            kind = TokenKind.TK_INTEGER;
            Value = intDummy;

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void readWiteSpace()
        {
            string symbol = "";

            start = pos;

            // at end of line check for end/start of a block
            // otherwise set kind to space token
            if (lineType.Length > 0 && (curChar == 10 || curChar == 13)
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
            while (char.IsLetter(curChar) || (symbol.Length > 1 && char.IsLetterOrDigit(curChar)) ||
                   curChar == '#' || curChar == '@' || curChar == '$' || (symbol.Length > 1 && curChar == '_'))
            {
                symbol += curChar;
                nextChar();
            }

            symbol = symbol.ToUpper();

            // get any declaration keywords
            if ((symbol == "DCL" || symbol == "END") && peek(0) == '-')
                symbol = getDeclaration(symbol);

            // check if the symbol is a start/end of a block
            setLineType(symbol);

            // assign keyword token
            switch (symbol)
            {
                case "END-PROC":
                case "ENDDO":
                case "ENDFOR":
                case "ENDIF":
                case "ENDSR":
                    kind = TokenKind.TK_BLOCKEND;
                    break;
                default:
                    kind = SyntaxFacts.getKeywordKind(symbol);
                    break;
            }

            // chech if symbol is a keyword
            if (kind == TokenKind.TK_IDENTIFIER)
            {
                // chech if symbol is a valid variabel name
                if (SyntaxFacts.isValidVariable(symbol))
                {
                    Value = symbol;
                    kind = TokenKind.TK_IDENTIFIER;
                    start += 1;
                }
            }
            else
                onEvalLine = chkOnBooleanLine(symbol);

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
            while (peekchar > 32)
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
                    kind = TokenKind.TK_BLOCKSTART;
                    break;
                case "DCL-PR":
                    kind = TokenKind.TK_VARDDATAS;
                    break;
                case "DCL-PI":
                    kind = TokenKind.TK_VARDDATAS;
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
                    kind = TokenKind.TK_BLOCKEND;
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
            peekPos = 0;
            symbol = "*";
            peekStr = "";

            // check if the word is an indicator by checking the first 2 chars
            nextChar();
            peekChar = curChar;
            while (peekChar > 32)
            {
                peekStr += peekChar;
                peekPos += 1;
                peekChar = peek(peekPos);
            }
            symbol = ("*" + peekStr);
            Value = symbol;
            peekStr = ("*" + peekStr.ToUpper());

            // a single * as passed 
            if (symbol == "*")
            {
                nextChar();
                kind = TokenKind.TK_MULT;
                return "*";
            }

            if (symbol == "**")
            {
                // exponental
            }

            // check if the symbol is an indicator
            kind = SyntaxFacts.getBuiltInIndicator(peekStr);
            if (kind != TokenKind.TK_BADTOKEN)
            {
                nextChar();
                while (char.IsLetterOrDigit(curChar) == true)
                    nextChar();
                start += 1;

                return symbol;
            }

            // check compiler constants
            kind = SyntaxFacts.getCompilerConstans(peekStr);
            if (kind != TokenKind.TK_BADTOKEN)
            {
                nextChar();
                while (char.IsLetterOrDigit(curChar) == true)
                    nextChar();
                start += 1;

                return symbol;
            }

            kind = TokenKind.TK_MULT;
            return "*";
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private string readBuiltInFunctions()
        {
            string symbol = "";

            start = pos;
            symbol = "%";

            nextChar();
            while (char.IsLetter(curChar) == true)
            {
                symbol += curChar;
                nextChar();
            }

            kind = SyntaxFacts.getBuiltInFunction(symbol.ToUpper());
            Value = symbol;
            start += 1;

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void ignoreCommentLine()
        {
            start = pos;
            while (true)
            {
                nextChar();
                if (curChar == '\n' || curChar == '\0')
                    break;
            }
            start += 1;

            kind = TokenKind.TK_SPACE;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void getLineType(string symbol)
        {
            // if there is a lineType set a
            switch (symbol)
            {
                case "BEGSR":
                case "DCL-PROC":
                case "DOU":
                case "DOW":
                case "ELSE":
                case "FOR":
                case "IF":
                case "MON":
                    kind = TokenKind.TK_BLOCKSTART;
                    break;
                case "END-PROC":
                case "ENDSR":
                case "ENDDO":
                case "ENDFOR":
                case "ENDIF":
                case "ENDMON":
                    kind = TokenKind.TK_BLOCKEND;
                    break;
            }
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
                    case "ELSE":
                    case "END-PROC":
                    case "ENDSR":
                    case "ENDDO":
                    case "ENDFOR":
                    case "ENDIF":
                    case "ENDMON":
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
