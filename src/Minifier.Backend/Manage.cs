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

            var (valid, message) = Valid(data);
            if (!valid)
            {
                return new BadRequestErrorMessageResult(message);
            }
            data.Created = DateTime.UtcNow;

            await createMinifiedUrlCommands.AddAsync(data, cancellationSource.Token);

            return new OkObjectResult(data.Slug);
        }

        private static (bool, string) Valid(MinifiedUrl data)
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
    }
}
