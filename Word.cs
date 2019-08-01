using System.Collections.Generic;

namespace followthewhiterabbit
{
    /// <summary>
    /// Model class for holding dictionary elements
    /// Power: length of the word without special characters, count of letters used actually...
    /// </summary>
    public class Word
    {
        public int Code { get; private set; }
        public int Power { get; private set; }
        public IList<string> Words { get; private set; }

        public Word(int code)
        {
            Code = code;
            Power = WordManager.PowerOfWordCode(code);
        }

        public void Add(string word)
        {
            if (Words == null)
                Words = new List<string>();

            if (!Words.Contains(word))
                Words.Add(word);
        }

        public bool IsAvailable(int wordMask)
        {
            return ((wordMask & Code) == Code);
        }
    }
}
