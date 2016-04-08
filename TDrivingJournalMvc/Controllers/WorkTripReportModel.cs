using System;

namespace TDrivingJournalMvc.Controllers
{
	public class WorkTripReportModel
	{
		public long Distance { get; set; }
		public long VehicleId { get; set; }
		public DateTime FirstDayInMonth { get; set; }
	}
}