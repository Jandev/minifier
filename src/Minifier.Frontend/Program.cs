using Microsoft.Extensions.Hosting;
using Minifier.Frontend.OpenAI;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

namespace Minifier.Frontend
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			var host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults()
				.ConfigureServices(services =>
				{
					var configuration = new Configuration();
					services.AddTransient<IGetFullUrlFromSlug, GetFullUrlFromSlug>();
					services.AddSingleton<Configuration>(configuration);
					services.AddTransient<CosmosClient>(s =>
					{
						var configuration = s.GetRequiredService<Configuration>();
						var connectionString = configuration.UrlMinifierRepository.ConnectionString;
						var clientBuilder = new CosmosClientBuilder(connectionString);
						return clientBuilder.Build();
					});

					if(configuration.OpenAi.UseSemanticKernel)
					{
						services.AddTransient<ISummarize, OpenAI.SemanticKernel.Summarize>();
					}
					else
					{
						services.AddTransient<ISummarize, OpenAI.Summarize>();
					}
				})
				.Build();
			await host.RunAsync();
		}
	}
}
