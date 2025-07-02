using Microsoft.Data.SqlClient;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class Reports :IReport
	{
		private readonly IConfiguration _configuration;
		private readonly ISecurity _security;
		public Reports(IConfiguration configuration, ISecurity security)
		{
			this._configuration = configuration;
			this._security = security;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public List<PublicTransportGroupViewModel> GetAllSupervisedVehiclesStatistics()
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDetailStatisticsBySupervisedDrivers", sqlConnection);

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
								Partners = (int)dr["Partners"],
								TotalDrivers = (int)dr["TotalDrivers"],
								TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"],
								PublicTransportGroupGUID = (string)dr["PublicTransportGroupGUID"],
								TotalWithVehicle = (int)dr["TotalWithVehicle"],
								TotalWithoutVehicle = (int)dr["TotalWithoutVehicle"],
								TotalWorkingVehicles = (int)dr["TotalWorkingVehicles"],
								TotalNotWorkingVehicles = (int)dr["TotalNotWorkingVehicles"],
								TotalDriverInPerson = (int)dr["TotalDriverInPerson"],
								TotalDriverNotInPerson = (int)dr["TotalDriverNotInPerson"],
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

		public List<PublicTransportGroupViewModel> GetAllSupervisedVehiclesStatisticsByStateId(int stateId)
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDetailStatisticsBySupervisedDrivers WHERE StateId = @StateId", sqlConnection);
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
								Partners = (int)dr["Partners"],
								TotalDrivers = (int)dr["TotalDrivers"],
								TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"],
								PublicTransportGroupGUID = (string)dr["PublicTransportGroupGUID"],
								TotalWithVehicle = (int)dr["TotalWithVehicle"],
								TotalWithoutVehicle = (int)dr["TotalWithoutVehicle"],
								TotalWorkingVehicles = (int)dr["TotalWorkingVehicles"],
								TotalNotWorkingVehicles = (int)dr["TotalNotWorkingVehicles"],
								TotalDriverInPerson = (int)dr["TotalDriverInPerson"],
								TotalDriverNotInPerson = (int)dr["TotalDriverNotInPerson"],
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

		public List<PublicTransportGroupViewModel> GetAllSupervisedDriversStatisticsInEstate()
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_SupervisedDriversStatisticsInEstate", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							ptg.Add(new PublicTransportGroupViewModel
							{
								StateId = (int)dr["StateId"],
								StateName = (string)dr["StateName"],
								TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"],
								TotalWithVehicle = (int)dr["TotalWithVehicle"],
								TotalWithoutVehicle = (int)dr["TotalWithoutVehicle"],
								TotalWorkingVehicles = (int)dr["TotalWorkingVehicles"],
								TotalNotWorkingVehicles = (int)dr["TotalNotWorkingVehicles"],
								TotalDriverInPerson = (int)dr["TotalDriverInPerson"],
								TotalDriverNotInPerson = (int)dr["TotalDriverNotInPerson"],
							});
						}
					}

					return ptg.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener todas las estadisticas del estado {ex.Message}", ex);
			}
		}

		public List<PublicTransportGroupViewModel> GetAllSupervisedDriversStatisticsInEstateByStateId(int stateId)
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_SupervisedDriversStatisticsInEstate WHERE StateId = @StateId", sqlConnection);
					cmd.Parameters.AddWithValue("@StateId", stateId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							ptg.Add(new PublicTransportGroupViewModel
							{
								StateId = (int)dr["StateId"],
								StateName = (string)dr["StateName"],
								TotalSupervisedDrivers = (int)dr["TotalSupervisedDrivers"],
								TotalWithVehicle = (int)dr["TotalWithVehicle"],
								TotalWithoutVehicle = (int)dr["TotalWithoutVehicle"],
								TotalWorkingVehicles = (int)dr["TotalWorkingVehicles"],
								TotalNotWorkingVehicles = (int)dr["TotalNotWorkingVehicles"],
								TotalDriverInPerson = (int)dr["TotalDriverInPerson"],
								TotalDriverNotInPerson = (int)dr["TotalDriverNotInPerson"],
							});
						}
					}

					return ptg.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener todas las estadisticas del estado por estado {ex.Message}", ex);
			}
		}
	}
}
