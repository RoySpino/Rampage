using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using rpgc.IO;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using rpgc.Symbols;

namespace rpgc.Binding
{
    internal static class BoundNodePrinter
    {
        public static void writeTo(this BoundNode node, TextWriter writer)
        {
            if (writer is IndentedTextWriter)
                writeTo(node, (IndentedTextWriter)writer);
            else
                writeTo(node, new IndentedTextWriter(writer));
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void writeTo(this BoundNode node, IndentedTextWriter writer)
        {
            switch (node.tok)
            {
                case BoundNodeToken.BNT_BLOCKSTMT:
                    writeBlockStatement((BoundBlockStatement)node, writer);
                    break;
                case BoundNodeToken.BNT_EXPRSTMT:
                    writeExpressionStatement((BoundExpressionStatement)node, writer);
                    break;
                case BoundNodeToken.BNT_VARDECLR:
                    writeVariableDeclaration((boundVariableDeclaration)node, writer);
                    break;
                case BoundNodeToken.BNT_IFSTMT:
                    writeIfStatement((BoundIfStatement)node, writer);
                    break;
                case BoundNodeToken.BNT_WHILESTMT:
                    writeWhileStatement((BoundWhileStatement)node, writer);
                    break;
                case BoundNodeToken.BNT_FORSTMT:
                    writeForStatement((BoundForStatement)node, writer);
                    break;
                case BoundNodeToken.BNT_DOUNTIL:
                    writeDoUntilStatement((BoundUntilStatement)node, writer);
                    break;
                case BoundNodeToken.BNT_GOTO:
                    writeGoToStatement((BoundGoToStatement)node, writer);
                    break;
                case BoundNodeToken.BNT_GOTOCOND:
                    writeGotoConditinalStatement((BoundGoToConditionalStatement)node, writer);
                    break;
                case BoundNodeToken.BNT_LABEL:
                    writeLableStatement((BoundLabelStatement)node, writer);
                    break;
                case BoundNodeToken.BNT_ERROREXP:
                    writeErrorStatement((BoundErrorExpression)node, writer);
                    break;
                case BoundNodeToken.BNT_CALLEXP:
                    writeCallExpression((BoundCallExpression)node, writer);
                    break;
                case BoundNodeToken.BNT_CONVEXP:
                    writeConversionExpression((BoundConversionExpression)node, writer);
                    break;
                case BoundNodeToken.BNT_ASNEX:
                    writeAssignmentExpression((BoundAssignmentExpression)node, writer);
                    break;
                case BoundNodeToken.BNT_UINEX:
                    writeUniaryExpression((BoundUniExpression)node, writer);
                    break;
                case BoundNodeToken.BNT_BINEX:
                    writeBinaryExpression((BoundBinExpression)node, writer);
                    break;
                case BoundNodeToken.BNT_LITEX:
                    writeBoundLiteralExp((BoundLiteralExp)node, writer);
                    break;
            }
        }

        private static void writeBlockStatement(BoundBlockStatement node, IndentedTextWriter writer)
        {
            writer.writePunctuation("{");
            writer.WriteLine();
            writer.Indent++;

            foreach (var s in node.Statements)
                s.writeTo(writer);

            writer.Indent--;
            writer.writePunctuation("}");
            writer.WriteLine();

        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeNestedStatement(this IndentedTextWriter writer, BoundStatement node)
        {
            bool needsIndent, isBoockStatement;

            isBoockStatement = (node is BoundBlockStatement);
            needsIndent = (isBoockStatement == false);

            if (needsIndent == true)
                writer.Indent++;

            if (isBoockStatement == true)
                node.writeTo(writer);

            if (needsIndent == true)
                writer.Indent--;
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeNestedExpression(this IndentedTextWriter writer, int parentPrecedence, BoundExpression expression)
        {
            BoundUniExpression unary;
            BoundBinExpression binary;

            if (expression is BoundUniExpression)
            {
                unary = (BoundUniExpression)expression;
                writer.writeNestedExpression(parentPrecedence, Syntax.SyntaxFacts.getUinaryOporatorPrecedence(unary.OP.SyntaxKind), unary);
            }
            else if (expression is BoundBinExpression)
            {
                binary = (BoundBinExpression)expression;
                writer.writeNestedExpression(parentPrecedence, Syntax.SyntaxFacts.getBinaryOporatorPrecedence(binary.OP.SyntaxKind), binary);
            }
            else
                expression.writeTo(writer);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeNestedExpression(this IndentedTextWriter writer, int parentPrecedence, int currentPrecedence, BoundExpression expression)
        {
            var needsParenthesis = parentPrecedence >= currentPrecedence;

            if (needsParenthesis)
                writer.writePunctuation(Syntax.TokenKind.TK_PARENOPEN);

            expression.writeTo(writer);

            if (needsParenthesis)
                writer.writePunctuation(Syntax.TokenKind.TK_PARENCLOSE);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeVariableDeclaration(boundVariableDeclaration node, IndentedTextWriter writer)
        {
            writer.writeKeyWord(node.Variable.IsReadOnly == true ? "CONSTANT" : "Variable");
            writer.writeIdentifier(node.Variable.Name);
            writer.writePunctuation(" = ");
            node.Initalizer.writeTo(writer);
            writer.WriteLine();
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeIfStatement(BoundIfStatement node, IndentedTextWriter writer)
        {
            writer.writeKeyWord("IF");
            node.Condition.writeTo(writer);
            writer.WriteLine();
            writer.Indent++;
            writer.writeNestedStatement(node.ThenStatement);
            writer.WriteLine();
            writer.Indent--;
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeWhileStatement(BoundWhileStatement node, IndentedTextWriter writer)
        {
            writer.writeKeyWord(Syntax.TokenKind.TK_DOW);
            writer.writeSpace();
            node.Condition.writeTo(writer);
            writer.WriteLine();
            writer.writeNestedStatement(node.Body);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeDoUntilStatement(BoundUntilStatement node, IndentedTextWriter writer)
        {
            writer.writeKeyWord(Syntax.TokenKind.TK_DOU);
            writer.writeSpace();
            node.Condition.writeTo(writer);
            writer.WriteLine();
            writer.writeNestedStatement(node.Body);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeForStatement(BoundForStatement node, IndentedTextWriter writer)
        {
            writer.writeKeyWord(Syntax.TokenKind.TK_FOR);
            writer.writeSpace();
            writer.writeIdentifier(node.Variable.Name);
            writer.writeSpace();
            writer.writePunctuation(Syntax.TokenKind.TK_ASSIGN);
            writer.writeSpace();
            node.LBound.writeTo(writer);
            writer.writeSpace();
            writer.writeKeyWord(Syntax.TokenKind.TK_TO);
            writer.writeSpace();
            node.Ubound.writeTo(writer);
            writer.WriteLine();
            writer.writeNestedStatement(node.Body);
        }

        private static void writeLableStatement(BoundLabelStatement node, IndentedTextWriter writer)
        {
            bool unindent;

            unindent = writer.Indent > 0;

            if (unindent == true)
                writer.Indent--;

            writer.writePunctuation(node.Label.Name);
            writer.writePunctuation(Syntax.TokenKind.TK_COLON);
            writer.WriteLine();

            if (unindent == true)
                writer.Indent++;
        }

        private static void writeGoToStatement(BoundGoToStatement node, IndentedTextWriter writer)
        {
            writer.writeKeyWord("GoTo");
            writer.writeSpace();
            writer.writeIdentifier(node.Label.Name);
            writer.WriteLine();
        }

        private static void writeGotoConditinalStatement(BoundGoToConditionalStatement node, IndentedTextWriter writer)
        {
            writer.writeKeyWord("GoTo");
            writer.writeSpace();
            writer.writeIdentifier(node.Label.Name);
            writer.writeSpace();
            writer.writeKeyWord(node.JumpIfFalse ? "If" : "Unless");
            writer.writeSpace();
            node.Condition.writeTo(writer);
            writer.WriteLine();
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void writeBinaryExpression(BoundBlockStatement node, IndentedTextWriter writer)
        {
            throw new NotImplementedException();
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /*
        private static void writeReturnStatement(BoundReturnStatement node, IndentedTextWriter writer)
        {
            writer.writeKeyWord(SyntaxKind.ReturnKeyword);
            if (node.Expression != null)
            {
                writer.WriteSpace();
                node.Expression.writeTo(writer);
            }
            writer.WriteLine();
        }
        */

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeExpressionStatement(BoundExpressionStatement node, IndentedTextWriter writer)
        {
            node.Expression.writeTo(writer);
            writer.WriteLine();
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeBoundLiteralExp(BoundLiteralExp node, IndentedTextWriter writer)
        {
            string value;

            if (node.Value != null)
                value = node.Value.ToString();
            else
                value = "";

            if (node.Type == TypeSymbol.Indicator)
                writer.writeKeyWord((bool)node.Value ? Syntax.TokenKind.TK_INDON : Syntax.TokenKind.TK_INDOFF);
            else
                if (node.Type == TypeSymbol.Integer)
                    writer.writeNumber(value);
                else
                    if (node.Type == TypeSymbol.Char)
                    {
                        value = "'" + value.Replace("''", "'") + "'";
                        writer.writeString(value);
                    }
                    else
                        throw new Exception($"Unexpected type {node.Type}");
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeBinaryExpression(BoundBinExpression node, IndentedTextWriter writer)
        {
            int precedence;

            precedence = Syntax.SyntaxFacts.getBinaryOporatorPrecedence(node.OP.SyntaxKind);

            writer.writeNestedExpression(precedence, node.Left);
            writer.writeSpace();
            writer.writePunctuation(node.OP.SyntaxKind);
            writer.writeSpace();
            writer.writeNestedExpression(precedence, node.Right);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeUniaryExpression(BoundUniExpression node, IndentedTextWriter writer)
        {
            int precedence;

            precedence = Syntax.SyntaxFacts.getUinaryOporatorPrecedence(node.OP.SyntaxKind);

            writer.writePunctuation(node.OP.SyntaxKind);
            writer.writeNestedExpression(precedence, node.right);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeAssignmentExpression(BoundAssignmentExpression node, IndentedTextWriter writer)
        {
            writer.writeIdentifier(node.Variable.Name);
            writer.writeSpace();
            writer.writePunctuation(Syntax.TokenKind.TK_ASSIGN);
            writer.writeSpace();
            node.Expression.writeTo(writer);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeConversionExpression(BoundConversionExpression node, IndentedTextWriter writer)
        {
            writer.writeIdentifier(node.Type.Name);
            writer.writePunctuation(Syntax.TokenKind.TK_PARENOPEN);
            node._Expression.writeTo(writer);
            writer.writePunctuation(Syntax.TokenKind.TK_PARENCLOSE);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeCallExpression(BoundCallExpression node, IndentedTextWriter writer)
        {
            bool isFirst;

            writer.writeIdentifier(node.Function.Name);
            writer.writePunctuation(Syntax.TokenKind.TK_PARENOPEN);

            isFirst = true;
            foreach (var argument in node.Arguments)
            {
                if (isFirst == true)
                {
                    isFirst = false;
                }
                else
                {
                    writer.writePunctuation(Syntax.TokenKind.TK_COLON);
                    writer.writeSpace();
                }

                argument.writeTo(writer);
            }

            writer.writePunctuation(Syntax.TokenKind.TK_PARENCLOSE);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void writeErrorStatement(BoundErrorExpression node, IndentedTextWriter writer)
        {
            writer.writeKeyWord("?");
        }
    }
}
