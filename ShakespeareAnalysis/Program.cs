using System.Diagnostics;

namespace ShakespeareAnalysis
{
    internal class Program
    {
        /// <summary>
        /// Lets analyse the complete works of Shakespeare and get some interesting stastistics.
        /// </summary>
        /// <param name="args">The filepath to the complete works of Shakespeare</param>
        static void Main(string[] args)
        {
            if(args.Length == 0 || !File.Exists(args[0]))
            {
                Console.WriteLine("Please provide the full fill path to the complete works of Shakespeare in the command line arguments");
            }

            Stopwatch sw = new();
            sw.Start();

            using var file = File.OpenText(args[0]);
            Dictionary<string, int> wordCount = new();
            int totalWords = 0;
            // Allocate two buffers on the stack to read into, these are exactly 4KB each, the size of a windows file block.
            bool buff = false;
            Span<char> charBuff = stackalloc char[8192];
            Span<char> charBuffA = charBuff[..4096];
            Span<char> charBuffB = charBuff[4096..];
            int lastInd = 0;
            int nextChar = 0;

            while (!file.EndOfStream)
            {
                int charsRead = 0;
                if (buff)
                {
                    charsRead = file.ReadBlock(charBuffB);
                }
                else
                {
                    charsRead = file.ReadBlock(charBuffA);
                    nextChar = 0;
                }


                buff = !buff;
                while ((buff && nextChar < charsRead) || (!buff && nextChar < charBuffA.Length + charsRead))
                {
                    // Skip any whitespace
                    while (((buff && nextChar < charsRead) || (!buff && nextChar < charBuffA.Length + charsRead))
                        && char.IsWhiteSpace(charBuff[nextChar]))
                        nextChar++;

                    // Now find any words in the buffers
                    lastInd = nextChar;
                    while (((buff && nextChar < charsRead) || (!buff && nextChar < charBuffA.Length + charsRead))
                        && !char.IsWhiteSpace(charBuff[nextChar]))
                        nextChar++;

                    // We've reached white space, we must have found a word!
                    if (((buff && nextChar < charsRead) || (!buff && nextChar < charBuffA.Length + charsRead))
                        && (char.IsWhiteSpace(charBuff[nextChar]) || file.EndOfStream))
                    {
                        string word;
                        if (lastInd <= nextChar)
                            word = new(charBuff[lastInd..nextChar]);
                        else
                            word = new string(charBuff[lastInd..]) + new string(charBuff[..nextChar]);

                        word = word.ToLowerInvariant();
                        if (wordCount.ContainsKey(word))
                            wordCount[word] += 1;
                        else
                            wordCount.Add(word, 1);
                        totalWords++;

                        lastInd = nextChar;
                    }
                }
            }

            var wordCountArr = wordCount.ToArray();
            Array.Sort(wordCountArr, (x,y) => -x.Value.CompareTo(y.Value));

            sw.Stop();
            Console.WriteLine($"Parsed corpus in {sw.Elapsed}!");
            Console.WriteLine($"Found {totalWords} words");
            foreach (var kvp in wordCountArr.Take(100))
                Console.WriteLine($"\tWord: '{kvp.Key}' used {kvp.Value} times");
            Console.ReadKey();
        }
    }
}