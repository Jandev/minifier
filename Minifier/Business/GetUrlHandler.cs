using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Minifier.DataAccess;
using Minifier.Model;

namespace Minifier.Business
{
    internal class GetUrlHandler : IGetUrlHandler
    {
        private ICosmosClient cosmosClient;

        public GetUrlHandler()
        {
            this.cosmosClient = new CosmosClient();
        }
        public async Task<MinifiedUrl> Execute(string slug)
        {
            var client = await cosmosClient.Get();
            var documentCollectionUri = cosmosClient.GetDocumentCollectionUri();
            
            var queryOptions = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };
            var minifiedUrl = client.CreateDocumentQuery<MinifiedUrl>(documentCollectionUri, queryOptions)
                .Where(u => u.MinifiedSlug == slug)
                .AsEnumerable();

            return minifiedUrl.FirstOrDefault();
        }
    }
}
