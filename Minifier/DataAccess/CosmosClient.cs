using System;
using System.Configuration;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Minifier.DataAccess
{
    internal class CosmosClient : ICosmosClient
    {
        private const string DatabaseName = "TablesDB";
        private const string CollectionName = "minified-urls";

        public async Task<DocumentClient> Get()
        {
            var connectionString = new CosmosDbConnectionString(ConfigurationManager.AppSettings["CosmosConnectionString"]);            
            var client = new DocumentClient(connectionString.ServiceEndpoint, connectionString.AuthKey);

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

        private class CosmosDbConnectionString
        {
            public CosmosDbConnectionString(string connectionString)
            {
                // Use this generic builder to parse the connection string
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder
                {
                    ConnectionString = connectionString
                };

                if (builder.TryGetValue("AccountKey", out var key))
                {
                    AuthKey = key.ToString();
                }

                if (builder.TryGetValue("AccountEndpoint", out var uri))
                {
                    ServiceEndpoint = new Uri(uri.ToString());
                }
            }

            public Uri ServiceEndpoint { get; }

            public string AuthKey { get; }
        }
    }
}
