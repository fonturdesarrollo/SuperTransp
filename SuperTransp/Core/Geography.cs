using Microsoft.Extensions.Configuration;
using SuperTransp.Models;
using Microsoft.Data.SqlClient;
using static SuperTransp.Core.Interfaces;
using System.Data;

namespace SuperTransp.Core
{
	public class Geography : IGeography
	{
		private readonly IConfiguration _configuration;
		public Geography(IConfiguration configuration)
		{
			this._configuration = configuration;
		}
		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public List<GeographyModel> GetAllStates()
		{
			List<GeographyModel> states = new();

			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					using (SqlCommand cmd = new SqlCommand("SELECT * FROM State ORDER BY StateName", sqlConnection))
					{
						if (sqlConnection.State == ConnectionState.Closed)
						{
							sqlConnection.Open();
						}

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							while (dr.Read())
							{
								states.Add(new GeographyModel
								{
									StateId = dr.GetInt32(dr.GetOrdinal("StateId")),
									StateName = dr.GetString(dr.GetOrdinal("StateName"))
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener los estados", ex);
			}

			return states;
		}

		public List<GeographyModel> GetStateById(int stateId)
		{
			List<GeographyModel> state = new();

			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					using (SqlCommand cmd = new SqlCommand("SELECT * FROM State WHERE StateId = @StateId", sqlConnection))
					{
						cmd.Parameters.AddWithValue("@StateId", stateId);
						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							while (dr.Read())
							{
								state.Add(new GeographyModel
								{
									StateId = (int)dr["StateId"],
									StateName = (string)dr["StateName"]
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw;
			}

			return state;
		}

		public List<GeographyModel> GetAllMunicipalities()
		{
			List<GeographyModel> municipalities = new();

			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					using (SqlCommand cmd = new SqlCommand("SELECT * FROM Municipality ORDER BY MunicipalityName", sqlConnection))
					{
						if (sqlConnection.State == ConnectionState.Closed)
						{
							sqlConnection.Open();
						}

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							while (dr.Read())
							{
								municipalities.Add(new GeographyModel
								{
									MunicipalityId = dr.GetInt32(dr.GetOrdinal("MunicipalityId")),
									MunicipalityName = dr.GetString(dr.GetOrdinal("MunicipalityName"))
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener los municipios", ex);
			}

			return municipalities;
		}

		public List<GeographyModel> GetMunicipalityByStateId(int stateId)
		{
			List<GeographyModel> municipality = new();

			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					using (SqlCommand cmd = new SqlCommand("SELECT * FROM Geography_GetMunicipalityByStateId WHERE StateId = @StateId", sqlConnection))
					{
						cmd.Parameters.AddWithValue("@StateId", stateId);
						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							while (dr.Read())
							{
								municipality.Add(new GeographyModel
								{
									MunicipalityId = (int)dr["MunicipalityId"],
									MunicipalityName = (string)dr["MunicipalityName"]
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw;
			}

			return municipality;
		}
	}
}
