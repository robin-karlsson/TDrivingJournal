using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace TDrivingJournalMvc.Controllers
{
	public class ExportWorkTripController : ApiController
	{
		public HttpResponseMessage Get(string id)
		{
			DateTime date;
			if (!DateTime.TryParseExact(id, "yyyy-mm-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The given date was in a invalid format.");
			}
			using (var stream = new MemoryStream())
			{
				IWorkbook wb = new XSSFWorkbook();
				ISheet sheet = wb.CreateSheet("Sheet1");
				ICreationHelper cH = wb.GetCreationHelper();
				for (int i = 0; i < 5; i++)
				{
					IRow row = sheet.CreateRow(i);
					for (int j = 0; j < 3; j++)
					{
						ICell cell = row.CreateCell(j);
						cell.SetCellValue(cH.CreateRichTextString("Cell " + j + ", " + i));
					}
				}
				wb.Write(stream);


				var streamContent = new PushStreamContent((outputStream, httpContext, transportContent) =>
				{
					try
					{
							stream.CopyTo(outputStream);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
					finally
					{
						outputStream.Close();
					}
				});
				streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
				streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
				streamContent.Headers.ContentDisposition.FileName = "reports.xlsx";

				var result = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = streamContent
				};
				return result;
			}
		}
	}
}