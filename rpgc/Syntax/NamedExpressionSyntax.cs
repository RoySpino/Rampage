using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class NamedExpressionSyntax : ExpresionSyntax
    {
        public override TokenKind kind => TokenKind.TK_NAMEDEXP;
        public SyntaxToken IDENTIFIERTOKEN { get;}

        public NamedExpressionSyntax(SyntaxToken identifier)
        {
            IDENTIFIERTOKEN = identifier;
            //kind = TokenKind.TK_NAMEDEXP;
        }
    }
}
