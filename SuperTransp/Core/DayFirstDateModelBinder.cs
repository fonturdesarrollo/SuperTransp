using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SuperTransp.Core
{
	// Parses dates independently of the server culture: the UI sends dd/MM/yyyy,
	// but ISO yyyy-MM-dd is accepted too.
	public class DayFirstDateModelBinder : IModelBinder
	{
		private static readonly string[] Formats =
		{
			"dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss"
		};

		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

			if (result == ValueProviderResult.None)
				return Task.CompletedTask;

			bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);

			var raw = result.FirstValue?.Trim();

			if (string.IsNullOrEmpty(raw))
				return Task.CompletedTask;

			if (DateTime.TryParseExact(raw, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
			{
				bindingContext.Result = ModelBindingResult.Success(date);
			}
			else
			{
				bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "La fecha de nacimiento no tiene un formato válido (dd/MM/yyyy)");
			}

			return Task.CompletedTask;
		}
	}
}
