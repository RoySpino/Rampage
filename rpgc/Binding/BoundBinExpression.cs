using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Symbols;

namespace rpgc.Binding
{
    internal sealed class BoundBinExpression : BoundExpression
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_BINEX;
        public override TypeSymbol Type => OP.ResultType;
        public BoundExpression Left { get; }
        public BoundBinOperator OP { get; }
        public BoundExpression Right { get; }
        // "type" is in BoundExpression

        public BoundBinExpression(BoundExpression left, BoundBinOperator op, BoundExpression right)
        {
            OP = op;
            Left = left;
            Right = right;
        }
        
        // /////////////////////////////////////////////////////////////////////////////////
        public TypeSymbol resultType()
        {
            return OP.ResultType;
        }
    }
}
