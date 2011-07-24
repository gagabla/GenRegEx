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
using GenRegEx.Vm.Instructions;
using GenRegEx.Expression;

namespace GenRegEx.Vm
{
    /// <summary>
    /// Compiles regex patterns to regex programs, which can be executed then by a regex processor.
    /// </summary>
    /// <typeparam name="TokenType"></typeparam>
    public class Compiler<TokenType>
    {
        public static Program<TokenType> Compile(Pattern<TokenType> pattern)
        {
            Compiler<TokenType> compiler = new Compiler<TokenType>();

            return compiler.CompilePattern(pattern);
        }

        public static Program<TokenType> Compile(string str, TokenWhisperer<TokenType> tokenWhisperer)
        {
            Pattern<TokenType> pattern = new Pattern<TokenType>(tokenWhisperer);

            return Compile(pattern);
        }

        protected Program<TokenType> program;

        public Program<TokenType> CompilePattern(Pattern<TokenType> pattern)
        {
            program = new Program<TokenType>();

            ProcessNode(pattern);

            if (pattern.MatchTillEnd)
            {
                RequireToken<TokenType> end = new RequireToken<TokenType>();
                end.IsEnd = true;

                program.Add(end);
            }

            program.Add(new SaveIndex(LimitType.End));
            program.Add(new FinalizeMatch());

            return program;
        }

        public void Descend(Node<TokenType> node)
        {
            if (node is Group<TokenType>)
            {
                ProcessGroup(node as Group<TokenType>);
            }
            else if (node is Token<TokenType>)
            {
                ProcessToken(node as Token<TokenType>);
            }
            else
            {
                throw new Exception("unkown node type");
            }
        }

        public void ProcessNode(Node<TokenType> node)
        {
            if (node.RepetitionMode == RepetitionMode.Any)
            {
                SplitExecution split = new SplitExecution();
                int startIndex = program.Count;

                // first split
                program.Add(split);

                // then loop over the content and jump back to split
                Descend(node);
                program.Add(new Jump(startIndex));

                if (node.Greedy)
                {
                    // greedy means: prefer to eat content
                    split.ThreadStarts.Add(startIndex + 1);

                    // later try to skip it
                    split.ThreadStarts.Add(program.Count);
                }
                else
                {
                    // not gready means: skip if possible
                    split.ThreadStarts.Add(program.Count);

                    // otherwise eat it up
                    split.ThreadStarts.Add(startIndex + 1);
                }

            }
            else if (node.RepetitionMode == RepetitionMode.Some)
            {
                int startIndex = program.Count;
                SplitExecution split = new SplitExecution();

                // we need the content at least once
                Descend(node);

                // then we have two possibilities
                program.Add(split);

                if (node.Greedy)
                {
                    // if possible we eat up the content once more
                    split.ThreadStarts.Add(startIndex);

                    // or we skip it
                    split.ThreadStarts.Add(program.Count);
                }
                else
                {
                    // prefer to skip the content
                    split.ThreadStarts.Add(program.Count);

                    // but we can eat it up, if required
                    split.ThreadStarts.Add(startIndex);
                }
            }
            else if (node.RepetitionMode == RepetitionMode.None)
            {
                // this is easy, eat up content exactly once
                Descend(node);
            }
        }

        public void ProcessGroup(Group<TokenType> group)
        {
            int startIndex = program.Count;

            if (group is Pattern<TokenType>)
            {
                Pattern<TokenType> pattern = (Pattern<TokenType>)group;

                if (!pattern.MatchFromStart)
                {
                    Token<TokenType> leading = new Token<TokenType>();
                    leading.AcceptAny = true;
                    leading.RepetitionMode = RepetitionMode.Any;
                    leading.Greedy = false;

                    ProcessNode(leading);
                }

                program.Add(new SaveIndex(LimitType.Start));
            }

            if (group.GroupMode == GroupMode.Concatenation)
            {
                // simply do one after the other
                foreach (Node<TokenType> node in group.Elements)
                {
                    ProcessNode(node);
                }
            }
            else if (group.GroupMode == GroupMode.Alternation)
            {
                // hmm, this can propably be implemented with a huge split
                throw new NotImplementedException();
            }
        }

        public void ProcessToken(Token<TokenType> token)
        {
            RequireToken<TokenType> result = new RequireToken<TokenType>(token.Value);

            if (token.AcceptAny)
            {
                result.AcceptAny = true;
            }

            program.Add(result);
        }
    }
}
