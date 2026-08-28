using Microsoft.Data.SqlClient;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Procedures : IProcedure
	{
		private readonly IConfiguration _configuration;
		private readonly ISecurity _security;
		public Procedures(IConfiguration configuration, ISecurity security)
		{
			this._configuration = configuration;
			this._security = security;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}
		public int AddOrEdit(ProcedureViewModel model)
		{
			int result = 0;

			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				if (model != null)
				{
					SqlCommand cmd = new("SuperTransp_ProcedureAddOrEdit", sqlConnection)
					{
						CommandType = System.Data.CommandType.StoredProcedure
					};

					cmd.Parameters.AddWithValue("@ProcedureId", model.ProcedureId);
					cmd.Parameters.AddWithValue("@ProcedureCategoryId", (object?)model.ProcedureCategoryId ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@ProcedureConcept", (object?)model.ProcedureConcept ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@ProcedureFrequencyId", (object?)model.ProcedureFrequencyId ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@Tariff", (object?)model.Tariff ?? DBNull.Value);

					result = Convert.ToInt32(cmd.ExecuteScalar());
				}

				return result;
			}
		}

		public int AddOrEditByDriver(ProcedureByDriverViewModel model)
		{
			int result = 0;

			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				if (model != null)
				{
					SqlCommand cmd = new("SuperTransp_ProcedureByDriverAddOrEdit", sqlConnection)
					{
						CommandType = System.Data.CommandType.StoredProcedure
					};

					cmd.Parameters.AddWithValue("@ProcedureByDriverId", model.ProcedureByDriverId);
					cmd.Parameters.AddWithValue("@ProcedureId", model.ProcedureId);
					cmd.Parameters.AddWithValue("@DriverId", model.DriverId);
					cmd.Parameters.AddWithValue("@DriverPTGRif", (object?)model.DriverPTGRif ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@DriverPTGName", (object?)model.DriverPTGName ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@DriverVehiclePlate", (object?)model.DriverVehiclePlate ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@DriverVehicleYear", (object?)model.DriverVehicleYear ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@DriverVehicleMake", (object?)model.DriverVehicleMake ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@DriverVehicleModel", (object?)model.DriverVehicleModel ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@ProcedureBankAccountId", model.ProcedureBankAccountId);
					cmd.Parameters.AddWithValue("@AccounTypeId", model.AccounTypeId);
					cmd.Parameters.AddWithValue("@BankId", model.BankId);
					cmd.Parameters.AddWithValue("@PayerCellPhoneNumber", (object?)model.PayerCellPhoneNumber ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@PayerIdNumber", (object?)model.PayerIdNumber ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@PayerAccountNumber", (object?)model.PayerAccountNumber ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@PayerReferenceNumber", (object?)model.PayerReferenceNumber ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@BCVPaid", model.BCVPaid);
					cmd.Parameters.AddWithValue("@Tariff", model.Tariff);
					cmd.Parameters.AddWithValue("@Rate", model.Rate);
					cmd.Parameters.AddWithValue("@ProcedureStatusId", model.ProcedureStatusId);

					result = Convert.ToInt32(cmd.ExecuteScalar());
				}

				return result;
			}
		}

		public int AddOrEditByPTG(ProcedureByPublicTransportGroupViewModel model)
		{
			int result = 0;

			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				if (model != null)
				{
					SqlCommand cmd = new("SuperTransp_ProcedureByPublicTransportGroupAddOrEdit", sqlConnection)
					{
						CommandType = System.Data.CommandType.StoredProcedure
					};

					cmd.Parameters.AddWithValue("@ProcedureByPublicTransportGroupId", model.ProcedureByPublicTransportGroupId);
					cmd.Parameters.AddWithValue("@ProcedureId", model.ProcedureId);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", model.PublicTransportGroupId);
					cmd.Parameters.AddWithValue("@ProcedureBankAccountId", model.ProcedureBankAccountId);
					cmd.Parameters.AddWithValue("@AccounTypeId", model.AccounTypeId);
					cmd.Parameters.AddWithValue("@BankId", model.BankId);
					cmd.Parameters.AddWithValue("@PayerCellPhoneNumber", (object?)model.PayerCellPhoneNumber ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@PayerIdNumber", (object?)model.PayerIdNumber ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@PayerAccountNumber", (object?)model.PayerAccountNumber ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@PayerReferenceNumber", (object?)model.PayerReferenceNumber ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@BCVPaid", model.BCVPaid);
					cmd.Parameters.AddWithValue("@Tariff", model.Tariff);
					cmd.Parameters.AddWithValue("@Rate", model.Rate);
					cmd.Parameters.AddWithValue("@ProcedureStatusId", model.ProcedureStatusId);

					result = Convert.ToInt32(cmd.ExecuteScalar());
				}

				return result;
			}
		}
		public List<ProcedureViewModel> GetAll()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<ProcedureViewModel> procedure = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_ProcedureDetail", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							procedure.Add(new ProcedureViewModel
							{
								ProcedureId = (int)dr["ProcedureId"],
								ProcedureCategoryId = (int)dr["ProcedureCategoryId"],
								ProcedureCategoryName = (string)dr["ProcedureCategoryName"],
								ProcedureConcept = (string)dr["ProcedureConcept"],
								ProcedureFrequencyId = (int)dr["ProcedureFrequencyId"],
								ProcedureFrequencyName = (string)dr["ProcedureFrequencyName"],
								Tariff = (decimal)dr["Tariff"]
							});
						}
					}

					return procedure;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los procedimientos {ex.Message}", ex);
			}
		}

		public List<ProcedureByDriverViewModel> GetByDriverByProcedureStatusId(int procedureStatusId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<ProcedureByDriverViewModel> procedure = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_ProcedureByDriverDetail WHERE ProcedureStatusId = @ProcedureStatusId", sqlConnection);
					cmd.Parameters.AddWithValue("@ProcedureStatusId", procedureStatusId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							procedure.Add(new ProcedureByDriverViewModel
							{
								ProcedureByDriverId = (int)dr["ProcedureByDriverId"],
								ProcedureId = (int)dr["ProcedureId"],
								ProcedureConcept = dr["ProcedureConcept"] == DBNull.Value ? string.Empty : (string)dr["ProcedureConcept"],
								ProcedureCategoryName = dr["ProcedureCategoryName"] == DBNull.Value ? string.Empty : (string)dr["ProcedureCategoryName"],
								DriverId = (int)dr["DriverId"],
								DriverIdentityDocument = (int)dr["DriverIdentityDocument"],
								DriverFullName = dr["DriverFullName"] == DBNull.Value ? string.Empty : (string)dr["DriverFullName"],
								DriverPhone = dr["DriverPhone"] == DBNull.Value ? string.Empty : (string)dr["DriverPhone"],
								ReceiverBankAccountTypeName = dr["ReceiverBankAccountTypeName"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankAccountTypeName"],
								ReceiverBankName = dr["ReceiverBankName"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankName"],
								ReceiverBankAccountNumber = dr["ReceiverBankAccountNumber"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankAccountNumber"],
								ReceiverBankCellPhone = dr["ReceiverBankCellPhone"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankCellPhone"],
								ReceiverBankIdNumber = dr["ReceiverBankIdNumber"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankIdNumber"],
								PayerBankName = dr["PayerBankName"] == DBNull.Value ? string.Empty : (string)dr["PayerBankName"],
								PayerCellPhoneNumber = dr["PayerCellPhoneNumber"] == DBNull.Value ? string.Empty : (string)dr["PayerCellPhoneNumber"],
								PayerIdNumber = dr["PayerIdNumber"] == DBNull.Value ? string.Empty : (string)dr["PayerIdNumber"],
								PayerAccountNumber = dr["PayerAccountNumber"] == DBNull.Value ? string.Empty : (string)dr["PayerAccountNumber"],
								PayerReferenceNumber = dr["PayerReferenceNumber"] == DBNull.Value ? string.Empty : (string)dr["PayerReferenceNumber"],
								Tariff = (decimal)dr["Tariff"],
								BCVPaid = (decimal)dr["BCVPaid"],
								Rate = (decimal)dr["Rate"],
								ProcedureStatusName = dr["ProcedureStatusName"] == DBNull.Value ? string.Empty : (string)dr["ProcedureStatusName"],
								ProcedureStatusId = (int)dr["ProcedureStatusId"],
								AccounTypeId = (int)dr["AccounTypeId"],
							});
						}
					}

					return procedure;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los procedimientos {ex.Message}", ex);
			}
		}

		public List<ProcedureByPublicTransportGroupViewModel> GetByPTGByProcedureStatusId(int procedureStatusId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<ProcedureByPublicTransportGroupViewModel> procedure = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_ProcedureByPTGDetail WHERE ProcedureStatusId = @ProcedureStatusId", sqlConnection);
					cmd.Parameters.AddWithValue("@ProcedureStatusId", procedureStatusId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							procedure.Add(new ProcedureByPublicTransportGroupViewModel
							{
								ProcedureByPublicTransportGroupId = (int)dr["ProcedureByPublicTransportGroupId"],
								ProcedureId = (int)dr["ProcedureId"],
								ProcedureConcept = dr["ProcedureConcept"] == DBNull.Value ? string.Empty : (string)dr["ProcedureConcept"],
								ProcedureCategoryName = dr["ProcedureCategoryName"] == DBNull.Value ? string.Empty : (string)dr["ProcedureCategoryName"],
								PublicTransportGroupId = (int)dr["PublicTransportGroupId"],
								PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"],
								PublicTransportGroupNameFullName = dr["PTGCompleteName"] == DBNull.Value ? string.Empty : (string)dr["PTGCompleteName"],
								RepresentativeIdentityDocument = dr["RepresentativeIdentityDocument"] == DBNull.Value ? string.Empty : dr["RepresentativeIdentityDocument"].ToString(),
								RepresentativeName = dr["RepresentativeName"] == DBNull.Value ? string.Empty : (string)dr["RepresentativeName"],
								RepresentativePhone = dr["RepresentativePhone"] == DBNull.Value ? string.Empty : (string)dr["RepresentativePhone"],
								Partners = dr["Partners"] == DBNull.Value ? 0 : (int)dr["Partners"],
								TotalDrivers = dr["TotalDrivers"] == DBNull.Value ? 0 : (int)dr["TotalDrivers"],
								TotalSupervisedDrivers = dr["TotalSupervisedDrivers"] == DBNull.Value ? 0 : (int)dr["TotalSupervisedDrivers"],
								ReceiverBankAccountTypeName = dr["ReceiverBankAccountTypeName"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankAccountTypeName"],
								ReceiverBankName = dr["ReceiverBankName"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankName"],
								ReceiverBankAccountNumber = dr["ReceiverBankAccountNumber"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankAccountNumber"],
								ReceiverBankCellPhone = dr["ReceiverBankCellPhone"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankCellPhone"],
								ReceiverBankIdNumber = dr["ReceiverBankIdNumber"] == DBNull.Value ? string.Empty : (string)dr["ReceiverBankIdNumber"],
								PayerBankName = dr["PayerBankName"] == DBNull.Value ? string.Empty : (string)dr["PayerBankName"],
								PayerCellPhoneNumber = dr["PayerCellPhoneNumber"] == DBNull.Value ? string.Empty : (string)dr["PayerCellPhoneNumber"],
								PayerIdNumber = dr["PayerIdNumber"] == DBNull.Value ? string.Empty : (string)dr["PayerIdNumber"],
								PayerAccountNumber = dr["PayerAccountNumber"] == DBNull.Value ? string.Empty : (string)dr["PayerAccountNumber"],
								PayerReferenceNumber = dr["PayerReferenceNumber"] == DBNull.Value ? string.Empty : (string)dr["PayerReferenceNumber"],
								Tariff = (decimal)dr["Tariff"],
								BCVPaid = (decimal)dr["BCVPaid"],
								Rate = (decimal)dr["Rate"],
								ProcedureStatusName = dr["ProcedureStatusName"] == DBNull.Value ? string.Empty : (string)dr["ProcedureStatusName"],
								ProcedureStatusId = (int)dr["ProcedureStatusId"],
								AccounTypeId = (int)dr["AccounTypeId"],
							});
						}
					}

					return procedure;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los procedimientos {ex.Message}", ex);
			}
		}

		public List<ProcedureBankAccountViewModel> GetByBankAccounTypeId(int accounTypeId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<ProcedureBankAccountViewModel> bank = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_ProcedureBankAccountDetail WHERE AccounTypeId = @AccounTypeId", sqlConnection);
					cmd.Parameters.AddWithValue("@AccounTypeId", accounTypeId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							bank.Add(new ProcedureBankAccountViewModel
							{
								BankId = (int)dr["BankId"],
								BankName = dr["BankName"] == DBNull.Value ? string.Empty : (string)dr["BankName"],
								SudebanCode = dr["SudebanCode"] == DBNull.Value ? string.Empty : (string)dr["SudebanCode"],
								ProcedureBankAccountId = (int)dr["ProcedureBankAccountId"],
								AccounTypeName = dr["AccounTypeName"] == DBNull.Value ? string.Empty : (string)dr["AccounTypeName"],
								AccounTypeId = (int)dr["AccounTypeId"],
								AccountNumber = dr["AccountNumber"] == DBNull.Value ? string.Empty : (string)dr["AccountNumber"],
								CellPhoneNumber = dr["CellPhoneNumber"] == DBNull.Value ? string.Empty : (string)dr["CellPhoneNumber"],
								IdNumber = dr["IdNumber"] == DBNull.Value ? string.Empty : (string)dr["IdNumber"],
							});
						}
					}

					return bank;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los bancos {ex.Message}", ex);
			}
		}

		public List<ProcedureStatusViewModel> GetStatusAll()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<ProcedureStatusViewModel> status = new();
					SqlCommand cmd = new("SELECT * FROM ProcedureStatus", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							status.Add(new ProcedureStatusViewModel
							{
								ProcedureStatusId = (int)dr["ProcedureStatusId"],
								ProcedureStatusName = dr["ProcedureStatusName"] == DBNull.Value ? string.Empty : (string)dr["ProcedureStatusName"],
							});
						}
					}

					return status;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los status {ex.Message}", ex);
			}
		}

		public List<ProcedureFrequencyViewModel> ProcedureFrequencyAll()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<ProcedureFrequencyViewModel> procedureFeq = new();
					SqlCommand cmd = new("SELECT * FROM ProcedureFrequency Order By ProcedureFrequencyId ", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							procedureFeq.Add(new ProcedureFrequencyViewModel
							{
								ProcedureFrequencyId = (int)dr["ProcedureFrequencyId"],
								ProcedureFrequencyName = (string)dr["ProcedureFrequencyName"],
							});
						}
					}

					return procedureFeq;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener las frecuencias {ex.Message}", ex);
			}
		}

		public List<ProcedureCategoryViewModel> ProcedureCategoryAll()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<ProcedureCategoryViewModel> procedureCat = new();
					SqlCommand cmd = new("SELECT * FROM ProcedureCategory Order By ProcedureCategoryId ", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							procedureCat.Add(new ProcedureCategoryViewModel
							{
								ProcedureCategoryId = (int)dr["ProcedureCategoryId"],
								ProcedureCategoryName = (string)dr["ProcedureCategoryName"],
							});
						}
					}

					return procedureCat;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener las categorias {ex.Message}", ex);
			}
		}
	}
}
