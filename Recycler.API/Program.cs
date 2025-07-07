using System.Globalization;
using Recycler.API;
using Recycler.API.Services;
CultureInfo.CurrentCulture = new CultureInfo("en-ZA") { NumberFormat = { NumberDecimalSeparator = "." } };

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddSingleton<ISimulationClock, SimulationClock>();
builder.Services.AddScoped<IRecyclingService, RecyclingService>();

builder.Services.AddOpenApi();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Configuration.AddJsonFile("secrets.json",
    optional: true,
    reloadOnChange: true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("InternalApiCors", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();

builder.Services.AddHttpClient();

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

await new Startup(builder).ConfigureApplication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MiniConomy Recycler API v1");
    });
    app.Use(async (context, next) =>
    {
        Console.WriteLine($"HTTP {context.Request.Method} {context.Request.Path}");
        await next();
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");

await app.RunAsync();