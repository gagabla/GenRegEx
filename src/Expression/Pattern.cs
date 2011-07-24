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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GenRegEx.Expression
{
    public class Pattern<TokenType> : Group<TokenType>,  IXmlSerializable
    {
        public static Pattern<TokenType> FromString(string str, TokenWhisperer<TokenType> tokenWhisperer)
        {
            Pattern<TokenType> pattern = new Pattern<TokenType>(tokenWhisperer);
            pattern.FromString(str);

            return pattern;
        }

        /// <summary>
        /// Configures, whether matching sequence of tokens can start anywhere, or whether it has to
        /// start at the beginning.
        /// 
        /// Defaults to false.
        /// </summary>
        public bool MatchFromStart = false;

        /// <summary>
        /// Configures, whether matching sequence of tokens can end anywhere, or whether it has to
        /// end with the last token.
        /// 
        /// Defaults to false.
        /// </summary>
        public bool MatchTillEnd = false;

        public Pattern(TokenWhisperer<TokenType> tokenWhisperer):base(tokenWhisperer)
        {
            //
        }


        public override void FromString(string str)
        {
            if (str.StartsWith("^"))
            {
                MatchFromStart = true;
                str = str.Substring(1);
            }
            else
            {
                MatchFromStart = false;
            }

            if (str.EndsWith("$"))
            {
                MatchTillEnd = true;
                str = str.Substring(0, str.Length - 1);
            }
            else
            {
                MatchTillEnd = false;
            }

            if (!(str.StartsWith("(") && str.EndsWith(")")))
            {
                str = "(" + str + ")";
            }

            base.FromString(str);
        }

        public override string ToString()
        {
            string str = base.ToString();

            if (str.StartsWith("(") && str.EndsWith(")"))
            {
                str = str.Substring(1, str.Length - 2);
            }

            return (MatchFromStart ? "^" : "") + str + (MatchTillEnd ? "$" : "");
        }

        public void ReadXml(XmlReader reader)
        {
            reader.Read();

            string value = reader.Value;

            
            FromString(value);
        }

        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(this.ToString());
        }
    }
}
