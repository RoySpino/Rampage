using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rpgc.Symbols;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class StructNode
    {
        public int linePos;
        public int chrPos;
        public string symbol;
        public int factor;

        public StructNode(int l, int ch, string sym)
        {
            linePos = l;
            chrPos = (l + ch);
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

    // ////////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     /////
    // //////////////////////////////////////////////////////////////////////////
    public static class Decimator
    {
        static char specChkStr = 'H';
        static bool isProcSection = false;
        static TokenKind kind;
        static char curChar;
        static object Value;
        static int start, lineStart;
        static string tmpVal;
        static DiagnosticBag diagnostics = null;
        static int assignmentCnt;
        static bool onEvalLine, onBooleanLine;
        static int parenCnt = 0;
        static string lineType = "", curOp="", prevOp="";
        static string factor, DBlockType;
        static int pos, sSize, linePos;
        private static List<SyntaxToken> localTokenLst = new List<SyntaxToken>();
        private static List<SyntaxToken> localTokenLst2 = new List<SyntaxToken>();
        private static List<SyntaxToken> localCSpecDclr = new List<SyntaxToken>();
        private static int ITagCnt = 0, PrPi_Idx=-1;


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
                diagnostics.reportBadSpec(spec, line);
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
                diagnostics.reportWrongSpecLoc('C', specChkStr, lineNo);

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

                // check if 
                doEvalSlice = (i == 4 && isOnEvalLine == true);

                tnode = new StructNode(lineNo, computeCharPos(slicer[i, 0]), sym.TrimEnd());
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
                diagnostics.reportWrongSpecLoc('D', specChkStr, lineNo);

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

                ret.Add(new StructNode(lineNo, computeCharPos(slicer[i, 0]), sym));
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
                diagnostics.reportWrongSpecLoc('P', specChkStr, lineNo);

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

                ret.Add(new StructNode(lineNo, computeCharPos(slicer[i, 0]), sym));
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
                diagnostics.reportWrongSpecLoc('F', specChkStr, lineNo);

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

                ret.Add(new StructNode(lineNo, computeCharPos(slicer[i, 0]), sym));
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
                diagnostics.reportWrongSpecLoc('H', specChkStr, lineNo);

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

                ret.Add(new StructNode(lineNo, computeCharPos(slicer[i, 0]), sym));
            }

            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////
        internal static List<SyntaxToken> performCSpecVarDeclar(string[] arr)
        {
            List<StructNode> nlist = new List<StructNode>();
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
                return new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_SPACE, 0, 0, "") });

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
                    new SyntaxToken(TokenKind.TK_VARDECLR, 0, 0, ""),
                    new SyntaxToken(TokenKind.TK_IDENTIFIER,  0, 0, varName),
                    new SyntaxToken(TokenKind.TK_STRING,  0, 0, "") };
                }
                else
                {
                    // create declarations for Ints and floats
                    intSize = ln.Substring(55, 5);
                    decSize = ln.Substring(63, 2);

                    tNodeArr = new SyntaxToken[] {
                    new SyntaxToken(TokenKind.TK_VARDECLR, 0, 0, ""),
                    new SyntaxToken(TokenKind.TK_IDENTIFIER,  0, 0, varName),
                    new SyntaxToken(TokenKind.TK_ZONED,  0, 0, "") };
                }

                localCSpecDclr.AddRange(tNodeArr);
            }

            //return localCSpecDclr;
            return new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_SPACE, 0, 0, "") });
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
                        kind = TokenKind.TK_DIV;
                        Value = "/";
                        nextChar();
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
                                diagnostics.reportBadCharacter(curChar, 1);
                                symbol = curChar.ToString();
                            }
                        }
                        Value = symbol;
                        break;
                }

                ret.Add(new SyntaxToken(kind, linePos, (start + factor_.chrPos), Value, start));
            }

            if (true)
            {
                getStartOrEndBlock();
                ret.Add(new SyntaxToken(kind, linePos, (start + factor_.chrPos), "", start));
                lineType = "";
            }

            return ret;
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
                        diagnostics.reportBadString(new TextSpan(start, charCnt, linePos, pos));
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
            int intDummy;

            start += pos;

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
            kind = SyntaxFacts.getKeywordKind(symbol);

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
                    lineType = symbol;
                    break;
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        // special indicators and constants defalts to mult if nothing is found
        private static string readCompilerConstantsOrMult()
        {
            string symbol, peekStr;
            char peekChar;
            int peekPos;

            start += pos;
            peekPos = 0;
            symbol = "*";
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

                Value = peekStr;
                return peekStr;
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

            Value = symbol.Trim().ToUpper();
            kind = SyntaxFacts.getBuiltInFunction(Value.ToString());

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
                    kind = TokenKind.TK_BLOCKSTART;
                    return;
            }

            kind = TokenKind.TK_SPACE;
            return;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static TokenKind getAssignmentOrComparisonToken()
        {
            TokenKind ret;

            // if the [=] is inside a parethisies then its a comparison
            onEvalLine = (parenCnt == 0 && onBooleanLine == false);

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

            // reset boolean
            onEvalLine = true;

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
            ret.Add(new SyntaxToken(TokenKind.TK_IF, Col2.linePos, Col2.chrPos, "IF"));
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
                    return new SyntaxToken(TokenKind.TK_EQ, node.linePos, computeCharPos(node.chrPos), node.symbol);
                case "NE":
                    return new SyntaxToken(TokenKind.TK_NE, node.linePos, computeCharPos(node.chrPos), node.symbol);

                case "LT":
                    return new SyntaxToken(TokenKind.TK_LT, node.linePos, computeCharPos(node.chrPos), node.symbol);
                case "GT":
                    return new SyntaxToken(TokenKind.TK_GT, node.linePos, computeCharPos(node.chrPos), node.symbol);

                case "GE":
                    return new SyntaxToken(TokenKind.TK_GE, node.linePos, computeCharPos(node.chrPos), node.symbol);
                case "LE":
                    return new SyntaxToken(TokenKind.TK_LE, node.linePos, computeCharPos(node.chrPos), node.symbol);
                default:
                    return new SyntaxToken(TokenKind.TK_BADTOKEN, node.linePos, computeCharPos(node.chrPos), "");
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
            diagnostics.reportNotLeftJustified(new TextSpan(snode.chrPos, snode.symbol.Length, snode.chrPos, snode.linePos), snode.factor, snode.linePos);

            return new SyntaxToken(TokenKind.TK_BADTOKEN, snode.linePos, computeCharPos(snode.chrPos), snode.symbol);
        }

        // ////////////////////////////////////////////////////////////////////////////
        public static List<StructNode> doDecimation(int lineNo, string line)
        {
            char Specification;
            string tmp;

            tmp = line.ToUpper().PadRight(72);
            Specification = line[0];

            // begin decimation
            switch (Specification)
            {
                case 'H':
                    return decimateHSpec(lineNo, tmp);
                case 'F':
                    return decimateFSpec(lineNo, tmp);
                case 'D':
                    return decimateDSpec(lineNo, tmp);
                case 'I':
                    return null;
                case 'C':
                    return decimateCSpec(lineNo, tmp);
                case 'O':
                    return null;
                case 'P':
                    return decimatePSpec(lineNo, tmp);
                default:
                    return null;
            }
        }

        // ////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> doDecimation2(int lineNo, int charPos, string line, ref DiagnosticBag diag)
        {
            char Specification;
            string tmp;
            List<StructNode> lst;
            List<SyntaxToken> ret;

            // setup global diagnostic bag
            diagnostics = diag;

            // setup line
            tmp = line.PadRight(72);
            Specification = line[0];
            lineStart = charPos;

            // end of file line
            if (line[0] == '\0')
                return new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_EOI, lineNo, charPos, "_") });

            // do not try to decimate a blank line
            if (tmp[0] == ' ')
                return new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_SPACE, lineNo, 0, "") });

            // handle comments
            if (tmp[1] == '*' || (tmp[1] == '/' && tmp[2] == '/'))
                return new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_SPACE, lineNo, 0, "") });



            // begin decimation
            switch (Specification)
            {
                case 'H':
                    lst = decimateHSpec(lineNo, tmp);
                    ret = dSpecRectifier(lst);
                    break;
                case 'F':
                    lst = decimateFSpec(lineNo, tmp);
                    ret = new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_BADTOKEN, lst[0].linePos, lst[0].chrPos, "") });
                    break;
                case 'D':
                    lst = decimateDSpec(lineNo, tmp);
                    ret = dSpecRectifier(lst);
                    break;
                case 'I':
                    ret = new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_BADTOKEN, lineNo, 0, "", linePos) });
                    break;
                case 'C':
                    lst = decimateCSpec(lineNo, tmp);

                    // ------------------------------------------------------------------------------------------------------------------------------------
                    // C  N01++++
                    // handle conditinal idicatiors for columns 1-8 
                    if (lst[0].symbol != "" || lst[1].symbol != "" || lst[2].symbol != "")
                    {
                        // at the start of indicator control insert an if block
                        if (localTokenLst2.Count == 0)
                            localTokenLst2.Add(new SyntaxToken(TokenKind.TK_IF, lst[0].linePos, computeCharPos(lst[0].chrPos), "", lst[0].chrPos));

                        // add a AND/ OR token if needed
                        switch (lst[0].symbol)
                        {
                            case "AN":
                                localTokenLst2.Add(new SyntaxToken(TokenKind.TK_AND, lst[0].linePos, computeCharPos(lst[0].chrPos), "AN", lst[0].chrPos));
                                break;
                            case "OR":
                                localTokenLst2.Add(new SyntaxToken(TokenKind.TK_OR, lst[0].linePos, computeCharPos(lst[0].chrPos), "OR", lst[0].chrPos));
                                break;
                            case "":
                                localTokenLst2.Add(new SyntaxToken(TokenKind.TK_SPACE, lst[0].linePos, computeCharPos(lst[0].chrPos), "", lst[0].chrPos));
                                break;
                            default:
                                localTokenLst2.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, lst[0].linePos, computeCharPos(lst[0].chrPos), "", lst[0].chrPos));
                                break;
                        }
                        pos += 2;

                        // add NOT if needed
                        switch (lst[1].symbol)
                        {
                            case "N":
                                localTokenLst2.Add(new SyntaxToken(TokenKind.TK_NOT, lst[0].linePos, computeCharPos(lst[1].chrPos), "N", lst[0].chrPos));
                                break;
                            case "":
                                localTokenLst2.Add(new SyntaxToken(TokenKind.TK_SPACE, lst[0].linePos, computeCharPos(lst[1].chrPos), "", lst[0].chrPos));
                                break;
                            default:
                                localTokenLst2.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, lst[0].linePos, computeCharPos(lst[1].chrPos), "", lst[0].chrPos));
                                break;
                        }
                        pos += 1;

                        // Add Indicator boolean logic
                        tmp = SyntaxFacts.getAllIndicators().Where(ik => ik == lst[2].symbol).FirstOrDefault();
                        if (tmp != null)
                        {
                            lst[2].symbol = $"*IN{lst[2].symbol}";
                            localTokenLst2.AddRange(doLex(lst[2]));
                            localTokenLst2.Add(new SyntaxToken(TokenKind.TK_EQ, lst[2].linePos, computeCharPos(lst[2].chrPos), "", lst[2].chrPos));
                            localTokenLst2.Add(new SyntaxToken(TokenKind.TK_INDOFF, lst[2].linePos, computeCharPos(lst[2].chrPos), "", lst[2].chrPos));
                        }

                        // return blank list this will request another card
                        if (lst[4].symbol == "")
                            return new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_BLOCKSTART, 0, 0, "", "!__ReqCard__", linePos) });
                        else
                        {
                            // compleate hidden goto statement
                            localTokenLst2.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                            localTokenLst2.Add(new SyntaxToken(TokenKind.TK_GOTO, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                            localTokenLst2.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[0].linePos, lst[0].chrPos, $"^^ITag{ITagCnt}", lst[0].chrPos));
                            localTokenLst2.Add(new SyntaxToken(TokenKind.TK_ENDIF, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                            ITagCnt += 1;
                        }
                    }
                    // ------------------------------------------------------------------------------------------------------------------------------
                    // handle multiline RPG conditinals (ANDEQ, ORGT, ORNE)
                    if (SyntaxFacts.doColectAnotherCard(lst[4].symbol) == true)
                    {
                        localTokenLst.AddRange(cSpecRectifier(lst));

                        // colect tokens for this line
                        if (localTokenLst[localTokenLst.Count - 1].kind == TokenKind.TK_BLOCKSTART)
                            localTokenLst.RemoveAt(localTokenLst.Count - 1);

                        // return blank list this will request another card
                        return new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_BLOCKSTART, 0, 0, "", "!__ReqCard__") });
                    }
                    if (localTokenLst.Count > 0)
                    {
                        // move local tokens to returning list 
                        // then reset for next run
                        ret = new List<SyntaxToken>(localTokenLst);
                        localTokenLst.Clear();

                        // add block start token
                        ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, ret[ret.Count - 1].line, computeCharPos(ret[ret.Count - 1].pos), ""));
                        ret.AddRange(cSpecRectifier(lst));
                        return ret;
                    }

                    // ------------------------------------------------------------------------------------------------------------------------------
                    // do standard lex
                    ret = cSpecRectifier(lst);
                    break;
                case 'O':
                    ret = new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_BADTOKEN, lineNo, 0, "", linePos) });
                    break;
                case 'P':
                    ret = new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_BADTOKEN, lineNo, 0, "", linePos) });
                    break;
                default:
                    ret = new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_BADTOKEN, lineNo, 0, "", linePos) });
                    break;
            }


            // return lex tokens
            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> doDecimation3(List<StructCard> cards, ref DiagnosticBag diag)
        {
            bool hasEndToken, doFreeBlock = false;
            char Specification;
            string tmp, line;
            int charPos, tn, lineNo;
            string peekOp, cascadeOp = null;
            List<StructNode> lst;
            List<SyntaxToken> ret;
            StructCard card;

            ret = new List<SyntaxToken>();

            // setup global diagnostic bag
            diagnostics = diag;

            for (int i = 0; i < cards.Count(); i++)
            {
                card = cards[i];
                line = card.Line;
                charPos = 0;
                lineNo = card.LinePos;

                // setup line
                tmp = line.PadRight(72);
                Specification = tmp[0];
                lineStart = charPos;

                // end of file line
                if (line.Length > 0)
                    if (line[0] == '\0')
                    {
                        ret.Add(new SyntaxToken(TokenKind.TK_EOI, lineNo, charPos, ""));
                        break;
                    }

                // do not try to decimate a blank line
                if (tmp.Trim() == "")
                    continue;

                // handle comments
                if (tmp[1] == '*' || (tmp[1] == '/' && tmp[2] == '/'))
                    continue;

                if (tmp.Contains(";") == true && doFreeBlock == false)
                {
                    diagnostics.reportSemiColonInFixedFormat(lineNo, tmp.IndexOf(";"));
                    ret.Add(new SyntaxToken(TokenKind.TK_SEMI, lineNo, tmp.IndexOf(";"), ";", lineNo));
                }

                // perform free block
                if (tmp.Substring(0,7).Contains("/FREE") == true)
                {
                    doFreeBlock = true;
                    continue;
                }
                if (tmp.Substring(0,11).Contains("/END-FREE") == true)
                {
                    doFreeBlock = false;
                    continue;
                }
                if (doFreeBlock == true)
                {
                    tmp = tmp.Trim();
                    ret.AddRange(doLex(new StructNode(lineNo, 0, tmp), true));
                    continue;
                }


                // begin decimation
                switch (Specification)
                {
                    case 'H':
                        lst = decimateHSpec(lineNo, tmp);
                        ret.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                        break;
                    case 'F':
                        lst = decimateFSpec(lineNo, tmp);
                        ret.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                        break;
                    case 'D':
                        lst = decimateDSpec(lineNo, tmp);
                        ret.AddRange(dSpecRectifier(lst));
                        break;
                    case 'I':
                        ret.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, linePos, 0, "", 1));
                        break;
                    case 'C':
                        lst = decimateCSpec(lineNo, tmp);

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
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_IF, lst[0].linePos, computeCharPos(lst[0].chrPos), "", lst[0].chrPos));

                            // add a AND/ OR token if needed
                            switch (lst[0].symbol)
                            {
                                case "AN":
                                    localTokenLst.Add(new SyntaxToken(TokenKind.TK_AND, lst[0].linePos, computeCharPos(lst[0].chrPos), "AN", lst[0].chrPos));
                                    break;
                                case "OR":
                                    localTokenLst.Add(new SyntaxToken(TokenKind.TK_OR, lst[0].linePos, computeCharPos(lst[0].chrPos), "OR", lst[0].chrPos));
                                    break;
                                case "":
                                    localTokenLst.Add(new SyntaxToken(TokenKind.TK_SPACE, lst[0].linePos, computeCharPos(lst[0].chrPos), "", lst[0].chrPos));
                                    break;
                                default:
                                    localTokenLst.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, lst[0].linePos, computeCharPos(lst[0].chrPos), "", lst[0].chrPos));
                                    break;
                            }

                            // add NOT if needed
                            switch (lst[1].symbol)
                            {
                                case "N":
                                    localTokenLst.Add(new SyntaxToken(TokenKind.TK_NOT, lst[0].linePos, computeCharPos(lst[1].chrPos), "N", lst[0].chrPos));
                                    break;
                                case "":
                                    localTokenLst.Add(new SyntaxToken(TokenKind.TK_SPACE, lst[0].linePos, computeCharPos(lst[1].chrPos), "", lst[0].chrPos));
                                    break;
                                default:
                                    localTokenLst.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, lst[0].linePos, computeCharPos(lst[1].chrPos), "", lst[0].chrPos));
                                    break;
                            }

                            // Add Indicator boolean logic
                            tmp = SyntaxFacts.getAllIndicators().Where(ik => ik == lst[2].symbol).FirstOrDefault();
                            if (tmp != null)
                            {
                                lst[2].symbol = $"*IN{lst[2].symbol}";
                                localTokenLst.AddRange(doLex(lst[2]));
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_EQ, lst[2].linePos, computeCharPos(lst[2].chrPos), "", lst[2].chrPos));
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_INDOFF, lst[2].linePos, computeCharPos(lst[2].chrPos), "", lst[2].chrPos));
                            }

                            // there is no op-code on this line go to next line and repeate this process
                            if (lst[4].symbol == "")
                                continue;
                            else
                            {
                                // compleate hidden goto statement
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_GOTO, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[0].linePos, lst[0].chrPos, $"^^ITag{ITagCnt}", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_ENDIF, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
                                localTokenLst.Add(new SyntaxToken(TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));

                                // merge tokens to main list
                                ret.AddRange(localTokenLst);
                                // add current line to list
                                ret.AddRange(cSpecRectifier(lst));

                                // add ending tag and clear control indicators
                                tmp = $"^^ITag{ITagCnt}";
                                ret.Add(new SyntaxToken(TokenKind.TK_TAG, lst[0].linePos, computeCharPos(lst[0].chrPos), "TAG", tmp, lst[0].chrPos));
                                ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[0].linePos, computeCharPos(lst[0].chrPos), tmp, lst[0].chrPos));
                                ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, "", lst[0].chrPos));
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
                                cascadeOp = cascadeOp.Substring(0,2);
                            else
                                cascadeOp = "";

                            // add block start token when the next token is not apart of the cascade
                            // this will compleate the if cascade
                            if (SyntaxFacts.doColectAnotherCard(peekOp) == false || SyntaxFacts.cascadeBlockStart(peekOp) == true)
                            {
                                // add block start token if needed
                                if (ret[ret.Count - 1].kind != TokenKind.TK_BLOCKSTART)
                                {
                                    ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, 0, 0, "", lst[0].chrPos));
                                    ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, 0, 0, cascadeOp, lst[0].chrPos));
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
                        ret = new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_BADTOKEN, lineNo, 0, "", linePos) });
                        break;
                    case 'P':
                        lst = decimatePSpec(lineNo, tmp);
                        ret.AddRange(pSpecRectifier(lst));
                        break;
                    default:
                        ret = new List<SyntaxToken>(new SyntaxToken[] { new SyntaxToken(TokenKind.TK_BADTOKEN, lineNo, 0, "", linePos) });
                        break;
                }
            }

            // ret has nothing to return move local ist to ret
            if (localTokenLst.Count() > 0)
            {
                ret.AddRange(localTokenLst);
                localTokenLst.Clear();
            }

            // check if end token is in list
            hasEndToken = ret.Select(tkn => tkn.kind == TokenKind.TK_EOI).FirstOrDefault();

            // add end of input token
            if (hasEndToken == false)
            {
                ret.Add(new SyntaxToken(TokenKind.TK_EOI, 0, 0, ""));

                ITagCnt = 0;
            }

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
                    ret.Add(new SyntaxToken(TokenKind.TK_PROCDCL, computeCharPos(lst[0].linePos), 1, "B", lst[0].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[1].linePos, computeCharPos(lst[1].chrPos), lst[0].symbol.Trim(), lst[1].chrPos));
                    break;
                case "E":
                    ret.Add(new SyntaxToken(TokenKind.TK_ENDPROC, computeCharPos(lst[0].linePos), 1, lst[0].symbol.Trim(), lst[0].chrPos));
                    break;
                default:
                    ret.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, lst[1].linePos, computeCharPos(lst[1].chrPos), lst[1].symbol, lst[1].chrPos));
                    diagnostics.reportBadProcedure(lst[0].linePos, lst[0].chrPos);
                    break;
            }
            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, lst[1].linePos, computeCharPos(lst[1].chrPos), "", lst[1].chrPos));

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
                ret.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, lst[itmCnt].linePos, lst[itmCnt].chrPos, "", lst[itmCnt].chrPos));
                diagnostics.reportMissingFactor1(new TextSpan(lst[3].chrPos, lst[3].symbol.Length),
                                                 lst[3].chrPos);
            }

            // rectify structured code to free lexicon
            if (itmCnt >= 6)
            {
                // check if the opcode is valid
                OpCode = lst[4].symbol.Trim();
                if (SyntaxFacts.isValidOpCode(OpCode) == false)
                {
                    diagnostics.reportBadOpcode(OpCode, lst[4].linePos, lst[4].chrPos);
                    ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[4].linePos, computeCharPos(lst[4].chrPos), OpCode, lst[4].chrPos));
                    return ret;
                }

                // one or more of the symbols are not left justified
                if (leftJustified(lst) != null)
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
                        diagnostics.reportOpCodeNotAlone(lst[4].linePos, lst[4].chrPos, lst[4].symbol);
                        ret.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, snode.linePos, computeCharPos(snode.chrPos), snode.symbol));
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
                            ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, RESULT.linePos, computeCharPos(RESULT.chrPos), OpCode, RESULT.chrPos));
                            ret.AddRange(doLex(RESULT));
                            ret.AddRange(doLex(OP));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        }
                        else
                        {
                            // factors 1,2 and 3
                            ret.AddRange(doLex(RESULT));
                            ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, RESULT.linePos, computeCharPos(RESULT.chrPos), OpCode, RESULT.chrPos));
                            ret.AddRange(doLex(FAC1));
                            ret.AddRange(doLex(OP));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));

                            if (OpCode == "DIV")
                            {
                                ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, OP.linePos, computeCharPos(OP.chrPos), "^^LO", OP.chrPos));
                                ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, OP.linePos, computeCharPos(OP.chrPos), "=", OP.chrPos));
                                snode = new StructNode(0, 0, $"%REM({FAC1.symbol}:{FAC2.symbol})");
                                ret.AddRange(doLex(snode));
                                ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                            }
                        }
                        break;
                    case "BEGSR":
                        ret.Add(new SyntaxToken(TokenKind.TK_PROCDCL, computeCharPos(OP.linePos), 1, "BEGSR", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, FAC1.linePos, computeCharPos(FAC1.chrPos), FAC1.symbol.Trim(), FAC1.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        break;
                    case "MVR":
                        ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, RESULT.linePos, computeCharPos(RESULT.chrPos), RESULT.symbol, RESULT.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, OP.linePos, computeCharPos(OP.chrPos), "=", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, OP.linePos, computeCharPos(OP.chrPos), "^^LO", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        break;
                    case "CALLB":
                    case "CALLP":
                        onBooleanLine = true;
                        break;
                    case "COMP":
                        snode = leftJustified(lst);
                        ret.AddRange(doLex(getComparisonInd(HI, LO, EQ)));
                        ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, OP.linePos, computeCharPos(OP.chrPos), "COMP", OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(SyntaxFacts.getindicatorOperation(HI.symbol, LO.symbol, EQ.symbol), OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        break;
                    case "CIN":
                        snode = leftJustified(lst);
                        //ret.AddRange(doLex(RESULT));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, RESULT.linePos, computeCharPos(RESULT.chrPos), "", RESULT.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_PARENOPEN, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_PARENCLOSE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        break;
                    case "COUT":
                    case "PRINT":
                    case "DSPLY":
                        ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        break;
                    case "ITER":
                        ret.Add(new SyntaxToken(TokenKind.TK_ITER, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, OP.chrPos, "", OP.chrPos));
                        break;
                    case "LEAVE":
                        if (FAC2.symbol == "" && FAC1.symbol == "")
                        {
                            ret.Add(new SyntaxToken(TokenKind.TK_LEAVE, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, OP.chrPos, "", OP.chrPos));
                        }
                        else
                        {
                            diagnostics.reportOpCodeNotAlone(OP.linePos, OP.chrPos, OP.symbol);
                        }
                        break;
                    case "DO":
                        ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        break;
                    case "DOUGE":
                    case "DOUGT":
                    case "DOULE":
                    case "DOULT":
                    case "DOUEQ":
                    case "DOUNE":
                        ret.Add(new SyntaxToken(TokenKind.TK_DOU, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.AddRange(doLex(OP));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, OP.linePos, computeCharPos(OP.chrPos), "DO", OP.chrPos));
                        lineType = "DOU";
                        break;
                    case "DOU":
                        onBooleanLine = true;
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor
                            ret.Add(new SyntaxToken(TokenKind.TK_SPACE, FAC1.linePos, computeCharPos(FAC1.chrPos), "", OP.chrPos));
                            diagnostics.reportBadFactor(new TextSpan(FAC1.chrPos, FAC1.symbol.Length), 1, FAC1.chrPos);
                        }
                        else
                        {
                            ret.Add(new SyntaxToken(TokenKind.TK_DOU, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                            ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, OP.linePos, computeCharPos(OP.chrPos), "DO", OP.chrPos));
                            lineType = "DOU";
                        }
                        break;
                    case "DOWGE":
                    case "DOWGT":
                    case "DOWLE":
                    case "DOWLT":
                    case "DOWEQ":
                    case "DOWNE":
                        ret.Add(new SyntaxToken(TokenKind.TK_DOW, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.AddRange(doLex(OP));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, OP.linePos, computeCharPos(OP.chrPos), "DO", OP.chrPos));
                        lineType = "DOW";
                        break;
                    case "DOW":
                        onBooleanLine = true;
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor 1
                            ret.Add(new SyntaxToken(TokenKind.TK_SPACE, FAC1.linePos, computeCharPos(FAC1.chrPos), "", FAC1.chrPos));
                            diagnostics.reportBadFactor(new TextSpan(FAC1.chrPos, FAC1.symbol.Length), 1, FAC1.chrPos);
                        }
                        else
                        {
                            ret.Add(new SyntaxToken(TokenKind.TK_DOW, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                            ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, OP.linePos, computeCharPos(OP.chrPos), "DO", OP.chrPos));
                            lineType = "DOW";
                        }
                        break;
                    case "IF":
                        onBooleanLine = true;
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor
                            ret.Add(new SyntaxToken(TokenKind.TK_SPACE, FAC1.linePos, computeCharPos(FAC1.chrPos), "", FAC1.chrPos));
                            diagnostics.reportBadFactor(new TextSpan(FAC1.chrPos, FAC1.symbol.Length), 1, FAC1.chrPos);
                        }
                        else
                        {
                            ret.Add(new SyntaxToken(TokenKind.TK_IF, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                            ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, OP.linePos, computeCharPos(OP.chrPos), "IF", OP.chrPos));
                            lineType = "IF";
                        }
                        break;
                    case "IFGE":
                    case "IFGT":
                    case "IFLE":
                    case "IFLT":
                    case "IFEQ":
                    case "IFNE":
                        ret.Add(new SyntaxToken(TokenKind.TK_IF, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.AddRange(doLex(OP));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, OP.linePos, computeCharPos(OP.chrPos), "IF", OP.chrPos));
                        lineType = "IF";
                        break;
                    case "ELSE":
                        ret.Add(new SyntaxToken(TokenKind.TK_ELSE, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, OP.linePos, computeCharPos(OP.chrPos), "IF", OP.chrPos));
                        lineType = "ELSE";
                        break;
                    case "END":
                        ret.Add(new SyntaxToken(TokenKind.TK_BLOCKEND, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDDO":
                        ret.Add(new SyntaxToken(TokenKind.TK_ENDDO, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDIF":
                        ret.Add(new SyntaxToken(TokenKind.TK_ENDIF, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDFOR":
                        ret.Add(new SyntaxToken(TokenKind.TK_ENDFOR, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDMON":
                        ret.Add(new SyntaxToken(TokenKind.TK_ENDMON, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDSL":
                        ret.Add(new SyntaxToken(TokenKind.TK_ENDSL, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ENDSR":
                        ret.Add(new SyntaxToken(TokenKind.TK_ENDPROC, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        lineType = "";
                        break;
                    case "EXSR":
                        ret.Add(new SyntaxToken(TokenKind.TK_EXSR, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, FAC2.linePos, FAC2.chrPos, FAC2.symbol, FAC2.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, OP.chrPos, "", OP.chrPos));
                        lineType = "";
                        break;
                    case "ORGE":
                    case "ORGT":
                    case "ORLE":
                    case "ORLT":
                    case "OREQ":
                    case "ORNE":
                        ret.Add(new SyntaxToken(TokenKind.TK_OR, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
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
                        ret.Add(new SyntaxToken(TokenKind.TK_AND, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC1));
                        ret.Add(getComparisonOpCode(OP));
                        ret.AddRange(doLex(FAC2));
                        break;
                    case "EVAL":
                    case "EVALR":
                        onEvalLine = true;
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor
                            ret.Add(new SyntaxToken(TokenKind.TK_SPACE, FAC1.linePos, computeCharPos(FAC1.chrPos), "", FAC1.chrPos));
                            diagnostics.reportBadFactor(new TextSpan(FAC1.chrPos, FAC1.symbol.Length), 1, FAC1.chrPos);
                        }
                        else
                        {
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        }
                        break;
                    case "FOR":
                        onEvalLine = true;
                        if (FAC1.symbol != "")
                        {
                            // somthing was entered in factor
                            ret.Add(new SyntaxToken(TokenKind.TK_SPACE, FAC1.linePos, computeCharPos(FAC1.chrPos), "", FAC1.chrPos));
                            diagnostics.reportBadFactor(new TextSpan(FAC1.chrPos, FAC1.symbol.Length), 1, FAC1.chrPos);
                        }
                        else
                        {
                            ret.AddRange(doLex(OP));
                            ret.AddRange(doLex(FAC2));
                            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                            ret.Add(new SyntaxToken(TokenKind.TK_BLOCKSTART, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        }
                        break;
                    case "LEAVESR":
                        if (FAC2.symbol == "" && FAC1.symbol == "")
                        {
                            ret.Add(new SyntaxToken(TokenKind.TK_RETURN, FAC2.linePos, computeCharPos(FAC2.chrPos), "RETURN", FAC2.chrPos));
                            ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        }
                        else
                        {
                            diagnostics.reportOpCodeNotAlone(OP.linePos, OP.chrPos, OP.symbol);
                        }
                        break;
                    case "MOVE":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, RESULT.linePos, computeCharPos(OP.chrPos), "MOVE", RESULT.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        break;
                    case "RETURN":
                        ret.Add(new SyntaxToken(TokenKind.TK_RETURN, FAC2.linePos, computeCharPos(FAC2.chrPos), "RETURN", FAC2.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
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
                            ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                            ret.Add(new SyntaxToken(
                                ((OP.symbol == "SETON") ? TokenKind.TK_INDON : TokenKind.TK_INDOFF),
                                OP.linePos,
                                computeCharPos(OP.chrPos),
                                "", OP.chrPos));
                        }
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, lst[0].linePos, lst[0].chrPos, ""));
                        break;
                    case "TAG":
                        tToken = new SyntaxToken(TokenKind.TK_TAG, OP.linePos, computeCharPos(OP.chrPos), OpCode, FAC1.symbol, OP.chrPos);
                        ret.Add(tToken);
                        ret.AddRange(doLex(FAC1));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        break;
                    case "GOTO":
                        ret.Add(new SyntaxToken(TokenKind.TK_GOTO, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        break;
                    case "Z-ADD":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, RESULT.linePos, RESULT.chrPos, "", RESULT.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_INTEGER, OP.linePos, OP.chrPos, "0", OP.chrPos));
                        ret.Add(new SyntaxToken(TokenKind.TK_ADD, OP.linePos, computeCharPos(OP.chrPos), OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, computeCharPos(OP.chrPos), "", OP.chrPos));
                        break;
                    case "Z-SUB":
                        ret.AddRange(doLex(RESULT));
                        ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, RESULT.linePos, RESULT.chrPos, "", RESULT.chrPos));
                        ret.AddRange(doLex(new StructNode(OP.linePos, OP.chrPos, "0")));
                        ret.Add(new SyntaxToken(TokenKind.TK_SUB, OP.linePos, OP.chrPos, OpCode, OP.chrPos));
                        ret.AddRange(doLex(FAC2));
                        ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, OP.linePos, RESULT.chrPos, "", OP.chrPos));
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

            return ret;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> dSpecRectifier(List<StructNode> lst)
        {
            List<SyntaxToken> ret = new List<SyntaxToken>();
            string dclType;
            TokenKind tk;

            dclType = lst[3].symbol;

            switch (dclType)
            {
                case "C":
                    ret.Add(new SyntaxToken(TokenKind.TK_VARDCONST, computeCharPos(lst[0].linePos), 1, "C", lst[3].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[0].linePos, computeCharPos(lst[0].chrPos), lst[0].symbol, lst[0].chrPos));
                    //ret.Add(new SyntaxToken(SyntaxFacts.getRPGType(lst[6].symbol), computeCharPos(lst[6].linePos), lst[6].chrPos, lst[6].symbol, lst[6].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, computeCharPos(lst[0].linePos), 0, "", lst[0].chrPos));
                    break;
                case "DS":
                    DBlockType = "pr";
                    break;
                case "PI":
                    ret.Add(new SyntaxToken(TokenKind.TK_PROCINFC, computeCharPos(lst[0].linePos), 1, "Pi", lst[3].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[0].linePos, computeCharPos(lst[0].chrPos), "*n", lst[1].chrPos));
                    if (lst.Count > 4)
                        ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[6].linePos, computeCharPos(lst[6].chrPos), lst[6].symbol, lst[6].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, computeCharPos(lst[0].linePos), 0, "", lst[0].chrPos));
                    DBlockType = "pi";
                    break;
                case "PR":
                    break;
                case "S":
                    ret.Add(new SyntaxToken(TokenKind.TK_VARDECLR, computeCharPos(lst[0].linePos), 1, "S", lst[3].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[0].linePos, computeCharPos(lst[0].chrPos), lst[0].symbol, lst[1].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[6].linePos, computeCharPos(lst[6].chrPos), lst[6].symbol, lst[6].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, computeCharPos(lst[0].linePos), 0, "", lst[0].chrPos));
                    break;
                default:
                    // PR and PI block paramiters
                    // remove end index and its new line
                    if (PrPi_Idx > 0 && ret != null && ret.Count > (PrPi_Idx+1))
                    {
                        ret.RemoveAt(PrPi_Idx);
                        ret.RemoveAt(PrPi_Idx+1);
                    }

                    // add paramiter
                    ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[0].linePos, computeCharPos(lst[0].chrPos), lst[0].symbol, lst[1].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[6].linePos, computeCharPos(lst[6].chrPos), lst[6].symbol, lst[6].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, computeCharPos(lst[0].linePos), 0, "", lst[0].chrPos));

                    // add end index and save its position
                    tk = (DBlockType == "pi") ? TokenKind.TK_ENDPI : TokenKind.TK_ENDPR;
                    ret.Add(new SyntaxToken(tk, computeCharPos(lst[0].linePos), 1, "", lst[0].chrPos));
                    ret.Add(new SyntaxToken(TokenKind.TK_NEWLINE, computeCharPos(lst[0].linePos), 0, "", lst[0].chrPos));
                    PrPi_Idx = (ret.Count - 1);
                    break;
            }


            return ret;
        }
    }
}