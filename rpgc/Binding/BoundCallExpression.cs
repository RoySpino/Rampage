using rpgc.Symbols;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundCallExpression : BoundExpression
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_CALLEXP;
        public override TypeSymbol Type { get; }
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }

        public BoundCallExpression(FunctionSymbol Function_, ImmutableArray<BoundExpression> args)
        {
            Arguments = args;
            Function = Function_;
            Type = Function_.Type;
        }
    }
}
