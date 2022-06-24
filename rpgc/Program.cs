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

            if (args.Length == 0)
            {
                /*
                RpgRepl rpl = new RpgRepl();
                rpl.run();
                */

                /*
                */
                //string[] ar = { @"D:\Documents\CodeSnipets\RPG\RampageTest\test4.rpg" };
                //string[] ar = { @"D:\Documents\CodeSnipets\RPG\RampageTest\RPMult2.rpg" };
                string[] ar = { @"G:\Documents\codeSnipets\RPG\RampageTest\YIN_FREE.rpg" };
                doCompile(ar);
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

                    _compilation = Complation.Create(st);
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
            string text, fnam, prv, tmp;
            Complation _compilation;
            EvaluationResult res;
            List<SyntaxTree> sTrees;
            List<string> lpths, warnDocs;

            sTrees = new List<SyntaxTree>();
            warnDocs = new List<string>();
            lpths = new List<string>(paths);
            lpths.Sort();
            prv = "";

            foreach (string path in lpths)
            {
                if (prv == path)
                {
                    fnam = $"warning: Source file `{Path.GetFileName(path)}' specified multiple times";
                    tmp = (from wd in warnDocs
                           where wd == fnam
                           select wd).FirstOrDefault();
                    if (tmp == null)
                        warnDocs.Add(fnam);

                    continue;
                }

                prv = path;
                

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

            // display warning message that user enterd multiple documents
            if (warnDocs.Any() == true)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                foreach(string itm in warnDocs)
                    Console.WriteLine(itm);
                Console.ResetColor();
            }

            _compilation =  Complation.Create(sTrees.ToArray());
            res = _compilation.evalate(new Dictionary<VariableSymbol, object>());

            // display diagnostics if any
            if (res._Diagnostics.Any() == true)
                TextWriterExtensions.WriteDiagnostics(Console.Error, res._Diagnostics);
        }
    }
}
