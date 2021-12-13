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

        public TagStatementSyntax(SyntaxToken TagStatement, string Name)
        {
            LableName = Name;
            TagKeyword = TagStatement;
        }
    }
}
