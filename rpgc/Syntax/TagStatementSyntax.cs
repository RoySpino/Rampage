using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class TagStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_TAG;
        public SyntaxToken TagKeyword { get; }
        public string LableName { get; }

        public TagStatementSyntax(SyntaxTree stree, SyntaxToken TagStatement, string Name)
            : base(stree)
        {
            LableName = Name;
            TagKeyword = TagStatement;
        }
    }
}
