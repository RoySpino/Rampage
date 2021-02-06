using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundUniExpression : BoundExpression
    {
        public BoundUniOperator OP;
        public BoundExpression right;

        public BoundUniExpression(BoundUniOperator op, BoundExpression operand)
        {
            OP = op;
            right = operand;

            type = operand.type;
            tok = BoundNodeToken.BNT_UINEX;
        }

        // /////////////////////////////////////////////////////////////////////////////////
        public Type resultType()
        {
            return OP.ResultType;
        }
    }
}
