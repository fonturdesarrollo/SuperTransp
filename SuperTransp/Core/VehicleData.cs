using Microsoft.Data.SqlClient;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class VehicleData : IVehicleData
	{
		private readonly IConfiguration _configuration;
		public VehicleData(IConfiguration configuration)
		{
			this._configuration = configuration;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public List<VehicleDataViewModel> GetAll()
		{
			List<VehicleDataViewModel> vehicleData = new();

			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					using (SqlCommand cmd = new SqlCommand("SELECT * From VehicleData ORDER BY Year", sqlConnection))
					{
						if (sqlConnection.State == ConnectionState.Closed)
						{
							sqlConnection.Open();
						}

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							while (dr.Read())
							{
								vehicleData.Add(new VehicleDataViewModel
								{
									VehicleDataId = dr.GetInt32(dr.GetOrdinal("vehicleData")),
									Year = dr.GetInt32(dr.GetOrdinal("Year")),
									Make = dr.GetString(dr.GetOrdinal("Make")),
									Model = dr.GetString(dr.GetOrdinal("Model"))
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener la data del vehículo {ex.Message}", ex);
			}

			return vehicleData;
		}
	}
}
