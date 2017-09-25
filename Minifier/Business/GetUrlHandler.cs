using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Minifier.Model;

namespace Minifier.Business
{
    internal class GetUrlHandler : IGetUrlHandler
    {
        private readonly ISecret secret;
        public GetUrlHandler()
        {
            this.secret = new Secret();
        }
        public async Task<MinifiedUrl> Execute(string slug)
        {
            const string databaseName = "TablesDB";
            const string collectionName = "minified-urls";
            var endpoint = await secret.Get("CosmosEndpoint");
            var authenticationToken = await secret.Get("CosmosAuthenticationKey");
            var endpointUri = new Uri(endpoint);
            var client = new DocumentClient(endpointUri, authenticationToken);

            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });
            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName),
                new DocumentCollection { Id = collectionName });
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
            var queryOptions = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };
            var minifiedUrl = client.CreateDocumentQuery<MinifiedUrl>(documentCollectionUri, queryOptions)
                .Where(u => u.MinifiedSlug == slug)
                .AsEnumerable();

            return minifiedUrl.FirstOrDefault();
        }
    }
}
