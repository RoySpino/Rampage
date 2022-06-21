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
             * continueLable:
             * gotoWhenFalse <condition> endLabel
             * <body>
             * goto continueLable
             * endLable:
             * 
            */
            //BoundLabel endLable, checkLable, BreakLable;
            //BoundLabel checkLable;
            //BoundGoToStatement gotoCheck;
            //BoundLabelStatement checkLableStatement;
            //BoundGoToConditionalStatement gotoTrue;
            BoundLabelStatement continueLableStatement;
            BoundLabelStatement endLableStatement;
            BoundBlockStatement result;
            BoundGoToConditionalStatement gotoWhenFalse;
            BoundGoToStatement gotoStart;

            //continueLable = generateLable();
            //checkLable = generateLable();
            //endLable = generateLable();

            //gotoCheck = new BoundGoToStatement(checkLable);
            //checkLableStatement = new BoundLabelStatement(checkLable);
            //gotoTrue = new BoundGoToConditionalStatement(node.ContinueLbl, node.Condition, true);

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





            continueLableStatement = new BoundLabelStatement(node.ContinueLbl);
            endLableStatement = new BoundLabelStatement(node.BreakLbl);
            gotoWhenFalse = new BoundGoToConditionalStatement(node.BreakLbl, node.Condition);
            gotoStart = new BoundGoToStatement(node.ContinueLbl);


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
        protected override BoundStatement rewriteBoundSelectStatement(BoundSelectWhenStatement node)
        {
            /* 
             * select
             *     when <condition> ;
             *         <block statement 1>
             *     otherwise ;
             *         <block statement 2>
             * endsl;
             * 
             * 
             * 
             * gotoWhenFalse <condition> LBL_cond_1
             * gotoWhenFalse <condition> LBL_cond_2
             * goto LBL_cond_other
             * LBL_cond_1:
             *     <block statement 1>
             *     goto LBL_end
             * LBL_cond_2:
             *     <block statement 2>
             *     goto LBL_end
             * LBL_cond_other
             *     <block statement 1>
             *     goto LBL_end
             * end_Lable:
             * 
            */
            ImmutableArray<BoundStatement>.Builder conditionTable;
            ImmutableArray<BoundStatement>.Builder blockTable;
            ImmutableArray<BoundStatement>.Builder ret;
            BoundLabel endLable, tmpLbl;
            BoundGoToStatement gotoOtherwiseStatement, gotoEND;
            BoundBlockStatement result;
            BoundLabelStatement endLableStatement, tmpLableStatement;
            BoundGoToConditionalStatement gotoWhenFalse;
            int lim;


            lim = node.BoundExpressions.Length;
            conditionTable = ImmutableArray.CreateBuilder<BoundStatement>();
            blockTable = ImmutableArray.CreateBuilder<BoundStatement>();
            ret = ImmutableArray.CreateBuilder<BoundStatement>();


            endLable = generateLable();
            endLableStatement = new BoundLabelStatement(endLable);
            gotoEND = new BoundGoToStatement(endLable);

            // rebind the statments and conditions
            for (int i=0; i<lim; i++)
            {
                tmpLbl = generateLable();
                tmpLableStatement = new BoundLabelStatement(tmpLbl);
                gotoWhenFalse = new BoundGoToConditionalStatement(tmpLbl, node.BoundExpressions[i], false);

                // build CONDITION table
                conditionTable.Add(gotoWhenFalse);

                // rebuild WHERE blocks
                blockTable.Add(new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    tmpLableStatement,
                    node.BoundStatements[i],
                    gotoEND
                    )));
            }

            // an Otherwise keyword was use
            // add it to the lists
            if (node.DefualtStatements != null)
            {
                tmpLbl = generateLable();
                tmpLableStatement = new BoundLabelStatement(tmpLbl);
                gotoOtherwiseStatement = new BoundGoToStatement(tmpLbl);

                // add a goto at the end of the condition list
                conditionTable.Add(gotoOtherwiseStatement);

                // add the default block to the WHERE list
                blockTable.Add(new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                    tmpLableStatement,
                    node.DefualtStatements,
                    gotoEND
                    )));
            }
            else
            {
                // no default case was found
                // add a GOTO to skip all WHEN blocks
                conditionTable.Add(gotoEND);
            }

            // add end lable to block list
            blockTable.Add(endLableStatement);

            // combine statements into one list
            ret.AddRange(conditionTable);
            ret.AddRange(blockTable);

            result = new BoundBlockStatement(ret.ToImmutable());

            return result;
        }
    }
}
