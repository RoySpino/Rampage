using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc
{
    public sealed class VariableSymbol
    {
        public string Name;
        public Type type;

        internal VariableSymbol(string name, Type t)
        {
            Name = name;
            type = t;
        }
    }
}
