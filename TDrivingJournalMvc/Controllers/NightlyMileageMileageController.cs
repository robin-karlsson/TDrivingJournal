using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using MySql.Data.MySqlClient;

namespace TDrivingJournalMvc.Controllers
{
	public class NightlyMileageMileageController : ApiController
	{
		public async Task<HttpResponseMessage> Get()
		{
			var vehiclesToGetMileageFor = new List<TrackMileage>();

			using (var connection =
				new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					var command = connection.CreateCommand();
					command.CommandText =
						"SELECT TDJ_TrackMileage.Id,TDJ_TrackMileage.VehicleId,TDJ_TrackMileage.Email,TDJ_TrackMileage.UserId,TDJ_TrackMileage.Token FROM TDJ_TrackMileage";
					command.Transaction = transaction;
					using (var result = command.ExecuteReader())
					{
						while (result.Read())
						{
							var idColumn = result.GetOrdinal("Id");
							var vehicleIdColumn = result.GetOrdinal("VehicleId");
							var userIdColumn = result.GetOrdinal("UserId");
							var emailColumn = result.GetOrdinal("Email");
							var tokenColumn = result.GetOrdinal("Token");
							while (result.Read())
							{
								vehiclesToGetMileageFor.Add(new TrackMileage
								{
									Id = result.GetInt32(idColumn),
									UserId = result.GetInt32(userIdColumn),
									Email = result.GetString(emailColumn),
									VehicleId = result.GetString(vehicleIdColumn),
									Token = result.GetString(tokenColumn)
								});
							}
						}
					}

					transaction.Commit();
				}
			}

			using (var connection =
				new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					foreach (var trackMileage in vehiclesToGetMileageFor)
					{
						var cars =
							await
								new TeslaClient(trackMileage.Token).Get(string.Format("https://owner-api.teslamotors.com/api/1/vehicles/{0}",
									trackMileage.VehicleId));
						var c = cars["response"];
						var vehicle = new
						{
							token = c["tokens"].First,
							id = c["id_s"]
						};

						using (var client = new HttpClient())
						{
							var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", trackMileage.Email, vehicle.token));
							client.DefaultRequestHeaders.Add("Authorization",
								string.Format("Basic {0}", Convert.ToBase64String(byteArray)));

							client.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);
							try
							{
								var stream =
									await
										client.GetStreamAsync(
											string.Format("https://streaming.vn.teslamotors.com/stream/{0}/?values=odometer,est_lat,est_lng", vehicle.id));

								using (var reader = new StreamReader(stream))
								{
									while (!reader.EndOfStream)
									{
										string line = reader.ReadLine();
										if (!string.IsNullOrEmpty(line))
										{
											var values = line.Split(',');

											var command = connection.CreateCommand();
											command.CommandText =
												"INSERT INTO TDJ_Mileage (VehicleId,UserId,Mileage,Date,Note) VALUES (?vehicleId,(SELECT Id FROM TDJ_User WHERE Email=?email),?mileage,?date,?note)";
											command.Parameters.Add("?mileage", MySqlDbType.Int64).Value = Convert.ToInt64(values[1]);
											command.Parameters.Add("?date", MySqlDbType.DateTime).Value = DateTime.Now;
											command.Parameters.Add("?note", MySqlDbType.VarChar, 400).Value = string.Empty;
											command.Parameters.Add("?email", MySqlDbType.VarChar, 400).Value = trackMileage.Email;
											command.Parameters.Add("?vehicleId", MySqlDbType.Int64).Value = trackMileage.VehicleId;
											command.Transaction = transaction;
											command.ExecuteNonQuery();

											break;
										}
									}
								}
							}
							catch (Exception)
							{
								Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "Wasn't able to connect to Tesla to get readings.");
							}
						}

					}

					transaction.Commit();
				}
			}

			return Request.CreateResponse(HttpStatusCode.OK);
		}
	}
}