using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace Minifier.Frontend
{
    public class Get
    {
        private readonly ILogger<Get> logger;

        public Get(ILogger<Get> logger)
        {
            this.logger = logger;
        }

        [FunctionName(nameof(Get))]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{Slug}")]
            HttpRequest req,
            [CosmosDB(
                "%UrlMinifierRepository:DatabaseName%",
                "%UrlMinifierRepository:CollectionName%",
                Connection = "UrlMinifierRepository",
                Id = "{Slug}",
                PartitionKey = "{Slug}")]
            MinifiedUrlEntity existingMinifiedUrlEntity)
        {
            return new RedirectResult(existingMinifiedUrlEntity.url);
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
}
