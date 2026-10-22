using System.Security.Cryptography;
using System.Text;

namespace SuperTransp.Core
{
	/// <summary>
	/// Signs and verifies the code carried by the driver QR (verification page QR/DriverData).
	/// Format: "{organization GUID}|{partner number}|{signature}". The organization GUID is shared by every
	/// partner of the organization and partner numbers are sequential, so without a signature anyone holding a
	/// single valid QR could walk through all the partners of that organization.
	/// </summary>
	public class QrCodeSigner
	{
		private readonly byte[] _key;

		public QrCodeSigner(IConfiguration configuration)
		{
			// Dedicated key when configured (Qr:SigningKey); otherwise derived from the JWT key so existing
			// installations work without new settings
			var configuredKey = configuration["Qr:SigningKey"];
			if (!string.IsNullOrEmpty(configuredKey))
			{
				_key = Encoding.UTF8.GetBytes(configuredKey);
			}
			else
			{
				var jwtKey = configuration["Jwt:SecretKey"];
				if (string.IsNullOrEmpty(jwtKey))
				{
					throw new InvalidOperationException("No hay clave para firmar los códigos QR: configure 'Qr:SigningKey' o 'Jwt:SecretKey'.");
				}

				_key = HMACSHA256.HashData(Encoding.UTF8.GetBytes(jwtKey), Encoding.UTF8.GetBytes("supertransp-qr-v1"));
			}

		}

		public string BuildDriverCode(string publicTransportGroupGUID, int partnerNumber)
		{
			return $"{publicTransportGroupGUID}|{partnerNumber}|{Sign(publicTransportGroupGUID, partnerNumber)}";
		}

		/// <summary>True when the code is well formed and its signature is valid.</summary>
		public bool TryParseDriverCode(string? driverCode, out string publicTransportGroupGUID, out int partnerNumber)
		{
			publicTransportGroupGUID = string.Empty;
			partnerNumber = 0;

			if (string.IsNullOrWhiteSpace(driverCode)) return false;

			var parts = driverCode.Split('|');
			// Only signed codes exist: "guid|partner|signature"
			if (parts.Length != 3) return false;
			if (string.IsNullOrWhiteSpace(parts[0]) || parts[0].Length > 100) return false;
			if (!int.TryParse(parts[1], out var partner) || partner <= 0) return false;

			var expected = Encoding.ASCII.GetBytes(Sign(parts[0], partner));
			var received = Encoding.ASCII.GetBytes(parts[2]);
			if (!CryptographicOperations.FixedTimeEquals(expected, received)) return false;

			publicTransportGroupGUID = parts[0];
			partnerNumber = partner;
			return true;
		}

		private string Sign(string publicTransportGroupGUID, int partnerNumber)
		{
			var hash = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes($"{publicTransportGroupGUID}|{partnerNumber}"));

			// 128 bits are enough and keep the QR small; base64url so it is safe inside the URL
			return Convert.ToBase64String(hash, 0, 16).TrimEnd('=').Replace('+', '-').Replace('/', '_');
		}
	}
}
