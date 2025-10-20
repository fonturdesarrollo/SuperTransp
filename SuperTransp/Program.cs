using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SuperTransp.Core;
using SuperTransp.Middleware;
using System.Text;
using static SuperTransp.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:SecretKey"];
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false;
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = false,
		ValidateAudience = false
	};
});

builder.Services.AddControllers();
builder.Services.AddControllersWithViews(options =>
{
	options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromDays(20);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

builder.Services.Configure<FormOptions>(options =>
{
	options.MultipartBodyLengthLimit = 20_000_000;
});

builder.Services.AddTransient<ISecurity, Security>();
builder.Services.AddTransient<IGeography, SuperTransp.Core.Geography>();
builder.Services.AddTransient<IPublicTransportGroup, PublicTransportGroup>();
builder.Services.AddTransient<IDesignation, Designation>();
builder.Services.AddTransient<IMode, Mode>();
builder.Services.AddTransient<IUnion, Union>();
builder.Services.AddTransient<IDriver, Driver>();
builder.Services.AddTransient<ISupervision, Supervision>();
builder.Services.AddTransient<ICommonData, CommonData>();
builder.Services.AddTransient<IReport, Reports>();
builder.Services.AddScoped<ClientInfoService>();
builder.Services.AddScoped<IFtpService, FtpService>();
builder.Services.AddScoped<IUniverse, Universe>();
builder.Services.AddScoped<IExcelExporter, ExcelExporter>();
builder.Services.AddScoped<IApiCore, ApiCore>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<MaintenanceMiddleware>();

//app.UseDeveloperExceptionPage();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
	ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.Use(async (context, next) =>
{
	if (context.Request.Body != null && context.Request.ContentLength > 0)
	{
		context.Request.EnableBuffering();
	}
	await next();
});

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Security}/{action=Login}/{id?}");

app.MapControllers();

app.MapRazorPages();
app.Run();