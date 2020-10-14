using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Minifier.Model;

namespace Minifier
{
    public static class Process
    {
        [FunctionName("Process")]
        public static async Task Run(
            [ServiceBusTrigger("new-urls", "CreateUrl", Connection = "GetNewUrlsServiceBusConnection")] MinifiedUrl validatedMinifiedUrl,
            Binder binder,
            TraceWriter log)
        {
            log.Info($"Processing the url: `{validatedMinifiedUrl.MinifiedSlug}`");

            var output = await CreateOutputBinding(binder);

            var processedMinifiedUrl = new MinifiedUrl
            {
                MinifiedSlug = validatedMinifiedUrl.MinifiedSlug,
                FullUrl = validatedMinifiedUrl.FullUrl,
                Created = validatedMinifiedUrl.Created
            };

            await output.AddAsync(processedMinifiedUrl);
        }

        private static async Task<IAsyncCollector<MinifiedUrl>> CreateOutputBinding(Binder binder)
        {
            var output = await binder.BindAsync<IAsyncCollector<MinifiedUrl>>(
                new DocumentDBAttribute("TablesDB", "minified-urls")
                {
                    CreateIfNotExists = true,
                    ConnectionStringSetting = "CosmosConnectionString",
                });
            return output;
        }
    }
}
