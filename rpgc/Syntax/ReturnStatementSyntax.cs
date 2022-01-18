using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class ReturnStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_RETURN;
        public SyntaxToken ReturnKeyword { get; }
        public ExpresionSyntax Expression { get; }

        public ReturnStatementSyntax(SyntaxTree stree, SyntaxToken return_KeyWord, ExpresionSyntax returnExp = null)
            :base(stree)
        {
            ReturnKeyword = return_KeyWord;
            Expression = returnExp;
        }
    }
}
