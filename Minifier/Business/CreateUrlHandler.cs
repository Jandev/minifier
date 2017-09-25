using System;
using System.Threading.Tasks;
using Minifier.DataAccess;
using Minifier.Model;

namespace Minifier.Business
{
    internal class CreateUrlHandler : ICreateUrlHandler
    {
        private readonly CosmosClient cosmosClient;

        public CreateUrlHandler()
        {
            this.cosmosClient = new CosmosClient();
        }
        public async Task Execute(MinifiedUrl minifiedUrl)
        {
            if (!IsValid(minifiedUrl))
            {
                throw new ArgumentException();
            }
            minifiedUrl.Created = DateTime.UtcNow;

            var client = await cosmosClient.Get();
            var documentCollectionUri = cosmosClient.GetDocumentCollectionUri();

            var response = await client.CreateDocumentAsync(documentCollectionUri, minifiedUrl);
        }

        private bool IsValid(MinifiedUrl minifiedUrl)
        {
            return !string.IsNullOrWhiteSpace(minifiedUrl.FullUrl) &&
                   !string.IsNullOrWhiteSpace(minifiedUrl.MinifiedSlug);
        }
    }
}
