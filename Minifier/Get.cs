using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Minifier.Model;

namespace Minifier
{
    public static class Get
    {
        [FunctionName("Get")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{slug}")]
            HttpRequestMessage req, 
            string slug,
            [DocumentDB("TablesDB", 
                "minified-urls", 
                ConnectionStringSetting = "CosmosConnectionString", 
                SqlQuery = "SELECT * FROM c WHERE c.MinifiedSlug = {slug}")]
            IEnumerable<MinifiedUrl> minifiedUrl,
            TraceWriter log)
        {
            log.Info($"Entering the function! Slug is {slug}");

            if (!minifiedUrl.Any())
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var retrievedMinifiedUrl = GetMinifiedUrl(slug, minifiedUrl, log);

            var response = req.CreateResponse(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri(retrievedMinifiedUrl.FullUrl);
            return response;
        }

        private static MinifiedUrl GetMinifiedUrl(string slug, IEnumerable<MinifiedUrl> minifiedUrl, TraceWriter log)
        {
            if (minifiedUrl.Count() > 1)
            {
                log.Warning($"Slug `{slug}` has multiple entries.");
                return minifiedUrl.Last();
            }

            return minifiedUrl.First();
        }
    }
}