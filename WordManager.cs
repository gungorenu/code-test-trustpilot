using System.Collections.Generic;

namespace followthewhiterabbit
{
    /// <summary>
    /// Special manager class for handling string-code related transformation
    /// Groups every word into special entities which are anagrams on their own
    /// Ex: "part" and "trap" are anagrams of same word for the algorithm and they are both in english dictionary
    /// </summary>
    public class WordManager
    {
        #region Constants
        /* bit index of characters
        .. 76 5432 1098 7654 3210 ..bit indexes
           ai lnoo prss tttt uuyw
        */
        public const string Characters = "ailnooprssttttuuyw";
        public const int Max = 0x3ffff; // letter bits set
        public const int MaxPower = 18;
        #endregion

        public WordManager()
        {
            Words = new Dictionary<int, Word>();
        }


        #region Public Members
        public IDictionary<int, Word> Words { get; private set; }

        /// <summary>
        /// Reads every word from provided word list and adds into dictionary if possible
        /// </summary>
        /// <param name="wordlist">Word list</param>
        /// <returns>Count of available words</returns>
        public int ReadWordsFrom(string wordlist)
        {
            string[] english = wordlist.Split('\r', '\n');
            //string[] english = System.IO.File.ReadAllLines(file);
            int count = 0;
            foreach (string s in english)
            {
                if (AddWord(s))
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Adds a word node to word dictionary
        /// </summary>
        /// <param name="word">Word to add to dictionary</param>
        /// <returns>True if word is available, false otherwise</returns>
        public bool AddWord(string word)
        {
            int wordCode = CodeFromWord(word);
            if (wordCode == -1)
                return false;

            Word w = null;
            if (!Words.TryGetValue(wordCode, out w))
            {
                w = new Word(wordCode);
                Words.Add(wordCode, w);
            }

            w.Add(word);
            return true;
        }

        /// <summary>
        /// Finds words within a mask
        /// </summary>
        /// <param name="mask">mask as an int</param>
        /// <param name="maxLength">Max length that word could be</param>
        /// <returns>List of possible words</returns>
        public IEnumerable<int> FindWords(int mask, int maxLength, int minLength)
        {
            foreach (Word w in Words.Values)
            {
                if (w.Power > maxLength)
                    continue;
                if (w.Power < minLength)
                    continue;

                if (w.IsAvailable(mask))
                    yield return w.Code;
            }
        }

        #endregion

        #region Static Members

        /// <summary>
        /// Finds power of a word given as integer, counts bits
        /// </summary>
        /// <param name="code">Word code</param>
        /// <returns>Power of a word</returns>
        public static int PowerOfWordCode(int code)
        {
            int power = 0;
            for (int i = 0, c = 0x1; i < sizeof(int) * 8; i++)
            {
                if ((code & (c << i)) == (c << i))
                    ++power;
            }
            return power;
        }

        /// <summary>
        /// Calculates word from given bitset code
        /// </summary>
        /// <param name="code">Word code</param>
        /// <returns>Letters of the word</returns>
        public static string WordFromCode(int code)
        {
            string res = "";
            for (int i = 0, c = 0x1; i < sizeof(int) * 8; i++)
            {
                if ((code & (c << i)) == (c << i))
                    res += Characters[Characters.Length - (i + 1)];
                else
                    res += "_";
            }

            return res;
        }

        /// <summary>
        /// Gets the single bit set code for a letter
        /// Duplicated letters return the lowest bit set but 'dup' flag set
        /// </summary>
        /// <param name="c">Character</param>
        /// <param name="dup">Duplicated flag</param>
        /// <returns>Single bit set int or -1 if letter is unknown</returns>
        private static int CodeFromLetter(char c, out bool dup)
        {
            dup = false;
            // hardcoded for performance
            switch (c)
            {
                case 'a': return 0x1 << 17;
                case 'i': return 0x1 << 16;

                case 'l': return 0x1 << 15;
                case 'n': return 0x1 << 14;
                case 'o': dup = true; return 0x1 << 12; // 12,13

                case 'p': return 0x1 << 11;
                case 'r': return 0x1 << 10;
                case 's': dup = true; return 0x1 << 8; // 8,9

                case 't': dup = true; return 0x1 << 4; // 4,5,6,7

                case 'u': dup = true; return 0x1 << 2; // 2,3
                case 'y': return 0x1 << 1;
                case 'w': return 0x1 << 0;

                default: return -1;
            }
        }

        /// <summary>
        /// Creates a code from a word with lowests bits set (for duplicate letters)
        /// -1 if word cannot be made from the available characters
        /// </summary>
        /// <param name="word">Word</param>
        /// <returns>-1 or word code</returns>
        public static int CodeFromWord(string word)
        {
            string temp = word.Replace("\'", "").ToLower();

            int wordCode = 0x0;
            int available = Max;

            // hardcoded for performance

            int bit_o = 0;
            int bit_s = 0;
            int bit_t = 0;
            int bit_u = 0;

            for (int i = 0; i < temp.Length; i++)
            {
                char c = temp[i];
                bool dup = false;
                int charCode = CodeFromLetter(c, out dup);

                // our characters cannot make up this word, anagram failure
                if (charCode == -1) return -1;

                // letter is not available, duplicated but not available
                if ((available & charCode) != charCode)
                    return -1;

                if (!dup)
                    available = available & ~charCode;

                // for duplicated letters only
                if (dup)
                {
                    // even for duplicated chars we cannot make this word, we cannot make "sooon" for example
                    if ((bit_o == 2 && c == 'o') ||
                        (bit_s == 2 && c == 's') ||
                        (bit_t == 4 && c == 't') ||
                        (bit_u == 2 && c == 'u'))
                        return -1;

                    if (c == 'o')
                    {
                        charCode = charCode << bit_o;
                        bit_o++;
                    }
                    else if (c == 's')
                    {
                        charCode = charCode << bit_s;
                        bit_s++;
                    }
                    else if (c == 't')
                    {
                        charCode = charCode << bit_t;
                        bit_t++;
                    }
                    else if (c == 'u')
                    {
                        charCode = charCode << bit_u;
                        bit_u++;
                    }

                    if (bit_o == 2 || bit_s == 2 || bit_t == 4 || bit_u == 2)
                        available = available & ~charCode; // remove duplicated letter from available
                }

                wordCode = wordCode | charCode;
            }

            return wordCode;
        }

        /// <summary>
        /// Counts set bits at specified index and count
        /// </summary>
        /// <param name="code">Value to check</param>
        /// <param name="index">Start bit index</param>
        /// <param name="count">Bit count to check</param>
        /// <returns>Set bit count</returns>
        private static int CountOfBits(int code, int index, int count)
        {
            int res = 0;
            for (int i = 0, c = 0x1 << index; i < count; i++, c = c << 1)
                if ((code & c) == c)
                    ++res;
            return res;
        }

        /// <summary>
        /// Shifts partial bits of a code given
        /// </summary>
        /// <param name="code">Value to shift</param>
        /// <param name="index">start bit index</param>
        /// <param name="size">Max size of the block (more than 1 for duplicated letters)</param>
        /// <param name="count">Shifting count</param>
        /// <returns>The value with shifted bits</returns>
        private static int ShiftBits(int code, int index, int size, int count)
        {
            if (size == count)
                return code;

            int block = 0;
            for (int i = 0, c = 0x1; i < size; i++, index++)
                block = block | (c << index);

            int antiblock = ~block;
            int inc = code & block;
            int exc = code & antiblock;

            inc = inc >> count;

            return (code & exc) | inc;
        }

        /// <summary>
        /// Uses up the wordCode bits from the actual code and also lowers the remaining high letter bits from actual if duplicated letters are not all used
        /// </summary>
        /// <param name="actual">Actual code (available letter code)</param>
        /// <param name="wordCode">Parameter word code</param>
        /// <returns>Remaining letter bit code</returns>
        public static int SubtractCode(int actual, int wordCode)
        {
            // we cannot make up this word from the actual letters
            if ((actual & wordCode) != wordCode)
                return -1;

            int temp = actual & ~wordCode;
            // at this point temp contains the result but its duplicated letter bits are on highest bits so we need to make it lower now

            // shift for o
            int bitcount = CountOfBits(wordCode, 12, 2);
            temp = ShiftBits(temp, 12, 2, bitcount);

            // shift for s
            bitcount = CountOfBits(wordCode, 8, 2);
            temp = ShiftBits(temp, 8, 2, bitcount);

            // shift for t
            bitcount = CountOfBits(wordCode, 4, 4);
            temp = ShiftBits(temp, 4, 4, bitcount);

            // shift for u
            bitcount = CountOfBits(wordCode, 2, 2);
            temp = ShiftBits(temp, 2, 2, bitcount);

            return temp;
        }

        #endregion
    }
}
