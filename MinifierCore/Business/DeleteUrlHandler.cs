using System;
using System.Threading.Tasks;
using MinifierCore.DataAccess;
using MinifierCore.Model;

namespace MinifierCore.Business
{
    internal class DeleteUrlHandler : IDeleteUrlHandler
    {
        private ICosmosClient cosmosClient;

        public DeleteUrlHandler()
        {
            this.cosmosClient = new CosmosClient();
        }

        public async Task<bool> Execute(MinifiedUrl minifiedUrl)
        {
            if (!IsValid(minifiedUrl))
            {
                throw new ArgumentException();
            }
            var client = await cosmosClient.Get();
            var response = await client.DeleteDocumentAsync(minifiedUrl.FullUrl);
            return response.StatusCode == System.Net.HttpStatusCode.OK;
            
        }

        private bool IsValid(MinifiedUrl minifiedUrl)
        {
            return !string.IsNullOrWhiteSpace(minifiedUrl.FullUrl) &&
                   !string.IsNullOrWhiteSpace(minifiedUrl.MinifiedSlug);
        }
    }
}
