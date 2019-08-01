using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace followthewhiterabbit
{

    /// <summary>
    /// Special operation class that finds anagram phrases
    /// </summary>
    public class AnagramFinder : IDisposable
    {
        private MD5 md5;
        private HashSet<string> resultsToLookFor;
        private WordManager manager;

        public AnagramFinder(WordManager words, params string[] hashes)
        {
            manager = words;
            md5 = MD5.Create();
        }

        /// <summary>
        /// Finds anagrams that produce the given hashes with the hardcoded phrase 
        /// </summary>
        /// <param name="hashes">Hashes to look for</param>
        public void FindAnagrams(params string[] hashes)
        {
            resultsToLookFor = new HashSet<string>();
            foreach (string hash in hashes)
            {
                resultsToLookFor.Add(hash);
            }

            Stopwatch sw = new Stopwatch();
            try
            {
                FindAnagramResult_2W(sw);

                sw.Stop();

                FindAnagramResult_3W(sw);

                sw.Stop();

                FindAnagramResult_4W(sw);

            }
            catch (Exception ex)
            {
                if (ex.Message != "BREAK-LOOPS")
                    throw ex;
                else
                    Console.WriteLine();
            }

            if (resultsToLookFor.Count == 0)
            {
                Console.WriteLine("All answers are found so stopped pushing through, would run around for 5 minutes otherwise...");
            }
        }
        private void FindAnagramResult_2W(Stopwatch sw)
        {
            IDictionary<int, WordNode> combinations = new Dictionary<int, WordNode>();

            try
            {
                sw.Start();
                Console.WriteLine("Looking at 2 word anagrams...");

                // step 1, pick A
                foreach (Word word in manager.Words.Values)
                {
                    // picking A, 9+ only
                    if (word.Power < 9)
                        continue;

                    combinations.Add(word.Code, new WordNode(word));
                }

                // step 2, pick B
                foreach (KeyValuePair<int, WordNode> wordInfo in combinations)
                {
                    int max = WordManager.Max;
                    // current holds B
                    int current = WordManager.SubtractCode(max, wordInfo.Key);

                    if (!manager.Words.ContainsKey(current))
                        continue;
                    wordInfo.Value.ChildNodes.Add(current, new WordNode(manager.Words[current]));
                }

                Permutation permof2Words = new Permutation(2);

                // brute force build sentences, too lazy to check in a sophisticated manner as remaining result set is not huge
                foreach (WordNode nodeA in combinations.Values)
                {
                    foreach (WordNode nodeB in nodeA.ChildNodes.Values)
                    {
                        foreach (string wordA in nodeA.WordRef.Words)
                        {
                            foreach (string wordB in nodeB.WordRef.Words)
                            {
                                // build sentences and get hashcode
                                CalculateHashOfSentencePermutations(permof2Words, wordA, wordB);
                            }
                        }
                    }
                }
            }
            finally
            {
                Console.WriteLine("Looking at 2 word anagrams... took {0}", sw.Elapsed.ToString());
            }
        }
        private void FindAnagramResult_3W(Stopwatch sw)
        {
            IDictionary<int, WordNode> combinations = new Dictionary<int, WordNode>();

            try
            {
                sw.Start();
                Console.WriteLine("Looking at 3 word anagrams...");

                // step 1, pick A
                foreach (Word word in manager.Words.Values)
                {
                    // picking A, 6+ only
                    if (word.Power < 6)
                        continue;

                    combinations.Add(word.Code, new WordNode(word));
                }

                // step 2, pick B
                foreach (KeyValuePair<int, WordNode> wordInfo in combinations)
                {
                    int max = WordManager.Max;
                    // current holds potential of B and C
                    int current = WordManager.SubtractCode(max, wordInfo.Key);
                    int powerofA = WordManager.PowerOfWordCode(wordInfo.Key);
                    int maxpowerofB = Math.Min(powerofA, max - powerofA);
                    int minpowerofB = Convert.ToInt32(Math.Ceiling(((float)(WordManager.MaxPower - powerofA)) / 2));

                    foreach (int possibleB in manager.FindWords(current, maxpowerofB, minpowerofB))
                    {
                        wordInfo.Value.ChildNodes.Add(possibleB, new WordNode(manager.Words[possibleB]));
                    }
                }

                // step 3.a, filter out invalid combinations of A+B, A+B left no possible C, check will allow possible A+B without a C
                foreach (int wordA in combinations.Keys)
                {
                    WordNode possibleB = combinations[wordA];
                    foreach (int wordB in possibleB.ChildNodes.Keys.ToArray())
                    {
                        int max = WordManager.Max;
                        int temp = WordManager.SubtractCode(max, wordA);
                        temp = WordManager.SubtractCode(temp, wordB);

                        if (!manager.Words.ContainsKey(temp))
                            possibleB.ChildNodes.Remove(wordB);

                        int powerA = WordManager.PowerOfWordCode(wordA);
                        int powerB = WordManager.PowerOfWordCode(wordB);
                        int powerC = WordManager.PowerOfWordCode(temp);

                        // if total length is not 18 then we missed letters in anagram, lets fail with exc
                        if (powerA + powerB + powerC != WordManager.MaxPower)
                            throw new InvalidOperationException("Algorithm has a length bug! Words A B C cannot make 18 characters");
                    }
                }

                // step 3.b, filter out invalid combinations of A+B, A has no B because it used all good letters perhaps
                foreach (int key in combinations.Keys.ToArray())
                {
                    if (combinations[key].ChildNodes.Count == 0)
                        combinations.Remove(key);
                }

                Permutation permof3Words = new Permutation(3);

                // brute force build sentences, too lazy to check in a sophisticated manner as remaining result set is not huge
                foreach (WordNode nodeA in combinations.Values)
                {
                    foreach (WordNode nodeB in nodeA.ChildNodes.Values)
                    {
                        int max = WordManager.Max;
                        int codeC = WordManager.SubtractCode(max, nodeA.WordRef.Code);
                        codeC = WordManager.SubtractCode(codeC, nodeB.WordRef.Code);

                        foreach (string wordA in nodeA.WordRef.Words)
                        {
                            foreach (string wordB in nodeB.WordRef.Words)
                            {
                                foreach (string wordC in manager.Words[codeC].Words)
                                {
                                    // build sentences and get hashcode
                                    CalculateHashOfSentencePermutations(permof3Words, wordA, wordB, wordC);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Console.WriteLine("Looking at 3 word anagrams... took {0}", sw.Elapsed.ToString());
            }
        }

        private void FindAnagramResult_4W(Stopwatch sw)
        {
            IDictionary<int, WordNode> combinations = new Dictionary<int, WordNode>();

            try
            {
                sw.Start();
                Console.WriteLine("Looking at 4 word anagrams...");

                // step 1, pick A
                foreach (Word word in manager.Words.Values)
                {
                    // picking A, 5+ only
                    if (word.Power < 5)
                        continue;

                    combinations.Add(word.Code, new WordNode(word));
                }

                // step 2, pick B for every A
                foreach (WordNode nodeA in combinations.Values)
                {
                    int max = WordManager.Max;
                    // current holds potential of B+C+D
                    int current = WordManager.SubtractCode(max, nodeA.WordRef.Code);

                    int powerofA = WordManager.PowerOfWordCode(nodeA.WordRef.Code);
                    int maxpowerofB = Math.Min(powerofA, WordManager.MaxPower - powerofA);
                    int minpowerofB = Convert.ToInt32(Math.Ceiling(((float)(WordManager.MaxPower - powerofA)) / 3));

                    foreach (int possibleB in manager.FindWords(current, maxpowerofB, minpowerofB))
                    {
                        nodeA.ChildNodes.Add(possibleB, new WordNode(manager.Words[possibleB]));
                    }
                }

                // step 3, pick C for every A + B
                foreach (WordNode nodeA in combinations.Values)
                {
                    foreach (WordNode nodeB in nodeA.ChildNodes.Values)
                    {
                        int max = WordManager.Max;
                        // current holds potential of C+D
                        int current = WordManager.SubtractCode(max, nodeA.WordRef.Code);
                        current = WordManager.SubtractCode(current, nodeB.WordRef.Code);
                        int currentpower = WordManager.PowerOfWordCode(current);

                        int powerofB = WordManager.PowerOfWordCode(nodeB.WordRef.Code);
                        int maxpowerofC = Math.Min(powerofB, WordManager.MaxPower - powerofB);
                        int minpowerofC = Convert.ToInt32(Math.Ceiling(((float)currentpower) / 2));

                        foreach (int possibleC in manager.FindWords(current, maxpowerofC, minpowerofC))
                        {
                            // prevents 3W sentences
                            if (possibleC == current)
                                continue;
                            nodeB.ChildNodes.Add(possibleC, new WordNode(manager.Words[possibleC]));
                        }
                    }
                }

                Permutation permof4Words = new Permutation(4);

                // brute force 4 word sentence hash checking
                foreach (WordNode nodeA in combinations.Values)
                {
                    foreach (WordNode nodeB in nodeA.ChildNodes.Values)
                    {
                        foreach (WordNode nodeC in nodeB.ChildNodes.Values)
                        {
                            int temp = WordManager.Max;
                            temp = WordManager.SubtractCode(temp, nodeA.WordRef.Code);
                            temp = WordManager.SubtractCode(temp, nodeB.WordRef.Code);
                            temp = WordManager.SubtractCode(temp, nodeC.WordRef.Code);

                            // not interested in 3W 
                            if (temp == 0)
                                continue;

                            // A+B+C left no D available
                            if (!manager.Words.ContainsKey(temp))
                                continue;

                            foreach (string wordA in nodeA.WordRef.Words)
                            {
                                foreach (string wordB in nodeB.WordRef.Words)
                                {
                                    foreach (string wordC in nodeC.WordRef.Words)
                                    {
                                        foreach (string wordD in manager.Words[temp].Words)
                                        {
                                            CalculateHashOfSentencePermutations(permof4Words, wordA, wordB, wordC, wordD);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Console.WriteLine("Looking at 4 word anagrams... took {0}", sw.Elapsed.ToString());
            }
        }

        private string CalculateHash(string sentence)
        {
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(sentence);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            string value = sb.ToString().ToLower();

            if (resultsToLookFor.Contains(value))
            {
                Console.WriteLine(" # Found answer: {0} : \"{1}\"", value, sentence);
                resultsToLookFor.Remove(value);
                if (resultsToLookFor.Count == 0)
                    throw new Exception("BREAK-LOOPS");
            }

            return value;
        }

        private void CalculateHashOfSentencePermutations(Permutation perm, params string[] words)
        {
            foreach (int[] permRes in perm.Results)
            {
                string sentenceFormat = "";
                foreach (int e in permRes)
                {
                    sentenceFormat = string.Format("{1} {{{0}}}", e, sentenceFormat);
                }
                string sentence = string.Format(sentenceFormat.Trim(), words);
                CalculateHash(sentence);
            }
        }

        void IDisposable.Dispose()
        {
            md5.Dispose();
        }
    }
}
