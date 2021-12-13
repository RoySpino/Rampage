using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed partial class ParenthesizedExpression : ExpresionSyntax
    {
        public override TokenKind kind => TokenKind.TK_PARENEXP;
        public SyntaxToken OPENParen { get; }
        public ExpresionSyntax Expression { get; }
        public SyntaxToken CLOSEParen { get; }

        public ParenthesizedExpression(SyntaxToken openParen, ExpresionSyntax expression, SyntaxToken closeParen)
        {
            OPENParen = openParen;
            CLOSEParen = closeParen;
            Expression = expression;

            //kind = TokenKind.TK_PARENEXP;
        }
    }
}
