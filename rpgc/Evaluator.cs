using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Binding;
using rpgc.Symbols;

namespace rpgc
{
    internal sealed class Evaluator
    {
        private readonly BoundProgram program;
        Dictionary<VariableSymbol, object> _Globals;
        Dictionary<FunctionSymbol, BoundBlockStatement> _functions = new Dictionary<FunctionSymbol, BoundBlockStatement>();
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new Stack<Dictionary<VariableSymbol, object>>();
        private object lastValue;
        private readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> FunctionBodies;

        //private readonly BoundBlockStatement ROOT;
        //private readonly BoundGlobalScope globals;
        //private Random randnum;


        public Evaluator(BoundProgram pgm, Dictionary<VariableSymbol, object> _variables)
        {
            BoundProgram c;

            program = pgm;
            _Globals = _variables;
            _locals.Push(new Dictionary<VariableSymbol, object>());
            //globals = pgm.GblScope;

            // step through all funcitons and 
            c = program;
            while (c != null)
            {
                foreach (var fn in c.Functions)
                {
                    _functions.Add(fn.Key, fn.Value);
                }
                c = c.Previous;
            }
        }

        // //////////////////////////////////////////////////////////////
        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> _variables)
        {
            program = null;
            _Globals = _variables;
            //ROOT = root;
        }

        // //////////////////////////////////////////////////////////////
        public Evaluator(ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functionBodies, BoundBlockStatement st, Dictionary<VariableSymbol, object> variables)
        {
            FunctionBodies = _functionBodies;
            _Globals = variables;
            //ROOT = st;
        }

        // //////////////////////////////////////////////////////////////
        private bool DoComparison(string Operation, object LT0, object TR0, object L, object R)
        {
            int intL, intR;
            string strL, strR;
            double douL, douR;
            DateTime datL, datR;


            if (LT0.GetType() == typeof(int))
            {
                intL = Convert.ToInt32(L);
                intR = Convert.ToInt32(R);

                switch (Operation)
                {
                    case ">":
                        return intL > intR;
                    case ">=":
                        return intL >= intR;
                    case "<":
                        return intL < intR;
                    default:
                        return intL <= intR;
                }
            }
            else
            {
                if (LT0.GetType() == typeof(double) || LT0.GetType() == typeof(float))
                {
                    douL = Convert.ToDouble(L);
                    douR = Convert.ToDouble(R);

                    switch (Operation)
                    {
                        case ">":
                            return douL > douR;
                        case ">=":
                            return douL >= douR;
                        case "<":
                            return douL < douR;
                        default:
                            return douL <= douR;
                    }
                }
                else
                {
                    if (LT0.GetType() == typeof(string))
                    {
                        strL = L.ToString();
                        strR = R.ToString();

                        switch (Operation)
                        {
                            case ">":
                                return String.Compare(strL, strR) > 0;
                            case ">=":
                                return String.Compare(strL, strR) >= 0;
                            case "<":
                                return String.Compare(strL, strR) < 0;
                            default:
                                return String.Compare(strL, strR) <= 0;
                        }
                    }
                    else
                    {
                        datL = Convert.ToDateTime(L);
                        datR = Convert.ToDateTime(R);

                        switch (Operation)
                        {
                            case ">":
                                return datL > datR;
                            case ">=":
                                return datL >= datR;
                            case "<":
                                return datL < datR;
                            default:
                                return datL <= datR;
                        }
                    }
                }
            }
        }

        // //////////////////////////////////////////////////////////////
        private object evaluateExpression(BoundExpression root)
        {
            switch (root.tok)
            {
                case BoundNodeToken.BNT_ASNEX:
                    return evaluateAssignmentExpression((BoundAssignmentExpression)root);
                case BoundNodeToken.BNT_LITEX:
                    return evaluateBoundLiteral((BoundLiteralExp)root);
                case BoundNodeToken.BNT_UINEX:
                    return evaluateBoundUniaryExpression((BoundUniExpression)root);
                case BoundNodeToken.BNT_BINEX:
                    return evaluateBoundBinaryExpression((BoundBinExpression)root);
                case BoundNodeToken.BNT_VAREX:
                    return evaluateVariableExpression((BoundVariableExpression)root);
                case BoundNodeToken.BNT_CALLEXP:
                    return evaluateCallExpression((BoundCallExpression)root);
                case BoundNodeToken.BNT_CONVEXP:
                    return evaluateConversionExpression((BoundConversionExpression)root);
                case BoundNodeToken.BNT_ERROREXP:
                    return new BoundErrorExpression();
                default:
                    throw new Exception(string.Format("unexpected token {0}", root.tok));
            }
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateConversionExpression(BoundConversionExpression node)
        {
            object value;
            string typeName;

            value = evaluateExpression(node._Expression);
            typeName = node.Type.Name;

            switch(typeName)
            {
                case "ANY":
                    return value;
                case "IND":
                    return Convert.ToBoolean(value);
                case "INT(10)":
                    return Convert.ToInt32(value);
                case "FLOAT(8)":
                    return Convert.ToDouble(value);
                case "CHAR":
                    return value.ToString();
                case "DATETIME":
                case "DATE":
                    return Convert.ToDateTime(value.ToString());
                case "TIME":
                    return Convert.ToDateTime($"1900-01-01 {value}");
                default:
                    throw new Exception($"Unexpected type {node.Type}");
            }
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateBoundUniaryExpression(BoundUniExpression uiTmp)
        {
            object value;
            int operand;

            var chameleonOBJ = reEvaluate(uiTmp.right);

            // convert to neg or positive
            switch (uiTmp.OP.tok)
            {
                case BoundUniOpToken.BUO_IDENTITY:
                    operand = Convert.ToInt32(chameleonOBJ);
                    value = operand;
                    break;
                case BoundUniOpToken.BUO_NEGATION:
                    operand = Convert.ToInt32(chameleonOBJ);
                    value = (-1 * operand);
                    break;
                case BoundUniOpToken.BUO_NOT:
                    value = !Convert.ToBoolean(chameleonOBJ);
                    break;
                default:
                    throw new Exception(string.Format("unrecognized uinary Operator [{0}] ", uiTmp.tok));
            }

            return value;
        }

        // //////////////////////////////////////////////////////////////////////////
        object reEvaluate(object exp)
        {
            object output;

            output = exp;

            if (output is BoundExpression)
            {
                output = evaluateExpression((BoundExpression)output);
                return reEvaluate(output);
            }

            return output;
        }

        // //////////////////////////////////////////////////////////////
        private Object arithmaticEvaluater(Object A, Object B, string TypeName, BoundBinOpToken Operation)
        {
            int i_AA, i_BB;
            double d_AA, d_BB;
            bool onFloat;

            onFloat = false;
            i_AA = 0;
            i_BB = 0;
            d_AA = 0;
            d_BB = 0;

            switch (TypeName)
            {
                case "VARCHAR":
                case "CHAR":
                    return A.ToString() + B.ToString();
                case "INT(10)":
                    i_AA = Convert.ToInt32(A);
                    i_BB = Convert.ToInt32(B);
                    onFloat = false;
                    break;
                case "FLOAT(8)":
                    d_AA = Convert.ToDouble(A);
                    d_BB = Convert.ToDouble(B);
                    onFloat = true;
                    break;
            }


            if (onFloat == false)
            {
                switch (Operation)
                {
                    case BoundBinOpToken.BBO_ADD:
                        return (i_AA + i_BB);
                    case BoundBinOpToken.BBO_SUB:
                        return (i_AA - i_BB);
                    case BoundBinOpToken.BBO_MULT:
                        return (i_AA * i_BB);
                    case BoundBinOpToken.BBO_DIV:
                        return (i_AA / i_BB);
                }
            }
            else
            {
                switch (Operation)
                {
                    case BoundBinOpToken.BBO_ADD:
                        return (d_AA + d_BB);
                    case BoundBinOpToken.BBO_SUB:
                        return (d_AA - d_BB);
                    case BoundBinOpToken.BBO_MULT:
                        return (d_AA * d_BB);
                    case BoundBinOpToken.BBO_DIV:
                        return (d_AA / d_BB);
                }
            }

            return null;
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateBoundBinaryExpression(BoundBinExpression biTmp)
        {
            object chameleonOBJL;
            object chameleonOBJR;
            object ans = null;

            chameleonOBJL = reEvaluate(biTmp.Left);
            chameleonOBJR = reEvaluate(biTmp.Right);

            switch (biTmp.OP.tok)
            {
                case BoundBinOpToken.BBO_ADD:
                    ans = arithmaticEvaluater(chameleonOBJL, chameleonOBJR, biTmp.OP.ResultType.Name, biTmp.OP.tok);
                    break;
                case BoundBinOpToken.BBO_SUB:
                    ans = arithmaticEvaluater(chameleonOBJL, chameleonOBJR, biTmp.OP.ResultType.Name, biTmp.OP.tok);
                    break;
                case BoundBinOpToken.BBO_MULT:
                    ans = arithmaticEvaluater(chameleonOBJL, chameleonOBJR, biTmp.OP.ResultType.Name, biTmp.OP.tok);
                    break;
                case BoundBinOpToken.BBO_DIV:
                    ans = arithmaticEvaluater(chameleonOBJL, chameleonOBJR, biTmp.OP.ResultType.Name, biTmp.OP.tok);
                    break;

                case BoundBinOpToken.BBO_AND:
                    ans = Convert.ToBoolean(chameleonOBJL) && Convert.ToBoolean(chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_OR:
                    ans = Convert.ToBoolean(chameleonOBJL) || Convert.ToBoolean(chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_EQ:
                    ans = Equals(chameleonOBJL, chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_NE:
                    ans = !Equals(chameleonOBJL, chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_GE:
                    ans = DoComparison(">=", chameleonOBJL, chameleonOBJR, chameleonOBJL, chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_GT:
                    ans = DoComparison(">", chameleonOBJL, chameleonOBJR, chameleonOBJL, chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_LE:
                    ans = DoComparison("<=", chameleonOBJL, chameleonOBJR, chameleonOBJL, chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_LT:
                    ans = DoComparison("<", chameleonOBJL, chameleonOBJR, chameleonOBJL, chameleonOBJR);
                    break;
                default:
                    throw new Exception(string.Format("unexpected Binary operator {0}", biTmp.OP.tok));
            }

            return ans;
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateCallExpression(BoundCallExpression node)
        {
            BoundBlockStatement stmnt = null;

            switch (node.Function.Name)
            {
                case "CIN":
                    return Console.ReadLine();
                case "COUT":
                case "DSPLY":
                    Console.WriteLine(reEvaluate(node.Arguments[0]));
                    return null;
                case "%INT":
                    return BuiltinFunctions_Definitions.BIF_INT(reEvaluate(node.Arguments[0]));
                case "%CHAR":
                    return BuiltinFunctions_Definitions.BIF_CHAR(reEvaluate((node.Arguments[0])));
                case "%LOG":
                    return (int)BuiltinFunctions_Definitions.BIF_LOG(reEvaluate(node.Arguments[0]));
                case "%LOG10":
                    return (int)BuiltinFunctions_Definitions.BIF_LOG10(reEvaluate(node.Arguments[0]));
                case "%ABS":
                    return (int)BuiltinFunctions_Definitions.BIF_ABS(reEvaluate(node.Arguments[0]));
                case "%SQRT":
                    return BuiltinFunctions_Definitions.BIF_SQRT(reEvaluate(node.Arguments[0]));
                case "%REM":
                    return BuiltinFunctions_Definitions.BIF_REM(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%LEN":
                    return BuiltinFunctions_Definitions.BIF_LEN(reEvaluate(node.Arguments[0]));
                case "%LOWER":
                    return BuiltinFunctions_Definitions.BIF_LOWER(reEvaluate(node.Arguments[0]));
                case "%UPPER":
                    return BuiltinFunctions_Definitions.BIF_UPPER(reEvaluate(node.Arguments[0]));
                case "%SUBST":
                    return BuiltinFunctions_Definitions.BIF_SUBST(
                        reEvaluate(node.Arguments[0]).ToString(),
                        reEvaluate(node.Arguments[1]).ToString(),
                        reEvaluate(node.Arguments[2]).ToString());
                case "%RAND":
                    return BuiltinFunctions_Definitions.BIF_RAND(reEvaluate((node.Arguments[0])));
                case "%DIV":
                    return BuiltinFunctions_Definitions.BIF_DIV(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%SCAN":
                    return BuiltinFunctions_Definitions.BIF_SCAN(
                        reEvaluate(node.Arguments[0]), 
                        reEvaluate(node.Arguments[1]));
                case "%REPLACE":
                    return BuiltinFunctions_Definitions.BIF_REPLACE(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]),
                        reEvaluate(node.Arguments[2]),
                        reEvaluate(node.Arguments[3]));
                case "%TRIM":
                    return BuiltinFunctions_Definitions.BIF_TRIM(reEvaluate((node.Arguments[0])));
                case "%XLATE":
                    return BuiltinFunctions_Definitions.BIF_XLATE(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]),
                        reEvaluate(node.Arguments[2]));
                case "%CHECK":
                    return BuiltinFunctions_Definitions.BIF_CHECK(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%CHECKR":
                    return BuiltinFunctions_Definitions.BIF_CHECKR(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%EDITW":
                    return BuiltinFunctions_Definitions.BIF_EDITW(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%SHIFTL":
                    return BuiltinFunctions_Definitions.BIF_SHIFTL(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%SHIFTR":
                    return BuiltinFunctions_Definitions.BIF_SHIFTR(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%BITAND":
                    return BuiltinFunctions_Definitions.BIF_BITAND(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%BITOR":
                    return BuiltinFunctions_Definitions.BIF_BITOR(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%BITXOR":
                    return BuiltinFunctions_Definitions.BIF_BITXOR(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%BITNOT":
                    return BuiltinFunctions_Definitions.BIF_BITNOT(reEvaluate(node.Arguments[0]));
                case "%BITANDNOT":
                    return BuiltinFunctions_Definitions.BIF_BITANDNOT(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                case "%TRIMR":
                    return BuiltinFunctions_Definitions.BIF_TRIMR(reEvaluate(node.Arguments[0]));
                case "%TRIML":
                    return BuiltinFunctions_Definitions.BIF_TRIML(reEvaluate(node.Arguments[0]));
                case "%FLOAT":
                    return BuiltinFunctions_Definitions.BIF_FLOAT(reEvaluate(node.Arguments[0]));
                case "%SCANRPL":
                    return BuiltinFunctions_Definitions.BIF_SCANRPL(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]),
                        reEvaluate(node.Arguments[2]));
                case "%CONCAT":
                    return BuiltinFunctions_Definitions.BIF_CONCAT(
                        reEvaluate(node.Arguments[0]),
                        reEvaluate(node.Arguments[1]));
                default:
                    // handle programmer defigned procedures/subrutines
                    ParamiterSymbol paramiter;
                    object vValue, result;
                    Dictionary<VariableSymbol, object> lcals;

                    lcals = new Dictionary<VariableSymbol, object>();

                    for (int i = 0; i < node.Arguments.Length; i++)
                    {
                        paramiter = node.Function.Paramiter[i];
                        vValue = reEvaluate(node.Arguments[i]);
                        lcals.Add(paramiter, vValue);
                    }
                    _locals.Push(lcals);

                    stmnt = _functions[node.Function];
                    result = EvaluateStatment(stmnt);

                    _locals.Pop();
                    return result;
            }
        }

        // //////////////////////////////////////////////////////////////////////////
        void performAssignment(VariableSymbol vriabl, object value)
        {
            Dictionary<VariableSymbol, object> lcl;

            // assign value to apropreate dictionary
            switch (vriabl.kind)
            {
                case SymbolKind.SYM_GLOBALVAR:
                    _Globals[vriabl] = value;
                    break;
                default:
                    lcl = _locals.Peek();
                    lcl[vriabl] = value;
                    break;
            }
        }

        // //////////////////////////////////////////////////////////////
        private void evaluateVariableDaclaration(boundVariableDeclaration node)
        {
            object value;

            value = reEvaluate(node.Initalizer);

            lastValue = value;

            performAssignment(node.Variable, value);
        }

        // //////////////////////////////////////////////////////////////
        private void evaluateExpressionStatement(BoundExpressionStatement stmnt)
        {
            lastValue = reEvaluate(stmnt.Expression);
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateVariableExpression(BoundVariableExpression vtmp)
        {
            object varb;
            Dictionary<VariableSymbol, object> lcl;
            VariableSymbol idx;

            idx = vtmp.Variable;
            lcl = _locals.Peek();

            // when searching for variables
            // check local stack first before checking global stack
            if (lcl.ContainsKey(idx) == true)
            {
                varb = lcl[idx];
            }
            else
            {
                varb = _Globals[idx];
            }

            return varb;
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateAssignmentExpression(BoundAssignmentExpression atmp)
        {
            object value;

            // get value
            value = reEvaluate(atmp.Expression);

            // assign value to apropreate dictionary
            performAssignment(atmp.Variable, value);

            return value;
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateBoundLiteral(BoundLiteralExp bLit)
        {
            return bLit.Value;
        }

        // //////////////////////////////////////////////////////////////
        private object evaluateReturnExpression(BoundReturnStatement s)
        {
            object returnValue;

            if (s == null)
                return null;

            if (s.Expression == null)
                returnValue = null;
            else
                returnValue = reEvaluate(s.Expression);

            return returnValue;
        }

        // //////////////////////////////////////////////////////////////
        public object EvaluateStatment(BoundBlockStatement statment)
        {
            Dictionary<string, int> lableToIndex;
            BoundLabelStatement lbl;
            BoundStatement smt;
            BoundGoToConditionalStatement cgts;
            BoundGoToStatement gs;

            int index;
            bool cond;
            string lblName;

            index = 0;
            lableToIndex = new Dictionary<string, int>();

            // collect all labels in the main function
            for (int i = 0; i < statment.Statements.Length; i++)
            {
                if (statment.Statements[i] is BoundLabelStatement)
                {
                    lbl = (BoundLabelStatement)statment.Statements[i];
                    lblName = lbl.Label.Name;
                    lableToIndex.Add(lblName, i + 1);
                }
            }

            // collect all statments in the main function
            while (index < statment.Statements.Length)
            {
                smt = statment.Statements[index];

                switch (smt.tok)
                {
                    case BoundNodeToken.BNT_EXPRSTMT:
                        evaluateExpressionStatement((BoundExpressionStatement)smt);
                        break;
                    case BoundNodeToken.BNT_VARDECLR:
                        evaluateVariableDaclaration((boundVariableDeclaration)smt);
                        break;
                    case BoundNodeToken.BNT_GOTOCOND:
                        cgts = (BoundGoToConditionalStatement)smt;
                        cond = (bool)reEvaluate(cgts.Condition);
                        if ((cond == true && cgts.JumpIfFalse == false) || (cond == false && cgts.JumpIfFalse == true))
                        {
                            lblName = cgts.Label.Name;
                            index = lableToIndex[lblName];
                            continue;
                        }
                        break;
                    case BoundNodeToken.BNT_GOTO:
                        gs = (BoundGoToStatement)smt;
                        lblName = gs.Label.Name;
                        index = lableToIndex[lblName];
                        continue;
                    case BoundNodeToken.BNT_RETSTMT:
                        lastValue =  evaluateReturnExpression((BoundReturnStatement)smt);
                        return lastValue;
                    case BoundNodeToken.BNT_LABEL:
                        break;
                    default:
                        smt.writeTo(Console.Out);
                        throw new Exception(string.Format("unexpected token {0}", smt.tok));
                }
                index++;
            }

            return lastValue;
        }

        // //////////////////////////////////////////////////////////////
        public object Evaluate()
        {
            FunctionSymbol fn;
            BoundBlockStatement body;

            // get program
            if (program.MainFunction != null)
                fn = program.MainFunction;
            else
                fn = program.ScriptFunction;

            // return nothing if thre is no program
            if (fn == null)
                return null;

            // get main body
            body = _functions[fn];

            // evaly program
            return EvaluateStatment(body);
        }
    }
}
