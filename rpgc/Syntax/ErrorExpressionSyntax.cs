namespace rpgc.Syntax
{
    internal class ErrorExpressionSyntax : ExpresionSyntax
    {
        public override TokenKind kind => TokenKind.TK_BADTOKEN;
        public ErrorExpressionSyntax()
        {
            //
        }
    }
}