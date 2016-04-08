using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TDrivingJournalMvc.Controllers
{
	public class TeslaClient
	{
		private readonly string _token;

		public TeslaClient(string token = null)
		{
			_token = token;
		}

		public JObject Post<T>(string url, T data)
		{
			using (var client = new HttpClient())
			{
				if (!string.IsNullOrEmpty(_token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

				return JObject.Parse(client.PostAsJsonAsync(url, data).Result.Content.ReadAsStringAsync().Result);
			}
		}

		public async Task<JObject> Get(string url)
		{
			using (var client = new HttpClient())
			{
				if (!string.IsNullOrEmpty(_token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

				return JObject.Parse(await client.GetStringAsync(url));
			}
		}
	}
}