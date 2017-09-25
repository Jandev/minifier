using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Minifier
{
    public static class Get
    {
        [FunctionName("Get")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{slug}")] HttpRequestMessage req, string slug, TraceWriter log)
        {
            var connectionstring = await GetConnectionString();

            log.Info($"Entering the function! Slug is {slug}");
            var testValue = ConfigurationManager.AppSettings["MyTest"];
            return req.CreateResponse(HttpStatusCode.OK, $"The value is {testValue}");
        }

        private static async Task<string> GetConnectionString()
        {
            // Fetch configuration values
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["ClientKey"];
            var connectionstringUrl = ConfigurationManager.AppSettings["ConnectionstringUrl"];

            // Create a Key Vault client with an Active Directory authentication callback
            var keyVault = new KeyVaultClient(async (authority, resource, scope) =>
            {
                var authContext = new AuthenticationContext(authority);
                var credential = new ClientCredential(clientId, clientSecret);
                var token = await authContext.AcquireTokenAsync(resource, credential);
                return token.AccessToken;
            });

            // Get the API key out of the vault
            var secret = await keyVault.GetSecretAsync(connectionstringUrl);
            return secret.Value;
        }
    }
}