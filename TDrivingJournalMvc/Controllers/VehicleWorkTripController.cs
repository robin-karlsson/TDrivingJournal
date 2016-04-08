using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;

namespace TDrivingJournalMvc.Controllers
{
	public class VehicleWorkTripController : ApiController
	{
		[RequireAPIKey]
		public HttpResponseMessage Get(long vehicleId)
		{
			var pageSize = 10;
			var page = 0;
			var model = new List<WorkTripModel>();
			using (var connection =
				new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					var command = connection.CreateCommand();
					command.CommandText = "SELECT TDJ_WorkTrip.Id,TDJ_WorkTrip.VehicleId,TDJ_WorkTrip.UserId,TDJ_WorkTrip.StartMileage,TDJ_WorkTrip.StartLat,TDJ_WorkTrip.StartLng,TDJ_WorkTrip.EndMileage,TDJ_WorkTrip.EndLat,TDJ_WorkTrip.EndLng,TDJ_WorkTrip.Commenced,TDJ_WorkTrip.Note FROM TDJ_WorkTrip INNER JOIN TDJ_User ON TDJ_WorkTrip.UserId = TDJ_User.Id WHERE TDJ_User.Email=?email AND TDJ_WorkTrip.VehicleId=?vehicleId ORDER BY StartMileage DESC LIMIT ?limit OFFSET ?offset";
					command.Parameters.Add("?email", MySqlDbType.VarChar, 400).Value = Request.Properties["Email"];
					command.Parameters.Add("?vehicleId", MySqlDbType.Int64).Value = vehicleId;
					command.Parameters.Add("?limit", MySqlDbType.Int32).Value = pageSize;
					command.Parameters.Add("?offset", MySqlDbType.Int32).Value = page*pageSize;
					command.Transaction = transaction;
					using (var result = command.ExecuteReader())
					{
						while (result.Read())
						{
							var endLatColumn = result.GetOrdinal("EndLat");
							var endLngColumn = result.GetOrdinal("EndLng");
							var startLatColumn = result.GetOrdinal("StartLat");
							var startLngColumn = result.GetOrdinal("StartLng");
							var startMileageColumn = result.GetOrdinal("StartMileage");
							var endMileageColumn = result.GetOrdinal("EndMileage");
							var noteColumn = result.GetOrdinal("Note");
							var commencedColumn = result.GetOrdinal("Commenced");
							var idColumn = result.GetOrdinal("Id");
							var vehicleIdColumn = result.GetOrdinal("VehicleId");
							while (result.Read())
							{
								model.Add(new WorkTripModel
								{
									Commenced = result.GetDateTime(commencedColumn),
									EndLat = result.IsDBNull(endLatColumn) ? null : result.GetString(endLatColumn),
									EndLng = result.IsDBNull(endLngColumn) ? null : result.GetString(endLngColumn),
									EndMileage = result.IsDBNull(endMileageColumn) ? (long?)null : result.GetInt64(endMileageColumn),
									Id = result.GetInt32(idColumn),
									Note = result.IsDBNull(noteColumn) ? null : result.GetString(noteColumn),
									StartLat = result.IsDBNull(startLatColumn) ? null : result.GetString(startLatColumn),
									StartLng = result.IsDBNull(startLngColumn) ? null : result.GetString(startLngColumn),
									StartMileage = result.GetInt64(startMileageColumn),
									VehicleId = result.GetInt64(vehicleIdColumn)
								});
							}
						}
					}

					transaction.Commit();
				}
			}
			return Request.CreateResponse(HttpStatusCode.OK, model);
		}
	}
}