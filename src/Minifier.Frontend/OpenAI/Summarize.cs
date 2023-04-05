using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Orchestration;
using Minifier.Frontend.OpenAI.SemanticKernel;

namespace Minifier.Frontend.OpenAI
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
			if(useSemanticKernel)
			{
				return await InvokeSemanticKernel(url);
			}
			else
			{
				return await InvokeOpenAiRaw(url);
			}
		}

		private async Task<string> InvokeSemanticKernel(string url)
		{
			
			var kernel = KernelFactory.CreateForRequest(
				this.configuration.OpenAi,
				this.logger);

			var summarizeFunction = kernel.Skills.GetFunction(KernelFactory.SummarizeFunctionName);
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
