using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;

namespace TDrivingJournalMvc.Controllers
{
	public class TokenController : ApiController
	{
		public HttpResponseMessage Get()
		{
			var tokenCookie = Request.Headers.GetCookies("token").FirstOrDefault();
			var emailCookie = Request.Headers.GetCookies("email").FirstOrDefault();

			if (tokenCookie != null && emailCookie != null)
			{
				return Request.CreateResponse(HttpStatusCode.OK,
					new {token = tokenCookie["token"].Value, email = emailCookie["email"].Value});
			}
			return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not cookie authenticated.");
		}

		public HttpResponseMessage Logon([FromBody]AuthenticationInput model)
		{
			var result = new TeslaClient().Post("https://owner-api.teslamotors.com/oauth/token",
				new
				{
					grant_type = "password",
					client_id = ConfigurationManager.AppSettings["tesla_client_id"],
					client_secret = ConfigurationManager.AppSettings["tesla_client_secret"],
					email = model.Email,
					password = model.Password
				});

			JToken token;
			if (result.TryGetValue("access_token", out token))
			{
				using (var connection =
					new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
				{
					connection.Open();
					using (var transaction = connection.BeginTransaction())
					{
						var command = connection.CreateCommand();
						command.CommandText = "INSERT IGNORE INTO TDJ_User (Email) VALUES (?email)";
						command.Parameters.Add("?email", MySqlDbType.VarChar, 400).Value = model.Email;
						command.Transaction = transaction;
						command.ExecuteNonQuery();

						transaction.Commit();
					}
				}

				var resp = Request.CreateResponse(HttpStatusCode.OK, new { token = token.Value<string>(), email = model.Email });

				var cookieToken = new CookieHeaderValue("token", token.Value<string>());
				cookieToken.Expires = DateTimeOffset.Now.AddDays(4);
				cookieToken.Domain = Request.RequestUri.Host;
				cookieToken.Path = "/";
				cookieToken.HttpOnly = true;

				var cookieEmail = new CookieHeaderValue("email", model.Email);
				cookieEmail.Expires = DateTimeOffset.Now.AddDays(4);
				cookieEmail.Domain = Request.RequestUri.Host;
				cookieEmail.Path = "/";
				cookieEmail.HttpOnly = true;
				resp.Headers.AddCookies(new[] { cookieToken, cookieEmail });

				return resp;
			}
			return Request.CreateErrorResponse(HttpStatusCode.Forbidden,"Authentication failed!");
		}

		public HttpResponseMessage Delete()
		{
			var resp = Request.CreateResponse(HttpStatusCode.OK);
			var cookieToken = new CookieHeaderValue("token", "");
			cookieToken.Expires = DateTimeOffset.Now.AddDays(-4);
			cookieToken.Domain = Request.RequestUri.Host;
			cookieToken.Path = "/";
			cookieToken.HttpOnly = true;

			var cookieEmail = new CookieHeaderValue("email", "");
			cookieEmail.Expires = DateTimeOffset.Now.AddDays(-4);
			cookieEmail.Domain = Request.RequestUri.Host;
			cookieEmail.Path = "/";
			cookieEmail.HttpOnly = true;
			resp.Headers.AddCookies(new[] { cookieToken, cookieEmail });
			return resp;
		}
	}
}