namespace rpgc.Syntax
{
    internal class ContinueStamentSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_ITER;
        public SyntaxToken keyword { get; }

        public ContinueStamentSyntax(SyntaxToken keywrd)
        {
            keyword = keywrd;
        }
    }
}