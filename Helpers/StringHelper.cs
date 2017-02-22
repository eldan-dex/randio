using System;
using System.Collections.Generic;

namespace Randio_2
{
    class StringHelper
    {
        static List<string> knownNames = new List<string>();

        #region Public methods
        public static void Reset()
        {
            knownNames = new List<string>();
        }

        public static string GenerateName(int words = -1)
        {
            string result = "";
            if (words == -1)
                words = AlgorithmHelper.GetRandom(1, 3); //1 or 2 words
            int maxTotalLen = 8;
            int remaining = maxTotalLen;
            for (int i = 0; i < words; ++i)
            {
                int len = AlgorithmHelper.GetRandom(3, Math.Max(3, Math.Min(remaining, 9)));
                string word = FirstLetterToUpper(GenerateWord(len));

                result += word + " ";
                remaining -= len;
            }

            var name = result.TrimEnd();
            if (!knownNames.Contains(name))
            {
                knownNames.Add(name);
                return name;
            }

            //Recursion, because we need to generate again if last name was already used
            return GenerateName();
        }

        public static string GenerateWord(int len)
        {
            string result = "";
            string[] syllables = new string[] { "e", "a", "o", "i", "u", "y", "r", "l" };
            string[] consonants = new string[] { "t", "n", "s", "h", "r", "d", "l", "c", "m", "w", "f", "g", "p", "b", "v", "k", "j", "x", "z", "qu" };
            string disabled = "";
            bool lastType = AlgorithmHelper.GetRandom(0, 2) == 0;
            string next = "?";

            for (int i = 0; i < len; ++i)
            {
                lastType = !lastType;
                string last = next;
                if (lastType)
                {
                    do { 
                        next = syllables[AlgorithmHelper.BiasedRandom(0, syllables.Length - 1, 1.6)];
                    }
                    while (disabled.Contains(next));


                    //prevent weird sounding combinations
                    if (next == "r" || next == "l")
                        disabled = "gxzqusj";
                    else
                        disabled = "";

                    if ((last == "r" || last == "l") && (next == "r" || next == "l"))
                        disabled += "rl";

                    if (next == "l")
                        disabled += "r";
                }
                else
                {
                    do
                    {
                        next = consonants[AlgorithmHelper.BiasedRandom(0, consonants.Length - 1, 1.5)];
                    }
                    while (disabled.Contains(next));


                    if (next == "r")
                        disabled = "rl";
                    else if (next == "l")
                        disabled = "r";
                    else if (next == "w")
                        disabled = "l";
                    else
                        disabled = "";

                    if (i == len - 2) //last letter
                        disabled += "w";
                }
                result += next;

                //keep the correct length even when adding multiple letters
                if (next.Length > 1)
                    i += next.Length - 1;
            }

            return result;
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
        #endregion
    }
}
