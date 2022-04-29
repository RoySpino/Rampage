using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace rpgc.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol dsply = new FunctionSymbol("DSPLY", ImmutableArray.Create(new ParamiterSymbol("A",TypeSymbol.Char)), TypeSymbol.Void);
        public static readonly FunctionSymbol cout = new FunctionSymbol("COUT", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Char)), TypeSymbol.Void);
        public static readonly FunctionSymbol cin = new FunctionSymbol("CIN", ImmutableArray<ParamiterSymbol>.Empty, TypeSymbol.Char);
        public static readonly FunctionSymbol BIF_Int = new FunctionSymbol("%INT", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Char)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_Log = new FunctionSymbol("%LOG", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Integer)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_Log10 = new FunctionSymbol("%LOG10", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Integer)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_Rem = new FunctionSymbol("%REM", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Integer), new ParamiterSymbol("B", TypeSymbol.Integer)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_char = new FunctionSymbol("%CHAR", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Integer)), TypeSymbol.Char);
        public static readonly FunctionSymbol BIF_abs = new FunctionSymbol("%ABS", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Integer)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_Lower = new FunctionSymbol("%LOWER", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Char)), TypeSymbol.Char);
        public static readonly FunctionSymbol BIF_Upper = new FunctionSymbol("%UPPER", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Char)), TypeSymbol.Char);
        public static readonly FunctionSymbol BIF_Len = new FunctionSymbol("%LEN", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Char)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_Sqrt = new FunctionSymbol("%SQRT", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Integer)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_Rand = new FunctionSymbol("%RAND", ImmutableArray.Create(new ParamiterSymbol("Max", TypeSymbol.Integer)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_Subst = new FunctionSymbol("%SUBST", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Char), new ParamiterSymbol("B", TypeSymbol.Integer), new ParamiterSymbol("C", TypeSymbol.Integer)), TypeSymbol.Char);
        public static readonly FunctionSymbol BIF_div = new FunctionSymbol("%DIV", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Integer),new ParamiterSymbol("B", TypeSymbol.Integer)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_Scan = new FunctionSymbol("%SCAN", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Char), new ParamiterSymbol("B", TypeSymbol.Char)), TypeSymbol.Integer);
        public static readonly FunctionSymbol BIF_Replace = new FunctionSymbol("%REPLACE", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Char), new ParamiterSymbol("B", TypeSymbol.Char), new ParamiterSymbol("C", TypeSymbol.Integer), new ParamiterSymbol("D", TypeSymbol.Integer)), TypeSymbol.Char);
        public static readonly FunctionSymbol BIF_Trim = new FunctionSymbol("%TRIM", ImmutableArray.Create(new ParamiterSymbol("A", TypeSymbol.Char)), TypeSymbol.Char);

        internal static IEnumerable<FunctionSymbol> getAll()
        {
            List<FunctionSymbol> lst = new List<FunctionSymbol>();

            // get all properties in this class that are public or static and 
            // generate a list of funciton symbols
            return (from ls in typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                   where ls.FieldType == typeof(FunctionSymbol)
                   select (FunctionSymbol)ls.GetValue(null)).ToArray();

            /*
            foreach (var x in typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static))
                if (x.FieldType == typeof(FunctionSymbol))
                    lst.Add((FunctionSymbol)x.GetValue(null));

            return lst.ToArray();
            */
        }
    }
}
