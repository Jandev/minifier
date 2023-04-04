using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
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

		[FunctionName(nameof(Get))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{slug}")]
			HttpRequest req,
			string slug)
		{
			string foundMinifiedUrl = await getFullUrlFromSlug.Run(slug);

			if (foundMinifiedUrl == null)
			{
				return new NotFoundResult();
			}
			return new RedirectResult(foundMinifiedUrl);
		}

		[FunctionName(nameof(UpdateLocalCache))]
		public void UpdateLocalCache(
			[ServiceBusTrigger("%IncomingUrlsTopicName%", "%IncomingUrlsProcessingSubscription%", Connection = "MinifierIncomingMessages")]
			MinifiedUrl incomingCreateMinifiedUrlCommand
			)
		{
			Cache.Entries[incomingCreateMinifiedUrlCommand.Slug] = incomingCreateMinifiedUrlCommand.Url;
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
