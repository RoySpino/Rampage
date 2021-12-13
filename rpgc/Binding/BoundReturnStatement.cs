using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal class BoundReturnStatement : BoundStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_RETSTMT;
        
        public BoundExpression Expression { get; }

        public BoundReturnStatement(BoundExpression exp)
        {
            Expression = exp;
        }
    }
}
