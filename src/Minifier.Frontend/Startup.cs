using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Minifier.Frontend;
using Minifier.Frontend.OpenAI;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Minifier.Frontend
{
	internal class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddTransient<IGetFullUrlFromSlug, GetFullUrlFromSlug>();
			builder.Services.AddSingleton<Configuration>();
			builder.Services.AddTransient<CosmosClient>(s =>
			{
				var configuration = s.GetRequiredService<Configuration>();
				var connectionString = configuration.UrlMinifierRepository.ConnectionString;
				var clientBuilder = new CosmosClientBuilder(connectionString);
				return clientBuilder.Build();
			});
			builder.Services.AddTransient<ISummarize, OpenAI.Summarize>();
		}
	}
}
