using rpgc.Symbols;

namespace rpgc.Binding
{
    internal sealed class BoundConversionExpression : BoundExpression
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_CONVEXP;
        public override TypeSymbol Type { get; }
        public BoundExpression _Expression { get; }

        public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
        {
            Type = type;
            _Expression = expression;
        }
    }
}