using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using MinifierCore.Business;
using MinifierCore.Model;
using Newtonsoft.Json;

namespace MinifierCore
{
    public static class Delete
    {
        [FunctionName("Delete")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "delete")] HttpRequestMessage req,
            TraceWriter log)
        {
            string jsonContent = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<MinifiedUrl>(jsonContent);

            if (!Validate(data))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }
            var delete = new DeleteUrlHandler();
            bool isDeleted = await delete.Execute(data);

            return req.CreateResponse(isDeleted ? HttpStatusCode.OK : HttpStatusCode.ExpectationFailed, $"api/{data.MinifiedSlug}");
        }

        private static bool Validate(MinifiedUrl inputDocument)
        {
            if (string.IsNullOrEmpty(inputDocument.FullUrl) ||
                string.IsNullOrEmpty(inputDocument.MinifiedSlug))
            {
                return false;
            }

            return true;
        }
    }
}
