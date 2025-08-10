using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using SuperTransp.Core;
using static SuperTransp.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMvc();
builder.Services.AddHttpContextAccessor();

// Manage Sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddControllersWithViews(options =>
{
	options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ClientInfoService>();
builder.Services.AddScoped<IFtpService, FtpService>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
    ForwardedHeaders.XForwardedProto
});

app.Use(async (context, next) =>
{
	context.Request.EnableBuffering();
	await next();
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Security}/{action=Login}/{id?}");

app.Run();
