using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    class BoundCABStatement : BoundStatement
    {
        public ImmutableArray<BoundExpression> BoundExpressions { get; }
        public ImmutableArray<BoundStatement> BoundStatements { get; }
        public override BoundNodeToken tok => BoundNodeToken.BNT_CAB;

        public BoundCABStatement(ImmutableArray<BoundExpression> boundExpressions, ImmutableArray<BoundStatement> boundStatements)
        {
            BoundExpressions = boundExpressions;
            BoundStatements = boundStatements;
        }
    }
}
