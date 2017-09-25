using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Minifier.Business;

namespace Minifier
{
    public static class Get
    {
        [FunctionName("Get")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{slug}")] HttpRequestMessage req, string slug,
            TraceWriter log)
        {
            log.Info($"Entering the function! Slug is {slug}");

            var getUrl = new GetUrlHandler();

            var minifiedUrl = await getUrl.Execute(slug);
            if (minifiedUrl == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.NotFound, $"Minified value `{slug}` is not found.");
            }
            var response = req.CreateResponse(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri(minifiedUrl.FullUrl);
            return response;
        }
    }
}