namespace rpgc.Syntax
{
    internal class ContinueStamentSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_ITER;
        public SyntaxToken keyword { get; }

        public ContinueStamentSyntax(SyntaxTree stree, SyntaxToken keywrd)
            : base(stree)
        {
            keyword = keywrd;
        }
    }
}