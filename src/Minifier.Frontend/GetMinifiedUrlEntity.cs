using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Minifier.Frontend
{
	internal class GetFullUrlFromSlug : IGetFullUrlFromSlug
	{
		private readonly Configuration configuration;
		private readonly CosmosClient client;
		private readonly ILogger<GetFullUrlFromSlug> logger;

		public GetFullUrlFromSlug(
			CosmosClient client,
			ILogger<GetFullUrlFromSlug> logger)
		{
			this.configuration = new Configuration();
			this.client = client;
			this.logger = logger;
		}

		public async Task<string> Run(string slug)
		{
			string foundMinifiedUrl = default;

			if (Cache.MinifierEntries.ContainsKey(slug))
			{
				foundMinifiedUrl = Cache.MinifierEntries[slug];
				this.logger.LogInformation("Retrieved `{slug}` from cache.", slug);
			}
			else
			{
				foundMinifiedUrl = await FindUrlFromRepository(slug);
				Cache.MinifierEntries[slug] = foundMinifiedUrl;
				this.logger.LogInformation("Added `{slug}` to cache.", slug);
			}

			return foundMinifiedUrl;
		}

		private async Task<string> FindUrlFromRepository(string slug)
		{
			var container = this.client.GetContainer(
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

		/// <summary>
		/// Lowercasing this property, because Cosmos DB is case sensitive about properties
		/// and using the output binding, like in this Azure Function, doesn't work appear
		/// to work with <see cref="JsonPropertyNameAttribute"/> definitions.
		/// </summary>
		public class MinifiedUrlEntity
		{
			public string url { get; set; }
		}
	}

	public interface IGetFullUrlFromSlug
	{
		public Task<string> Run(string slug);
	}
}
