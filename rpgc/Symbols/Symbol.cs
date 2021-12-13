using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace rpgc.Symbols
{
    public abstract class Symbol
    {
        public string Name { get; set;  }
        public abstract SymbolKind kind { get; }

        internal Symbol(string _name)
        {
            Name = _name;
        }

        // ///////////////////////////////////////////////////////////////////////
        public override string ToString()
        {
            //return Name;
            using (StringWriter wrtr = new StringWriter())
            {
                writeTo(wrtr);
                return wrtr.ToString();
            }
        }

        // ///////////////////////////////////////////////////////////////////////
        public void writeTo(TextWriter writer)
        {
            SymbolPrinter.writeTo(this, writer);
        }
    }

    // ////////////////////////////////////////////////////////////////////////////
    // /////     /////     /////     /////     /////     /////     /////     /////
    // //////////////////////////////////////////////////////////////////////////
    public enum SymbolKind
    {
        SYM_VARIABLE,
        SYM_FUNCTION,
        SYM_PROCEDURE,
        SYM_SUBRUTINE,
        SYM_TYPE,
        SYM_PARAMITER,
        SYM_LOCALVAR,
        SYM_GLOBALVAR
    }
}
