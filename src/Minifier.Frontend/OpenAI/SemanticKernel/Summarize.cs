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

			return result.Result.Trim();
		}
	}
}
