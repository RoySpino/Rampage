using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Syntax;

namespace rpgc.Binding
{
    public enum BoundNodeToken
    {
        BNT_UINEX,
        BNT_LITEX,
        BNT_VAREX,
        BNT_ASNEX
    }

    public enum BoundUniOpToken
    {
        BUO_IDENTITY,
        BUO_NEGATION,
        BUO_NOT
    }

    public enum BoundBinOpToken
    {
        BBO_ADD,
        BBO_SUB,
        BBO_MULT,
        BBO_DIV,

        BBO_AND,
        BBO_NOT,
        BBO_OR,
        BBO_GE,
        BBO_GT,
        BBO_LE,
        BBO_LT,
        BBO_EQ,
        BBO_NE
    }


    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////
    public abstract class BoundNode
    {
        public BoundNodeToken tok;
    }


    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////
    public abstract class BoundExpression : BoundNode
    {
        public Type type;
    }


    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////
    internal sealed class BoundLiteralExp : BoundExpression
    {
        public object Value;

        public BoundLiteralExp(object value)
        {
            Value = value;
            type = Value.GetType();
            tok = BoundNodeToken.BNT_LITEX;
        }
    }

    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////

    internal class BoundVariableExpression : BoundExpression
    {
        public string Name;
        public VariableSymbol Variable;

        public BoundVariableExpression(VariableSymbol variable)
        {
            Variable = variable;

            type = variable.type;
            Name = variable.Name;

            tok = BoundNodeToken.BNT_VAREX;
        }
    }

    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////

    internal class BoundAssignmentExpression : BoundExpression
    {
        public BoundExpression Expression;
        public VariableSymbol Variable;

        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression expression)
        {
            tok = BoundNodeToken.BNT_ASNEX;
            type = expression.type;
            Expression = expression;
            Variable = variable;
        }
    }

    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////
    internal sealed class Binder
    {
        public DiagnosticBag diagnostics = new DiagnosticBag();
        public Dictionary<VariableSymbol, object> variables;

        public Binder(Dictionary<VariableSymbol, object> _variables)
        {
            variables = _variables;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public BoundExpression BindExpression(ExpresionSyntax syntax)
        {
            switch (syntax.kind)
            {
                case TokenKind.TK_PARENEXP:
                    return BindParenthesizedExpression((ParenthesizedExpression)syntax);
                case TokenKind.TK_LITEXPR:
                    return BindLiteralExp((LiteralExpressionSyntax)syntax);
                case TokenKind.TK_UNIEXP:
                    return BindUniExpression((UinaryExpressionSyntax)syntax);
                case TokenKind.TK_BYNARYEXPR:
                    return BindBinExpression((BinaryExpressionSyntax)syntax);
                case TokenKind.TK_NAMEDEXP:
                    return BindNamedExpression((NamedExpressionSyntax)syntax);
                case TokenKind.TK_ASSIGN:
                    return BindAssignmentExpression((AssignmentExpressionSyntax)syntax);
                default:
                    throw new Exception(string.Format("Unexpected Syntax {0}", syntax.kind));
            }
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindParenthesizedExpression(ParenthesizedExpression syntax)
        {
            return BindExpression(syntax.Expression);
            //return null;
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindNamedExpression(NamedExpressionSyntax syntax)
        {
            string name = syntax.IDENTIFIERTOKEN.sym.ToString();
            object value;
            VariableSymbol _variable;

            _variable = variables.Keys.FirstOrDefault(v => v.Name == name);

            if (_variable == null)
            {
                diagnostics.reportUndefinedName(syntax.IDENTIFIERTOKEN.span, name);
                return new BoundLiteralExp(0);
            }
            
            return new BoundVariableExpression(_variable);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            string name;
            BoundExpression boundExp;
            object defaultType;
            VariableSymbol _variable, existingVar;

            name = syntax.IDENTIFIERTOKEN.sym.ToString();
            boundExp = BindExpression(syntax.EXPRESSION);
            existingVar = variables.Keys.FirstOrDefault(v => v.Name == name);

            if (existingVar != null)
                variables.Remove(existingVar);

            _variable = new VariableSymbol(name, boundExp.type);
            variables[_variable] = null;
            
            return new BoundAssignmentExpression(_variable, boundExp);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindLiteralExp(LiteralExpressionSyntax syntax)
        {
            //int Value=0;
            object tmp;
            object Value;

            Value = syntax.value ?? 0;
            // try { Value = Convert.ToInt32(tmp.ToString()); }
            // catch(Exception) { Value = 0; }

            return new BoundLiteralExp(Value);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindUniExpression(UinaryExpressionSyntax syntax)
        {
            BoundExpression expression;
            //BoundUniOpToken? boundOperatorKind;
            BoundUniOperator boundOperatorKind;

            expression = BindExpression(syntax.right);
            boundOperatorKind = BoundUniOperator.bind(syntax.Operand.tok, expression.type);

            // account for errors
            if (boundOperatorKind == null)
            {
                diagnostics.reportUndefinedUniaryOp(syntax.Operand.span, syntax.Operand.sym.ToString(), expression.type);
                //addError(string.Format("uninary operator {0} is not defined for type {1}", syntax.Operand.sym, expression.type));
                return expression;
            }

            //return new BoundUniExpression((BoundUniOpToken)boundOperatorKind, expression);
            return new BoundUniExpression(boundOperatorKind, expression);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        private BoundExpression BindBinExpression(BinaryExpressionSyntax syntax)
        {
            BoundExpression left, right;
            //BoundBinOpToken? boundOperatorKind;
            BoundBinOperator boundOperatorKind;

            left = BindExpression(syntax.left);
            right = BindExpression(syntax.right);
            boundOperatorKind = BoundBinOperator.bind(syntax.operatorToken.tok, left.type, right.type);

            // account for errors
            if (boundOperatorKind == null)
            {
                diagnostics.reportUndefinedBynaryOp(syntax.operatorToken.span, syntax.operatorToken.sym.ToString(), left.type, right.type);
                //addError(string.Format("binary operator {0} is not defined for types {1} and {2}", syntax.operatorToken.sym, left.type, right.type));
                return left;
            }

            //return new BoundBinExpression(left,(BoundBinOpToken)boundOperatorKind, right);
            return new BoundBinExpression(left, boundOperatorKind, right);
        }

        // ///////////////////////////////////////////////////////////////////////////////
        public DiagnosticBag getDiagnostics()
        {
            return diagnostics;
        }
    }
}
