using Python.Runtime;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Wacton.Desu.Japanese;
using System.Collections.ObjectModel;
using System.Net;

namespace testcardgen
{
    public class Program
    {
		public static IEnumerable<IJapaneseEntry> JapaneseEntries = JapaneseDictionary.ParseEntries();
		public static void Main(string[] args)
        {
			//Extractor.GenerateLists(@"C:/Users/Logan Lowe/Documents/Japanese/Subs/Clannad", "*.ass");

			// STORE ORDERED LIST OF KANJI
			// ADD ORDERED LIST OF WORDS
			// BUBBLE WORDS UP PAST REQUISITE KANJI
			/*
			ObservableCollection<string> bubbleList = new ObservableCollection<string>();

			foreach (string line in File.ReadAllLines(@"lists/Azumanga Daioh/ranked-sentences.csv"))
			{
				if (double.Parse(line.Split(",")[1]) > 0 && double.Parse(line.Split(",")[1]) <= 20) { bubbleList.Add(line.Split(",")[0] + ",sentence"); }
			}

			foreach (string line in File.ReadAllLines(@"lists/Azumanga Daioh/frequency-kanji.csv"))
			{
				bubbleList.Add(line.Split(",")[0] + ",kanji");
			}

			foreach (string line in File.ReadAllLines(@"lists/Azumanga Daioh/frequency-vocab.csv"))
			{
				bubbleList.Add(line.Split(",")[0] + ",vocab");
			}

			// i have no clue how to tell when this is actually done so we'll do it enough times to be SURE we're sorted lmao
			for(int x = 0; x < 100; x++)
			{
				int i = 0; while (i < bubbleList.Count())
				{
					if (bubbleList[i].Contains(",kanji"))
					{
						// Get index of first occurance of vocab with this kanji
						int vocabIndex = 0;

						foreach (string line in bubbleList)
						{
							if (line.Contains(",vocab") && line.Contains(bubbleList[i].Split(",")[0]))
							{
								vocabIndex = bubbleList.IndexOf(line);
								break;
							}
						}

						bubbleList.Move(i, vocabIndex);
					}
					i++;
				}
			}

			ManagedDango tokenizer = new ManagedDango();

			// i have no clue how to tell when this is actually done so we'll do it enough times to be SURE we're sorted lmao
			for (int x = 0; x < 10; x++)
			{
				int i = 0; while (i < bubbleList.Count())
				{
					if (bubbleList[i].Contains(",sentence"))
					{
						var matches = tokenizer.Tokenize(bubbleList[i].Split(",")[0]);
						List<string> listMatches = new List<string>();

						foreach (var m in matches)
						{
							listMatches.Add(m.DictionaryForm);
						}

						// Get index of first occurance of vocab with this kanji
						int vocabIndex = 0;

						foreach (string line in bubbleList)
						{
							foreach (string match in listMatches)
							{
								if (line.Contains(",vocab") && line.Contains(match))
								{
									vocabIndex = bubbleList.IndexOf(line);
									break;
								}
							}
						}

						bubbleList.Move(i, vocabIndex + 1);
					}
					i++;
				}
			}

			// at this point we MIGHT JUST BE SORTED (poorly)
			StringBuilder sb = new();

			foreach (string line in bubbleList)
			{
				sb.Append(line + ",");
			}

			File.WriteAllText("sorted-list.csv", sb.ToString());

			Directory.CreateDirectory("cards");
			*/

			List<string> bubbleList = new();
			bubbleList.Add("石,kanji");

			foreach (string line in bubbleList)
			{
				string outcard = "[card]\n";
				if (line.Contains(",kanji"))
				{
					string kanji = line.Split(",")[0];
					List<IJapaneseEntry> examples = new();

					foreach (var entry in GetWordsUsingKanji(kanji))
					{
						examples.Add(SearchByText(entry.Key, ref JapaneseEntries, true).First());
					}

					outcard += "template=kanji-card-jisho-template\n";
					outcard += $"answer={kanji}\n";
					outcard += $"kanji-example-1={templates.HtmlHelper.GenerateRuby(examples[0].Kanjis.First().Text, ref JapaneseEntries).Replace(kanji, "▧")} + <i>{examples[0].Senses.First().Glosses.First().Term} ({examples[0].Senses.First().PartsOfSpeech.First().DisplayName})</i>\n";
					outcard += $"kanji-example-2={templates.HtmlHelper.GenerateRuby(examples[1].Kanjis.First().Text, ref JapaneseEntries).Replace(kanji, "▧")} + <i>{examples[1].Senses.First().Glosses.First().Term} ({examples[1].Senses.First().PartsOfSpeech.First().DisplayName})</i>\n";
					outcard += $"kanji-example-3={templates.HtmlHelper.GenerateRuby(examples[2].Kanjis.First().Text, ref JapaneseEntries).Replace(kanji, "▧")} + <i>{examples[2].Senses.First().Glosses.First().Term} ({examples[2].Senses.First().PartsOfSpeech.First().DisplayName})</i>\n";
					outcard += $"kanji-example-4={templates.HtmlHelper.GenerateRuby(examples[3].Kanjis.First().Text, ref JapaneseEntries).Replace(kanji, "▧")} + <i>{examples[3].Senses.First().Glosses.First().Term} ({examples[3].Senses.First().PartsOfSpeech.First().DisplayName})</i>\n";
					;
				}
			}


			//Console.WriteLine(templates.HtmlHelper.GenerateRuby("竜宮", ref JapaneseEntries));
			Console.WriteLine(SearchByText("お願い", ref JapaneseEntries).First().Senses.First().PartsOfSpeech.First().DisplayName);
			//GetWordsUsingKanji("");
		}

		/// <summary>
		/// Get a list of words that use the given kanji, in order of frequency.
		/// </summary>
		/// <param name="kanji"></param>
		/// <returns></returns>
		public static IEnumerable<KeyValuePair<string, int>> GetWordsUsingKanji(string kanji)
		{
			var freqList = new FrequencyList(@"C:\Users\Logan Lowe\Documents\GitHub\Hitode\src\testcardgen\japanese2022freq.txt");
			var x = SearchByText(kanji, ref JapaneseEntries);

			Dictionary<string, int> wordsKanjiUseRanking = new Dictionary<string, int>();

			foreach (var result in x)
			{
				var rank = freqList.Ranking.FirstOrDefault(x => x.Value == result.Kanjis.First().Text).Key;
				if (rank == 0) continue;
				if (!wordsKanjiUseRanking.ContainsKey(result.Kanjis.First().Text) && result.Kanjis.First().Text.Contains(kanji)) wordsKanjiUseRanking.Add(result.Kanjis.First().Text, rank);
			}

			var sortedDict = from entry in wordsKanjiUseRanking orderby entry.Value ascending select entry;

			return sortedDict;
		}

		public static IEnumerable<IJapaneseEntry> SearchByText(string query, ref IEnumerable<IJapaneseEntry> dict, bool exact = false)
		{
			if(exact) return dict.Where(w => w.Kanjis.Any(k => k.Text == query));
			else return dict.Where(w => w.Kanjis.Any(k => k.Text.Contains(query)));
		}
	}
}