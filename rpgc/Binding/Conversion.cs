using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Symbols;

namespace rpgc.Binding
{
    internal sealed class Conversion
    {
        public static readonly Conversion NONE = new Conversion(exits: false, isIdentity: false, isImplicit: false);
        public static readonly Conversion IDENTITY = new Conversion(exits: true, isIdentity: true, isImplicit: true);
        public static readonly Conversion IMPLICIT = new Conversion(exits: true, isIdentity: false, isImplicit: true);
        public static readonly Conversion EXPLICIT = new Conversion(exits: true, isIdentity: false, isImplicit: false);

        public bool Exits { get; }
        public bool Identity { get; }
        public bool Implicit { get; }
        public bool Explicit => (Exits == true && Implicit == false);

        private Conversion(bool exits, bool isIdentity, bool isImplicit)
        {
            Exits = exits;
            Identity = isIdentity;
            Implicit = isImplicit;
        }

        // //////////////////////////////////////////////////////////////////////////////////////
        public static Conversion Clasifiyer(TypeSymbol from, TypeSymbol to)
        {
            // both types are the same
            if (from == to)
                return Conversion.IDENTITY;

            // to Any
            if (from != TypeSymbol.Void && to == TypeSymbol.Any)
            {
                return Conversion.IMPLICIT;
            }

            if (from == TypeSymbol.Any && to != TypeSymbol.Void)
            {
                return Conversion.EXPLICIT;
            }

            // to string
            if (from == TypeSymbol.Indicator || from == TypeSymbol.Integer || from == TypeSymbol.Date || from == TypeSymbol.DateTime || from == TypeSymbol.Float)
            {
                if (to == TypeSymbol.Char)
                    return Conversion.EXPLICIT;

            }

            // char to integer boolean float date time timestamp
            if (from == TypeSymbol.Char)
            {
                if (to == TypeSymbol.Indicator || to == TypeSymbol.Integer || to == TypeSymbol.Date || to == TypeSymbol.DateTime || to == TypeSymbol.Float || to == TypeSymbol.Time)
                    return Conversion.EXPLICIT;

            }

            // integer to float
            if (from == TypeSymbol.Integer || from  == TypeSymbol.Float)
            {
                if (to == TypeSymbol.Float || to == TypeSymbol.Integer)
                    return Conversion.EXPLICIT;

            }

            return Conversion.NONE;
        }

        // //////////////////////////////////////////////////////////////////////////////////////
        public static Conversion assignmentConvertion(TypeSymbol from, TypeSymbol to)
        {
            // both types are the same
            if (from == to)
                return Conversion.IDENTITY;

            // string to ind
            if (from == TypeSymbol.Indicator)
            {
                if (to == TypeSymbol.Char)
                    return Conversion.EXPLICIT;

            }

            // integer to float
            if (from == TypeSymbol.Integer || from == TypeSymbol.Float)
            {
                if (to == TypeSymbol.Float || to == TypeSymbol.Integer)
                    return Conversion.EXPLICIT;

            }

            // daete conversion
            if (from == TypeSymbol.Date || from == TypeSymbol.DateTime)
            {
                if (to == TypeSymbol.DateTime || to == TypeSymbol.Date)
                    return Conversion.EXPLICIT;

            }

            return Conversion.NONE;
        }
    }
}
