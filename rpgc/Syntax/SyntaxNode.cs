using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc
{
    public abstract class SyntaxNode
    {
        public TokenKind kind;

        public SyntaxNode()
        {
            kind = 0;
        }
        public abstract IEnumerable<SyntaxNode> getCildren();
    }


    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////
    public class ExpresionSyntax : SyntaxNode
    {

        // constructor (SyntaxToken) is handeld on number expresion
        public override IEnumerable<SyntaxNode> getCildren()
        {
            yield return null;
        }
    }


    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////
    // NumberExpressionSyntaxpressionSyntax
    public sealed class LiteralExpressionSyntax : ExpresionSyntax
    {
        public SyntaxToken LiteralToken;
        public object value;

        public LiteralExpressionSyntax(SyntaxToken token) : this(token, token.sym)
        {
        }

        public LiteralExpressionSyntax(SyntaxToken token, object Value)
        {
            kind = TokenKind.TK_LITEXPR;

            // assign literal type
            switch (token.tok)
            {
                case TokenKind.TK_INTEGER:
                case TokenKind.TK_ZONED:
                case TokenKind.TK_PACKED:
                    value = Convert.ToInt32(Value);
                    break;
                default:
                    value = Value;
                    break;
            }

            LiteralToken = token;
            LiteralToken.kind = LiteralToken.tok;
        }

        public override IEnumerable<SyntaxNode> getCildren()
        {
            yield return LiteralToken;
        }
    }


    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////
    public sealed class BinaryExpressionSyntax : ExpresionSyntax
    {
        public ExpresionSyntax left, right;
        public SyntaxToken operatorToken;

        public BinaryExpressionSyntax(ExpresionSyntax A, SyntaxToken operaton, ExpresionSyntax B)
        {
            left = A;
            right = B;
            kind = TokenKind.TK_BYNARYEXPR;

            operatorToken = operaton;
            operatorToken.kind = operaton.tok;
        }

        public override IEnumerable<SyntaxNode> getCildren()
        {
            yield return left;
            yield return operatorToken;
            yield return right;
        }
    }

    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////
    public sealed class UinaryExpressionSyntax : ExpresionSyntax
    {
        public ExpresionSyntax right;
        public SyntaxToken Operand;

        public UinaryExpressionSyntax(SyntaxToken operand, ExpresionSyntax B)
        {
            right = B;
            kind = TokenKind.TK_UNIEXP;

            Operand = operand;
            Operand.kind = operand.tok;
        }

        public override IEnumerable<SyntaxNode> getCildren()
        {
            yield return Operand;
            yield return right;
        }
    }


    // /////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     //
    // ///////////////////////////////////////////////////////////////////////
    public sealed class ParenthesizedExpression : ExpresionSyntax
    {
        SyntaxToken OPENParen, CLOSEParen;
        public ExpresionSyntax Expression;

        public ParenthesizedExpression(SyntaxToken openParen, ExpresionSyntax expression, SyntaxToken closeParen)
        {
            OPENParen = openParen;
            CLOSEParen = closeParen;
            Expression = expression;

            kind = TokenKind.TK_PARENEXP;
        }

        public override IEnumerable<SyntaxNode> getCildren()
        {
            yield return OPENParen;
            yield return Expression;
            yield return CLOSEParen;
        }
    }
}
