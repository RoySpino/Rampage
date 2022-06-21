using System.Collections.Generic;

namespace rpgc.Syntax
{
    internal class SelectStatementSyntax : StatementSyntax
    {
        public SyntaxToken keyword { get; }
        public List<ExpresionSyntax> whenCond { get; }
        public List<StatementSyntax> whenBdy { get; }
        public StatementSyntax DefaultBody { get; }

        public override TokenKind kind => TokenKind.TK_SELECT;

        public SelectStatementSyntax(SyntaxTree sTree, SyntaxToken keyword, List<ExpresionSyntax> whenCond, List<StatementSyntax> whenBdy, StatementSyntax defBody = null)
            : base(sTree)
        {
            this.keyword = keyword;
            this.whenCond = whenCond;
            this.whenBdy = whenBdy;
            DefaultBody = defBody;
        }
    }
}