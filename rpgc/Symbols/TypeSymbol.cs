using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public override SymbolKind kind => SymbolKind.SYM_TYPE;
        public static readonly TypeSymbol Integer = new TypeSymbol("INT(10)");
        public static readonly TypeSymbol Indicator = new TypeSymbol("IND");
        public static readonly TypeSymbol Char = new TypeSymbol("CHAR");
        public static readonly TypeSymbol varchar = new TypeSymbol("VARCHAR");
        public static readonly TypeSymbol Date = new TypeSymbol("DATE");
        public static readonly TypeSymbol Time = new TypeSymbol("TIME");
        public static readonly TypeSymbol DateTime = new TypeSymbol("DATETIME");
        public static readonly TypeSymbol Float = new TypeSymbol("FLOAT(8)");
        public static readonly TypeSymbol Void = new TypeSymbol("?");
        public static readonly TypeSymbol ERROR = new TypeSymbol("?");

        public TypeSymbol(string name) : base(name)
        {
            //
        }
    }
}
