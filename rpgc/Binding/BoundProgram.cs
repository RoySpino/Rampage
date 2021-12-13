using rpgc.Symbols;
using System.Collections.Immutable;

namespace rpgc.Binding
{
    internal sealed class BoundProgram
    {
        public BoundGlobalScope GblScope { get; }
        public ImmutableArray<Diagnostics> Diagnostics { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> FunctionBodies { get; }


        public BoundProgram Previous { get; }
        public FunctionSymbol MainFunction { get; }
        public FunctionSymbol ScriptFunction { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }

        public BoundProgram(BoundGlobalScope _gblScope, 
                            ImmutableArray<Diagnostics> _diagnostics, 
                            ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functionBodies)
        {
            GblScope = _gblScope;
            Diagnostics = _diagnostics;
            FunctionBodies = _functionBodies;
        }

        // //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public BoundProgram(BoundProgram previous,
                            ImmutableArray<Diagnostics> diagnostics,
                            FunctionSymbol mainFunction,
                            FunctionSymbol scriptFunction,
                            ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            MainFunction = mainFunction;
            ScriptFunction = scriptFunction;
            Functions = functions;
        }
    }
}