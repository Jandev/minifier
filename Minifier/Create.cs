using System.Configuration;
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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "create")]HttpRequestMessage req, 
            Binder binder,
            TraceWriter log)
        {
            string jsonContent = req.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<MinifiedUrl>(jsonContent);
            var secret = new Secret();
            var connectionString = await secret.Get("CosmosConnectionStringSecret");
            ConfigurationManager.AppSettings["CosmosConnectionString"] = connectionString;

            var output = await binder.BindAsync<IAsyncCollector<MinifiedUrl>>(new DocumentDBAttribute("TablesDB", "minified-urls")
            {
                CreateIfNotExists = true,
                ConnectionStringSetting = "CosmosConnectionString",
            });

            var create = new CreateUrlHandler();
            var minifiedUrl = create.Execute(data);

            await output.AddAsync(minifiedUrl);
            
            return req.CreateResponse(HttpStatusCode.Created, $"api/{data.MinifiedSlug}");
        }
    }
}
