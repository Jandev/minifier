using System.Collections.Generic;

namespace Minifier.Frontend
{
	internal static class Cache
	{
		public static Dictionary<string, string> MinifierEntries = new Dictionary<string, string>();
		public static Dictionary<string, string> SummaryEntries = new Dictionary<string, string>();
	}
}
