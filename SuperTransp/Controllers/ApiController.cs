using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static SuperTransp.Core.Interfaces;

[ApiController]
[Authorize]
[Route("api/transport")]
public class ApiController : ControllerBase
{
	private IApiCore _apiCore;

	public ApiController(IApiCore apiCore)
	{
		_apiCore = apiCore;
	}

	[HttpGet("person/{idCard}")]
	public IActionResult GetAll(string idCard)
	{
		try
		{
			if (!int.TryParse(idCard, out _))
			{
				return BadRequest("La cedula permitida debe consistir solo de numeros.");
			}

			var model = _apiCore.MapToDriversModel(int.Parse(idCard));

			if(model != null)
			{
				return Ok(model);
			}

			return BadRequest("No existe el numero de cedula indicado.");
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	[HttpGet("super-routes/all")]
	public IActionResult GetAllRoutes()
	{
		try
		{
			var model = _apiCore.MapToRoutesModel();

			if(model != null)
			{
				return Ok(model);
			}

			return BadRequest("No existen rutas registradas.");	
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	[HttpGet("super-route/{rif}")]
	public IActionResult GetAllRoutesByRif(string rif)
	{
		try
		{
			if(string.IsNullOrEmpty(rif))
			{
				return BadRequest("El rif no puede estar vacio.");
			}

			var model = _apiCore.MapToRoutesByRifModel(rif);

			if (model != null)
			{
				return Ok(model);
			}

			return BadRequest("No existen rutas registradas.");
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}
}