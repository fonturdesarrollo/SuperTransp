using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Union : IUnion
	{
		private readonly IConfiguration _configuration;
		public Union(IConfiguration configuration)
		{
			this._configuration = configuration;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public List<UnionViewModel> GetAll()
		{
			List<UnionViewModel> unions = new();

			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					using (SqlCommand cmd = new SqlCommand("SELECT * FROM [Union] ORDER BY [UnionName]", sqlConnection))
					{
						if (sqlConnection.State == ConnectionState.Closed)
						{
							sqlConnection.Open();
						}

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							while (dr.Read())
							{
								unions.Add(new UnionViewModel
								{
									UnionId = dr.GetInt32(dr.GetOrdinal("UnionId")),
									UnionName = dr.GetString(dr.GetOrdinal("UnionName"))
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener los gremios", ex);
			}

			return unions;
		}
	}
}
