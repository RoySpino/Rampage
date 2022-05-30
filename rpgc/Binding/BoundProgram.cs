using rpgc.Symbols;
using System.Collections.Immutable;

namespace rpgc.Binding
{
    internal sealed class BoundProgram
    {
        public BoundGlobalScope GblScope { get; }
        public ImmutableArray<Diagnostics> Diagnostics { get; }


        public BoundProgram Previous { get; }
        public FunctionSymbol MainFunction { get; }
        public FunctionSymbol ScriptFunction { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }



        public BoundProgram(BoundProgram previous,
                            BoundGlobalScope _gblScope,
                            ImmutableArray<Diagnostics> _diagnostics,
                            FunctionSymbol mainFunction,
                            FunctionSymbol scriptFunction,
                            ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions)
        {
            Previous = previous;
            GblScope = _gblScope;
            Diagnostics = _diagnostics;
            MainFunction = mainFunction;
            ScriptFunction = scriptFunction;
            Functions = functions;
        }
    }
}