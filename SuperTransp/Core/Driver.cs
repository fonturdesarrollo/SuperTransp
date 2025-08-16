﻿using Microsoft.Data.SqlClient;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Driver : IDriver
	{
		private readonly IConfiguration _configuration;
		private readonly ISecurity _security;

		public Driver(IConfiguration configuration, ISecurity security)
		{
			this._configuration = configuration;
			this._security = security;
		}
		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public int AddOrEdit(DriverViewModel model)
		{
			int result = 0;
			int? beforeDriverIdentityDocument = 0;
			string? beforeDriverFullName = string.Empty;
			int? beforePartnerNumber = 0;
			string? beforeDriverPhone = string.Empty;
			string? beforeSexName = string.Empty;
			string? beforePTGName = string.Empty;
			DateTime beforeBirthdate = DateTime.Now;
			DriverViewModel driverValues = new DriverViewModel();
			bool isEditing = false;

			try
			{
				if(model.DriverId > 0)
				{
					driverValues = GetById(model.DriverId);

					if (driverValues != null) 
					{
						isEditing = true;
						beforeDriverIdentityDocument = driverValues.DriverIdentityDocument;
						beforeDriverFullName = driverValues.DriverFullName;
						beforePartnerNumber = driverValues.PartnerNumber;
						beforeDriverPhone = driverValues.DriverPhone;
						beforeSexName= driverValues.SexName;
						beforeBirthdate = driverValues.Birthdate;
						beforePTGName = driverValues.PTGCompleteName;
					}
				}

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
						cmd.Parameters.AddWithValue("@SexId", model.SexId);
						cmd.Parameters.AddWithValue("@Birthdate", model.Birthdate);

						result = Convert.ToInt32(cmd.ExecuteScalar());

						if(!isEditing)
						{
							driverValues = GetById(result);

							_security.AddLogbook(model.DriverId, false, 
								$" Socio ->" +
								$" cedula: {model.DriverIdentityDocument} -" +
								$" nombre: {model.DriverFullName.ToUpper().Trim()} -" +
								$" socio n°: {model.PartnerNumber} -" +
								$" telefono: {model.DriverPhone} -" +
								$" organización: {driverValues.PTGCompleteName} -" +
								$" sexo: {driverValues.SexName} -" +
								$" fecha de nacimiento: {model.Birthdate.ToString("dd/MM/yyyy")}");
						}
						else
						{
							driverValues = GetById(model.DriverId);

							_security.AddLogbook(model.DriverId, false,
								$" Socio ->" +
								$" ANTES: [" +
								$" cedula: {beforeDriverIdentityDocument} -" +
								$" nombre: {beforeDriverFullName} -" +
								$" socio n°: {beforePartnerNumber} -" +
								$" telefono: {beforeDriverPhone} -" +
								$" organización: {beforePTGName} -" +
								$" sexo: {beforeSexName} -" +
								$" fecha de nacimiento {beforeBirthdate.ToString("dd/MM/yyyy")}]" +
								$" DESPUES: [" +
								$" cedula: {driverValues.DriverIdentityDocument} -" +
								$" nombre: {driverValues.DriverFullName} -" +
								$" socio n°: {driverValues.PartnerNumber} -" +
								$" telefono: {driverValues.DriverPhone} -" +
								$" organización: {driverValues.PTGCompleteName} -" +
								$" sexo: {driverValues.SexName} -" +
								$" fecha de nacimiento: {driverValues.Birthdate.ToString("dd/MM/yyyy")}]");
						}						
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al añadir o editar el transportista {ex.Message}", ex);
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
								DriverPublicTransportGroupId = (int)dr["DriverPublicTransportGroupId"],
								PublicTransportGroupGUID = (string)dr["PublicTransportGroupGUID"],
								SexId = (int)dr["SexId"],
								SexName = (string)dr["SexName"],
								Birthdate = (DateTime)dr["Birthdate"],
							});
						}
					}

					return drivers;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los transportistas {ex.Message}", ex);
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
							driver.SexId = (int)dr["SexId"];
							driver.SexName = (string)dr["SexName"];
							driver.Birthdate = (DateTime)dr["Birthdate"];
						}
					}

					return driver;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los transportistas {ex.Message}", ex);
			}
		}

		public DriverViewModel GetByIdentityDocument(int driverIdentityDocument)
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_DriversDetail WHERE DriverIdentityDocument = @DriverIdentityDocument", sqlConnection);
					cmd.Parameters.AddWithValue("@DriverIdentityDocument", driverIdentityDocument);

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
							driver.SexId = (int)dr["SexId"];
							driver.SexName = (string)dr["SexName"];
							driver.Birthdate = (DateTime)dr["Birthdate"];
							break;
						}
					}

					return driver;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener el transportista por cedula {ex.Message}", ex);
			}
		}

		public DriverViewModel GetById(int driverId)
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_DriversDetail WHERE DriverId = @DriverId", sqlConnection);
					cmd.Parameters.AddWithValue("@DriverId", driverId);

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
							driver.SexId = (int)dr["SexId"];
							driver.SexName = (string)dr["SexName"];
							driver.Birthdate = (DateTime)dr["Birthdate"];
							driver.StateName = (string)dr["StateName"];
							driver.PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"];
							driver.StateId = (int)dr["StateId"];
						}
					}

					return driver;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener el transportista por Id {ex.Message}", ex);
			}
		}

		public bool Delete(int driverId)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				var driver = GetById(driverId);

				SqlCommand cmd = new("DELETE FROM Driver WHERE DriverId = @DriverId", sqlConnection);
				cmd.Parameters.AddWithValue("@DriverId", driverId);

				int rowsAffected = cmd.ExecuteNonQuery();

				if (rowsAffected > 0)
				{
					if (driver != null)
					{
						_security.AddLogbook(driverId, true, $"transportista cedula {driver.DriverIdentityDocument} nombre {driver.DriverFullName.ToUpper().Trim()} socio n° {driver.PartnerNumber} telefono {driver.DriverPhone} linea {driver.PTGCompleteName}");
					}
				}

				//Cascade deletions

				cmd.Parameters.Clear();

				cmd = new("DELETE FROM Supervision WHERE DriverId = @DriverId", sqlConnection);
				cmd.Parameters.AddWithValue("@DriverId", driverId);

				rowsAffected = cmd.ExecuteNonQuery();

				cmd.Parameters.Clear();

				cmd = new("DELETE FROM SupervisionPicture WHERE PublicTransportGroupId = @PublicTransportGroupId AND PartnerNumber = @PartnerNumber", sqlConnection);
				cmd.Parameters.AddWithValue("@PublicTransportGroupId", driver.PublicTransportGroupId);
				cmd.Parameters.AddWithValue("@PartnerNumber", driver.PartnerNumber);

				rowsAffected = cmd.ExecuteNonQuery();

				return true;
			}
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

		public int TotalDriversByPublicTransportGroupId(int publicTransportGroupId)
		{
			int totalDrivers = 0;
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				string sql = "SELECT PublicTransportGroupId, COUNT(DriverId) AS TotalDriversByPTG FROM dbo.DriverPublicTransportGroup GROUP BY PublicTransportGroupId HAVING  (PublicTransportGroupId = @PublicTransportGroupId)";
				SqlCommand cmd = new(sql, sqlConnection);
				cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while(dr.Read())
					{
						totalDrivers = (int)dr["TotalDriversByPTG"];
					}
				}
			}

			return totalDrivers;
		}
	}
}
