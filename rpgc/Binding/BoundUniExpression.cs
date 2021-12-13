using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rpgc.Symbols;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundUniExpression : BoundExpression
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_UINEX;
        public override TypeSymbol Type => OP.ResultType;
        public BoundUniOperator OP { get; }
        public BoundExpression right { get; }

        public BoundUniExpression(BoundUniOperator op, BoundExpression operand)
        {
            OP = op;
            right = operand;
        }

        // /////////////////////////////////////////////////////////////////////////////////
        public TypeSymbol resultType()
        {
            return OP.ResultType;
        }
    }
}
