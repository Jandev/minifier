using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Minifier.Frontend
{
    public static class Health
    {
        [FunctionName(nameof(Live))]
        public static IActionResult Live(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/Live")]
            HttpRequest req)
        {
            return new OkResult();
        }
    }
}
