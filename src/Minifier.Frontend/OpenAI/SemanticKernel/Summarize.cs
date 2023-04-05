using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Orchestration;

namespace Minifier.Frontend.OpenAI.SemanticKernel
{
	internal class Summarize : ISummarize
	{
		private readonly Configuration configuration;
		private readonly ILogger<Summarize> logger;
		private bool useSemanticKernel = true;

		public Summarize(
			Configuration configuration,
			ILogger<Summarize> logger)
		{
			this.configuration = configuration;
			this.logger = logger;
		}

		public async Task<string> Invoke(string url)
		{
			var kernel = KernelFactory.CreateForRequest(
				configuration.OpenAi,
				logger);
			const string summaryFunctionName = "summarize";
			var summarizeFunction = kernel.Skills.GetFunction(KernelFactory.MinifierSkills, summaryFunctionName);
			var contextVariables = new ContextVariables();
			contextVariables.Set("url", url);

			var result = await kernel.RunAsync(contextVariables, summarizeFunction);

			if (result.ErrorOccurred)
			{
				throw new Exception(result.LastErrorDescription);
			}

			return result.Result;
		}

		private async Task<string> InvokeOpenAiRaw(string url)
		{
			var request = $"Summarize the contents of the following web page: {url}";
			OpenAIClient client = new OpenAIClient(
				new Uri("https://westeurope.api.cognitive.microsoft.com/"),
				new AzureKeyCredential(configuration.OpenAi.ApiKey));

			Response<Completions> completionsResponse = await client.GetCompletionsAsync(
				deploymentOrModelName: configuration.OpenAi.DeploymentId,
				new CompletionsOptions()
				{
					Prompts = { request },
					Temperature = 1,
					MaxTokens = 1000,
					NucleusSamplingFactor = (float)0.5,
					FrequencyPenalty = 0,
					PresencePenalty = 0,
					GenerationSampleCount = 1,
				});
			Completions completions = completionsResponse.Value;

			return completions.Choices[0].Text;
		}
	}
}
