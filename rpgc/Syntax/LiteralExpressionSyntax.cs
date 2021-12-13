using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed partial class LiteralExpressionSyntax : ExpresionSyntax
    {
        public override TokenKind kind => TokenKind.TK_LITEXPR;
        public SyntaxToken LiteralToken { get; }
        public object value { get; }

        public LiteralExpressionSyntax(SyntaxToken token) : this(token, token.sym)
        {
        }

        // /////////////////////////////////////////////////////////////////////////
        public LiteralExpressionSyntax(SyntaxToken token, object Value)
        {
            LiteralToken = token;

            // assign literal type
            switch (token.kind)
            {
                case TokenKind.TK_INTEGER:
                case TokenKind.TK_ZONED:
                case TokenKind.TK_PACKED:
                    value = Convert.ToInt32(Value);
                    break;
                case TokenKind.TK_STRING:
                    value = Value.ToString();
                    break;
                case TokenKind.TK_FLOAT:
                    value = Convert.ToDouble(Value);
                    break;
                case TokenKind.TK_DATE:
                case TokenKind.TK_TIMESTAMP:
                    value = Convert.ToDateTime(Value);
                    break;
                case TokenKind.TK_INDICATOR:
                case TokenKind.TK_INDON:
                case TokenKind.TK_INDOFF:
                    value = ((Value == null) ? true : (bool)Value);
                    break;
                default:
                    value = Value.ToString();
                    break;
            }
        }
    }
}
