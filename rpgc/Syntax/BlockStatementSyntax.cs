using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class BlockStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_BLOCKSYNTX;
        public SyntaxToken BlockStartToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken BlockEndToken { get; }

        public BlockStatementSyntax(SyntaxTree stree, SyntaxToken blockStartToken, ImmutableArray<StatementSyntax> statements, SyntaxToken blockEndToken)
            : base(stree)
        {
            BlockStartToken = blockStartToken;
            Statements = statements;
            BlockEndToken = blockEndToken;
        }
    }
}
