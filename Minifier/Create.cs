using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Minifier.Business;
using Minifier.Model;
using Newtonsoft.Json;

namespace Minifier
{
    public static class Create
    {
        [FunctionName("Create")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "create")]HttpRequestMessage req, 
            [DocumentDB("TablesDB", "minified-urls", ConnectionStringSetting = "Minified_ConnectionString", CreateIfNotExists = true)] out MinifiedUrl minifiedUrl,
            TraceWriter log)
        {
            string jsonContent = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<MinifiedUrl>(jsonContent);

            var create = new CreateUrlHandler();
            minifiedUrl = create.Execute(data);

            return req.CreateResponse(HttpStatusCode.Created, $"api/{data.MinifiedSlug}");
        }
    }
}
