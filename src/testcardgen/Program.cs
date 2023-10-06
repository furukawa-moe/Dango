using Python.Runtime;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Wacton.Desu.Japanese;

namespace testcardgen
{
    public class Program
    {
        public static void Main(string[] args)
        {
			//Extractor.GenerateLists(@"C:/Users/Logan Lowe/Documents/Japanese/Subs/Clannad", "*.ass");

			var freqList = new FrequencyList(@"C:\Users\Logan Lowe\Documents\GitHub\Hitode\src\testcardgen\japanese2022freq.txt");

			var japaneseEntries = JapaneseDictionary.ParseEntries();
			var x = SearchByText("一", ref japaneseEntries);

			foreach(var result in x)
			{
				var myKey = freqList.Ranking.FirstOrDefault(x => x.Value == result.Kanjis.First().Text).Key;
				if(myKey != 0) Console.WriteLine(result.Kanjis.First().Text + ", " + myKey);


			}

		}

		public static IEnumerable<IJapaneseEntry> SearchByText(string kanji, ref IEnumerable<IJapaneseEntry> dict, bool exact = false)
		{
			if(exact) return dict.Where(w => w.Kanjis.Any(k => k.Text == kanji));
			else return dict.Where(w => w.Kanjis.Any(k => k.Text.Contains(kanji)));
		}
	}
}