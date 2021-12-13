using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Symbols;
using rpgc.Syntax;

namespace rpgc.Binding
{
    internal static class BoundNodeFactory
    {
        /*
        public static BoundBlockStatement Block(SyntaxNode syntax, params BoundStatement[] statements)
        {
            
            return new BoundBlockStatement(syntax, ImmutableArray.Create(statements));
        }

        public static boundVariableDeclaration VariableDeclaration(SyntaxNode syntax, VariableSymbol symbol, BoundExpression initializer)
        {
            return new boundVariableDeclaration(syntax, symbol, initializer);
        }

        public static boundVariableDeclaration VariableDeclaration(SyntaxNode syntax, string name, BoundExpression initializer)
            => VariableDeclarationInternal(syntax, name, initializer, isReadOnly: false);

        public static boundVariableDeclaration ConstantDeclaration(SyntaxNode syntax, string name, BoundExpression initializer)
            => VariableDeclarationInternal(syntax, name, initializer, isReadOnly: true);

        private static boundVariableDeclaration VariableDeclarationInternal(SyntaxNode syntax, string name, BoundExpression initializer, bool isReadOnly)
        {
            LocalVariableSymbol local;
            local = new LocalVariableSymbol(name, isReadOnly, initializer.Type, initializer.ConstantValue);
            return new boundVariableDeclaration(syntax, local, initializer);
        }

        public static BoundWhileStatement While(SyntaxNode syntax, BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
        {
            //return new BoundWhileStatement(syntax, condition, body, breakLabel, continueLabel);
            return new BoundWhileStatement(condition, body, new LabelSymbol(breakLabel.Name), new LabelSymbol(continueLabel.Name));
        }

        public static BoundGoToStatement Goto(SyntaxNode syntax, BoundLabel label)
        {
            //return new BoundGoToStatement(syntax, label);
            return new BoundGoToStatement(new LabelSymbol(label.Name));
        }

        public static BoundGoToConditionalStatement GotoTrue(SyntaxNode syntax, BoundLabel label, BoundExpression condition)
            => new BoundGoToConditionalStatement(syntax, label, condition, jumpIfFalse: true);

        public static BoundGoToConditionalStatement GotoFalse(SyntaxNode syntax, BoundLabel label, BoundExpression condition)
            => new BoundGoToConditionalStatement(syntax, label, condition, jumpIfFalse: false);

        public static BoundLabelStatement Label(SyntaxNode syntax, BoundLabel label)
        {
            //return new BoundLabelStatement(syntax, label);
            return new BoundLabelStatement(new LabelSymbol(label.Name));
        }

        public static BoundNopStatement Nop(SyntaxNode syntax)
        {
            return new BoundNopStatement(syntax);
        }

        public static BoundAssignmentExpression Assignment(SyntaxNode syntax, VariableSymbol variable, BoundExpression expression)
        {
            return new BoundAssignmentExpression(syntax, variable, expression);
        }

        public static BoundBinExpression Binary(SyntaxNode syntax, BoundExpression left, SyntaxToken kind, BoundExpression right)
        {
            var op = BoundBinExpression.Bind(kind, left.Type, right.Type)!;
            //return Binary(syntax, left, op, right);
            return Binary(left, op, right);
        }

        public static BoundBinExpression Binary(SyntaxNode syntax, BoundExpression left, BoundBinOperator op, BoundExpression right)
        {
            //return new BoundBinExpression(syntax, left, op, right);
            return new BoundBinExpression(left, op, right);
        }

        public static BoundBinExpression Add(SyntaxNode syntax, BoundExpression left, BoundExpression right)
            //=> Binary(syntax, left, SyntaxKind.PlusToken, right);
            => Binary(left, TokenKind.TK_ADD, right);

        public static BoundBinExpression LessOrEqual(SyntaxNode syntax, BoundExpression left, BoundExpression right)
            //=> Binary(syntax, left, SyntaxKind.LessOrEqualsToken, right);
            => Binary(left, TokenKind.TK_LE, right);

        public static BoundExpressionStatement Increment(SyntaxNode syntax, BoundVariableExpression variable)
        {
            BoundAssignmentExpression incrementAssign;
            BoundBinExpression increment;

            increment = Add(syntax, variable, Literal(syntax, 1));

            //incrementAssign = new BoundAssignmentExpression(syntax, variable.Variable, increment);
            //return new BoundAssignmentExpression(syntax, incrementAssign);
            incrementAssign = new BoundAssignmentExpression(variable.Variable, increment);
            return new BoundAssignmentExpression(incrementAssign);
        }

        public static BoundUniExpression Not(SyntaxNode syntax, BoundExpression condition)
        {
            Debug.Assert(condition.Type == TypeSymbol.Indicator);
            
            var op = BoundUniOperator.Bind(SyntaxKind.BangToken, TypeSymbol.Indicator);
            Debug.Assert(op != null);
            return new BoundUniExpression(syntax, op, condition);
        }

        public static BoundVariableExpression Variable(SyntaxNode syntax, boundVariableDeclaration variable)
        {
            return Variable(syntax, variable.Variable);
        }

        public static BoundVariableExpression Variable(SyntaxNode syntax, VariableSymbol variable)
        {
            //return new BoundVariableExpression(syntax, variable);
            return new BoundVariableExpression(variable);
        }

        public static BoundLiteralExp Literal(SyntaxNode syntax, object literal)
        {
            Debug.Assert(literal is string || literal is bool || literal is int);
            
            //return new BoundLiteralExp(syntax, literal);
            return new BoundLiteralExp(literal);
        }
        */
    }
}
