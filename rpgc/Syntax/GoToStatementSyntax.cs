using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class GoToStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_GOTO;
        public SyntaxToken TagKeyword { get; }
        public string LableName { get; }

        public GoToStatementSyntax(SyntaxToken TagStatement, string Name)
        {
            LableName = Name;
            TagKeyword = TagStatement;
        }
    }
}
