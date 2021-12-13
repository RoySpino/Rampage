using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundForStatement : BoundStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_FORSTMT;
        public VariableSymbol Variable { get; }
        public BoundExpression LBound { get; }
        public BoundExpression Ubound { get; }
        public BoundExpression IncrementBy { get; }
        public BoundStatement Body { get; }
        public BoundLabel BreakLbl { get; }
        public BoundLabel ContinueLbl { get; }
        public bool IsCountUP;

        public BoundForStatement(VariableSymbol variable, BoundExpression lBound, BoundExpression ubound, BoundStatement body, bool isCountUp, BoundExpression incrementBy = null, BoundLabel breakLbl = null, BoundLabel continueLbl = null)
        {
            Variable = variable;
            LBound = lBound;
            Ubound = ubound;
            Body = body;
            IsCountUP = isCountUp;
            BreakLbl = breakLbl;
            ContinueLbl = continueLbl;

            if (incrementBy == null)
                IncrementBy = new BoundLiteralExp(1);
            else
                IncrementBy = incrementBy;
        }

    }
}
