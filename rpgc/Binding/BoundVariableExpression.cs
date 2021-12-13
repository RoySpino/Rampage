using rpgc.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal class BoundVariableExpression : BoundExpression
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_VAREX;
        public override TypeSymbol Type => Variable.getType();
        public string Name;
        public VariableSymbol Variable { get; }

        public BoundVariableExpression(VariableSymbol variable)
        {
            Variable = variable;

            Name = variable.Name;
        }
    }
}
