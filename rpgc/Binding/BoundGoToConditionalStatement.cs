using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    sealed class BoundGoToConditionalStatement : BoundStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_GOTOCOND;
        public BoundLabel Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfFalse { get; }

        public BoundGoToConditionalStatement(BoundLabel lbl, BoundExpression condition, bool jumpIfFalse = false)
        {
            Label = lbl;
            Condition = condition;
            JumpIfFalse = jumpIfFalse;
        }
    }
}
