using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wacton.Desu.Japanese;

namespace testcardgen.templates
{
	internal class HtmlHelper
	{
		/// <summary>
		/// Generate a properly formatted HTML Ruby string for showing furigana over the kanji of a given word.
		/// </summary>
		/// <param name="text">The text to generate a ruby for.</param>
		/// <param name="dict">The dictionary to search for readings in.</param>
		/// <returns></returns>
		public static string GenerateRuby(string text, ref IEnumerable<IJapaneseEntry> dict)
		{
			IJapaneseEntry dictText = Program.SearchByText(text, ref dict, true).First();
			string textReading = dictText.Readings.FirstOrDefault().Text;
			
			if (string.IsNullOrWhiteSpace(textReading)) return "";

			Dictionary<string, string> ruby = new Dictionary<string, string>();

			foreach (char c in text)
			{
				// Skip if the character is a kanji
				if (c >= 19968) continue;

				// Split the string by the kana, and if the left half of the split is
				// longer than zero, look for the first occurance of c in dictReading,
				// substr, and now you have furigana for that length of the first string
				string part = text.Split(c)[0];

				if (part.Length > 0)
				{
					ruby.Add(part, textReading.Substring(0, textReading.IndexOf(c)));
				}

				text = text.Substring(text.IndexOf(c) + 1);
				textReading = textReading.Substring(textReading.IndexOf(c) + 1);
				ruby.Add(c.ToString(), "");
			}

			// If we still have text left in the reading string, it wasn't applied for some reason
			// (the word might end in kanji or be entirely kanji), so let's apply it here
			if (textReading != "") ruby.Add(text, textReading);
			
			// Generate HTML for given ruby
			string html = "";

			foreach (KeyValuePair<string, string> entry in ruby)
			{
				if (entry.Value != "")
				{
					html += $"<ruby><span class=\"ruby-kanji\">{entry.Key}</span><rt>{entry.Value}</rt></ruby>";
				}
				else
				{
					html += $"<span>{entry.Key}</span>";
				}
			}

			return html;
		}
	}
}
