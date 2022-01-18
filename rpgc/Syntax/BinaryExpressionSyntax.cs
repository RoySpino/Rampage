using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed partial class BinaryExpressionSyntax : ExpresionSyntax
    {
        public override TokenKind kind => TokenKind.TK_BYNARYEXPR;
        public ExpresionSyntax left { get; }
        public SyntaxToken operatorToken { get; }
        public ExpresionSyntax right { get; }

        public BinaryExpressionSyntax(SyntaxTree stree, ExpresionSyntax A, SyntaxToken operaton, ExpresionSyntax B)
            : base(stree)
        {
            left = A;
            right = B;
            //kind = TokenKind.TK_BYNARYEXPR;

            operatorToken = operaton;
            //operatorToken.kind = operaton.tok;
        }
    }
}
