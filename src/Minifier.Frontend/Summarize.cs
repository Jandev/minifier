using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Minifier.Frontend.OpenAI;

namespace Minifier.Frontend
{
	public class Summarize
	{
		private readonly Configuration configuration;
		private readonly IGetFullUrlFromSlug getFullUrlFromSlug;
		private readonly ISummarize summarize;
		private readonly ILogger<Summarize> logger;

		public Summarize(
			IGetFullUrlFromSlug getFullUrlFromSlug,
			ISummarize summarize,
			ILogger<Summarize> logger)
		{
			this.configuration = new Configuration();
			this.getFullUrlFromSlug = getFullUrlFromSlug;
			this.summarize = summarize;
			this.logger = logger;
		}

		[FunctionName(nameof(Summarize))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "summarize/{slug}")]
			HttpRequest req,
			string slug)
		{
			this.logger.LogInformation("Requesting summary for `{slug}`.", slug);

			string foundMinifiedUrl = await getFullUrlFromSlug.Run(slug);
			if (foundMinifiedUrl == null)
			{
				return new NotFoundResult();
			}

			var summary = await this.summarize.Invoke(foundMinifiedUrl);

			return new OkObjectResult(
				new SummarizeResponse
				{
					Url = foundMinifiedUrl,
					Summary = summary
				}
			);
		}

		public class SummarizeResponse
		{
			public string Url { get; set; }
			public string Summary { get; set; }
		}
	}
}
