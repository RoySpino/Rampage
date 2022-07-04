using rpgc.Syntax;
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
        bool onEvalLine = true, doAddMainFunciton, doAddMainProcEnd, doAddMainProcSrt, onStartCSpec, onEndCSpec;
        List<SyntaxToken> strucLexLine = new List<SyntaxToken>();
        int parenCnt = 0, linePos;
        string lineType = "";
        bool doDecmiation;
        List<string> sourceLines = new List<string>();
        bool isProcSection = false, isInDBLock;
        private string specChkStr;
        List<StructCard> lineFeeder = new List<StructCard>();
        string currentSub;
        int symStart, prevLine;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        private SyntaxTree _SyntaxTree;
        private int originalSrucLinesCount;
        private List<SyntaxToken> MainProcedureInterface = new List<SyntaxToken>();
        private Regex declarBlkEnd = new Regex(@"(?i)^end-(pi|pr|ds)", RegexOptions.IgnoreCase);
        private bool onSelectStart;
        string curSpec, lstSpec;

        int start;
        TokenKind kind;
        object Value;

        public Lexer(SyntaxTree stree)
        {
            _SyntaxTree = stree;
            source = stree.TEXT;
            sSize = source.Length;
            doDecmiation = true;
            doAddMainFunciton = true;
            doAddMainProcEnd = false;
            doAddMainProcSrt = false;
            onStartCSpec = false;
            pos = -1;
            linePos = 0;
            lineNum = 1;
            prevLine = -1;
            lstSpec = "";
            specChkStr = "CTL-OPT";
            onSelectStart = true;



            nextChar();
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private bool isGoodSpec(string spec, int line)
        {
            Dictionary<string, int> specVal2 = new Dictionary<string, int>() { { "CTL-OPT", 1 }, { "DCL-F", 2 }, { "DCL-S", 3 }, { "DCL-C", 3 }, { "DCL-DS", 3 }, { "END-DS", 3 }, { "DCL-PR", 3 }, { "END-PR", 3 }, { "DCL-PI", 3 }, { "END-PI", 3 }, { "¢", 4 }, { "DCL-PROC", 5 }, { "END-PROC", 5 } };
            Dictionary<string, int> procSpec = new Dictionary<string, int>() { { "DCL-S", 3 }, { "DCL-C", 3 }, { "DCL-DS", 3 }, { "END-DS", 3 }, { "DCL-PR", 3 }, { "END-PR", 3 }, { "DCL-PI", 3 }, { "END-PI", 3 }, { "¢", 4 }, { "DCL-PROC", 5 }, { "END-PROC", 5 } };
            Dictionary<string, int> mainDic = null;

            curSpec = spec;

            // standardize dictionary
            if (isProcSection == false)
                mainDic = specVal2;
            else
                mainDic = procSpec;

            // set D block flag
            if (SyntaxFacts.isInDBlock(spec) == true)
            {
                isInDBLock = true;
             
                if (declarBlkEnd.Match(spec).Success == true)
                    isInDBLock = false;
            }

            // invalid specification found
            // assign D or C spec
            if (mainDic.ContainsKey(curSpec) == false)
            {
                curSpec = "¢";

                if (isInDBLock == true)
                    curSpec = "DCL-S";
            }

            // set C-spec flag (ON START)
            if (curSpec == "¢" && lstSpec != "¢")
                onStartCSpec = true;
            // set C-spec flag (ON END)
            if (curSpec != "¢" && lstSpec == "¢")
                onEndCSpec = true;

            // save the last spec
            if (curSpec != lstSpec)
                lstSpec = curSpec;

            // spec is the same 
            if (curSpec == specChkStr)
                return true;

            // within the main procedure AND spec are not the same 
            if (isProcSection == false)
            {
                // add hidden procedure declaration line
                if (doAddMainFunciton == true)
                {
                    /*
                    // start of the C specificaton
                    // first line is a main funciton
                    if (mainDic[curSpec] == 4 && mainDic[specChkStr] == 3)
                        doAddMainProcSrt = true;

                    // at the end of the C spec and starting O or P
                    // compleate the end procedure
                    if (mainDic[curSpec] > 4 && mainDic[specChkStr] == 4)
                        doAddMainProcEnd = true;
                    */
                }

                // compute line spec
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
        private bool specCheckHelper(int lineNum, string spec)
        {
            bool result;

            if (lineNum == prevLine)
                return true;
            else
            {
                result = isGoodSpec(spec, lineNum);
                prevLine = lineNum;
            }

            return result;
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
        private bool checkFree()
        {
            string vlu;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            char chr = '^';
            int idx = 0;

            // -------------------------------------------------------------------------------------------------
            // check if using free format
            if (pos == 0 && curChar == '*' && peek(1) == '*')
            {
                // get the symbol
                while (chr >= ' ')
                {
                    chr = peek(idx);
                    sb.Append(chr);
                    idx += 1;
                }

                // convert symbol to upercase string
                vlu = sb.ToString().ToUpper().Trim();

                // check if result is the FREE simble
                if (vlu == "**FREE")
                {
                    // advanc character position
                    for (int i = 0; i < 6; i++)
                        nextChar();

                    // goto next instruction
                    while (peek(0) < 33)
                        nextChar();

                    return true;
                }
            }

            // FREE symbol was not found
            return false;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        public SyntaxToken doLex()
        {
            string symbol = "";
            TextLocation location;
            TextSpan tmpTS;

            kind = TokenKind.TK_BADTOKEN;
            Value = null;








            // -------------------------------------------------------------------------------------------------
            // c++ style comment line
            if (curChar == '/' && peek(1) == '/')
            {
                ignoreCommentLine();
                return new SyntaxToken(_SyntaxTree, kind, 0, 0, "", symStart);
            }

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
                    readCompilerConstantsOrMult();
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
                case '.':
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
                            tmpTS = new TextSpan(pos, 1, lineNum, linePos);
                            location = new TextLocation(source, tmpTS);
                            diagnostics.reportBadCharacter(location, curChar, 1);
                            Value = "";
                            symbol = curChar.ToString();
                            nextChar();
                        }
                    }
                    break;
            }

            return new SyntaxToken(_SyntaxTree, kind, lineNum, start, Value, symStart);
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void readString()
        {
            bool isInString;
            int charCnt;
            string text;
            TextLocation location;

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
                        location = new TextLocation(source, new TextSpan(start, 1, lineNum, linePos));
                        diagnostics.reportBadString(location);
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
            double doubleDummy;
            TextLocation location;

            start = pos;
            symStart = linePos;

            while (char.IsDigit(curChar) == true || curChar == '.')
            {
                symbol += curChar;
                nextChar();
            }

            // try to convert string to a number
            // on bad number return bad token
            if (double.TryParse(symbol, out doubleDummy) == false)
            {
                location = new TextLocation(source, new TextSpan(start, 1, lineNum, linePos));
                diagnostics.reportInvalidNumber(location, symbol, TypeSymbol.Integer, start, symbol.Length);
                kind = TokenKind.TK_BADTOKEN;
                return symbol;
            }


            // if the symbol is good check for a [.]
            // when found return FLOAT otherwise INT
            if (symbol.Contains("."))
            {
                kind = TokenKind.TK_FLOAT;
                Value = doubleDummy;
            }
            else
            {
                kind = TokenKind.TK_INTEGER;
                Value = (int)doubleDummy;
            }

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
            if (String.IsNullOrEmpty(lineType) == false  && (curChar == 10 || curChar == 13))
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
            bool isGoodSpecResult = false;
            TextLocation location;

            start = pos;
            symStart = linePos;
            while (char.IsLetterOrDigit(curChar) || curChar == '#' || curChar == '@' || curChar == '$' || curChar == '_')
            {
                sb.Append(curChar);
                nextChar();
            }

            symbol = sb.ToString();
            sb.Clear();
            symbol = symbol.ToUpper();

            // check if symbol is a type
            //kind = SyntaxFacts.getRPGTypeFree(symbol);
            //if (kind != TokenKind.TK_BADTOKEN)
            //    return symbol;

            // get any declaration keywords
            if (((symbol == "DCL" || symbol == "END") && peek(0) == '-') || (symbol == "BEGSR" || symbol == "ENDSR"))
                symbol = getDeclaration(symbol);

            // handl H spec
            // line check is to ensure that this is the first symbol on the line
            if (symbol == "CTL" && peek(0) == '-' && lineNum != prevLine)
            {
                prevLine = lineNum;
                handleHSpec();
                kind = TokenKind.TK_SPACE;
                return "";
            }
            
            // check spec order
            isGoodSpecResult = specCheckHelper(lineNum, symbol);
            if (isGoodSpecResult == false)
            {
                location = new TextLocation(source, new TextSpan(start, 1, lineNum, linePos));
                diagnostics.reportWrongSpecLoc(location, symbol, specChkStr, lineNum, linePos);
            }

            // check if the symbol is a start/end of a block
            setLineType(symbol);

            // assign keyword token
            kind = SyntaxFacts.getFreeFormatKind(symbol, true);

            // check if symbol is a function name or identifier
            if (kind == TokenKind.TK_IDENTIFIER)
            {
                // set subroutine name
                //subroutinesHandler(symbol);

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
                // this prevents [=] being treeted as assignment when only the 
                // first is an assignment all others are comparisons
                onEvalLine = chkOnBooleanLine(symbol);
            }

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void handleHSpec()
        {
            string line;
            string[] larr;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // read the rest of the key word
            while (curChar > 32)
            {
                sb.Append(curChar);
                nextChar();
            }

            // check if the line is an h spec line
            // if not report an error
            line = $"CTL{sb.ToString().ToUpper()}";
            if (line != "CTL-OPT")
                diagnostics.reportWrongSpecLoc(_SyntaxTree.ROOT.Location(), line, "CTL-OPT", lineNum, linePos);

            sb = new System.Text.StringBuilder();

            // read h spec line to the end
            while (true)
            {
                nextChar();

                if (curChar == ';' || curChar < 31)
                    break;
                
                sb.Append(curChar);
            }

            // consume [;]
            nextChar();

            // split string into words
            line = sb.ToString().ToUpper().Trim();
            larr = line.Split(' ');
            
            // change compiler settings
            foreach(string itm in larr)
            {
                if (itm == "NOMAIN")
                    doAddMainFunciton = false;
            }
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
                case "END-PR":
                    kind = TokenKind.TK_ENDPR;
                    break;
                case "END-DS":
                    kind = TokenKind.TK_ENDDS;
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

                symbol = SyntaxFacts.getCompilerConstansLiteral(peekStr);
                if (symbol == null)
                    symbol = peekStr;

                Value = symbol;

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
                if (curChar <= 20)
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
        private void subroutinesHandler2(string identifier = "")
        {
            if (String.IsNullOrEmpty(currentSub) == false && currentSub == identifier)
                kind = TokenKind.TK_BADTOKEN;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void setCurSubrutine(string curScope = "")
        {
            currentSub = curScope;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private void getLineType(string symbol)
        {
            // if there is a lineType set a
            switch (symbol)
            {
                case "DOU":
                case "DOW":
                case "ELSE":
                case "FOR":
                case "IF":
                case "MON":
                case "WHEN":
                case "OTHER":
                    kind = TokenKind.TK_BLOCKSTART;
                    return;
                case "ENDDO":
                case "ENDFOR":
                case "ENDIF":
                case "ENDMON":
                case "ENDSR":
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
                    case "WHEN":
                    case "OTHER":
                        lineType = symbol;
                        break;
                }
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        public List<SyntaxToken> getLexTokenList()
        {
            SyntaxToken tok;
            List<SyntaxToken> ret;

            ret = new List<SyntaxToken>();

            // check if user has writen a fixed format program 
            if (checkFree() == false)
            {
                var xtn = doStructLex2();
                return xtn;
            }

            // get tokens
            do
            {
                // get token
                tok = doLex();

                // add hidden main procedure
                if (doAddMainFunciton == true)
                {
                    if (onStartCSpec == true)
                    {
                        onStartCSpec = false;
                        ret.AddRange(SyntaxFacts.prepareMainFunction(_SyntaxTree, TokenKind.TK_SEMI, true));
                    }
                    if (onEndCSpec == true )
                    {
                        // at end stop adding main funciton
                        doAddMainFunciton = false;
                        ret.AddRange(SyntaxFacts.prepareMainFunction(_SyntaxTree, TokenKind.TK_SEMI, false));
                    }
                }

                // add extra tokens before the lexed token
                switch (tok.kind)
                {
                    case TokenKind.TK_ELSE:
                        // special case for else
                        // add block end before else
                        ret.Add(new SyntaxToken(_SyntaxTree, TokenKind.TK_ENDIF, tok.line, tok.pos, ""));
                        break;
                    case TokenKind.TK_ENDSL:
                        ret.Add(new SyntaxToken(_SyntaxTree, TokenKind.TK_BLOCKEND, tok.line, tok.pos, ""));
                        break;
                    case TokenKind.TK_WHEN:
                    case TokenKind.TK_OTHER:
                        if (onSelectStart == false)
                            ret.Add(new SyntaxToken(_SyntaxTree, TokenKind.TK_BLOCKEND, tok.line, tok.pos, ""));
                        else
                            onSelectStart = false;
                        break;
                    case TokenKind.TK_SELECT:
                        // reset when block
                        onSelectStart = true;
                        break;
                }

                // save avalable tokens skipping nulls and space
                if (tok == null || tok.kind == TokenKind.TK_SPACE || tok.kind == TokenKind.TK_BADTOKEN)
                    continue;

                ret.Add(tok);
            }
            while (tok.kind != TokenKind.TK_EOI);

            return ret;
        }


        // ////////////////////////////////////////////////////////////////////////////////////
        public List<SyntaxToken> doStructLex2()
        {
            string line = "", sorc;
            string[] arr;
            bool isEOIInList, isIBMSource;
            List<SyntaxToken> ret = new List<SyntaxToken>();

            isIBMSource = false;


            // create a list of lines
            if (doDecmiation == true)
            {
                doDecmiation = false;

                sorc = Regex.Replace(source.ToString(), @"(\r\n|\n|\0)", "¶");
                //sorc = sorc.Substring(0, sorc.Length - 1);
                arr = sorc.Split('¶');
                isIBMSource = SyntaxFacts.isIBMDoc(arr);
                originalSrucLinesCount = arr.Length;

                // save array as list
                sourceLines = new List<string>(arr);
            }


            lineFeeder = new List<StructCard>();

            for (int i = 0; i < sourceLines.Count(); i++)
            {
                // get a line and capatilize all letters but not the strings
                line = SyntaxFacts.normalizeLine(sourceLines[i], isIBMSource);

                // remove comments and add line to list
                line = SyntaxFacts.normalizeComments(line);

                sourceLines[i] = line;
                lineFeeder.Add(new StructCard(line, (i + 1)));
            }

            // get inline declares
            strucLexLine = Decimator.performCSpecVarDeclar(sourceLines);

            // generate a list of tokens from the soruce code
            strucLexLine = Decimator.doDecimation(lineFeeder, source, ref _SyntaxTree, ref diagnostics);

            // check if there is a EOI token in the token list
            isEOIInList = (from tkn in strucLexLine
                           select tkn.kind == TokenKind.TK_EOI).FirstOrDefault();
            if (isEOIInList == false)
                strucLexLine.Add(new SyntaxToken(_SyntaxTree, TokenKind.TK_EOI, 1, 0, originalSrucLinesCount, 0));

            // ------------------------------------------------------------------
            // remove any spaces from source file
            foreach (SyntaxToken el in strucLexLine)
                if (el.kind != TokenKind.TK_SPACE)
                    ret.Add(el);

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////

        public DiagnosticBag getDiagnostics()
        {
            return diagnostics;
        }
    }
}
