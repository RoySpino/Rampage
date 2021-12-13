using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundUntilStatement : BoundLoopStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_DOUNTIL;
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
        public BoundLabel BreakLbl { get; }
        public BoundLabel ContinueLbl { get; }

        public BoundUntilStatement(BoundExpression condition, BoundStatement body, BoundLabel breakLbl = null, BoundLabel continueLbl = null) 
            : base (breakLbl, continueLbl)
        {
            Condition = condition;
            Body = body;
            BreakLbl = breakLbl;
            ContinueLbl = continueLbl;
        }
    }
}
