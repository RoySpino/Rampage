using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed partial class UinaryExpressionSyntax : ExpresionSyntax
    {
        public override TokenKind kind => TokenKind.TK_UNIEXP;
        public SyntaxToken Operand { get; }
        public ExpresionSyntax right { get; }

        public UinaryExpressionSyntax(SyntaxTree stree, SyntaxToken operand, ExpresionSyntax B)
            : base(stree)
        {
            right = B;
            //kind = TokenKind.TK_UNIEXP;

            Operand = operand;
            //Operand.kind = operand.tok;
        }
    }
}
