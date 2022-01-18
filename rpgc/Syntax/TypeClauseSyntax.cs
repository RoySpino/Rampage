using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class TypeClauseSyntax : SyntaxNode
    {
        public override TokenKind kind => TokenKind.TK_TYPCLAUSE;
        public SyntaxToken Identifier { get; }

        public TypeClauseSyntax(SyntaxTree stree, SyntaxToken idenifir)
            : base(stree)
        {
            Identifier = idenifir;
        }
    }
}
