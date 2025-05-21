using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Data.SqlClient;
using SuperTransp.Models;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Supervision : ISupervision
	{
		private readonly IConfiguration _configuration;
		public Supervision(IConfiguration configuration)
		{
			this._configuration = configuration;
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
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al añadir la supervisión {ex.Message}", ex);
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDriverDetail WHERE StateId = @StateId", sqlConnection);
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

					SupervisionViewModel ptg = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDriverDetail WHERE PublicTransportGroupId = @PublicTransportGroupId AND DriverId = @DriverId AND PartnerNumber = @PartnerNumber AND StateId = @StateId", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);
					cmd.Parameters.AddWithValue("@DriverId", driverId);
					cmd.Parameters.AddWithValue("@PartnerNumber", partnerNumber);
					cmd.Parameters.AddWithValue("@StateId", stateId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							ptg.PublicTransportGroupId = (int)dr["PublicTransportGroupId"];
							ptg.PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"];
							//ptg.DesignationId = (int)dr["DesignationId"];
							//ptg.PublicTransportGroupName = (string)dr["PublicTransportGroupName"];
							ptg.PTGCompleteName = (string)dr["PTGCompleteName"];
							ptg.ModeId = (int)dr["ModeId"];
							//ptg.UnionId = (int)dr["UnionId"];
							//ptg.MunicipalityId = (int)dr["MunicipalityId"];
							//ptg.StateId = (int)dr["StateId"];
							//ptg.RepresentativeIdentityDocument = (int)dr["RepresentativeIdentityDocument"];
							//ptg.RepresentativeName = (string)dr["RepresentativeName"];
							//ptg.RepresentativePhone = (string)dr["RepresentativePhone"];
							//ptg.DesignationName = (string)dr["DesignationName"];
							ptg.StateName = (string)dr["StateName"];
							//ptg.MunicipalityName = (string)dr["MunicipalityName"];
							ptg.ModeName = (string)dr["ModeName"];
							//ptg.UnionName = (string)dr["UnionName"];
							ptg.DriverId = (int)dr["DriverId"];
							ptg.DriverFullName = (string)dr["DriverFullName"];
							ptg.DriverIdentityDocument = (int)dr["DriverIdentityDocument"];
							ptg.PartnerNumber = (int)dr["PartnerNumber"];
							ptg.SupervisionStatusName = (string)dr["SupervisionStatusText"];
							ptg.TotalDrivers = (int)dr["TotalDrivers"];
							ptg.TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"];
							ptg.SupervisionId = (int)dr["SupervisionId"];
							ptg.DriverWithVehicle = (bool)dr["DriverWithVehicle"];
							ptg.WorkingVehicle = (bool)dr["WorkingVehicle"];
							ptg.InPerson = (bool)dr["InPerson"];
							ptg.Plate = (string)dr["Plate"];
							ptg.Year = (int)dr["Year"];
							ptg.Make = (string)dr["Make"];
							ptg.Model = (string)dr["Model"];
							ptg.Passengers = (int)dr["Passengers"];
							ptg.RimName = (string)dr["RimName"];
							ptg.Wheels = (int)dr["Wheels"];
							ptg.MotorOilName = (string)dr["MotorOilName"];
							ptg.Liters = (int)dr["Liters"];
							ptg.FuelTypeName = (string)dr["FuelTypeName"];
							ptg.TankCapacity = (int)dr["TankCapacity"];
							ptg.BatteryName = (string)dr["BatteryName"];
							ptg.NumberOfBatteries = (int)dr["NumberOfBatteries"];
							ptg.FailureTypeName = (string)dr["FailureTypeName"];
							ptg.VehicleImageUrl = (string)dr["VehicleImageUrl"];
							ptg.FingerprintTrouble = (bool)dr["FingerprintTrouble"];
							ptg.Remarks = (string)dr["Remarks"];							
						}
					}

					return ptg;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener todas las líneas {ex.Message}", ex);
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
									SupervisionSummaryPictureUrl = (string)dr["SupervisionSummaryPictureUrl"],
									SupervisionSummaryId = (int)dr["SupervisionSummaryId"]
								};

								images.Add(picture);
							}
						}

						if (images.Any())
						{
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
	}
}
