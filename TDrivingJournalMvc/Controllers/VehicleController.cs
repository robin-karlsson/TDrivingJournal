using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace TDrivingJournalMvc.Controllers
{
	public class VehicleController : ApiController
	{
		[RequireAPIKey]
		public async Task<HttpResponseMessage> Get()
		{
			var cars = await 
				new TeslaClient((string)Request.Properties["ApiKey"]).Get("https://owner-api.teslamotors.com/api/1/vehicles");
			var vehicles = cars["response"].Select(c => new
			{
				state = c["state"],
				token = c["tokens"].First,
				vehicleId = c["vehicle_id"],
				displayName = c["display_name"],
				id = c["id_s"]
			}).ToArray();
			return Request.CreateResponse(HttpStatusCode.OK, vehicles);
		}

		[RequireAPIKey]
		public async Task<HttpResponseMessage> Get(string id)
		{
			var cars = await 
				new TeslaClient((string)Request.Properties["ApiKey"]).Get(string.Format("https://owner-api.teslamotors.com/api/1/vehicles/{0}",id));
			var c = cars["response"];
			var vehicle = new {
				state = c["state"],
				token = c["tokens"].First,
				vehicleId = c["vehicle_id"],
				displayName = c["display_name"],
				id = c["id_s"]
			};
			return Request.CreateResponse(HttpStatusCode.OK, vehicle);
		}
	}
}