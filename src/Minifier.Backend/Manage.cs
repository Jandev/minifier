using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Azure.Messaging.ServiceBus;

namespace Minifier.Backend
{
    public class Manage
    {
        private readonly ILogger<Manage> logger;

        public Manage(ILogger<Manage> logger)
        {
            this.logger = logger;
        }
        
        [FunctionName(nameof(Create))]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequest req,
            [ServiceBus("%IncomingUrlsTopicName%", Connection = "MinifierIncomingMessages")]
            IAsyncCollector<MinifiedUrl> createMinifiedUrlCommands,
            CancellationToken hostCancellationToken)
        {
            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(hostCancellationToken, req.HttpContext.RequestAborted);

            var data = await JsonSerializer.DeserializeAsync<MinifiedUrl>(
                new StreamReader(req.Body).BaseStream,
                cancellationToken: cancellationSource.Token);
            if (data == null)
            {
                return new BadRequestResult();
            }
            logger.LogDebug("Receiving `{slug}` for url `{url}`", data.Slug, data.Url);

            var (valid, message) = IsValidIncomingMinifiedUrlRequest(data);
            if (!valid)
            {
                return new BadRequestErrorMessageResult(message);
            }
            data.Created = DateTime.UtcNow;

            await createMinifiedUrlCommands.AddAsync(data, cancellationSource.Token);

            return new OkObjectResult(data.Slug);
        }

        [FunctionName(nameof(Process))]
        public async Task Process(
            [ServiceBusTrigger("%IncomingUrlsTopicName%", "%IncomingUrlsProcessingSubscription%", Connection = "MinifierIncomingMessages")]
            MinifiedUrl incomingCreateMinifiedUrlCommand,
            [CosmosDB(
                "%UrlMinifierRepository:DatabaseName%", 
                "%UrlMinifierRepository:CollectionName%", 
                Connection = "UrlMinifierRepository",
                Id = "{Slug}",
                PartitionKey = "{Slug}")]
            MinifiedUrlEntity existingMinifiedUrlEntity,
            [CosmosDB(
                "%UrlMinifierRepository:DatabaseName%", 
                "%UrlMinifierRepository:CollectionName%", 
                Connection = "UrlMinifierRepository")] 
            IAsyncCollector<MinifiedUrlEntity> minifiedUrls
            )
        {
            if (existingMinifiedUrlEntity == null)
            {
                // When querying, we're only using the `Slug`, therefore it makes sense
                // to use it as the identifier and also partitionkey
                // Source: https://stackoverflow.com/a/54637561/352640
                // Also makes it easier with the Input binding in this Azure Function.
                await minifiedUrls.AddAsync(new MinifiedUrlEntity
                {
                    slug = incomingCreateMinifiedUrlCommand.Slug,
                    url = incomingCreateMinifiedUrlCommand.Url,
                    created = incomingCreateMinifiedUrlCommand.Created,
                    id = incomingCreateMinifiedUrlCommand.Slug
                });
            }
            else
            {
                throw new ArgumentException($"The slug `{existingMinifiedUrlEntity.slug}` already exists.");
            }
        }

        private static (bool, string) IsValidIncomingMinifiedUrlRequest(MinifiedUrl data)
        {
            if (string.IsNullOrWhiteSpace(data.Slug) || string.IsNullOrWhiteSpace(data.Url))
            {
                return (false, "Either the slug or url is empty.");
            }

            if (!Uri.TryCreate(data.Url, UriKind.Absolute, out _))
            {
                return (false, "The specified `url` isn't a valid URL.");
            }

            return (true, default);
        }

        public class MinifiedUrl
        {
            [JsonPropertyName("slug")]
            public string Slug { get; set; }
            [JsonPropertyName("url")]
            public string Url { get; set; }
            public DateTime Created { get; set; }
        }

        /// <summary>
        /// Lowercasing this property, because Cosmos DB is case sensitive about properties
        /// and using the output binding, like in this Azure Function, doesn't work appear
        /// to work with <see cref="JsonPropertyNameAttribute"/> definitions.
        /// </summary>
        public class MinifiedUrlEntity
        {
            public string id { get; set; }
            public string slug { get; set; }
            public string url { get; set; }
            public DateTime created { get; set; }

        }
    }
}
