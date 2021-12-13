using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Symbols
{
    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        public override SymbolKind kind => SymbolKind.SYM_GLOBALVAR;

        public GlobalVariableSymbol(string name, bool isReadOnly, TypeSymbol t) : base(name, isReadOnly, t)
        {
        }
    }
}
