using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal sealed class BoundLabel
    {
        public string Name { get; }
        public override string ToString() => Name;

        internal BoundLabel(string name)
        {
            Name = name;
        }
    }
}
