using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public static class SyntaxFacts
    {
        public static char[] badChars_Var = { '.', '\\', ',', '<', '>', '?', ';', ':', '"', '\'', '[', ']', '{', '}', '+', '*', '-', '=', '(', ')', '^', '!', '`', '~' };
        public static char[] badChars_ToStartVar = { '_', '&', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private static string[] struckeyWords = {
                                "ACQ","ADD","ADDDUR","ALLOC","ANDEQ","ANDGE","ANDGT","ANDLE","ANDLT","ANDNE","ASSIGN","BEGSR",
                                "CAB","CALL","CALLB","CALLP","CAS","CAT","CHAIN","CHECK","CHECKR","CIN","CLEAR","CLOSE","COLON",
                                "COMMIT","COMP","COUT","DATE","DEALLOC","DEFINE","DELETE","DIV","DO","DOU","DOUEQ","DOUGE",
                                "DOUGT", "DOULE","DOULT","DOUNE","DOW","DOWEQ","DOWGE","DOWGT","DOWLE","DOWLT","DOWNE","DSPLY",
                                "DUMP","ELSE","END","ENDCS", "ENDDO","ENDFOR","ENDIF","ENDMON","ENDSL","ENDSR","EVAL","EVALR",
                                "EXCEPT","EXFMT","EXSR", "EXTRCT","FEOD","FOR","FORCE","GOTO","IF","IFEQ","IFGE","IFGT","IFLE",
                                "IFLT","IFNE","IN", "ITER","KFLD","KLIST","LEAVE","LEAVESR","LITEXPR","LOOKUP","MHHZO","MHLZO",
                                "MLHZO","MLLZO", "MONITOR","MOVE","MOVEA","MOVEL","MULT","MVR","NEXT","NONE","NOT","OCCUR","OPEN",
                                "OREQ","ORGE","ORGT","ORLE","ORLT","ORNE","OTHER","OUT","PARM","PLIST","POST","PRINT","READ",
                                "READC","READE","READP","READPE","REALLOC","REL","RESET","RETURN","ROLBK","SCAN","SELECT","SEMI",
                                "SETGT","SETLL","SETOFF","SETON","SHTDN","SORTA","SPACE","SQRT","SUB","SUBDUR","SUBST","TAG","TEST",
                                "TESTB","TESTN","TESTZ","TIME","UNIEXP","UNIOP","UNLOCK","UPDATE","WHEN","WRITE","XFOOT","XLATE",
                                "Z-ADD","Z-SUB"};
        private static string[] freeKeyWords = {"ACQ","ADDDUR","ALLOC","AND","ASSIGN","BEGSR",
                                "CIN","CHAIN","CLEAR",
                                "CLOSE","COMMIT", "COUT","DCL-C","DCL-S","DEALLOC","DEFINE","DELETE",
                                "DOU","DOW","DSPLY","DUMP","ELSE","END","ENDCS","ENDDO","ENDFOR",
                                "ENDIF","ENDMON","ENDSL","ENDSR","EXCEPT","EXFMT","EXSR",
                                "EXTRCT","FEOD","FOR","FORCE","IF","IN","ITER","LEAVE",
                                "LEAVESR","LITEXPR","MHHZO","MONITOR",
                                "MULT","NOT","OCCUR","OPEN","OR",
                                "OTHER","OUT","POST","PRINT","READ","READC",
                                "READE","READP","READPE","REALLOC","REL","RESET","RETURN","ROLBK","SCAN",
                                "SELECT","SEMI","SETGT","SETLL","SHTDN","SORTA",
                                "SUBST","TEST",
                                "UNIEXP","UNIOP","UNLOCK","UPDATE","WHEN","WRITE"};
        private static string[] freeBIFWithNoParan = { "DSPLY", "SORTA", "COUT", "EXSR", "READ", "READE", "SETLL", "SETGT", "OPEN", "CLOSE", "READC", "DELETE", "WRITE", "UPDATE", "CHAIN", "CLEAR", "SORTA", "SUBDUR" };
        private static string[] BIIndicators = { "01","02","03","04","05","06","07","08","09","10","11","12","13","14","15","16","17","18","19","20","21","22","23","24","25","26","27","28","29","30"
                                                ,"31","32","33","34","35","36","37","38","39","40","41","42","43","44","45","46","47","48","49","50","51","52","53","54","55","56","57","58","59","60"
                                                ,"61","62","63","64","65","66","67","68","69","70","71","72","73","74","75","76","77","78","79","80","81","82","83","84","85","86","87","88","89","90"
                                                ,"91","92","93","94","95","96","97","98","99","LR","H1","H2","H3","H4","H5","H6","H7","H8","H9","KA","KB","KC","KD","KE","KF","KG","KH","KI","KJ","KK"
                                                ,"KL","KM","KN","KO","KP","KQ","KR","KS","KT","KU","KV","KW","KX","L0","L1","L2","L3","L4","L5","L6","L7","L8","L9","M1","M2","M3","M4","M5","M6","M7"
                                                ,"M8","M9","MR","OA","OG","OV","RT"};

        public static bool isCharLiteralOrControl(char chr)
        {
            if (chr >= 48 && chr <= 57)
                return true;
            if (chr >= 65 && chr <= 90)
                return true;
            if (chr >= 97 && chr <= 122)
                return true;

            switch (chr)
            {
                case '_':
                case '#':
                case '@':
                case '$':
                    return true;
            }

            return false;
        }

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
                case "BY":
                    return TokenKind.TK_BY;
                case "DIV":
                    return TokenKind.TK_DIV;
                case "DCL-C":
                    return TokenKind.TK_VARDCONST;
                case "DCL-DS":
                    return TokenKind.TK_VARDDATAS;
                case "DCL-PROC":
                    return TokenKind.TK_PROCDCL;
                case "BEGSR":
                    return TokenKind.TK_BEGSR;
                case "DCL-PI":
                    return TokenKind.TK_PROCINFC;
                case "DCL-PR":
                    return TokenKind.TK_BADTOKEN;
                case "DCL-S":
                    return TokenKind.TK_VARDECLR;
                case "DOU":
                    return TokenKind.TK_DOU;
                case "DOUQ":
                    return TokenKind.TK_EQ;
                case "DOUGE":
                    return TokenKind.TK_GE;
                case "DOUGT":
                    return TokenKind.TK_GT;
                case "DOULE":
                    return TokenKind.TK_LE;
                case "DOULT":
                    return TokenKind.TK_LT;
                case "DOUNE":
                    return TokenKind.TK_NE;
                case "DOWEQ":
                    return TokenKind.TK_EQ;
                case "DOWGE":
                    return TokenKind.TK_GE;
                case "DOWGT":
                    return TokenKind.TK_GT;
                case "DOWLE":
                    return TokenKind.TK_LE;
                case "DOWLT":
                    return TokenKind.TK_LT;
                case "DOWNE":
                    return TokenKind.TK_NE;
                case "DOW":
                    return TokenKind.TK_DOW;
                case "DOWNTO":
                    return TokenKind.TK_DOWNTO;
                case "ELSE":
                    return TokenKind.TK_ELSE;
                case "END-PROC":
                case "ENDSR":
                    return TokenKind.TK_ENDPROC;
                case "END-PI":
                    return TokenKind.TK_ENDPI;
                case "ENDDO":
                    return TokenKind.TK_ENDDO;
                case "ENDIF":
                    return TokenKind.TK_ENDIF;
                case "ENDFOR":
                    return TokenKind.TK_ENDFOR;
                case "ENDMON":
                    return TokenKind.TK_ENDMON;
                case "ENDSL":
                    return TokenKind.TK_ENDSL;
                case "END":
                    return TokenKind.TK_BLOCKEND;
                case "INZ":
                    return TokenKind.TK_INZ;
                case "FOR":
                    return TokenKind.TK_FOR;
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
                case "INT10":
                    return TokenKind.TK_INTEGER;
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
                case "MONITOR":
                    return TokenKind.TK_MONITOR;
                case "OR":
                    return TokenKind.TK_OR;
                case "SELECT":
                    return TokenKind.TK_SELECT;
                case "SUB":
                    return TokenKind.TK_SUB;
                case "TO":
                    return TokenKind.TK_TO;
                case "ITER":
                    return TokenKind.TK_ITER;
                case "LEAVE":
                    return TokenKind.TK_LEAVE;
                case "RETURN":
                    return TokenKind.TK_RETURN;
                default:
                    return TokenKind.TK_IDENTIFIER;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        internal static bool isExtededFector2Keyword(string keywrd)
        {
            keywrd = keywrd.Trim();

            switch (keywrd)
            {
                case "DOW":
                case "DOU":
                case "EVAL":
                case "EVAL(H)":
                case "FOR":
                case "CALL":
                case "CALLB":
                case "IF":
                    return true;
                default:
                    return false;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static TokenKind getBuiltInFunction(string symbol)
        {
            symbol = symbol.ToUpper();
            switch (symbol)
            {
                case "%ABS":
                case "%CHAR":
                case "%CHECK":
                case "%CHECKR":
                case "%DATE":
                case "%DAYS":
                case "%DEC":
                case "%DECH":
                case "%DIFF":
                case "%EDITC":
                case "%EDITW":
                case "%ELEM":
                case "%EOF":
                case "%EQUAL":
                case "%ERROR":
                case "%FIELDS":
                case "%FOUND":
                case "%HOURS":
                case "%INTH":
                case "%INT":
                case "%LOOKUP":
                case "%MINUTES":
                case "%MONTHS":
                case "%MSSECONDS":
                case "%OPEN":
                case "%PARMS":
                case "%REPLACE":
                case "%SCAN":
                case "%SECONDS":
                case "%SIZE":
                case "%STATUS":
                case "%SUBST":
                case "%TIMESTAMP":
                case "%TRIM":
                case "%TRIML":
                case "%TRIMR":
                case "%YEARS":
                case "%LOG":
                case "%LOG10":
                case "%REM":
                case "%RAND":
                case "%SQRT":
                case "%LEN":
                    return TokenKind.TK_IDENTIFIER;
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
            isStartsWith_IN = symbol.Contains("*IN");
            is5CharsLong = (symbol.Length == 5);

            // check if symbol is a built-in indicator
            if (isStartsWith_IN == true && is5CharsLong == true)
            {
                num = BIIndicators.Where(ind => $"*IN{ind}".Equals(symbol)).FirstOrDefault();

                if (num != null)
                    return TokenKind.TK_IDENTIFIER;
                else
                    return TokenKind.TK_BADTOKEN;
            }
            else
            {
                switch (symbol)
                {
                    case "*ON":
                        return TokenKind.TK_INDON;
                    case "*OFF":
                        return TokenKind.TK_INDOFF;
                    case "*N":
                        return TokenKind.TK_IDENTIFIER;
                    default:
                        return TokenKind.TK_BADTOKEN;
                }
            }
        }
        // //////////////////////////////////////////////////////////////////////////////////
        // FREE FORMAT OPCODES
        public static bool isValidFunction(string funcNam)
        {
            string symbol;

            symbol = funcNam;
            symbol = freeBIFWithNoParan.Where(kt => (kt == symbol)).FirstOrDefault();

            return (symbol != null);
        }


        // //////////////////////////////////////////////////////////////////////////////////
        // FREE FORMAT OPCODES
        public static bool isKeyword(string kw)
        {
            string ishere;

            // find the symbol in the list
            ishere = freeKeyWords.Where(kt => (kt == kw)).FirstOrDefault();

            // if found return true
            return (ishere != null);
        }

        // //////////////////////////////////////////////////////////////////////////////////
        internal static bool isNoParenthesisFunction(object sym)
        {
            string symbol;

            symbol = sym.ToString();
            symbol = freeBIFWithNoParan.Where(kt => (kt == symbol)).FirstOrDefault();

            return (symbol != null);
        }


        // //////////////////////////////////////////////////////////////////////////////////
        // STRUCTURED OPCODES
        public static bool isValidOpCode(string kw)
        {
            string ishere;

            // find the symbol in the list
            ishere = struckeyWords.Where(kt => (kt == kw)).FirstOrDefault();

            // if found return true
            return (ishere != null);
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static bool isValidVariable(string v)
        {
            // variable must start with a valid char
            // and not contain any bad characters
            if (badChars_ToStartVar.Contains(v[0]))
                return false;
            if (v.IndexOfAny(badChars_Var) >= 0)
                return false;

            return true;
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
                case "L0":
                case "L1":
                case "L2":
                case "L3":
                case "L4":
                case "L5":
                case "L6":
                case "L7":
                case "L8":
                case "L9":
                    kind = TokenKind.TK_IDENTIFIER;
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
                case "ADDLE":
                case "ADDLT":
                case "ADDGE":
                case "ADDGT":
                case "ADDEQ":
                case "ADDNE":
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
                case "ORLE":
                case "ORLT":
                case "ORGE":
                case "ORGT":
                case "OREQ":
                case "ORNE":
                    kind = TokenKind.TK_OR;
                    break;
                case "SUB":
                    kind = TokenKind.TK_SUB;
                    break;
                case "CIN":
                case "COUT":
                case "DSPLY":
                case "SUBST":
                    kind = TokenKind.TK_IDENTIFIER;
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
                case "02":
                case "03":
                case "04":
                case "05":
                case "06":
                case "07":
                case "08":
                case "09":
                case "10":
                case "11":
                case "12":
                case "13":
                case "14":
                case "15":
                case "16":
                case "17":
                case "18":
                case "19":
                case "20":
                case "21":
                case "22":
                case "23":
                case "24":
                case "25":
                case "26":
                case "27":
                case "28":
                case "29":
                case "30":
                case "31":
                case "32":
                case "33":
                case "34":
                case "35":
                case "36":
                case "37":
                case "38":
                case "39":
                case "40":
                case "41":
                case "42":
                case "43":
                case "44":
                case "45":
                case "46":
                case "47":
                case "48":
                case "49":
                case "50":
                case "51":
                case "52":
                case "53":
                case "54":
                case "55":
                case "56":
                case "57":
                case "58":
                case "59":
                case "60":
                case "61":
                case "62":
                case "63":
                case "64":
                case "65":
                case "66":
                case "67":
                case "68":
                case "69":
                case "70":
                case "71":
                case "72":
                case "73":
                case "74":
                case "75":
                case "76":
                case "77":
                case "78":
                case "79":
                case "80":
                case "81":
                case "82":
                case "83":
                case "84":
                case "85":
                case "86":
                case "87":
                case "88":
                case "89":
                case "90":
                case "91":
                case "92":
                case "93":
                case "94":
                case "95":
                case "96":
                case "97":
                case "98":
                case "99":
                case "LR":
                case "H1":
                case "H2":
                case "H3":
                case "H4":
                case "H5":
                case "H6":
                case "H7":
                case "H8":
                case "H9":
                case "KA":
                case "KB":
                case "KC":
                case "KD":
                case "KE":
                case "KF":
                case "KG":
                case "KH":
                case "KI":
                case "KJ":
                case "KK":
                case "KL":
                case "KM":
                case "KN":
                case "KO":
                case "KP":
                case "KQ":
                case "KR":
                case "KS":
                case "KT":
                case "KU":
                case "KV":
                case "KW":
                case "KX":
                case "L0":
                case "L1":
                case "L2":
                case "L3":
                case "L4":
                case "L5":
                case "L6":
                case "L7":
                case "L8":
                case "L9":
                case "M1":
                case "M2":
                case "M3":
                case "M4":
                case "M5":
                case "M6":
                case "M7":
                case "M8":
                case "M9":
                case "MR":
                case "OA":
                case "OG":
                case "OV":
                case "RT":
                    kind = TokenKind.TK_IDENTIFIER;
                    break;
                default:
                    kind = TokenKind.TK_BADTOKEN;
                    break;
            }

            return new SyntaxToken(kind, ind.linePos, ind.chrPos, ind.symbol);
        }
        // //////////////////////////////////////////////////////////////////////////////////
        public static TokenKind getRPGType(string vtype)
        {
            vtype = vtype.ToUpper();

            switch (vtype)
            {
                case "I":
                    return TokenKind.TK_INTEGER;
                case "F":
                    return TokenKind.TK_FLOAT;
                case "P":
                    return TokenKind.TK_PACKED;
                case "S":
                    return TokenKind.TK_ZONED;
                case "N":
                    return TokenKind.TK_INDICATOR;
                case "D":
                    return TokenKind.TK_DATE;
                case "T":
                    return TokenKind.TK_TIME;
                case "Z":
                    return TokenKind.TK_TIMESTAMP;
                case "A":
                case " ":
                    return TokenKind.TK_STRING;
                default:
                    return TokenKind.TK_BADTOKEN;
            }
        }


        // //////////////////////////////////////////////////////////////////////////////////
        public static TokenKind getRPGTypeFree(string vtype)
        {
            vtype = vtype.ToUpper();


            switch (vtype)
            {
                case "INT":
                case "FLOAT":
                case "PACKED":
                case "ZONED":
                case "IND":
                case "DATE":
                case "TIME":
                case "TIMESTAMP":
                case "CHAR":
                case "VARCHAR":
                    return TokenKind.TK_IDENTIFIER;
                default:
                    return TokenKind.TK_BADTOKEN;
            }
            /*
            switch (vtype)
            {
                case "INT":
                    return TokenKind.TK_INTEGER;
                case "FLOAT":
                    return TokenKind.TK_FLOAT;
                case "PACKED":
                    return TokenKind.TK_PACKED;
                case "ZONED":
                    return TokenKind.TK_ZONED;
                case "IND":
                    return TokenKind.TK_INDICATOR;
                case "DATE":
                    return TokenKind.TK_DATE;
                case "TIME":
                    return TokenKind.TK_TIME;
                case "TIMESTAMP":
                    return TokenKind.TK_TIMESTAMP;
                case "CHAR":
                case "VARCHAR":
                    return TokenKind.TK_STRING;
                default:
                    return TokenKind.TK_BADTOKEN;
            }
            */
        }

        public static Symbols.TypeSymbol lookupType(string typeName)
        {
            if (typeName == null)
                return Symbols.TypeSymbol.Void;

            switch (typeName)
            {
                case "IND":
                case "N":
                    return Symbols.TypeSymbol.Indicator;
                case "INT(10)":
                case "ZONED":
                case "PACKED":
                case "FLOAT":
                case "S":
                case "I":
                case "F":
                case "P":
                    return Symbols.TypeSymbol.Integer;
                case "DATE":
                case "D":
                    return Symbols.TypeSymbol.Date;
                case "TIMESTAMP":
                case "TIME":
                case "T":
                    return Symbols.TypeSymbol.DateTime;
                case "STRING":
                case "CHAR":
                case "VARCHAR":
                case "A":
                case " ":
                    return Symbols.TypeSymbol.Char;
                default:
                    return Symbols.TypeSymbol.Void;
            }
        }


        // //////////////////////////////////////////////////////////////////////////////////
        public static bool doColectAnotherCard(string symbol)
        {
            switch (symbol)
            {
                case "ANDEQ":
                case "ANDNE":
                case "ANDLT":
                case "ANDGT":
                case "ANDGE":
                case "ANDLE":
                case "OREQ":
                case "ORNE":
                case "ORLT":
                case "ORGT":
                case "ORGE":
                case "ORLE":
                case "IFEQ":
                case "IFNE":
                case "IFLT":
                case "IFGT":
                case "IFGE":
                case "IFLE":
                case "DOWEQ":
                case "DOWNE":
                case "DOWLT":
                case "DOWGT":
                case "DOWGE":
                case "DOWLE":
                case "DOUEQ":
                case "DOUNE":
                case "DOULT":
                case "DOUGT":
                case "DOUGE":
                case "DOULE":
                    return true;
                default:
                    return false;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static TokenKind getindicatorOperation(string Hi, string Lo, string Eq)
        {
            string cmp = "";

            Hi = Hi.Trim();
            Lo = Lo.Trim();
            Eq = Eq.Trim();

            cmp += (Hi != "") ? "1" : "0";
            cmp += (Lo != "") ? "1" : "0";
            cmp += (Eq != "") ? "1" : "0";

            switch (cmp)
            {
                case "001":
                    return TokenKind.TK_EQ;
                case "010":
                    return TokenKind.TK_LT;
                case "011":
                    return TokenKind.TK_LE;
                case "100":
                    return TokenKind.TK_GT;
                case "101":
                    return TokenKind.TK_GE;
                case "110":
                    return TokenKind.TK_NE;
                default:
                    return TokenKind.TK_BADTOKEN;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static string[] getAllIndicators()
        {
            return BIIndicators;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static string[] getSpecialOpCodes()
        {
            return freeBIFWithNoParan;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static string normalizeLine(string line)
        {
            char[] ret;
            bool onString = false;
            char ch;
            StringBuilder sbuild;

            sbuild = new StringBuilder();
            ret = line.ToCharArray();

            // if the line does not have a quote
            if (line.Contains("'") == false)
                return line.ToUpper();
            else
            {
                // the line has a quote
                foreach (char c  in ret)
                {
                    ch = c;

                    // quote was found keep case togle boolean
                    if (ch == 39)
                        onString = !onString;

                    // set to uppercase
                    if (onString == false)
                        ch = Char.ToUpper(ch);

                    sbuild.Append(ch);
                }
            }

            return sbuild.ToString();
        }
        // //////////////////////////////////////////////////////////////////////////////////
        public static string normalizeComments(string line)
        {
            char curSpec;
            string tmp;

            // remove comment from the string
            if (line.Length > 2)
            {
                tmp = line.Substring(1, 2);
                if (line[1] == '*')
                {
                    curSpec = ' ';
                    line = " ";
                }

                if (tmp.Contains("//") == true)
                {
                    tmp = line.Substring(0, line.IndexOf("//"));
                    line = tmp;
                }
            }

            return line;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        // compiler requests another card
        internal static bool goFish(string line)
        {
            string opCode;

            // nothing was given return false
            if (String.IsNullOrEmpty(line))
                return false;

            // extract op code from line
            if (line.Length > 10)
            {
                opCode = line.PadRight(72).ToUpper();
                opCode = opCode.Substring(19, 10).Trim();
            }
            else
                // only op code was given use line
                opCode = line.Trim();


            switch (opCode)
            {
                case "ANDEQ":
                case "ANDNE":
                case "ANDLT":
                case "ANDGT":
                case "ANDGE":
                case "ANDLE":
                case "OREQ":
                case "ORNE":
                case "ORLT":
                case "ORGT":
                case "ORGE":
                case "ORLE":
                case "IFEQ":
                case "IFNE":
                case "IFLT":
                case "IFGT":
                case "IFGE":
                case "IFLE":
                case "DOWEQ":
                case "DOWNE":
                case "DOWLT":
                case "DOWGT":
                case "DOWGE":
                case "DOWLE":
                case "DOUEQ":
                case "DOUNE":
                case "DOULT":
                case "DOUGT":
                case "DOUGE":
                case "DOULE":
                    return true;
                default:
                    return false;
            }
        }

        internal static bool cascadeBlockStart(string line)
        {
            string opCode;

            // nothing was given return false
            if (String.IsNullOrEmpty(line))
                return false;

            // extract op code from line
            if (line.Length > 10)
            {
                opCode = line.PadRight(72).ToUpper();
                opCode = opCode.Substring(19, 10).Trim();
            }
            else
                // only op code was given use line
                opCode = line.Trim();

            switch (opCode)
            {
                case "IFEQ":
                case "IFNE":
                case "IFLT":
                case "IFGT":
                case "IFGE":
                case "IFLE":
                case "DOWEQ":
                case "DOWNE":
                case "DOWLT":
                case "DOWGT":
                case "DOWGE":
                case "DOWLE":
                case "DOUEQ":
                case "DOUNE":
                case "DOULT":
                case "DOUGT":
                case "DOUGE":
                case "DOULE":
                    return true;
                default:
                    return false;
            }
        }
    }
}
