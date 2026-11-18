using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SuperTransp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static SuperTransp.Core.Interfaces;

[ApiController]
[Authorize]
[Route("api/transport")]
public class ApiController : ControllerBase
{
	private IApiCore _apiCore;
	private readonly IConfiguration _configuration;

	public ApiController(IApiCore apiCore, IConfiguration configuration)
	{
		_apiCore = apiCore;
		_configuration = configuration;
	}

	// Issues a short-lived JWT for external consumers. Credentials live in appsettings (Api:Username / Api:Password)
	[AllowAnonymous]
	[IgnoreAntiforgeryToken]
	[HttpPost("auth/token")]
	public IActionResult GetToken([FromBody] ApiTokenRequest request)
	{
		var expectedUser = _configuration["Api:Username"];
		var expectedPassword = _configuration["Api:Password"];

		if (string.IsNullOrEmpty(expectedUser) || string.IsNullOrEmpty(expectedPassword))
		{
			return StatusCode(503, "Las credenciales de la API no estan configuradas.");
		}

		bool valid = request != null
			&& FixedTimeEquals(request.Username, expectedUser)
			& FixedTimeEquals(request.Password, expectedPassword);

		if (!valid)
		{
			return Unauthorized("Usuario o clave invalidos.");
		}

		int minutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out int m) && m > 0 ? m : 60;
		var expires = DateTime.UtcNow.AddMinutes(minutes);

		var credentials = new SigningCredentials(
			new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]!)),
			SecurityAlgorithms.HmacSha256);

		var jwt = new JwtSecurityToken(
			issuer: _configuration["Jwt:Issuer"],
			audience: _configuration["Jwt:Audience"],
			claims: new[] { new Claim(JwtRegisteredClaimNames.Sub, expectedUser) },
			notBefore: DateTime.UtcNow,
			expires: expires,
			signingCredentials: credentials);

		return Ok(new
		{
			accessToken = new JwtSecurityTokenHandler().WriteToken(jwt),
			tokenType = "Bearer",
			expiresInSeconds = minutes * 60,
			expiresAtUtc = expires
		});
	}

	private static bool FixedTimeEquals(string? a, string b)
	{
		return CryptographicOperations.FixedTimeEquals(
			SHA256.HashData(Encoding.UTF8.GetBytes(a ?? string.Empty)),
			SHA256.HashData(Encoding.UTF8.GetBytes(b)));
	}

	[HttpGet("dashboard")]
	public IActionResult GetDashboard()
	{
		try
		{
			return Ok(_apiCore.MapToDashboardModel());
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
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

	[HttpGet("representative/{representativeIdentityDocument}")]
	public IActionResult GetRepresentativeByIdentityDocument(int representativeIdentityDocument)
	{
		try
		{
			var model = _apiCore.MapToRepresentativeByIdentityDocument(representativeIdentityDocument);

			if (model != null)
			{
				return Ok(model);
			}

			return BadRequest("No existen representantes registrados.");
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	[HttpGet("driver/{driverIdentityDocument}")]
	public IActionResult GetDriverByIdentityDocument(int driverIdentityDocument)
	{
		try
		{
			var model = _apiCore.MapToDriverByIdentityDocument(driverIdentityDocument);

			if (model != null)
			{
				return Ok(model);
			}

			return BadRequest("No existen socios registrados.");
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}

	[HttpGet("driver-ptg/{driverIdentityDocument}")]
	public IActionResult GetDriverPTGByIdentityDocument(int driverIdentityDocument)
	{
		try
		{
			var model = _apiCore.MapToDriverPTGByIdentityDocument(driverIdentityDocument);

			if (model != null)
			{
				return Ok(model);
			}

			return BadRequest("No existen socios registrados.");
		}
		catch (Exception e)
		{
			return BadRequest(e.Message);
		}
	}
}