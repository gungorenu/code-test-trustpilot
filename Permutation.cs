using System.Collections.Generic;
using System.Linq;

namespace followthewhiterabbit
{
    /// <summary>
    /// Simple permutation algorithm (from web)
    /// </summary>
    public class Permutation
    {
        private List<int[]> m_Results = new List<int[]>();
        public List<int[]> Results { get { return m_Results; } }

        public Permutation(int count)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < count; i++)
                list.Add(i);

            foreach( IEnumerable<int> set in GetPermutations(list,count)  )
            {
                m_Results.Add((new List<int>(set).ToArray()));
            }
        }

        private IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }
    }
}
