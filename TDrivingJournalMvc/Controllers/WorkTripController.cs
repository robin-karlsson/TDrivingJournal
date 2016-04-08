using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;

namespace TDrivingJournalMvc.Controllers
{
	public class WorkTripController : ApiController
	{
		[RequireAPIKey]
		public HttpResponseMessage Get()
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
					command.CommandText = "SELECT TDJ_WorkTrip.Id,TDJ_WorkTrip.VehicleId,TDJ_WorkTrip.UserId,TDJ_WorkTrip.StartMileage,TDJ_WorkTrip.StartLat,TDJ_WorkTrip.StartLng,TDJ_WorkTrip.EndMileage,TDJ_WorkTrip.EndLat,TDJ_WorkTrip.EndLng,TDJ_WorkTrip.Commenced,TDJ_WorkTrip.Note FROM TDJ_WorkTrip INNER JOIN TDJ_User ON TDJ_WorkTrip.UserId = TDJ_User.Id WHERE TDJ_User.Email=?email ORDER BY Commenced DESC LIMIT ?limit OFFSET ?offset";
					command.Parameters.Add("?email", MySqlDbType.VarChar, 400).Value = Request.Properties["Email"];
					command.Parameters.Add("?limit", MySqlDbType.Int32).Value = pageSize;
					command.Parameters.Add("?offset", MySqlDbType.Int32).Value = page * pageSize;
					command.Transaction = transaction;
					using (var result = command.ExecuteReader())
					{
						var endLatColumn = result.GetOrdinal("EndLat");
						var endLngColumn = result.GetOrdinal("EndLng");
						var startLatColumn = result.GetOrdinal("StartLat");
						var startLngColumn = result.GetOrdinal("StartLng");
						var endMileageColumn = result.GetOrdinal("EndMileage");
						var noteColumn = result.GetOrdinal("Note");
						while (result.Read())
						{
							model.Add(new WorkTripModel
							{
								Commenced = result.GetDateTime("Commenced"),
								EndLat = result.IsDBNull(endLatColumn) ? null : result.GetString(endLatColumn),
								EndLng = result.IsDBNull(endLngColumn) ? null : result.GetString(endLngColumn),
								EndMileage = result.IsDBNull(endMileageColumn) ? (long?)null : result.GetInt64(endMileageColumn),
								Id = result.GetInt32("Id"),
								Note = result.IsDBNull(noteColumn) ? null : result.GetString(noteColumn),
								StartLat = result.IsDBNull(startLatColumn) ? null : result.GetString(startLatColumn),
								StartLng = result.IsDBNull(startLngColumn) ? null : result.GetString(startLngColumn),
								StartMileage = result.GetInt64("StartMileage"),
								VehicleId = result.GetInt64("VehicleId")
							});
						}
					}

					transaction.Commit();
				}
			}
			return Request.CreateResponse(HttpStatusCode.OK, model);
		}

		[RequireAPIKey]
		public HttpResponseMessage Get(int id)
		{
			WorkTripModel model = null;
			using (var connection =
				new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					var command = connection.CreateCommand();
					command.CommandText = "SELECT TDJ_WorkTrip.Id,StartMileage,StartLat,StartLng,EndMileage,EndLat,EndLng,Commenced,Note FROM TDJ_WorkTrip INNER JOIN TDJ_User ON TDJ_WorkTrip.UserId = TDJ_User.Id WHERE TDJ_User.Email=?email AND TDJ_WorkTrip.Id=?id";
					command.Parameters.Add("?email", MySqlDbType.VarChar, 400).Value = Request.Properties["Email"];
					command.Parameters.Add("?id", MySqlDbType.Int32).Value = id;
					command.Transaction = transaction;
					using (var result = command.ExecuteReader())
					{
						var endLatColumn = result.GetOrdinal("EndLat");
						var endLngColumn = result.GetOrdinal("EndLng");
						var startLatColumn = result.GetOrdinal("StartLat");
						var startLngColumn = result.GetOrdinal("StartLng");
						var endMileageColumn = result.GetOrdinal("EndMileage");
						var noteColumn = result.GetOrdinal("Note");
						while (result.Read())
						{
							model = new WorkTripModel
							{
								Commenced = result.GetDateTime("Commenced"),
								EndLat = result.IsDBNull(endLatColumn) ? null : result.GetString(endLatColumn),
								EndLng = result.IsDBNull(endLngColumn) ? null : result.GetString(endLngColumn),
								EndMileage = result.IsDBNull(endMileageColumn) ? (long?)null : result.GetInt64(endMileageColumn),
								Id = result.GetInt32("Id"),
								Note = result.IsDBNull(noteColumn) ? null : result.GetString(noteColumn),
								StartLat = result.IsDBNull(startLatColumn) ? null : result.GetString(startLatColumn),
								StartLng = result.IsDBNull(startLngColumn) ? null : result.GetString(startLngColumn),
								StartMileage = result.GetInt64("StartMileage"),
								VehicleId = result.GetInt64("VehicleId")
							};
							break;
						}
					}
					transaction.Commit();
				}
			}
			if (model == null)
				return Request.CreateErrorResponse(HttpStatusCode.NotFound,
					string.Format("A work trip with id {0} could not be found.", id));

			return Request.CreateResponse(HttpStatusCode.OK, model);
		}

		[RequireAPIKey]
		public HttpResponseMessage Post([FromBody]WorkTripModel model)
		{
			model.Commenced = model.Commenced.GetValueOrDefault(DateTime.UtcNow);
			using (var connection =
				new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					var command = connection.CreateCommand();
					command.CommandText = "INSERT INTO TDJ_WorkTrip (VehicleId,UserId,StartMileage,StartLat,StartLng,EndMileage,EndLat,EndLng,Commenced,Note) VALUES (?vehicleId,(SELECT Id FROM TDJ_User WHERE Email=?email),?startMileage,?startLat,?startLng,?endMileage,?endLat,?endLng,?commenced,?note)";
					command.Parameters.Add("?startMileage", MySqlDbType.Int64).Value = model.StartMileage;
					command.Parameters.Add("?endMileage", MySqlDbType.Int64).Value = model.EndMileage;
					command.Parameters.Add("?startLat", MySqlDbType.VarChar,50).Value = model.StartLat;
					command.Parameters.Add("?startLng", MySqlDbType.VarChar,50).Value = model.StartLng;
					command.Parameters.Add("?endLat", MySqlDbType.VarChar,50).Value = model.EndLat;
					command.Parameters.Add("?endLng", MySqlDbType.VarChar,50).Value = model.EndLng;
					command.Parameters.Add("?commenced", MySqlDbType.DateTime).Value = model.Commenced.Value;
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

		[RequireAPIKey]
		public HttpResponseMessage Put(int id, [FromBody]WorkTripModel model)
		{
			model.Commenced = model.Commenced.GetValueOrDefault(DateTime.UtcNow);
			using (var connection =
				new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					var command = connection.CreateCommand();
					command.CommandText = "UPDATE TDJ_WorkTrip JOIN TDJ_User ON TDJ_WorkTrip.UserId = TDJ_User.Id SET TDJ_WorkTrip.StartMileage=?startMileage,TDJ_WorkTrip.StartLat=?startLat,TDJ_WorkTrip.StartLng=?startLng,TDJ_WorkTrip.EndMileage=?endMileage,TDJ_WorkTrip.EndLat=?endLat,TDJ_WorkTrip.EndLng=?endLng,TDJ_WorkTrip.Commenced=?commenced,TDJ_WorkTrip.Note=?note WHERE TDJ_User.Email=?email AND TDJ_WorkTrip.Id=?id";
					command.Parameters.Add("?startMileage", MySqlDbType.Int64).Value = model.StartMileage;
					command.Parameters.Add("?endMileage", MySqlDbType.Int64).Value = model.EndMileage;
					command.Parameters.Add("?startLat", MySqlDbType.VarChar, 50).Value = model.StartLat;
					command.Parameters.Add("?startLng", MySqlDbType.VarChar, 50).Value = model.StartLng;
					command.Parameters.Add("?endLat", MySqlDbType.VarChar, 50).Value = model.EndLat;
					command.Parameters.Add("?endLng", MySqlDbType.VarChar, 50).Value = model.EndLng;
					command.Parameters.Add("?commenced", MySqlDbType.DateTime).Value = model.Commenced.Value;
					command.Parameters.Add("?note", MySqlDbType.VarChar, 400).Value = model.Note;
					command.Parameters.Add("?vehicleId", MySqlDbType.Int64).Value = model.VehicleId;
					command.Parameters.Add("?id", MySqlDbType.Int32).Value = model.Id;
					command.Parameters.Add("?email", MySqlDbType.VarChar, 400).Value = Request.Properties["Email"];
					command.Transaction = transaction;
					command.ExecuteNonQuery();

					transaction.Commit();
				}
			}
			return Request.CreateResponse(HttpStatusCode.OK, model);
		}

		[RequireAPIKey]
		public HttpResponseMessage Delete(int id)
		{
			using (var connection =
				new MySqlConnection(ConfigurationManager.ConnectionStrings["db"].ConnectionString))
			{
				connection.Open();
				using (var transaction = connection.BeginTransaction())
				{
					var command = connection.CreateCommand();
					command.CommandText = "DELETE TDJ_WorkTrip FROM TDJ_WorkTrip INNER JOIN TDJ_User ON TDJ_WorkTrip.UserId = TDJ_User.Id WHERE TDJ_User.Email=?email AND TDJ_WorkTrip.Id=?id";
					command.Parameters.Add("?email", MySqlDbType.VarChar, 400).Value = Request.Properties["Email"];
					command.Parameters.Add("?id", MySqlDbType.Int32).Value = id;
					command.Transaction = transaction;
					command.ExecuteNonQuery();

					transaction.Commit();
				}
			}
			
			return Request.CreateResponse(HttpStatusCode.OK);
		}
	}
}