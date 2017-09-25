using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Minifier
{
    internal class Secret : ISecret
    {
        public async Task<string> Get(string secretKey)
        {
            // Fetch configuration values
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["ClientKey"];
            var connectionstringUrl = ConfigurationManager.AppSettings[secretKey];

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
