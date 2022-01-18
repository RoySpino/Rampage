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
    internal static class Program
    {
        public static void Main(string[] args)
        {
            string paths, text, fnam;
            Complation _compilation;
            SyntaxTree st;
            EvaluationResult res;
            List<SyntaxTree> trees;

            if (args.Length == 0)
            {
                RpgRepl rpl = new RpgRepl();
                rpl.run();
            }
            else
            {
                if (args.Length >= 1)
                {
                    doCompile(args);
                }
                else
                {
                    paths = args.Single();
                    text = File.ReadAllText(paths);
                    fnam = Path.GetFileName(paths);
                    st = SyntaxTree.Parse(text, fnam);
                    
                    //st.ROOT.writeTo(Console.Out);

                    _compilation = new Complation(st);
                    res = _compilation.evalate(new Dictionary<VariableSymbol, object>());

                    // display diagnostics if any
                    if (res._Diagnostics.Any() == true)
                        TextWriterExtensions.WriteDiagnostics(Console.Error, res._Diagnostics);
                }
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////
        private static void doCompile(string[] paths)
        {
            string text, fnam;
            Complation _compilation;
            SyntaxTree st;
            EvaluationResult res;
            List<SyntaxTree> sTrees;

            sTrees = new List<SyntaxTree>();

            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    text = File.ReadAllText(path);
                    fnam = Path.GetFileName(path);
                    sTrees.Add(SyntaxTree.Parse(text, fnam));
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"error: Source file `{Path.GetFileName(path)}' could not be found");
                    Console.ResetColor();
                    Console.WriteLine("compilation terminated.");

                    sTrees.Clear();
                    sTrees = null;
                    return;
                }
            }

            _compilation = new Complation(sTrees.ToArray());
            res = _compilation.evalate(new Dictionary<VariableSymbol, object>());

            // display diagnostics if any
            if (res._Diagnostics.Any() == true)
                TextWriterExtensions.WriteDiagnostics(Console.Error, res._Diagnostics);
        }
    }
}
