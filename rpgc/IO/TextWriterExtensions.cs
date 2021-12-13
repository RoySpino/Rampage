using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.IO
{
    internal static class TextWriterExtensions
    {
        public static bool isConsolOut(this TextWriter writer)
        {
            if (writer == Console.Out)
                return true;

            if (writer == Console.Error)
                return (Console.IsErrorRedirected == false) && (Console.IsOutputRedirected == false);

            if (writer is IndentedTextWriter)
            {
                if (((IndentedTextWriter)writer).InnerWriter.isConsolOut() == true)
                    return true;
            }

            return false;
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void setForeground(this TextWriter writer, ConsoleColor colr)
        {
            if (writer.isConsolOut() == true)
                Console.ForegroundColor = colr;
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void resetColor(this TextWriter writer)
        {
            if (writer.isConsolOut() == true)
                Console.ResetColor();
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void writeKeyWord(this TextWriter writer, Syntax.TokenKind kind)
        {
            string text;

            text = DiagnosticBag.enumToSym(kind);
            writer.setForeground(ConsoleColor.Blue);
            writer.Write(text);
            writer.resetColor();
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void writeKeyWord(this TextWriter writer, string text)
        {
            writer.setForeground(ConsoleColor.Blue);
            writer.Write(text);
            writer.resetColor();
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void writeIdentifier(this TextWriter writer, string text)
        {
            writer.setForeground(ConsoleColor.Cyan);
            writer.Write(text);
            writer.resetColor();
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void writeNumber(this TextWriter writer, string text)
        {
            writer.setForeground(ConsoleColor.Green);
            writer.Write(text);
            writer.resetColor();
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void writeString(this TextWriter writer, string text)
        {
            writer.setForeground(ConsoleColor.DarkYellow);
            writer.Write(text);
            writer.resetColor();
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void writeSpace(this TextWriter writer)
        {
            writer.writePunctuation(" ");
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void writePunctuation(this TextWriter writer, Syntax.TokenKind kind)
        {
            string text;

            text = DiagnosticBag.enumToSym(kind);
            Debug.Assert(text != null);
            writer.writePunctuation(text);
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void writePunctuation(this TextWriter writer, string text)
        {
            writer.setForeground(ConsoleColor.Gray);
            writer.Write(text);
            writer.resetColor();
        }

        // /////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostics> diagnostics)
        {
            ConsoleColor messageColor;


            Console.ResetColor();
            foreach (Diagnostics _diagnostic in diagnostics)
            {
                messageColor = _diagnostic.IsWarning ? ConsoleColor.DarkYellow : ConsoleColor.DarkRed;
                writer.setForeground(messageColor);
                writer.WriteLine(_diagnostic.ToString());
                Console.ResetColor();
            }
        }
    }
}
