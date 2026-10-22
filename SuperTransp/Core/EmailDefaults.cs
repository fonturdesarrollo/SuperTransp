namespace SuperTransp.Core
{
	// TEMP-EMAIL: temporary rule, empty email is stored as N/D. Remove this class when the email becomes required again.
	public static class EmailDefaults
	{
		public const string NotAvailable = "N/D";

		public static string Normalize(string? email)
		{
			var value = email?.Trim();

			if (string.IsNullOrEmpty(value) || value.Equals(NotAvailable, StringComparison.OrdinalIgnoreCase))
				return NotAvailable;

			return value.ToLower();
		}
	}
}
