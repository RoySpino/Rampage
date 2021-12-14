using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.IO;
using rpgc.Syntax;
using System.Threading.Tasks;
using rpgc.Binding;
using rpgc.IO;

namespace rpgc
{
    public static class Program
    {
        static void Main(string[] args)
        {
            string path, text;
            Complation _compilation;
            SyntaxTree st;
            EvaluationResult res;

            if (args.Length == 0)
            {
                RpgRepl rpl = new RpgRepl();
                rpl.run();
            }
            else
            {
                if (args.Length > 1)
                    Console.Error.WriteLine("Only one file document is valid now");
                else
                {
                    path = args.Single();
                    text = File.ReadAllText(path);
                    st = SyntaxTree.parce(text);
                    _compilation = new Complation(st);
                    res = _compilation.evalate(new Dictionary<VariableSymbol, object>());
                    TextWriterExtensions.WriteDiagnostics(Console.Out, res._Diagnostics);
                }
            }
        }
    }
}
