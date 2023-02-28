using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Minifier.Frontend
{
	public class Get
	{
		private readonly Configuration configuration;
		private readonly ILogger<Get> logger;

		private static Dictionary<string, string> localCache = new Dictionary<string, string>();

		public Get(
			ILogger<Get> logger)
		{
			this.configuration = new Configuration();
			this.logger = logger;
		}

		[FunctionName(nameof(Get))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{slug}")]
			HttpRequest req,
			string slug,
			[CosmosDB(
				databaseName: "%UrlMinifierRepository:DatabaseName%",
				containerName: "%UrlMinifierRepository:CollectionName%",
				Connection = "UrlMinifierRepository"
				)]
			CosmosClient client)
		{
			string foundMinifiedUrl = default(string);
			if (localCache.ContainsKey(slug))
			{
				foundMinifiedUrl = Get.localCache[slug];
				this.logger.LogInformation("Retrieved `{slug}` from cache.", slug);
			}
			else
			{
				foundMinifiedUrl = await FindUrlFromRepository(slug, client);
				localCache[slug] = foundMinifiedUrl;
				this.logger.LogInformation("Added `{slug}` to cache.", slug);
			}

			if (foundMinifiedUrl == null)
			{
				return new NotFoundResult();
			}
			return new RedirectResult(foundMinifiedUrl);
		}

		private async Task<string> FindUrlFromRepository(string slug, CosmosClient client)
		{
			var container = client.GetContainer(
												this.configuration.UrlMinifierRepository.DatabaseName,
												this.configuration.UrlMinifierRepository.CollectionName);

			var queryDefinition = new QueryDefinition(
				"SELECT u.url FROM urls u WHERE u.id = @slug")
				.WithParameter("@slug", slug);
			var queryRequestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(slug) };

			string foundMinifiedUrl = default;
			using (var resultSet = container.GetItemQueryIterator<MinifiedUrlEntity>(
																	queryDefinition,
																	requestOptions: queryRequestOptions))
			{
				while (resultSet.HasMoreResults)
				{
					var response = await resultSet.ReadNextAsync();
					foundMinifiedUrl = response.First()?.url;
					break;
				}
			}

			return foundMinifiedUrl;
		}

		[FunctionName(nameof(UpdateLocalCache))]
		public void UpdateLocalCache(
			[ServiceBusTrigger("%IncomingUrlsTopicName%", "%IncomingUrlsProcessingSubscription%", Connection = "MinifierIncomingMessages")]
			MinifiedUrl incomingCreateMinifiedUrlCommand
			)
		{
			localCache[incomingCreateMinifiedUrlCommand.Slug] = incomingCreateMinifiedUrlCommand.Url;
			this.logger.LogInformation("Upserted {slug} with {url} to the local cache.", incomingCreateMinifiedUrlCommand.Slug, incomingCreateMinifiedUrlCommand.Url);
		}

		/// <summary>
		/// Lowercasing this property, because Cosmos DB is case sensitive about properties
		/// and using the output binding, like in this Azure Function, doesn't work appear
		/// to work with <see cref="JsonPropertyNameAttribute"/> definitions.
		/// </summary>
		public class MinifiedUrlEntity
		{
			public string url { get; set; }
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
