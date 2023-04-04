using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;


namespace Minifier.Frontend.OpenAI
{
	internal class Summarize : ISummarize
	{
		private readonly Configuration configuration;

		public Summarize(
			Configuration configuration)
		{
			this.configuration = configuration;
		}

		public async Task<string> Invoke(string url)
		{
			var request = $"Summarize the contents of the following web page: {url}";
			OpenAIClient client = new OpenAIClient(
				new Uri("https://westeurope.api.cognitive.microsoft.com/"),
				new AzureKeyCredential(this.configuration.OpenAi.ApiKey));

			Response<Completions> completionsResponse = await client.GetCompletionsAsync(
				deploymentOrModelName: this.configuration.OpenAi.DeploymentId,
				new CompletionsOptions()
				{
					Prompts = { request },
					Temperature = (float)1,
					MaxTokens = 1000,
					NucleusSamplingFactor = (float)0.5,
					FrequencyPenalty = (float)0,
					PresencePenalty = (float)0,
					GenerationSampleCount = 1,
				});
			Completions completions = completionsResponse.Value;

			return completions.Choices[0].Text;
		}
	}

	public interface ISummarize
	{
		Task<string> Invoke(string url);
	}
}
