using System;

namespace Minifier.Frontend 
{
	internal class Configuration 
	{
		public UrlMinifierRepository UrlMinifierRepository { get; } = new UrlMinifierRepository();
	}

	internal class UrlMinifierRepository 
	{
		public string ConnectionString { get; } = Environment.GetEnvironmentVariable("UrlMinifierRepository", EnvironmentVariableTarget.Process);

		public string DatabaseName { get; } = Environment.GetEnvironmentVariable($"{nameof(UrlMinifierRepository)}__{nameof(DatabaseName)}", EnvironmentVariableTarget.Process);
		public string CollectionName { get; } = Environment.GetEnvironmentVariable($"{nameof(UrlMinifierRepository)}__{nameof(CollectionName)}", EnvironmentVariableTarget.Process);
	}
}
