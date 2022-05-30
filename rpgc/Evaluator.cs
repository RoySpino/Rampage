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
        private readonly BoundBlockStatement ROOT;
        private readonly BoundGlobalScope globals;
        Dictionary<VariableSymbol, object> _Globals;
        Dictionary<FunctionSymbol, BoundBlockStatement> _functions = new Dictionary<FunctionSymbol, BoundBlockStatement>();
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new Stack<Dictionary<VariableSymbol, object>>();
        private object lastValue;
        private Random randnum;
        private readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> FunctionBodies;


        public Evaluator(BoundProgram pgm, Dictionary<VariableSymbol, object> _variables)
        {
            BoundProgram c;

            program = pgm;
            _Globals = _variables;
            _locals.Push(new Dictionary<VariableSymbol, object>());
            globals = pgm.GblScope;

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

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> _variables)
        {
            program = null;
            ROOT = root;
            _Globals = _variables;

            var c = program;
        }

        public Evaluator(ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functionBodies, BoundBlockStatement st, Dictionary<VariableSymbol, object> variables)
        {
            FunctionBodies = _functionBodies;
            ROOT = st;
            _Globals = variables;
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
            var value = evaluateExpression(node._Expression);

            if (node.Type == TypeSymbol.Any)
                return value;
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

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateBoundBinaryExpression(BoundBinExpression biTmp)
        {
            object chameleonOBJL;
            object chameleonOBJR;
            object ans;

            chameleonOBJL = reEvaluate(biTmp.Left);
            chameleonOBJR = reEvaluate(biTmp.Right);

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

            if (node.Function == rpgc.Symbols.BuiltinFunctions.cin)
                return Console.ReadLine();
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.cout)
            {
                string msg = reEvaluate((node.Arguments[0])).ToString();
                Console.WriteLine(msg);
                return null;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.dsply)
            {
                string msg = reEvaluate((node.Arguments[0])).ToString();
                Console.WriteLine(msg);
                return null;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Int)
            {
                string Val = reEvaluate((node.Arguments[0])).ToString();
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
                string Val = reEvaluate((node.Arguments[0])).ToString();
                return Val;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Log)
            {
                string Val = reEvaluate((node.Arguments[0])).ToString();
                return (int)Math.Log(Convert.ToDouble(Val));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Log10)
            {
                string Val = reEvaluate((node.Arguments[0])).ToString();
                return (int)(Math.Log(Convert.ToDouble(Val)) / Math.Log(10.0));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_abs)
            {
                string Val = reEvaluate((node.Arguments[0])).ToString();
                return (int)Math.Abs(Convert.ToDouble(Val));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Sqrt)
            {
                string Val = reEvaluate((node.Arguments[0])).ToString();
                return (int)Math.Sqrt(Convert.ToDouble(Val));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Rem)
            {
                string Val0 = reEvaluate((node.Arguments[0])).ToString();
                string Val1 = reEvaluate((node.Arguments[1])).ToString();
                return (int)((Convert.ToDouble(Val0) % Convert.ToDouble(Val1)));
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Len)
            {
                string Val = reEvaluate((node.Arguments[0])).ToString();
                return Val.Length;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Lower)
            {
                string Val = reEvaluate((node.Arguments[0])).ToString();
                return Val.ToLower();
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Upper)
            {
                string Val = reEvaluate((node.Arguments[0])).ToString();
                return Val.ToUpper();
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Subst)
            {
                string Val0 = reEvaluate((node.Arguments[0])).ToString();
                string Val1 = reEvaluate((node.Arguments[1])).ToString();
                string Val2 = reEvaluate((node.Arguments[2])).ToString();
                string result;
                int sidx, len;

                sidx = Convert.ToInt32(Val1) - 1;
                len = Convert.ToInt32(Val2);
                result = Val0.Substring(sidx, len);

                return result;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Rand)
            {
                int max;
                string Val0 = reEvaluate((node.Arguments[0])).ToString();

                if (randnum == null)
                    randnum = new Random();

                max = Convert.ToInt32(Val0);
                return randnum.Next(max);
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_div)
            {
                int Val0 = Convert.ToInt32(reEvaluate((node.Arguments[0])));
                int Val1 = Convert.ToInt32(reEvaluate((node.Arguments[1])));

                return (int)(Val0 / Val1);
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Scan)
            {
                string whatToFind = reEvaluate((node.Arguments[0])).ToString();
                string source = reEvaluate((node.Arguments[1])).ToString();
                int idx;

                idx = source.IndexOf(whatToFind) + 1;

                return idx;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Replace)
            {
                string wrdValue = reEvaluate((node.Arguments[0])).ToString();
                string source = reEvaluate((node.Arguments[1])).ToString();
                int startIndex = Convert.ToInt32(reEvaluate((node.Arguments[2])));
                int length = Convert.ToInt32(reEvaluate((node.Arguments[3])));
                string tmp;

                tmp = wrdValue.Substring(0, length);

                if ((startIndex + length) >= source.Length)
                    return (source.Substring(0, startIndex - 1) + tmp);

                tmp = source.Substring(0, startIndex - 1) + tmp + source.Substring((startIndex + length - 1));
                return tmp;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Trim)
            {
                string value = reEvaluate((node.Arguments[0])).ToString();

                value = value.Trim();
                return value;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Xlate)
            {
                string fromStr, toStr, source, res;
                int lim;

                fromStr = reEvaluate((node.Arguments[0])).ToString();
                toStr = reEvaluate((node.Arguments[1])).ToString();
                source = reEvaluate((node.Arguments[2])).ToString();

                // get the smallest length as the limit
                lim = fromStr.Length;
                if (lim > toStr.Length)
                    lim = toStr.Length;

                // perform translation
                res = source;
                for (int i = 0; i < lim; i++)
                {
                    // dont only if symbol is in string
                    if (res.Contains(fromStr[i]) == true)
                        res = res.Replace(fromStr[i], toStr[i]);
                }

                return res;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Check)
            {
                string fromStr, source;
                int idx;

                fromStr = reEvaluate((node.Arguments[0])).ToString();
                source = reEvaluate((node.Arguments[1])).ToString();

                var mtch = System.Text.RegularExpressions.Regex.Match(source, $"[^{fromStr}]");
                idx = mtch.Index;

                return idx + 1;
            }
            else if (node.Function == rpgc.Symbols.BuiltinFunctions.BIF_Checkr)
            {
                string fromStr, source;
                int idx;

                fromStr = reEvaluate((node.Arguments[0])).ToString();
                source = reEvaluate((node.Arguments[1])).ToString();

                source.Reverse();
                var mtch = System.Text.RegularExpressions.Regex.Match(source, $"[^{fromStr}]");
                idx = mtch.Index;
                idx = (source.Length - 1) - idx;

                return idx + 1;
            }
            else
            {
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
            Dictionary<VariableSymbol, object> tmpLocalStack;

            value = reEvaluate(node.Initalizer);
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

            // get value from apropreate dictionary
            /*
            switch (vtmp.Variable.kind)
            {
                case SymbolKind.SYM_GLOBALVAR:
                    varb = _Globals[idx];
                    break;
                default:
                    lcl = _locals.Peek();
                    varb = lcl[idx];
                    break;
            }
            */

            return varb;
        }

        // //////////////////////////////////////////////////////////////////////////
        private object evaluateAssignmentExpression(BoundAssignmentExpression atmp)
        {
            object value;
            Dictionary<VariableSymbol, object> lcl;

            // get value
            value = reEvaluate(atmp.Expression);

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
                returnValue = reEvaluate(s.Expression);

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
                        cond = (bool)reEvaluate(cgts.Condition);
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
