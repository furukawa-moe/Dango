using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace testcardgen
{
	internal class FrequencyList
	{
		public FrequencyList(string path)
		{
			this.Ranking = new();

			File.ReadAllLines(path);
			foreach (string line in File.ReadAllLines(path))
			{
				int test = 0;
				if (!int.TryParse(line.Split(",")[0], out test)) continue;
				Ranking.Add(test, line.Split(",")[2]);
			}
		}

		public Dictionary<int, string> Ranking { get; set; }
	}
}
