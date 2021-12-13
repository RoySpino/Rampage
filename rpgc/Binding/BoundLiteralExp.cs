using rpgc.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundLiteralExp : BoundExpression
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_LITEX;
        public override TypeSymbol Type { get; }
        public object Value { get; }

        public BoundLiteralExp(object value)
        {
            Value = value;

            if (value is int)
                Type = TypeSymbol.Integer;
            else
                if (value is float || value is double)
                    Type = TypeSymbol.Float;
                else
                    if (value is bool)
                        Type = TypeSymbol.Indicator;
                    else
                        if (value is string || value is char)
                            Type = TypeSymbol.Char;
                        else
                            if (value is VariableSymbol)
                                Type = ((VariableSymbol)value).getType();
                            else
                                throw new Exception($"Unexpected literal {value} of type{value.GetType()}");
        }
    }
}
