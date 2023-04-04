using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Minifier.Frontend;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Minifier.Frontend
{
	internal class Startup : FunctionsStartup
	{
		public override void Configure(IFunctionsHostBuilder builder)
		{
			builder.Services.AddLogging();
			builder.Services.AddTransient<IGetFullUrlFromSlug, GetFullUrlFromSlug>();
			builder.Services.AddSingleton<Configuration>();
			builder.Services.AddTransient<CosmosClient>(s =>
			{
				var configuration = s.GetRequiredService<Configuration>();
				var connectionString = configuration.UrlMinifierRepository.ConnectionString;
				var clientBuilder = new CosmosClientBuilder(connectionString);
				return clientBuilder.Build();
			});
		}
	}
}
