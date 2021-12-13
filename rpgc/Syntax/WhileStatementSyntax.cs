using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    internal sealed class WhileStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_DOW;
        public SyntaxToken Keyword { get; }
        public ExpresionSyntax Condition { get; }
        public StatementSyntax Body { get; }

        public WhileStatementSyntax(SyntaxToken keyword, ExpresionSyntax condition, StatementSyntax body)
        {
            Keyword = keyword;
            Condition = condition;
            Body = body;
        }
    }
}
