namespace rpgc.Syntax
{
    internal class BreakStamentSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_LEAVE;
        public SyntaxToken Keyword { get; }

        public BreakStamentSyntax(SyntaxToken keywrd)
        {
            Keyword = keywrd;
        }
    }
}