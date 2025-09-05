using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using SuperTransp.Core;
using SuperTransp.Middleware;
using static SuperTransp.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddTransient<ISecurity, Security>();
builder.Services.AddTransient<IGeography, Geography>();
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

var app = builder.Build();

app.UseMiddleware<MaintenanceMiddleware>();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error"); 
	app.UseHsts();
}
else
{
	app.UseDeveloperExceptionPage();
}

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
	context.Request.EnableBuffering();
	await next();
});

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Security}/{action=Login}/{id?}");

app.MapRazorPages();

app.Run();