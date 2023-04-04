using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Minifier.Frontend
{
	public class Summarize
	{
		private readonly Configuration configuration;
		private readonly IGetFullUrlFromSlug getFullUrlFromSlug;
		private readonly ILogger<Summarize> logger;

		public Summarize(
			IGetFullUrlFromSlug getFullUrlFromSlug,
			ILogger<Summarize> logger)
		{
			this.configuration = new Configuration();
			this.getFullUrlFromSlug = getFullUrlFromSlug;
			this.logger = logger;
		}

		[FunctionName(nameof(Summarize))]
		public async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "summarize/{slug}")]
			HttpRequest req,
			string slug)
		{
			this.logger.LogInformation("Requesting summary for `{slug}`.", slug);

			return new OkObjectResult("ok");
		}
	}
}
