using SuperTransp.Models;
using System.Data.SqlClient;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
    public class Security : ISecurity
    {
        private readonly IConfiguration _configuration;
        public Security(IConfiguration configuration)
        {
            this._configuration = configuration;
        }
		public SecurityUserModel GetValidUser(string login, string password)
		{
			try
			{
				SecurityUserModel user = new();
				using SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
				sqlConnection.Open();
				SqlCommand cmd = new($"SELECT * FROM SecurityUser Where Login = '{login}' AND Password = '{password}'", sqlConnection);
				SqlDataReader dr = cmd.ExecuteReader();

				while (dr.Read())
				{
					user.SecurityUserId = (int)dr["SecurityUserId"];
					user.FullName = (string)dr["FullName"];
					user.Login = (string)dr["Login"];
					user.Password = (string)dr["Password"];
					user.SecurityGroupId = (int)dr["SecurityGroupId"];
				}

				return user;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
