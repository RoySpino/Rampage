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
        private readonly BoundBlockStatement ROOT;
        Dictionary<VariableSymbol, object> _Globals;
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new Stack<Dictionary<VariableSymbol, object>>();
        private object lastValue;
        private Random randnum;
        private readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> FunctionBodies;

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> _variables)
        {
            ROOT = root;
            _Globals = _variables;
        }

        public Evaluator(ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functionBodies, BoundBlockStatement st, Dictionary<VariableSymbol, object> variables)
        {
            FunctionBodies = _functionBodies;
            ROOT = st;
            _Globals = variables;
        }

        // //////////////////////////////////////////////////////////////
        private bool DoComparison(string Operation, Type TL, Type TR, object L, object R)
        {
            int intL, intR;
            string strL, strR;
            double douL, douR;
            DateTime datL, datR;


            if (TL == typeof(int))
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
                if (TL == typeof(double))
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
                    if (TL == typeof(string))
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
            var value = evaluateExpression(node._Expression);

            if (node.Type == TypeSymbol.Indicator)
                return Convert.ToBoolean(value);
            if (node.Type == TypeSymbol.Integer)
                return Convert.ToInt32(value);
            if (node.Type == TypeSymbol.Char)
                return value.ToString();
            if (node.Type == TypeSymbol.Float)
                return Convert.ToDouble(value);
            if (node.Type == TypeSymbol.Date || node.Type == TypeSymbol.DateTime)
                return Convert.ToDateTime(value.ToString());
            if (node.Type == TypeSymbol.Time)
                return Convert.ToDateTime($"1900-01-01 {value}");

            throw new Exception($"Unexpected type {node.Type}");
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateBoundUniaryExpression(BoundUniExpression uiTmp)
        {
            object value;
            int operand;

            var chameleonOBJ = evaluateExpression(uiTmp.right);

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
        private object evaluateBoundBinaryExpression(BoundBinExpression biTmp)
        {
            object chameleonOBJL;
            object chameleonOBJR;
            object ans;

            chameleonOBJL = evaluateExpression(biTmp.Left);
            chameleonOBJR = evaluateExpression(biTmp.Right);

            switch (biTmp.OP.tok)
            {
                case BoundBinOpToken.BBO_ADD:
                    if (biTmp.Type == Symbols.TypeSymbol.Char || biTmp.Type == Symbols.TypeSymbol.varchar)
                        ans = chameleonOBJL.ToString() + chameleonOBJR.ToString();
                    else
                        ans = Convert.ToInt32(chameleonOBJL) + Convert.ToInt32(chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_SUB:
                    ans = Convert.ToInt32(chameleonOBJL) - Convert.ToInt32(chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_MULT:
                    ans = Convert.ToInt32(chameleonOBJL) * Convert.ToInt32(chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_DIV:
                    ans = Convert.ToInt32(chameleonOBJL) / Convert.ToInt32(chameleonOBJR);
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
                    ans = DoComparison(">=", chameleonOBJL.GetType(), chameleonOBJR.GetType(), chameleonOBJL, chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_GT:
                    ans = DoComparison(">", chameleonOBJL.GetType(), chameleonOBJR.GetType(), chameleonOBJL, chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_LE:
                    ans = DoComparison("<=", chameleonOBJL.GetType(), chameleonOBJR.GetType(), chameleonOBJL, chameleonOBJR);
                    break;
                case BoundBinOpToken.BBO_LT:
                    ans = DoComparison("<", chameleonOBJL.GetType(), chameleonOBJR.GetType(), chameleonOBJL, chameleonOBJR);
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

            if (node.Function == rpgc.Symbols.BuiltinFunctions.cin)
                return Console.ReadLine();
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.cout)
            {
                string msg = evaluateExpression((node.Arguments[0])).ToString();
                Console.WriteLine(msg);
                return null;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.dsply)
            {
                string msg = evaluateExpression((node.Arguments[0])).ToString();
                Console.WriteLine(msg);
                return null;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Int)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                try
                {
                    return Convert.ToInt32(Val);
                }
                catch (Exception)
                {
                    throw new Exception($"Input string ‘{Val}’ was not in a correct format");
                }
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_char)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                return Val;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Log)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                return (int)Math.Log(Convert.ToDouble(Val));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Log10)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                return (int)(Math.Log(Convert.ToDouble(Val)) / Math.Log(10.0));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_abs)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                return (int)Math.Abs(Convert.ToDouble(Val));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Sqrt)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                return (int)Math.Sqrt(Convert.ToDouble(Val));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Rem)
            {
                string Val0 = evaluateExpression((node.Arguments[0])).ToString();
                string Val1 = evaluateExpression((node.Arguments[1])).ToString();
                return (int)((Convert.ToDouble(Val0) % Convert.ToDouble(Val1)));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Len)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                return Val.Length;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Lower)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                return Val.ToLower();
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Upper)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                return Val.ToUpper();
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Subst)
            {
                string Val = evaluateExpression((node.Arguments[0])).ToString();
                return Val.ToUpper();
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Subst)
            {
                string Val0 = evaluateExpression((node.Arguments[0])).ToString();
                string Val1 = evaluateExpression((node.Arguments[1])).ToString();
                string Val2 = evaluateExpression((node.Arguments[2])).ToString();
                string result;
                int sidx, len;

                sidx = Convert.ToInt32(Val1);
                len = Convert.ToInt32(Val2);
                result = Val0.Substring(sidx, len);

                return result;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Rand)
            {
                int max;
                string Val0 = evaluateExpression((node.Arguments[0])).ToString();

                if (randnum == null)
                    randnum = new Random();

                max = Convert.ToInt32(Val0);
                return randnum.Next(max);
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_div)
            {
                int Val0 = Convert.ToInt32(evaluateExpression((node.Arguments[0])));
                int Val1 = Convert.ToInt32(evaluateExpression((node.Arguments[1])));

                return (int)(Val0 / Val1);
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Scan)
            {
                string whatToFind = evaluateExpression((node.Arguments[0])).ToString();
                string source = evaluateExpression((node.Arguments[1])).ToString();
                int idx;

                idx = source.IndexOf(whatToFind) + 1;

                return idx;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Replace)
            {
                string wrdValue = evaluateExpression((node.Arguments[0])).ToString();
                string source = evaluateExpression((node.Arguments[1])).ToString();
                int startIndex = Convert.ToInt32(evaluateExpression((node.Arguments[2])));
                int length = Convert.ToInt32(evaluateExpression((node.Arguments[3])));
                string tmp;

                tmp = wrdValue.Substring(0, length);

                if ((startIndex + length) >= source.Length)
                    return (source.Substring(0, startIndex-1) + tmp);

                tmp = source.Substring(0, startIndex - 1) + tmp + source.Substring((startIndex + length - 1));
                return tmp;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Trim)
            {
                string value = evaluateExpression((node.Arguments[0])).ToString();

                value = value.Trim();
                return value;
            }
            else
            {
                // handle programmer defigned procedures/subrutines
                var lcals = new Dictionary<VariableSymbol, object>();
                for (int i = 0; i < node.Arguments.Length; i++)
                {
                    var paramiter = node.Function.Paramiter[i];
                    var vValue = evaluateExpression(node.Arguments[i]);
                    lcals.Add(paramiter, vValue);
                }
                _locals.Push(lcals);

                stmnt = FunctionBodies[node.Function];
                var result = EvaluateStatment(stmnt);

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
            Dictionary<VariableSymbol, object> tmpLocalStack;

            value = evaluateExpression(node.Initalizer);
            //_Globals[node.Variable] = value;
            lastValue = value;

            performAssignment(node.Variable, value);
            /*
            if (node.Variable.kind == SymbolKind.SYM_GLOBALVAR)
            {
                _Globals[node.Variable] = value;
            }
            else
            {
                tmpLocalStack = _locals.Peek();
                tmpLocalStack[node.Variable] = value;
            }
            */
        }

        // //////////////////////////////////////////////////////////////
        private void evaluateExpressionStatement(BoundExpressionStatement stmnt)
        {
            lastValue = evaluateExpression(stmnt.Expression);
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateVariableExpression(BoundVariableExpression vtmp)
        {
            object varb;
            Dictionary<VariableSymbol, object> lcl;
            VariableSymbol idx;

            // get value from apropreate dictionary
            switch (vtmp.Variable.kind)
            {
                case SymbolKind.SYM_GLOBALVAR:
                    idx = vtmp.Variable;
                    varb = _Globals[idx];
                    break;
                default:
                    idx = vtmp.Variable;
                    lcl = _locals.Peek();
                    varb = lcl[idx];
                    break;
            }

            return varb;
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateAssignmentExpression(BoundAssignmentExpression atmp)
        {
            object value;
            Dictionary<VariableSymbol, object> lcl;

            // get value
            value = evaluateExpression(atmp.Expression);

            // assign value to apropreate dictionary
            performAssignment(atmp.Variable, value);

            /* assign value to apropreate dictionary
            switch (atmp.Variable.kind)
            {
                case SymbolKind.SYM_GLOBALVAR:
                    _Globals[atmp.Variable] = value;
                    break;
                default:
                    lcl = _locals.Peek();
                    lcl[atmp.Variable] = value;
                    break;
            }
            */

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
                returnValue = evaluateExpression(s.Expression);

            return returnValue;
        }

        // //////////////////////////////////////////////////////////////
        public object EvaluateStatment(BoundBlockStatement statment)
        {
            Dictionary<string, int> lableToIndex;
            BoundLabelStatement l;
            BoundStatement s;
            BoundGoToConditionalStatement cgts;
            BoundGoToStatement gs;
            BoundBlockStatement body;
            int index;
            bool cond;
            string lblName;

            body = ROOT;
            lableToIndex = new Dictionary<string, int>();

            for (int i = 0; i < statment.Statements.Length; i++)
            {
                if (statment.Statements[i] is BoundLabelStatement)
                {
                    l = (BoundLabelStatement)statment.Statements[i];
                    lblName = l.Label.Name;
                    lableToIndex.Add(lblName, i + 1);
                }
            }

            index = 0;
            while (index < statment.Statements.Length)
            {
                s = statment.Statements[index];

                switch (s.tok)
                {
                    case BoundNodeToken.BNT_EXPRSTMT:
                        evaluateExpressionStatement((BoundExpressionStatement)s);
                        break;
                    case BoundNodeToken.BNT_VARDECLR:
                        evaluateVariableDaclaration((boundVariableDeclaration)s);
                        break;
                    case BoundNodeToken.BNT_GOTOCOND:
                        cgts = (BoundGoToConditionalStatement)s;
                        cond = (bool)evaluateExpression(cgts.Condition);
                        if ((cond == true && cgts.JumpIfFalse == false) || (cond == false && cgts.JumpIfFalse == true))
                        {
                            lblName = cgts.Label.Name;
                            index = lableToIndex[lblName];
                            continue;
                        }
                        break;
                    case BoundNodeToken.BNT_GOTO:
                        gs = (BoundGoToStatement)s;
                        lblName = gs.Label.Name;
                        index = lableToIndex[lblName];
                        continue;
                    case BoundNodeToken.BNT_RETSTMT:
                        lastValue =  evaluateReturnExpression((BoundReturnStatement)s);
                        return lastValue;
                    case BoundNodeToken.BNT_LABEL:
                        break;
                    default:
                        s.writeTo(Console.Out);
                        throw new Exception(string.Format("unexpected token {0}", s.tok));
                }
                index++;
            }

            return lastValue;
        }

        // //////////////////////////////////////////////////////////////
        public object Evaluate()
        {
            return EvaluateStatment(ROOT);
        }
    }
}
