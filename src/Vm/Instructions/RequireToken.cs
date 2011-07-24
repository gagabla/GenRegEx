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

namespace GenRegEx.Vm.Instructions
{
    public class RequireToken<TokenType> : Instruction
    {
        public TokenType Value;
        public bool AcceptAny = false;
        public bool IsEnd = false;

        public RequireToken(TokenType value)
        {
            this.Value = value;
        }

        public RequireToken()
        {
            //
        }

        public override void Print()
        {
            if (AcceptAny)
            {
                Console.WriteLine("Instruction Require *");
            }
            else if (IsEnd)
            {
                Console.WriteLine("Instruction Require -- end --");
            }
            else
            {
                Console.WriteLine("Instruction Require '" + Value.ToString() + "'");
            }
        }
    }
}
