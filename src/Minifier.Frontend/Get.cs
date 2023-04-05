using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Minifier.Frontend
{
	public class Get
	{
		private readonly IGetFullUrlFromSlug getFullUrlFromSlug;
		private readonly ILogger<Get> logger;

		public Get(
			IGetFullUrlFromSlug getFullUrlFromSlug,
			ILogger<Get> logger)
		{
			this.getFullUrlFromSlug = getFullUrlFromSlug;
			this.logger = logger;
		}

		[Function(nameof(Get))]
		public async Task<HttpResponseData> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{slug}")]
			HttpRequestData req,
			string slug)
		{
			string foundMinifiedUrl = await getFullUrlFromSlug.Run(slug);

			if (foundMinifiedUrl == null)
			{
				return req.CreateResponse(HttpStatusCode.NotFound);
			}
			var redirectResponse = req.CreateResponse(HttpStatusCode.Redirect);
			redirectResponse.Headers.Add("Location", foundMinifiedUrl);
			return redirectResponse;
		}

		[Function(nameof(UpdateLocalCache))]
		public void UpdateLocalCache(
			[ServiceBusTrigger("%IncomingUrlsTopicName%", "%IncomingUrlsProcessingSubscription%", Connection = "MinifierIncomingMessages")]
			MinifiedUrl incomingCreateMinifiedUrlCommand
			)
		{
			Cache.MinifierEntries[incomingCreateMinifiedUrlCommand.Slug] = incomingCreateMinifiedUrlCommand.Url;
			this.logger.LogInformation("Upserted {slug} with {url} to the local cache.", incomingCreateMinifiedUrlCommand.Slug, incomingCreateMinifiedUrlCommand.Url);
		}

		public class MinifiedUrl
		{
			[JsonPropertyName("slug")]
			public string Slug { get; set; }
			[JsonPropertyName("url")]
			public string Url { get; set; }
		}
	}
}
