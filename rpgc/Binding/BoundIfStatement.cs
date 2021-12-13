namespace rpgc.Binding
{
    internal sealed class BoundIfStatement :BoundStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_IFSTMT;
        public BoundExpression Condition { get; }
        public BoundStatement ThenStatement { get; }
        public BoundStatement ElseStatement { get; }

        public BoundIfStatement(BoundExpression condition, BoundStatement thenStatement, BoundStatement elseStatement)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
        }
    }
}