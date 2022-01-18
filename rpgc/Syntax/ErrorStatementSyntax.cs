namespace rpgc.Syntax
{
    internal class ErrorStatementSyntax : StatementSyntax
    {
        public override TokenKind kind => TokenKind.TK_BADTOKEN;
        public ErrorStatementSyntax(SyntaxTree st)
            :base(st)
        {
            //
        }
    }
}