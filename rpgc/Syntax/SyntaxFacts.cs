using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
                                "Z-ADD","Z-SUB", "CASEQ", "CASNE", "CASLE", "CASLT", "CASGE", "CASGT", "CABEQ", "CABNE", "CABLE", 
                                "CABLT", "CABGE", "CABGT"};
        private static string[] freeKeyWords = {"ACQ","ALLOC","AND","ASSIGN","BEGSR","BY",
                                "CIN","CHAIN","CLEAR","INZ","SUBST","TEST",
                                "CLOSE","COMMIT", "COUT","DEALLOC","DEFINE","DELETE",
                                "DOU","DOW","DSPLY","DUMP","ELSE","END","ENDCS","ENDDO","ENDFOR",
                                "ENDIF","ENDMON","ENDSL","ENDSR","EXCEPT","EXFMT","EXSR","OUT",
                                "EXTRCT","FEOD","FOR","FORCE","IF","IN","ITER","LEAVE","LEAVESR",
                                "MONITOR","NOT","OCCUR","OPEN","OR","TO","OTHER",
                                "POST","PRINT","READ","READC","READE","READP","READPE","REALLOC",
                                "REL","RESET","RETURN","ROLBK","SCAN","SELECT","SETGT","SETLL",
                                "SHTDN","SORTA","UNIEXP","UNIOP","UNLOCK","UPDATE","WHEN","WRITE"};
        private static string[] freeDecalres = { "DCL-C", "DCL-S", "DCL-PROC", "DCL-PI", "DCL-DS",
                                "DCL-PR", "CTL-OPT","END-PROC", "END-PI", "END-PR", "END-DS","ON-ERROR"};
        private static string[] dBlocks = { "DCL-DS", "DCL-PR", "DCL-PI", "END-DS", "END-PR", "END-PI" };

        private static string[] freeBIFWithNoParan = { "DSPLY", "SORTA", "COUT", "EXSR", "READ", "READE", "SETLL", "SETGT", "OPEN", "CLOSE", "READC", "DELETE", "WRITE", "UPDATE", "CHAIN", "CLEAR", "SORTA", "SUBDUR" };
        private static string[] BIIndicators = { "01","02","03","04","05","06","07","08","09","10","11","12","13","14","15","16","17","18","19","20","21","22","23","24","25","26","27","28","29","30"
                                                ,"31","32","33","34","35","36","37","38","39","40","41","42","43","44","45","46","47","48","49","50","51","52","53","54","55","56","57","58","59","60"
                                                ,"61","62","63","64","65","66","67","68","69","70","71","72","73","74","75","76","77","78","79","80","81","82","83","84","85","86","87","88","89","90"
                                                ,"91","92","93","94","95","96","97","98","99","LR","H1","H2","H3","H4","H5","H6","H7","H8","H9","KA","KB","KC","KD","KE","KF","KG","KH","KI","KJ","KK"
                                                ,"KL","KM","KN","KO","KP","KQ","KR","KS","KT","KU","KV","KW","KX","L0","L1","L2","L3","L4","L5","L6","L7","L8","L9","M1","M2","M3","M4","M5","M6","M7"
                                                ,"M8","M9","MR","OA","OG","OV","RT"};
        private static string[] allFreeKeywords = null;
        private static Dictionary<string, TokenKind> opDic = new Dictionary<string, TokenKind>() { 
            {"EQ", TokenKind.TK_EQ}, 
            {"NE", TokenKind.TK_NE }, 
            {"LE", TokenKind.TK_LE},
            {"GE", TokenKind.TK_GE },
            {"LT", TokenKind.TK_LT },
            {"GT", TokenKind.TK_GT }
        };

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
                case "BEGSR":
                    return TokenKind.TK_PROCDCL;
                case "DCL-PI":
                    return TokenKind.TK_PROCINFC;
                case "DCL-S":
                    return TokenKind.TK_VARDECLR;
                case "DOU":
                    return TokenKind.TK_DOU;
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
                case "EXSR":
                    return TokenKind.TK_EXSR;
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
                case "OTHER":
                    return TokenKind.TK_OTHER;
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
                case "WHEN":
                    return TokenKind.TK_WHEN;
                default:
                    return TokenKind.TK_IDENTIFIER;
            }
        }
        // //////////////////////////////////////////////////////////////////////////////////
        public static TokenKind LegacyComparison(string symbol)
        {
            string operation;

            // dont do there is nothing in the string
            if (string.IsNullOrEmpty(symbol) == true)
                return TokenKind.TK_SPACE;
            if (symbol.Length < 2)
                return TokenKind.TK_SPACE;

            // get the operation code
            operation = symbol.Substring(symbol.Length - 2);

            // get token
            switch (operation)
            {
                case "EQ":
                    return TokenKind.TK_EQ;
                case "NE":
                    return TokenKind.TK_NE;

                case "LT":
                    return TokenKind.TK_LT;
                case "GT":
                    return TokenKind.TK_GT;

                case "GE":
                    return TokenKind.TK_GE;
                case "LE":
                    return TokenKind.TK_LE;
                default:
                    return TokenKind.TK_BADTOKEN;
            }

            return TokenKind.TK_SPACE;
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
                case "RETURN":
                case "WHEN":
                    return true;
                default:
                    return false;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        internal static bool isStandaloneOpCode(string keywrd)
        {
            keywrd = keywrd.Trim();

            switch (keywrd)
            {
                case "DUMP":
                case "ITER":
                case "LEAVE":
                case "LEAVESR":
                case "ELSE":
                case "END":
                case "ENDCS":
                case "ENDDO":
                case "ENDFOR":
                case "ENDIF":
                case "ENDMON":
                case "ENDSL":
                case "ENDSR":
                case "SETOFF":
                case "SETON":
                case "MONITOR":
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
                case "%XLATE":
                case "%SHIFTL":
                case "%SHIFTR":
                case "%BITAND":
                case "%BITOR":
                case "%BITNOT":
                case "%BITANDNOT":
                case "%BITXOR":
                case "%FLOAT":
                case "%SCANRPL":
                case "%CONCAT":
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
                        return getCompilerConstans(symbol);
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
        public static TokenKind getFreeFormatKind(string kw, bool onFree = false)
        {
            string kwl;

            // find only free format C specs (for fix format syntax)
            // otherwise find all free format specs (for full free)
            if (isKeyword(kw) == true && onFree == false)
                return getKeywordKind(kw);
            else
            {
                // all free keywords have not been initalized yet
                // create a new list by adding D format keywords
                // and C format keywords
                if (allFreeKeywords == null)
                {
                    allFreeKeywords = new string[freeKeyWords.Length];
                    freeKeyWords.CopyTo(allFreeKeywords, 0);
                    allFreeKeywords = allFreeKeywords.Concat(freeDecalres).ToArray();
                }

                // with the new list find the key word
                kwl = (from itm in allFreeKeywords
                       where itm == kw
                       select itm).FirstOrDefault();

                // keyword not found retun identifier token
                if (kwl == null)
                    return TokenKind.TK_IDENTIFIER;

                return getKeywordKind(kwl);
            }
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
        public static string getCompilerConstansLiteral(string constKw)
        {
            switch (constKw)
            {
                case "*BLANK":
                case "*BLANKS":
                    return " ";
                case "*ZERO":
                case "*ZEROS":
                    return "0";
                default:
                    return null;
            }
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
                case "INT":
                case "ZONED":
                case "PACKED":
                case "FLOAT":
                case "S":
                case "I":
                case "P":
                    return Symbols.TypeSymbol.Integer;
                case "F":
                    return Symbols.TypeSymbol.Float;
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
                case "FLOAT(8)":
                    return Symbols.TypeSymbol.Float;
                case "void":
                default:
                    return Symbols.TypeSymbol.Void;
            }
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static string lookupTypeName(string typeName)
        {
            if (typeName == null)
                return "void";

            switch (typeName)
            {
                case "IND":
                case "N":
                    return "bool";
                case "INT(10)":
                case "ZONED":
                case "PACKED":
                case "FLOAT":
                case "S":
                case "I":
                case "F":
                case "P":
                case "Int32":
                    return "Int(10)";
                case "DATE":
                case "D":
                    return "Date";
                case "TIMESTAMP":
                case "TIME":
                case "T":
                    return "Date";
                case "STRING":
                case "CHAR":
                case "VARCHAR":
                case "A":
                case " ":
                    return "String";
                case "FLOAT(8)":
                    return "Float";
                case "void":
                default:
                    return "void";
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
        public static string normalizeLine(string line, bool doIBMLine = false)
        {
            char[] ret;
            bool onString = false;
            char ch;
            StringBuilder sbuild;


            // cut out comments in traditinal IBM RPG
            if (doIBMLine == true)
            {
                if (line.Length > 4)
                    line = line.Substring(5);
            }

            // if the line does not have a quote
            if (line.Contains("'") == false)
                return line.ToUpper();
            else
            {
                sbuild = new StringBuilder();
                ret = line.ToCharArray();

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
        public static bool isInDBlock(string spec)
        {
            string res;

            res = (from el in dBlocks
                   where el == spec
                   select el).FirstOrDefault();

            if (res == null)
                return false;

            return true;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public static string normalizeComments(string line)
        {
            string tmp;

            // remove comment from the string if any
            if (line.Length > 2)
            {
                tmp = line.Substring(1, 3);

                // RPG clasic comments
                if (line[1] == '*')
                    line = " ";

                // C style comments
                if (tmp.Contains("//") == true)
                    line = " ";
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

        // ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static List<SyntaxToken> prepareMainFunction(SyntaxTree sTree_, TokenKind lineTerminator, bool isBeginingMain)
        {
            List<SyntaxToken> ret;

            ret = new List<SyntaxToken>();

            if (isBeginingMain == true)
            {
                ret.Add(new SyntaxToken(sTree_, TokenKind.TK_PROCDCL, 0, 0, "B", 0));
                ret.Add(new SyntaxToken(sTree_, TokenKind.TK_IDENTIFIER, 0, 0, "MAIN", 0));
            }
            else
            {
                ret.Add(new SyntaxToken(sTree_, TokenKind.TK_ENDPROC, 0, 0, "E", 0));
            }

            ret.Add(new SyntaxToken(sTree_, lineTerminator, 0, 0, "", 0));

            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool isIBMDoc(string[] lines)
        {
            int col1prpb, col6prob;
            bool onCol1, onCol6;
            Regex regCol1, regCol6,
                  regRampageFree, regIBMFree;

            regCol1 = new Regex(@"^([hfdicop])([^*]{2})([ n])([kml0-9][0-9a-z]|\s\s)", RegexOptions.IgnoreCase);
            regCol6 = new Regex(@"^(.....)([hfdicop])([^*]{2})([ n])([kml0-9][0-9a-z]|\s\s)", RegexOptions.IgnoreCase);
            regRampageFree = new Regex(@"^(?i)./free");
            regIBMFree = new Regex(@"^(?i)....../free");
            col1prpb = 0;
            col6prob = 0;

            foreach (string lin in lines)
            {
                // check for free tags
                // this is an instant affirmation
                //    IBM free can only exist on column 7
                //    Rampage free can only exist on column 2
                if (regRampageFree.Match(lin).Success)
                    return false;
                if (regIBMFree.Match(lin).Success)
                    return true;

                // check line for spec position
                onCol1 = regCol1.Match(lin).Success;
                onCol6 = regCol6.Match(lin).Success;

                // found a instruction line match
                if (onCol1 == true || onCol6 == true)
                {
                    // add 1 only if the code is a rampage line
                    if (onCol1 == true)
                    {
                        col1prpb += 1;
                        col6prob -= 1;
                    }
                    else
                    {
                        // add 1 only if the code is a IBM line
                        col1prpb -= 1;
                        col6prob += 1;
                    }
                }
            }

            // check which column has the greatest chance of beeing the spec column
            if (col1prpb > col6prob)
                return false;
            else
                return true;
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////
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

        internal static rpgc.Symbols.TypeSymbol iToD(rpgc.Binding.BoundExpression L, rpgc.Binding.BoundExpression R)
        {
            bool isFloat_R, isFloat_L;

            switch (R.Type.Name)
            {
                case "FLOAT(8)":
                    isFloat_R = true;
                    break;
                default:
                    isFloat_R = false;
                    break;
            }

            switch (L.Type.Name)
            {
                case "INT(10)":
                    isFloat_L = true;
                    break;
                default:
                    isFloat_L = false;
                    break;

            }

            if (isFloat_R == true || isFloat_L == true)
                return rpgc.Symbols.TypeSymbol.Float;

            return rpgc.Symbols.TypeSymbol.Integer;
        }
    }
}
