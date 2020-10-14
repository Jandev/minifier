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
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "create")] HttpRequestMessage req, 
            [ServiceBus("new-urls", Connection = "CreateNewUrlsServiceBusConnection")] out MinifiedUrl outputUrl,
            TraceWriter log)
        {
            string jsonContent = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<MinifiedUrl>(jsonContent);

            if (!Validate(data))
            {
                outputUrl = null;
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            var create = new CreateUrlHandler();
            outputUrl = create.Execute(data);
            
            return req.CreateResponse(HttpStatusCode.Created, $"api/{data.MinifiedSlug}");
        }

        private static bool Validate(MinifiedUrl inputDocument)
        {
            if (inputDocument == null ||
                string.IsNullOrEmpty(inputDocument.FullUrl) ||
                string.IsNullOrEmpty(inputDocument.MinifiedSlug))
            {
                return false;
            }

            return true;
        }
    }
}
