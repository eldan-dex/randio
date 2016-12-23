using System;

namespace Randio_2
{
    class StringHelper
    {
        #region Public methods
        public static string GenerateName()
        {
            string result = "";

            int words = AlgorithmHelper.BiasedRandom(1, 3, 1.8);
            int maxTotalLen = 12;
            int maxWordLen = 6;
            int minLen;
            for (int i = 0; i < words; ++i)
            {
                if (i == 0 || i == words - 1)
                    minLen = 2;
                else
                    minLen = AlgorithmHelper.GetRandom(0, 2) == 0 ? 1 : 3; //either 1 or 3 as min len for middle words

                int len = AlgorithmHelper.GetRandom(minLen, maxWordLen + 1);
                result += FirstLetterToUpper(GenerateWord(len)) + " ";

                //so that the last word is never a single letter
                if (words == 3 && result.Length > maxTotalLen - minLen)
                    words = 2;
            }
            return result.Substring(0, Math.Min(result.Length, maxTotalLen));
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
                        disabled = "gxzqus";

                    else if ((last == "r" || last == "l") && (next == "r" || next == "l"))
                        disabled = "rl";

                    else if (next == "l")
                        disabled = "r";

                    else
                        disabled = "";
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
