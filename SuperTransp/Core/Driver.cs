using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SuperTransp.Models;
using System.Data;
using System.Reflection;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Driver : IDriver
	{
		private readonly IConfiguration _configuration;

		public Driver(IConfiguration configuration)
		{
			this._configuration = configuration;
		}
		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public int AddOrEdit(DriverViewModel model)
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
						SqlCommand cmd = new("SuperTransp_DriverAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@DriverId", model.DriverId);
						cmd.Parameters.AddWithValue("@DriverIdentityDocument", model.DriverIdentityDocument);
						cmd.Parameters.AddWithValue("@DriverFullName", model.DriverFullName.ToUpper().Trim());
						cmd.Parameters.AddWithValue("@PartnerNumber", model.PartnerNumber);
						cmd.Parameters.AddWithValue("@DriverPhone", model.DriverPhone);
						cmd.Parameters.AddWithValue("@PublicTransportGroupId", model.PublicTransportGroupId);
						cmd.Parameters.AddWithValue("@DriverModifiedDate", DateTime.Now);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al añadir o editar el transportista", ex);
			}
		}

		public List<DriverViewModel> GetByPublicTransportGroupId(int publicTransportGroupId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<DriverViewModel> drivers = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_DriversDetail WHERE PublicTransportGroupId = @PublicTransportGroupId", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							drivers.Add(new DriverViewModel
							{
								DriverId = (int)dr["DriverId"],
								PublicTransportGroupId = (int)dr["PublicTransportGroupId"],
								DriverIdentityDocument = (int)dr["DriverIdentityDocument"],
								DriverFullName = (string)dr["DriverFullName"],
								PartnerNumber = (int)dr["PartnerNumber"],
								DriverPhone = (string)dr["DriverPhone"],
								PTGCompleteName = (string)dr["PTGCompleteName"],
								DriverPublicTransportGroupId = (int)dr["DriverPublicTransportGroupId"]
							});
						}
					}

					return drivers;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener los transportistas", ex);
			}
		}

		public DriverViewModel GetByDriverPublicTransportGroupId(int driverPublicTransportGroupId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					DriverViewModel driver = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_DriversDetail WHERE DriverPublicTransportGroupId = @DriverPublicTransportGroupId", sqlConnection);
					cmd.Parameters.AddWithValue("@DriverPublicTransportGroupId", driverPublicTransportGroupId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							driver.DriverId = (int)dr["DriverId"];
							driver.PublicTransportGroupId = (int)dr["PublicTransportGroupId"];
							driver.DriverIdentityDocument = (int)dr["DriverIdentityDocument"];
							driver.DriverFullName = (string)dr["DriverFullName"];
							driver.PartnerNumber = (int)dr["PartnerNumber"];
							driver.DriverPhone = (string)dr["DriverPhone"];
							driver.PTGCompleteName = (string)dr["PTGCompleteName"];
							driver.DriverPublicTransportGroupId = (int)dr["DriverPublicTransportGroupId"];
						}
					}

					return driver;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener los transportistas", ex);
			}
		}

		public List<DriverViewModel> GetAll()
		{
			throw new NotImplementedException();
		}

		public List<DriverViewModel> GetByStateId(int stateId)
		{
			throw new NotImplementedException();
		}

		public bool Delete(int driverId)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				SqlCommand cmd = new("DELETE FROM Driver WHERE DriverId = @DriverId", sqlConnection);
				cmd.Parameters.AddWithValue("@DriverId", driverId);

				int rowsAffected = cmd.ExecuteNonQuery();

				if(rowsAffected > 0)
				{
					return true;
				}
			}

			return false;
		}

		public bool RegisteredDocumentId(int driverIdentityDocument, int publicTransportGroupId)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				SqlCommand cmd = new("SELECT DriverIdentityDocument, PublicTransportGroupId FROM SuperTransp_DriversDetail WHERE DriverIdentityDocument = @DriverIdentityDocument AND PublicTransportGroupId = @PublicTransportGroupId", sqlConnection);
				cmd.Parameters.AddWithValue("@DriverIdentityDocument", driverIdentityDocument);
				cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					return dr.HasRows;
				}
			}
		}

		public bool RegisteredPhone(string driverPhone, int publicTransportGroupId)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				SqlCommand cmd = new("SELECT DriverPhone, PublicTransportGroupId FROM SuperTransp_DriversDetail WHERE DriverPhone = @DriverPhone AND PublicTransportGroupId = @PublicTransportGroupId", sqlConnection);
				cmd.Parameters.AddWithValue("@DriverPhone", driverPhone);
				cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					return dr.HasRows;
				}
			}
		}

		public bool RegisteredPartnerNumber(int partnerNumber, int publicTransportGroupId)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				SqlCommand cmd = new("SELECT PartnerNumber, PublicTransportGroupId FROM SuperTransp_DriversDetail WHERE PartnerNumber = @PartnerNumber AND PublicTransportGroupId = @PublicTransportGroupId", sqlConnection);
				cmd.Parameters.AddWithValue("@PartnerNumber", partnerNumber);
				cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					return dr.HasRows;
				}
			}
		}
	}
}
