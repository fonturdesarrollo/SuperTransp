using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Mode : IMode
	{
		private readonly IConfiguration _configuration;
		public Mode(IConfiguration configuration)
		{
			this._configuration = configuration;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public List<ModeViewModel> GetAll()
		{
			List<ModeViewModel> modes = new();

			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					using (SqlCommand cmd = new SqlCommand("SELECT * FROM Mode ORDER BY ModeName", sqlConnection))
					{
						if (sqlConnection.State == ConnectionState.Closed)
						{
							sqlConnection.Open();
						}

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							while (dr.Read())
							{
								modes.Add(new ModeViewModel
								{
									ModeId = dr.GetInt32(dr.GetOrdinal("ModeId")),
									ModeName = dr.GetString(dr.GetOrdinal("ModeName"))
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener las Modalidades", ex);
			}

			return modes;
		}
	}
}
