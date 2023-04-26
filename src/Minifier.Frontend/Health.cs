using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Minifier.Frontend
{
	public static class Health
	{
		[Function(nameof(Live))]
		public static IActionResult Live(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/Live")]
			HttpRequestData req)
		{
			return new OkResult();
		}
	}
}
