using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                foreach (DangoWord word in dango.Tokenize(line))
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

            Dictionary<string, int> sortedWords = new Dictionary<string, int>();

            foreach (var k in frequencyMap.OrderByDescending(x => x.Value))
            {
                sortedWords.Add(k.Key, k.Value);
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

            Dictionary<string, int> sortedKanji = new Dictionary<string, int>();

            foreach (var k in kanjiFrequency.OrderByDescending(x => x.Value))
            {
                sortedKanji.Add(k.Key, k.Value);
            }

            ExtractedLists final = new ExtractedLists();
            final.KanjiFrequency = sortedKanji;
            final.WordFrequency = sortedWords;

            List<string> sentences = new List<string>();

            foreach(string sentence in output.Split("\n"))
            {
                if (!sentences.Contains(sentence) && sentence.Length > 8)
                {
                    sentences.Add(sentence);
                }
            }

            string lmao = "";
            foreach (string s in sentences)
            {
                lmao += s + "\n";
            }
            File.WriteAllText("../../../gpt2-japanese/sentences.txt", lmao);
            PythonEngine.Shutdown();

            Process cmd = new Process();
            cmd.StartInfo.FileName = "python.exe";
            cmd.StartInfo.Arguments = ".\\score.py sentences.txt";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.WorkingDirectory = @"..\..\..\gpt2-japanese";
            cmd.Start();

            StreamReader sr = cmd.StandardOutput;
            string mlOutput = sr.ReadToEnd();
            cmd.WaitForExit();
            
            File.WriteAllText("scoredSentences.txt", mlOutput);

			string[] scoredSentencesRaw = File.ReadAllLines("scoredSentences.txt");

            Dictionary<string, double> scoredSentencesUnsorted = new Dictionary<string, double>();

            foreach (string s in scoredSentencesRaw)
            {
                if (!scoredSentencesUnsorted.ContainsKey(s.Split("\t-")[0]))
                {
					string key = s.Split("\t-")[0];

					// After parsing the double, value = logprob/sqrt(len) where len is the raw unicode character count
					double val = double.Parse(s.Split("\t-")[1]) / (double)Math.Sqrt(key.Length);

                    // take the raw score and add an offset depending on how unique the characters occuring in the sentence are
                    // very repetitive sentence should end up having a negative score
                    List<char> uniqueOccurances = new List<char>();
                    foreach (char c in key)
                    {
                        if(!uniqueOccurances.Contains(c)) uniqueOccurances.Add(c);
                    }
                    if ((double)uniqueOccurances.Count / key.Length < 0.3)
                    {
                        val -= 100;
                    }

					scoredSentencesUnsorted.Add(key, val);
				}
            }

            Dictionary<string, double> sortedRanked = new Dictionary<string, double>();

            foreach (var k in scoredSentencesUnsorted.OrderByDescending(x => -x.Value))
            {
                sortedRanked.Add(k.Key, k.Value);
            }

            final.RankedSentences = sortedRanked;

            return final;
        }

		public static void GenerateLists(string path, string extension)
		{
			string title = path.Split("/").Last();

			ExtractedLists output = Extractor.ExtractLists(path, extension);

			Directory.CreateDirectory("lists/" + title);

			StringBuilder createSentenceCsv = new StringBuilder();

			foreach (var k in output.RankedSentences.OrderByDescending(x => x.Value))
			{
				createSentenceCsv.AppendLine(k.Key + "," + k.Value + ",");
			}

			File.WriteAllText($"lists/{title}/ranked-sentences.csv", createSentenceCsv.ToString());

			StringBuilder createKanjiCsv = new StringBuilder();

			foreach (var k in output.KanjiFrequency.OrderByDescending(x => x.Value))
			{
				createKanjiCsv.AppendLine(k.Key + "," + k.Value + ",");
			}

			File.WriteAllText($"lists/{title}/frequency-kanji.csv", createKanjiCsv.ToString());

			StringBuilder createWordCsv = new StringBuilder();

			foreach (var k in output.WordFrequency.OrderByDescending(x => x.Value))
			{
				createWordCsv.AppendLine(k.Key + "," + k.Value + ",");
			}

			File.WriteAllText($"lists/{title}/frequency-vocab.csv", createWordCsv.ToString());

		}
	}
    internal class ExtractedLists
    {
        internal Dictionary<string, int> KanjiFrequency { get; set; }
        internal Dictionary<string, int> WordFrequency { get; set; }
        internal Dictionary<string, double> RankedSentences { get; set; }
    }
}
