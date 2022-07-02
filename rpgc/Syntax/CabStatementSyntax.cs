using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Syntax
{
    class CabStatementSyntax : StatementSyntax
    {
        public SyntaxTree STree { get; }
        public SyntaxToken Keyword { get; }
        public List<ExpresionSyntax> Conditions { get; }
        public List<StatementSyntax> Statements { get; }
        public override TokenKind kind => TokenKind.TK_CAB;

        public CabStatementSyntax(SyntaxTree sTree, SyntaxToken keyword, List<ExpresionSyntax> conditions, List<StatementSyntax> statements)
            :base(sTree)
        {
            STree = sTree;
            Keyword = keyword;
            Conditions = conditions;
            Statements = statements;
        }
    }
}
