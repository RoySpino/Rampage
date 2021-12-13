using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    sealed class BoundLabelStatement : BoundStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_LABEL;
        public BoundLabel Label { get; }

        public BoundLabelStatement(BoundLabel symbol)
        {
            Label = symbol;
        }
    }
}
