using System;
using System.Threading.Tasks;

namespace Minifier.Frontend.OpenAI
{
	internal class Summarize : ISummarize
	{
		public Task<string> Invoke(string url)
		{
			throw new NotImplementedException();
		}
	}

	public interface ISummarize
	{
		Task<string> Invoke(string url);
	}
}
