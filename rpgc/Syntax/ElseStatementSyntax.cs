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

        public ElseStatementSyntax(SyntaxToken elseKeyword, StatementSyntax elseStatement)
        {
            ElseKeyword = elseKeyword;
            ElseStatement = elseStatement;
        }
    }
}
