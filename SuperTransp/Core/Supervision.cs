using Microsoft.Data.SqlClient;
using SuperTransp.Extensions;
using SuperTransp.Models;
using System.Data;
using System.Reflection;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Supervision : ISupervision
	{
		private readonly IConfiguration _configuration;
		private readonly ISecurity _security;
		private readonly IDriver _driver;

		public Supervision(IConfiguration configuration, ISecurity security, IDriver driver)
		{
			this._configuration = configuration;
			this._security = security;
			this._driver = driver;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public int AddOrEdit(SupervisionViewModel model)
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
						SqlCommand cmd = new("SuperTransp_SupervisionAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@SupervisionId", model.SupervisionId);
						cmd.Parameters.AddWithValue("@DriverId", model.DriverId);
						cmd.Parameters.AddWithValue("@DriverWithVehicle", model.DriverWithVehicle);
						cmd.Parameters.AddWithValue("@WorkingVehicle", model.WorkingVehicle);
						cmd.Parameters.AddWithValue("@InPerson", model.InPerson);
						cmd.Parameters.AddWithValue("@Plate", model.Plate);
						cmd.Parameters.AddWithValue("@VehicleDataId", model.VehicleDataId);
						cmd.Parameters.AddWithValue("@Passengers", model.Passengers);
						cmd.Parameters.AddWithValue("@RimId", model.RimId);
						cmd.Parameters.AddWithValue("@Wheels", model.Wheels);
						cmd.Parameters.AddWithValue("@MotorOilId", model.MotorOilId);
						cmd.Parameters.AddWithValue("@Liters", model.Liters);
						cmd.Parameters.AddWithValue("@FuelTypeId", model.FuelTypeId);
						cmd.Parameters.AddWithValue("@TankCapacity", model.TankCapacity);
						cmd.Parameters.AddWithValue("@BatteryId", model.BatteryId);
						cmd.Parameters.AddWithValue("@NumberOfBatteries", model.NumberOfBatteries);
						cmd.Parameters.AddWithValue("@FailureTypeId", model.FailureTypeId);
						cmd.Parameters.AddWithValue("@FingerprintTrouble", model.FingerprintTrouble);
						cmd.Parameters.AddWithValue("@VehicleImageUrl", model.VehicleImageUrl);
						cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrEmpty(model.Remarks) ? string.Empty : model.Remarks.ToUpper().Trim());
						cmd.Parameters.AddWithValue("@SecurityUserId", model.SecurityUserId);
						cmd.Parameters.AddWithValue("@SupervisionStatus", model.SupervisionStatus);

						result = Convert.ToInt32(cmd.ExecuteScalar());

						// Add pictures process
						if(model.Pictures != null && model.Pictures.Any())
						{
							DeletePicturesByPTGIdAndPartnerNumber(model.PublicTransportGroupId, model.PartnerNumber);

							cmd = new("SuperTransp_SupervisionPicturesAddOrEdit", sqlConnection)
							{
								CommandType = System.Data.CommandType.StoredProcedure
							};

							foreach (var picture in model.Pictures)
							{
								cmd.Parameters.Clear();

								cmd.Parameters.AddWithValue("@SupervisionPictureId", 0);
								cmd.Parameters.AddWithValue("@PublicTransportGroupId", model.PublicTransportGroupId);
								cmd.Parameters.AddWithValue("@PartnerNumber", model.PartnerNumber);
								cmd.Parameters.AddWithValue("@VehicleImageUrl", picture.VehicleImageUrl);

								cmd.ExecuteScalar();
							}
						}

						var driver = _driver.GetById(model.DriverId);

						if (driver != null)
						{
							var ptg = GetDriverPublicTransportGroupByPtgId(model.PublicTransportGroupId);

							var ptgFullName = string.Empty;
							var ptgRif = string.Empty;

							if (ptg != null)
							{
								ptgFullName = ptg.FirstOrDefault().PTGCompleteName;
								ptgRif = ptg.FirstOrDefault().PublicTransportGroupRif;
							}

							_security.AddLogbook(model.SupervisionId, false, $"supervisión transportista codigo {model.DriverId} nombre {driver.DriverFullName} cedula {driver.DriverIdentityDocument} organización {ptgFullName} RIF {ptgRif} socio con vehículo {model.DriverWithVehicle.ToSpanishYesNo().ToLower()} vehículo en funcionamiento {model.WorkingVehicle.ToSpanishYesNo().ToLower()} socio presente {model.InPerson.ToSpanishYesNo().ToLower()} " +
								$"placa {model.Plate} id del vehiculo {model.VehicleDataId} año vehículo {model.Year} marca vehículo {model.Make} modelo vehículo {model.ModeName} pasajeros {model.Passengers} litros de combustible {model.TankCapacity} litros de aceite {model.Liters} problemas con la huella {model.FingerprintTrouble.ToSpanishYesNo().ToLower()} observaciones {model.Remarks} cantidad de imagenes vehiculo {model.Pictures?.Count()}");
						}
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al añadir o editar la supervisión {ex.Message}", ex);
			}
		}

		public int AddSimple(SupervisionViewModel model)
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
						SqlCommand cmd = new("SuperTransp_SupervisionAddSimple", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@SupervisionId", model.SupervisionId);
						cmd.Parameters.AddWithValue("@DriverId", model.DriverId);
						cmd.Parameters.AddWithValue("@DriverWithVehicle", model.DriverWithVehicle);
						cmd.Parameters.AddWithValue("@SecurityUserId", model.SecurityUserId);

						result = Convert.ToInt32(cmd.ExecuteScalar());

						var driver = _driver.GetById(model.DriverId);

						if(driver != null)
						{
							var ptg = GetDriverPublicTransportGroupByPtgId(model.PublicTransportGroupId);

							var ptgFullName = string.Empty;
							var ptgRif = string.Empty;

							if (ptg != null)
							{
								ptgFullName = ptg.FirstOrDefault().PTGCompleteName;
								ptgRif = ptg.FirstOrDefault().PublicTransportGroupRif;
							}

							_security.AddLogbook(model.SupervisionId, false, $"supervisión de socio sin vehículo, código {model.DriverId} nombre {driver.DriverFullName} cedula {model.DriverIdentityDocument} organización {ptgFullName} RIF {ptgRif} ");
						}						
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al añadir la supervisión {ex.Message}", ex);
			}
		}

		public bool DeletePicturesByPTGIdAndPartnerNumber(int publicTransportGroupId, int partnerNumber)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				SqlCommand cmd = new("DELETE FROM SupervisionPicture WHERE PublicTransportGroupId = @PublicTransportGroupId AND PartnerNumber = @PartnerNumber", sqlConnection);
				cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);
				cmd.Parameters.AddWithValue("@PartnerNumber", partnerNumber);

				int rowsAffected = cmd.ExecuteNonQuery();

				return true;				
			}
		}

		public List<PublicTransportGroupViewModel> GetDriverPublicTransportGroupByStateId(int stateId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<PublicTransportGroupViewModel> ptg = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDriverDetail WHERE StateId = @StateId AND Partners = TotalDrivers", sqlConnection);
					cmd.Parameters.AddWithValue("@StateId", stateId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							ptg.Add(new PublicTransportGroupViewModel
							{
								PublicTransportGroupId = (int)dr["PublicTransportGroupId"],
								PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"],
								DesignationId = (int)dr["DesignationId"],
								PublicTransportGroupName = (string)dr["PublicTransportGroupName"],
								PTGCompleteName = (string)dr["PTGCompleteName"],
								ModeId = (int)dr["ModeId"],
								UnionId = (int)dr["UnionId"],
								MunicipalityId = (int)dr["MunicipalityId"],
								StateId = (int)dr["StateId"],
								RepresentativeIdentityDocument = (int)dr["RepresentativeIdentityDocument"],
								RepresentativeName = (string)dr["RepresentativeName"],
								RepresentativePhone = (string)dr["RepresentativePhone"],
								DesignationName = (string)dr["DesignationName"],
								StateName = (string)dr["StateName"],
								MunicipalityName = (string)dr["MunicipalityName"],
								ModeName = (string)dr["ModeName"],
								UnionName = (string)dr["UnionName"],
								DriverId = (int)dr["DriverId"],
								DriverFullName = (string)dr["DriverFullName"],
								DriverIdentityDocument = (int)dr["DriverIdentityDocument"],
								PartnerNumber = (int)dr["PartnerNumber"],
								SupervisionStatusName = (string)dr["SupervisionStatusText"],
								TotalDrivers = (int)dr["TotalDrivers"],
								TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"],
								SupervisionId = (int)dr["SupervisionId"],
								DriverWithVehicle = (bool)dr["DriverWithVehicle"],
								WorkingVehicle = (bool)dr["WorkingVehicle"],
								InPerson = (bool)dr["InPerson"],
								Plate = (string)dr["Plate"],
								Year = (int)dr["Year"],
								Make = (string)dr["Make"],
								Model = (string)dr["Model"],
								Passengers = (int)dr["Passengers"],
								RimName = (string)dr["RimName"],
								Wheels = (int)dr["Wheels"],
								MotorOilName = (string)dr["MotorOilName"],
								Liters = (int)dr["Liters"],
								FuelTypeName = (string)dr["FuelTypeName"],
								TankCapacity = (int)dr["TankCapacity"],
								BatteryName = (string)dr["BatteryName"],
								NumberOfBatteries = (int)dr["NumberOfBatteries"],
								FailureTypeName = (string)dr["FailureTypeName"],
								VehicleImageUrl = (string)dr["VehicleImageUrl"],
								FingerprintTrouble = (bool)dr["FingerprintTrouble"],
								Remarks = (string)dr["Remarks"],
								UserFullName = (string)dr["UserFullName"],
								Pictures = GetPicturesByPTGIdAndPartnerNumber((int)dr["PublicTransportGroupId"], (int)dr["PartnerNumber"]),
							});
						}
					}

					return ptg.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener todas las líneas {ex.Message}", ex);
			}
		}

		public List<PublicTransportGroupViewModel> GetDriverPublicTransportGroupByPtgId(int publicTransportGroupId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<PublicTransportGroupViewModel> ptg = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDriverDetail WHERE PublicTransportGroupId = @PublicTransportGroupId", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							ptg.Add(new PublicTransportGroupViewModel
							{
								PublicTransportGroupId = (int)dr["PublicTransportGroupId"],
								PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"],
								DesignationId = (int)dr["DesignationId"],
								PublicTransportGroupName = (string)dr["PublicTransportGroupName"],
								PTGCompleteName = (string)dr["PTGCompleteName"],
								ModeId = (int)dr["ModeId"],
								UnionId = (int)dr["UnionId"],
								MunicipalityId = (int)dr["MunicipalityId"],
								StateId = (int)dr["StateId"],
								RepresentativeIdentityDocument = (int)dr["RepresentativeIdentityDocument"],
								RepresentativeName = (string)dr["RepresentativeName"],
								RepresentativePhone = (string)dr["RepresentativePhone"],
								DesignationName = (string)dr["DesignationName"],
								StateName = (string)dr["StateName"],
								MunicipalityName = (string)dr["MunicipalityName"],
								ModeName = (string)dr["ModeName"],
								UnionName = (string)dr["UnionName"],
								DriverId = (int)dr["DriverId"],
								DriverFullName = (string)dr["DriverFullName"],
								DriverIdentityDocument = (int)dr["DriverIdentityDocument"],
								PartnerNumber = (int)dr["PartnerNumber"],
								SupervisionStatusName = (string)dr["SupervisionStatusText"],
								TotalDrivers = (int)dr["TotalDrivers"],
								TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"],
								SupervisionId = (int)dr["SupervisionId"],
								DriverWithVehicle = (bool)dr["DriverWithVehicle"],
								WorkingVehicle = (bool)dr["WorkingVehicle"],
								InPerson = (bool)dr["InPerson"],
								Plate = (string)dr["Plate"],
								Year = (int)dr["Year"],
								Make = (string)dr["Make"],
								Model = (string)dr["Model"],
								Passengers = (int)dr["Passengers"],
								RimName = (string)dr["RimName"],
								Wheels = (int)dr["Wheels"],
								MotorOilName = (string)dr["MotorOilName"],
								Liters = (int)dr["Liters"],
								FuelTypeName = (string)dr["FuelTypeName"],
								TankCapacity = (int)dr["TankCapacity"],
								BatteryName = (string)dr["BatteryName"],
								NumberOfBatteries = (int)dr["NumberOfBatteries"],
								FailureTypeName = (string)dr["FailureTypeName"],
								VehicleImageUrl = (string)dr["VehicleImageUrl"],
								FingerprintTrouble = (bool)dr["FingerprintTrouble"],
								Remarks = (string)dr["Remarks"],
								UserFullName = (string)dr["UserFullName"],
								SecurityUserId = (int)dr["SecurityUserId"],
								Pictures = GetPicturesByPTGIdAndPartnerNumber((int)dr["PublicTransportGroupId"], (int)dr["PartnerNumber"])
							});
						}
					}

					return ptg.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener transportistas {ex.Message}", ex);
			}
		}

		public SupervisionViewModel GetByPublicTransportGroupIdAndDriverIdAndPartnerNumberStateId(int publicTransportGroupId, int driverId, int partnerNumber, int stateId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SupervisionViewModel supervision = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDriverDetail WHERE PublicTransportGroupId = @PublicTransportGroupId AND DriverId = @DriverId AND PartnerNumber = @PartnerNumber AND StateId = @StateId", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);
					cmd.Parameters.AddWithValue("@DriverId", driverId);
					cmd.Parameters.AddWithValue("@PartnerNumber", partnerNumber);
					cmd.Parameters.AddWithValue("@StateId", stateId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							supervision.PublicTransportGroupId = (int)dr["PublicTransportGroupId"];
							supervision.PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"];
							supervision.PTGCompleteName = (string)dr["PTGCompleteName"];
							supervision.ModeId = (int)dr["ModeId"];
							supervision.StateName = (string)dr["StateName"];
							supervision.ModeName = (string)dr["ModeName"];
							supervision.DriverId = (int)dr["DriverId"];
							supervision.DriverFullName = (string)dr["DriverFullName"];
							supervision.DriverIdentityDocument = (int)dr["DriverIdentityDocument"];
							supervision.PartnerNumber = (int)dr["PartnerNumber"];
							supervision.SupervisionStatusName = (string)dr["SupervisionStatusText"];
							supervision.TotalDrivers = (int)dr["TotalDrivers"];
							supervision.TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"];
							supervision.SupervisionId = (int)dr["SupervisionId"];
							supervision.DriverWithVehicle = (bool)dr["DriverWithVehicle"];
							supervision.WorkingVehicle = (bool)dr["WorkingVehicle"];
							supervision.InPerson = (bool)dr["InPerson"];
							supervision.Plate = (string)dr["Plate"];
							supervision.Year = (int)dr["Year"];
							supervision.Make = (string)dr["Make"];
							supervision.Model = (string)dr["Model"];
							supervision.Passengers = (int)dr["Passengers"];
							supervision.RimName = (string)dr["RimName"];
							supervision.RimId = (int)dr["RimId"];
							supervision.Wheels = (int)dr["Wheels"];
							supervision.MotorOilName = (string)dr["MotorOilName"];
							supervision.MotorOilId = (int)dr["MotorOilId"];
							supervision.Liters = (int)dr["Liters"];
							supervision.FuelTypeName = (string)dr["FuelTypeName"];
							supervision.FuelTypeId = (int)dr["FuelTypeId"];
							supervision.TankCapacity = (int)dr["TankCapacity"];
							supervision.BatteryName = (string)dr["BatteryName"];
							supervision.BatteryId = (int)dr["BatteryId"];
							supervision.NumberOfBatteries = (int)dr["NumberOfBatteries"];
							supervision.FailureTypeName = (string)dr["FailureTypeName"];
							supervision.FailureTypeId = (int)dr["FailureTypeId"];
							supervision.VehicleImageUrl = (string)dr["VehicleImageUrl"];
							supervision.FingerprintTrouble = (bool)dr["FingerprintTrouble"];
							supervision.Remarks = (string)dr["Remarks"];							
							supervision.VehicleDataId = (int)dr["VehicleDataId"];
							supervision.SupervisionStatus = (bool)dr["SupervisionStatus"];
							supervision.SupervisionDateAdded = (DateTime)dr["SupervisionDateAdded"];
							supervision.Pictures = GetPicturesByPTGIdAndPartnerNumber(publicTransportGroupId, partnerNumber);
						}
					}

					return supervision;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los datos del transportista y el vehiculo {ex.Message}", ex);
			}
		}

		public SupervisionViewModel GetByPublicTransportGroupGUIDAndPartnerNumber(string publicTransportGroupGUID, int partnerNumber)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SupervisionViewModel supervision = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDriverDetail WHERE PublicTransportGroupGUID = @PublicTransportGroupGUID AND PartnerNumber = @PartnerNumber", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupGUID", publicTransportGroupGUID);
					cmd.Parameters.AddWithValue("@PartnerNumber", partnerNumber);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							supervision.PublicTransportGroupId = (int)dr["PublicTransportGroupId"];
							supervision.PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"];
							supervision.PTGCompleteName = (string)dr["PTGCompleteName"];
							supervision.ModeId = (int)dr["ModeId"];
							supervision.StateName = (string)dr["StateName"];
							supervision.ModeName = (string)dr["ModeName"];
							supervision.DriverId = (int)dr["DriverId"];
							supervision.DriverFullName = (string)dr["DriverFullName"];
							supervision.DriverIdentityDocument = (int)dr["DriverIdentityDocument"];
							supervision.PartnerNumber = (int)dr["PartnerNumber"];
							supervision.SupervisionStatusName = (string)dr["SupervisionStatusText"];
							supervision.TotalDrivers = (int)dr["TotalDrivers"];
							supervision.TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"];
							supervision.SupervisionId = (int)dr["SupervisionId"];
							supervision.DriverWithVehicle = (bool)dr["DriverWithVehicle"];
							supervision.WorkingVehicle = (bool)dr["WorkingVehicle"];
							supervision.InPerson = (bool)dr["InPerson"];
							supervision.Plate = (string)dr["Plate"];
							supervision.Year = (int)dr["Year"];
							supervision.Make = (string)dr["Make"];
							supervision.Model = (string)dr["Model"];
							supervision.Passengers = (int)dr["Passengers"];
							supervision.RimName = (string)dr["RimName"];
							supervision.RimId = (int)dr["RimId"];
							supervision.Wheels = (int)dr["Wheels"];
							supervision.MotorOilName = (string)dr["MotorOilName"];
							supervision.MotorOilId = (int)dr["MotorOilId"];
							supervision.Liters = (int)dr["Liters"];
							supervision.FuelTypeName = (string)dr["FuelTypeName"];
							supervision.FuelTypeId = (int)dr["FuelTypeId"];
							supervision.TankCapacity = (int)dr["TankCapacity"];
							supervision.BatteryName = (string)dr["BatteryName"];
							supervision.BatteryId = (int)dr["BatteryId"];
							supervision.NumberOfBatteries = (int)dr["NumberOfBatteries"];
							supervision.FailureTypeName = (string)dr["FailureTypeName"];
							supervision.FailureTypeId = (int)dr["FailureTypeId"];
							supervision.VehicleImageUrl = (string)dr["VehicleImageUrl"];
							supervision.FingerprintTrouble = (bool)dr["FingerprintTrouble"];
							supervision.Remarks = (string)dr["Remarks"];
							supervision.VehicleDataId = (int)dr["VehicleDataId"];
							supervision.SupervisionStatus = (bool)dr["SupervisionStatus"];
							supervision.SupervisionDateAdded = (DateTime)dr["SupervisionDateAdded"];
							supervision.Pictures = GetPicturesByPTGIdAndPartnerNumber((int)dr["PublicTransportGroupId"], (int)dr["PartnerNumber"]);
						}
					}

					return supervision;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los datos del transportista y el vehiculo {ex.Message}", ex);
			}
		}

		public List<PublicTransportGroupViewModel> GetAllDriverPublicTransportGroup()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<PublicTransportGroupViewModel> ptg = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDriverDetail ORDER BY StateName, PTGCompleteName, PartnerNumber", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							ptg.Add(new PublicTransportGroupViewModel
							{
								PublicTransportGroupId = (int)dr["PublicTransportGroupId"],
								PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"],
								DesignationId = (int)dr["DesignationId"],
								PublicTransportGroupName = (string)dr["PublicTransportGroupName"],
								PTGCompleteName = (string)dr["PTGCompleteName"],
								ModeId = (int)dr["ModeId"],
								UnionId = (int)dr["UnionId"],
								MunicipalityId = (int)dr["MunicipalityId"],
								StateId = (int)dr["StateId"],
								RepresentativeIdentityDocument = (int)dr["RepresentativeIdentityDocument"],
								RepresentativeName = (string)dr["RepresentativeName"],
								RepresentativePhone = (string)dr["RepresentativePhone"],
								DesignationName = (string)dr["DesignationName"],
								StateName = (string)dr["StateName"],
								MunicipalityName = (string)dr["MunicipalityName"],
								ModeName = (string)dr["ModeName"],
								UnionName = (string)dr["UnionName"],
								DriverId = (int)dr["DriverId"],
								DriverFullName = (string)dr["DriverFullName"],
								DriverIdentityDocument = (int)dr["DriverIdentityDocument"],
								PartnerNumber = (int)dr["PartnerNumber"],
								SupervisionStatusName = (string)dr["SupervisionStatusText"],
								TotalDrivers = (int)dr["TotalDrivers"],
								TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"],
								SupervisionId = (int)dr["SupervisionId"],
								DriverWithVehicle = (bool)dr["DriverWithVehicle"],
								WorkingVehicle = (bool)dr["WorkingVehicle"],
								InPerson = (bool)dr["InPerson"],
								Plate = (string)dr["Plate"],
								Year = (int)dr["Year"],
								Make = (string)dr["Make"],
								Model = (string)dr["Model"],
								Passengers = (int)dr["Passengers"],
								RimName = (string)dr["RimName"],
								Wheels = (int)dr["Wheels"],
								MotorOilName = (string)dr["MotorOilName"],
								Liters = (int)dr["Liters"],
								FuelTypeName = (string)dr["FuelTypeName"],
								TankCapacity = (int)dr["TankCapacity"],
								BatteryName = (string)dr["BatteryName"],
								NumberOfBatteries = (int)dr["NumberOfBatteries"],
								FailureTypeName = (string)dr["FailureTypeName"],
								VehicleImageUrl = (string)dr["VehicleImageUrl"],
								FingerprintTrouble = (bool)dr["FingerprintTrouble"],
								Remarks = (string)dr["Remarks"],
								UserFullName = (string)dr["UserFullName"],
								SecurityUserId = (int)dr["SecurityUserId"],
							});
						}
					}

					return ptg.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener todas las líneas {ex.Message}", ex);
			}
		}

		public List<SupervisionPictures> GetPicturesByPTGIdAndPartnerNumber(int publicTransportGroupId, int partnerNumber)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<SupervisionPictures> pictures = new();
					SqlCommand cmd = new("SELECT * FROM SupervisionPicture WHERE PublicTransportGroupId = @PublicTransportGroupId AND PartnerNumber = @PartnerNumber", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);
					cmd.Parameters.AddWithValue("@PartnerNumber", partnerNumber);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							pictures.Add(new SupervisionPictures
							{
								SupervisionPictureId = (int)dr["SupervisionPictureId"],
								PublicTransportGroupId = (int)dr["PublicTransportGroupId"],
								PartnerNumber = (int)dr["PartnerNumber"],
								VehicleImageUrl = (string)dr["VehicleImageUrl"],	
								SupervisionPictureDateAdded = (DateTime)dr["SupervisionPictureDateAdded"],
							});
						}
					}

					return pictures.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener las imagenes de los vehiculos {ex.Message}", ex);
			}
		}

		public List<PublicTransportGroupViewModel> RegisteredPlate(string plate)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				List<PublicTransportGroupViewModel> existingPlate = new();

				SqlCommand cmd = new("SELECT  dbo.Supervision.Plate, dbo.Driver.DriverIdentityDocument, dbo.Driver.DriverFullName, dbo.PublicTransportGroup.PublicTransportGroupRif, dbo.Designation.DesignationName + ' ' + dbo.PublicTransportGroup.PublicTransportGroupName AS PTGCompleteName, dbo.State.StateName, dbo.Driver.DriverId " +
					"FROM  dbo.Supervision INNER JOIN  dbo.Driver ON dbo.Supervision.DriverId = dbo.Driver.DriverId " +
					"INNER JOIN dbo.DriverPublicTransportGroup ON dbo.Driver.DriverId = dbo.DriverPublicTransportGroup.DriverId " +
					"INNER JOIN dbo.PublicTransportGroup ON dbo.DriverPublicTransportGroup.PublicTransportGroupId = dbo.PublicTransportGroup.PublicTransportGroupId " +
					"INNER JOIN dbo.Designation ON dbo.PublicTransportGroup.DesignationId = dbo.Designation.DesignationId " +
					"INNER JOIN  dbo.Municipality ON dbo.PublicTransportGroup.MunicipalityId = dbo.Municipality.MunicipalityId " +
					"INNER JOIN dbo.State ON dbo.Municipality.StateId = dbo.State.StateId " +
					"WHERE (dbo.Supervision.Plate = @Plate)", sqlConnection);

				cmd.Parameters.AddWithValue("@Plate", plate);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						existingPlate.Add(new PublicTransportGroupViewModel
						{
							Plate = (string)dr["Plate"],
							DriverId = (int)dr["DriverId"],
							DriverIdentityDocument = (int)dr["DriverIdentityDocument"],
							DriverFullName = (string)dr["DriverFullName"],
							PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"],
							PTGCompleteName = (string)dr["PTGCompleteName"],
							StateName = (string)dr["StateName"],
						});
					}
				}

				return existingPlate.ToList();
			}
		}

		public int AddOrEditSummary(SupervisionSummaryViewModel model)
		{
			int summaryId = 0;
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
						SqlCommand cmd = new("SuperTransp_SupervisionSummaryAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@SupervisionSummaryId", model.SupervisionSummaryId);
						cmd.Parameters.AddWithValue("@SupervisionDate", model.SupervisionDate);
						cmd.Parameters.AddWithValue("@PublicTransportGroupId", model.PublicTransportGroupId);
						cmd.Parameters.AddWithValue("@SupervisionAddress", model.SupervisionAddress);
						cmd.Parameters.AddWithValue("@SupervisionSummaryRemarks", model.SupervisionSummaryRemarks);
						cmd.Parameters.AddWithValue("@SupervisionStatusId", model.SupervisionStatusId);
						cmd.Parameters.AddWithValue("@SecurityUserId", model.SecurityUserId);

						summaryId = Convert.ToInt32(cmd.ExecuteScalar());

						// Add pictures process
						if(model.SupervisionSummaryId == 0)
						{
							cmd = new("SuperTransp_SupervisionSummaryPicturesDelete", sqlConnection)
							{
								CommandType = System.Data.CommandType.StoredProcedure
							};

							cmd.Parameters.AddWithValue("@SupervisionSummaryId", summaryId);

							cmd.ExecuteNonQuery();

							cmd = new("SuperTransp_SupervisionSummaryPicturesAddOrEdit", sqlConnection)
							{
								CommandType = System.Data.CommandType.StoredProcedure
							};

							foreach (var picture in model.Pictures)
							{
								cmd.Parameters.Clear();

								cmd.Parameters.AddWithValue("@SupervisionSummaryPictureId", picture.SupervisionSummaryPictureId);
								cmd.Parameters.AddWithValue("@SupervisionSummaryId", summaryId);
								cmd.Parameters.AddWithValue("@SupervisionSummaryPictureUrl", picture.SupervisionSummaryPictureUrl);

								cmd.ExecuteScalar();
							}
						}

						var ptg = GetDriverPublicTransportGroupByPtgId(model.PublicTransportGroupId);

						var ptgFullName = string.Empty;
						var ptgRif = string.Empty;
						var files = 0;

						if (ptg != null)
						{
							ptgFullName = ptg.FirstOrDefault().PTGCompleteName;
							ptgRif = ptg.FirstOrDefault().PublicTransportGroupRif;
						}

						if(model.Pictures != null && model.Pictures.Any())
						{
							files = model.Pictures.Count;
						}						

						_security.AddLogbook(model.SupervisionSummaryId, false, $"resumen de supervisión organización {ptgFullName} RIF {ptgRif} " +
							$" fecha {model.SupervisionDate.ToShortDateString()} dirección {model.SupervisionAddress} observaciones {model.SupervisionSummaryRemarks} cantidad de fotos o archivos {files}");
					}				
				}

				return summaryId;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al añadir o editar el resumen de la supervisión {ex.Message}", ex);
			}
		}

		public List<SupervisionSummaryViewModel> GetSupervisionSummaryByStateId(int stateId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<SupervisionSummaryViewModel> summary = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_SupervisionSummaryDetail WHERE StateId = @StateId ORDER BY SupervisionSummaryId", sqlConnection);
					cmd.Parameters.AddWithValue("@StateId", stateId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							summary.Add(new SupervisionSummaryViewModel
							{
								SupervisionSummaryId = (int)dr["SupervisionSummaryId"],
								PublicTransportGroupId = (int)dr["PublicTransportGroupId"],
								PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"],
								SupervisionDate = (DateTime)dr["SupervisionDate"],
								PTGCompleteName = (string)dr["PTGCompleteName"],
								StateId = (int)dr["StateId"],
								StateName = (string)dr["StateName"],
								ModeName = (string)dr["ModeName"],				
								SupervisionAddress = (string)dr["SupervisionAddress"],
								SupervisionSummaryRemarks = (string)dr["SupervisionSummaryRemarks"],
								UserFullName = (string)dr["UserFullName"],
								SecurityUserId = (int)dr["SecurityUserId"],
							});
						}
					}

					return summary.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los resumenes de supervision por estado {ex.Message}", ex);
			}
		}

		public SupervisionSummaryViewModel GetSupervisionSummaryById(int supervisionSummaryId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SupervisionSummaryViewModel summary = new();
					List<SupervisionSummaryPictures> images = new List<SupervisionSummaryPictures>();

					SqlCommand cmd = new("SELECT * FROM SuperTransp_SupervisionSummaryDetail WHERE SupervisionSummaryId = @SupervisionSummaryId", sqlConnection);
					cmd.Parameters.AddWithValue("@SupervisionSummaryId", supervisionSummaryId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{ 
							summary.SupervisionSummaryId = (int)dr["SupervisionSummaryId"];
							summary.PublicTransportGroupId = (int)dr["PublicTransportGroupId"];
							summary.PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"];
							summary.SupervisionDate = (DateTime)dr["SupervisionDate"];
							summary.PTGCompleteName = (string)dr["PTGCompleteName"];
							summary.StateId = (int)dr["StateId"];
							summary.StateName = (string)dr["StateName"];
							summary.ModeName = (string)dr["ModeName"];
							summary.SupervisionAddress = (string)dr["SupervisionAddress"];
							summary.SupervisionSummaryRemarks = (string)dr["SupervisionSummaryRemarks"];
							summary.UserFullName = (string)dr["UserFullName"];
						}
					}

					if(summary != null)
					{
						cmd = new("SELECT * FROM SupervisionSummaryPicture WHERE SupervisionSummaryId = @SupervisionSummaryId", sqlConnection);
						cmd.Parameters.AddWithValue("@SupervisionSummaryId", supervisionSummaryId);

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							while (dr.Read())
							{
								SupervisionSummaryPictures picture = new SupervisionSummaryPictures
								{
									SupervisionSummaryPictureId = (int)dr["SupervisionSummaryPictureId"],
									SupervisionSummaryPictureUrl = dr["SupervisionSummaryPictureUrl"] as string ?? "",
									SupervisionSummaryId = (int)dr["SupervisionSummaryId"]
								};

								images.Add(picture);
							}		
						}

						if (images.Any())
						{
							summary.Pictures = images;
						}
						else
						{
							SupervisionSummaryPictures picture = new SupervisionSummaryPictures
							{
								SupervisionSummaryPictureId = 0,
								SupervisionSummaryPictureUrl = string.Empty,
								SupervisionSummaryId = 0
							};

							images.Add(picture);

							summary.Pictures = images;
						}
					}

					return summary;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los transportistas {ex.Message}", ex);
			}
		}

		public bool IsSupervisionSummaryDoneByPtgId(int publicTransportGroupId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SupervisionSummaryViewModel summary = new();
					List<SupervisionSummaryPictures> images = new List<SupervisionSummaryPictures>();

					SqlCommand cmd = new("SELECT * FROM SuperTransp_SupervisionSummaryDetail WHERE PublicTransportGroupId = @PublicTransportGroupId", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						return dr.HasRows;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los transportistas {ex.Message}", ex);
			}
		}

		public List<SupervisionSummaryViewModel> GetAllSupervisionSummary()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<SupervisionSummaryViewModel> summary = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_SupervisionSummaryDetail ORDER BY StateName, PTGCompleteName", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							summary.Add(new SupervisionSummaryViewModel
							{
								SupervisionSummaryId = (int)dr["SupervisionSummaryId"],
								PublicTransportGroupId = (int)dr["PublicTransportGroupId"],
								PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"],
								SupervisionDate = (DateTime)dr["SupervisionDate"],
								PTGCompleteName = (string)dr["PTGCompleteName"],
								StateId = (int)dr["StateId"],
								StateName = (string)dr["StateName"],
								ModeName = (string)dr["ModeName"],
								SupervisionAddress = (string)dr["SupervisionAddress"],
								SupervisionSummaryRemarks = (string)dr["SupervisionSummaryRemarks"],
								UserFullName = (string)dr["UserFullName"],
							});
						}
					}

					return summary.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener todos los resumenes de supervision {ex.Message}", ex);
			}
		}

		public bool IsUserSupervisingPublicTransportGroup(int securityUserId, int publicTransportGroupId)
		{
			var pTGs = GetDriverPublicTransportGroupByPtgId(publicTransportGroupId);
			var isSupervised = pTGs.Where(x => x.SupervisionStatusName == "SUPERVISADO");

			if(isSupervised.Any())
			{
				var supervisor = isSupervised.Where(z => z.SecurityUserId == securityUserId);
				
				if(supervisor.Any())
				{
					return true;
				}
			}
			else
			{
				return true;
			}

			return false;
		}
	}
}
