using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class NamedExpressionSyntax : ExpresionSyntax
    {
        public SyntaxToken IDENTIFIERTOKEN;

        public NamedExpressionSyntax(SyntaxToken identifier)
        {
            IDENTIFIERTOKEN = identifier;
            kind = TokenKind.TK_NAMEDEXP;
        }

        public override IEnumerable<SyntaxNode> getCildren()
        {
            yield return IDENTIFIERTOKEN;
        }
    }
}
