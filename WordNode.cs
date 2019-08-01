using System.Collections.Generic;

namespace followthewhiterabbit
{
    /// <summary>
    /// Model class for representing a word in an anagram sentence like a tree
    /// it is a sub-anagram on its own
    /// </summary>
    public struct WordNode
    {
        public Word WordRef { get; private set; }
        public IDictionary<int, WordNode> ChildNodes { get; private set; }

        public WordNode(Word wordRef)
        {
            WordRef = wordRef;
            ChildNodes = new Dictionary<int, WordNode> ();
        }

    }
}
