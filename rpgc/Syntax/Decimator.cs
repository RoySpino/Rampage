using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rpgc.Symbols;
using System.Text.RegularExpressions;
using rpgc.Text;

namespace rpgc.Syntax
{
    public static class Decimator
    {
        static char specChkStr = 'H';
        static bool isProcSection = false;
        static TokenKind kind;
        static char curChar, prevSpec;
        static object Value;
        static int start, lineStart;
        static string tmpVal;
        static DiagnosticBag diagnostics = null;
        static int assignmentCnt;
        static bool onEvalLine, onBooleanLine;
        static int parenCnt = 0;
        static string lineType = "", prevOp="";
        static string factor, DBlockType;
        static int pos, sSize, linePos;
        private static SyntaxToken TopToken;
        private static List<SyntaxToken> localTokenLst = new List<SyntaxToken>();
        private static List<SyntaxToken> localTokenLst2 = new List<SyntaxToken>();
        private static List<SyntaxToken> localCSpecDclr = new List<SyntaxToken>();
        private static int ITagCnt = 0;
        private static SyntaxTree sTree_;
        private static SourceText source;
        private static TextLocation location;
        private static bool isInFreeTag;
        private static bool inDecareBlock,
            doAddMainFunciton = true,
            doAddMainProcEnd = false,
            doAddMainProcSrt = true,
            onSelectStart = false,
            doMainInjectBeforEOF = false;

        private static bool isGoodSpec(char spec, int line)
        {
            Dictionary<char, int> specVal2 = new Dictionary<char, int>() { { 'H', 1 }, { 'F', 2 }, { 'D', 3 }, { 'I', 4 }, { 'C', 5 }, { 'O', 6 }, { 'P', 7 } };
            Dictionary<char, int> procSpec = new Dictionary<char, int>() { { 'D', 3 }, { 'C', 5 }, { 'P', 7 } };
            Dictionary<char, int> mainDic;

            // spec is the same as preveous
            if (spec == specChkStr)
                return true;

            // standardize dictionary
            if (isProcSection == false)
                mainDic = specVal2;
            else
                mainDic = procSpec;

            // invalid specification
            if (mainDic.ContainsKey(spec) == false)
            {
                location = new TextLocation(source, new TextSpan(0, 1, line, 1));
                diagnostics.reportBadSpec(location, spec, line);
                return false;
            }

            // within the main procedure AND spec are not the same 
            if (isProcSection == false)
            {
                if (mainDic[spec] >= mainDic[specChkStr])
                {
                    // start of procedure section reset to D and return true
                    if (spec == 'P')
                    {
                        isProcSection = true;
                        specChkStr = 'D';
                    }
                    else
                        specChkStr = spec;

                    return true;
                }
                return false;
            }
            else
            {
                // in procedure section AND spec are not the same
                if (mainDic[spec] >= mainDic[specChkStr])
                {
                    // when P is encountered reset last spec to D
                    if (spec == 'P')
                        specChkStr = 'D';
                    else
                        specChkStr = spec;

                    return true;
                }
                return false;
            }
        }

        // //////////////////////////////////////////////////////////////////////////
        private static int computeCharPos(int pos)
        {
            return lineStart + pos;
        }

        // //////////////////////////////////////////////////////////////////////////
        private static List<StructNode> decimateCSpec(int lineNo, string line)
        {
            List<StructNode> ret = new List<StructNode>();
            StructNode tnode;
            int strLen;
            string sym;
            bool isOnEvalLine = false, doEvalSlice = false;

            int[,] slicer = {
                {1, 2},
                {3, 1},
                {4, 2},
                {6, 14},
                {20, 10},
                {30, 14},
                {44, 14},
                {58, 5},
                {63, 2},
                {65, 2},
                {67, 2},
                {69, 2}
            };

            // check spec position
            if (isGoodSpec(line[0], lineNo) == false)
            {
                location = new TextLocation(source, new TextSpan(0, 1, lineNo, 1));
                diagnostics.reportWrongSpecLoc(location, 'C', specChkStr, lineNo);
            }

            strLen = line.Length - 1;

            // setup for extended factor 2
            // otherwise use traditinal rpg 14 character long factors
            sym = line.Substring(20, 10).Trim();
            isOnEvalLine = SyntaxFacts.isExtededFector2Keyword(sym);

            for (int i = 0; i < 12; i++)
            {
                if (strLen <= 0)
                    break;

                if (doEvalSlice == false)
                {
                    // slice standard RPG C spec
                    if (strLen >= slicer[i, 1])
                        sym = line.Substring(slicer[i, 0], slicer[i, 1]);
                    else
                        sym = line.Substring(slicer[i, 0]);
                    strLen -= slicer[i, 1];
                }
                else
                {
                    // slice evaluation
                    sym = line.Substring(slicer[i, 0]);
                    strLen = 0;
                }

                // trim string
                if (i == 7 || i == 8)
                    sym = sym.TrimStart();
                else
                    sym = sym.TrimEnd();

                // set EVAL boolean
                doEvalSlice = (i == 4 && isOnEvalLine == true);

                tnode = new StructNode(lineNo, (slicer[i, 0]), sym.TrimEnd());
                switch(i)
                {
                    case 3:
                        tnode.factor = 1;
                        break;
                    case 4:
                        tnode.factor = 4;
                        break;
                    case 5:
                        tnode.factor = 2;
                        break;
                    case 6:
                        tnode.factor = 3;
                        break;
                    default:
                        tnode.factor = 0;
                        break;
                }

                ret.Add(tnode);
            }

            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////
        private static List<StructNode> decimateDSpec(int lineNo, string line)
        {
            List<StructNode> ret = new List<StructNode>();
            int strLen;
            string sym;

            int[,] slicer = {
                {1, 15},
                {16, 1},
                {17, 1},
                {18, 2},
                {20, 7},
                {27, 7},
                {34, 1},
                {35, 2},
                {38, 33}
            };

            // check spec position
            if (isGoodSpec(line[0], lineNo) == false)
            {
                location = new TextLocation(source, new TextSpan(0, 1, lineNo, 1));
                diagnostics.reportWrongSpecLoc(location, 'D', specChkStr, lineNo);
            }

            strLen = line.Length - 1;

            for (int i = 0; i < 9; i++)
            {
                if (strLen <= 0)
                    break;

                // slice string according to column position
                if (strLen >= slicer[i, 1])
                    sym = line.Substring(slicer[i, 0], slicer[i, 1]);
                else
                    sym = line.Substring(slicer[i, 0]);
                strLen -= slicer[i, 1];

                // trim string
                sym = sym.Trim();
                /*
                if (i == 4 || i == 5 || i == 7)
                    sym = sym.TrimStart();
                else
                    sym = sym.TrimEnd();
                */

                ret.Add(new StructNode(lineNo, (slicer[i, 0]), sym));
            }

            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////
        private static List<StructNode> decimatePSpec(int lineNo, string line)
        {
            List<StructNode> ret = new List<StructNode>();
            int strLen;
            string sym;

            int[,] slicer = {
                {1, 15},
                {18, 1},
                {30, 4},
                {34, 1},
                {35, 2},
                {38, 33}
            };

            // check spec position
            if (isGoodSpec(line[0], lineNo) == false)
            {
                location = new TextLocation(source, new TextSpan(0, 1, lineNo, 1));
                diagnostics.reportWrongSpecLoc(location,'P', specChkStr, lineNo);
            }

            strLen = line.Length - 1;

            for (int i = 0; i < 6; i++)
            {
                if (strLen <= 0)
                    break;

                // slice string according to column position
                if (strLen >= slicer[i, 1])
                    sym = line.Substring(slicer[i, 0], slicer[i, 1]);
                else
                    sym = line.Substring(slicer[i, 0]);
                strLen -= slicer[i, 1];

                ret.Add(new StructNode(lineNo, (slicer[i, 0]), sym));
            }

            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////
        private static List<StructNode> decimateFSpec(int lineNo, string line)
        {
            List<StructNode> ret = new List<StructNode>();
            int strLen;
            string sym;

            int[,] slicer = {
                {1, 10},
                {11,1},
                {12,1},
                {13,1},
                {14,1},
                {15,1},
                {16,1},
                {22,1},
                {28,1},
                {30,7},
                {38, 33}
            };

            // check spec position
            if (isGoodSpec(line[0], lineNo) == false)
            {
                location = new TextLocation(source, new TextSpan(0,1,lineNo,1));
                diagnostics.reportWrongSpecLoc(location,'F', specChkStr, lineNo);
            }

            strLen = line.Length - 1;

            for (int i = 0; i < 11; i++)
            {
                if (strLen <= 0)
                    break;

                if (strLen >= slicer[i, 1])
                    sym = line.Substring(slicer[i, 0], slicer[i, 1]);
                else
                    sym = line.Substring(slicer[i, 0]);
                strLen -= slicer[i, 1];

                ret.Add(new StructNode(lineNo, (slicer[i, 0]), sym));
            }

            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////
        private static List<StructNode> decimateHSpec(int lineNo, string line)
        {
            List<StructNode> ret = new List<StructNode>();
            int strLen;
            string sym;

            int[,] slicer = {
                {1, 79}
            };

            // check spec position
            if (isGoodSpec(line[0], lineNo) == false)
            {
                location = new TextLocation(source, new TextSpan(0, 1, lineNo, 1));
                diagnostics.reportWrongSpecLoc(location, 'H', specChkStr, lineNo);
            }

            strLen = line.Length - 1;

            for (int i = 0; i < slicer.Length; i++)
            {
                if (strLen <= 0)
                    break;

                if (strLen >= slicer[i, 1])
                    sym = line.Substring(slicer[i, 0], slicer[i, 1]);
                else
                    sym = line.Substring(slicer[i, 0]);
                strLen -= slicer[i, 1];

                ret.Add(new StructNode(lineNo, (slicer[i, 0]), sym));
            }

            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////
        internal static List<SyntaxToken> performCSpecVarDeclar(List<string> arr)
        {
            List<string> lstVars = new List<string>();
            SyntaxToken[] tNodeArr;
            string decSize, intSize, varName, chkr, kwrd;
            string[] declarLines;


            // find all C Spec declaration lines
            //declarLines = arr.Where(csl => csl.StartsWith("C") == true && csl.Length >= 64).ToArray();
            // where line begins with C and line is Grater than 64 and line 58 <> [blank]
            declarLines = arr.Where(csl => csl.StartsWith("C") == true).ToArray()
                             .Where(gln => gln.Length >= 64).ToArray()
                             .Where(dln => dln[58] != ' ').ToArray();
            
            // no declared lines found do nothing
            if (declarLines == null)
                return new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(sTree_ ,TokenKind.TK_SPACE, 0, 0, "") });

            // capatalize each line
            for (int i = 0; i < declarLines.Length; i++)
                declarLines[i] = declarLines[i].ToUpper();

            // normalize l
            foreach (string ln in declarLines)
            {
                // get op-code 
                // do not do if the opcode is an extended Fac 2
                kwrd = ln.Substring(20, 10).Trim();
                if (SyntaxFacts.isExtededFector2Keyword(kwrd) == true)
                    continue;

                varName = ln.Substring(44, 14).Trim();

                // chekc if variable is already declared
                chkr = lstVars.Where(vn => vn == varName).FirstOrDefault();
                if (chkr != null)
                    continue;

                // add varialbe to declared list
                lstVars.Add(varName);

                if (ln.Length == 64)
                {
                    // create string declarations ONLY
                    intSize = ln.Substring(55, 5);

                    tNodeArr = new SyntaxToken[] {
                    new SyntaxToken(sTree_ ,TokenKind.TK_VARDECLR, 0, 0, ""),
                    new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER,  0, 0, varName),
                    new SyntaxToken(sTree_ ,TokenKind.TK_STRING,  0, 0, "") };
                }
                else
                {
                    // create declarations for Ints and floats
                    intSize = ln.Substring(55, 5);
                    decSize = ln.Substring(63, 2);

                    tNodeArr = new SyntaxToken[] {
                    new SyntaxToken(sTree_ ,TokenKind.TK_VARDECLR, 0, 0, ""),
                    new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER,  0, 0, varName),
                    new SyntaxToken(sTree_ ,TokenKind.TK_ZONED,  0, 0, "") };
                }

                localCSpecDclr.AddRange(tNodeArr);
            }

            //return localCSpecDclr;
            return new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(sTree_ ,TokenKind.TK_SPACE, 0, 0, "") });
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static void nextChar()
        {
            pos += 1;

            if (pos >= sSize)
                curChar = '\0';
            else
            {
                curChar = factor[pos];

                if (curChar == '\n')
                {
                    pos += 1;
                    start = 0;
                    curChar = factor[pos];
                }
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static char peek(int offset)
        {
            int index;

            index = pos + offset;

            if (index >= sSize)
                return '\0';
            else
                return factor[index];
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> doLex(StructNode factor_, bool isFreeBlock = false)
        {
            List<SyntaxToken> ret = new List<SyntaxToken>();
            string symbol = "", line;
            int sz;

            pos = -1;
            line = factor_.symbol;
            start = factor_.chrPos;
            linePos = factor_.linePos;
            factor = line;
            sz = line.Length;
            sSize = sz;
            assignmentCnt = 0;

            // compleate pi/pr/ds block if any
            if (inDecareBlock == true)
            {
                ret.AddRange(getEndInterfaceOrPrototype());
                inDecareBlock = false;
                DBlockType = null;
            }

            nextChar();

            while (pos != sz)
            {
                // exit decimator lexer
                if (curChar < 32)
                    break;
                
                switch (curChar)
                {
                    case '+':
                        start = pos;
                        kind = TokenKind.TK_ADD;
                        Value = "+";
                        nextChar();
                        break;
                    case '-':
                        start = pos;
                        kind = TokenKind.TK_SUB;
                        Value = "-";
                        nextChar();
                        break;
                    case ';':
                        start = pos;
                        kind = (isFreeBlock == true? TokenKind.TK_NEWLINE: TokenKind.TK_BADTOKEN);
                        Value = ";";
                        nextChar();
                        break;
                    case '*':
                        symbol = readCompilerConstantsOrMult();
                        Value = symbol;
                        break;
                    case '/':
                        start = pos;
                        if (peek(1) == '/')
                        {
                            ignoreCommentLine();
                        }
                        else
                        {
                            kind = TokenKind.TK_DIV;
                            Value = "/";
                            nextChar();
                        }
                        break;
                    case '(':
                        start = pos;
                        kind = TokenKind.TK_PARENOPEN;
                        Value = "(";
                        parenCnt += 1;
                        nextChar();
                        break;
                    case ')':
                        start = pos;
                        kind = TokenKind.TK_PARENCLOSE;
                        Value = ")";
                        parenCnt -= 1;
                        nextChar();
                        break;
                    case ':':
                        start = pos;
                        kind = TokenKind.TK_COLON;
                        Value = ":";
                        nextChar();
                        break;
                    case '=':
                        start = pos;
                        kind = getAssignmentOrComparisonToken();
                        Value = "=";
                        nextChar();
                        break;
                    case '<':
                        start = pos;
                        kind = getLessGreaterThanOperator('<', peek(1));
                        Value = tmpVal;
                        nextChar();
                        break;
                    case '>':
                        start = pos;
                        kind = getLessGreaterThanOperator('>', peek(1));
                        Value = tmpVal;
                        nextChar();
                        break;
                    case '%':
                        symbol = readBuiltInFunctions();//readBuiltInFunctions(line, ref i);
                        break;
                    case '\'':
                        readString();
                        break;
                    case '@':
                    case '#':
                    case '$':
                        symbol = readIdentifierOrKeyword();
                        Value = symbol;
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
                        Value = symbol;
                        break;
                    case ' ':
                    case '\0':
                    case '\n':
                    case '\t':
                    case '\r':
                        readWiteSpace();
                        symbol = " ";
                        break;
                    default:
                        if (char.IsLetter(curChar) == true)
                        {
                            symbol = readIdentifierOrKeyword();
                        }
                        else
                        {
                            if (char.IsWhiteSpace(curChar) == true)
                            {
                                symbol = "";
                                readWiteSpace();
                            }
                            else
                            {
                                location = new TextLocation(source, new TextSpan(start, 1, factor_.linePos, factor_.chrPos));
                                diagnostics.reportBadCharacter(location, curChar, 1);
                                symbol = curChar.ToString();
                                nextChar();
                            }
                        }
                        Value = symbol;
                        break;
                }

                // skip spaces
                if (kind == TokenKind.TK_SPACE)
                    continue;

                ret.Add(new SyntaxToken(sTree_ ,kind, linePos, (start + factor_.chrPos), Value, start));
            }

            if (isInFreeTag == true)
            {
                getStartOrEndBlock();
                ret.Add(new SyntaxToken(sTree_ ,kind, linePos, (start + factor_.chrPos), "", start));
                lineType = "";
            }

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static void ignoreCommentLine()
        {
            // loop until a control char is reached
            // preferably [\n] or [\r]
            while (true)
            {
                nextChar();
                if (curChar <= 20)
                    break;
            }

            kind = TokenKind.TK_SPACE;
            Value = "";
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static void readString()
        {
            bool isInString;
            int charCnt;
            string text;

            // record current position and skip first single quoat
            start += pos;
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
                        location = new TextLocation(source, new TextSpan(start, charCnt, linePos, pos));
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
        private static string readNumberToken()
        {
            string symbol = "";
            double doubleDummy;

            start += pos;

            while (char.IsDigit(curChar) == true || curChar == '.')
            {
                symbol += curChar;
                nextChar();
            }

            // try to convert string to a number
            // on bad number return bad token
            if (double.TryParse(symbol, out doubleDummy) == false)
            {
                location = new TextLocation(source, new TextSpan(start, symbol.Length));
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
        private static void readWiteSpace()
        {
            string symbol = "";

            start += pos;

            // skip whitespace
            while (char.IsWhiteSpace(curChar) == true)
            {
                symbol += curChar;
                nextChar();
            }

            Value = "";
            kind = TokenKind.TK_SPACE;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static string readIdentifierOrKeyword()
        {
            string symbol = "";

            start += pos;
            while (char.IsLetterOrDigit(curChar) || curChar == '#' || curChar == '@' || curChar == '$' || curChar == '_')
            {
                symbol += curChar;
                nextChar();
            }

            symbol = symbol.ToUpper();

            setLineType(symbol);

            // assign keyword token
            kind = SyntaxFacts.getFreeFormatKind(symbol);

            // chech if symbol is a valid variabel name
            if (kind == TokenKind.TK_IDENTIFIER)
            {
                // chech if symbol is a valid variabel name
                if (SyntaxFacts.isValidVariable(symbol))
                    Value = symbol;
                else
                    kind = TokenKind.TK_BADTOKEN;
            }

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static void setLineType(string symbol)
        {
            switch(symbol)
            {
                case "DOU":
                case "DOW":
                case "ELSE":
                case "FOR":
                case "IF":
                case "MONITOR":
                case "WHEN":
                    lineType = symbol;
                    break;
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        // special indicators and constants defalts to mult if nothing is found
        private static string readCompilerConstantsOrMult()
        {
            string peekStr, tmp;
            char peekChar;
            int peekPos;

            start += pos;
            peekPos = 0;
            peekStr = "";

            while (true)
            {
                peekChar = peek(peekPos);
                peekPos += 1;

                if (SyntaxFacts.isCharLiteralOrControl(peekChar) == false && peekChar != '*')
                    break;

                peekStr += peekChar;
            }
            kind = SyntaxFacts.getBuiltInIndicator(peekStr);

            if (kind != TokenKind.TK_BADTOKEN)
            {
                for (int i = 0; i < peekStr.Length; i++)
                {
                    nextChar();
                }

                tmp = SyntaxFacts.getCompilerConstansLiteral(peekStr);
                if (tmp == null)
                    tmp = peekStr;
                    
                Value = tmp;

                return tmp;
            }
            else
            {
                nextChar();
                kind = TokenKind.TK_MULT;
                Value = "*";
            }

            kind = TokenKind.TK_MULT;
            return peekStr;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static string readBuiltInFunctions()
        {
            string symbol = "";

            start += pos;
            symbol = "%";

            nextChar();
            while (char.IsLetterOrDigit(curChar) == true)
            {
                symbol += curChar;
                nextChar();
            }

            symbol = symbol.Trim().ToUpper();
            Value = symbol;
            kind = SyntaxFacts.getBuiltInFunction(symbol);

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static void getStartOrEndBlock()
        {
            // if there is a lineType set a
            switch (lineType)
            {
                case "DOU":
                case "DOW":
                case "ELSE":
                case "FOR":
                case "IF":
                case "MONITOR":
                case "WHEN":
                    kind = TokenKind.TK_BLOCKSTART;
                    return;
            }

            kind = TokenKind.TK_SPACE;
            return;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static TokenKind getAssignmentOrComparisonToken()
        {
            TokenKind ret, tKind;

            // if the [=] is inside a parethisies then its a comparison
            onEvalLine = (parenCnt == 0 && onBooleanLine == false);

            // if the line started with a comparison keyword then its a comparison
            switch (lineType)
            {
                case "DOU":
                case "DOW":
                case "IF":
                case "WHEN":
                    onEvalLine = false;
                    TopToken = null;
                    break;
            }

            // check for end of boolean statement
            if (TopToken != null)
            {
                tKind = TopToken.kind;
                if (tKind == TokenKind.TK_NEWLINE || tKind == TokenKind.TK_BLOCKSTART)
                    onEvalLine = true;
            }

            // check if the current line is a comparison or assignment
            if (onEvalLine == true && assignmentCnt < 1)
            {
                // first = is an assignment all others are comparisons
                ret = TokenKind.TK_ASSIGN;
                assignmentCnt += 1;
                onEvalLine = false;
            }
            else
                ret = TokenKind.TK_EQ;

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static TokenKind getLessGreaterThanOperator(char first, char op)
        {
            TokenKind ret;

            // check if the fisrt char is a Less than symbol
            if (first == '<')
                switch (op)
                {
                    case '>':
                        ret = TokenKind.TK_NE;
                        tmpVal = ("" + first + op);
                        pos += 1;
                        break;
                    case '=':
                        ret = TokenKind.TK_LE;
                        tmpVal = ("" + first + op);
                        pos += 1;
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
                        pos += 1;
                        break;
                    default:
                        ret = TokenKind.TK_GT;
                        tmpVal = ("" + first);
                        break;
                }

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////

        private static StructNode getComparisonInd(StructNode node1, StructNode node2, StructNode node3)
        {
            StructNode tmp;

            // get the first non blank symbol
            if (node1.symbol.Trim().Length > 0)
                tmp = node1;
            else
            {
                if (node2.symbol.Trim().Length > 0)
                    tmp = node2;
                else
                    tmp = node3;
            }

            // convert two digit indicator to standard indicator
            tmp.symbol = $"*IN{tmp.symbol}";

            return tmp;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static List<SyntaxToken> normalizeConditinalIndicators(StructNode Col2, StructNode Col3)
        {
            string N, ind01, condition;
            List<SyntaxToken> ret = new List<SyntaxToken>();

            // rewrite column 2 
            if (Col2.symbol == "")
                N = " = *On";
            else
                N = " = *Off";

            // convert two didget indicator to freeformat indicator
            ind01 = Col3.symbol;
            if (ind01 != "")
                ind01 = $"*in{ind01}";

            // build comparison string
            condition = ind01 + N;

            // lex the condition line
            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IF, Col2.linePos, Col2.chrPos, "IF"));
            ret.AddRange(doLex(new StructNode(Col3.linePos, Col3.chrPos, condition)));
            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        private static SyntaxToken getComparisonOpCode(StructNode node)
        {
            string op;

            op = node.symbol;
            op = op.Substring(op.Length - 2);

            switch (op)
            {
                case "EQ":
                    return new SyntaxToken(sTree_ ,TokenKind.TK_EQ, node.linePos, (node.chrPos), node.symbol);
                case "NE":
                    return new SyntaxToken(sTree_ ,TokenKind.TK_NE, node.linePos, (node.chrPos), node.symbol);

                case "LT":
                    return new SyntaxToken(sTree_ ,TokenKind.TK_LT, node.linePos, (node.chrPos), node.symbol);
                case "GT":
                    return new SyntaxToken(sTree_ ,TokenKind.TK_GT, node.linePos, (node.chrPos), node.symbol);

                case "GE":
                    return new SyntaxToken(sTree_ ,TokenKind.TK_GE, node.linePos, (node.chrPos), node.symbol);
                case "LE":
                    return new SyntaxToken(sTree_ ,TokenKind.TK_LE, node.linePos, (node.chrPos), node.symbol);
                default:
                    return new SyntaxToken(sTree_ ,TokenKind.TK_BADTOKEN, node.linePos, (node.chrPos), "");
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        private static StructNode leftJustified(List<StructNode> lst)
        {
            for (int i = 3; i < 7; i++)
                if (i < lst.Count)
                    if (string.IsNullOrEmpty(lst[i].symbol) == false)
                        if (lst[i].isLeftJustified() == true)
                            return lst[i];

            return null;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        private static SyntaxToken reportCSpecPositionError(StructNode snode)
        {
            // one of the factors is not left justified
            location = new TextLocation(source, new TextSpan(snode.chrPos, snode.symbol.Length, snode.linePos, snode.chrPos));
            diagnostics.reportNotLeftJustified(location, snode.factor, snode.linePos);

            return new SyntaxToken(sTree_ ,TokenKind.TK_BADTOKEN, snode.linePos, snode.chrPos, snode.symbol);
        }

        // ////////////////////////////////////////////////////////////////////////////
        // use global varialbe to assign ending type
        // this returns a value other than space on when DBlockType changes
        private static List<SyntaxToken> getEndInterfaceOrPrototype()
        {
            TokenKind tk;
            List<SyntaxToken> rarr = new List<SyntaxToken>();

            switch (DBlockType)
            {
                case "pi":
                    tk = TokenKind.TK_ENDPI;
                    break;
                case "pr":
                    tk = TokenKind.TK_ENDPR;
                    break;
                case "ds":
                    tk = TokenKind.TK_ENDDS;
                    break;
                default:
                    tk = TokenKind.TK_SPACE;
                    break;
            }

            if (tk != TokenKind.TK_SPACE)
            {
                rarr.Add(new SyntaxToken(sTree_, tk, linePos, 1, "", 0));
                rarr.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, 0, 0, "", 0));
            }

            return rarr;
        }

        // ////////////////////////////////////////////////////////////////////////////
        private static List<SyntaxToken> inject(string sym){
            StructNode snode;
            List<SyntaxToken> lst;

            snode = new StructNode(0, 0, sym);
            lst = doLex(snode);

            return lst;
        }

        // ////////////////////////////////////////////////////////////////////////////
        private static List<SyntaxToken> injectMainEnd()
        {
            List<SyntaxToken> ret = new List<SyntaxToken>();

            doAddMainProcEnd = false;
            doAddMainFunciton = false;
            doMainInjectBeforEOF = false;
            ret.AddRange(SyntaxFacts.prepareMainFunction(sTree_, TokenKind.TK_NEWLINE, false));

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> doDecimation(List<StructCard> cards, SourceText txt, ref SyntaxTree st, ref DiagnosticBag diag)
        {
            bool doFreeBlock = false, onEndCSpec, onStartCSpec, onFreeTagStart, onBegSR;
            char Specification;
            string tmp, line;
            int charPos, lineNo, cardLimit;
            string peekOp, cascadeOp = null;
            List<StructNode> lst;
            List<SyntaxToken> ret;
            StructCard card;
            Regex regBegSr = new Regex(@"(?i)^([c])(.{19})(begsr)");

            sTree_ = st;
            ret = new List<SyntaxToken>();
            source = txt;
            Specification = ' ';
            cardLimit = cards.Count();

            // setup global diagnostic bag
            diagnostics = diag;

            for (int i = 0; i < cardLimit; i++)
            {
                card = cards[i];
                line = card.Line;
                charPos = 0;
                lineNo = card.LinePos;

                // get last specification
                // ignore blanks
                if (Specification > 32)
                    prevSpec = Specification;

                // setup line
                tmp = line.PadRight(72);
                Specification = tmp[0];
                lineStart = charPos;

                // do not try to decimate a blank line
                if (tmp.Trim() == "")
                {
                    lineNo += 1;
                    continue;
                }

                // handle comments
                if (tmp[1] == '*' || (tmp[1] == '/' && tmp[2] == '/'))
                    continue;

                if (tmp.Contains(";") == true && doFreeBlock == false)
                {
                    location = new TextLocation(source, new TextSpan(0, 0, lineNo, tmp.IndexOf(";")));
                    diagnostics.reportSemiColonInFixedFormat(location, lineNo, tmp.IndexOf(";"));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_SEMI, lineNo, tmp.IndexOf(";"), ";", lineNo));
                }

                // perform free block
                if (tmp.Substring(0, 7).Contains("/FREE") == true)
                {
                    doFreeBlock = true;
                    isInFreeTag = true;
                    continue;
                }
                if (tmp.Substring(0, 11).Contains("/END-FREE") == true)
                {
                    doFreeBlock = false;
                    isInFreeTag = false;
                    continue;
                }

                // set main procedure setup flags
                if (doAddMainFunciton == true)
                {
                    onStartCSpec = (prevSpec != 'C' && Specification == 'C');
                    onFreeTagStart = doFreeBlock;
                    onBegSR = regBegSr.Match(line).Success;
                    onEndCSpec = (prevSpec == 'C' && Specification != 'C');

                    //if (doAddMainProcSrt == true && ((prevSpec != 'C' && Specification == 'C') || tmp.Substring(0, 7).Contains("/FREE") == true))
                    if ((doAddMainProcSrt == true && onStartCSpec == true) || (doAddMainProcSrt == true && onFreeTagStart == true))
                    {
                        // add starting half of the HIDDEN main procedure
                        doAddMainProcSrt = false;
                        ret.AddRange(SyntaxFacts.prepareMainFunction(st, TokenKind.TK_NEWLINE, true));
                    }
                    else
                    {
                        // set flag to add ending part of the main procedure
                        if (doAddMainProcSrt == false)
                        {
                            // O or P spec is found after
                            // and not within a free block
                            if (onFreeTagStart == false && onEndCSpec == true || onFreeTagStart == false && onBegSR == true)
                            {
                                ret.AddRange(injectMainEnd());
                            }

                            // only main procedure is in the program
                            // this is done at the end of this loop
                            if ((i + 1) == cardLimit)
                            {
                                doMainInjectBeforEOF = true;
                            }
                        }
                    }
                }

                if (doFreeBlock == true)
                {
                    tmp = tmp.Trim();
                    ret.AddRange(doLex(new StructNode(lineNo, 0, tmp), doFreeBlock));
                    continue;
                }

                // begin decimation
                switch (Specification)
                {
                    case 'H':
                        lst = decimateHSpec(lineNo, tmp);
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_BADTOKEN, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                        break;
                    case 'F':
                        lst = decimateFSpec(lineNo, tmp);
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_BADTOKEN, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                        break;
                    case 'D':
                        lst = decimateDSpec(lineNo, tmp);
                        ret.AddRange(dSpecRectifier(lst));
                        break;
                    case 'I':
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_BADTOKEN, linePos, 0, "", 1));
                        break;
                    case 'C':

                        if (inDecareBlock == true)
                        {
                            ret.AddRange(getEndInterfaceOrPrototype());
                            inDecareBlock = false;
                            DBlockType = null;
                        }

                        lst = decimateCSpec(lineNo, tmp);

                        // save operation this will be used for multiline IF and EVAL
                        if (string.IsNullOrEmpty(lst[4].symbol) == false)
                            prevOp = lst[4].symbol;

                        // fisrt subroutine was found complete the main funciton
                        if (doAddMainFunciton == true && doAddMainProcSrt == false && prevOp == "BEGSR")
                            ret.AddRange(injectMainEnd());

                        // peek ahead
                        if ((i + 1) < cards.Count())
                        {
                            line = cards[i + 1].Line;

                            if (line.Length > 30)
                                peekOp = line.Substring(20, 10).Trim();
                            else
                                peekOp = "";
                        }
                        else
                            peekOp = "";

                        // ------------------------------------------------------------------------------------------------------------------------------------
                        // C  N01++++
                        // handle conditinal idicatiors for columns 1-8 
                        if (lst[0].symbol != "" || lst[1].symbol != "" || lst[2].symbol != "")
                        {
                            // at the start of indicator control insert an if block
                            if (localTokenLst.Count == 0)
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_IF, lst[0].linePos, (lst[0].chrPos), "", lst[0].chrPos));

                            // add a AND/ OR token if needed
                            switch (lst[0].symbol)
                            {
                                case "AN":
                                    localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_AND, lst[0].linePos, (lst[0].chrPos), "AN", lst[0].chrPos));
                                    break;
                                case "OR":
                                    localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_OR, lst[0].linePos, (lst[0].chrPos), "OR", lst[0].chrPos));
                                    break;
                                case "":
                                    localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_SPACE, lst[0].linePos, (lst[0].chrPos), "", lst[0].chrPos));
                                    break;
                                default:
                                    localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_BADTOKEN, lst[0].linePos, (lst[0].chrPos), "", lst[0].chrPos));
                                    break;
                            }

                            // add NOT if needed
                            switch (lst[1].symbol)
                            {
                                case "N":
                                    localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_NOT, lst[0].linePos, (lst[1].chrPos), "N", lst[0].chrPos));
                                    break;
                                case "":
                                    localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_SPACE, lst[0].linePos, (lst[1].chrPos), "", lst[0].chrPos));
                                    break;
                                default:
                                    localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_BADTOKEN, lst[0].linePos, (lst[1].chrPos), "", lst[0].chrPos));
                                    break;
                            }

                            // Add Indicator boolean logic
                            tmp = SyntaxFacts.getAllIndicators().Where(ik => ik == lst[2].symbol).FirstOrDefault();
                            if (tmp != null)
                            {
                                lst[2].symbol = $"*IN{lst[2].symbol}";
                                localTokenLst.AddRange(doLex(lst[2]));
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_EQ, lst[2].linePos, (lst[2].chrPos), "", lst[2].chrPos));
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_INDOFF, lst[2].linePos, (lst[2].chrPos), "", lst[2].chrPos));
                            }

                            // there is no op-code on this line go to next line and repeate this process
                            if (lst[4].symbol == "")
                                continue;
                            else
                            {
                                // compleate hidden goto statement
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_BLOCKSTART, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_GOTO, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, lst[0].linePos, lst[0].chrPos, $"^^ITag{ITagCnt}", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_ENDIF, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));

                                // merge tokens to main list
                                ret.AddRange(localTokenLst);
                                // add current line to list
                                ret.AddRange(cSpecRectifier(lst));

                                // add ending tag and clear control indicators
                                tmp = $"^^ITag{ITagCnt}";
                                ret.Add(new SyntaxToken(sTree_, TokenKind.TK_TAG, lst[0].linePos, (lst[0].chrPos), "TAG", tmp, lst[0].chrPos));
                                ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, lst[0].linePos, (lst[0].chrPos), tmp, lst[0].chrPos));
                                ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Clear();

                                ITagCnt += 1;
                                continue;
                            }
                        }
                        // ------------------------------------------------------------------------------------------------------------------------------
                        // handle multiline RPG conditinals (ANDEQ, ORGT, ORNE)
                        if (SyntaxFacts.doColectAnotherCard(lst[4].symbol) == true)
                        {
                            ret.AddRange(cSpecRectifier(lst));

                            // set cascade operation symbol [cascadeOp] will have a value only for IF and DO bocks
                            cascadeOp = lst[4].symbol;
                            if (SyntaxFacts.cascadeBlockStart(cascadeOp) == true)
                                cascadeOp = cascadeOp.Substring(0, 2);
                            else
                                cascadeOp = "";

                            // add block start token when the next token is not apart of the cascade
                            // this will compleate the if cascade
                            if (SyntaxFacts.doColectAnotherCard(peekOp) == false || SyntaxFacts.cascadeBlockStart(peekOp) == true)
                            {
                                // add block start token if needed
                                if (ret[ret.Count - 1].kind != TokenKind.TK_BLOCKSTART)
                                {
                                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, 0, 0, "", lst[0].chrPos));
                                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_BLOCKSTART, 0, 0, cascadeOp, lst[0].chrPos));
                                }
                                cascadeOp = "";
                            }
                            else
                            {
                                // remove start block token
                                if (ret[ret.Count - 1].kind == TokenKind.TK_BLOCKSTART)
                                {
                                    ret.RemoveAt(ret.Count - 1);
                                    ret.RemoveAt(ret.Count - 1);
                                }
                            }

                            continue;
                        }

                        // ------------------------------------------------------------------------------------------------------------------------------
                        // do standard lex
                        ret.AddRange(cSpecRectifier(lst));
                        break;
                    case 'O':
                        ret = new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(sTree_, TokenKind.TK_BADTOKEN, lineNo, 0, "", linePos) });
                        break;
                    case 'P':
                        lst = decimatePSpec(lineNo, tmp);
                        ret.AddRange(pSpecRectifier(lst));
                        break;
                    default:
                        ret = new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(sTree_, TokenKind.TK_BADTOKEN, lineNo, 0, "", linePos) });
                        break;
                }


                /* add hidden main procedure
                if (doAddMainFunciton == true && doMainInjectBeforEOF == true)
                {
                    ret.AddRange(injectMainEnd());
                }
                */
            }

            // at end of sorce but main procedure was not compleate
            // add hidden end for main procedure
            if (doAddMainFunciton == true && doAddMainProcSrt == false)
            {
                ret.AddRange(injectMainEnd());
            }

            // ret has nothing to return move local ist to ret
            if (localTokenLst.Count() > 0)
            {
                ret.AddRange(localTokenLst);
                localTokenLst.Clear();
            }

            // reset conditinal tag counter
            ITagCnt = 0;

            // return lex tokens
            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        private static IEnumerable<SyntaxToken> pSpecRectifier(List<StructNode> lst)
        {
            List<SyntaxToken> ret = new List<SyntaxToken>();
            string lineType;

            lineType = lst[1].symbol;

            switch (lineType)
            {
                case "B":
                    ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_PROCDCL, (lst[0].linePos), 1, "B", lst[0].chrPos));
                    ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER, lst[1].linePos, (lst[1].chrPos), lst[0].symbol.Trim(), lst[1].chrPos));
                    break;
                case "E":
                    ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ENDPROC, (lst[0].linePos), 1, lst[0].symbol.Trim(), lst[0].chrPos));
                    break;
                default:
                    ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BADTOKEN, lst[1].linePos, (lst[1].chrPos), lst[1].symbol, lst[1].chrPos));
                    location = new TextLocation(source, new TextSpan(0, 0, lst[0].linePos, lst[0].chrPos));
                    diagnostics.reportBadProcedure(location, lst[0].linePos, lst[0].chrPos);
                    break;
            }
            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, lst[1].linePos, (lst[1].chrPos), "", lst[1].chrPos));

            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> cSpecRectifier(List<StructNode> lst)
        {
            List<SyntaxToken> ret = new List<SyntaxToken>();
            SyntaxToken tToken;
            StructNode snode = null, nNode;
            int itmCnt;
            string OpCode, tmp;
            StructNode FAC1, FAC2, RESULT, OP, HI, LO, EQ;

            itmCnt = lst.Count;
            onEvalLine = false;
            onBooleanLine = false;
            nNode = new StructNode(0, 0, "");

            // factor 1 is not empty and has no key word
            if (itmCnt == 4 && lst[3].symbol.Length > 0)
            {
                ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BADTOKEN, lst[itmCnt].linePos, lst[itmCnt].chrPos, "", lst[itmCnt].chrPos));
                location = new TextLocation(source, new TextSpan(lst[3].chrPos, lst[3].symbol.Length));
                diagnostics.reportMissingFactor1(location,
                                                 lst[3].chrPos);
            }

            // rectify structured code to free lexicon
            if (itmCnt >= 6)
            {
                // check if the opcode is valid
                OpCode = lst[4].symbol.Trim();
                if (SyntaxFacts.isValidOpCode(OpCode) == false)
                {
                    location = new TextLocation(source, new TextSpan(0, 0, lst[4].linePos, lst[4].chrPos));
                    diagnostics.reportBadOpcode(location, OpCode, lst[4].linePos, lst[4].chrPos);
                    ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER, lst[4].linePos, (lst[4].chrPos), OpCode, lst[4].chrPos));
                    return ret;
                }

                // one or more of the symbols are not left justified
                snode = leftJustified(lst);
                if (snode != null)
                {
                    ret.Add(reportCSpecPositionError(snode));
                    return ret;
                }

                FAC1 = lst[3];
                OP = lst[4];
                FAC2 = (itmCnt >= 6) ? lst[5] : nNode;
                RESULT = (itmCnt >= 7) ? lst[6] : nNode;
                HI = (itmCnt >= 10) ? lst[9] : nNode;
                LO = (itmCnt >= 11) ? lst[10] : nNode;
                EQ = (itmCnt >= 12) ? lst[11] : nNode;

                // check if the opcode takes no arguments or returns a value
                // if so check if there are any errors
                if (SyntaxFacts.isStandaloneOpCode(OpCode) == true)
                    if (FAC1.symbol.Trim() != "" && FAC2.symbol.Trim() != "" && RESULT.symbol.Trim() != "")
                    {
                        location = new TextLocation(source, new TextSpan(0, 0, lst[4].linePos, lst[4].chrPos));
                        diagnostics.reportOpCodeNotAlone(location, lst[4].linePos, lst[4].chrPos, lst[4].symbol);
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BADTOKEN, OP.linePos, (OP.chrPos), OP.symbol));
                        return ret;
                    }

                // add control indicators to the output
                if (localTokenLst2.Count > 0)
                    ret.AddRange(localTokenLst2);

                // perform rectifier
                switch (OpCode)
                {
                    case "ADD":
                    case "SUB":
                    case "MULT":
                    case "DIV":
                        if (FAC1.symbol == "")
                        {
                            // +=,-=,*=,/= factor 2 to factor 3
                            ret.AddRange(doLex(RESULT));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, RESULT.linePos, (RESULT.chrPos), OpCode, RESULT.chrPos));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(sTree_, SyntaxFacts.getKeywordKind(OP.symbol), OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                            ret.AddRange(doLex(RESULT));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        }
                        else
                        {
                            // factors 1,2 and 3
                            ret.AddRange(doLex(RESULT));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, RESULT.linePos, (RESULT.chrPos), OpCode, RESULT.chrPos));
                            ret.AddRange(doLex(FAC1));
                            ret.Add(new SyntaxToken(sTree_, SyntaxFacts.getKeywordKind(OP.symbol), OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));

                            if (OpCode == "DIV")
                            {
                                ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER, OP.linePos, (OP.chrPos), "^^LO", OP.chrPos));
                                ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, OP.linePos, (OP.chrPos), "=", OP.chrPos));
                                ret.AddRange(inject($"%REM({FAC1.symbol}:{FAC2.symbol})"));
                                ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                            }
                        }
                        break;
                    case "BEGSR":
                        if (doAddMainFunciton == true && doAddMainProcEnd == true)
                        {
                            ret.AddRange(injectMainEnd());
                        }

                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_PROCDCL, (OP.linePos), 1, "BEGSR", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER, FAC1.linePos, (FAC1.chrPos), FAC1.symbol.Trim(), FAC1.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "MVR":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER, RESULT.linePos, (RESULT.chrPos), RESULT.symbol, RESULT.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, OP.linePos, (OP.chrPos), "=", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER, OP.linePos, (OP.chrPos), "^^LO", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "CALLB":
                    case "CALLP":
                        onBooleanLine = true;
                        break;
                    case "COMP":
                        snode = leftJustified(lst);
                        ret.AddRange(doLex(getComparisonInd(HI, LO, EQ)));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, OP.linePos, (OP.chrPos), "COMP", OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(sTree_, SyntaxFacts.getindicatorOperation(HI.symbol, LO.symbol, EQ.symbol), OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "CIN":
                        snode = leftJustified(lst);
                        //ret.AddRange(doLex(RESULT));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, RESULT.linePos, (RESULT.chrPos), "", RESULT.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_PARENOPEN, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_PARENCLOSE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "COUT":
                    case "PRINT":
                    case "DSPLY":
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.AddRange(inject("(%char("));
                        ret.AddRange(doLex(FAC1));
                        ret.AddRange(inject("))"));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "ITER":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ITER, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, OP.chrPos, "", OP.chrPos));
                        break;
                    case "LEAVE":
                        if (FAC2.symbol == "" && FAC1.symbol == "")
                        {
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_LEAVE, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, OP.chrPos, "", OP.chrPos));
                        }
                        else
                        {
                            location = new TextLocation(source, new TextSpan(0, 0, OP.linePos, OP.chrPos));
                            diagnostics.reportOpCodeNotAlone(location, OP.linePos, OP.chrPos, OP.symbol);
                        }
                        break;
                    case "DO":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BLOCKSTART, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        break;
                    case "DOUGE":
                    case "DOUGT":
                    case "DOULE":
                    case "DOULT":
                    case "DOUEQ":
                    case "DOUNE":
                        lineType = "DOU";
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_DOU, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(sTree_, SyntaxFacts.getKeywordKind(OP.symbol), OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BLOCKSTART, OP.linePos, (OP.chrPos), "DO", OP.chrPos));
                        break;
                    case "DOU":
                        onBooleanLine = true;
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor 1
                            ret.Add(new SyntaxToken(sTree_, TokenKind.TK_SPACE, FAC1.linePos, (FAC1.chrPos), "", OP.chrPos));
                            location = new TextLocation(source, new TextSpan(FAC1.chrPos, FAC1.symbol.Length));
                            diagnostics.reportBadFactor(location, 1, FAC1.chrPos);
                        }
                        else
                        {
                            lineType = "DOU";
                            ret.Add(new SyntaxToken(sTree_, TokenKind.TK_DOU, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                            ret.Add(new SyntaxToken(sTree_, TokenKind.TK_BLOCKSTART, OP.linePos, (OP.chrPos), "DO", OP.chrPos));
                        }
                        break;
                    case "DOWGE":
                    case "DOWGT":
                    case "DOWLE":
                    case "DOWLT":
                    case "DOWEQ":
                    case "DOWNE":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_DOW, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(sTree_, SyntaxFacts.getKeywordKind(OP.symbol), OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BLOCKSTART, OP.linePos, (OP.chrPos), "DO", OP.chrPos));
                        lineType = "DOW";
                        break;
                    case "DOW":
                        onBooleanLine = true;
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor 1
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_SPACE, FAC1.linePos, (FAC1.chrPos), "", FAC1.chrPos));
                            location = new TextLocation(source, new TextSpan(FAC1.chrPos, FAC1.symbol.Length));
                            diagnostics.reportBadFactor(location, 1, FAC1.chrPos);
                        }
                        else
                        {
                            lineType = "DOW";
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_DOW, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BLOCKSTART, OP.linePos, (OP.chrPos), "DO", OP.chrPos));
                        }
                        break;
                    case "IF":
                        onBooleanLine = true;
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_SPACE, FAC1.linePos, (FAC1.chrPos), "", FAC1.chrPos));
                            location = new TextLocation(source, new TextSpan(FAC1.chrPos, FAC1.symbol.Length));
                            diagnostics.reportBadFactor(location, 1, FAC1.chrPos);
                        }
                        else
                        {
                            lineType = "IF";
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IF, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BLOCKSTART, OP.linePos, (OP.chrPos), "IF", OP.chrPos));
                        }
                        break;
                    case "IFGE":
                    case "IFGT":
                    case "IFLE":
                    case "IFLT":
                    case "IFEQ":
                    case "IFNE":
                        lineType = "IF";
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IF, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(sTree_, SyntaxFacts.getKeywordKind(OP.symbol), OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BLOCKSTART, OP.linePos, (OP.chrPos), "IF", OP.chrPos));
                        break;
                    case "ELSE":
                        lineType = "ELSE";
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_BLOCKEND, OP.linePos, (OP.chrPos), "IF", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ELSE, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BLOCKSTART, OP.linePos, (OP.chrPos), "IF", OP.chrPos));
                        break;
                    case "END":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BLOCKEND, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDDO":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ENDDO, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDIF":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ENDIF, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDFOR":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ENDFOR, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDMON":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ENDMON, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDSL":
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_BLOCKEND, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ENDSL, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDSR":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ENDPROC, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "EXSR":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_EXSR, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_IDENTIFIER, FAC2.linePos, FAC2.chrPos, FAC2.symbol, FAC2.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, OP.chrPos, "", OP.chrPos));
                        lineType = "";
                        break;
                    case "SCAN":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_ASSIGN, OP.linePos, (OP.chrPos), "=", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, OP.linePos, (OP.chrPos), "%SCAN", OP.chrPos));
                        ret.AddRange(inject("("));
                        ret.AddRange(doLex(FAC1));
                        ret.AddRange(inject(":"));
                        ret.AddRange(doLex(FAC2));
                        ret.AddRange(inject(")"));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "XLATE":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_ASSIGN, OP.linePos, (OP.chrPos), "=", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, OP.linePos, (OP.chrPos), "%XLATE", OP.chrPos));
                        ret.AddRange(inject("("));
                        ret.AddRange(doLex(FAC1));
                        ret.AddRange(inject(":"));
                        ret.AddRange(doLex(FAC2));
                        ret.AddRange(inject(")"));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "CHECK":
                    case "CHECKR":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_ASSIGN, OP.linePos, (OP.chrPos), "=", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, OP.linePos, (OP.chrPos), $"%{OpCode}", OP.chrPos));
                        ret.AddRange(inject("("));
                        ret.AddRange(doLex(FAC1));
                        ret.AddRange(inject(":"));
                        ret.AddRange(doLex(FAC2));
                        ret.AddRange(inject(")"));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "ORGE":
                    case "ORGT":
                    case "ORLE":
                    case "ORLT":
                    case "OREQ":
                    case "ORNE":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_OR, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(getComparisonOpCode(OP));
                        ret.AddRange(doLex(FAC2));
                        break;
                    case "ANDGE":
                    case "ANDGT":
                    case "ANDLE":
                    case "ANDLT":
                    case "ANDEQ":
                    case "ANDNE":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_AND, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(getComparisonOpCode(OP));
                        ret.AddRange(doLex(FAC2));
                        break;
                    case "EVAL":
                    case "EVALR":
                        onEvalLine = true;
                        lineType = "";
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor 1
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_SPACE, FAC1.linePos, (FAC1.chrPos), "", FAC1.chrPos));
                            location = new TextLocation(source, new TextSpan(FAC1.chrPos, FAC1.symbol.Length));
                            diagnostics.reportBadFactor(location, 1, FAC1.chrPos);
                        }
                        else
                        {
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        }
                        break;
                    case "FOR":
                        onEvalLine = true;
                        lineType = "";
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_SPACE, FAC1.linePos, (FAC1.chrPos), "", FAC1.chrPos));
                            location = new TextLocation(source, new TextSpan(FAC1.chrPos, FAC1.symbol.Length));
                            diagnostics.reportBadFactor(location, 1, FAC1.chrPos);
                        }
                        else
                        {
                            ret.Add(new SyntaxToken(sTree_, TokenKind.TK_FOR, OP.linePos, OP.chrPos, "FOR", OP.chrPos));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, OP.chrPos, "_", OP.chrPos));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_BLOCKSTART, OP.linePos, OP.chrPos, "~", OP.chrPos));
                        }
                        break;
                    case "LEAVESR":
                        if (FAC2.symbol == "" && FAC1.symbol == "")
                        {
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_RETURN, FAC2.linePos, (FAC2.chrPos), "RETURN", FAC2.chrPos));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        }
                        else
                        {
                            location = new TextLocation(source, new TextSpan(0,0, OP.linePos, OP.chrPos));
                            diagnostics.reportOpCodeNotAlone(location, OP.linePos, OP.chrPos, OP.symbol);
                        }
                        break;
                    case "CAT":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_ASSIGN, RESULT.linePos, (RESULT.chrPos), OpCode, RESULT.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_ADD, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "MOVE":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, RESULT.linePos, (OP.chrPos), "MOVE", RESULT.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "RETURN":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_RETURN, FAC2.linePos, (FAC2.chrPos), "RETURN", FAC2.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "SELECT":
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_SELECT, OP.linePos, (OP.chrPos), "SELECT", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        onSelectStart = true;
                        break;
                    case "SETON":
                    case "SETOFF":
                        for (int i = 0; i < 3; i++)
                        {
                            tmp = lst[9 + i].symbol;
                            if (tmp == "")
                                continue;

                            lst[9 + i].symbol = $"*IN{tmp}";
                            ret.AddRange(doLex(lst[9 + i]));
                            ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, OP.linePos, (OP.chrPos), "", OP.chrPos));
                            ret.Add(new SyntaxToken(sTree_,
                                ((OP.symbol == "SETON") ? TokenKind.TK_INDON : TokenKind.TK_INDOFF),
                                OP.linePos,
                                (OP.chrPos),
                                "", OP.chrPos));
                        }
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, ""));
                        break;
                    case "TAG":
                        tToken = new SyntaxToken(sTree_ ,TokenKind.TK_TAG, OP.linePos, (OP.chrPos), OpCode, FAC1.symbol, OP.chrPos);
                        ret.Add(tToken);
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "OTHER":
                    case "WHEN":
                        lineType = OP.symbol;

                        // close the preveouse block if any
                        if (onSelectStart == false)
                            ret.Add(new SyntaxToken(sTree_, TokenKind.TK_BLOCKEND, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        else
                            onSelectStart = false;


                        // Add condition statment
                        if (lineType == "WHEN")
                        {
                            ret.Add(new SyntaxToken(sTree_, TokenKind.TK_WHEN, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                            ret.AddRange(doLex(FAC2));
                        }
                        else
                            ret.Add(new SyntaxToken(sTree_, TokenKind.TK_OTHER, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_BLOCKSTART, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        break;
                    case "GOTO":
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_GOTO, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "Z-ADD":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, RESULT.linePos, RESULT.chrPos, "", RESULT.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_INTEGER, OP.linePos, OP.chrPos, "0", OP.chrPos));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ADD, OP.linePos, (OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, (OP.chrPos), "", OP.chrPos));
                        break;
                    case "Z-SUB":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_ASSIGN, RESULT.linePos, RESULT.chrPos, "", RESULT.chrPos));
                        ret.AddRange(doLex(new StructNode(OP.linePos, OP.chrPos, "0")));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_SUB, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(sTree_ ,TokenKind.TK_NEWLINE, OP.linePos, RESULT.chrPos, "", OP.chrPos));
                        break;
                }
            }

            // add cSpec variable Declaration
            if (localCSpecDclr.Count > 0)
            {
                localCSpecDclr.AddRange(ret);
                ret = new List<SyntaxToken>(localCSpecDclr);
                localCSpecDclr.Clear();
            }

            // save last token
            if (ret.Count > 0)
                TopToken = ret[ret.Count - 1];

            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> dSpecRectifier(List<StructNode> lst)
        {
            List<SyntaxToken> ret = new List<SyntaxToken>();
            string dclType;

            dclType = lst[3].symbol;
            inDecareBlock = true;

            switch (dclType)
            {
                case "C":
                    ret.AddRange(getEndInterfaceOrPrototype());
                    DBlockType = null;
                    inDecareBlock = false;
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_VARDCONST, (lst[0].linePos), 1, "C", lst[3].chrPos));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, lst[0].linePos, (lst[0].chrPos), lst[0].symbol, lst[0].chrPos));
                    //ret.Add(new SyntaxToken(SyntaxFacts.getRPGType(lst[6].symbol), (lst[6].linePos), lst[6].chrPos, lst[6].symbol, lst[6].chrPos));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, (lst[0].linePos), 0, "", lst[0].chrPos));
                    break;
                case "DS":
                    ret.AddRange(getEndInterfaceOrPrototype());
                    DBlockType = "ds";
                    break;
                case "PI":
                    ret.AddRange(getEndInterfaceOrPrototype());
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_PROCINFC, (lst[0].linePos), 1, "Pi", lst[3].chrPos));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, lst[0].linePos, (lst[0].chrPos), "*n", lst[1].chrPos));
                    if (lst.Count > 4)
                        ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, lst[6].linePos, (lst[6].chrPos), lst[6].symbol, lst[6].chrPos));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, (lst[0].linePos), 0, "", lst[0].chrPos));
                    DBlockType = "pi";
                    break;
                case "PR":
                    ret.AddRange(getEndInterfaceOrPrototype());
                    DBlockType = "pr";
                    break;
                case "S":
                    ret.AddRange(getEndInterfaceOrPrototype());
                    DBlockType = null;
                    inDecareBlock = false;
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_VARDECLR, (lst[0].linePos), 1, "S", lst[3].chrPos));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, lst[0].linePos, (lst[0].chrPos), lst[0].symbol, lst[1].chrPos));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, lst[6].linePos, (lst[6].chrPos), lst[6].symbol, lst[6].chrPos));
                    
                    // add initilizer or other keyword
                    if (lst.Count > 8)
                        ret.AddRange(doLex(lst[8]));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, (lst[0].linePos), 0, "", lst[0].chrPos));
                    break;
                default:
                    // PR and PI block paramiters
                    // add paramiter
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, lst[0].linePos, (lst[0].chrPos), lst[0].symbol, lst[1].chrPos));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, lst[6].linePos, (lst[6].chrPos), lst[6].symbol, lst[6].chrPos));
                    ret.Add(new SyntaxToken(sTree_, TokenKind.TK_NEWLINE, (lst[0].linePos), 0, "", lst[0].chrPos));

                    break;
            }

            return ret;
        }
    }

    // ////////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     /////
    // //////////////////////////////////////////////////////////////////////////



    public class StructNode
    {
        public int linePos;
        public int chrPos;
        public string symbol;
        public int factor;

        public StructNode(int l, int ch, string sym)
        {
            linePos = l;
            chrPos = (ch + 1);
            symbol = sym;
            factor = 0;
        }

        public bool isLeftJustified()
        {
            return (symbol[0] == 32);
        }

        public bool isRightJustified()
        {
            int endIdx;

            endIdx = symbol.Length - 1;

            return (symbol[endIdx] == 32);
        }
    }

    public class StructCard
    {
        public int LinePos;
        public int LineSiz;
        public string Line;

        public StructCard(string l, int lPos)
        {
            Line = l;
            LinePos = lPos;
            LineSiz = l.Length;
        }
    }
}
