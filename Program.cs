using System;
using System.IO;
using System.Reflection;

namespace followthewhiterabbit
{

    class Program
    {
        static void Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = typeof(Program).Namespace + ".wordlist";

            string wordlist = "";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                wordlist = reader.ReadToEnd();
            }

            try
            {
                WordManager manager = new WordManager();
                manager.ReadWordsFrom(wordlist);
                using (AnagramFinder finder = new AnagramFinder(manager))
                {
                    finder.FindAnagrams("e4820b45d2277f3844eac66c903e84be",
                                                            "23170acc097c24edb98fc5488ab033fe",
                                                            "665e5bcb0c20062fe8abaaf4628bb154");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured: {0}", ex.Message);
                Console.WriteLine("Stack: {0}", ex.StackTrace);
            }

            Console.WriteLine("Enter any key to exit...");
            Console.ReadLine();
        }
    }
}
