using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Binding;

namespace rpgc
{
    public class Evaluator
    {
        private readonly BoundExpression ROOT;
        Dictionary<VariableSymbol, object> variables;

        public Evaluator(BoundExpression root)
        {
            ROOT = root;
        }
        public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> _variables)
        {
            ROOT = root;
            variables = _variables;
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
        private object EvaluatExpression(BoundExpression root)
        {
            BoundLiteralExp bLit;
            BoundBinExpression biTmp;
            ParenthesizedExpression prTmp;
            BoundUniExpression uiTmp;
            BoundVariableExpression vtmp;
            BoundAssignmentExpression atmp;
            object value;
            int L, R, operand;

            //handle variables
            if (root is BoundVariableExpression)
            {
                vtmp = (BoundVariableExpression)root;
                variables.TryGetValue(vtmp.Variable, out value);
                value = variables[vtmp.Variable];
                return value;
            }

            // perform or create variable for assignment
            if (root is BoundAssignmentExpression)
            {
                atmp = (BoundAssignmentExpression)root;
                value = EvaluatExpression(atmp.Expression);
                variables[atmp.Variable] = value;
                return value;
            }

            // evaluate single numbers/vars
            if (root is BoundLiteralExp)
            {
                bLit = (BoundLiteralExp)root;
                return bLit.Value;
            }

            // handle -/+ operator assignment
            if (root is BoundUniExpression)
            {
                uiTmp = (BoundUniExpression)root;
                var chameleonOBJ = EvaluatExpression(uiTmp.right);

                // convert to neg or positive
                switch (uiTmp.OP.tok)
                {
                    case BoundUniOpToken.BUO_IDENTITY:
                        operand = Convert.ToInt32(chameleonOBJ);
                        return operand;
                    case BoundUniOpToken.BUO_NEGATION:
                        operand = Convert.ToInt32(chameleonOBJ);
                        return (-1 * operand);
                    case BoundUniOpToken.BUO_NOT:
                        return !Convert.ToBoolean(chameleonOBJ);
                    default:
                        throw new Exception(string.Format("unrecognized uinary Operator [{0}] ", uiTmp.tok));
                }
            }


            // evaluate mathmatic/Binary expressions
            if (root is BoundBinExpression)
            {
                biTmp = (BoundBinExpression)root;

                var chameleonOBJL = EvaluatExpression(biTmp.Left);
                var chameleonOBJR = EvaluatExpression(biTmp.Right);

                switch (biTmp.OP.tok)
                {
                    case BoundBinOpToken.BBO_ADD:
                        return Convert.ToInt32(chameleonOBJL) + Convert.ToInt32(chameleonOBJR);
                    case BoundBinOpToken.BBO_SUB:
                        return Convert.ToInt32(chameleonOBJL) - Convert.ToInt32(chameleonOBJR);
                    case BoundBinOpToken.BBO_MULT:
                        return Convert.ToInt32(chameleonOBJL) * Convert.ToInt32(chameleonOBJR);
                    case BoundBinOpToken.BBO_DIV:
                        return Convert.ToInt32(chameleonOBJL) / Convert.ToInt32(chameleonOBJR);

                    case BoundBinOpToken.BBO_AND:
                        return Convert.ToBoolean(chameleonOBJL) && Convert.ToBoolean(chameleonOBJR);
                    case BoundBinOpToken.BBO_OR:
                        return Convert.ToBoolean(chameleonOBJL) || Convert.ToBoolean(chameleonOBJR);
                    case BoundBinOpToken.BBO_EQ:
                        return Equals(chameleonOBJL, chameleonOBJR);
                    case BoundBinOpToken.BBO_NE:
                        return !Equals(chameleonOBJL, chameleonOBJR);
                    case BoundBinOpToken.BBO_GE:
                        return DoComparison(">=", chameleonOBJL.GetType(), chameleonOBJR.GetType(), chameleonOBJL, chameleonOBJR);
                    case BoundBinOpToken.BBO_GT:
                        return DoComparison(">", chameleonOBJL.GetType(), chameleonOBJR.GetType(), chameleonOBJL, chameleonOBJR);
                    case BoundBinOpToken.BBO_LE:
                        return DoComparison("<=", chameleonOBJL.GetType(), chameleonOBJR.GetType(), chameleonOBJL, chameleonOBJR);
                    case BoundBinOpToken.BBO_LT:
                        return DoComparison("<", chameleonOBJL.GetType(), chameleonOBJR.GetType(), chameleonOBJL, chameleonOBJR);
                    default:
                        throw new Exception(string.Format("unexpected Binary operator {0}", biTmp.OP.tok));
                }
            }

            throw new Exception(string.Format("unexpected token {0}", root.tok));
        }

        // //////////////////////////////////////////////////////////////
        public object Evaluate()
        {
            return EvaluatExpression(ROOT);
        }
    }
}
