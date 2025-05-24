using Microsoft.Data.SqlClient;
using SuperTransp.Controllers;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class PublicTransportGroup : IPublicTransportGroup
	{
		private readonly IConfiguration _configuration;
		private readonly ISecurity _security;

		public PublicTransportGroup(IConfiguration configuration, ISecurity security)
		{
			this._configuration = configuration;
			this._security = security;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}
		public int AddOrEdit(PublicTransportGroupViewModel model)
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
						SqlCommand cmd = new("SuperTransp_PublicTransportGroupAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@PublicTransportGroupId", model.PublicTransportGroupId);
						cmd.Parameters.AddWithValue("@PublicTransportGroupRif", model.PublicTransportGroupRif);
						cmd.Parameters.AddWithValue("@DesignationId", model.DesignationId);
						cmd.Parameters.AddWithValue("@PublicTransportGroupName", model.PublicTransportGroupName.ToUpper().Trim());
						cmd.Parameters.AddWithValue("@ModeId", model.ModeId);
						cmd.Parameters.AddWithValue("@UnionId", model.UnionId);
						cmd.Parameters.AddWithValue("@MunicipalityId", model.MunicipalityId);
						cmd.Parameters.AddWithValue("@RepresentativeIdentityDocument", model.RepresentativeIdentityDocument);
						cmd.Parameters.AddWithValue("@RepresentativeName", model.RepresentativeName.ToUpper().Trim());
						cmd.Parameters.AddWithValue("@RepresentativePhone", model.RepresentativePhone);
						cmd.Parameters.AddWithValue("@PublicTransportGroupIdModifiedDate", DateTime.Now);
						cmd.Parameters.AddWithValue("@Partners", model.Partners);

						result = Convert.ToInt32(cmd.ExecuteScalar());

						_security.AddLogbook(model.PublicTransportGroupId, $"linea de transporte RIF {model.PublicTransportGroupRif} nombre {model.PublicTransportGroupName} cantidad socios: {model.Partners} representante {model.RepresentativeName} cedula representante {model.RepresentativeIdentityDocument}");											
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al añadir o editar la línea de transporte {ex.Message}", ex);
			}
		}

		public PublicTransportGroupViewModel GetPublicTransportGroupById(int publicTransportGroupId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					PublicTransportGroupViewModel publicTransportGroup = new();
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDetail WHERE PublicTransportGroupId = @PublicTransportGroupId", sqlConnection);
					cmd.Parameters.AddWithValue("@PublicTransportGroupId", publicTransportGroupId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							publicTransportGroup.PublicTransportGroupId = (int)dr["PublicTransportGroupId"];
							publicTransportGroup.PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"];
							publicTransportGroup.DesignationId = (int)dr["DesignationId"];
							publicTransportGroup.PublicTransportGroupName = (string)dr["PublicTransportGroupName"];
							publicTransportGroup.PTGCompleteName = (string)dr["PTGCompleteName"];
							publicTransportGroup.ModeId = (int)dr["ModeId"];
							publicTransportGroup.UnionId = (int)dr["UnionId"];
							publicTransportGroup.StateId = (int)dr["StateId"];
							publicTransportGroup.MunicipalityId = (int)dr["MunicipalityId"];
							publicTransportGroup.RepresentativeIdentityDocument = (int)dr["RepresentativeIdentityDocument"];
							publicTransportGroup.RepresentativeName = (string)dr["RepresentativeName"];
							publicTransportGroup.RepresentativePhone = (string)dr["RepresentativePhone"];
							publicTransportGroup.StateName = (string)dr["StateName"];
							publicTransportGroup.Partners = (int)dr["Partners"];

							if (dr["PublicTransportGroupIdModifiedDate"] != DBNull.Value)
							{
								publicTransportGroup.PublicTransportGroupIdModifiedDate = (DateTime)dr["PublicTransportGroupIdModifiedDate"];
							}
						}
					}

					return publicTransportGroup;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener la línea por ID {ex.Message}", ex);
			}
		}

		public List<PublicTransportGroupViewModel> GetAll()
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDetail", sqlConnection);

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

		public List<PublicTransportGroupViewModel> GetByStateId(int stateId)
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
					SqlCommand cmd = new("SELECT * FROM SuperTransp_PublicTransportGroupDetail WHERE StateId = @StateId", sqlConnection);
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

		public string? RegisteredRif(string publicTransportGroupRif)
		{
			PublicTransportGroupViewModel publicTransportGroup = new();
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				SqlCommand cmd = new("SELECT PublicTransportGroupRif FROM PublicTransportGroup WHERE PublicTransportGroupRif = @PublicTransportGroupRif", sqlConnection);
				cmd.Parameters.AddWithValue("@PublicTransportGroupRif", publicTransportGroupRif);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						publicTransportGroup.PublicTransportGroupRif = (string)dr["PublicTransportGroupRif"];
					}
				}

				return publicTransportGroup.PublicTransportGroupRif;
			}
		}
	}
}
