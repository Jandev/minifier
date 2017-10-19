using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Minifier
{
    public static class LetsEncrypt
    {
        [FunctionName("letsencrypt")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{code}")]
            HttpRequestMessage req, 
            string code, 
            TraceWriter log)
        {
            log.Info($"C# HTTP trigger function processed a request. {code}");

            var content = File.ReadAllText(@"D:\home\site\wwwroot\.well-known\acme-challenge\" + code);
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(content, System.Text.Encoding.UTF8, "text/plain");
            return resp;
        }
    }
}
