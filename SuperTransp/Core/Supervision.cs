using Microsoft.Data.SqlClient;
using SuperTransp.Models;
using System.Data;
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
						cmd.Parameters.AddWithValue("@InPerson", model.InPerson);
						cmd.Parameters.AddWithValue("@Operational", model.Operational);
						cmd.Parameters.AddWithValue("@Plate", model.Plate);
						cmd.Parameters.AddWithValue("@RimId", model.RimId);
						cmd.Parameters.AddWithValue("@Wheels", model.Wheels);
						cmd.Parameters.AddWithValue("@MotorOilId", model.MotorOilId);
						cmd.Parameters.AddWithValue("@Liters", model.Liters);
						cmd.Parameters.AddWithValue("@FuelTypeId", model.FuelTypeId);
						cmd.Parameters.AddWithValue("@TankCapacity", model.TankCapacity);
						cmd.Parameters.AddWithValue("@FailureTypeId", model.FailureTypeId);
						cmd.Parameters.AddWithValue("@FingerprintTrouble", model.FingerprintTrouble);
						cmd.Parameters.AddWithValue("@Remarks", model.Remarks);
						cmd.Parameters.AddWithValue("@SecurityUserId", model.SecurityUserId);

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
	}
}
