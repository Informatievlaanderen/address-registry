namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System;

    public class FuzzyMatch
    {
        /// <summary>
        /// Calculates the similarity between two strings based on a weighted average of the Dice Coefficient,
        /// the Levensthein Edit Distance and the Longest Common Subsequence algorithms.
        /// </summary>
        /// <param name="pStr1">First string to compare</param>
        /// <param name="pStr2">Second string to compare</param>
        /// <returns>A similarity percentage</returns>
        public static double Calculate(String pStr1, String pStr2)
        {
            if (string.IsNullOrEmpty(pStr1) || string.IsNullOrEmpty(pStr1))
                return 0.0;

            pStr1 = pStr1.ToUpper();
            pStr2 = pStr2.ToUpper();

            double dice = Math.Min(FuzzyMatchAlgorithms.DiceCoefficient(pStr1, pStr2), 1.00);
            double LED = FuzzyMatchAlgorithms.LevenshteinEditDistance(pStr1, pStr2);
            double LCS = FuzzyMatchAlgorithms.LongestCommonSubsequence(pStr1, pStr2);

            double similarity = (3 * dice + LED / 0.8 + LCS / 0.8) / 5.5;
            return similarity * 100;
        }

        class FuzzyMatchAlgorithms
        {

            /// <summary>
            /// Calculates the Dice Coefficient for two given strings.
            /// </summary>
            /// <remarks>This is a similarity metric based on bigrams that is calculated as follows:
            /// * D is Dice coefficient
            /// * SB is Shared Bigrams
            /// * TBg1 is total number of bigrams in Qgram1
            /// * TBg2 is total number of bigrams in Qgram2
            /// * D = (2SB)/(TBg1+TBg2)
            /// A good Dice Coefficient value would be a value greater than 0.33</remarks>
            /// <param name="pStr1">First string to compare</param>
            /// <param name="pStr2">Second string to compare</param>
            /// <returns>Dice Coefficient</returns>
            internal static double DiceCoefficient(String pStr1, String pStr2)
            {
                // faulty input parameters
                if (string.IsNullOrEmpty(pStr1) || string.IsNullOrEmpty(pStr1))
                    return 0.0;

                var bigram1 = Bigram.Parse(pStr1);
                var bigram2 = Bigram.Parse(pStr2);

                // calculate number of shared bigrams
                int sharedBigrams = 0;
                foreach (var s1 in bigram1)
                {
                    foreach (var s2 in bigram2)
                    {
                        if (s1.Equals(s2))
                            sharedBigrams++;
                    }
                }

                // calculate dice coefficient
                double dice = Convert.ToDouble(sharedBigrams * 2) / Convert.ToDouble(bigram1.Length + bigram2.Length);
                return dice;
            }

            /// <summary>
            /// Calculates the Levenshtein Edit Distance for two given strings.
            /// This is the number of insertions, deletions or replacements of
            /// a single character or transposition of two characters that are
            /// required to convert one string to the other.
            /// </summary>
            /// <remarks>Each cost is given a weight of 1. The LED is meaningless
            /// without evaluating the lengths with the two comparison strings as well.</remarks>
            /// <param name="pStr1">First string to compare</param>
            /// <param name="pStr2">Second string to compare</param>
            /// <returns>Levenshtein Edit Distance divided by the length of the longest strings</returns>
            internal static double LevenshteinEditDistance(String pStr1, String pStr2)
            {
                // faulty input parameters
                if (string.IsNullOrEmpty(pStr1) || string.IsNullOrEmpty(pStr1))
                    return 0.0;

                char[] string1 = pStr1.ToCharArray();
                char[] string2 = pStr2.ToCharArray();

                // initilize the matrix
                int n = string1.Length;
                int m = string2.Length;
                int[,] matrix = new int[n + 1, m + 1];
                for (int i = 0; i < n + 1; i++)
                    matrix[i, 0] = i;
                for (int j = 0; j < m + 1; j++)
                    matrix[0, j] = j;

                // cost calculation
                for (int i = 1; i < n + 1; i++)
                {
                    for (int j = 1; j < m + 1; j++)
                    {
                        int cost;
                        if (!string1[i - 1].Equals(string2[j - 1]))
                            cost = 1;
                        else
                            cost = 0;
                        int above = matrix[i - 1, j];
                        int left = matrix[i, j - 1];
                        int diag = matrix[i - 1, j - 1];
                        int cell = Math.Min(Math.Min(above + 1, left + 1), diag + cost);
                        if (i > 1 && j > 1)
                        {
                            int trans = matrix[i - 2, j - 2] + 1;
                            if (!string1[i - 2].Equals(string2[j - 1]))
                                trans++;
                            if (!string1[i - 1].Equals(string2[j - 2]))
                                trans++;
                            if (cell > trans)
                                cell = trans;
                        }
                        matrix[i, j] = cell;
                    }
                }
                double result = 1.0 - Convert.ToDouble(matrix[n, m]) / Convert.ToDouble(Math.Max(pStr1.Length, pStr2.Length));

                return result;
            }

            /// <summary>
            /// Calculates the Longest Common Subsequence for two given strings.
            /// </summary>
            /// <remarks>The LCS is calculated by the length of the longest common,
            /// not necessarily contiguous, sub-sequence of characters
            /// divided by the average character lengths of both strings.
            /// In this case:  c(m, n) / (((Str1Len) + (Str2Len)) / 2))
            /// A good value for LCS is greater than or equal to 0.8.
            ///</remarks>
            /// <param name="pStr1">First string to compare</param>
            /// <param name="pStr2">Second string to compare</param>
            /// <returns>Longest Common Subsequence length divided by the average lenght of the two input strings</returns>
            internal static double LongestCommonSubsequence(String pStr1, String pStr2)
            {
                // faulty input parameters
                if (string.IsNullOrEmpty(pStr1) || string.IsNullOrEmpty(pStr1))
                    return 0.0;

                char[] X;
                char[] Y;

                if (pStr1.Length > pStr2.Length)
                {
                    X = pStr1.ToCharArray();
                    Y = pStr2.ToCharArray();
                }
                else
                {
                    X = pStr2.ToCharArray();
                    Y = pStr1.ToCharArray();
                }

                int n = Math.Min(X.Length, Y.Length);
                int m = Math.Max(X.Length, Y.Length);

                int[,] c = new int[m + 1, m + 1];
                int[,] b = new int[m + 1, m + 1];

                for (int i = 1; i < m + 1; i++)
                {
                    for (int j = 1; j < n + 1; j++)
                    {
                        if (X[i - 1] == Y[j - 1])
                        {
                            c[i, j] = c[i - 1, j - 1] + 1;
                            b[i, j] = 1; // from north west
                        }
                        else if (c[i - 1, j] >= c[i, j - 1])
                        {
                            c[i, j] = c[i - 1, j];
                            b[i, j] = 2; // from north
                        }
                        else
                        {
                            c[i, j] = c[i, j - 1];
                            b[i, j] = 3; // from west
                        }
                    }
                }

                if (c[m, n] > 0)
                    return Convert.ToDouble(c[m, n] / Convert.ToDouble((X.Length + Y.Length) / 2));

                return 0.00;
            }
        }

        class Bigram
        {
            /// <summary>
            /// Breaks pStr into bigram segments.
            /// <example>zantac - %z za an nt ta ac c#</example>
            /// </summary>
            /// <param name="pStr">the string to break into bigram segments</param>
            /// <returns>an array of bigrams</returns>
            public static String[] Parse(String pStr)
            {
                if (pStr == null)
                    return null;

                pStr = "%" + pStr + "#";
                String[] parsed = new String[pStr.Length - 1];
                for (int i = 0; i < pStr.Length - 1; i++)
                {
                    parsed[i] = pStr.Substring(i, 2);
                }
                return parsed;
            }
        }
    }
}
