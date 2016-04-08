using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace TDrivingJournalMvc.Controllers
{
	public class VehicleStateController : ApiController
	{
		[RequireAPIKey, HttpGet]
		public async Task<HttpResponseMessage> Get(string vehicleId, string token)
		{
			using (var client = new HttpClient())
			{
				var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Request.Properties["Email"], token));
				client.DefaultRequestHeaders.Add("Authorization",
					string.Format("Basic {0}", Convert.ToBase64String(byteArray)));

				client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
				try
				{
					var stream =
						await
							client.GetStreamAsync(
								string.Format("https://streaming.vn.teslamotors.com/stream/{0}/?values=odometer,est_lat,est_lng", vehicleId));

					using (var reader = new StreamReader(stream))
					{
						while (!reader.EndOfStream)
						{
							string line = reader.ReadLine();
							if (!string.IsNullOrEmpty(line))
							{
								var values = line.Split(',');
								return Request.CreateResponse(HttpStatusCode.OK,
									new {odometer = values[1], est_lat = values[2], est_lng = values[3], original_response = line});
							}
						}
					}
				}
				catch (Exception)
				{
					Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "Wasn't able to connect to Tesla to get readings.");
				}
			}
			return Request.CreateErrorResponse(HttpStatusCode.NotFound, "No readings found.");
		}
	}
}