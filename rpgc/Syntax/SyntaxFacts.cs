using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public static class SyntaxFacts
    {
        public static char[] badChars_Var = { '.', '\\', ',', '<', '>', '?', ';', ':', '"', '\'', '[', ']', '{', '}', '+', '*', '-', '=', '(', ')', '^', '!', '`', '~'};
        public static char[] badChars_ToStartVar = { '_', '&','0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };


        // extention method 
        // useage: tokenKind.TK_ADD.getBinaryOporatorPrecedence()
        public static int getBinaryOporatorPrecedence(this TokenKind tok)
        {
            switch (tok)
            {
                case TokenKind.TK_MULT:
                case TokenKind.TK_DIV:
                    return 5;
                case TokenKind.TK_ADD:
                case TokenKind.TK_SUB:
                    return 4;
                case TokenKind.TK_EQ:
                case TokenKind.TK_NE:
                case TokenKind.TK_GE:
                case TokenKind.TK_GT:
                case TokenKind.TK_LE:
                case TokenKind.TK_LT:
                    return 3;
                case TokenKind.TK_AND:
                    return 2;
                case TokenKind.TK_OR:
                    return 1;
                default:
                    return 0;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        // extention method 
        // useage: tokenKind.TK_ADD.getBinaryOporatorPrecedence()
        public static int getUinaryOporatorPrecedence(this TokenKind tok)
        {
            switch (tok)
            {
                case TokenKind.TK_ADD:
                case TokenKind.TK_SUB:
                case TokenKind.TK_NOT:
                    return 6;
                default:
                    return 0;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static TokenKind getKeywordKind(string symbol)
        {
            symbol = symbol.ToUpper();

            if (symbol.StartsWith("*"))
                return getBuiltInIndicator(symbol);
            if (symbol.StartsWith("%"))
                return getBuiltInFunction(symbol);

            switch (symbol)
            {
                case "ADD":
                    return TokenKind.TK_ADD;
                case "AND":
                    return TokenKind.TK_AND;
                case "DIV":
                    return TokenKind.TK_DIV;
                case "IF":
                    return TokenKind.TK_IF;
                case "IFEQ":
                    return TokenKind.TK_EQ;
                case "IFGE":
                    return TokenKind.TK_GE;
                case "IFGT":
                    return TokenKind.TK_GT;
                case "IFLE":
                    return TokenKind.TK_LE;
                case "IFLT":
                    return TokenKind.TK_LT;
                case "IFNE":
                    return TokenKind.TK_NE;
                case "MOVE":
                    return TokenKind.TK_ASSIGN;
                case "MOVEA":
                    return TokenKind.TK_MOVEA;
                case "MOVEL":
                    return TokenKind.TK_MOVEL;
                case "NOT":
                    return TokenKind.TK_NOT;
                case "MULT":
                    return TokenKind.TK_MULT;
                case "OR":
                    return TokenKind.TK_OR;
                case "SUB":
                    return TokenKind.TK_SUB;
                default:
                    if (char.IsDigit(symbol[0]) == true)
                        return TokenKind.TK_BADTOKEN;
                    else
                        return TokenKind.TK_IDENTIFIER;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static TokenKind getBuiltInFunction(string symbol)
        {
            symbol = symbol.ToUpper();
            switch (symbol)
            {
                case "%ABS":
                    return TokenKind.TK_BIFABS;
                case "%CHAR":
                    return TokenKind.TK_BIFCHAR;
                case "%CHECK":
                    return TokenKind.TK_CHECK;
                case "%CHECKR":
                    return TokenKind.TK_CHECKR;
                case "%DATE":
                    return TokenKind.TK_BIFDATE;
                case "%DAYS":
                    return TokenKind.TK_BIFDAYS;
                case "%DEC":
                    return TokenKind.TK_BIFDEC;
                case "%DECH":
                    return TokenKind.TK_BIFDECH;
                case "%DIFF":
                    return TokenKind.TK_BIFDIFF;
                case "%EDITC":
                    return TokenKind.TK_BIFEDITC;
                case "%EDITW":
                    return TokenKind.TK_BIFEDITW;
                case "%ELEM":
                    return TokenKind.TK_BIFELEM;
                case "%EOF":
                    return TokenKind.TK_BIFEOF;
                case "%EQUAL":
                    return TokenKind.TK_BIFEQUAL;
                case "%ERROR":
                    return TokenKind.TK_BIFERROR;
                case "%FIELDS":
                    return TokenKind.TK_BIFFIELDS;
                case "%FOUND":
                    return TokenKind.TK_BIFFOUND;
                case "%HOURS":
                    return TokenKind.TK_BIFHOURS;
                case "%INTH":
                    return TokenKind.TK_BIFINTH;
                case "%MINUTES":
                    return TokenKind.TK_BIFMINUTES;
                case "%MONTHS":
                    return TokenKind.TK_BIFMONTHS;
                case "%MSSECONDS":
                    return TokenKind.TK_BIFMSECONDS;
                case "%OPEN":
                    return TokenKind.TK_BIFOPEN;
                case "%PARMS":
                    return TokenKind.TK_BIFPARMS;
                case "%REPLACE":
                    return TokenKind.TK_BIFREPLACE;
                case "%SCAN":
                    return TokenKind.TK_BIFSCAN;
                case "%SECONDS":
                    return TokenKind.TK_BIFSECONDS;
                case "%SIZE":
                    return TokenKind.TK_BIFSIZE;
                case "%STATUS":
                    return TokenKind.TK_BIFSTATUS;
                case "%SUBST":
                    return TokenKind.TK_BIFSUBST;
                case "%TIMESTAMP":
                    return TokenKind.TK_BIFTIMESTAMP;
                case "%TRIM":
                    return TokenKind.TK_BIFTRIM;
                case "%TRIML":
                    return TokenKind.TK_BIFTRIML;
                case "%TRIMR":
                    return TokenKind.TK_BIFTRIMR;
                case "%YEARS":
                    return TokenKind.TK_BIFYEARS;
                default:
                    return TokenKind.TK_BADTOKEN;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static TokenKind getBuiltInIndicator(string symbol)
        {
            string num;
            bool isStartsWith_IN, is5CharsLong;
            symbol = symbol.ToUpper();

            // set boolean values
            isStartsWith_IN = symbol.Contains("IN");
            is5CharsLong = (symbol.Length == 5);

            // check boolean values
            if (isStartsWith_IN == true && is5CharsLong == true)
            {
                num = symbol.Substring(3);

                switch (num)
                {
                    case "LR":
                        return TokenKind.TK_INLR;
                    case "L1":
                        return TokenKind.TK_INL1;
                    case "L2":
                        return TokenKind.TK_INL2;
                    case "L3":
                        return TokenKind.TK_INL3;
                    case "L4":
                        return TokenKind.TK_INL4;
                    case "L5":
                        return TokenKind.TK_INL5;
                    case "L6":
                        return TokenKind.TK_INL6;
                    case "L7":
                        return TokenKind.TK_INL7;
                    case "L8":
                        return TokenKind.TK_INL8;
                    case "L9":
                        return TokenKind.TK_INL9;
                    case "M1":
                        return TokenKind.TK_INM1;
                    case "M2":
                        return TokenKind.TK_INM2;
                    case "M3":
                        return TokenKind.TK_INM3;
                    case "M4":
                        return TokenKind.TK_INM4;
                    case "M5":
                        return TokenKind.TK_INM5;
                    case "M6":
                        return TokenKind.TK_INM6;
                    case "M7":
                        return TokenKind.TK_INM7;
                    case "M8":
                        return TokenKind.TK_INM8;
                    case "M9":
                        return TokenKind.TK_INM9;
                    case "OA":
                        return TokenKind.TK_INOA;
                    case "OG":
                        return TokenKind.TK_INOG;
                    case "OV":
                        return TokenKind.TK_INOV;
                    case "RT":
                        return TokenKind.TK_INRT;

                    case "01":
                        return TokenKind.TK_IN01;
                    case "02":
                        return TokenKind.TK_IN02;
                    case "03":
                        return TokenKind.TK_IN03;
                    case "04":
                        return TokenKind.TK_IN04;
                    case "05":
                        return TokenKind.TK_IN05;
                    case "06":
                        return TokenKind.TK_IN06;
                    case "07":
                        return TokenKind.TK_IN07;
                    case "08":
                        return TokenKind.TK_IN08;
                    case "09":
                        return TokenKind.TK_IN09;
                    case "10":
                        return TokenKind.TK_IN10;

                    case "11":
                        return TokenKind.TK_IN11;
                    case "12":
                        return TokenKind.TK_IN12;
                    case "13":
                        return TokenKind.TK_IN13;
                    case "14":
                        return TokenKind.TK_IN14;
                    case "15":
                        return TokenKind.TK_IN15;
                    case "16":
                        return TokenKind.TK_IN16;
                    case "17":
                        return TokenKind.TK_IN17;
                    case "18":
                        return TokenKind.TK_IN18;
                    case "19":
                        return TokenKind.TK_IN19;
                    case "20":
                        return TokenKind.TK_IN20;

                    case "21":
                        return TokenKind.TK_IN21;
                    case "22":
                        return TokenKind.TK_IN22;
                    case "23":
                        return TokenKind.TK_IN23;
                    case "24":
                        return TokenKind.TK_IN24;
                    case "25":
                        return TokenKind.TK_IN25;
                    case "26":
                        return TokenKind.TK_IN26;
                    case "27":
                        return TokenKind.TK_IN27;
                    case "28":
                        return TokenKind.TK_IN28;
                    case "29":
                        return TokenKind.TK_IN29;
                    case "30":
                        return TokenKind.TK_IN30;

                    case "31":
                        return TokenKind.TK_IN31;
                    case "32":
                        return TokenKind.TK_IN32;
                    case "33":
                        return TokenKind.TK_IN33;
                    case "34":
                        return TokenKind.TK_IN34;
                    case "35":
                        return TokenKind.TK_IN35;
                    case "36":
                        return TokenKind.TK_IN36;
                    case "37":
                        return TokenKind.TK_IN37;
                    case "38":
                        return TokenKind.TK_IN38;
                    case "39":
                        return TokenKind.TK_IN39;
                    case "40":
                        return TokenKind.TK_IN40;

                    case "41":
                        return TokenKind.TK_IN41;
                    case "42":
                        return TokenKind.TK_IN42;
                    case "43":
                        return TokenKind.TK_IN43;
                    case "44":
                        return TokenKind.TK_IN44;
                    case "45":
                        return TokenKind.TK_IN45;
                    case "46":
                        return TokenKind.TK_IN46;
                    case "47":
                        return TokenKind.TK_IN47;
                    case "48":
                        return TokenKind.TK_IN48;
                    case "49":
                        return TokenKind.TK_IN49;
                    case "50":
                        return TokenKind.TK_IN50;

                    case "51":
                        return TokenKind.TK_IN51;
                    case "52":
                        return TokenKind.TK_IN52;
                    case "53":
                        return TokenKind.TK_IN53;
                    case "54":
                        return TokenKind.TK_IN54;
                    case "55":
                        return TokenKind.TK_IN55;
                    case "56":
                        return TokenKind.TK_IN56;
                    case "57":
                        return TokenKind.TK_IN57;
                    case "58":
                        return TokenKind.TK_IN58;
                    case "59":
                        return TokenKind.TK_IN59;
                    case "60":
                        return TokenKind.TK_IN60;

                    case "61":
                        return TokenKind.TK_IN61;
                    case "62":
                        return TokenKind.TK_IN62;
                    case "63":
                        return TokenKind.TK_IN63;
                    case "64":
                        return TokenKind.TK_IN64;
                    case "65":
                        return TokenKind.TK_IN65;
                    case "66":
                        return TokenKind.TK_IN66;
                    case "67":
                        return TokenKind.TK_IN67;
                    case "68":
                        return TokenKind.TK_IN68;
                    case "69":
                        return TokenKind.TK_IN69;
                    case "70":
                        return TokenKind.TK_IN70;

                    case "71":
                        return TokenKind.TK_IN71;
                    case "72":
                        return TokenKind.TK_IN72;
                    case "73":
                        return TokenKind.TK_IN73;
                    case "74":
                        return TokenKind.TK_IN74;
                    case "75":
                        return TokenKind.TK_IN75;
                    case "76":
                        return TokenKind.TK_IN76;
                    case "77":
                        return TokenKind.TK_IN77;
                    case "78":
                        return TokenKind.TK_IN78;
                    case "79":
                        return TokenKind.TK_IN89;
                    case "80":
                        return TokenKind.TK_IN30;

                    case "81":
                        return TokenKind.TK_IN81;
                    case "82":
                        return TokenKind.TK_IN82;
                    case "83":
                        return TokenKind.TK_IN83;
                    case "84":
                        return TokenKind.TK_IN84;
                    case "85":
                        return TokenKind.TK_IN85;
                    case "86":
                        return TokenKind.TK_IN86;
                    case "87":
                        return TokenKind.TK_IN87;
                    case "88":
                        return TokenKind.TK_IN88;
                    case "89":
                        return TokenKind.TK_IN89;
                    case "90":
                        return TokenKind.TK_IN90;

                    case "91":
                        return TokenKind.TK_IN91;
                    case "92":
                        return TokenKind.TK_IN92;
                    case "93":
                        return TokenKind.TK_IN93;
                    case "94":
                        return TokenKind.TK_IN94;
                    case "95":
                        return TokenKind.TK_IN95;
                    case "96":
                        return TokenKind.TK_IN96;
                    case "97":
                        return TokenKind.TK_IN97;
                    case "98":
                        return TokenKind.TK_IN98;
                    case "99":
                        return TokenKind.TK_IN99;

                    default:
                        return TokenKind.TK_BADTOKEN;
                }
            }
            else
            {
                switch (symbol)
                {
                    case "*ON":
                        return TokenKind.TK_INDON;
                    case "*OFF":
                        return TokenKind.TK_INDOFF;
                    default:
                        return TokenKind.TK_BADTOKEN;
                }
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static bool isKeyword(string kw)
        {
            int max, mid, min, diff;
            string[] keyWords = {"ACQ","ADDDUR","ALLOC","AND","ASSIGN","BEGSR",
                                "CHAIN","CLEAR",
                                "CLOSE","COMMIT","DEALLOC","DEFINE","DELETE",
                                "DOU","DOW","DSPLY","DUMP","ELSE","END","ENDCS","ENDDO","ENDFOR",
                                "ENDIF","ENDMON","ENDSL","ENDSR","EXCEPT","EXFMT","EXSR",
                                "EXTRCT","FEOD","FOR","FORCE","IF","IN","ITER","LEAVE",
                                "LEAVESR","LITEXPR","MHHZO","MONITOR",
                                "MULT","NOT","OCCUR","OPEN","OR",
                                "OTHER","OUT","POST","READ","READC",
                                "READE","READP","READPE","REALLOC","REL","RESET","RETURN","ROLBK","SCAN",
                                "SELECT","SEMI","SETGT","SETLL","SHTDN","SORTA",
                                "SUBDUR","SUBST",
                                "UNIEXP","UNIOP","UNLOCK","UPDATE","WHEN","WRITE"};

            max = keyWords.Length;
            min = 0;
            mid = max >> 1;

            while (true)
            {
                // check symbol
                if (keyWords[mid] == kw)
                    return true;

                // set new range
                if (keyWords[mid].CompareTo(kw) > 0)
                    min = mid;
                else
                    max = mid;

                // compute next symbol
                diff = (max - mid);
                mid = diff >> 1;
                mid += min;

                // exit symbol not found
                if (diff == 2)
                    return false;
            }
        }


        // //////////////////////////////////////////////////////////////////////////////////
        public static bool isValidOpCode(string kw)
        {
            int max, mid, min, diff;
            string[] keyWords = {"ACQ","ADD","ADDDUR","ALLOC","ANDGE","ANDGT","ANDLE","ANDLT","ASSIGN","BEGSR",
                                "CAB","CALL","CALLB","CALLP","CAS","CAT","CHAIN","CHECK","CHECKR","CLEAR",
                                "CLOSE","COLON","COMMIT","COMP","DATE","DEALLOC","DEFINE","DELETE","DIV",
                                "DO","DOU","DOW","DSPLY","DUMP","ELSE","END","ENDCS","ENDDO","ENDFOR",
                                "ENDIF","ENDMON","ENDSL","ENDSR","EVAL","EVALR","EXCEPT","EXFMT","EXSR",
                                "EXTRCT","FEOD","FOR","FORCE","GOTO","IF","IN","ITER","KFLD","KLIST","LEAVE",
                                "LEAVESR","LITEXPR","LOOKUP","MHHZO","MHLZO","MLHZO","MLLZO","MONITOR","MOVE",
                                "MOVEA","MOVEL","MULT","MVR","NEXT","NONE","NOT","OCCUR","OPEN","ORGE",
                                "ORGT","ORLE","ORLT","OTHER","OUT","PARM","PLIST","POST","READ","READC",
                                "READE","READP","READPE","REALLOC","REL","RESET","RETURN","ROLBK","SCAN",
                                "SELECT","SEMI","SETGT","SETLL","SETOFF","SETON","SHTDN","SORTA","SPACE",
                                "SQRT","SUB","SUBDUR","SUBST","TAG","TEST","TESTB","TESTN","TESTZ","TIME",
                                "UNIEXP","UNIOP","UNLOCK","UPDATE","WHEN","WRITE","XFOOT","XLATE","Z-ADD","Z-SUB",};

            max = keyWords.Length;
            min = 0;
            mid = max >> 1;

            while (true)
            {
                // check symbol
                if (keyWords[mid] == kw)
                    return true;

                // set new range
                if (string.Compare(keyWords[mid],kw) < 0)
                    min = mid;
                else
                    max = mid;

                // compute next symbol
                diff = (max - min);
                mid = diff >> 1;
                mid += min;

                // exit symbol not found
                if (diff <= 1)
                    return false;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static bool isValidVariable(string v)
        {
            // variable must start with a valid char
            // and not contain any bad characters
            if (v.IndexOfAny(badChars_ToStartVar) > 0)
                if (v.IndexOfAny(badChars_Var) < 0)
                    return true;

            return false;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static TokenKind getCompilerConstans(string constKw)
        {
            switch (constKw)
            {
                case "*BLANK":
                case "*BLANKS":
                    return TokenKind.TK_STRING;

                case "*ZERO":
                case "*ZEROS":
                    return TokenKind.TK_INTEGER;
                case "*HIVAL":
                case "*LOVAL":
                case "*ALL":
                case "*ALLX":
                    return TokenKind.TK_BADTOKEN;
            }

            return TokenKind.TK_BADTOKEN;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static SyntaxToken getColum3Kyes(StructNode symbol)
        {
            TokenKind kind;

            switch (symbol.symbol)
            {
                case "N":
                    kind = TokenKind.TK_NOT;
                    break;
                case "":
                    kind = TokenKind.TK_SPACE;
                    break;
                default:
                    kind = TokenKind.TK_BADTOKEN;
                    break;
            }
            return new SyntaxToken(kind, symbol.linePos, symbol.chrPos, symbol.symbol);
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static SyntaxToken getColum2Kyes(StructNode symbol)
        {
            TokenKind kind;

            switch (symbol.symbol)
            {
                case "AN":
                    kind = TokenKind.TK_AND;
                    break;
                case "OR":
                    kind = TokenKind.TK_OR;
                    break;
                case "LR":
                    kind = TokenKind.TK_INLR;
                    break;
                case "L0":
                    kind = TokenKind.TK_INDON;
                    break;
                case "L1":
                    kind = TokenKind.TK_INL1;
                    break;
                case "L2":
                    kind = TokenKind.TK_INL2;
                    break;
                case "L3":
                    kind = TokenKind.TK_INL3;
                    break;
                case "L4":
                    kind = TokenKind.TK_INL4;
                    break;
                case "L5":
                    kind = TokenKind.TK_INL5;
                    break;
                case "L6":
                    kind = TokenKind.TK_INL6;
                    break;
                case "L7":
                    kind = TokenKind.TK_INL7;
                    break;
                case "L8":
                    kind = TokenKind.TK_INL8;
                    break;
                case "L9":
                    kind = TokenKind.TK_INL9;
                    break;
                case "SR":
                case "":
                    kind = TokenKind.TK_SPACE;
                    break;
                default:
                    kind = TokenKind.TK_BADTOKEN;
                    break;
            }
            return new SyntaxToken(kind, symbol.linePos, symbol.chrPos, symbol.symbol);
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static SyntaxToken getIdentifierToken(StructNode symbol)
        {
            string sym;
            TokenKind kind;

            sym = symbol.symbol;

            // check if there are any numbers at the start of variable
            if (char.IsDigit(sym[0]) == true)
                kind = TokenKind.TK_BADTOKEN;
            else
            {
                kind = TokenKind.TK_BADTOKEN;

                // check if there are illegal chars in name
                if (sym.IndexOfAny(badChars_ToStartVar) > 0)
                    if (sym.IndexOfAny(badChars_Var) < 0)
                        kind = TokenKind.TK_IDENTIFIER;
            }

            return new SyntaxToken(kind, symbol.linePos, symbol.chrPos, sym);
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static SyntaxToken getKeywordToken(StructNode symbol)
        {
            TokenKind kind;

            switch (symbol.symbol)
            {
                case "ADD":
                    kind = TokenKind.TK_ADD;
                    break;
                case "AND":
                    kind = TokenKind.TK_AND;
                    break;
                case "DIV":
                    kind = TokenKind.TK_DIV;
                    break;
                case "IF":
                    kind = TokenKind.TK_IF;
                    break;
                case "IFEQ":
                    kind = TokenKind.TK_EQ;
                    break;
                case "IFGE":
                    kind = TokenKind.TK_GE;
                    break;
                case "IFGT":
                    kind = TokenKind.TK_GT;
                    break;
                case "IFLE":
                    kind = TokenKind.TK_LE;
                    break;
                case "IFLT":
                    kind = TokenKind.TK_LT;
                    break;
                case "IFNE":
                    kind = TokenKind.TK_NE;
                    break;
                case "MOVE":
                    kind = TokenKind.TK_ASSIGN;
                    break;
                case "MOVEA":
                    kind = TokenKind.TK_MOVEA;
                    break;
                case "MOVEL":
                    kind = TokenKind.TK_MOVEL;
                    break;
                case "NOT":
                    kind = TokenKind.TK_NOT;
                    break;
                case "MULT":
                    kind = TokenKind.TK_MULT;
                    break;
                case "OR":
                    kind = TokenKind.TK_OR;
                    break;
                case "SUB":
                    kind = TokenKind.TK_SUB;
                    break;
                default:
                    if (char.IsDigit(symbol.symbol[0]) == true)
                        kind = TokenKind.TK_BADTOKEN;
                    else
                        kind = TokenKind.TK_IDENTIFIER;
                    break;
            }

            return new SyntaxToken(kind, symbol.linePos, symbol.chrPos, symbol.symbol);
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        public static SyntaxToken twoDigitIndicators(StructNode ind)
        {
            TokenKind kind;

            switch (ind.symbol)
            {
                case "":
                case "  ":
                    kind = TokenKind.TK_SPACE;
                    break;
                case "01":
                    kind = TokenKind.TK_IN01;
                    break;
                case "02":
                    kind = TokenKind.TK_IN02;
                    break;
                case "03":
                    kind = TokenKind.TK_IN03;
                    break;
                case "04":
                    kind = TokenKind.TK_IN04;
                    break;
                case "05":
                    kind = TokenKind.TK_IN05;
                    break;
                case "06":
                    kind = TokenKind.TK_IN06;
                    break;
                case "07":
                    kind = TokenKind.TK_IN07;
                    break;
                case "08":
                    kind = TokenKind.TK_IN08;
                    break;
                case "09":
                    kind = TokenKind.TK_IN09;
                    break;
                case "10":
                    kind = TokenKind.TK_IN10;
                    break;
                case "11":
                    kind = TokenKind.TK_IN11;
                    break;
                case "12":
                    kind = TokenKind.TK_IN12;
                    break;
                case "13":
                    kind = TokenKind.TK_IN13;
                    break;
                case "14":
                    kind = TokenKind.TK_IN14;
                    break;
                case "15":
                    kind = TokenKind.TK_IN15;
                    break;
                case "16":
                    kind = TokenKind.TK_IN16;
                    break;
                case "17":
                    kind = TokenKind.TK_IN17;
                    break;
                case "18":
                    kind = TokenKind.TK_IN18;
                    break;
                case "19":
                    kind = TokenKind.TK_IN19;
                    break;
                case "20":
                    kind = TokenKind.TK_IN20;
                    break;
                case "21":
                    kind = TokenKind.TK_IN21;
                    break;
                case "22":
                    kind = TokenKind.TK_IN22;
                    break;
                case "23":
                    kind = TokenKind.TK_IN23;
                    break;
                case "24":
                    kind = TokenKind.TK_IN24;
                    break;
                case "25":
                    kind = TokenKind.TK_IN25;
                    break;
                case "26":
                    kind = TokenKind.TK_IN26;
                    break;
                case "27":
                    kind = TokenKind.TK_IN27;
                    break;
                case "28":
                    kind = TokenKind.TK_IN28;
                    break;
                case "29":
                    kind = TokenKind.TK_IN29;
                    break;
                case "30":
                    kind = TokenKind.TK_IN30;
                    break;
                case "31":
                    kind = TokenKind.TK_IN31;
                    break;
                case "32":
                    kind = TokenKind.TK_IN32;
                    break;
                case "33":
                    kind = TokenKind.TK_IN33;
                    break;
                case "34":
                    kind = TokenKind.TK_IN34;
                    break;
                case "35":
                    kind = TokenKind.TK_IN35;
                    break;
                case "36":
                    kind = TokenKind.TK_IN36;
                    break;
                case "37":
                    kind = TokenKind.TK_IN37;
                    break;
                case "38":
                    kind = TokenKind.TK_IN38;
                    break;
                case "39":
                    kind = TokenKind.TK_IN39;
                    break;
                case "40":
                    kind = TokenKind.TK_IN40;
                    break;
                case "41":
                    kind = TokenKind.TK_IN41;
                    break;
                case "42":
                    kind = TokenKind.TK_IN42;
                    break;
                case "43":
                    kind = TokenKind.TK_IN43;
                    break;
                case "44":
                    kind = TokenKind.TK_IN44;
                    break;
                case "45":
                    kind = TokenKind.TK_IN45;
                    break;
                case "46":
                    kind = TokenKind.TK_IN46;
                    break;
                case "47":
                    kind = TokenKind.TK_IN47;
                    break;
                case "48":
                    kind = TokenKind.TK_IN48;
                    break;
                case "49":
                    kind = TokenKind.TK_IN49;
                    break;
                case "50":
                    kind = TokenKind.TK_IN50;
                    break;
                case "51":
                    kind = TokenKind.TK_IN51;
                    break;
                case "52":
                    kind = TokenKind.TK_IN52;
                    break;
                case "53":
                    kind = TokenKind.TK_IN53;
                    break;
                case "54":
                    kind = TokenKind.TK_IN54;
                    break;
                case "55":
                    kind = TokenKind.TK_IN55;
                    break;
                case "56":
                    kind = TokenKind.TK_IN56;
                    break;
                case "57":
                    kind = TokenKind.TK_IN57;
                    break;
                case "58":
                    kind = TokenKind.TK_IN58;
                    break;
                case "59":
                    kind = TokenKind.TK_IN59;
                    break;
                case "60":
                    kind = TokenKind.TK_IN60;
                    break;
                case "61":
                    kind = TokenKind.TK_IN61;
                    break;
                case "62":
                    kind = TokenKind.TK_IN62;
                    break;
                case "63":
                    kind = TokenKind.TK_IN63;
                    break;
                case "64":
                    kind = TokenKind.TK_IN64;
                    break;
                case "65":
                    kind = TokenKind.TK_IN65;
                    break;
                case "66":
                    kind = TokenKind.TK_IN66;
                    break;
                case "67":
                    kind = TokenKind.TK_IN67;
                    break;
                case "68":
                    kind = TokenKind.TK_IN68;
                    break;
                case "69":
                    kind = TokenKind.TK_IN69;
                    break;
                case "70":
                    kind = TokenKind.TK_IN70;
                    break;
                case "71":
                    kind = TokenKind.TK_IN71;
                    break;
                case "72":
                    kind = TokenKind.TK_IN72;
                    break;
                case "73":
                    kind = TokenKind.TK_IN73;
                    break;
                case "74":
                    kind = TokenKind.TK_IN74;
                    break;
                case "75":
                    kind = TokenKind.TK_IN75;
                    break;
                case "76":
                    kind = TokenKind.TK_IN76;
                    break;
                case "77":
                    kind = TokenKind.TK_IN77;
                    break;
                case "78":
                    kind = TokenKind.TK_IN78;
                    break;
                case "79":
                    kind = TokenKind.TK_IN79;
                    break;
                case "80":
                    kind = TokenKind.TK_IN80;
                    break;
                case "81":
                    kind = TokenKind.TK_IN81;
                    break;
                case "82":
                    kind = TokenKind.TK_IN82;
                    break;
                case "83":
                    kind = TokenKind.TK_IN83;
                    break;
                case "84":
                    kind = TokenKind.TK_IN84;
                    break;
                case "85":
                    kind = TokenKind.TK_IN85;
                    break;
                case "86":
                    kind = TokenKind.TK_IN86;
                    break;
                case "87":
                    kind = TokenKind.TK_IN87;
                    break;
                case "88":
                    kind = TokenKind.TK_IN88;
                    break;
                case "89":
                    kind = TokenKind.TK_IN89;
                    break;
                case "90":
                    kind = TokenKind.TK_IN90;
                    break;

                case "91":
                    kind = TokenKind.TK_IN91;
                    break;
                case "92":
                    kind = TokenKind.TK_IN92;
                    break;
                case "93":
                    kind = TokenKind.TK_IN93;
                    break;
                case "94":
                    kind = TokenKind.TK_IN94;
                    break;
                case "95":
                    kind = TokenKind.TK_IN95;
                    break;
                case "96":
                    kind = TokenKind.TK_IN96;
                    break;
                case "97":
                    kind = TokenKind.TK_IN97;
                    break;
                case "98":
                    kind = TokenKind.TK_IN98;
                    break;
                case "99":
                    kind = TokenKind.TK_IN99;
                    break;
                case "LR":
                    kind = TokenKind.TK_INLR;
                    break;
                case "H1":
                    kind = TokenKind.TK_INH1;
                    break;
                case "H2":
                    kind = TokenKind.TK_INH2;
                    break;
                case "H3":
                    kind = TokenKind.TK_INH3;
                    break;
                case "H4":
                    kind = TokenKind.TK_INH4;
                    break;
                case "H5":
                    kind = TokenKind.TK_INH5;
                    break;
                case "H6":
                    kind = TokenKind.TK_INH6;
                    break;
                case "H7":
                    kind = TokenKind.TK_INH7;
                    break;
                case "H8":
                    kind = TokenKind.TK_INH8;
                    break;
                case "H9":
                    kind = TokenKind.TK_INH9;
                    break;
                case "KA":
                    kind = TokenKind.TK_INKA;
                    break;
                case "KB":
                    kind = TokenKind.TK_INKB;
                    break;
                case "KC":
                    kind = TokenKind.TK_INKC;
                    break;
                case "KD":
                    kind = TokenKind.TK_INKD;
                    break;
                case "KE":
                    kind = TokenKind.TK_INKE;
                    break;
                case "KF":
                    kind = TokenKind.TK_INKF;
                    break;
                case "KG":
                    kind = TokenKind.TK_INKG;
                    break;
                case "KH":
                    kind = TokenKind.TK_INKH;
                    break;
                case "KI":
                    kind = TokenKind.TK_INKI;
                    break;
                case "KJ":
                    kind = TokenKind.TK_INKJ;
                    break;
                case "KK":
                    kind = TokenKind.TK_INKK;
                    break;
                case "KL":
                    kind = TokenKind.TK_INKL;
                    break;
                case "KM":
                    kind = TokenKind.TK_INKM;
                    break;
                case "KN":
                    kind = TokenKind.TK_INKN;
                    break;
                case "KO":
                    kind = TokenKind.TK_INKO;
                    break;
                case "KP":
                    kind = TokenKind.TK_INKP;
                    break;
                case "KQ":
                    kind = TokenKind.TK_INKQ;
                    break;
                case "KR":
                    kind = TokenKind.TK_INKR;
                    break;
                case "KS":
                    kind = TokenKind.TK_INKS;
                    break;
                case "KT":
                    kind = TokenKind.TK_INKT;
                    break;
                case "KU":
                    kind = TokenKind.TK_INKU;
                    break;
                case "KV":
                    kind = TokenKind.TK_INKV;
                    break;
                case "KW":
                    kind = TokenKind.TK_INKW;
                    break;
                case "KX":
                    kind = TokenKind.TK_INKX;
                    break;
                case "1":
                    kind = TokenKind.TK_INL1;
                    break;
                case "L2":
                    kind = TokenKind.TK_INL2;
                    break;
                case "L3":
                    kind = TokenKind.TK_INL3;
                    break;
                case "L4":
                    kind = TokenKind.TK_INL4;
                    break;
                case "L5":
                    kind = TokenKind.TK_INL5;
                    break;
                case "L6":
                    kind = TokenKind.TK_INL6;
                    break;
                case "L7":
                    kind = TokenKind.TK_INL7;
                    break;
                case "L8":
                    kind = TokenKind.TK_INL8;
                    break;
                case "L9":
                    kind = TokenKind.TK_INL9;
                    break;
                case "M1":
                    kind = TokenKind.TK_INM1;
                    break;
                case "M2":
                    kind = TokenKind.TK_INM2;
                    break;
                case "M3":
                    kind = TokenKind.TK_INM3;
                    break;
                case "M4":
                    kind = TokenKind.TK_INM4;
                    break;
                case "M5":
                    kind = TokenKind.TK_INM5;
                    break;
                case "M6":
                    kind = TokenKind.TK_INM6;
                    break;
                case "M7":
                    kind = TokenKind.TK_INM7;
                    break;
                case "M8":
                    kind = TokenKind.TK_INM8;
                    break;
                case "M9":
                    kind = TokenKind.TK_INM9;
                    break;
                case "MR":
                    kind = TokenKind.TK_INMR;
                    break;
                case "OA":
                    kind = TokenKind.TK_INOA;
                    break;
                case "OG":
                    kind = TokenKind.TK_INOG;
                    break;
                case "OV":
                    kind = TokenKind.TK_INOV;
                    break;
                case "RT":
                    kind = TokenKind.TK_INRT;
                    break;

                default:
                    kind = TokenKind.TK_BADTOKEN;
                    break;
            }

            return new SyntaxToken(kind, ind.linePos, ind.chrPos, ind.symbol);
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static string getText(TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.TK_ADD:
                    return "+";
            }

            return null;
        }
    }
}
