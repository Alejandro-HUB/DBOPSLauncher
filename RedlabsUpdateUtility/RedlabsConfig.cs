using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RedlabsUpdateUtility
{
	public static class RedlabsConfig
	{
		public static RedlabsConfigInfo Parse(string text)
		{
			RedlabsConfigInfo info = new RedlabsConfigInfo();

			string[] lines = text.Split('\n');
			for(int i = 0; i < lines.Length; i++)
			{
				if(lines[i][0].Equals("#")) // If the line starts with a comment symbol, forget it
				{
					continue;
				}

				string[] elements = lines[i].Split(new[] { '=' }, 2);
				info.values.Add(elements[0].Trim(), elements[1].Split('#')[0].Trim());
			}

			return info;
		}
	}

	public class RedlabsConfigInfo
	{
		public Dictionary<string, string> values;

		public RedlabsConfigInfo(Dictionary<string, string> values = null)
		{
			if(values == null)
			{
				this.values = new Dictionary<string, string>();
			}
			else
			{
				this.values = values;
			}
		}
	}
}
