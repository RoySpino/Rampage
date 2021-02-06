using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class AssignmentExpressionSyntax : ExpresionSyntax
    {
        public SyntaxToken IDENTIFIERTOKEN, ASSIGNMENTTOKEN;
        public ExpresionSyntax EXPRESSION;

        public AssignmentExpressionSyntax(SyntaxToken identifier, SyntaxToken assignmentSymbol, ExpresionSyntax expression)
        {
            IDENTIFIERTOKEN = identifier;
            ASSIGNMENTTOKEN = assignmentSymbol;
            EXPRESSION = expression;
            kind = TokenKind.TK_ASSIGN; ;
        }

        public override IEnumerable<SyntaxNode> getCildren()
        {
            yield return IDENTIFIERTOKEN;
            yield return ASSIGNMENTTOKEN;
            yield return EXPRESSION;
        }
    }
}
