using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    sealed class BoundGoToStatement : BoundStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_GOTO;
        public BoundLabel Label { get; }

        public BoundGoToStatement(BoundLabel lbl)
        {
            Label = lbl;
        }
    }
}
