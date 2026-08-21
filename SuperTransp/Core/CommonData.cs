using Microsoft.Data.SqlClient;
using SuperTransp.Models;
using System.Data;
using System.Globalization;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class CommonData : ICommonData
	{
		private readonly IConfiguration _configuration;
		public CommonData(IConfiguration configuration)
		{
			this._configuration = configuration;
		}
		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public List<CommonDataViewModel> GetYesNo()
		{
			return new List<CommonDataViewModel>
			{
				new CommonDataViewModel { YesNoId = true, YesNoName = "Sí" },
				new CommonDataViewModel { YesNoId = false, YesNoName = "No" }
			};
		}

		public List<CommonDataViewModel> GetYears()
		{
			var lastYear = DateTime.Now.AddYears(1);
			List<CommonDataViewModel> years = new List<CommonDataViewModel>();

			for (int i = lastYear.Year; i >=  1930; i--)
			{
				years.Add(new CommonDataViewModel { YearId = i, YearName = i.ToString() });
			}

			return years;
		}

		public List<CommonDataViewModel> GetCurrentYears()
		{
			var lastYear = DateTime.Now;
			List<CommonDataViewModel> years = new List<CommonDataViewModel>();

			for (int i = 0; i < 1; i++)
			{
				years.Add(new CommonDataViewModel
				{
					YearId = DateTime.Now.AddYears(i).Year,
					YearName = DateTime.Now.AddYears(i).Year.ToString()
				});
			}

			return years;
		}

		public List<CommonDataViewModel> GetMonthNames()
		{
			List<CommonDataViewModel> months = new List<CommonDataViewModel>();

			for (int i = 1; i <= 12; i++)
			{
				string monthName = new DateTime(2020, i, 1).ToString("MMMM", new CultureInfo("es-ES"));

				months.Add(new CommonDataViewModel { MonthId = i, MonthName = monthName.ToUpper() });
			}

			return months;
		}

		public List<CommonDataViewModel> GetPassengers()
		{
			List<CommonDataViewModel> passengers = new List<CommonDataViewModel>();

			for (int i = 1; i <= 80; i++)
			{
				passengers.Add(new CommonDataViewModel { Passengers = i, PassengerId = i });
			}

			return passengers;
		}

		public CommonDataViewModel GetVehicleDataById(int? vehicleDataId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					CommonDataViewModel vData = new();
					SqlCommand cmd = new("SELECT * FROM VehicleData WHERE VehicleDataId = @VehicleDataId", sqlConnection);
					cmd.Parameters.AddWithValue("@VehicleDataId", vehicleDataId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							vData.YearId = (int)dr["Year"];
							vData.Make = (string)dr["Make"];
							vData.ModelName = (string)dr["Model"];
						}
					}

					return vData;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los datos del vehiculo, año, marca modelo {ex.Message}", ex);
			}
		}
		public List<CommonDataViewModel> GetMakesByYear(int year)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<CommonDataViewModel> makes = new();
					SqlCommand cmd = new("SELECT Make, Year FROM VehicleData GROUP BY Make, Year HAVING (Year = @Year)", sqlConnection);
					cmd.Parameters.AddWithValue("@Year", year);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						int makeId = 1;
						while (dr.Read())
						{
							makes.Add(new CommonDataViewModel
							{
								Make = (string)dr["Make"]
							});
							makeId++;
						}
					}

					return makes.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener las marcas {ex.Message}", ex);
			}
		}

		public List<CommonDataViewModel> GetModelsByYearAndMake(int year, string make)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<CommonDataViewModel> makes = new();
					SqlCommand cmd = new("SELECT Make, Year, Model, VehicleDataId FROM VehicleData GROUP BY Make, Year, Model, VehicleDataId HAVING  (Year = @Year) AND (Make = @Make) ORDER BY Model", sqlConnection);
					cmd.Parameters.AddWithValue("@Year", year);
					cmd.Parameters.AddWithValue("@Make", make);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							makes.Add(new CommonDataViewModel
							{
								ModelName = (string)dr["Model"],
								Make = (string)dr["Make"],
								VehicleDataId = (int)dr["VehicleDataId"],
							});
						}
					}

					return makes.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener las marcas {ex.Message}", ex);
			}
		}

		public List<CommonDataViewModel> GetRims()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<CommonDataViewModel> rims = new();
					SqlCommand cmd = new("SELECT * FROM Rim ORDER BY RimName", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							rims.Add(new CommonDataViewModel
							{
								RimId = (int)dr["RimId"],
								RimName = (string)dr["RimName"],
							});
						}
					}

					return rims.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los tipos de neumaticos {ex.Message}", ex);
			}
		}

		public List<CommonDataViewModel> GetWheels()
		{
			List<CommonDataViewModel> wheels = new List<CommonDataViewModel>();

			for (int i = 2; i <= 8; i += 2)
			{
				wheels.Add(new CommonDataViewModel { WheelId = i, Wheels = i });
			}

			return wheels;
		}

		public List<CommonDataViewModel> GetFuelTypes()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<CommonDataViewModel> fuelTypes = new();
					SqlCommand cmd = new("SELECT * FROM FuelType ORDER BY FuelTypeiD", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							fuelTypes.Add(new CommonDataViewModel
							{
								FuelTypeId = (int)dr["FuelTypeId"],
								FuelTypeName = (string)dr["FuelTypeName"],
							});
						}
					}

					return fuelTypes.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los tipos de combustible {ex.Message}", ex);
			}
		}

		public List<CommonDataViewModel> GetTankCapacity()
		{
			List<CommonDataViewModel> capacity = new List<CommonDataViewModel>();

			for (int i = 5; i <= 250; i++)
			{
				capacity.Add(new CommonDataViewModel { TankCapacityId = i, TankCapacity = i });
			}

			capacity.Add(new CommonDataViewModel { TankCapacityId = 650, TankCapacity = 600 });

			return capacity;
		}

		public List<CommonDataViewModel> GetBatteries()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<CommonDataViewModel> batteries = new();
					SqlCommand cmd = new("SELECT * FROM Battery", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							batteries.Add(new CommonDataViewModel
							{
								BatteryId = (int)dr["BatteryId"],
								BatteryName = (string)dr["BatteryName"],
							});
						}
					}

					return batteries.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener las baterias {ex.Message}", ex);
			}
		}

		public List<CommonDataViewModel> GetNumberOfBatteries()
		{
			List<CommonDataViewModel> batteries = new List<CommonDataViewModel>();

			for (int i = 1; i <= 9; i++)
			{
				batteries.Add(new CommonDataViewModel { BatteriesId = i, Batteries = i });
			}

			return batteries;
		}

		public List<CommonDataViewModel> GetMotorOil()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<CommonDataViewModel> oil = new();
					SqlCommand cmd = new("SELECT * FROM MotorOil", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							oil.Add(new CommonDataViewModel
							{
								MotorOilId = (int)dr["MotorOilId"],
								MotorOilName = (string)dr["MotorOilName"],
							});
						}
					}

					return oil.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los tipos de aceite {ex.Message}", ex);
			}
		}

		public List<CommonDataViewModel> GetOilLitters()
		{
			List<CommonDataViewModel> litters = new List<CommonDataViewModel>();

			for (int i = 1; i <= 90; i++)
			{
				litters.Add(new CommonDataViewModel { OilLittersId = i, OilLitters = i });
			}

			return litters;
		}

		public List<CommonDataViewModel> GetFailureType()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<CommonDataViewModel> failureType = new();
					SqlCommand cmd = new("SELECT * FROM FailureType ORDER BY FailureTypeName", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							failureType.Add(new CommonDataViewModel
							{
								FailureTypeId = (int)dr["FailureTypeId"],
								FailureTypeName = (string)dr["FailureTypeName"],
							});
						}
					}

					return failureType.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los tipos de falla {ex.Message}", ex);
			}
		}

		public int AddOrEditMakeModel(CommonDataViewModel model)
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
						SqlCommand cmd = new("SuperTransp_MakeModelAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@VehicleDataId", model.VehicleDataId);
						cmd.Parameters.AddWithValue("@Year", model.YearId);
						cmd.Parameters.AddWithValue("@Make", model.Make.Trim());
						cmd.Parameters.AddWithValue("@Model", model.ModelName.Trim());

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al añadir o editar la marca del vehículo", ex);
			}
		}

		public CommonDataViewModel GetCommonDataValueByName(string commonDataName)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					CommonDataViewModel cd = new();
					SqlCommand cmd = new("SELECT * FROM CommonData WHERE CommonDataName = @CommonDataName", sqlConnection);
					cmd.Parameters.AddWithValue("@CommonDataName", commonDataName);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							cd.CommonDataId = (int)dr["CommonDataId"];
							cd.CommonDataName = (string)dr["CommonDataName"];
							cd.CommonDataValue = (string)dr["CommonDataValue"];
						}
					}

					return cd;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener la data comun {ex.Message}", ex);
			}
		}
		public List<CommonDataViewModel> GetSex()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<CommonDataViewModel> sex = new();
					SqlCommand cmd = new("SELECT * FROM Sex ORDER BY SexId", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							sex.Add(new CommonDataViewModel
							{
								SexId = (int)dr["SexId"],
								SexName = (string)dr["SexName"],
							});
						}
					}

					return sex.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los tipos de sexo {ex.Message}", ex);
			}
		}

		public List<GastStationViewModel> GetGasStationsAll()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<GastStationViewModel> gas = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_GasStationsDetail ORDER BY StateName", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							gas.Add(new GastStationViewModel
							{
								GasStationId = dr["GasStationId"] == DBNull.Value ? 0 : (int)dr["GasStationId"],
								StateId = dr["StateId"] == DBNull.Value ? 0 : (int)dr["StateId"],
								MunicipalityId = dr["MunicipalityId"] == DBNull.Value ? 0 : (int)dr["MunicipalityId"],
								GasStationName = dr["GasStationName"] as string,
								GasStationAddress = dr["GasStationAddress"] as string,
								StateName = dr["StateName"] as string,
								MunicipalityName = dr["MunicipalityName"] as string,
								TotalByPTG = dr["TotalByPTG"] == DBNull.Value ? 0 : (int)dr["TotalByPTG"],
							});
						}
					}

					return gas.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los municipios {ex.Message}", ex);
			}
		}

		public List<GastStationViewModel> GetGasStationsByStateId(int stateId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<GastStationViewModel> gas = new();
					SqlCommand cmd = new(@"
						SELECT v.*,
							(SELECT COUNT(*) FROM dbo.GasStationPTG gsp WHERE gsp.GasStationId = v.GasStationId) AS TotalByGasStation
						FROM SuperTransp_GasStationsDetail v
						WHERE v.StateId = @StateId
						ORDER BY v.StateName", sqlConnection);
					cmd.Parameters.AddWithValue("@StateId", stateId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							gas.Add(new GastStationViewModel
							{
								GasStationId        = dr["GasStationId"]        == DBNull.Value ? 0    : (int)dr["GasStationId"],
								StateId             = dr["StateId"]             == DBNull.Value ? 0    : (int)dr["StateId"],
								MunicipalityId      = dr["MunicipalityId"]      == DBNull.Value ? 0    : (int)dr["MunicipalityId"],
								GasStationName      = dr["GasStationName"]      as string,
								GasStationAddress   = dr["GasStationAddress"]   as string,
								StateName			= dr["StateName"]			 as string,
								MunicipalityName   = dr["MunicipalityName"]   as string,
								TotalByMunicipality = dr["TotalByMunicipality"] == DBNull.Value ? 0    : (int)dr["TotalByMunicipality"],
								TotalByPTG 			= dr["TotalByPTG"]		 == DBNull.Value ? 0    : (int)dr["TotalByPTG"],
								TotalByGasStation   = dr["TotalByGasStation"]   == DBNull.Value ? 0    : (int)dr["TotalByGasStation"],
							});
						}
					}

					return gas.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los municipios {ex.Message}", ex);
			}
		}

		public int GasStationAddOrEdit(GastStationViewModel model)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SqlCommand cmd = new("SuperTransp_GasStationAddOrEdit", sqlConnection);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.AddWithValue("@GasStationId", model.GasStationId);
					cmd.Parameters.AddWithValue("@MunicipalityId", model.MunicipalityId);
					cmd.Parameters.AddWithValue("@GasStationName", model.GasStationName ?? string.Empty);
					cmd.Parameters.AddWithValue("@GasStationAddress", model.GasStationAddress ?? string.Empty);

					var result = cmd.ExecuteScalar();

					if (model.GasStationId == 0)
					{
						return result != null ? Convert.ToInt32(result) : 0;
					}

					return model.GasStationId;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al guardar la estación de servicio: {ex.Message}", ex);
			}
		}

		public List<GastStationViewModel> GetGasStationsByPTGId(int publicTransportGroupId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<GastStationViewModel> gas = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_GasStationsPTGDetail WHERE PublicTransportGroupId = @PublicTransportGroupId ORDER BY StateName", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							gas.Add(new GastStationViewModel
							{
								GasStationId = dr["GasStationId"] == DBNull.Value ? 0 : (int)dr["GasStationId"],
								StateId = dr["StateId"] == DBNull.Value ? 0 : (int)dr["StateId"],
								MunicipalityId = dr["MunicipalityId"] == DBNull.Value ? 0 : (int)dr["MunicipalityId"],
								GasStationName = dr["GasStationName"] as string,
								GasStationAddress = dr["GasStationAddress"] as string,
								StateName = dr["StateName"] as string,
								MunicipalityName = dr["MunicipalityName"] as string,
								PublicTransportGroupId = dr["PublicTransportGroupId"] == DBNull.Value ? 0 : (int)dr["PublicTransportGroupId"],
							});
						}
					}

					return gas.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener las estaciones por organización {ex.Message}", ex);
			}
		}

		public List<PublicTransportGroupViewModel> GetPTGsByGasStationId(int gasStationId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					const string sql = @"
						SELECT DISTINCT
							p.PublicTransportGroupId,
							p.PublicTransportGroupRif,
							p.PublicTransportGroupName,
							p.PTGCompleteName,
							p.MunicipalityName,
							p.StateName,
							p.ModeName,
							p.DesignationName
						FROM dbo.GasStationPTG gsp
						INNER JOIN dbo.SuperTransp_PublicTransportGroupDetailNoUser p
							ON p.PublicTransportGroupId = gsp.PublicTransportGroupId
						WHERE gsp.GasStationId = @GasStationId
						ORDER BY p.PublicTransportGroupName";

					List<PublicTransportGroupViewModel> result = new();
					using SqlCommand cmd = new(sql, sqlConnection);
					cmd.Parameters.AddWithValue("@GasStationId", gasStationId);

					using SqlDataReader dr = cmd.ExecuteReader();
					while (dr.Read())
					{
						result.Add(new PublicTransportGroupViewModel
						{
							PublicTransportGroupId = dr["PublicTransportGroupId"] == DBNull.Value ? 0 : (int)dr["PublicTransportGroupId"],
							PublicTransportGroupRif = dr["PublicTransportGroupRif"] as string,
							PublicTransportGroupName = dr["PublicTransportGroupName"] as string,
							PTGCompleteName = dr["PTGCompleteName"] as string,
							MunicipalityName = dr["MunicipalityName"] as string,
							StateName = dr["StateName"] as string,
							ModeName = dr["ModeName"] as string,
							DesignationName = dr["DesignationName"] as string,
						});
					}

					return result;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener las organizaciones de la estación de servicio {ex.Message}", ex);
			}
		}

		public List<BankViewModel> GetBanksAll()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<BankViewModel> banks = new();
					SqlCommand cmd = new("SELECT * FROM Bank ORDER BY BankName", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							banks.Add(new BankViewModel
							{
								BankId = dr["BankId"] == DBNull.Value ? 0 : (int)dr["BankId"],
								BankName = dr["BankName"] == DBNull.Value ? string.Empty : (string)dr["BankName"],
								SudebanCode = dr["SudebanCode"] == DBNull.Value ? string.Empty : (string)dr["SudebanCode"],
							});
						}
					}

					return banks.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los bancos {ex.Message}", ex);
			}
		}

		public List<AccounTypeViewModel> GetAccounTypesAll()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<AccounTypeViewModel> accounts = new();
					SqlCommand cmd = new("SELECT * FROM AccounType", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							accounts.Add(new AccounTypeViewModel
							{
								AccounTypeId = dr["AccounTypeId"] == DBNull.Value ? 0 : (int)dr["AccounTypeId"],
								AccounTypeName = dr["AccounTypeName"] == DBNull.Value ? string.Empty : (string)dr["AccounTypeName"],
							});
						}
					}

					return accounts.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener los tipo de cuentas de banco {ex.Message}", ex);
			}
		}

		public (int TotalStates, int TotalMunicipalities, int TotalStations, int TotalPTG) GetGasStationsGlobalStats()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					const string sql = @"
						SELECT
							COUNT(DISTINCT StateId)        AS TotalStates,
							COUNT(DISTINCT MunicipalityId) AS TotalMunicipalities,
							COUNT(DISTINCT GasStationId)   AS TotalStations,
							(SELECT COUNT(PublicTransportGroupId) FROM dbo.GasStationPTG) AS TotalPTG
						FROM SuperTransp_GasStationsDetail";

					using SqlCommand cmd = new(sql, sqlConnection);
					using SqlDataReader dr = cmd.ExecuteReader();

					if (dr.Read())
					{
						int states         = dr["TotalStates"]         == DBNull.Value ? 0 : (int)dr["TotalStates"];
						int municipalities = dr["TotalMunicipalities"] == DBNull.Value ? 0 : (int)dr["TotalMunicipalities"];
						int stations       = dr["TotalStations"]       == DBNull.Value ? 0 : (int)dr["TotalStations"];
						int ptg            = dr["TotalPTG"]            == DBNull.Value ? 0 : (int)dr["TotalPTG"];
						return (states, municipalities, stations, ptg);
					}

					return (0, 0, 0, 0);
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener estadísticas globales de estaciones {ex.Message}", ex);
			}
		}
	}
}
