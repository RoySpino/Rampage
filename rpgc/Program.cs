using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Binding;

namespace rpgc
{
    class Program
    {
        static void Main(string[] args)
        {
            string ln, prefix, error, sufix;
            bool doShowTree = true;
            SyntaxTree stree;
            Evaluator eval;
            Complation complation;
            EvaluationResult bexpr;
            DiagnosticBag RPGDiagnostics = new DiagnosticBag();
            Dictionary<VariableSymbol, object> variables = new Dictionary<VariableSymbol, object>();

            while (true)
            {
                Console.Write(">>> ");
                ln = Console.ReadLine();

                if (ln == "exit")
                    break;

                //lex = new Lexer(ln);
                //paar = new Parser(ln);
                stree = SyntaxTree.parce(ln);
                complation = new Complation(stree);
                bexpr = complation.evalate(variables);
                RPGDiagnostics.AddRange(bexpr.Diagnostics);
                //RPGDiagnostics = stree.getDiagnostics();
                //Diagnostics.AddRange(Diagnostics);

                if (doShowTree == true)
                {
                    Console.BackgroundColor = ConsoleColor.Green;
                    Console.ForegroundColor = ConsoleColor.Black;

                    if (stree != null)
                        printTree(stree.ROOT);
                    Console.ResetColor();
                }

                if (RPGDiagnostics.Count() > 0)
                {
                    //foreach (string x in stree.getDiagnostics())
                    foreach (Diagnostics x in RPGDiagnostics)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(x.ToString());
                        Console.ResetColor();

                        for (int i = 0; i < ln.Length; i++)
                        {
                            if (!(i >= x.SPAN.START) && (i <= x.SPAN.END) || !(i >= x.SPAN.START) && (i <= x.SPAN.END))
                                Console.ForegroundColor = ConsoleColor.Red;
                            if (i <= x.SPAN.END)
                                Console.ResetColor();
                        }


                        prefix = ln.Substring(0, x.SPAN.START);
                        error = ln.Substring(x.SPAN.START, x.SPAN.LENGTH);
                        sufix = ln.Substring(x.SPAN.END);

                        Console.Write("     " + prefix);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(error);
                        Console.ResetColor();
                        Console.WriteLine(sufix);
                        
                    }

                    RPGDiagnostics.Clear();
                }
                else
                {
                    //eval = new Evaluator(bexpr);
                    //Console.WriteLine(eval.Evaluate());
                    Console.WriteLine(bexpr.Value);
                }
            }
        }

        // ///////////////////////////////////////////////////////////////////
        static void printTree(SyntaxNode node, string indent = "", bool isLast = true)
        {
            if (node == null)
                return;

            string marker = ((isLast == true) ? "└──" : "├──");
            Console.Write(indent + marker + node.kind);

            if (node is SyntaxToken)
            {
                SyntaxToken t = (SyntaxToken)node;
                Console.Write(" " + t.sym);
            }

            Console.WriteLine("");

            var lastChild = node.getCildren().LastOrDefault();

            indent += ((isLast == true) ? "   " : "│  ");
            foreach (SyntaxNode child in node.getCildren())
                printTree(child, indent, child == lastChild);

        }
    }
}
