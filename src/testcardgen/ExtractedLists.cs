using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace testcardgen
{
    internal static class Extractor
    {
        private static void ReadFileIntoList(ref List<string> list, string path)
        {
            foreach (string line in File.ReadAllLines(path))
            {
                list.Add(line);
            }
        }

        internal static ExtractedLists ExtractLists(string path, string extension)
        {
            Dictionary<string, int> frequencyMap = new Dictionary<string, int>();

            ManagedDango dango = new ManagedDango();

            List<string> DAIAZUMANGA = new List<string>();

            DirectoryInfo d = new DirectoryInfo(path);

            foreach (var file in d.GetFiles(extension))
            {
                ReadFileIntoList(ref DAIAZUMANGA, file.FullName);
            }

            string[] azumanga = DAIAZUMANGA.ToArray();

            Regex reg = new Regex(@"[\u3000-\u303F]|[\u3040-\u309F]|[\u30A0-\u30FF]|[\uFF00-\uFFEF]|[\u4E00-\u9FAF]|[\u2605-\u2606]|[\u2190-\u2195]|\u203B", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string output = "";

            using (var stream = new MemoryStream())
            {
                var sw = new StreamWriter(stream);

                foreach (string line in azumanga)
                {
                    if (line == "") continue;

                    MatchCollection matches = reg.Matches(line);

                    if (!matches.Any()) continue;

                    foreach (Match match in matches)
                    {
                        sw.Write(match.Value);
                        sw.Flush();
                    }

                    sw.Write("\n");
                    sw.Flush();
                }

                output = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }

            foreach (string line in output.Split("\n"))
            {
                if (line == "") continue;
                foreach (Word word in dango.Tokenize(line))
                {
                    if (word.PartOfSpeech == "PARTICLE" || word.PartOfSpeech == "NAME" || word.PartOfSpeech == "SYMBOL" || word.PartOfSpeech == "INTERJECTION") continue;

                    if (!frequencyMap.ContainsKey(word.DictionaryForm))
                    {
                        frequencyMap.Add(word.DictionaryForm, 1);
                    }
                    else
                    {
                        frequencyMap[word.DictionaryForm]++;
                    }
                }
            }

            Dictionary<string, int> kanjiFrequency = new Dictionary<string, int>();

            foreach (char entry in output)
            {
                if (entry < 19968 || entry > 40879) continue;

                if (!kanjiFrequency.ContainsKey(entry.ToString()))
                {
                    kanjiFrequency.Add(entry.ToString(), 1);
                }
                else
                {
                    kanjiFrequency[entry.ToString()]++;
                }
            }

            ExtractedLists final = new ExtractedLists();
            final.KanjiFrequency = kanjiFrequency;
            final.WordFrequency = frequencyMap;

            List<string> sentences = new List<string>();

            foreach(string sentence in output.Split("\n"))
            {
                sentences.Add(sentence);
            }

            final.Sentences = sentences;

            return final;
        }
    }
    internal class ExtractedLists
    {
        internal Dictionary<string, int> KanjiFrequency { get; set; }
        internal Dictionary<string, int> WordFrequency { get; set; }
        internal List<string> Sentences { get; set; }
    }
}
