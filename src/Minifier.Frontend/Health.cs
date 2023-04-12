using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Minifier.Frontend
{
    public static class Health
    {
        [Function(nameof(Live))]
        public static IActionResult Live(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/Live")]
            HttpRequest req)
        {
            return new OkResult();
        }
    }
}
