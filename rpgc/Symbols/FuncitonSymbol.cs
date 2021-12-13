using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using rpgc.Syntax;
using System.Threading.Tasks;

namespace rpgc.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public override SymbolKind kind => SymbolKind.SYM_FUNCTION;
        public ImmutableArray<ParamiterSymbol> Paramiter { get; }
        public TypeSymbol Type { get; }
        public ProcedureDeclarationSyntax Declaration { get; }
        public bool isSubroutine { get; }

        public FunctionSymbol(string name, ImmutableArray<ParamiterSymbol> parm, TypeSymbol retType, ProcedureDeclarationSyntax declare = null, bool isSub = false) : base(name)
        {
            Paramiter = parm;
            Type = retType;
            Declaration = declare;
            isSubroutine = isSub;
        }
    }
}
