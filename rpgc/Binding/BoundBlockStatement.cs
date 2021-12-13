using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundBlockStatement : BoundStatement
    {
        public ImmutableArray<BoundStatement> Statements { get; }
        public override BoundNodeToken tok => BoundNodeToken.BNT_BLOCKSTMT;

        public BoundBlockStatement(ImmutableArray<BoundStatement> statement)
        {
            Statements = statement;
        }
    }
}
