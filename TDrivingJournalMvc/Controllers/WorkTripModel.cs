using System;

namespace TDrivingJournalMvc.Controllers
{
	public class WorkTripModel
	{
		public int Id { get; set; }
		public long VehicleId { get; set; }
		public long StartMileage { get; set; }
		public string StartLat { get; set; }
		public string StartLng { get; set; }
		public DateTime? Commenced { get; set; }
		public string Note { get; set; }
		public long? EndMileage { get; set; }
		public string EndLat { get; set; }
		public string EndLng { get; set; }

		public long? Distance
		{
			get { return EndMileage.HasValue ? EndMileage.Value - StartMileage : (long?) null; }
		}
	}
}