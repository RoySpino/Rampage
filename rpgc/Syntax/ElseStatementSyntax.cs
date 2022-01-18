using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class ElseStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_ELSE;
        public SyntaxToken ElseKeyword { get; }
        public StatementSyntax ElseStatement { get; }

        public ElseStatementSyntax(SyntaxTree stree, SyntaxToken elseKeyword, StatementSyntax elseStatement)
            : base(stree)
        {
            ElseKeyword = elseKeyword;
            ElseStatement = elseStatement;
        }
    }
}
