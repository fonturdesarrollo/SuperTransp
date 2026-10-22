using Microsoft.Data.SqlClient;
using SuperTransp.Models;
using System.Data;
using static SuperTransp.Core.Interfaces;

namespace SuperTransp.Core
{
	public class ExchangeRate : IExchangeRate
	{
		private readonly IConfiguration _configuration;
		public ExchangeRate(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		private SqlConnection GetConnection()
		{
			SqlConnection sqlConnection = new(_configuration.GetConnectionString("connectionString"));
			return sqlConnection;
		}

		public List<ExchangeRateViewModel> GetAll()
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					List<ExchangeRateViewModel> rates = new();
					SqlCommand cmd = new("SELECT * FROM ExchangeRate", sqlConnection);

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							rates.Add(new ExchangeRateViewModel
							{
								CurrencyCode = ((string)dr["CurrencyCode"]).Trim(),
								Rate = (decimal)dr["Rate"],
								FetchedAt = (DateTime)dr["FetchedAt"]
							});
						}
					}

					return rates;
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al obtener la tasa de cambio {ex.Message}", ex);
			}
		}

		public void Save(string currencyCode, decimal rate)
		{
			try
			{
				using (SqlConnection sqlConnection = GetConnection())
				{
					if (sqlConnection.State == ConnectionState.Closed)
					{
						sqlConnection.Open();
					}

					SqlCommand cmd = new("SuperTransp_ExchangeRateAddOrEdit", sqlConnection)
					{
						CommandType = CommandType.StoredProcedure
					};

					cmd.Parameters.AddWithValue("@CurrencyCode", currencyCode);
					cmd.Parameters.AddWithValue("@Rate", rate);

					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al guardar la tasa de cambio {ex.Message}", ex);
			}
		}
	}
}
