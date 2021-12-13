using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundWhileStatement : BoundLoopStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_WHILESTMT;
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }

        public BoundWhileStatement(BoundExpression condition, BoundStatement body, BoundLabel breakLbl = null, BoundLabel continueLbl = null)
            : base(breakLbl, continueLbl)
        {
            Condition = condition;
            Body = body;
        }
    }
}
