using System;
using System.Collections;
using System.Collections.Generic;
using rpgc.Syntax;
using rpgc.Symbols;
using System.Collections.Immutable;

namespace rpgc
{
    public sealed class DiagnosticBag : IEnumerable<Diagnostics>
    {
        private List<Diagnostics> _diagnostic = new List<Diagnostics>();
        private static Dictionary<TokenKind, string> enumDict = new Dictionary<TokenKind, string>()
            {{TokenKind.TK_ACQ,"ACQ"},
            {TokenKind.TK_ADD,"Addition"},
            {TokenKind.TK_ADDDUR,"AddDur"},
            {TokenKind.TK_ALLOC,"Alloc"},
            {TokenKind.TK_AND,"And"},
            {TokenKind.TK_ASSIGN,"Assign"},
            {TokenKind.TK_BADTOKEN,""},
            {TokenKind.TK_BEGSR,"BegSr"},
            {TokenKind.TK_BITOFF,"BitOff"},
            {TokenKind.TK_BITON,"BitOn"},
            {TokenKind.TK_BLOCKEND,"End"},
            {TokenKind.TK_BLOCKSTART,""},
            {TokenKind.TK_BLOCKSYNTX,""},
            {TokenKind.TK_BY,"By"},
            {TokenKind.TK_BYNARYEXPR,"Bynary_Expression"},
            {TokenKind.TK_CAB,"Cab"},
            {TokenKind.TK_CALL,"Call"},
            {TokenKind.TK_CALLB,"CallB"},
            {TokenKind.TK_CALLP,"CallP"},
            {TokenKind.TK_CAS,"Cas"},
            {TokenKind.TK_CAT,"Cat"},
            {TokenKind.TK_CHAIN,"Chain"},
            {TokenKind.TK_CHECK,"Check"},
            {TokenKind.TK_CHECKR,"CheckR"},
            {TokenKind.TK_CLEAR,"Clear"},
            {TokenKind.TK_CLOSE,"Close"},
            {TokenKind.TK_COLON,"Colon"},
            {TokenKind.TK_COMMIT,"Commit"},
            {TokenKind.TK_COMP,"Comp"},
            {TokenKind.TK_COMPLATIONUNT,""},
            {TokenKind.TK_DATE,"Date"},
            {TokenKind.TK_DATETIME,"DatetTime"},
            {TokenKind.TK_DEALLOC,"Dealloc"},
            {TokenKind.TK_DEFINE,"Define"},
            {TokenKind.TK_DELETE,"Delete"},
            {TokenKind.TK_DIV,"Division"},
            {TokenKind.TK_DO,"Do"},
            {TokenKind.TK_DOEND,"DoEnd"},
            {TokenKind.TK_DOU,"DoU"},
            {TokenKind.TK_DOW,"DoW"},
            {TokenKind.TK_DOWNTO,"DownTo"},
            {TokenKind.TK_DSPLY,"Dsply"},
            {TokenKind.TK_DUMP,"Dump"},
            {TokenKind.TK_ELSE,"Else"},
            {TokenKind.TK_ENDCS,"EndCs"},
            {TokenKind.TK_ENDDO,"EndDo"},
            {TokenKind.TK_ENDIF,"EndIf"},
            {TokenKind.TK_ENDFOR,"EndFor"},
            {TokenKind.TK_ENDMON,"EndMon"},
            {TokenKind.TK_ENDPROC,"EndProc"},
            {TokenKind.TK_ENDPR, "End-Pr"},
            {TokenKind.TK_ENDSR,"EndSr"},
            {TokenKind.TK_ENDSL,"EndSl"},
            {TokenKind.TK_EOI,"EOI"},
            {TokenKind.TK_EQ,"Equal_To"},
            {TokenKind.TK_EVAL,"Eval"},
            {TokenKind.TK_EVALR,"EvalR"},
            {TokenKind.TK_EXCEPT,"Except"},
            {TokenKind.TK_EXFMT,"ExFmt"},
            {TokenKind.TK_EXPONENT,"Exponent"},
            {TokenKind.TK_EXPRNSTMNT,"Expression_Statement"},
            {TokenKind.TK_EXSR,"ExSr"},
            {TokenKind.TK_EXTRCT,"Extrct"},
            {TokenKind.TK_FEOD,"Feod"},
            {TokenKind.TK_FLOAT,"Float"},
            {TokenKind.TK_FOR,"For"},
            {TokenKind.TK_FORCE,"Force"},
            {TokenKind.TK_GE,"Greater_Than_Or_Equal_To"},
            {TokenKind.TK_GOTO,"GoTo"},
            {TokenKind.TK_GRAPHIC,"Graphic"},
            {TokenKind.TK_GT,"Greater_Than"},
            {TokenKind.TK_IDENTIFIER,"Identifier"},
            {TokenKind.TK_IF,"If"},
            {TokenKind.TK_IFEND,"EndIf"},
            {TokenKind.TK_IN,"In"},
            {TokenKind.TK_INDOFF,"*Off"},
            {TokenKind.TK_INDON,"*On"},
            {TokenKind.TK_INTEGER,"Integer"},
            {TokenKind.TK_INZ,"Inz"},
            {TokenKind.TK_ITER,"Iter"},
            {TokenKind.TK_KFLD,"Kfld"},
            {TokenKind.TK_KLIST,"Klist"},
            {TokenKind.TK_LEAVE,"Leave"},
            {TokenKind.TK_LEAVESR,"LeaveSr"},
            {TokenKind.TK_LOOKUP,"Lookup"},
            {TokenKind.TK_LE,"Less_Than_Or_Equal_To"},
            {TokenKind.TK_LITEXPR,"Literal_Expression"},
            {TokenKind.TK_LT,"Less_Than"},
            {TokenKind.TK_MHHZO,"MHHZO"},
            {TokenKind.TK_MHLZO,"MHLZO"},
            {TokenKind.TK_MLHZO,"MLHZO"},
            {TokenKind.TK_MLLZO,"MLLZO"},
            {TokenKind.TK_MONITOR,"Monitor"},
            {TokenKind.TK_MOVE,"Move"},
            {TokenKind.TK_MOVEA,"MoveA"},
            {TokenKind.TK_MOVEL,"MoveL"},
            {TokenKind.TK_MULT,"Mult"},
            {TokenKind.TK_MVR,"MVR"},
            {TokenKind.TK_NAMEDEXP,"Named_Expression"},
            {TokenKind.TK_NE,"Not_Equal"},
            {TokenKind.TK_NEXT,"Next"},
            {TokenKind.TK_NONE,"None"},
            {TokenKind.TK_NOT,"Not"},
            {TokenKind.TK_OCCUR,"Occur"},
            {TokenKind.TK_OPEN,"Open"},
            {TokenKind.TK_OR,"Or"},
            {TokenKind.TK_OTHER,"Other"},
            {TokenKind.TK_OUT,"Out"},
            {TokenKind.TK_PACKED,"Packed"},
            {TokenKind.TK_PARENEXP,"Parenthesis_Expression"},
            {TokenKind.TK_PARENCLOSE,")"},
            {TokenKind.TK_PARENOPEN,"("},
            {TokenKind.TK_PARM,"Parm"},
            {TokenKind.TK_PLIST,"Plist"},
            {TokenKind.TK_POST,"Post"},
            {TokenKind.TK_PROCDCL,"Dcl-Proc"},
            {TokenKind.TK_PROCEND,"End-Proc"},
            {TokenKind.TK_READ,"Read"},
            {TokenKind.TK_READC,"ReadC"},
            {TokenKind.TK_READE,"ReadE"},
            {TokenKind.TK_READP,"ReadP"},
            {TokenKind.TK_READPE,"ReadPE"},
            {TokenKind.TK_REALLOC,"Realloc"},
            {TokenKind.TK_REL,"Rel"},
            {TokenKind.TK_RESET,"Reset"},
            {TokenKind.TK_RETURN,"Return"},
            {TokenKind.TK_ROLBK,"RolBk"},
            {TokenKind.TK_SCAN,"Scan"},
            {TokenKind.TK_SELECT,"Select"},
            {TokenKind.TK_SEMI,";"},
            {TokenKind.TK_SETGT,"SetGT"},
            {TokenKind.TK_SETLL,"SetLL"},
            {TokenKind.TK_SETOFF,"SetOff"},
            {TokenKind.TK_SETON,"SetOn"},
            {TokenKind.TK_SHTDN,"ShtDn"},
            {TokenKind.TK_SORTA,"SortA"},
            {TokenKind.TK_SPACE,"Space"},
            {TokenKind.TK_SQRT,"Sqrt"},
            {TokenKind.TK_STRING,"String"},
            {TokenKind.TK_SUB,"Sub"},
            {TokenKind.TK_SUBDUR,"SubDur"},
            {TokenKind.TK_SUBST,"SubSt"},
            {TokenKind.TK_SUBRTNDCL,"Subroutine_Dcl"},
            {TokenKind.TK_SUBRTNEND,"Subroutine_End"},
            {TokenKind.TK_TAG,"Tag"},
            {TokenKind.TK_TEST,"Test"},
            {TokenKind.TK_TESTB,"TestB"},
            {TokenKind.TK_TESTN,"TestN"},
            {TokenKind.TK_TESTZ,"TestZ"},
            {TokenKind.TK_TIME,"Time"},
            {TokenKind.TK_TO,"To"},
            {TokenKind.TK_UNIOP,"UniOp"},
            {TokenKind.TK_UNIEXP,"Unary_Expression"},
            {TokenKind.TK_UNLOCK,"UnLock"},
            {TokenKind.TK_UPDATE,"Update"},
            {TokenKind.TK_VARDCONST,"Constant_Variable"},
            {TokenKind.TK_VARDDATAS,"Data_Structure"},
            {TokenKind.TK_VARDECLR,"Variable_Declaration"},
            {TokenKind.TK_WHEN,"When"},
            {TokenKind.TK_WRITE,"Write"},
            {TokenKind.TK_XFOOT,"XFoot"},
            {TokenKind.TK_XLATE,"XLate"},
            {TokenKind.TK_ZADD,"Z-Add"},
            {TokenKind.TK_ZONED,"Zoned"},
            {TokenKind.TK_ZSUB,"Z-Sub"},
            {TokenKind.TK_INDICATOR,"Indicator"},
            {TokenKind.TK_TIMESTAMP,"TimeStamp"},
            {TokenKind.TK_TYPCLAUSE,"Type_Clause"},
            {TokenKind.TK_GLBSTMNT,"Global_Statement"},
            {TokenKind.TK_PRCKKEYWRD,"Procedure_Keyword"},
            {TokenKind.TK_PROCINFC,"Procedure_Interface"},
            {TokenKind.TK_ENDPI,"End-Pi"},
            {TokenKind.TK_NEWLINE,"NewLine"}};

        public void report(TextSpan span, string message)
        {
            Diagnostics diag = new Diagnostics(span, message);
            _diagnostic.Add(diag);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void report(string message, int start, int len)
        {
            Diagnostics diag = new Diagnostics(new TextSpan(start, len), message);
            _diagnostic.Add(diag);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportInvalidNumber(string symbol, TypeSymbol typVal, int start, int len)
        {
            string message;

            message = string.Format("rpgc:({2}): error: the symbol {0} is not a valid {1}", symbol, typVal, "{0},{1}");

            report(message, start, len);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadCharacter(char symbol, int position)
        {
            string message;

            message = string.Format("rpgc:({1}): error: bad character imput ({0})", symbol, "{0},{1}");

            report(message, (position - 1), 1);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUnexpectedToken(TextSpan span, TokenKind actual, TokenKind expected)
        {
            string message;

            message = string.Format("rpgc:({2},{3}): error: unexpected token ‘{0}’ expected ‘{1}’", enumHMI(actual), enumHMI(expected), span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUndefinedUniaryOp(TextSpan span, string opSym, TypeSymbol opType)
        {
            string message;

            message = string.Format("rpgc:({2},{3}): error: uninary operator [{0}] is not defined for type {1}", opSym, opType, span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUndefinedBynaryOp(TextSpan span, string opSym, TypeSymbol LeftType, TypeSymbol RightType)
        {
            string message;

            message = string.Format("rpgc:({3},{4}): error: binnary operator [{0}] is not defined for types {1} and {2}", opSym, LeftType, RightType, span.LineNo, span.LinePos);

            report(span, message);
        }

        internal void reportInvalidReturn(TextSpan span)
        {
            reportUnexpectedSymbol(span, "return");
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportUnexpectedSymbol(TextSpan span, string symbol)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: Unexpected symbol ‘{2}’ in member declaration", span.LineNo, span.LinePos, symbol);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportFunctionReturnsNULL(TextSpan span, TypeSymbol type)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: return-statement with no value, in function returning ‘{2}’", span.LineNo, span.LinePos, type);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportVoidFunctionReturnsValue(TextSpan span)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: return-statement with a value, in function returning void", span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportRedeclareParamiter(TextSpan span, string name, TypeSymbol typ)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: declaration of ‘{3} {2}’ shadows a parameter", span.LineNo, span.LinePos, typ, name);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUndefinedName(TextSpan span, string name)
        {
            string message;

            message = string.Format("rpgc:({1},{2}): error: the variable [{0}] is not defined", name, span.LineNo, span.LinePos);

            report(span, message);
        }
        // //////////////////////////////////////////////////////////////////////////
        public void reportMissingFactor1(TextSpan span, int ln)
        {
            string message;

            message = string.Format("rpgc:({0},6): error: missing argument in factor 1", span.LineNo);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportNotAllCodePathsReturn(TextSpan span, string procName)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: in procedure ‘{2}’: not all code paths return a value", span.LineNo, span.LinePos, procName);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportNotLeftJustified(TextSpan span, int factorCode, int lp)
        {
            string message;
            Dictionary<int, string> factors = new Dictionary<int, string> { { 1, "Factor 1" }, { 2, "Factor 2" }, { 3, "Result" }, { 4, "Op-Code" } };

            message = string.Format("rpgc:({1},{2}): error: {0} is not left justified", factors[factorCode], span.LineNo, lp);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportNotRightJustified(TextSpan span, int factor, int lp)
        {
            string message;

            message = string.Format("rpgc:({1},{2}): error: factor {0} is not right justifide", factor, lp, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportVariableAlreadyDeclared(TextSpan span, string name)
        {
            string message;

            message = string.Format("rpgc:({1},{2}): error: redeclaration of ‘{0}’", name, span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportVariableDoesNotExist(TextSpan span, string name)
        {
            string message;

            message = string.Format("rpgc:({1},{2}): error: ‘{0}’ was not declared in this scope", name, span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadFactor(TextSpan span, int factor, int lp)
        {
            string message;

            message = string.Format("rpgc:({1},{2}): error: factor {0} is not empty", factor, lp, span.START);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadSpec(char symbol, int linePosition)
        {
            string message;

            message = string.Format("rpgc:({1},1): error: unknown specification ‘{0}’", symbol, linePosition);

            report(new TextSpan(0, 1), message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportWrongSpecLoc(char symbol, char expected, int linePosition)
        {
            string message;

            message = string.Format("rpgc:({0},1): error: ‘{1}’ specification found in wrong position expected ‘{2}’", linePosition, symbol, expected);

            report(new TextSpan(0, 1), message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadOpcode(string symbol, int linePosition, int CharPos)
        {
            string message;

            message = string.Format("rpgc:({1},21): error: unknown Operation Code [{0}]", symbol, linePosition);

            report(new TextSpan(CharPos, symbol.Length), message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportExpressionMustHaveValue(TextSpan span)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: value can not be null", span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportAssignmentOfConstantVar(TextSpan span, string name)
        {
            string message;

            message = string.Format("rpgc:({1},{2}): error: assignment of read-only variable ‘{0}’", name, span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportWrongArgumentCount(TextSpan span, string FunctionName, bool isGreaterThan, int argCnt)
        {
            string message, qual, argumentCount;

            if (isGreaterThan == true)
                qual = "many";
            else
                qual = "few";

            if (argCnt == 0)
                argumentCount = "none";
            else
                argumentCount = argCnt.ToString();

            message = string.Format("rpgc:({0},{1}): error: too {3} arguments to function ‘{2}’ expected {4}", 
                                    span.LineNo, span.LinePos, FunctionName, qual, argumentCount);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportAssignmentWithotResult(TextSpan span)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: Assignment or Move without result", span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportSubroutineRecursion(TextSpan span)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: Subroutines cannot used for recursions", span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportDuplicateParamiterName(TextSpan span, string name, string type)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: redefinition of paramiter ‘{2} {3}’", span.LineNo, span.LinePos, name, type);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportCannotConvert(TextSpan span, TypeSymbol givenType, TypeSymbol expectedResult)
        {
            string message;

            message = string.Format("rpgc:({2},{3}): error: cannot convert from ‘{0}’ to ‘{1}’", givenType.Name, expectedResult.Name, span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportFunctionAlreadyDeclared(TextSpan span, string name)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: redefinition of ‘{2}’", span.LineNo, span.LinePos, name);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportCannotConvertType(TextSpan span, TypeSymbol GivenType, TypeSymbol ExpectedType)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: invalid conversion from ‘{2}’ to ‘{3}’", span.LineNo, span.LinePos, GivenType, ExpectedType);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportFunctionParamiterTypeMismatch(TextSpan span, TypeSymbol type1, TypeSymbol type2)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: invalid conversion from ‘{2}’ to ‘{3}’", span.LineNo, span.LinePos, type1, type2);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportBadString(TextSpan span)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: missing terminating ' character in string constant", span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportLoopWithoutCondition(TextSpan span, string LoopSym)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: missing conditional statement for {2}", span.LineNo, span.LinePos, LoopSym);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportSubroutineParamiters(TextSpan span)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): Subroutines do not accept paramiters", span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportIfWithoutCondition(TextSpan span)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: missing conditional expression for [If]", span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportVariableWithNoName(TextSpan span)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): declaration does not declare anything", span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportBadFunctionOrProcedure(TextSpan span, string name)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: ‘{2}’ was not declared in this scope", span.LineNo, span.LinePos, name);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportMissingTag(TextSpan span, string tagName)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: No such lable: ‘{2}’ within the scope of the goto statement", span.LineNo, span.LinePos, tagName);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportTypeNotGiven(TextSpan span)
        {
            string message;
            
            message = string.Format("rpgc:({0},{1}): error: Variable declaration without a type", span.LineNo, span.LinePos);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportUndefinedType(TextSpan span, string sym)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: The type ‘{2}’ is not recognized as an RPG type", span.LineNo, span.LinePos, sym);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportOpCodeNotAlone(int linePos, int chrPos, string symbol)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: The Op-Code ‘{2}’ takes no factor or gives a result", linePos, chrPos, symbol);

            report(new TextSpan(0, symbol.Length, linePos, chrPos), message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportLeaveOrIterWithoutLoop(TextSpan span, string v)
        {
            string message, sym;

            if (v == "break")
                sym = "Leave";
            else
                sym = "Iter";

            message = string.Format("rpgc:({0},{1}): error: {2} statement not within loop", span.LineNo, span.LinePos, sym);

            report(span, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportBadProcedure(int lineNo, int charPos)
        {
            string message;

            message = string.Format("rpgc:({0},{1}): error: syntaxError, invalid syntax", lineNo, charPos);

            report(new TextSpan(0, 0, lineNo, charPos), message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public IEnumerator<Diagnostics> GetEnumerator()
        {
            return _diagnostic.GetEnumerator();
        }

        // //////////////////////////////////////////////////////////////////////////
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // //////////////////////////////////////////////////////////////////////////
        public void AddRange(DiagnosticBag a)
        {
            if (a != null)
                _diagnostic.AddRange(a._diagnostic);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void Clear()
        {
            _diagnostic.Clear();
        }

        // //////////////////////////////////////////////////////////////////////////
        public static string enumToSym(TokenKind tok)
        {
            return enumDict[tok];
        }

        // //////////////////////////////////////////////////////////////////////////
        private string enumHMI(TokenKind kind)
        {
            return enumDict[kind];
        }
    }
}
