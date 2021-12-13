using rpgc.Binding;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpgc.Symbols;

namespace rpgc.Lowering
{
    internal sealed class Lowerer : BoundTreeRewriter
    {
        private int _lableCount;

        private Lowerer()
        {
            //
        }

        public BoundLabel generateLable()
        {
            string lbl;

            lbl = $"^Lable{_lableCount}";
            _lableCount += 1;

            return new BoundLabel(lbl);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////
        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            Lowerer lorr;
            BoundStatement res;
            BoundBlockStatement results;

            lorr = new Lowerer();
            res = lorr.rewriteStatement(statement);
            results = flatten(res);

            return results;
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////
        private static BoundBlockStatement flatten(BoundStatement stmnt)
        {
            ImmutableArray<BoundStatement>.Builder builder;
            Stack<BoundStatement> stack;
            BoundStatement current;
            BoundBlockStatement blocks;
            BoundBlockStatement ret;

            builder = ImmutableArray.CreateBuilder<BoundStatement>();
            stack = new Stack<BoundStatement>();
            stack.Push(stmnt);

            while (stack.Count > 0)
            {
                current = stack.Pop();

                if (current is BoundBlockStatement)
                {
                    blocks = (BoundBlockStatement)current;
                    foreach (BoundStatement s in blocks.Statements.Reverse())
                    {
                        stack.Push(s);
                    }
                }
                else
                {
                    builder.Add(current);
                }
            }

            ret = new BoundBlockStatement(builder.ToImmutable());
            return ret;
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////
        protected override BoundStatement rewriteIfStatement(BoundIfStatement node)
        {
            BoundLabel endLable, elseLable;
            BoundGoToStatement gotoEndStatement;
            BoundBlockStatement result;
            BoundLabelStatement endLableStatement, elseLableStatement;
            BoundGoToConditionalStatement gotoWhenFalse;

            if (node.ElseStatement == null)
            {
                /*
                 * if <condition> ;
                 *     <then Statement>
                 * endif;
                 * 
                 * gotoWhenfalse <condition> end
                 * <then Statement>
                 * end:
                 */
                endLable = generateLable();
                gotoWhenFalse = new BoundGoToConditionalStatement(endLable, node.Condition, true);
                endLableStatement = new BoundLabelStatement(endLable);
                result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    gotoWhenFalse,
                    node.ThenStatement,
                    endLableStatement));

                return rewriteStatement(result);
            }
            else
            {
                /*
                 * if <condition> ;
                 *     <then Statement>
                 * else;
                 *     <else Statement>
                 * endif;
                 * 
                 * gotoWhenfalse <condition> else
                 * <then Statement>
                 * goto end
                 * else:
                 * <else Statement>
                 * end:
                 */
                endLable = generateLable();
                elseLable = generateLable();

                gotoWhenFalse = new BoundGoToConditionalStatement(elseLable, node.Condition, true);
                gotoEndStatement = new BoundGoToStatement(endLable);
                elseLableStatement = new BoundLabelStatement(elseLable);
                endLableStatement = new BoundLabelStatement(endLable);
                result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    gotoWhenFalse,
                    node.ThenStatement,
                    gotoEndStatement,
                    elseLableStatement,
                    node.ElseStatement,
                    endLableStatement
                ));

                return rewriteStatement(result);
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////
        protected override BoundStatement rewriteBoundForStatement(BoundForStatement node)
        {
            // treet for loop as a while loop
            /*
             * for <variable> = 0 to <limit>;
             *     <loop statement>
             * endfor;
             * 
             * dow <condition> ;
             *     <loop statement>
             *     <variable> = <variable> + inc
             * enddo;
            */
            BoundBinExpression condition;
            BoundExpressionStatement inc;
            BoundExpressionStatement setLowerBound;
            BoundVariableExpression variableExpression;
            BoundBlockStatement result;
            BoundLabelStatement Continue_Label;
            BoundLabelStatement iterationStart;
            BoundLabel lblIter;
            BoundGoToConditionalStatement gotoWhenFalse;
            BoundGoToStatement gotoStart;
            BoundLabelStatement endLableStatement;




            lblIter = generateLable();
            Continue_Label = new BoundLabelStatement(lblIter);
            gotoStart = new BoundGoToStatement(lblIter);

            variableExpression = new BoundVariableExpression(node.Variable);
            condition = new BoundBinExpression(variableExpression,
                                               BoundBinOperator.bind(Syntax.TokenKind.TK_LE, TypeSymbol.Integer, TypeSymbol.Integer),
                                               node.Ubound);

            gotoWhenFalse = new BoundGoToConditionalStatement(node.BreakLbl, condition, true);
            endLableStatement = new BoundLabelStatement(node.BreakLbl);
            iterationStart = new BoundLabelStatement(node.ContinueLbl);

            setLowerBound = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                        node.Variable,
                        node.LBound
                    )
            );
            inc = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinExpression(
                            variableExpression,
                                BoundBinOperator.bind(Syntax.TokenKind.TK_ADD, TypeSymbol.Integer, TypeSymbol.Integer),
                                node.IncrementBy
                            )
                    )
            );


            result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                setLowerBound,
                Continue_Label,
                gotoWhenFalse,
                node.Body,
                iterationStart,
                inc,
                gotoStart,
                endLableStatement
            ));

            return rewriteStatement(result);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////
        protected override BoundStatement rewriteBoundWhileStatement(BoundWhileStatement node)
        {
            /* 
             * dow <condition> ;
             *     <loop statement>
             *     variable += inc
             * enddo;
             * 
             * continueLable:
             * goto_WhenFalse <condition> endLable
             * <body>
             * goto continueLable
             * endLable:
             * 
            */
            BoundLabelStatement continueLableStatement;
            BoundLabelStatement endLableStatement;
            BoundBlockStatement result;
            BoundGoToConditionalStatement gotoWhenFalse;
            BoundGoToStatement gotoStart;


            gotoWhenFalse = new BoundGoToConditionalStatement(node.BreakLbl, node.Condition, true);
            gotoStart = new BoundGoToStatement(node.ContinueLbl);
            endLableStatement = new BoundLabelStatement(node.BreakLbl);

            if (node.ContinueLbl == null)
                continueLableStatement = new BoundLabelStatement(generateLable());
            else
                continueLableStatement = new BoundLabelStatement(node.ContinueLbl);



            result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                continueLableStatement,
                gotoWhenFalse,
                node.Body,
                gotoStart,
                endLableStatement
            ));

            return rewriteStatement(result);
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////
        protected override BoundStatement rewriteBoundDoUntilStatement(BoundUntilStatement node)
        {
            /* 
             * dou <condition> ;
             *     <loop statement>
             * enddo;
             * 
             * 
             * goto checkLable
             * continueLable:
             * <body>
             * checkLable:
             * gotoTrue <condition> continueLable
             * endLable:
             * 
            */
            //BoundLabel endLable, checkLable, continueLable;
            BoundLabel checkLable;
            BoundGoToStatement gotoCheck;
            BoundLabelStatement continueLableStatement;
            BoundLabelStatement checkLableStatement;
            BoundGoToConditionalStatement gotoTrue;
            BoundLabelStatement endLableStatement;
            BoundBlockStatement result;

            //continueLable = generateLable();
            checkLable = generateLable();
            //endLable = generateLable();

            gotoCheck = new BoundGoToStatement(checkLable);
            continueLableStatement = new BoundLabelStatement(node.ContinueLbl);
            checkLableStatement = new BoundLabelStatement(checkLable);
            gotoTrue = new BoundGoToConditionalStatement(node.ContinueLbl, node.Condition, true);
            endLableStatement = new BoundLabelStatement(node.BreakLbl);

            /*
            result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoCheck,
                continueLableStatement,
                node.Body,
                checkLableStatement,
                gotoTrue,
                endLableStatement
            ));
            */





            var gotoWhenFalse = new BoundGoToConditionalStatement(node.BreakLbl, node.Condition);
            var gotoStart = new BoundGoToStatement(node.ContinueLbl);


            result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                continueLableStatement,
                gotoWhenFalse,
                node.Body,
                gotoStart,
                endLableStatement
            ));

            return rewriteStatement(result);
        }
    }
}
