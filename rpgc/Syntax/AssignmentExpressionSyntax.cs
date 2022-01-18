using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    public class AssignmentExpressionSyntax : ExpresionSyntax
    {
        public override TokenKind kind => TokenKind.TK_ASSIGN;
        public SyntaxToken IDENTIFIERTOKEN { get; }
        public SyntaxToken ASSIGNMENTTOKEN { get; }
        public ExpresionSyntax EXPRESSION { get; }

        public AssignmentExpressionSyntax(SyntaxTree stree,SyntaxToken identifier, SyntaxToken assignmentSymbol, ExpresionSyntax expression)
            :base(stree)
        {
            IDENTIFIERTOKEN = identifier;
            ASSIGNMENTTOKEN = assignmentSymbol;
            EXPRESSION = expression;
            //kind = TokenKind.TK_ASSIGN;
        }
    }
}
