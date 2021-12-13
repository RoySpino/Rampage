using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    class BoundExpressionStatement : BoundStatement
    {
        public BoundExpression Expression { get; }
        public override BoundNodeToken tok => BoundNodeToken.BNT_EXPRSTMT;

        public BoundExpressionStatement(BoundExpression expression)
        {
            Expression = expression;
        }
    }
}
