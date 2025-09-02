using Microsoft.Data.SqlClient;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Universe : IUniverse
	{
		private readonly IConfiguration _configuration;
		public Universe(IConfiguration configuration)
		{
			this._configuration = configuration;
		}
		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public int AddOrEdit(UniverseViewModel model)
		{
			int result = 0;
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					if (model != null)
					{
						SqlCommand cmd = new("SuperTransp_UniverseAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@UniverseId", model.UniverseId);
						cmd.Parameters.AddWithValue("@StateId", model.StateId);
						cmd.Parameters.AddWithValue("@TotalPublicTransportGroups", model.TotalPublicTransportGroups);
						cmd.Parameters.AddWithValue("@TotalDrivers", model.TotalDrivers);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al añadir o editar el universo", ex);
			}
		}

		public UniverseViewModel GetByStateId(int stateId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					UniverseViewModel universe = new();
					SqlCommand cmd = new("SELECT * FROM Universe WHERE StateId = @StateId", sqlConnection);
					cmd.Parameters.AddWithValue("@StateId", stateId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							universe.UniverseId = (int)dr["UniverseId"];
							universe.StateId = (int)dr["StateId"];
							universe.TotalPublicTransportGroups = (int)dr["TotalPublicTransportGroups"];
							universe.TotalDrivers = (int)dr["TotalDrivers"];
						}
					}

					return universe;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener el universo por estado {ex.Message}", ex);
			}
		}
	}
}
