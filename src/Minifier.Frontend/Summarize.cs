using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minifier.Frontend.OpenAI;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json.Serialization;

namespace Minifier.Frontend
{
	public class Summarize
	{
		private readonly IGetFullUrlFromSlug getFullUrlFromSlug;
		private readonly ISummarize summarize;
		private readonly ILogger<Summarize> logger;

		public Summarize(
			IGetFullUrlFromSlug getFullUrlFromSlug,
			ISummarize summarize,
			ILogger<Summarize> logger)
		{
			this.getFullUrlFromSlug = getFullUrlFromSlug;
			this.summarize = summarize;
			this.logger = logger;
		}

		[Function(nameof(Summarize))]
		public async Task<HttpResponseData> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "summarize/{slug}")]
			HttpRequestData req,
			string slug)
		{
			this.logger.LogInformation("Requesting summary for `{slug}`.", slug);

			string foundMinifiedUrl = await getFullUrlFromSlug.Run(slug);
			if (foundMinifiedUrl == null)
			{
				return req.CreateResponse(HttpStatusCode.NotFound);
			}

			if (Cache.SummaryEntries.ContainsKey(slug))
			{
				return await CreateSummaryResponse(req, foundMinifiedUrl, Cache.SummaryEntries[slug]);
			}			

			var summary = await this.summarize.Invoke(foundMinifiedUrl);
			Cache.SummaryEntries[slug] = summary;

			return await CreateSummaryResponse(req, foundMinifiedUrl, summary);
		}

		private static async Task<HttpResponseData> CreateSummaryResponse(HttpRequestData req, string foundMinifiedUrl, string summary)
		{
			var response = req.CreateResponse(HttpStatusCode.OK);
			await response.WriteAsJsonAsync(
				new SummarizeResponse
				{
					Url = foundMinifiedUrl,
					Summary = summary
				});
			return response;
		}

		public class SummarizeResponse
		{
			[JsonPropertyName("url")]
			public string Url { get; set; }
			
			[JsonPropertyName("summary")]
			public string Summary { get; set; }
		}
	}
}
