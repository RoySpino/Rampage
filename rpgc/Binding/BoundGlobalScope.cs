using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Symbols;
using rpgc.Syntax;

namespace rpgc.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope Preveous { get; }
        public ImmutableArray<Diagnostics> Diagnostic { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<FunctionSymbol> Functons { get; }
        public BoundStatement Statement { get; set; }

        public BoundGlobalScope(BoundGlobalScope prev, ImmutableArray<Diagnostics> diag, ImmutableArray<FunctionSymbol> functon, ImmutableArray<VariableSymbol> vars, BoundStatement stmnt)
        {
            Preveous = prev;
            Diagnostic = diag;
            Variables = vars;
            Statement = stmnt;
            Functons = functon;
        }
    }
}
