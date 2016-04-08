using System;

namespace TDrivingJournalMvc.Controllers
{
	public class MileageModel
	{
		public long Mileage { get; set; }
		public DateTime Date { get; set; }
		public string Note { get; set; }
		public long VehicleId { get; set; }
		public int Id { get; set; }
	}
}