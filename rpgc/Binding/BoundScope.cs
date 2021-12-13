using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using rpgc.Symbols;

namespace rpgc.Binding
{
    internal sealed class BoundScope
    {
        public Dictionary<string, VariableSymbol> variables = null;
        public Dictionary<string, FunctionSymbol> functions = null;
        public BoundScope Parant { get; }

        public BoundScope(BoundScope parant)
        {
            Parant = parant;
            
            if (parant != null)
                variables = new Dictionary<string, VariableSymbol>(parant.variables);
        }

        // ///////////////////////////////////////////////////////////////////////////
        public bool declare(VariableSymbol var)
        {
            if (variables == null)
                variables = new Dictionary<string, VariableSymbol>();

            if (variables.ContainsKey(var.Name) == true)
                return false;

            variables.Add(var.Name, var);
            return true;
        }

        // ///////////////////////////////////////////////////////////////////////////
        public bool lookup(string name, out VariableSymbol var)
        {
            if (variables == null)
                variables = new Dictionary<string, VariableSymbol>();

            // try to find the variale within this scope
            if (variables.TryGetValue(name, out var) == true)
                return true;

            // failed to find the variable check if there is a parant
            if (Parant == null)
                return false;

            // return parants variable
            return Parant.lookup(name, out var);
            /*
            foreach(string vr in variables.Keys)
                if (name == vr)
                    return true;

            return false;
            */
        }

        // ///////////////////////////////////////////////////////////////////////////
        public bool checkLocalVariables(string name)
        {
            VariableSymbol dmy;

            // dont do no variables declared
            if (variables == null)
                return false;

            // try to find the variale within this scope
            if (variables.TryGetValue(name, out dmy) == true)
                return true;


            return false;
        }




        // ////////////////////////////////////////////////////////////////////////////////////////////////////
        // ////////////////////////////////////////////////////////////////////////////////////////////////////
        // ///////////////////////////////////////////////////////////////////////////
        public bool declareFunction(FunctionSymbol funct)
        {
            if (functions == null)
                functions = new Dictionary<string, FunctionSymbol>();

            if (functions.ContainsKey(funct.Name) == true)
                return false;

            functions.Add(funct.Name, funct);
            return true;
        }

        // ///////////////////////////////////////////////////////////////////////////
        public bool lookupFuncion(string name, out FunctionSymbol var)
        {
            if (functions == null)
                functions = new Dictionary<string, FunctionSymbol>();

            // try to find the variale within this scope
            if (functions.TryGetValue(name, out var) == true)
                return true;

            // failed to find the variable check if there is a parant
            if (Parant == null)
                return false;

            // return parants variable
            return Parant.lookupFuncion(name, out var);
            /*
            foreach (string vr in functions.Keys)
                if (name == vr)
                    return true;

            return false;
            */
        }

        // ///////////////////////////////////////////////////////////////////////////
        public bool checkLocalFunctions(string name)
        {
            //FunctionSymbol dmy;
            // if (functions.TryGetValue(name, out dmy) == true)
            string xtn;

            // dont do no functions declared
            if (functions == null)
                return false;

            // try to find the function within this scope
            xtn = functions.Where(fn => fn.Key == name).FirstOrDefault().Key;

            // not found in current scope check parant
            if (xtn == null)
            {
                if (Parant != null)
                    Parant.checkLocalFunctions(name);
                else
                    return false;
            }

            // return true when found
            return true;
        }




        // ///////////////////////////////////////////////////////////////////////////
        public ImmutableArray<VariableSymbol> getDeclaredVariables()
        {
            if (variables == null)
                return ImmutableArray<VariableSymbol>.Empty;

            return variables.Values.ToImmutableArray();
        }

        // ///////////////////////////////////////////////////////////////////////////
        public ImmutableArray<FunctionSymbol> getDeclaredFunctions()
        {
            if (functions == null)
                return ImmutableArray<FunctionSymbol>.Empty;

            return functions.Values.ToImmutableArray();
        }
    }
}
