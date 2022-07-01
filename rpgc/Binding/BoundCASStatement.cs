using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    class BoundCASStatement : BoundStatement
    {
        public override BoundNodeToken tok => BoundNodeToken.BNT_CAS;
        public ImmutableArray<BoundExpression> BoundExpressions { get; }
        public ImmutableArray<BoundStatement> BoundStatements { get; }

        public BoundCASStatement(ImmutableArray<BoundExpression> boundExpressions, ImmutableArray<BoundStatement> boundStatements)
        {
            BoundExpressions = boundExpressions;
            BoundStatements = boundStatements;
        }

    }
}
