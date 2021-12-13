namespace rpgc.Binding
{
    internal class BoundErrorStatement : BoundStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_ERROREXP;

        public BoundErrorStatement()
        {
            //
        }
    }
}