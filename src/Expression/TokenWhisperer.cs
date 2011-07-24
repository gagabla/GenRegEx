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

namespace GenRegEx.Expression
{
    /// <summary>
    /// The methods described in this interface should match the following specification:
    /// 
    /// Equal(Token, FromString(ToString(Token))) = true
    /// </summary>
    /// <typeparam name="TokenType">Type of the tokens that the TokenWhisperer can handle</typeparam>
    public interface TokenWhisperer<TokenType>
    {
        /// <summary>
        /// Parses a string and returns an appropriate token-instance
        /// </summary>
        /// <param name="tokenString">The string representing a token</param>
        /// <returns>A token as described by the string</returns>
        TokenType FromString(string tokenString);

        /// <summary>
        /// Renders the given token as a string, such that it can be parsed to an equal token later on
        /// </summary>
        /// <param name="token">The token that should be represented as a string</param>
        /// <returns>The string representing the token</returns>
        string ToString(TokenType token);

        /// <summary>
        /// Tests whether two tokens are euqal, returns true if this is the case, otherwise
        /// returns false.
        /// </summary>
        /// <param name="token1">The first token to be compared</param>
        /// <param name="token2">The second token to be compared</param>
        /// <returns>true if the tokens are equal, otherwise false</returns>
        bool Equal(TokenType token1, TokenType token2);
    }
}
