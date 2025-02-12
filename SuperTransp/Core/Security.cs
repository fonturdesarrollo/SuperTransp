using SuperTransp.Models;
using Microsoft.Data.SqlClient;
using static SuperTransp.Core.Interfaces;
using System.Text.RegularExpressions;
using System.Reflection;

namespace SuperTransp.Core
{
    public class Security : ISecurity
    {
        private readonly IConfiguration _configuration;
        public Security(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

		private SqlConnection GetOpenConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			sqlConnection.Open();
			return sqlConnection;
		}

		public SecurityUserModel GetValidUser(string login, string password)
		{
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					SecurityUserModel user = new();
					SqlCommand cmd = new($"SELECT * FROM SecurityUser Where Login = '{login}' AND Password = '{password}'", sqlConnection);
					SqlDataReader dr = cmd.ExecuteReader();

					while (dr.Read())
					{
						user.SecurityUserId = (int)dr["SecurityUserId"];
						user.FullName = (string)dr["FullName"];
						user.Login = (string)dr["Login"];
						user.Password = (string)dr["Password"];
						user.SecurityGroupId = (int)dr["SecurityGroupId"];
						user.StateId = (int)dr["StateId"];
					}

					dr.Close();
					sqlConnection.Close();

					return user;
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		public bool GroupModuleHasAccess(int securityGroupId, int securityModuleId)
		{
			using (SqlConnection sqlConnection = GetOpenConnection())
			{
				SqlCommand cmd = new($"SELECT * FROM View_SecurityGetUserGroupModuleAccess Where SecurityGroupId = {securityGroupId} AND SecurityModuleId = {securityModuleId}", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					return true;
				}

				dr.Close();
				sqlConnection.Close();

				return false;
			}
		}

		public List<SecurityGroupModel> GetAllGroups()
		{
			using (SqlConnection sqlConnection = GetOpenConnection())
			{
				List<SecurityGroupModel> groups = new();
				SqlCommand cmd = new($"SELECT * FROM SecurityGroup Where SecurityGroupId <> 1", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					groups.Add(new SecurityGroupModel
					{
						SecurityGroupId = (int)dr["SecurityGroupId"],
						SecurityGroupName = (string)dr["SecurityGroupName"],
						SecurityGroupDescription = (string)dr["SecurityGroupDescription"]
					});
				}

				dr.Close();
				sqlConnection.Close();

				return groups.ToList();
			}
		}

		public List<SecurityGroupModel> GetGroupById(int groupId)
		{
			using (SqlConnection sqlConnection = GetOpenConnection())
			{
				List<SecurityGroupModel> group = new();
				SqlCommand cmd = new($"SELECT * FROM SecurityGroup Where SecurityGroupId = {groupId}", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					group.Add(new SecurityGroupModel
					{
						SecurityGroupId = (int)dr["SecurityGroupId"],
						SecurityGroupName = (string)dr["SecurityGroupName"],
						SecurityGroupDescription = (string)dr["SecurityGroupDescription"]
					});
				}

				dr.Close();
				sqlConnection.Close();

				return group.ToList();
			}
		}

		public List<SecurityStatusUserModel> GetAllUsersStatus()
		{
			using (SqlConnection sqlConnection = GetOpenConnection())
			{
				List<SecurityStatusUserModel> status = new();
				SqlCommand cmd = new($"SELECT * FROM SecurityStatus", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					status.Add(new SecurityStatusUserModel
					{
						SecurityStatusId = (int)dr["SecurityStatusId"],
						SecurityStatusName = (string)dr["SecurityStatusName"],
					});
				}

				dr.Close();
				sqlConnection.Close();

				return status.ToList();
			}
		}

		public int AddOrEditUser(SecurityUserModel model)
		{
			int result = 0;
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					if (model != null)
					{
						SqlCommand cmd = new("Security_UserAddOrEdit", sqlConnection);
						cmd.CommandType = System.Data.CommandType.StoredProcedure;

						cmd.Parameters.AddWithValue("SecurityUserId", model.SecurityUserId);
						cmd.Parameters.AddWithValue("SecurityUserDocumentIdNumber", model.SecurityUserDocumentIdNumber);
						cmd.Parameters.AddWithValue("Login", model.Login);
						cmd.Parameters.AddWithValue("Password", model.Password);
						cmd.Parameters.AddWithValue("FullName", model.FullName.ToUpper());
						cmd.Parameters.AddWithValue("SecurityGroupId", model.SecurityGroupId);
						cmd.Parameters.AddWithValue("SecurityStatusId", model.SecurityStatusId);
						cmd.Parameters.AddWithValue("StateId", model.StateId);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public SecurityUserModel GetUserById(int securityUserId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					SecurityUserModel user = new();
					SqlCommand cmd = new($"SELECT * FROM SecurityUser Where SecurityUserId = {securityUserId}", sqlConnection);
					SqlDataReader dr = cmd.ExecuteReader();

					while (dr.Read())
					{
						user.SecurityUserId = (int)dr["SecurityUserId"];
						user.SecurityUserDocumentIdNumber = (int)dr["SecurityUserDocumentIdNumber"];
						user.FullName = (string)dr["FullName"];
						user.Login = (string)dr["Login"];
						user.Password = (string)dr["Password"];
						user.SecurityGroupId = (int)dr["SecurityGroupId"];
						user.StateId = (int)dr["StateId"];
						user.SecurityStatusId = (int)dr["SecurityStatusId"];
					}

					dr.Close();
					sqlConnection.Close();

					return user;
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		public int RegisteredUser(string paramValue, string verifyBy)
		{
			SecurityUserModel user = new();
			string queryParam = string.Empty;
			using (SqlConnection sqlConnection = GetOpenConnection())
			{
				switch (verifyBy)
				{
					case "SecurityUserDocumentIdNumber":
						queryParam = $"SELECT * FROM SecurityUser WHERE SecurityUserDocumentIdNumber = {paramValue}";
						break;
	                case "Login":
						queryParam = $"SELECT * FROM SecurityUser WHERE Login = '{paramValue}'";
						break ;
				}

				SqlCommand cmd = new($"{queryParam}", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					user.SecurityUserId = (int)dr["SecurityUserId"];
				}

				dr.Close();
				sqlConnection.Close();

				return user.SecurityUserId;
			}
		}

		public List<SecurityUserModel> GetAllUsers()
		{
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					List<SecurityUserModel> users = new();
					SqlCommand cmd = new($"SELECT * FROM Security_GetAllUsers", sqlConnection);
					SqlDataReader dr = cmd.ExecuteReader();

					while (dr.Read())
					{
						users.Add(new SecurityUserModel
						{
							SecurityUserDocumentIdNumber = (int)dr["SecurityUserDocumentIdNumber"],
							Login = (string)dr["Login"],
							Password = (string)dr["Password"],
							FullName = (string)dr["FullName"],
							StateName = (string)dr["StateName"],
							SecurityGroupName = (string)dr["SecurityGroupName"],
							SecurityStatusName = (string)dr["SecurityStatusName"],
							SecurityStatusId = (int)dr["SecurityStatusId"],
							SecurityUserId = (int)dr["SecurityUserId"],
							SecurityGroupId = (int)dr["SecurityGroupId"],
							StateId = (int)dr["StateId"],
						});
					}

					dr.Close();
					sqlConnection.Close();

					return users.ToList();
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		public List<SecurityUserModel> GetAllUsersByStateId(int stateId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					List<SecurityUserModel> users = new();
					SqlCommand cmd = new($"SELECT * FROM Security_GetAllUsers WHERE StateId = {stateId}", sqlConnection);
					SqlDataReader dr = cmd.ExecuteReader();

					while (dr.Read())
					{
						users.Add(new SecurityUserModel
						{
							SecurityUserDocumentIdNumber = (int)dr["SecurityUserDocumentIdNumber"],
							Login = (string)dr["Login"],
							Password = (string)dr["Password"],
							FullName = (string)dr["FullName"],
							StateName = (string)dr["StateName"],
							SecurityGroupName = (string)dr["SecurityGroupName"],
							SecurityStatusName = (string)dr["SecurityStatusName"],
							SecurityStatusId = (int)dr["SecurityStatusId"],
							SecurityUserId = (int)dr["SecurityUserId"],
							SecurityGroupId = (int)dr["SecurityGroupId"],
							StateId = (int)dr["StateId"],
						});
					}

					dr.Close();
					sqlConnection.Close();

					return users.ToList();
				}
			}
			catch (Exception)
			{

				throw;
			}
		}

		public int AddOrEditGroup(SecurityGroupModel model)
		{
			int result = 0;
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					if (model != null)
					{
						SqlCommand cmd = new("Security_GroupAddOrEdit", sqlConnection);
						cmd.CommandType = System.Data.CommandType.StoredProcedure;

						cmd.Parameters.AddWithValue("SecurityGroupId", model.SecurityGroupId);
						cmd.Parameters.AddWithValue("SecurityGroupName", model.SecurityGroupName);
						cmd.Parameters.AddWithValue("SecurityGroupDescription", model.SecurityGroupDescription);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public int AddOrEditModule(SecurityModuleModel model)
		{
			int result = 0;
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					if (model != null)
					{
						SqlCommand cmd = new("Security_ModuleAddOrEdit", sqlConnection);
						cmd.CommandType = System.Data.CommandType.StoredProcedure;

						cmd.Parameters.AddWithValue("SecurityModuleId", model.SecurityModuleId);
						cmd.Parameters.AddWithValue("SecurityModuleName", model.SecurityModuleName);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public List<SecurityModuleModel> GetModuleById(int securityModuleId)
		{
			using (SqlConnection sqlConnection = GetOpenConnection())
			{
				List<SecurityModuleModel> module = new();
				SqlCommand cmd = new($"SELECT * FROM SecurityModule Where SecurityModuleId = {securityModuleId}", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					module.Add(new SecurityModuleModel
					{
						SecurityModuleId = (int)dr["SecurityModuleId"],
						SecurityModuleName = (string)dr["SecurityModuleName"],
					});
				}

				dr.Close();
				sqlConnection.Close();

				return module.ToList();
			}
		}

		public List<SecurityModuleModel> GetAllModules()
		{
			using (SqlConnection sqlConnection = GetOpenConnection())
			{
				List<SecurityModuleModel> modules = new();
				SqlCommand cmd = new($"SELECT * FROM SecurityModule", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					modules.Add(new SecurityModuleModel
					{
						SecurityModuleId = (int)dr["SecurityModuleId"],
						SecurityModuleName = (string)dr["SecurityModuleName"]
					});
				}

				dr.Close();
				sqlConnection.Close();

				return modules.ToList();
			}
		}

		public List<SecurityAccessTypeModel> GetAllAccessTypes()
		{
			using (SqlConnection sqlConnection = GetOpenConnection())
			{
				List<SecurityAccessTypeModel> accessTypes = new();
				SqlCommand cmd = new($"SELECT * FROM SecurityAccessType", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					accessTypes.Add(new SecurityAccessTypeModel
					{
						SecurityAccessTypeId = (int)dr["SecurityAccessTypeId"],
						SecurityAccessTypeName = (string)dr["SecurityAccessTypeName"]
					});
				}

				dr.Close();
				sqlConnection.Close();

				return accessTypes.ToList();
			}
		}

		public List<SecurityGroupModuleModel> GetAllSecurityGroupModuleDetail()
		{
			using (SqlConnection sqlConnection = GetOpenConnection())
			{
				List<SecurityGroupModuleModel> groupMoulesDetail = new();
				SqlCommand cmd = new($"SELECT * FROM Security_SecurityGroupModuleDetail", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					groupMoulesDetail.Add(new SecurityGroupModuleModel
					{
						SecurityGroupModuleId = (int)dr["SecurityGroupModuleId"],
						SecurityGroupId = (int)dr["SecurityGroupId"],
						SecurityGroupName = (string)dr["SecurityGroupName"],
						SecurityModuleId = (int)dr["SecurityModuleId"],
						SecurityModuleName = (string)dr["SecurityModuleName"],
						SecurityAccessTypeId= (int)dr["SecurityAccessTypeId"],
						SecurityAccessTypeName = (string)dr["SecurityAccessTypeName"]
					});
				}

				dr.Close();
				sqlConnection.Close();

				return groupMoulesDetail.ToList();
			}
		}

		public int AddOrEditGroupModules(SecurityGroupModuleModel model)
		{
			int result = 0;
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					if (model != null)
					{
						SqlCommand cmd = new("Security_GroupModuleAddOrEdit", sqlConnection);
						cmd.CommandType = System.Data.CommandType.StoredProcedure;

						cmd.Parameters.AddWithValue("@SecurityGroupModuleId", model.SecurityGroupModuleId);
						cmd.Parameters.AddWithValue("SecurityGroupId", model.SecurityGroupId);
						cmd.Parameters.AddWithValue("SecurityModuleId", model.SecurityModuleId);
						cmd.Parameters.AddWithValue("SecurityAccessTypeId", model.SecurityAccessTypeId);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}

		public int DeleteGroupModules(int securityGroupModuleId)
		{
			int result = 0;
			try
			{
				using (SqlConnection sqlConnection = GetOpenConnection())
				{
					if (securityGroupModuleId != 0)
					{
						SqlCommand cmd = new($"DELETE FROM SecurityGroupModule WHERE SecurityGroupModuleId = {securityGroupModuleId}", sqlConnection);
						SqlDataReader dr = cmd.ExecuteReader();
					}
				}

				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
