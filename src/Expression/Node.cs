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
using System.Xml.Serialization;

namespace GenRegEx.Expression
{
    /// <summary>
    /// Available modes for repetition of tokens or groups.
    /// </summary>
    public enum RepetitionMode
    {
        None,
        Some,
        Any,
        Maybe
    }

    public class Node<TokenType>
    {
        /// <summary>
        /// Configures, whether this node can be repeated, available options are:
        /// <list type="table">
        ///  <item><term>RepetitionMode.None</term><description>exactly one matching token required</description></item>
        ///  <item><term>RepetitionMode.Some</term><description>One ore more matching tokens required</description></item>
        ///  <item><term>RepetitionMode.Any</term><description>Zero or more matching tokens required</description></item>
        ///  <item><term>RepetitionMode.Maybe</term><description>Zero or one matching token required</description></item>
        /// </list>
        /// 
        /// Defaults to RepetitionMode.None.
        /// </summary>
        public RepetitionMode RepetitionMode = RepetitionMode.None;

        /// <summary>
        /// Configures, whether this node will "eat up" as much tokens
        /// as possible.
        /// Defaults to true.
        /// </summary>
        public bool Greedy = true;

        public TokenWhisperer<TokenType> tokenWhisperer;

        public virtual void FromString(string str)
        {
            Regex re = new Regex(@"([\+\*\?])?(\?)?$");

            Match match = re.Match(str);

            if (match.Groups[1].Value == "+")
            {
                this.RepetitionMode = RepetitionMode.Some;
            }
            else if (match.Groups[1].Value == "*")
            {
                this.RepetitionMode = RepetitionMode.Any;
            }
            else if (match.Groups[1].Value == "?")
            {
                this.RepetitionMode = RepetitionMode.Maybe;
            }
            else
            {
                this.RepetitionMode = RepetitionMode.None;
            }

            Greedy = (match.Groups[2].Value != "?");
        }

        public override string ToString()
        {
            string result = "";

            if (RepetitionMode == Expression.RepetitionMode.Some) result += "+";
            else if (RepetitionMode == Expression.RepetitionMode.Any) result += "*";
                
            result += (!Greedy ? "?" : "");

            return result;
        }
    }
}
