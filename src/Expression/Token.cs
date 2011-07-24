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
using System.Xml.Serialization;

namespace GenRegEx.Expression
{
    public class Token<TokenType> : Node<TokenType>
    {
        /// <summary>
        /// Value of this token, depending of the type, this could be
        /// any value, primitive or object.
        /// </summary>
        public TokenType Value;

        /// <summary>
        /// If no specific value is required, but there must be "something"
        /// set this to true.
        /// </summary>
        public bool AcceptAny = false;

        public static Token<TokenType> AnyToken(TokenWhisperer<TokenType> tokenWhisperer)
        {
            Token<TokenType> result = new Token<TokenType>();
            result.AcceptAny = true;
            result.tokenWhisperer = tokenWhisperer;
            result.Value = default(TokenType);

            return result;
        }

        public Token()
        {
            //
        }

        public Token(TokenWhisperer<TokenType> tokenWhisperer)
        {
            this.tokenWhisperer = tokenWhisperer;
        }

        public Token(TokenType value)
        {
            this.Value = value;
        }

        public Token(TokenType value, RepetitionMode repetition)
        {
            this.Value = value;
            this.RepetitionMode = repetition;
        }

        public override void FromString(string str)
        {
            base.FromString(str);

            str = str.TrimEnd(new char[] { '+', '*', '?' });

            if (str == ".")
            {
                AcceptAny = true;
            }
            else
            {
                if (tokenWhisperer == null)
                {
                    throw new Exception("You have to provide an tokenWhisperer to this token instance, to use the FromString method");
                }

                this.Value = tokenWhisperer.FromString(str);
            }
        }

        override public string ToString()
        {
            if (tokenWhisperer == null)
            {
                throw new Exception("You have to provide an tokenWhisperer to this token instance, to use the ToString method");
            }

            return (AcceptAny ? "." : tokenWhisperer.ToString(Value) + base.ToString());
        }
    }
}
