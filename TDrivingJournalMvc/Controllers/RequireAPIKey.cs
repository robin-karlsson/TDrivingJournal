using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace TDrivingJournalMvc.Controllers
{
	public class RequireAPIKey : ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext context)
		{
			var apiKeyHeader = context.Request.Headers.SingleOrDefault(x => x.Key == "ApiKey");
			var emailHeader = context.Request.Headers.SingleOrDefault(x => x.Key == "Email");

			var valid = apiKeyHeader.Value != null && emailHeader.Value != null;
			if (!valid)
			{
				context.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
			}
			else
			{
				context.Request.Properties.Add("ApiKey", apiKeyHeader.Value.First());
				context.Request.Properties.Add("Email", emailHeader.Value.First());
			}
		}
	}
}