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

		public int AddOrEdit(DesignationViewModel model)
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
						SqlCommand cmd = new("SuperTransp_DesignationAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@DesignationId", model.DesignationId);
						cmd.Parameters.AddWithValue("@DesignationName", model.DesignationName);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al añadir o editar la entidad legal", ex);
			}
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
				throw new Exception("Error al obtener la entidad legal", ex);
			}

			return designations;
		}
	}
}
