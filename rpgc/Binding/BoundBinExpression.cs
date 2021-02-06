using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundBinExpression : BoundExpression
    {
        public BoundBinOperator OP;
        public BoundExpression Left, Right;
        // "type" is in BoundExpression

        public BoundBinExpression(BoundExpression left, BoundBinOperator op, BoundExpression right)
        {
            OP = op;
            Left = left;
            Right = right;

            type = op.ResultType;
            tok = BoundNodeToken.BNT_UINEX;
        }
        
        // /////////////////////////////////////////////////////////////////////////////////
        public Type resultType()
        {
            return OP.ResultType;
        }
    }
}
