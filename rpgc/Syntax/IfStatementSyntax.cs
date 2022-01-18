using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class IfStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_IF;
        public SyntaxToken IfKeyword { get; }
        public ExpresionSyntax Condition { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseStatementSyntax ElseBlock { get; }

        public IfStatementSyntax(SyntaxTree stree, SyntaxToken ifKeyword, ExpresionSyntax condition, StatementSyntax thenStatement, ElseStatementSyntax elseBlock)
            : base(stree)
        {
            IfKeyword = ifKeyword;
            Condition = condition;
            ThenStatement = thenStatement;
            ElseBlock = elseBlock;
        }
    }
}
