namespace rpgc.Syntax
{
    internal class BreakStamentSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_LEAVE;
        public SyntaxToken Keyword { get; }

        public BreakStamentSyntax(SyntaxTree stree, SyntaxToken keywrd)
            : base(stree)
        {
            Keyword = keywrd;
        }
    }
}