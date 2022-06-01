using System;
using System.Collections;
using System.Collections.Generic;
using rpgc.Syntax;
using rpgc.Symbols;
using rpgc.Text;
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
            {TokenKind.TK_BADTOKEN,">Bad_Token<"},
            {TokenKind.TK_BEGSR,"BegSr"},
            {TokenKind.TK_BITOFF,"BitOff"},
            {TokenKind.TK_BITON,"BitOn"},
            {TokenKind.TK_BLOCKEND,"End"},
            {TokenKind.TK_BLOCKSTART,">Block_Start<"},
            {TokenKind.TK_BLOCKSYNTX,">Block_Syntax<"},
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
            {TokenKind.TK_COMPLATIONUNT,">Complation_Unit<"},
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


        public void report(TextLocation txtLoc, string message)
        {
            Diagnostics diag = new Diagnostics(txtLoc, message);
            _diagnostic.Add(diag);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void report(TextLocation txtLoc, string message, int start, int len)
        {
            Diagnostics diag = new Diagnostics(txtLoc, new TextSpan(start, len), message);
            _diagnostic.Add(diag);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportInvalidNumber(TextLocation txtLoc, string symbol, TypeSymbol typVal, int start, int len)
        {
            string message;

            message = string.Format("({2}): error: the symbol {0} is not a valid {1}", symbol, typVal, "{0},{1}");

            report(txtLoc, message, start, len);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadCharacter(TextLocation txtLoc, char symbol, int position)
        {
            string message;

            message = string.Format("({1}): error: bad character imput ({0})", symbol, "{0},{1}");

            report(txtLoc, message, (position - 1), 1);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUnexpectedToken(TextLocation txtLoc, TokenKind actual, TokenKind expected)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({2},{3}): error: unexpected token ‘{0}’ expected ‘{1}’", enumHMI(actual), enumHMI(expected), span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportInvalidExpressionStatement(TextLocation textLocation)
        {
            string message;
            TextSpan span = textLocation.SPAN;

            message = string.Format("({0},{1}): error: only assignment, call, increment, decrement, await, and new object expressions can be used as a statement", span.LineNo, span.LinePos);

            report(textLocation, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUndefinedUniaryOp(TextLocation txtLoc, string opSym, TypeSymbol opType)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({2},{3}): error: uninary operator [{0}] is not defined for type {1}", opSym, opType, span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUndefinedBynaryOp(TextLocation txtLoc, string opSym, TypeSymbol LeftType, TypeSymbol RightType)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({3},{4}): error: binnary operator [{0}] is not defined for types {1} and {2}", opSym, LeftType, RightType, span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportInvalidReturn(TextLocation txtLoc)
        {
            reportUnexpectedSymbol(txtLoc, "return");
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportUnexpectedSymbol(TextLocation txtLoc, string symbol)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: Unexpected symbol ‘{2}’ in member declaration", span.LineNo, span.LinePos, symbol);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportFunctionReturnsNULL(TextLocation txtLoc, TypeSymbol type)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: return-statement with no value, in function returning ‘{2}’", span.LineNo, span.LinePos, type);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportVoidFunctionReturnsValue(TextLocation txtLoc)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: return-statement with a value, in function returning void", span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportRedeclareParamiter(TextLocation txtLoc, string name, TypeSymbol typ)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: declaration of ‘{3} {2}’ shadows a parameter", span.LineNo, span.LinePos, typ, name);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportUndefinedName(TextLocation txtLoc, string name)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({1},{2}): error: the variable [{0}] is not defined", name, span.LineNo, span.LinePos);

            report(txtLoc, message);
        }
        // //////////////////////////////////////////////////////////////////////////
        public void reportMissingFactor1(TextLocation txtLoc, int ln)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},6): error: missing argument in factor 1", span.LineNo);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportNotAllCodePathsReturn(TextLocation txtLoc, string procName)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: in procedure ‘{2}’: not all code paths return a value", span.LineNo, span.LinePos, procName);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportNotLeftJustified(TextLocation txtLoc, int factorCode, int lp)
        {
            string message;
            TextSpan span = txtLoc.SPAN;
            Dictionary<int, string> factors = new Dictionary<int, string> { { 1, "Factor 1" }, { 2, "Factor 2" }, { 3, "Result" }, { 4, "Op-Code" } };

            message = string.Format("({1},{2}): error: {0} is not left justified", factors[factorCode], span.LineNo, lp);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportNotRightJustified(TextLocation txtLoc, int factor, int lp)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({1},{2}): error: factor {0} is not right justifide", factor, lp, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportVariableAlreadyDeclared(TextLocation txtLoc, string name)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({1},{2}): error: redeclaration of ‘{0}’", name, span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportVariableDoesNotExist(TextLocation txtLoc, string name)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({1},{2}): error: ‘{0}’ was not declared in this scope", name, span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadFactor(TextLocation txtLoc, int factor, int lp)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({1},{2}): error: factor {0} is not empty", factor, lp, span.START);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadSpec(TextLocation txtLoc, char symbol, int linePosition)
        {
            string message;

            message = string.Format("({1},1): error: unknown specification ‘{0}’", symbol, linePosition);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportWrongSpecLoc(TextLocation txtLoc, char symbol, char expected, int linePosition)
        {
            string message;

            message = string.Format("({0},1): error: ‘{1}’ specification found in wrong position expected ‘{2}’", linePosition, symbol, expected);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportWrongSpecLoc(TextLocation txtLoc, string symbol, string expected, int linePosition, int symStart)
        {
            string message;

            if (expected == "C")
                message = string.Format("({0},{1}): error: ‘{2}’ specification found in wrong position expected KEYWORD or COMMAND", linePosition, symStart, symbol);
            else
                message = string.Format("({0},{1}): error: ‘{2}’ specification found in wrong position expected ‘{3}’", linePosition, symStart, symbol, expected);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        public void reportBadOpcode(TextLocation txtLoc, string symbol, int linePosition, int CharPos)
        {
            string message;

            message = string.Format("({1},21): error: unknown Operation Code [{0}]", symbol, linePosition);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportExpressionMustHaveValue(TextLocation txtLoc)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: expression results in void", span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportAssignmentOfConstantVar(TextLocation txtLoc, string name)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({1},{2}): error: assignment of read-only variable ‘{0}’", name, span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportWrongArgumentCount(TextLocation txtLoc, string FunctionName, bool isGreaterThan, int argCnt)
        {
            string message, qual, argumentCount;
            TextSpan span = txtLoc.SPAN;

            if (isGreaterThan == true)
                qual = "many";
            else
                qual = "few";

            if (argCnt == 0)
                argumentCount = "none";
            else
                argumentCount = argCnt.ToString();

            message = string.Format("({0},{1}): error: too {3} arguments to function ‘{2}’ expected {4}", 
                                    span.LineNo, span.LinePos, FunctionName, qual, argumentCount);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportAssignmentWithotResult(TextLocation txtLoc)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: Assignment or Move without result", span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportSubroutineRecursion(TextLocation txtLoc)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: Subroutines cannot used for recursions", span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportProcedureCalledWithExsr(TextLocation txtLoc, string name)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: the Procedure ‘{2}’ cannot be called using EXSR", span.LineNo, span.LinePos, name);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportDuplicateParamiterName(TextLocation txtLoc, string name, string type)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: redefinition of paramiter ‘{2} {3}’", span.LineNo, span.LinePos, name, type);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportSubroutineCalledAsProcedure(TextLocation txtLoc, string name)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: the Subroutine ‘{2}’ must be called using EXSR", span.LineNo, span.LinePos, name);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportCannotConvert(TextLocation txtLoc, TypeSymbol givenType, TypeSymbol expectedResult)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({2},{3}): error: cannot convert from ‘{0}’ to ‘{1}’", givenType.Name, expectedResult.Name, span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportFunctionAlreadyDeclared(TextLocation txtLoc, string name)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: redefinition of ‘{2}’", span.LineNo, span.LinePos, name);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportCannotConvertType(TextLocation txtLoc, TypeSymbol GivenType, TypeSymbol ExpectedType)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: invalid conversion from ‘{2}’ to ‘{3}’", span.LineNo, span.LinePos, GivenType, ExpectedType);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportFunctionParamiterTypeMismatch(TextLocation txtLoc, TypeSymbol type1, TypeSymbol type2)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: invalid conversion from ‘{2}’ to ‘{3}’", span.LineNo, span.LinePos, type1, type2);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportBadString(TextLocation txtLoc)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: missing terminating ' character in string constant", span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportLoopWithoutCondition(TextLocation txtLoc, string LoopSym)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: missing conditional statement for {2}", span.LineNo, span.LinePos, LoopSym);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportSubroutineParamiters(TextLocation txtLoc)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): Subroutines do not accept paramiters", span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportIfWithoutCondition(TextLocation txtLoc)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: missing conditional expression for [If]", span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportVariableWithNoName(TextLocation txtLoc)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): declaration does not declare anything", span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportProcedureSouldNotReturnVoid(TextLocation txtLoc, string val)
        {
            string message, typename;
            TextSpan span = txtLoc.SPAN;

            typename = SyntaxFacts.lookupTypeName(val);

            message = string.Format("({0},{1}): Procedure returns {2} but statement returns void", span.LineNo, span.LinePos, typename);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportBadFunctionOrProcedure(TextLocation txtLoc, string name)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: ‘{2}’ was not declared in this scope", span.LineNo, span.LinePos, name);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportMissingTag(TextLocation txtLoc, string tagName)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: No such lable: ‘{2}’ within the scope of the goto statement", span.LineNo, span.LinePos, tagName);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportTypeNotGiven(TextLocation txtLoc)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: Variable declaration without a type", span.LineNo, span.LinePos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportUndefinedType(TextLocation txtLoc, string sym)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: The type ‘{2}’ is not recognized as an RPG type", span.LineNo, span.LinePos, sym);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportOpCodeNotAlone(TextLocation txtLoc, int linePos, int chrPos, string symbol)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: The Op-Code ‘{2}’ takes no factor or gives a result", linePos, chrPos, symbol);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportLeaveOrIterWithoutLoop(TextLocation txtLoc, string v)
        {
            string message, sym;
            TextSpan span = txtLoc.SPAN;

            if (v == "break")
                sym = "Leave";
            else
                sym = "Iter";

            message = string.Format("({0},{1}): error: {2} statement not within loop", span.LineNo, span.LinePos, sym);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportBadProcedure(TextLocation txtLoc, int lineNo, int charPos)
        {
            string message;
            TextSpan span = txtLoc.SPAN;

            message = string.Format("({0},{1}): error: syntaxError, invalid syntax", lineNo, charPos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportSemiColonInFixedFormat(TextLocation txtLoc, int lineNo, int charPos)
        {
            string message;

            message = string.Format("({0},{1}):error: stray ‘;’ in program", lineNo, charPos);

            report(txtLoc, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportVariableAssignedToVoid(TextLocation textLocation, TypeSymbol type, string variableName)
        {
            string message;
            TextSpan span = textLocation.SPAN;

            message = string.Format("({0},{1}): error: variable [{2}] assigned void expected {3}", span.LineNo, span.LinePos, variableName, type.Name);

            report(textLocation, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportProcedureNameMismatch(TextLocation location, string name)
        {
            string message;
            TextSpan span = location.SPAN;

            message = string.Format("({0},{1}): error: procedure name mismatch expected *n or {2}", span.LineNo, span.LinePos, name);

            report(location, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportReturnTypeMismatch(TextLocation textLocation, string returnKW, string expectedReturn)
        {
            string message;
            TextSpan span = textLocation.SPAN;

            if (expectedReturn == "")
                message = string.Format("({0},{1}): error: procedure returns void but key word is returning {1}", span.LineNo, span.LinePos, expectedReturn);
            else
                message = string.Format("({0},{1}): error: procedure returns {3} but key word is returning {2}", span.LineNo, span.LinePos, returnKW, expectedReturn);

            report(textLocation, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void repotCannotMixMainWithGlobalInstruction(TextLocation textLocation)
        {
            string message;
            TextSpan span = textLocation.SPAN;

            message = string.Format("({0},{1}): error: cannot declare main function in no main file", span.LineNo, span.LinePos);

            report(textLocation, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportMainReturningAValue(TextLocation textLocation)
        {
            string message;
            TextSpan span = textLocation.SPAN;

            message = string.Format("({0},{1}): error: Main cannot return a value", span.LineNo, span.LinePos);

            report(textLocation, message);
        }

        // //////////////////////////////////////////////////////////////////////////
        internal void reportMultipleMainFunctions(TextLocation textLocation)
        {
            string message;
            TextSpan span = textLocation.SPAN;

            message = string.Format("({0},{1}): error: Program has more than one entry point defined", span.LineNo, span.LinePos);

            report(textLocation, message);
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
            if (enumDict.ContainsKey(kind) == true)
                return enumDict[kind];

            return "Bad_Key";
        }
    }
}
