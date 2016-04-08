using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;

namespace TDrivingJournalMvc.Controllers
{
	public class WorkTripReportController : ApiController
	{
		[RequireAPIKey]
		public HttpResponseMessage Get()
		{
			const string query = "SELECT SUM(TDJ_WorkTrip.EndMileage-TDJ_WorkTrip.StartMileage) Distance,TDJ_WorkTrip.VehicleId, STR_TO_DATE(DATE_FORMAT(TDJ_WorkTrip.Commenced ,'%Y-%m-01'),'%Y-%m-%d') AS Month FROM TDJ_WorkTrip INNER JOIN TDJ_User ON TDJ_WorkTrip.UserId = TDJ_User.Id WHERE TDJ_User.Email=?email GROUP BY TDJ_WorkTrip.VehicleId, Month ORDER BY Month DESC LIMIT ?limit";
			const int pageSize = 10;
			var model = new List<WorkTripReportModel>();
			using (var connection =
				new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					var command = connection.CreateCommand();
					command.CommandText = query;
					command.Parameters.Add("?email", MySqlDbType.VarChar, 400).Value = Request.Properties["Email"];
					command.Parameters.Add("?limit", MySqlDbType.Int32).Value = pageSize;
					command.Transaction = transaction;
					using (var result = command.ExecuteReader())
					{
						while (result.Read())
						{
							model.Add(new WorkTripReportModel
							{
								FirstDayInMonth = result.GetDateTime("Month"),
								Distance = result.GetInt64("Distance"),
								VehicleId = result.GetInt64("VehicleId")
							});
						}
					}

					transaction.Commit();
				}
			}
			return Request.CreateResponse(HttpStatusCode.OK, model);
		}
	}
}