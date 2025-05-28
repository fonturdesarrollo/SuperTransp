using SuperTransp.Models;
using Microsoft.Data.SqlClient;
using static SuperTransp.Core.Interfaces;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Data;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Security.Cryptography;
using System.Text;

namespace SuperTransp.Core
{
    public class Security : ISecurity
    {
        private readonly IConfiguration _configuration;
		private static readonly string Key = "supertranspPasswordKey*-";
		private readonly IHttpContextAccessor _httpContextAccessor;

		public Security(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            this._configuration = configuration;
			this._httpContextAccessor = httpContextAccessor;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public SecurityUserViewModel GetValidUser(string login, string password)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SecurityUserViewModel user = new();
					SqlCommand cmd = new("SELECT  dbo.SecurityUser.SecurityUserId, dbo.SecurityUser.SecurityUserDocumentIdNumber, dbo.SecurityUser.Login, dbo.SecurityUser.Password, dbo.SecurityUser.FullName, dbo.SecurityUser.SecurityGroupId," +
					" dbo.SecurityUser.SecurityStatusId, dbo.State.StateId, dbo.State.StateName, dbo.SecurityUser.SecurityUserDateAdded " +
					" FROM  dbo.SecurityUser INNER JOIN  dbo.State ON dbo.SecurityUser.StateId = dbo.State.StateId" +
					" WHERE  (dbo.SecurityUser.Login = @Login) AND (dbo.SecurityUser.Password = @Password)", sqlConnection);
					cmd.Parameters.AddWithValue("@Login", login);
					cmd.Parameters.AddWithValue("@Password", password);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							user.SecurityUserId = (int)dr["SecurityUserId"];
							user.FullName = (string)dr["FullName"];
							user.Login = (string)dr["Login"];
							user.Password = (string)dr["Password"];
							user.SecurityGroupId = (int)dr["SecurityGroupId"];
							user.StateId = (int)dr["StateId"];
							user.StateName = (string)dr["StateName"];
						}
					}

					return user;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener el usuario válido " + _configuration.GetConnectionString("connectionString") + " " + ex.Message, ex);
			}
		}

		public bool GroupHasAccessToModule(int securityGroupId, int securityModuleId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					using (SqlCommand cmd = new SqlCommand("SELECT * FROM Security_GetUserGroupModuleAccess WHERE SecurityGroupId = @SecurityGroupId AND SecurityModuleId = @SecurityModuleId", sqlConnection))
					{
						cmd.Parameters.Add("@SecurityGroupId", SqlDbType.Int).Value = securityGroupId;
						cmd.Parameters.Add("@SecurityModuleId", SqlDbType.Int).Value = securityModuleId;

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							return dr.HasRows;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al verificar el acceso del módulo del grupo", ex);
			}
		}

		public bool IsInactiveLogin(string login)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					using (SqlCommand cmd = new SqlCommand("SELECT * FROM SecurityUser WHERE Login = @Login AND SecurityStatusId = @SecurityStatusId", sqlConnection))
					{
						cmd.Parameters.Add("@Login", SqlDbType.VarChar).Value = login;
						cmd.Parameters.Add("@SecurityStatusId", SqlDbType.Int).Value = 2;

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							return dr.HasRows;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al verificar el tipo de acceso al modulo {ex.Message}", ex);
			}
		}

		public bool IsBlockedLogin(string login)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					using (SqlCommand cmd = new SqlCommand("SELECT * FROM SecurityUser WHERE Login = @Login AND SecurityStatusId = @SecurityStatusId", sqlConnection))
					{
						cmd.Parameters.Add("@Login", SqlDbType.VarChar).Value = login;
						cmd.Parameters.Add("@SecurityStatusId", SqlDbType.Int).Value = 3;

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							return dr.HasRows;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al verificar el tipo de acceso al modulo {ex.Message}", ex);
			}
		}

		public bool BlockLogin(string login)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					using (SqlCommand cmd = new SqlCommand("UPDATE SecurityUser SET SecurityStatusId = @SecurityStatusId WHERE Login = @Login", sqlConnection))
					{
						cmd.Parameters.Add("@SecurityStatusId", SqlDbType.Int).Value = 3;
						cmd.Parameters.Add("@Login", SqlDbType.VarChar).Value = login;

						cmd.ExecuteNonQuery();

						return true;
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al bloquear el login {ex.Message}", ex);
			}
		}

		public bool IsTotalAccess(int securityModuleId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}
					
					var userGroup = _httpContextAccessor.HttpContext?.Session.GetInt32("SecurityGroupId");

					using (SqlCommand cmd = new SqlCommand("SELECT * FROM Security_GetUserGroupModuleAccess WHERE SecurityGroupId = @SecurityGroupId AND SecurityModuleId = @SecurityModuleId AND SecurityAccessTypeId = @SecurityAccessTypeId", sqlConnection))
					{
						cmd.Parameters.Add("@SecurityGroupId", SqlDbType.Int).Value = userGroup;
						cmd.Parameters.Add("@SecurityModuleId", SqlDbType.Int).Value = securityModuleId;
						cmd.Parameters.Add("@SecurityAccessTypeId", SqlDbType.Int).Value = 1; //total access

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							return dr.HasRows;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al verificar el tipo de acceso al modulo {ex.Message}", ex);
			}
		}

		public List<SecurityGroupModel> GetAllGroups()
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				List<SecurityGroupModel> groups = new();
				SqlCommand cmd = new("SELECT * FROM SecurityGroup WHERE SecurityGroupId <> 1", sqlConnection);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						groups.Add(new SecurityGroupModel
						{
							SecurityGroupId = (int)dr["SecurityGroupId"],
							SecurityGroupName = (string)dr["SecurityGroupName"],
							SecurityGroupDescription = (string)dr["SecurityGroupDescription"]
						});
					}
				}

				return groups.ToList();
			}
		}

		public List<SecurityGroupModel> GetGroupById(int groupId)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				List<SecurityGroupModel> group = new();
				SqlCommand cmd = new("SELECT * FROM SecurityGroup WHERE SecurityGroupId = @GroupId", sqlConnection);
				cmd.Parameters.AddWithValue("@GroupId", groupId);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						group.Add(new SecurityGroupModel
						{
							SecurityGroupId = (int)dr["SecurityGroupId"],
							SecurityGroupName = (string)dr["SecurityGroupName"],
							SecurityGroupDescription = (string)dr["SecurityGroupDescription"]
						});
					}
				}

				return group.ToList();
			}
		}

		public List<SecurityStatusUserModel> GetAllUsersStatus()
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				List<SecurityStatusUserModel> status = new();
				SqlCommand cmd = new("SELECT * FROM SecurityStatus WHERE SecurityStatusId < @SecurityStatusId", sqlConnection);
				cmd.Parameters.AddWithValue("@SecurityStatusId", 3);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						status.Add(new SecurityStatusUserModel
						{
							SecurityStatusId = (int)dr["SecurityStatusId"],
							SecurityStatusName = (string)dr["SecurityStatusName"],
						});
					}
				}

				return status.ToList();
			}
		}

		public int AddOrEditUser(SecurityUserViewModel model)
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
						SqlCommand cmd = new("Security_UserAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@SecurityUserId", model.SecurityUserId);
						cmd.Parameters.AddWithValue("@SecurityUserDocumentIdNumber", model.SecurityUserDocumentIdNumber);
						cmd.Parameters.AddWithValue("@Login", model.Login);
						cmd.Parameters.AddWithValue("@Password", model.Password);
						cmd.Parameters.AddWithValue("@FullName", model.FullName.ToUpper());
						cmd.Parameters.AddWithValue("@SecurityGroupId", model.SecurityGroupId);
						cmd.Parameters.AddWithValue("@SecurityStatusId", model.SecurityStatusId);
						cmd.Parameters.AddWithValue("@StateId", model.StateId);

						result = Convert.ToInt32(cmd.ExecuteScalar());

						AddLogbook(model.SecurityUserId, false, $"usuario {model.FullName.ToUpper()} login {model.Login} grupo Id {model.SecurityGroupId} estatus {model.SecurityStatusId}");
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al añadir o editar el usuario {ex.Message}", ex);
			}
		}

		public SecurityUserViewModel GetUserById(int securityUserId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SecurityUserViewModel user = new();
					SqlCommand cmd = new("SELECT * FROM SecurityUser WHERE SecurityUserId = @SecurityUserId", sqlConnection);
					cmd.Parameters.AddWithValue("@SecurityUserId", securityUserId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
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
					}

					return user;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener el usuario por ID", ex);
			}
		}

		public int RegisteredUser(string paramValue, string verifyBy)
		{
			SecurityUserViewModel user = new();
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				string queryParam = verifyBy switch
				{
					"SecurityUserDocumentIdNumber" => "SELECT * FROM SecurityUser WHERE SecurityUserDocumentIdNumber = @ParamValue",
					"Login" => "SELECT * FROM SecurityUser WHERE Login = @ParamValue",
					_ => throw new ArgumentException("Invalid verification parameter")
				};

				SqlCommand cmd = new(queryParam, sqlConnection);
				cmd.Parameters.AddWithValue("@ParamValue", paramValue);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						user.SecurityUserId = (int)dr["SecurityUserId"];
					}
				}

				return user.SecurityUserId;
			}
		}

		public List<SecurityUserViewModel> GetAllUsers()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<SecurityUserViewModel> users = new();
					SqlCommand cmd = new("SELECT * FROM Security_GetAllUsers", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							users.Add(new SecurityUserViewModel
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
					}

					return users.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener todos los usuarios", ex);
			}
		}

		public List<SecurityUserViewModel> GetAllUsersByStateId(int stateId)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<SecurityUserViewModel> users = new();
					SqlCommand cmd = new("SELECT * FROM Security_GetAllUsers WHERE StateId = @StateId", sqlConnection);
					cmd.Parameters.AddWithValue("@StateId", stateId);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							users.Add(new SecurityUserViewModel
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
					}

					return users.ToList();
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al obtener todos los usuarios por estado ID", ex);
			}
		}

		public int AddOrEditGroup(SecurityGroupModel model)
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
						SqlCommand cmd = new("Security_GroupAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@SecurityGroupId", model.SecurityGroupId);
						cmd.Parameters.AddWithValue("@SecurityGroupName", model.SecurityGroupName);
						cmd.Parameters.AddWithValue("@SecurityGroupDescription", model.SecurityGroupDescription);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al añadir o editar el grupo de seguridad", ex);
			}
		}

		public int AddOrEditModule(SecurityModuleModel model)
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
						SqlCommand cmd = new("Security_ModuleAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@SecurityModuleId", model.SecurityModuleId);
						cmd.Parameters.AddWithValue("@SecurityModuleName", model.SecurityModuleName);
						cmd.Parameters.AddWithValue("@SecurityModuleDescription", model.SecurityModuleDescription);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al añadir o editar el módulo de seguridad", ex);
			}
		}

		public List<SecurityModuleModel> GetModuleById(int securityModuleId)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				List<SecurityModuleModel> module = new();
				SqlCommand cmd = new("SELECT * FROM SecurityModule WHERE SecurityModuleId = @SecurityModuleId", sqlConnection);
				cmd.Parameters.AddWithValue("@SecurityModuleId", securityModuleId);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						module.Add(new SecurityModuleModel
						{
							SecurityModuleId = (int)dr["SecurityModuleId"],
							SecurityModuleName = (string)dr["SecurityModuleName"],
							SecurityModuleDescription = (string)dr["SecurityModuleDescription"],
						});
					}
				}

				return module.ToList();
			}
		}

		public List<SecurityModuleModel> GetAllModules()
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				List<SecurityModuleModel> modules = new();
				SqlCommand cmd = new("SELECT * FROM SecurityModule", sqlConnection);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						modules.Add(new SecurityModuleModel
						{
							SecurityModuleId = (int)dr["SecurityModuleId"],
							SecurityModuleName = (string)dr["SecurityModuleName"],
							SecurityModuleDescription = (string?)dr["SecurityModuleDescription"]
						});
					}
				}

				return modules.ToList();
			}
		}

		public List<SecurityModuleModel> GetModulesByGroupId(int groupId)
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				List<SecurityModuleModel> modules = new();
				SqlCommand cmd = new("SELECT * FROM SecurityGroupModule WHERE SecurityGroupId = @SecurityGroupId ORDER BY SecurityGroupModule.SecurityModuleId", sqlConnection);
				cmd.Parameters.AddWithValue("@SecurityGroupId", groupId);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						modules.Add(new SecurityModuleModel
						{
							SecurityModuleId = (int)dr["SecurityModuleId"]
						});
					}
				}

				return modules.ToList();
			}
		}

		public List<SecurityAccessTypeModel> GetAllAccessTypes()
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				List<SecurityAccessTypeModel> accessTypes = new();
				SqlCommand cmd = new("SELECT * FROM SecurityAccessType", sqlConnection);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						accessTypes.Add(new SecurityAccessTypeModel
						{
							SecurityAccessTypeId = (int)dr["SecurityAccessTypeId"],
							SecurityAccessTypeName = (string)dr["SecurityAccessTypeName"]
						});
					}
				}

				return accessTypes.ToList();
			}
		}

		public List<SecurityGroupModuleModel> GetAllSecurityGroupModuleDetail()
		{
			using (SqlConnection sqlConnection = GetConnection())
			{
				if (sqlConnection.State == ConnectionState.Closed)
				{
					sqlConnection.Open();
				}

				List<SecurityGroupModuleModel> groupModulesDetail = new();
				SqlCommand cmd = new("SELECT * FROM Security_SecurityGroupModuleDetail", sqlConnection);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						groupModulesDetail.Add(new SecurityGroupModuleModel
						{
							SecurityGroupModuleId = (int)dr["SecurityGroupModuleId"],
							SecurityGroupId = (int)dr["SecurityGroupId"],
							SecurityGroupName = (string)dr["SecurityGroupName"],
							SecurityModuleId = (int)dr["SecurityModuleId"],
							SecurityModuleName = (string)dr["SecurityModuleName"],
							SecurityAccessTypeId = (int)dr["SecurityAccessTypeId"],
							SecurityAccessTypeName = (string)dr["SecurityAccessTypeName"]
						});
					}
				}

				return groupModulesDetail.ToList();
			}
		}

		public int AddOrEditGroupModules(SecurityGroupModuleModel model)
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
						SqlCommand cmd = new("Security_GroupModuleAddOrEdit", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@SecurityGroupModuleId", model.SecurityGroupModuleId);
						cmd.Parameters.AddWithValue("@SecurityGroupId", model.SecurityGroupId);
						cmd.Parameters.AddWithValue("@SecurityModuleId", model.SecurityModuleId);
						cmd.Parameters.AddWithValue("@SecurityAccessTypeId", model.SecurityAccessTypeId);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al añadir o editar los módulos de grupo de seguridad", ex);
			}
		}
		public int DeleteGroupModules(int securityGroupModuleId)
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

					if (securityGroupModuleId != 0)
					{
						SqlCommand cmd = new("DELETE FROM SecurityGroupModule WHERE SecurityGroupModuleId = @SecurityGroupModuleId", sqlConnection);
						cmd.Parameters.AddWithValue("@SecurityGroupModuleId", securityGroupModuleId);

						result = cmd.ExecuteNonQuery();
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al eliminar los módulos de grupo de seguridad", ex);
			}
		}

		public int ChangePassword(SecurityUserViewModel model)
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
						SqlCommand cmd = new("Security_ChangePassword", sqlConnection)
						{
							CommandType = System.Data.CommandType.StoredProcedure
						};

						cmd.Parameters.AddWithValue("@SecurityUserId", model.SecurityUserId);
						cmd.Parameters.AddWithValue("@Password", model.NewPassword);

						result = Convert.ToInt32(cmd.ExecuteScalar());
					}
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception("Error al añadir o editar el usuario", ex);
			}
		}

		public bool OldPasswordValid(int securityUserId, string oldPassword)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					using (SqlCommand cmd = new SqlCommand("SELECT * FROM SecurityUser WHERE SecurityUserId = @SecurityUserId AND Password = @Password", sqlConnection))
					{
						cmd.Parameters.Add("@SecurityUserId", SqlDbType.Int).Value = securityUserId;
						cmd.Parameters.Add("@Password", SqlDbType.VarChar).Value = oldPassword;

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							return dr.HasRows;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Error al verificar el password anterior", ex);
			}
		}

		public string? Encrypt(string plainText)
		{
			using (Aes aes = Aes.Create())
			{
				aes.Key = Encoding.UTF8.GetBytes(Key);
				aes.IV = new byte[16];

				using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
				using (var ms = new MemoryStream())
				{
					using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
					using (var writer = new StreamWriter(cs))
					{
						writer.Write(plainText);
					}
					return Convert.ToBase64String(ms.ToArray());
				}
			}
		}

		public string Decrypt(string encryptedText)
		{
			using (Aes aes = Aes.Create())
			{
				aes.Key = Encoding.UTF8.GetBytes(Key);
				aes.IV = new byte[16];

				using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
				using (var ms = new MemoryStream(Convert.FromBase64String(encryptedText)))
				using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
				using (var reader = new StreamReader(cs))
				{
					return reader.ReadToEnd();
				}
			}
		}

		public int AddLogbook(int processId, bool isDeleteAction, string actionDescription)
		{
			int result = 0;
			string addEditDelete = processId == 0 ? "Agregó" : "Modificó";

			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SqlCommand cmd = new("Security_LogbookAdd", sqlConnection)
					{
						CommandType = System.Data.CommandType.StoredProcedure
					};

					var userFullName = _httpContextAccessor.HttpContext?.Session.GetString("FullName");
					var userLogin = _httpContextAccessor.HttpContext?.Session.GetString("UserLogin");
					var userState = _httpContextAccessor.HttpContext?.Session.GetString("StateName");
					var deviceIP = _httpContextAccessor.HttpContext?.Session.GetString("DeviceIP");

					if (isDeleteAction)
					{
						addEditDelete = "Eliminó";
					}					

					cmd.Parameters.AddWithValue("@DeviceIP", deviceIP);
					cmd.Parameters.AddWithValue("@UserFullName", userFullName);
					cmd.Parameters.AddWithValue("@UserLogin", userLogin);
					cmd.Parameters.AddWithValue("@UserState", userState);
					cmd.Parameters.AddWithValue("@ActionDescription",  $"{addEditDelete} {actionDescription}");

					result = Convert.ToInt32(cmd.ExecuteScalar());					
				}

				return result;
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al añadir el logbook {ex.Message}", ex);
			}
		}
	}
}
