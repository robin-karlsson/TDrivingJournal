using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;

namespace TDrivingJournalMvc.Controllers
{
	public class MileageController : ApiController
	{
		[RequireAPIKey]
		public HttpResponseMessage Post([FromBody]MileageModel model)
		{
			using (var connection =
				new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					var command = connection.CreateCommand();
					command.CommandText = "INSERT INTO TDJ_Mileage (VehicleId,UserId,Mileage,Date,Note) VALUES (?vehicleId,(SELECT Id FROM TDJ_User WHERE Email=?email),?mileage,?date,?note)";
					command.Parameters.Add("?mileage", MySqlDbType.Int64).Value = model.Mileage;
					command.Parameters.Add("?date", MySqlDbType.DateTime).Value = model.Date;
					command.Parameters.Add("?note", MySqlDbType.VarChar, 400).Value = model.Note;
					command.Parameters.Add("?email", MySqlDbType.VarChar, 400).Value = Request.Properties["Email"];
					command.Parameters.Add("?vehicleId", MySqlDbType.Int64).Value = model.VehicleId;
					command.Transaction = transaction;
					command.ExecuteNonQuery();

					model.Id = (int)command.LastInsertedId;

					transaction.Commit();
				}
			}
			return Request.CreateResponse(HttpStatusCode.OK, model);
		}
	}
}