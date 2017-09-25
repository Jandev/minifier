using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Minifier.DataAccess
{
    internal class CosmosClient : ICosmosClient
    {
        private const string DatabaseName = "TablesDB";
        private const string CollectionName = "minified-urls";

        private readonly Secret secret;

        public CosmosClient()
        {
            this.secret = new Secret();
        }

        public async Task<DocumentClient> Get()
        {
            var endpoint = await secret.Get("CosmosEndpoint");
            var authenticationToken = await secret.Get("CosmosAuthenticationKey");
            var endpointUri = new Uri(endpoint);
            var client = new DocumentClient(endpointUri, authenticationToken);

            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = DatabaseName });
            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DatabaseName),
                new DocumentCollection { Id = CollectionName });

            return client;
        }

        public Uri GetDocumentCollectionUri()
        {
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, CollectionName);
            return documentCollectionUri;
        }
    }
}
