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
using GenRegEx.Vm;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GenRegEx
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Pattern<string> pattern = new Pattern<string>(new StringWhisperer());
            pattern.FromString("a+ b+");

            assertSuccess(pattern, "ab", 0, 1);
            assertSuccess(pattern, "cab", 1, 2);
            assertSuccess(pattern, "abc", 0, 1);
            assertSuccess(pattern, "cabc", 1, 2);
            assertSuccess(pattern, "caaabbbc", 1, 6);
            assertFail(pattern, "aaacbbb");
            assertFail(pattern, "cccc");


            pattern.FromString("^a+ b+");

            assertFail(pattern, "cab");
            assertSuccess(pattern, "ab", 0, 1);
            assertSuccess(pattern, "abc", 0, 1);
            assertSuccess(pattern, "aabc", 0, 2);

            pattern.FromString("a+ b+$");

            assertFail(pattern, "abc");
            assertSuccess(pattern, "ab", 0, 1);
            assertSuccess(pattern, "cab", 1, 2);


            pattern.FromString("^a+ b+$");

            assertFail(pattern, "cab");
            assertFail(pattern, "abc");
            assertFail(pattern, "cabc");
            assertSuccess(pattern, "ab", 0, 1);
            assertSuccess(pattern, "aabb", 0, 3);


            pattern.FromString("a* (b c)+ d");

            assertSuccess(pattern, "bcd", 0, 2);
            assertSuccess(pattern, "abcd", 0, 3);
            assertSuccess(pattern, "aabcd", 0, 4);
            assertSuccess(pattern, "abcbcd", 0, 5);
            assertFail(pattern, "aad");
            assertFail(pattern, "aabd");


            // Test greedy/not greedy
            pattern.FromString("a+ b+");
            assertSuccess(pattern, "aabbbcde", 0, 4);

            pattern.FromString("a+ b+?");
            assertSuccess(pattern, "aabbbcde", 0, 2);



            // Tests für First/Last/Longest Match
            pattern.FromString("a+ b+");
            assertSuccess(pattern, "aabcabb", 0, 2);



            // Test-Pattern für Performance-Test erzeugen

            testPerformanceGenRegEx();
            testPerformanceCSharp();

            Console.ReadLine();
        }

        static void testPerformanceCSharp()
        {
            int n = 1;

            while (true)
            {
                string patternString = "";
                string targetString = "";

                for (int i = 0; i < n; i++)
                {
                    patternString += "a?";
                    targetString += "a";
                }

                for (int i = 0; i < n; i++)
                {
                    patternString += "a";
                }

                Regex re = new Regex(patternString);

                Stopwatch watch = new Stopwatch();
                watch.Start();
                System.Text.RegularExpressions.Match match = re.Match(targetString);
                watch.Stop();
                Console.WriteLine("C-Sharp time (n=" + n + "): " + watch.Elapsed.ToString());

                if (watch.Elapsed.TotalSeconds > 10) break;

                n++;
            }
        }

        static void testPerformanceGenRegEx()
        {
            int n = 1;

            while (true)
            {
                string patternString = "";
                string targetString = "";

                for (int i = 0; i < n; i++)
                {
                    patternString += "a? ";
                    targetString += "a";
                }

                for (int i = 0; i < n - 1; i++)
                {
                    patternString += "a ";
                }

                patternString += "a";

                Pattern<string> pattern = Pattern<string>.FromString(patternString, new StringWhisperer());
                Processor<string> processor = Processor<string>.FromPattern(pattern);

                Stopwatch watch = new Stopwatch();
                watch.Start();

                for (int i = 0; i < targetString.Length; i++)
                {
                    if (!processor.ProcessToken(targetString[i].ToString()))
                    {
                        break;
                    }

                }

                processor.Finish();
                watch.Stop();
                Console.WriteLine("GenRegEx time (n=" + n + "): " + watch.Elapsed.ToString());

                if (watch.Elapsed.TotalSeconds > 0.001) break;

                n++;
            }

        }

        static void assertSuccess(Pattern<string> pattern, string haystack, int startIndex, int endIndex)
        {
            test(pattern, haystack, true, startIndex, endIndex);
        }

        static void assertFail(Pattern<string> pattern, string haystack)
        {
            test(pattern, haystack, false, -1, -1);
        }

        static void test(Pattern<string> pattern, string haystack, bool shouldMatch, int startIndex, int endIndex)
        {
            Processor<string> processor = Processor<string>.FromPattern(pattern);

            for (int i = 0; i < haystack.Length; i++)
            {
                if (!processor.ProcessToken(haystack[i].ToString()))
                {
                    break;
                }
                else if (shouldMatch && (i > endIndex))
                {
                    // Rückgabe hätte false sein sollen:
                    Console.WriteLine("FAIL (dind't stop after expected endIndex)");
                    return;
                }

            }

            if (processor.Finish() != shouldMatch)
            {
                if (shouldMatch)
                {
                    Console.WriteLine("FAIL (didn't match) looking for '" + pattern.ToString() + "' in '" + haystack + "'");
                }
                else
                {
                    Console.WriteLine("FAIL (wrong match) looking for '" + pattern.ToString() + "' in '" + haystack + "', found " + processor.lastMatch.StartIndex + " .. " + processor.lastMatch.EndIndex);
                }
            }
            else
            {
                if (!shouldMatch)
                {
                    Console.WriteLine("SUCCESS (no match)");
                }
                else if ((processor.lastMatch.StartIndex == startIndex) && (processor.lastMatch.EndIndex == endIndex))
                {

                    Console.WriteLine("SUCCESS ('" + pattern.ToString() + "' matched '" + haystack.Substring(startIndex, endIndex - startIndex + 1) + "' in '" + haystack + "')");
                }
                else
                {
                    Console.WriteLine("FAIL (indizes differ, should be " + startIndex + " .. " + endIndex + " was " + processor.lastMatch.StartIndex + " .. " + processor.lastMatch.EndIndex + " looking for '" + pattern.ToString() + "' in '" + haystack + "'");
                }
            }
        }
    }
}
