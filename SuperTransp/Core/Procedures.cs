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
					cmd.Parameters.AddWithValue("@DriverPTGRif", (object?)model.DriverPTGRif ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@DriverPTGName", (object?)model.DriverPTGName ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@DriverVehiclePlate", (object?)model.DriverVehiclePlate ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@DriverVehicleYear", model.DriverVehicleYear);
					cmd.Parameters.AddWithValue("@DriverVehicleMake", (object?)model.DriverVehicleMake ?? DBNull.Value);
					cmd.Parameters.AddWithValue("@DriverVehicleModel", (object?)model.DriverVehicleModel ?? DBNull.Value);

					result = Convert.ToInt32(cmd.ExecuteScalar());
				}

				return result;
			}
		}

		// Writes the logbook entry for a newly added request, using the row as it was finally saved.
		public void LogProcedureRequest(int procedureByDriverId)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				SqlCommand cmd = new("SELECT TOP 1 * FROM SuperTransp_ProcedureByDriverDetail WHERE ProcedureByDriverId = @ProcedureByDriverId", sqlConnection);
				cmd.Parameters.AddWithValue("@ProcedureByDriverId", procedureByDriverId);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					if (!dr.Read()) return;

					string Text(string column) => dr[column] == DBNull.Value ? string.Empty : Convert.ToString(dr[column]) ?? string.Empty;

					_security.AddLogbook(0, false,
						$" Solicitud de trámite ->" +
						$" solicitud n°: {procedureByDriverId} -" +
						$" socio: {Text("DriverFullName").ToUpper().Trim()} -" +
						$" cedula: {Text("DriverIdentityDocument")} -" +
						$" organización: {Text("DriverPTGName")} ({Text("DriverPTGRif")}) -" +
						$" vehículo: {Text("DriverVehiclePlate")} {Text("DriverVehicleMake")} {Text("DriverVehicleModel")} {Text("DriverVehicleYear")} -" +
						$" trámite: {Text("ProcedureCategoryName")} - {Text("ProcedureConcept")} -" +
						$" tarifa: {Convert.ToDecimal(dr["Tariff"]):0.00} -" +
						$" monto pagado: {Convert.ToDecimal(dr["BCVPaid"]):0.00} -" +
						$" tasa BCV: {Convert.ToDecimal(dr["Rate"]):0.00} -" +
						$" banco: {Text("PayerBankName")} -" +
						$" referencia: {Text("PayerReferenceNumber")} -" +
						$" estatus: {Text("ProcedureStatusName")}");
				}
			}
		}

		public RegistrationRequestViewModel? GetRegistrationRequestData(int procedureByDriverId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					// Vehicle and organization come from the snapshot saved with the request; partner number,
					// mode and GUID (for the verification QR) come from the driver's link to that organization.
					SqlCommand cmd = new(@"SELECT TOP 1 pd.DriverFullName, pd.DriverIdentityDocument, pd.DriverPTGName, pd.DriverPTGRif,
							d.PartnerNumber, d.ModeName, d.PublicTransportGroupGUID,
							pd.DriverVehiclePlate, pd.DriverVehicleMake, pd.DriverVehicleModel, pd.DriverVehicleYear, pd.ProcedureStatusId
						FROM SuperTransp_ProcedureByDriverDetail pd
						INNER JOIN SuperTransp_DriversDetailNoUser d ON d.DriverId = pd.DriverId AND d.PublicTransportGroupRif = pd.DriverPTGRif
						WHERE pd.ProcedureByDriverId = @ProcedureByDriverId", sqlConnection);
					cmd.Parameters.AddWithValue("@ProcedureByDriverId", procedureByDriverId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						if (!dr.Read()) return null;

						return new RegistrationRequestViewModel
						{
							DriverFullName = dr["DriverFullName"] == DBNull.Value ? string.Empty : (string)dr["DriverFullName"],
							DriverIdentityDocument = (int)dr["DriverIdentityDocument"],
							PTGName = dr["DriverPTGName"] == DBNull.Value ? string.Empty : (string)dr["DriverPTGName"],
							PTGRif = dr["DriverPTGRif"] == DBNull.Value ? string.Empty : (string)dr["DriverPTGRif"],
							PartnerNumber = (int)dr["PartnerNumber"],
							ModeName = dr["ModeName"] == DBNull.Value ? string.Empty : (string)dr["ModeName"],
							PublicTransportGroupGUID = dr["PublicTransportGroupGUID"] == DBNull.Value ? string.Empty : (string)dr["PublicTransportGroupGUID"],
							Plate = dr["DriverVehiclePlate"] == DBNull.Value ? string.Empty : (string)dr["DriverVehiclePlate"],
							Make = dr["DriverVehicleMake"] == DBNull.Value ? string.Empty : (string)dr["DriverVehicleMake"],
							Model = dr["DriverVehicleModel"] == DBNull.Value ? string.Empty : (string)dr["DriverVehicleModel"],
							Year = dr["DriverVehicleYear"] == DBNull.Value ? 0 : (int)dr["DriverVehicleYear"],
							ProcedureStatusId = (int)dr["ProcedureStatusId"]
						};
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los datos de la solicitud {ex.Message}", ex);
			}
		}

		public List<DriverVehicleOptionViewModel> GetDriverVehiclesByIdentityDocument(int driverIdentityDocument)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<DriverVehicleOptionViewModel> options = new();
					SqlCommand cmd = new("SELECT DISTINCT DriverId, DriverIdentityDocument, DriverFullName, PublicTransportGroupId, PublicTransportGroupRif, ModeId, ModeName, PTGCompleteName, Plate, Year, Make, Model FROM SuperTransp_PublicTransportGroupDriverDetail WHERE DriverIdentityDocument = @DriverIdentityDocument ORDER BY PTGCompleteName, Plate", sqlConnection);
					cmd.Parameters.AddWithValue("@DriverIdentityDocument", driverIdentityDocument);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							options.Add(new DriverVehicleOptionViewModel
							{
								DriverId = (int)dr["DriverId"],
								DriverIdentityDocument = (int)dr["DriverIdentityDocument"],
								DriverFullName = dr["DriverFullName"] == DBNull.Value ? string.Empty : (string)dr["DriverFullName"],
								PublicTransportGroupId = (int)dr["PublicTransportGroupId"],
								PublicTransportGroupRif = dr["PublicTransportGroupRif"] == DBNull.Value ? string.Empty : (string)dr["PublicTransportGroupRif"],
								ModeId = (int)dr["ModeId"],
								ModeName = dr["ModeName"] == DBNull.Value ? string.Empty : (string)dr["ModeName"],
								PTGCompleteName = dr["PTGCompleteName"] == DBNull.Value ? string.Empty : (string)dr["PTGCompleteName"],
								Plate = dr["Plate"] == DBNull.Value ? string.Empty : (string)dr["Plate"],
								Year = dr["Year"] == DBNull.Value ? 0 : (int)dr["Year"],
								Make = dr["Make"] == DBNull.Value ? string.Empty : (string)dr["Make"],
								Model = dr["Model"] == DBNull.Value ? string.Empty : (string)dr["Model"]
							});
						}
					}

					return options;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los vehículos del socio {ex.Message}", ex);
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_ProcedureDetail ORDER BY ProcedureCategoryName", sqlConnection);

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

		// The tariff comes from ProcedureTariffByMode (per procedure and transport mode), not from Procedure.Tariff.
		// Procedures without a tariff for the mode are not offered. procedureId > 0 restricts the list to that procedure.
		public List<ProcedureViewModel> GetAllByMode(int modeId, int procedureId = 0)
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
					SqlCommand cmd = new(@"SELECT pd.ProcedureId, pd.ProcedureCategoryId, pd.ProcedureCategoryName, pd.ProcedureConcept,
							pd.ProcedureFrequencyId, pd.ProcedureFrequencyName, t.Tariff
						FROM SuperTransp_ProcedureDetail pd
						INNER JOIN ProcedureTariffByMode t ON t.ProcedureId = pd.ProcedureId AND t.ModeId = @ModeId
						WHERE (@ProcedureId = 0 OR pd.ProcedureId = @ProcedureId)
						ORDER BY pd.ProcedureCategoryName", sqlConnection);
					cmd.Parameters.AddWithValue("@ModeId", modeId);
					cmd.Parameters.AddWithValue("@ProcedureId", procedureId);

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
				throw new Exception($"Error al obtener los procedimientos por modalidad {ex.Message}", ex);
			}
		}

		public List<ProcedureViewModel> GetById(int procedureId)
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_ProcedureDetail WHERE ProcedureId = @ProcedureId", sqlConnection);
					cmd.Parameters.AddWithValue("@ProcedureId", procedureId);

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
								DriverId = (int)dr["DriverId"],
								DriverIdentityDocument = (int)dr["DriverIdentityDocument"],
								DriverVehicleYear = dr["DriverVehicleYear"] == DBNull.Value ? 0 : (int)dr["DriverVehicleYear"],
								ProcedureConcept = dr["ProcedureConcept"] == DBNull.Value ? string.Empty : (string)dr["ProcedureConcept"],
								ProcedureCategoryName = dr["ProcedureCategoryName"] == DBNull.Value ? string.Empty : (string)dr["ProcedureCategoryName"],
								DriverFullName = dr["DriverFullName"] == DBNull.Value ? string.Empty : (string)dr["DriverFullName"],
								DriverPhone = dr["DriverPhone"] == DBNull.Value ? string.Empty : (string)dr["DriverPhone"],
								DriverPTGRif = dr["DriverPTGRif"] == DBNull.Value ? string.Empty : (string)dr["DriverPTGRif"],
								DriverPTGName = dr["DriverPTGName"] == DBNull.Value ? string.Empty : (string)dr["DriverPTGName"],
								DriverVehiclePlate = dr["DriverVehiclePlate"] == DBNull.Value ? string.Empty : (string)dr["DriverVehiclePlate"],
								DriverVehicleMake = dr["DriverVehicleMake"] == DBNull.Value ? string.Empty : (string)dr["DriverVehicleMake"],
								DriverVehicleModel = dr["DriverVehicleModel"] == DBNull.Value ? string.Empty : (string)dr["DriverVehicleModel"],
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
								ProcedureStatusName = dr["ProcedureStatusName"] == DBNull.Value ? string.Empty : (string)dr["ProcedureStatusName"],
								Tariff = dr["Tariff"] == DBNull.Value ? 0 : (decimal)dr["Tariff"],
								BCVPaid = dr["BCVPaid"] == DBNull.Value ? 0 : (decimal)dr["BCVPaid"],
								Rate = dr["Rate"] == DBNull.Value ? 0 : (decimal)dr["Rate"],
								ProcedureStatusId = (int)dr["ProcedureStatusId"],
								AccounTypeId = (int)dr["AccounTypeId"],
								ProcedureDate = dr["ProcedureDate"] == DBNull.Value ? null : (DateTime)dr["ProcedureDate"]
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

		public int GetMonthlyFeePendingsByPTG(int publicTransportGroupId){
			try
			{
				int totalMonthlyFeesPaid = 0;
				int monthlyFeePendings = 0;
				int elapsedMonths = DateTime.Now.Month - 1;
				return monthlyFeePendings;
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SqlCommand cmd = new("SELECT * FROM SuperTransp_MonthlyFeesPaid WHERE PublicTransportGroupId = @PublicTransportGroupId AND ProcedureId = 13", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						if (dr.Read())
						{
							totalMonthlyFeesPaid = (int)dr["TotalMonthlyFeesPaid"];
						}

						monthlyFeePendings = elapsedMonths - totalMonthlyFeesPaid;
					}

					return monthlyFeePendings;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener las cuotas mensuales pagadas {ex.Message}", ex);
			}
		}
	}
}
