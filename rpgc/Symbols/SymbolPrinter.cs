using System;
using System.Collections.Generic;
using rpgc.IO;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace rpgc.Symbols
{
    internal static class SymbolPrinter
    {
        public static void writeTo(Symbol sym, TextWriter writer)
        {
            switch(sym.kind)
            {
                case SymbolKind.SYM_FUNCTION:
                    writeFunction((FunctionSymbol)sym, writer);
                    break;
                case SymbolKind.SYM_GLOBALVAR:
                    writeGlobalVariableSymbol((GlobalVariableSymbol)sym, writer);
                    break;
                case SymbolKind.SYM_LOCALVAR:
                    writeLocalVariableSymbol((LocalVariableSymbol)sym, writer);
                    break;
                case SymbolKind.SYM_PARAMITER:
                    writeParamiterSymbol((ParamiterSymbol)sym, writer);
                    break;
                case SymbolKind.SYM_TYPE:
                    writeTypeSymbol((TypeSymbol)sym, writer);
                    break;
                default:
                    throw new Exception($"Unexpected symbol: {sym.kind}");
            }
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private static void writeFunction(FunctionSymbol sym, TextWriter writer)
        {
            writer.writeKeyWord("funciton ");
            writer.writeIdentifier(sym.Name);
            writer.writePunctuation("(");

            // print all paramiters from the paramiter list
            for (int i = 0; i < sym.Paramiter.Length; i++)
            {
                if (i > 0)
                {
                    writer.writePunctuation(Syntax.TokenKind.TK_COLON);
                    writer.writeSpace();
                }

                sym.Paramiter[i].writeTo(writer);
            }

            writer.writePunctuation(")");
            writer.WriteLine();
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private static void writeGlobalVariableSymbol(GlobalVariableSymbol sym, TextWriter writer)
        {
            writer.writeKeyWord(sym.IsReadOnly ? Syntax.TokenKind.TK_VARDECLR : Syntax.TokenKind.TK_VARDCONST);
            writer.writeSpace();
            writer.writeIdentifier(sym.Name);
            writer.writeSpace();
            writer.writeKeyWord(sym.type.Name);
            sym.type.writeTo(writer);
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private static void writeLocalVariableSymbol(LocalVariableSymbol sym, TextWriter writer)
        {
            writer.writeKeyWord(sym.IsReadOnly ? Syntax.TokenKind.TK_VARDECLR : Syntax.TokenKind.TK_VARDCONST);
            writer.writeSpace();
            writer.writeIdentifier(sym.Name);
            writer.writeSpace();
            writer.writeKeyWord(sym.type.Name);
            sym.type.writeTo(writer);
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private static void writeParamiterSymbol(ParamiterSymbol sym, TextWriter writer)
        {
            writer.writeIdentifier(sym.Name);
            writer.writePunctuation(Syntax.TokenKind.TK_COLON);
            writer.writeSpace();
            sym.Type.writeTo(writer);
        }

        // /////////////////////////////////////////////////////////////////////////////////////
        private static void writeTypeSymbol(TypeSymbol sym, TextWriter writer)
        {
            writer.writeIdentifier(sym.Name);
        }
    }
}
