using Microsoft.Extensions.Logging;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.KernelExtensions;
using Microsoft.SemanticKernel.TemplateEngine;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Minifier.Frontend.OpenAI.SemanticKernel
{
	internal class KernelFactory
	{
		internal const string SummarizeFunctionName = "summarize";

		private static IEnumerable<string> skillsToLoad = new List<string>
		{
			SummarizeFunctionName
		};

		internal static IKernel CreateForRequest(
			OpenAi openAiConfiguration,
			ILogger logger)
		{
			KernelBuilder builder = Kernel.Builder;
			builder = ConfigureKernelBuilder(openAiConfiguration, builder);
			return CompleteKernelSetup(builder, logger);
		}

		private static KernelBuilder ConfigureKernelBuilder(
			OpenAi openAiConfiguration,
			KernelBuilder builder
			)
		{
			builder = builder
				.Configure(c =>
				{
					c.AddAzureOpenAITextCompletionService(
						openAiConfiguration.DeploymentId,
						openAiConfiguration.DeploymentId,
						openAiConfiguration.Endpoint,
						openAiConfiguration.ApiKey);
				});

			return builder;
		}

		private static IKernel CompleteKernelSetup(
			KernelBuilder builder,
			ILogger logger)
		{
			IKernel kernel = builder.Build();

			RegisterSemanticSkills(kernel, SampleSkillsPath(), logger);

			return kernel;
		}

		private static void RegisterSemanticSkills(
			IKernel kernel,
			string skillsFolder,
			ILogger logger)
		{
			foreach (string skPromptPath in Directory.EnumerateFiles(skillsFolder, "*.txt", SearchOption.AllDirectories))
			{
				FileInfo fInfo = new(skPromptPath);
				DirectoryInfo? currentFolder = fInfo.Directory;

				while (currentFolder?.Parent?.FullName != skillsFolder)
				{
					currentFolder = currentFolder?.Parent;
				}

				if (ShouldLoad(currentFolder.Name, skillsToLoad))
				{
					try
					{
						_ = kernel.ImportSemanticSkillFromDirectory(skillsFolder, currentFolder.Name);
					}
					catch (TemplateException e)
					{
						logger.LogWarning("Could not load skill from {0} with error: {1}", currentFolder.Name, e.Message);
					}
				}
			}
		}

		private static string SampleSkillsPath()
		{
			string skillsPath =
				"OpenAI" + Path.DirectorySeparatorChar +
				"SemanticKernel" + Path.DirectorySeparatorChar +
				"skills";

			bool SearchPath(string pathToFind, out string result, int maxAttempts = 10)
			{
				var currDir = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
				bool found;
				do
				{
					result = Path.Join(currDir, pathToFind);
					found = Directory.Exists(result);
					currDir = Path.GetFullPath(Path.Combine(currDir, ".."));
				} while (maxAttempts-- > 0 && !found);

				return found;
			}

			if (!SearchPath(skillsPath, out string path))
			{
				throw new ConfigurationErrorsException("Skills directory not found.");
			}

			return path;
		}

		private static bool ShouldLoad(string skillName, IEnumerable<string> skillsToLoad)
		{
			return skillsToLoad.Contains(skillName, StringComparer.InvariantCultureIgnoreCase) != false;
		}
	}
}
