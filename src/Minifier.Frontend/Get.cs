using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

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
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{slug}")]
            HttpRequest req,
            string slug)
        {
            logger.LogInformation("Entering the function! Slug is {slug}", slug);

            return new OkObjectResult(slug);
        }
    }
}
