using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Designation : IDesignation
	{
		private readonly IConfiguration _configuration;
		public Designation(IConfiguration configuration)
		{
			this._configuration = configuration;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public List<DesignationViewModel> GetAll()
		{
			List<DesignationViewModel> designations = new();

			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					using (SqlCommand cmd = new SqlCommand("SELECT * From Designation ORDER BY DesignationName", sqlConnection))
					{
						if (sqlConnection.State == ConnectionState.Closed)
						{
							sqlConnection.Open();
						}

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							while (dr.Read())
							{
								designations.Add(new DesignationViewModel
								{
									DesignationId = dr.GetInt32(dr.GetOrdinal("DesignationId")),
									DesignationName = dr.GetString(dr.GetOrdinal("DesignationName"))
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener las nomenclaturas", ex);
			}

			return designations;
		}
	}
}
