/* ***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is a generic regular expression matching engine
 * written in C#.
 *
 * The Initial Developer of the Original Code is
 * Janosch Scharlipp.
 * 
 * Portions created by the Initial Developer are Copyright (C) 2011
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *  <none yet>
 *
 * ***** END LICENSE BLOCK ***** */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenRegEx.Expression;
using GenRegEx.Vm.Instructions;

namespace GenRegEx.Vm
{
    /// <summary>
    /// Instances of this class are able to process an input sequence according
    /// to a pattern, which is first compiled to a program.
    /// </summary>
    /// <typeparam name="TokenType"></typeparam>
    public class Processor<TokenType>
    {
        public static Processor<TokenType> FromPattern(Pattern<TokenType> pattern)
        {
            Program<TokenType> program = Compiler<TokenType>.Compile(pattern);

            Processor<TokenType> processor = new Processor<TokenType>(program, pattern.tokenWhisperer);

            return processor;
        }

        List<Thread> threads;
        public Match lastMatch;
        Program<TokenType> program;
        TokenWhisperer<TokenType> tokenWhisperer;
        int tokenCounter;
        Dictionary<Instruction, int> instructionGenerations;
        TokenType nextToken;

        bool debug = false;

        public Processor(Program<TokenType> program, TokenWhisperer<TokenType> tokenWhisperer)
        {
            this.tokenWhisperer = tokenWhisperer;

            Execute(program);
        }

        public void Execute(Program<TokenType> program)
        {
            this.program = program;

            Reset();
        }

        public void Reset()
        {
            threads = new List<Thread>();

            // start working immediately, as far as possible
            RunThreadWithoutTokens(new Thread(0, null), 0, threads);

            lastMatch = null;
            tokenCounter = 0;

            nextToken = default(TokenType);

            instructionGenerations = new Dictionary<Instruction, int>();
        }

        public void Print()
        {
            if (!debug) return;

            Console.WriteLine("Processer state:");

            foreach (Thread thread in threads)
            {
                Console.WriteLine("  Thread: " + thread.ToString());
            }
        }

        public bool ProcessToken(TokenType token)
        {
            nextToken = token;

            return RunStep(true);
        }

        /// <summary>
        /// Advances the virtual machine one step.
        /// </summary>
        /// <param name="consumeToken">Is there a token to be processed?</param>
        /// <returns>Can this virtual machine process more tokens, or is the program completely processed</returns>
        public bool RunStep(bool consumeToken)
        {
            TokenType token = default(TokenType);

            if (consumeToken)
            {
                token = nextToken;

                if (debug) Console.WriteLine("Step: " + token.ToString() + " (Counter: " + tokenCounter + ")");
            }
            else
            {
                if (debug) Console.WriteLine("Step without token (Index: " + tokenCounter + ")");
            }

            if (debug) Console.Write("START "); 
            Print();

            // has the current token already been eaten up by a thread?
            bool tokenConsumed = false;

            // threads which have to wait for the next token to continue
            List<Thread> nextThreads = new List<Thread>();

            foreach (Thread thread in threads)
            {
                Instruction instruction = program[thread.NextInstruction];

                // can multiple threads reach the same instruction with different past?
                // if we don't care about the past, then we could oust all exept the first.
                // if (instructionAlreadyVisited(instruction)) continue;

                if (instruction is RequireToken<TokenType>)
                {
                    RequireToken<TokenType> require = (RequireToken<TokenType>)instruction;

                    if (consumeToken)
                    {
                        if (require.AcceptAny || (!require.IsEnd && tokenWhisperer.Equal(token, require.Value)))
                        {
                            tokenConsumed = true;
                            thread.NextInstruction++;

                            RunThreadWithoutTokens(thread, tokenCounter + 1, nextThreads);
                        }
                        else
                        {
                            // this is a dead end, token didn't match, this
                            // thread will die silently
                        }
                    }
                    else
                    {
                        if (require.IsEnd)
                        {
                            thread.NextInstruction++;
                            RunThreadWithoutTokens(thread, tokenCounter, nextThreads);
                        }
                    }
                }
                else if (instruction is FinalizeMatch)
                {
                    // we found a match!

                    if (tokenConsumed)
                    {
                        // but someone else was faster (with higher prio)
                        // so we ignore this match
                    }
                    else
                    {
                        // no other thread with higher prio consumed the current token
                        // so this a real match

                        lastMatch = thread.Match;

                        // all threads with lower prio die
                        break;
                    }
                }
                else
                {
                    throw new Exception("Instruction '" + instruction + "' not handled at this level!");
                }
            }


            threads = nextThreads;

            if (debug) Console.Write("END ");
            Print();

            if (consumeToken)
            {
                tokenCounter++;
            }

            return (threads.Count > 0);
        }

        private void RunThreadWithoutTokens(Thread initialThread, int nextTokenIndex, List<Thread> nextThreads)
        {
            Stack<Thread> threadStack = new Stack<Thread>();
            threadStack.Push(initialThread);

            while (threadStack.Count != 0)
            {
                Thread thread = threadStack.Pop();
                Instruction instruction = program[thread.NextInstruction];

                if (instruction is SplitExecution)
                {
                    SplitExecution split = (SplitExecution)instruction;
                    thread.NextInstruction = split.ThreadStarts[0];

                    // respect prio when adding new threads on the stack
                    // first low prio, then high prio
                    threadStack.Push(new Thread(split.ThreadStarts[1], thread.Match));
                    threadStack.Push(thread);
                }
                else if (instruction is SaveIndex)
                {
                    SaveIndex save = (SaveIndex)instruction;

                    if (save.LimitType == LimitType.Start)
                    {
                        thread.Match.StartIndex = nextTokenIndex;
                    }
                    else if (save.LimitType == LimitType.End)
                    {
                        thread.Match.EndIndex = nextTokenIndex - 1;
                    }
                    else
                    {
                        throw new Exception("Unknown LimitType " + save.LimitType.ToString());
                    }

                    // continue directly with this thread
                    thread.NextInstruction++;
                    threadStack.Push(thread);
                }
                else if (instruction is Jump)
                {
                    Jump jump = (Jump)instruction;

                    // continue directly with this thread
                    thread.NextInstruction = jump.Target;
                    threadStack.Push(thread);
                }
                else
                {
                    // only RequireToken and FinalizeMatch instructions remain
                    // these should be processed with the next token
                    nextThreads.Add(thread);
                }
            }
        }

        /// <summary>
        /// Checks whether an instruction has already been processed by a 
        /// thread with higher prio.
        /// </summary>
        /// <param name="instruction"></param>
        /// <returns></returns>
        private bool instructionAlreadyVisited(Instruction instruction)
        {
            if (instructionGenerations.ContainsKey(instruction) && (instructionGenerations[instruction] == tokenCounter))
            {
                return true;
            }

            instructionGenerations[instruction] = tokenCounter;

            return false;
        }

        /// <summary>
        /// Runs the virtual machine till the end, without consuming
        /// any more tokens.
        /// </summary>
        /// <returns>Do we have a match?</returns>
        public bool Finish()
        {
            while (threads.Count > 0)
            {
                RunStep(false);
            }

            return (lastMatch != null);
        }
    }
}
