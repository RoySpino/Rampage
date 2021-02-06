using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class StructNode
    {
        public int linePos;
        public int chrPos;
        public string symbol;

        public StructNode(int l, int ch, string sym)
        {
            linePos = l;
            chrPos = ch;
            symbol = sym;
        }
    }

    // ////////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     /////
    // //////////////////////////////////////////////////////////////////////////
    public static class Decimator
    {
        static string specChkStr = "H";
        static bool isProcSection = false;
        static TokenKind kind;
        static char curChar;
        static object Value;
        static int start;
        static string tmpVal;
        static DiagnosticBag diagnostics = new DiagnosticBag();
        static int assignmentCnt;
        static bool onEvalLine;

        private static bool isGoodSpec(char spec)
        {
            char prevSpec;
            int prevSpecVal, curSpecVal;
            int[,] specVal = { { 72, 1 }, { 70, 2 }, { 68, 3 }, { 73, 4 }, { 67, 5 }, { 79, 6 } };

            prevSpec = specChkStr[specChkStr.Length - 1];
            prevSpecVal = 1;
            curSpecVal = -1;

            // see if current line is in the pocedure section
            if (spec == 'P' && (prevSpec == 'C' || prevSpec == 'P'))
            {
                isProcSection = true;
                specChkStr = "D";
                return true;
            }

            // get spec value
            for (int i = 0; i < specVal.Length; i++)
                if ((char)specVal[i, 0] == spec)
                {
                    curSpecVal = specVal[i, 1];
                    break;
                }

            // check if specs are grouped and ordered together
            if (isProcSection == false)
            {
                if (curSpecVal >= prevSpecVal)
                {
                    specChkStr += spec;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (prevSpecVal >= curSpecVal && (curSpecVal == 68 || curSpecVal == 67))
                {
                    specChkStr += spec;
                    return true;
                }
                else
                    return false;
            }
        }

        // //////////////////////////////////////////////////////////////////////////
        private static List<StructNode> decimateCSpec(int lineNo, string line)
        {
            List<StructNode> ret = new List<StructNode>();
            int strLen;
            string sym;
            bool isOnEvalLine = false;

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
            if (isGoodSpec(line[0]) == false)
                return null;

            strLen = line.Length - 1;

            for (int i = 0; i < slicer.Length; i++)
            {
                if (strLen <= 0)
                    break;

                if (isOnEvalLine == false)
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
                isOnEvalLine = (i == 4 && (sym.Contains("EVAL") || sym == "FOR" || sym == "IF"));

                ret.Add(new StructNode(lineNo, slicer[i, 0], sym.TrimEnd()));
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
            if (isGoodSpec(line[0]) == false)
                return null;

            strLen = line.Length - 1;

            for (int i = 0; i < slicer.Length; i++)
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
                if (i == 4 || i == 5 || i == 7)
                    sym = sym.TrimStart();
                else
                    sym = sym.TrimEnd();

                ret.Add(new StructNode(lineNo, slicer[i, 0], sym));
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
            if (isGoodSpec(line[0]) == false)
                return null;

            strLen = line.Length - 1;

            for (int i = 0; i < slicer.Length; i++)
            {
                if (strLen <= 0)
                    break;

                // slice string according to column position
                if (strLen >= slicer[i, 1])
                    sym = line.Substring(slicer[i, 0], slicer[i, 1]);
                else
                    sym = line.Substring(slicer[i, 0]);
                strLen -= slicer[i, 1];

                ret.Add(new StructNode(lineNo, slicer[i, 0], sym));
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
            if (isGoodSpec(line[0]) == false)
                return null;

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

                ret.Add(new StructNode(lineNo, slicer[i, 0], sym));
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
            if (isGoodSpec(line[0]) == false)
                return null;

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

                ret.Add(new StructNode(lineNo, slicer[i, 0], sym));
            }

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> doLex(StructNode factor, string KeyWord = "")//string line, int start)
        {
            List<SyntaxToken> ret = new List<SyntaxToken>();
            string symbol = "", line;
            int sz, linePos, idx;

            line = factor.symbol;
            start = factor.chrPos;
            linePos = factor.linePos;
            sz = line.Length;
            assignmentCnt = 0;

            for (int i = 0; i < sz; i++)
            {
                curChar = line[i];
                idx = (((i + 1) >= sz) ? (sz - 1) : (i + 1));

                // exit decimator lexer
                if (curChar < 33)
                    break;

                // c++ style comment line
                if (linePos >= 1 && (curChar == '/' && line[idx] == '/'))
                    ignoreCommentLine(line, ref i);

                // RPG style comment line
                if (linePos == 2 && curChar == '*')
                    ignoreCommentLine(line, ref i);

                switch (curChar)
                {
                    case '+':
                        start += i;
                        kind = TokenKind.TK_ADD;
                        Value = "+";
                        break;
                    case '-':
                        start += i;
                        kind = TokenKind.TK_SUB;
                        Value = "-";
                        break;
                    case '*':
                        start += i;
                        symbol = readCompilerConstantsOrMult(line, ref i);
                        break;
                    case '/':
                        start += i;
                        kind = TokenKind.TK_DIV;
                        Value = "/";
                        break;
                    case '(':
                        start += i;
                        kind = TokenKind.TK_PARENOPEN;
                        Value = "(";
                        break;
                    case ')':
                        start += i;
                        kind = TokenKind.TK_PARENCLOSE;
                        Value = ")";
                        break;
                    case '=':
                        start += i;
                        kind = getAssignmentOrComparisonToken(line, ref i);
                        Value = "=";
                        break;
                    case '<':
                        start += i;
                        kind = getLessGreaterThanOperator('<', line[idx], ref i);
                        Value = tmpVal;
                        break;
                    case '>':
                        start += i;
                        kind = getLessGreaterThanOperator('>', line[idx], ref i);
                        Value = tmpVal;
                        break;
                    case '%':
                        symbol = readBuiltInFunctions(line, ref i);
                        break;
                    case '@':
                    case '#':
                    case '$':
                        symbol = readIdentifierOrKeyword(line, ref i);
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
                        symbol = readNumberToken(line, ref i);
                        break;
                    case ' ':
                    case '\n':
                    case '\t':
                    case '\r':
                        readWiteSpace(line, ref i);
                        break;
                    default:
                        if (char.IsLetter(curChar) == true)
                            Value = readIdentifierOrKeyword(line, ref i);
                        else
                        {
                            if (char.IsWhiteSpace(curChar) == true)
                            {
                                Value = "";
                                readWiteSpace(line, ref i);
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

                ret.Add(new SyntaxToken(kind, linePos, start, Value));
            }

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static string readNumberToken(string line, ref int pos)
        {
            string symbol = "";
            int intDummy;

            start = pos;

            while (pos < line.Length && char.IsDigit(line[pos]) == true)
            {
                symbol += line[pos];
                pos += 1;
            }
            pos -= 1;

            if (int.TryParse(symbol, out intDummy) == false)
                diagnostics.reportInvalidNumber(symbol, typeof(int), start, symbol.Length);

            kind = TokenKind.TK_INTEGER;
            Value = intDummy;

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static void readWiteSpace(string line, ref int pos)
        {
            string symbol = "";

            start = pos;
            while (pos < line.Length && char.IsWhiteSpace(line[pos]) == true)
            {
                symbol += curChar;
                pos += 1;
            }
            pos -= 1;

            kind = TokenKind.TK_SPACE;
            Value = "";
            start += pos;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static string readIdentifierOrKeyword(string line, ref int pos)
        {
            string symbol = "";

            start = pos;
            while (pos < line.Length && char.IsLetter(line[pos]) == true)
            {
                symbol += line[pos];
                pos += 1;
            }
            pos -= 1;

            // assign keyword token
            kind = SyntaxFacts.getKeywordKind(symbol);
            //tmpTok = new SyntaxToken(SyntaxFacts.getKeywordKind(symbol), lineNum, (start + 1), symbol);

            // chech if symbol is a valid variabel name
            if (kind == TokenKind.TK_BADTOKEN)
                if (SyntaxFacts.isValidVariable(symbol))
                {
                    Value = symbol;
                    kind = TokenKind.TK_IDENTIFIER;
                    start += pos;
                }

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        // special indicators and constants defalts to mult if nothing is found
        private static string readCompilerConstantsOrMult(string line, ref int pos)
        {
            string symbol, peekStr;
            char peekChar;
            int peekPos;

            start = pos;
            peekPos = pos;
            symbol = "*";
            peekStr = "";

            // check if the word is an indicator by checking the first 2 chars
            peekPos += 1;
            pos += 1;
            while (peekPos < line.Length && char.IsLetterOrDigit(line[peekPos]) == true)
            {
                peekStr += line[peekPos];
                peekPos += 1;
                peekChar = line[peekPos];
            }

            symbol = ("*" + peekStr);
            Value = symbol;
            peekStr = ("*" + peekStr.ToUpper());

            // check special case * values
            switch (symbol)
            {
                case "*":
                    kind = TokenKind.TK_MULT;
                    return symbol;
                case "**":
                    kind = TokenKind.TK_EXPONENT;
                    return symbol;
            }

            // check if the symbol is an indicator
            kind = SyntaxFacts.getBuiltInIndicator(peekStr);
            if (kind != TokenKind.TK_BADTOKEN)
            {
                while (pos < line.Length && char.IsLetterOrDigit(line[pos]) == true)
                    pos += 1;
                pos -= 1;
                start += pos;

                return symbol;
            }

            // check compiler constants
            kind = SyntaxFacts.getCompilerConstans(peekStr);
            if (kind != TokenKind.TK_BADTOKEN)
            {
                while (pos < line.Length && char.IsLetterOrDigit(line[pos]) == true)
                    pos += 1;
                pos -= 1;
                start += pos;

                return symbol;
            }

            kind = TokenKind.TK_MULT;
            return "*";
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static string readBuiltInFunctions(string line, ref int pos)
        {
            string symbol = "";

            start = pos;
            symbol = "%";

            pos += 1;
            while (pos < line.Length && char.IsLetter(curChar) == true)
            {
                symbol += curChar;
                pos += 1;
            }
            pos -= 1;

            kind = SyntaxFacts.getBuiltInFunction(symbol.ToUpper());
            Value = symbol;
            start += pos;

            return symbol;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static void ignoreCommentLine(string line, ref int pos)
        {
            start = pos;
            while (true)
            {
                pos += 1;
                if (line[pos] == '\n' || line[pos] == '\0' || line[pos] == '\r')
                    break;
            }
            start += pos;

            kind = TokenKind.TK_SPACE;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static TokenKind getAssignmentOrComparisonToken(string line, ref int pos)
        {
            TokenKind ret;

            // check if the current line is a comparison or assignment
            if (onEvalLine == true && assignmentCnt < 1)
            {
                ret = TokenKind.TK_ASSIGN;
                assignmentCnt += 1;
            }
            else
                ret = TokenKind.TK_EQ;

            // reset boolean
            onEvalLine = true;

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static TokenKind getLessGreaterThanOperator(char first, char op, ref int pos)
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

        // ////////////////////////////////////////////////////////////////////////////
        public static List<StructNode> doDecimation(int lineNo, string line)
        {
            char Specification;
            string tmp;

            tmp = line.ToUpper();
            Specification = line[0];

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

        // //////////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> cSpecRectifier(List<StructNode> lst, ref DiagnosticBag diagnostics)
        {
            List<SyntaxToken> ret = new List<SyntaxToken>();
            int itmCnt;
            string OpCode;
            bool atEOF;

            itmCnt = lst.Count;
            onEvalLine = false;

            // factor 1 is not empty and has no key word
            if (itmCnt == 4 && lst[3].symbol.Length > 0)
            {
                ret.Add(new SyntaxToken(TokenKind.TK_BADTOKEN, lst[itmCnt].linePos, lst[itmCnt].chrPos, lst[3].symbol));
                diagnostics.reportMissingFactor1(new TextSpan(lst[3].chrPos, lst[3].symbol.Length),
                                                 lst[3].linePos);
            }

            // check if there is a EOF char in the string
            atEOF = (lst[itmCnt - 1].symbol.Contains("\0") == true);

            // rectify structured code to free lexicon
            if (itmCnt <= 7)
            {
                // check if the opcode is valid
                OpCode = lst[4].symbol;
                if (SyntaxFacts.isValidOpCode(OpCode) == false)
                {
                    diagnostics.reportBadOpcode(OpCode, lst[4].linePos);
                    ret.Add(new SyntaxToken(TokenKind.TK_IDENTIFIER, lst[4].linePos, lst[4].chrPos, OpCode));
                    return ret;
                }

                // perform rectifier
                switch (OpCode)
                {
                    case "ADD":
                    case "SUB":
                    case "MULT":
                    case "DIV":
                        if (lst[3].symbol == "")
                        {
                            ret.AddRange(doLex(lst[6]));
                            ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, lst[6].linePos, lst[6].chrPos, OpCode));
                            ret.AddRange(doLex(lst[6]));
                            ret.AddRange(doLex(lst[4]));
                            ret.AddRange(doLex(lst[5]));
                        }
                        else
                        {
                            ret.AddRange(doLex(lst[6]));
                            ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, lst[6].linePos, lst[6].chrPos, OpCode));
                            ret.AddRange(doLex(lst[3]));
                            ret.AddRange(doLex(lst[4]));
                            ret.AddRange(doLex(lst[5]));
                        }
                        break;
                    case "DSPLY":
                        break;
                    case "IFGE":
                    case "IFGT":
                    case "IFLE":
                    case "IFLT":
                    case "IFEQ":
                    case "IFNE":
                        ret.AddRange(doLex(lst[3]));
                        ret.AddRange(doLex(lst[4]));
                        ret.AddRange(doLex(lst[5]));
                        break;
                    case "ORGE":
                    case "ORGT":
                    case "ORLE":
                    case "ORLT":
                    case "OREQ":
                    case "ORNE":
                        break;
                    case "ANDGE":
                    case "ANDGT":
                    case "ANDLE":
                    case "ANDLT":
                    case "ANDEQ":
                    case "ANDNE":
                        break;
                    case "EVAL":
                    case "EVALR":
                        onEvalLine = true;
                        if (lst[3].symbol != "")
                        {
                            ret.Add(new SyntaxToken(TokenKind.TK_SPACE, lst[3].linePos, lst[3].chrPos, ""));
                            diagnostics.reportBadFactor(new TextSpan(lst[3].chrPos, lst[3].symbol.Length), 1, lst[3].linePos);
                        }
                        else
                            ret.AddRange(doLex(lst[5]));
                        break;
                    case "MOVE":
                        ret.AddRange(doLex(lst[6]));
                        ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, lst[6].linePos, lst[6].chrPos, "MOVE"));
                        ret.AddRange(doLex(lst[5]));
                        break;
                    case "Z-ADD":
                        ret.AddRange(doLex(lst[6]));
                        ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, lst[6].linePos, lst[6].chrPos, OpCode));
                        ret.Add(new SyntaxToken(TokenKind.TK_INTEGER, lst[4].linePos, lst[4].chrPos, "0"));
                        ret.Add(new SyntaxToken(TokenKind.TK_ADD, lst[4].linePos, lst[4].chrPos, OpCode));
                        ret.AddRange(doLex(lst[5]));
                        break;
                    case "Z-SUB":
                        ret.AddRange(doLex(lst[6]));
                        ret.Add(new SyntaxToken(TokenKind.TK_ASSIGN, lst[6].linePos, lst[6].chrPos, OpCode));
                        ret.AddRange(doLex(new StructNode(lst[4].linePos, lst[4].chrPos, "0")));
                        ret.Add(new SyntaxToken(TokenKind.TK_SUB, lst[4].linePos, lst[4].chrPos, OpCode));
                        ret.AddRange(doLex(lst[5]));
                        break;
                }
            }

            if (atEOF == true)
                ret.Add(new SyntaxToken(TokenKind.TK_EOI, lst[6].linePos, lst[itmCnt - 1].chrPos + lst[itmCnt - 1].symbol.Length, "_"));

            return ret;
        }
    }
}