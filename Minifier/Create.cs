using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "create")]HttpRequestMessage req, TraceWriter log)
        {
            string jsonContent = await req.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<MinifiedUrl>(jsonContent);

            var create = new CreateUrlHandler();
            await create.Execute(data);

            return req.CreateResponse(HttpStatusCode.Created, $"api/{data.MinifiedSlug}");
        }
    }
}
