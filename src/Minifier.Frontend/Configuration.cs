using System;

namespace Minifier.Frontend 
{
	internal class Configuration 
	{
		public UrlMinifierRepository UrlMinifierRepository { get; } = new UrlMinifierRepository();
		public OpenAi OpenAi { get; } = new OpenAi();
	}

	internal class UrlMinifierRepository 
	{
		public string AccountEndpoint { get; } = Environment.GetEnvironmentVariable($"{nameof(UrlMinifierRepository)}__accountEndpoint", EnvironmentVariableTarget.Process);

		public string DatabaseName { get; } = Environment.GetEnvironmentVariable($"{nameof(UrlMinifierRepository)}__{nameof(DatabaseName)}", EnvironmentVariableTarget.Process);
		public string CollectionName { get; } = Environment.GetEnvironmentVariable($"{nameof(UrlMinifierRepository)}__{nameof(CollectionName)}", EnvironmentVariableTarget.Process);
	}

	internal class OpenAi
	{
		public string ApiKey { get; } = Environment.GetEnvironmentVariable("OpenAiServiceKey", EnvironmentVariableTarget.Process);

		public string Endpoint { get; } = Environment.GetEnvironmentVariable("OpenAiServiceCompletionEndpoint", EnvironmentVariableTarget.Process);

		public string DeploymentId { get; } = Environment.GetEnvironmentVariable("OpenAiServiceDeploymentId", EnvironmentVariableTarget.Process);
		public string ModelName { get; } = Environment.GetEnvironmentVariable("OpenAiServiceModelName", EnvironmentVariableTarget.Process);
		public bool UseSemanticKernel { get; } = bool.Parse(Environment.GetEnvironmentVariable("OpenAiServiceUseSemanticKernel", EnvironmentVariableTarget.Process));
	}
}
