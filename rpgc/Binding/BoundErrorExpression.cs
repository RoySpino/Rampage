using rpgc.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_ERROREXP;
        public override TypeSymbol Type => TypeSymbol.ERROR;

        public BoundErrorExpression()
        {
            //
        }
    }
}
