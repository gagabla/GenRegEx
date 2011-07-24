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
using System.Text.RegularExpressions;

namespace GenRegEx.Expression
{
    /// <summary>
    /// Available modes to combine the elements of a group.
    /// Concatenation:
    ///   elements are concatenated
    ///   
    /// Alternation:
    ///   one of the elements is selected
    ///   
    /// </summary>
    public enum GroupMode
    {
        Concatenation,
        Alternation
    }

    /// <summary>
    /// This a container, all it can do is parse a string representation or
    /// generate it for a group.
    /// </summary>
    /// <typeparam name="TokenType"></typeparam>
    public class Group<TokenType> : Node<TokenType>
    {
        public List<Node<TokenType>> Elements = new List<Node<TokenType>>();
        public GroupMode GroupMode = GroupMode.Concatenation;

        protected static Regex concatRe = new Regex(
                @"^" + // Make sure it starts at the beginning
                @"(" + // leading tokens or groups with trailing space
                    @"(?'Tokens'" + // group for tokens or groups
                        @"([^\(\) \|]+)" + // either tokens
                        @"|" +  // or
                        @"(" + // groups
                            @"(" + // maybe starting hierachicaly
                                @"(?'Open'\()" + // starting with "("
                                @"[^\(\)]*" + // containing anything but no "(" and ")"
                            @")*" +
                            @"(" + // ending hierachicaly too
                                @"(?'Close-Open'\))" + // with a ")"
                                @"[^\(\)]*" + // and maybe other trailing stuff
                            @")*" +
                        @")" +
                    @")" +
                    @" " + // and a space
                @")*" + // dont care how many
                @"(" +  // 
                    @"(?'LastToken'[^\(\) \|]+)|(\(.+\))" + //
                @")" +
                @"$" // nothing should be left
                , RegexOptions.Compiled
            );

        protected static Regex alternateRe = new Regex(
                @"^" +
                @"(" +
                    @"(?'Tokens'" +
                        @"([^\(\) \|]+)" +
                        @"|" +
                        @"(" +
                            @"(" +
                                @"(?'Open'\()" +
                                @"[^\(\)]*" +
                            @")*" +
                            @"(" +
                                @"(?'Close-Open'\))" +
                                @"[^\(\)]*" +
                            @")*" +
                        @")" +
                    @")" +
                    @"\|" +
                @")*" +
                @"(" +
                    @"(?'LastToken'[^\(\) \|]+)|(\(.+\))" +
                @")" +
                @"$"
                , RegexOptions.Compiled
            );

        public Group(TokenWhisperer<TokenType> tokenWhisperer)
        {
            this.tokenWhisperer = tokenWhisperer;
        }

        public override void FromString(string str)
        {
            Elements = new List<Node<TokenType>>();

            base.FromString(str);

            str = str.TrimEnd(new char[]{'+', '*', '?'});

            if (!str.StartsWith("(") || !str.EndsWith(")"))
            {
                throw new Exception("Invalid GenRegEx Group, missing starting '(' or ending ')' in string '" + str + "'");
            }

            str = str.Substring(1, str.Length - 2);

            List<string> elementStrings = new List<string>();
            Match match;

            if (concatRe.IsMatch(str))
            {
                match = concatRe.Match(str);
                this.GroupMode = GroupMode.Concatenation;
            }
            else if (alternateRe.IsMatch(str))
            {
                match = alternateRe.Match(str);
                this.GroupMode = GroupMode.Alternation;
            }
            else
            {
                throw new Exception("Invalid GenRegEx Group, neither concatenation nor alternation detected in '" + str + "'");
            }

            // now collect all element strings
            foreach (Capture capture in match.Groups["Tokens"].Captures)
            {
                elementStrings.Add(capture.Value.Substring(0, capture.Value.Length));
            }

            elementStrings.Add(match.Groups["LastToken"].Value);

            // and now process them recursively
            foreach (string elementString in elementStrings)
            {
                // is it a group?
                if (elementString.StartsWith("("))
                {
                    Group<TokenType> group = new Group<TokenType>(tokenWhisperer);
                    group.FromString(elementString);
                    Elements.Add(group);
                }
                else
                {
                    Token<TokenType> token = new Token<TokenType>(tokenWhisperer);
                    token.FromString(elementString);
                    Elements.Add(token);
                }

            }
        }

        public override string ToString()
        {
            string result = "(";

            result += String.Join(GroupMode == Expression.GroupMode.Concatenation ? " " : "|", Elements);
            result += ")";
            result += base.ToString();

            return result;
        }
    }
}
