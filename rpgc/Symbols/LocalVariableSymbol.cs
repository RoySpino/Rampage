using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Symbols
{
    public class LocalVariableSymbol : VariableSymbol
    {
        public override SymbolKind kind => SymbolKind.SYM_LOCALVAR;

        public LocalVariableSymbol(string name, bool isReadOnly, TypeSymbol t) : base(name, isReadOnly, t)
        {
        }
    }
}
