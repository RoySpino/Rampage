using rpgc.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Text;

namespace rpgc
{
    internal sealed class RpgRepl : Repl
    {
        Complation prev;

        // /////////////////////////////////////////////////////////////////////////////////////
        protected override void processMetaCommand(string ln)
        {
            switch (ln)
            {
                case "!exit":
                    Environment.Exit(0);
                    break;
                case "!clear":
                    Console.Clear();
                    break;
                case "!reset":
                    prev = null;
                    break;
                case "!pgm":
                    doShowProgramTree = !doShowProgramTree;
                    break;
                case "!tree":
                    doShowTree = !doShowTree;
                    break;
            }
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        protected override void evaluatePgm(string text)
        {
            string prefix, error, sufix, strLine;
            int lineIndex, lineNumber, caracterPos, tmpidx;
            Complation complation;
            EvaluationResult bexpr;
            DiagnosticBag RPGDiagnostics = new DiagnosticBag();
            Text.TextLine Line;
            rpgc.Text.SourceText Text;
            TextSpan spanPrefix, spanError;

            complation = ((prev == null) ? new Complation(stree) : prev.continueWith(stree));
            //bexpr = complation.evalate(variables);

            // show lexed program tree
            if (doShowTree == true)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Black;

                if (stree != null)
                    stree.ROOT.writeTo(Console.Out);
                Console.ResetColor();
            }

            // parce\ tokenize\ bind\ evaluate program
            bexpr = complation.evalate(variables);

            // show flattend program tree
            // this is the program baised on goto's
            if (doShowProgramTree == true)
            {
                complation.emitTree(Console.Out);
                Console.ResetColor();
            }

            // check for any errors
            if (bexpr._Diagnostics.Count() > 0)
            {
                Text = stree.text;

                //foreach (string x in stree.getDiagnostics())
                foreach (Diagnostics err in bexpr._Diagnostics.OrderBy(dign => dign.SPAN.START))
                {
                    //lineIndex = Text.getLineIndex(err.SPAN.LineNo);
                    lineNumber = err.SPAN.LineNo;
                    lineIndex = lineNumber - 1;
                    Line = Text.Lines[lineIndex];
                    strLine = Line.ToString();
                    caracterPos = err.SPAN.START - Text.Lines[lineIndex].Start + 1;


                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format(err.ToString(), lineNumber, caracterPos));
                    Console.ResetColor();

                    


                    spanPrefix = TextSpan.fromBounds(Line.Start, err.SPAN.LinePos);
                    spanError = TextSpan.fromBounds(err.SPAN.LinePos, err.SPAN.LinePos + err.SPAN.LENGTH);

                    /*
                    prefix = stree.text.ToString(spanPrefix); //ln.Substring(0, x.SPAN.START);
                    error = stree.text.ToString(spanError);   //ln.Substring(x.SPAN.START, x.SPAN.LENGTH);
                    sufix = stree.text.ToString(Line.Length);  //ln.Substring(x.SPAN.END);
                    */

                    tmpidx = err.SPAN.LinePos-1 + err.SPAN.LENGTH;

                    if (tmpidx < strLine.Length || (err.SPAN.LinePos - 1 + err.SPAN.LENGTH) < strLine.Length)
                    {
                        prefix = strLine.Substring(0, err.SPAN.LinePos - 1);
                        error = strLine.Substring(err.SPAN.LinePos - 1, err.SPAN.LENGTH);
                        sufix = strLine.Substring(tmpidx);

                        Console.Write("\n" + prefix);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(error);
                        Console.ResetColor();
                        Console.WriteLine(sufix);
                    }
                }

                RPGDiagnostics.Clear();
            }
            else
            {
                // show compiled result if any
                if (bexpr.Value != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine(bexpr.Value);
                    Console.ResetColor();
                }

                 prev = complation;
            }
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        protected override bool isCompleateSubmition(string txt)
        {
            bool hasTwoBlankLines;

            if (String.IsNullOrEmpty(txt))
                return true;
                //return false;

            stree = SyntaxTree.parce(txt);

            hasTwoBlankLines = txt.Split(Environment.NewLine.ToCharArray()).Reverse().TakeWhile(s => string.IsNullOrEmpty(s)).Take(2).Count() == 2;

            if (hasTwoBlankLines == true)
                return true;
            /*
            if (stree.ROOT.Members.Length > 0)
            {
                var tmp = stree.ROOT.Members.Last().GetLastToken();
                if (stree.ROOT.Members.Last().GetLastToken().isMissing())
                    return false;
            }
            */

            var tdiag = stree.getDiagnostics();
            if (tdiag.Any() == true)
                return false;

            return true;
        }
    }
}
