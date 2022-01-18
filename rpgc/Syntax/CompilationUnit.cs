using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class CompilationUnit : SyntaxNode
    {
        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken EndOfFileToken { get; }

        public override TokenKind kind => TokenKind.TK_COMPLATIONUNT;

        //public CompilationUnit(StatementSyntax satement, SyntaxToken eofToken)
        public CompilationUnit(SyntaxTree stree,ImmutableArray<MemberSyntax> memberSatement, SyntaxToken eofToken)
            :base (stree)
        {
            Members = memberSatement;
            EndOfFileToken = eofToken;
        }
    }
}
