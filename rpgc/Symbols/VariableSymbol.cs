using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Symbols;

namespace rpgc
{
    public abstract class VariableSymbol : Symbol
    {
        public TypeSymbol type { get; }
        public bool IsReadOnly { get; }

        public VariableSymbol(string name, bool isReadOnly, TypeSymbol t) : base (name)
        {
            type = t;
            IsReadOnly = isReadOnly;
        }

        // ////////////////////////////////////////////////////////////////////////////
        public TypeSymbol getType()
        {
            return type;
        }
    }
}
