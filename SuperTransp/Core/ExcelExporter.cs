using ClosedXML.Excel;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{

	public class ExcelExporter : IExcelExporter
	{
		private readonly IConfiguration _configuration;
		public ExcelExporter(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public async Task<byte[]> GenerateExcelPublicTransportGroupAndDriversAsync(int stateId)
		{
			var dt = new DataTable();

			var whereClause = stateId > 0 ? "WHERE [Codigo Estado] = @StateId" : string.Empty;

			using (var con = (GetConnection()))
			using (var cmd = new SqlCommand($"SELECT * FROM SuperTransp_PublicTransportGroupAndDrivers {whereClause} ORDER BY Estado, Municipio, Organizacion", con))
			{
				if(stateId > 0)
				{
					cmd.Parameters.AddWithValue("@StateId", stateId);
				}

				await con.OpenAsync();
				using (var reader = await cmd.ExecuteReaderAsync())
				{
					dt.Load(reader);
				}
			}

			using (var workbook = new XLWorkbook())
			using (var stream = new MemoryStream())
			{
				workbook.Worksheets.Add(dt, "OrganizacionesYSocios");
				workbook.SaveAs(stream);
				return stream.ToArray();
			}
		}

		public async Task<byte[]> GenerateExcelSupervisionDetailAsync(int stateId)
		{
			var dt = new DataTable();

			var whereClause = stateId > 0 ? "WHERE StateId = @StateId" : string.Empty;

			using (var con = (GetConnection()))
			using (var cmd = new SqlCommand($"SELECT * FROM SuperTransp_SupervisionDetail {whereClause} ORDER BY Estado, Municipio, Organizacion", con))
			{
				if (stateId > 0)
				{
					cmd.Parameters.AddWithValue("@StateId", stateId);
				}

				await con.OpenAsync();
				using (var reader = await cmd.ExecuteReaderAsync())
				{
					dt.Load(reader);
				}
			}

			using (var workbook = new XLWorkbook())
			using (var stream = new MemoryStream())
			{
				workbook.Worksheets.Add(dt, "DetalleDeSupervisión");
				workbook.SaveAs(stream);
				return stream.ToArray();
			}
		}
	}
}
