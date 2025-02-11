using Microsoft.Extensions.Configuration;
using SuperTransp.Models;
using Microsoft.Data.SqlClient;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Geography : IGeography
	{
		private readonly IConfiguration _configuration;
		public Geography(IConfiguration configuration)
		{
			this._configuration = configuration;
		}
		private SqlConnection GetOpenConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			sqlConnection.Open();
			return sqlConnection;
		}

		public List<GeographyModel> GetAllStates()
		{
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					List<GeographyModel> states = new();
					SqlCommand cmd = new($"SELECT * FROM State ORDER BY StateId", sqlConnection);
					SqlDataReader dr = cmd.ExecuteReader();

					while (dr.Read())
					{
						states.Add(new GeographyModel
						{
							StateId = (int)dr["StateId"],
							StateName = (string)dr["StateName"]
						});
					}

					dr.Close();
					sqlConnection.Close();

					return states.ToList();
				};
			}
			catch (Exception)
			{
				throw;
			}
		}
		public List<GeographyModel> GetStateById(int stateId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					List<GeographyModel> state = new();
					SqlCommand cmd = new($"SELECT * FROM State Where StateId = {stateId}", sqlConnection);
					SqlDataReader dr = cmd.ExecuteReader();

					while (dr.Read())
					{
						state.Add(new GeographyModel
						{
							StateId = (int)dr["StateId"],
							StateName = (string)dr["StateName"]
						});
					}

					dr.Close();
					sqlConnection.Close();

					return state.ToList();
				};
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
