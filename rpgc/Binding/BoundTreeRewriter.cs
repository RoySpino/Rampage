using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rpgc.Binding
{
    internal abstract class BoundTreeRewriter
    {
        public BoundStatement rewriteStatement(BoundStatement node)
        {
            switch (node.tok)
            {
                case BoundNodeToken.BNT_ERROREXP:
                    return node;
                case BoundNodeToken.BNT_BLOCKSTMT:
                    return rewriteBlockStatement((BoundBlockStatement)node);
                case BoundNodeToken.BNT_EXPRSTMT:
                    return rewriteExpressionStatement((BoundExpressionStatement)node);
                case BoundNodeToken.BNT_VARDECLR:
                    return rewriteVariableDeclaration((boundVariableDeclaration)node);
                case BoundNodeToken.BNT_IFSTMT:
                    return rewriteIfStatement((BoundIfStatement)node);
                case BoundNodeToken.BNT_WHILESTMT:
                    return rewriteBoundWhileStatement((BoundWhileStatement)node);
                case BoundNodeToken.BNT_FORSTMT:
                    return rewriteBoundForStatement((BoundForStatement)node);
                case BoundNodeToken.BNT_DOUNTIL:
                    return rewriteBoundDoUntilStatement((BoundUntilStatement)node);
                case BoundNodeToken.BNT_SELECTSTMT:
                    return rewriteBoundSelectStatement((BoundSelectWhenStatement)node);
                case BoundNodeToken.BNT_LABEL:
                    return rewriteBoundLabelStatement((BoundLabelStatement)node);
                case BoundNodeToken.BNT_GOTO:
                    return rewriteBoundGoToStatement((BoundGoToStatement)node);
                case BoundNodeToken.BNT_GOTOCOND:
                    return rewriteBoundGoToConditinalStatement((BoundGoToConditionalStatement)node);
                case BoundNodeToken.BNT_RETSTMT:
                    return rewriteReturnStatement((BoundReturnStatement)node);
                default:
                    throw new Exception($"Unexpected node{node.tok}");
            }
        }

        // /////////////////////////////////////////////////////////////////////////////
        public BoundExpression rewriteExpression(BoundExpression node)
        {
            switch (node.tok)
            {
                case BoundNodeToken.BNT_ERROREXP:
                    return node;
                case BoundNodeToken.BNT_UINEX:
                    return rewriteUniaryExpression((BoundUniExpression)node);
                case BoundNodeToken.BNT_LITEX:
                    return rewriteLiteralExpression((BoundLiteralExp)node);
                case BoundNodeToken.BNT_VAREX:
                    return rewriteVariableExpression((BoundVariableExpression)node);
                case BoundNodeToken.BNT_ASNEX:
                    return rewriteAssignmentExpression((BoundAssignmentExpression)node);
                case BoundNodeToken.BNT_BINEX:
                    return rewriteBinaryExpression((BoundBinExpression)node);
                case BoundNodeToken.BNT_CALLEXP:
                    return rewriteCallExression((BoundCallExpression)node);
                case BoundNodeToken.BNT_CONVEXP:
                    return rewriteConversionExpression((BoundConversionExpression)node);
                default:
                    throw new Exception($"Unexpected node{node.tok}");
            }
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteReturnStatement(BoundReturnStatement node)
        {
            BoundExpression expression;

            if (node.Expression != null)
                expression = rewriteExpression(node.Expression);
            else
                expression = null;

            if (expression == node.Expression)
                return node;

            return new BoundReturnStatement(expression);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundExpression rewriteConversionExpression(BoundConversionExpression node)
        {
            BoundExpression expr;

            expr = rewriteExpression(node._Expression);

            if (expr == node._Expression)
                return node;

            return new BoundConversionExpression(node.Type, expr);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundExpression rewriteBinaryExpression(BoundBinExpression node)
        {
            BoundExpression left;
            BoundExpression right;

            left = rewriteExpression(node.Left);
            right = rewriteExpression(node.Right);

            if (right == node.Right && left == node.Left)
                return node;

            return new BoundBinExpression(left, node.OP, right);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundExpression rewriteAssignmentExpression(BoundAssignmentExpression node)
        {
            BoundExpression expression;

            expression = rewriteExpression(node.Expression);

            if (expression == node.Expression)
                return node;

            return new BoundAssignmentExpression(node.Variable, expression);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundExpression rewriteUniaryExpression(BoundUniExpression node)
        {
            BoundExpression operand;

            operand = rewriteExpression(node.right);

            if (operand == node.right)
                return node;

            return new BoundUniExpression(node.OP, operand);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundExpression rewriteVariableExpression(BoundVariableExpression node)
        {
            return node;
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundExpression rewriteLiteralExpression(BoundLiteralExp node)
        {
            return node;
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundExpression rewriteCallExression(BoundCallExpression node)
        {
            ImmutableArray<BoundExpression>.Builder builder = null;
            BoundExpression oldArgument;
            BoundExpression newArgument;

            for (int i = 0; i < node.Arguments.Length; i++)
            {
                oldArgument = node.Arguments[i];
                newArgument = rewriteExpression(oldArgument);

                if (newArgument != oldArgument)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpression>(node.Arguments.Length);

                        for (int u = 0; u < i; u++)
                            builder.Add(node.Arguments[u]);
                    }
                }

                if (builder != null)
                    builder.Add(newArgument);
            }

            if (builder == null)
                return node;

            return new BoundCallExpression(node.Function, builder.MoveToImmutable());
        }


        // //////////////////////////////////////////////////////////////////////////////////////////////////////////
        // /////     /////     /////     /////     /////     /////     /////     /////     /////     /////     /////
        // ////////////////////////////////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteBlockStatement(BoundBlockStatement node)
        {
            ImmutableArray<BoundStatement>.Builder builder = null;
            BoundStatement oldStatement;
            BoundStatement newStatement;

            for (int i = 0; i < node.Statements.Length; i++)
            {
                oldStatement = node.Statements[i];
                newStatement = rewriteStatement(oldStatement);

                if (newStatement != oldStatement)
                {
                    if (builder == null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);

                        for (int u = 0; u < i; u++)
                        {
                            builder.Add(node.Statements[u]);
                        }
                    }
                }

                if (builder != null)
                    builder.Add(newStatement);
            }

            if (builder == null)
                return node;

            return new BoundBlockStatement(builder.MoveToImmutable());
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteExpressionStatement(BoundExpressionStatement node)
        {
            BoundExpression expression;

            expression = rewriteExpression(node.Expression);

            if (expression != node.Expression)
                return node;

            return new BoundExpressionStatement(expression);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteVariableDeclaration(boundVariableDeclaration node)
        {
            BoundExpression initilizer;

            initilizer = rewriteExpression(node.Initalizer);

            if (initilizer == node.Initalizer)
                return node;

            return new boundVariableDeclaration(node.Variable, initilizer);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteIfStatement(BoundIfStatement node)
        {
            BoundExpression condition;
            BoundStatement trueStatementBody;
            BoundStatement elseStaetmentBody;

            condition = rewriteExpression(node.Condition);
            trueStatementBody = rewriteStatement(node.ThenStatement);

            if (node.ElseStatement != null)
                elseStaetmentBody = rewriteStatement(node.ElseStatement);
            else
                elseStaetmentBody = null;

            if (node.Condition == condition && node.ThenStatement == trueStatementBody && node.ElseStatement == elseStaetmentBody)
                return node;

            return new BoundIfStatement(condition, trueStatementBody, elseStaetmentBody);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteBoundWhileStatement(BoundWhileStatement node)
        {
            BoundExpression condition;
            BoundStatement body;

            condition = rewriteExpression(node.Condition);
            body = rewriteStatement(node.Body);

            if (node.Body == body && node.Condition == condition)
                return node;

            return new BoundWhileStatement(condition, body, node.BreakLbl, node.ContinueLbl);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteBoundDoWhileStatement(BoundUntilStatement node)
        {
            BoundExpression condition;
            BoundStatement body;

            condition = rewriteExpression(node.Condition);
            body = rewriteStatement(node.Body);

            if (node.Body == body && node.Condition == condition)
                return node;

            return new BoundUntilStatement(condition, body, node.BreakLbl, node.ContinueLbl);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteBoundForStatement(BoundForStatement node)
        {
            BoundExpression lowerBound;
            BoundExpression uperBound;
            BoundStatement body;

            lowerBound = rewriteExpression(node.LBound);
            uperBound = rewriteExpression(node.Ubound);
            body = rewriteStatement(node.Body);

            if (node.LBound == lowerBound && node.Ubound == uperBound && node.Body == body)
                return node;

            return new BoundForStatement(node.Variable, lowerBound, uperBound, body, node.IsCountUP, node.IncrementBy, node.BreakLbl, node.ContinueLbl);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteBoundDoUntilStatement(BoundUntilStatement node)
        {
            BoundExpression condition;
            BoundStatement body;

            condition = rewriteExpression(node.Condition);
            body = rewriteStatement(node.Body);

            if (node.Body == body && node.Condition == condition)
                return node;

            return new BoundUntilStatement(condition, body, node.BreakLbl, node.ContinueLbl);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteBoundSelectStatement(BoundSelectWhenStatement node)
        {
            ImmutableArray<BoundExpression> conditionList = node.BoundExpressions;
            ImmutableArray<BoundStatement> whenBodiesLst = node.BoundStatements;
            ImmutableArray<BoundExpression>.Builder newConditionList;
            ImmutableArray<BoundStatement>.Builder newWhenBodiesLst;
            BoundStatement defaultBlock;
            BoundStatement tmpStatemnt;

            newConditionList = ImmutableArray.CreateBuilder<BoundExpression>();
            newWhenBodiesLst = ImmutableArray.CreateBuilder<BoundStatement>();

            // create new flatten condition expressions
            foreach (BoundExpression condition in conditionList)
                newConditionList.Add(rewriteExpression(condition));

            // flaten the WHEN blocks
            foreach (BoundStatement body in whenBodiesLst)
                newWhenBodiesLst.Add(rewriteStatement(body));

            // prepare the default block
            tmpStatemnt = node.DefualtStatements;
            defaultBlock = null;

            // rewrite the OTHER (default) block
            if (tmpStatemnt != null)
                defaultBlock = rewriteStatement(node.DefualtStatements);
                //defaultBlock = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(rewriteStatement(tmpStatemnt)));

            // create new flaten SELECT statement
            return new BoundSelectWhenStatement(newConditionList.ToImmutableArray(), newWhenBodiesLst.ToImmutableArray(), defaultBlock);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteBoundGoToConditinalStatement(BoundGoToConditionalStatement node)
        {
            BoundExpression condition;

            condition = rewriteExpression(node.Condition);

            if (condition == node.Condition)
                return node;

            return new BoundGoToConditionalStatement(node.Label, condition, node.JumpIfFalse);
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteBoundGoToStatement(BoundGoToStatement node)
        {
            return node;
        }

        // /////////////////////////////////////////////////////////////////////////////
        protected virtual BoundStatement rewriteBoundLabelStatement(BoundLabelStatement node)
        {
            return node;
        }

    }
}
