using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Symbols
{
    public sealed class ParamiterSymbol : LocalVariableSymbol
    {
        public override SymbolKind kind => SymbolKind.SYM_PARAMITER;

        public TypeSymbol Type { get; }

        public ParamiterSymbol(string name, TypeSymbol type) : base(name, true, type)
        {
            Type = type;
        }

        // //////////////////////////////////////////////////////////////////////////////////
        public TypeSymbol getParamiterType()
        {
            return this.Type;
        }
    }
}
