using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public sealed class ExpressionStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_EXPRNSTMNT;
        public ExpresionSyntax Expression { get; }

        public ExpressionStatementSyntax(ExpresionSyntax expresion)
        {
            Expression = expresion;
        }

    }
}
