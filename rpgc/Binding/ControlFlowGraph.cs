using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace rpgc.Binding
{
    internal sealed class ControlFlowGraph
    {
        public BasicBlock Start { get; }
        public BasicBlock End { get; }
        public List<BasicBlock> Blocks { get; }
        public List<BasicBlockBranch> Branches { get; }

        private ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockBranch> branches)
        {
            Start = start;
            End = end;
            Blocks = blocks;
            Branches = branches;
        }

        // /////////////////////////////////////////////////////////////////////////////
        public void WriteTo(TextWriter writer)
        {
            Dictionary<BasicBlock, string> blockIds;
            string id, label, fromId, toId;

            string Quote(string text)
            {
                return "\"" + text.TrimEnd().Replace("\\", "\\\\").Replace(Environment.NewLine, "\\l") + "\"";
            }

            writer.WriteLine("digraph G {");

            blockIds = new Dictionary<BasicBlock, string>();

            for (int i = 0; i < Blocks.Count; i++)
            {
                id = $"N{i}";
                blockIds.Add(Blocks[i], id);
            }

            foreach (BasicBlock block in Blocks)
            {
                id = blockIds[block];
                label = Quote(block.ToString());
                writer.WriteLine($"    {id} [label = {label}, shape = box]");
            }

            foreach (BasicBlockBranch branch in Branches)
            {
                fromId = blockIds[branch.From];
                toId = blockIds[branch.To];
                label = Quote(branch.ToString());
                writer.WriteLine($"    {fromId} -> {toId} [label = {label}]");
            }

            writer.WriteLine("}");
        }

        // /////////////////////////////////////////////////////////////////////////////
        public static ControlFlowGraph Create(BoundBlockStatement body)
        {
            BasicBlockBuilder basicBlockBuilder;
            List<BasicBlock> blocks;
            GraphBuilder graphBuilder;

            basicBlockBuilder = new BasicBlockBuilder();
            blocks = basicBlockBuilder.Build(body);
            graphBuilder = new GraphBuilder();

            return graphBuilder.Build(blocks);
        }

        // /////////////////////////////////////////////////////////////////////////////
        public static bool AllPathsReturn(BoundBlockStatement body)
        {
            ControlFlowGraph graph;
            BoundStatement lastStatement;

            graph = Create(body);

            foreach (var branch in graph.End.Incoming)
            {
                lastStatement = branch.From.Statements.LastOrDefault();
                if (lastStatement == null || lastStatement.tok != BoundNodeToken.BNT_RETSTMT)
                    return false;
            }

            return true;
        }


        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // /////     /////     /////     /////     /////     /////     /////     /////     /////     /////
        // //////////////////////////////////////////////////////////////////////////////////////////////
        public sealed class BasicBlock
        {
            public bool IsStart { get; }
            public bool IsEnd { get; }
            public List<BoundStatement> Statements { get; } = new List<BoundStatement>();
            public List<BasicBlockBranch> Incoming { get; } = new List<BasicBlockBranch>();
            public List<BasicBlockBranch> Outgoing { get; } = new List<BasicBlockBranch>();

            // /////////////////////////////////////////////////////////////////////////////
            public BasicBlock()
            {
            }

            // /////////////////////////////////////////////////////////////////////////////
            public BasicBlock(bool isStart)
            {
                IsStart = isStart;
                IsEnd = !isStart;
            }

            // /////////////////////////////////////////////////////////////////////////////
            public override string ToString()
            {
                if (IsStart)
                    return "<Start>";

                if (IsEnd)
                    return "<End>";

                using (StringWriter writer = new StringWriter())
                using (IndentedTextWriter indentedWriter = new IndentedTextWriter(writer))
                {
                    /*
                    foreach (var statement in Statements)
                        statement.WriteTo(indentedWriter);
                    */
                    return writer.ToString();
                }
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // /////     /////     /////     /////     /////     /////     /////     /////     /////     /////
        // //////////////////////////////////////////////////////////////////////////////////////////////
        public sealed class BasicBlockBranch
        {
            public BasicBlock From { get; }
            public BasicBlock To { get; }
            public BoundExpression Condition { get; }

            public BasicBlockBranch(BasicBlock from, BasicBlock to, BoundExpression condition)
            {
                From = from;
                To = to;
                Condition = condition;
            }

            // //////////////////////////////////////////////////////////////////////////////////////////////
            public override string ToString()
            {
                if (Condition == null)
                    return string.Empty;

                return Condition.ToString();
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // /////     /////     /////     /////     /////     /////     /////     /////     /////     /////
        // //////////////////////////////////////////////////////////////////////////////////////////////
        public sealed class BasicBlockBuilder
        {
            private List<BoundStatement> _statements = new List<BoundStatement>();
            private List<BasicBlock> _blocks = new List<BasicBlock>();

            public List<BasicBlock> Build(BoundBlockStatement block)
            {
                foreach (var statement in block.Statements)
                {
                    switch (statement.tok)
                    {
                        case BoundNodeToken.BNT_LABEL:
                            StartBlock();
                            _statements.Add(statement);
                            break;
                        case BoundNodeToken.BNT_GOTO:
                        case BoundNodeToken.BNT_GOTOCOND:
                        case BoundNodeToken.BNT_RETSTMT:
                            _statements.Add(statement);
                            StartBlock();
                            break;
                        case BoundNodeToken.BNT_NOOP:
                        case BoundNodeToken.BNT_VARDECLR:
                        case BoundNodeToken.BNT_EXPRSTMT:
                        case BoundNodeToken.BNT_ERROREXP:
                            _statements.Add(statement);
                            break;
                        default:
                            throw new Exception($"Unexpected statement: {statement.tok}");
                    }
                }

                EndBlock();

                return _blocks.ToList();
            }

            // //////////////////////////////////////////////////////////////////////////////////////////////
            private void StartBlock()
            {
                EndBlock();
            }

            // //////////////////////////////////////////////////////////////////////////////////////////////
            private void EndBlock()
            {
                if (_statements.Count > 0)
                {
                    var block = new BasicBlock();
                    block.Statements.AddRange(_statements);
                    _blocks.Add(block);
                    _statements.Clear();
                }
            }
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////
        // /////     /////     /////     /////     /////     /////     /////     /////     /////     /////
        // //////////////////////////////////////////////////////////////////////////////////////////////
        public sealed class GraphBuilder
        {
            private Dictionary<BoundStatement, BasicBlock> _blockFromStatement = new Dictionary<BoundStatement, BasicBlock>();
            private Dictionary<BoundLabel, BasicBlock> _blockFromLabel = new Dictionary<BoundLabel, BasicBlock>();
            private List<BasicBlockBranch> _branches = new List<BasicBlockBranch>();
            private BasicBlock _start = new BasicBlock(isStart: true);
            private BasicBlock _end = new BasicBlock(isStart: false);

            public ControlFlowGraph Build(List<BasicBlock> blocks)
            {
                BoundGoToStatement gs;
                BasicBlock toBlock, thenBlock, next, elseBlock, current;
                BoundGoToConditionalStatement cgs;
                BoundExpression negatedCondition, thenCondition, elseCondition;
                bool isLastStatementInBlock;

                if (!blocks.Any())
                    Connect(_start, _end);
                else
                    Connect(_start, blocks.First());

                foreach (BasicBlock block in blocks)
                {
                    foreach (var statement in block.Statements)
                    {
                        _blockFromStatement.Add(statement, block);
                        
                        if (statement is BoundLabelStatement labelStatement)
                            _blockFromLabel.Add(labelStatement.Label, block);
                    }
                }


                for (int i = 0; i < blocks.Count; i++)
                {
                    current = blocks[i];
                    next = ((i == blocks.Count - 1) ? _end : blocks[i + 1]);

                    foreach (var statement in current.Statements)
                    {
                        isLastStatementInBlock = (statement == current.Statements.Last());

                        switch (statement.tok)
                        {
                            case BoundNodeToken.BNT_ERROREXP:
                                // error was encountered: do not examin flow
                                return new ControlFlowGraph(_start, _end, blocks, _branches);
                            case BoundNodeToken.BNT_GOTO:
                                gs = (BoundGoToStatement)statement;
                                toBlock = _blockFromLabel[gs.Label];
                                Connect(current, toBlock);
                                break;
                            case BoundNodeToken.BNT_GOTOCOND:
                                cgs = (BoundGoToConditionalStatement)statement;
                                thenBlock = _blockFromLabel[cgs.Label];
                                elseBlock = next;
                                negatedCondition = Negate(cgs.Condition);
                                thenCondition = cgs.JumpIfFalse == false ? cgs.Condition : negatedCondition;
                                elseCondition = cgs.JumpIfFalse == false ? negatedCondition : cgs.Condition;
                                Connect(current, thenBlock, thenCondition);
                                Connect(current, elseBlock, elseCondition);
                                break;
                            case BoundNodeToken.BNT_RETSTMT:
                                Connect(current, _end);
                                break;
                            case BoundNodeToken.BNT_VARDECLR:
                            case BoundNodeToken.BNT_LABEL:
                            case BoundNodeToken.BNT_EXPRSTMT:
                                if (isLastStatementInBlock)
                                    Connect(current, next);
                                break;
                            default:
                                throw new Exception($"Unexpected statement: {statement.tok}");
                        }
                    }
                }

                ScanAgain:
                foreach (var block in blocks)
                {
                    if (!block.Incoming.Any())
                    {
                        RemoveBlock(blocks, block);
                        goto ScanAgain;
                    }
                }

                blocks.Insert(0, _start);
                blocks.Add(_end);

                return new ControlFlowGraph(_start, _end, blocks, _branches);
            }

            // //////////////////////////////////////////////////////////////////////////////////////////////
            private void Connect(BasicBlock from, BasicBlock to, BoundExpression condition = null)
            {
                bool value;
                BasicBlockBranch branch;

                if (condition is BoundLiteralExp l)
                {
                    value = (bool)l.Value;
                    if (value)
                        condition = null;
                    else
                        return;
                }

                branch = new BasicBlockBranch(from, to, condition);
                from.Outgoing.Add(branch);
                to.Incoming.Add(branch);
                _branches.Add(branch);
            }

            // //////////////////////////////////////////////////////////////////////////////////////////////
            private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
            {
                foreach (BasicBlockBranch branch in block.Incoming)
                {
                    branch.From.Outgoing.Remove(branch);
                    _branches.Remove(branch);
                }

                foreach (BasicBlockBranch branch in block.Outgoing)
                {
                    branch.To.Incoming.Remove(branch);
                    _branches.Remove(branch);
                }

                blocks.Remove(block);
            }

            // //////////////////////////////////////////////////////////////////////////////////////////////
            private BoundExpression Negate(BoundExpression condition)
            {
                BoundLiteralExp lit;
                bool valu;

                if (condition is BoundLiteralExp)
                {
                    lit = (BoundLiteralExp)condition;
                    valu = (bool)lit.Value;
                    return new BoundLiteralExp(!valu);
                }

                return condition;
            }
            /*
            private BoundExpression Negate(BoundExpression condition)
            {
                var negated = BoundNodeFactory.Not(condition.Syntax, condition);
                if (negated.ConstantValue != null)
                    //return new BoundLiteralExp(condition.Syntax, negated.ConstantValue.Value);
                    return new BoundLiteralExp(negated.ConstantValue.Value);

                return negated;
            }
            */
        }
    }
}
