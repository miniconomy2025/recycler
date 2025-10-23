using System.Globalization;
using System.Text;
using System.Text.Json;
using Npgsql;
using Recycler.API;
using Recycler.API.Services;
using Recycler.API.Utils;
using Swashbuckle.AspNetCore.SwaggerUI;

CultureInfo.CurrentCulture = new CultureInfo("en-ZA") { NumberFormat = { NumberDecimalSeparator = "." } };

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddSingleton<ISimulationClock, SimulationClock>();
builder.Services.AddSingleton<ICommercialBankService, CommercialBankService>();
builder.Services.AddScoped<IRecyclingService, RecyclingService>();
builder.Services.AddHostedService<RecyclingBackgroundService>();
builder.Services.AddScoped<IDatabaseResetService, DatabaseResetService>();
builder.Services.AddScoped<SimulationBootstrapService>();
builder.Services.AddScoped<SimulationBootstrapService>();
builder.Services.AddScoped<BankAccountService>();
builder.Services.AddScoped<LoanService>();
builder.Services.AddScoped<MachineMarketService>();
builder.Services.AddScoped<ISimulationBootstrapService, SimulationBootstrapService>();

builder.Services.AddOpenApi(config =>
{
    config.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
        {
            new Microsoft.OpenApi.Models.OpenApiServer
            {
                Url = "https://api.recycler.susnet.co.za",
                Description = "Production Server"
            }
        };

        if (!builder.Environment.IsProduction())
        {
            servers.Add(new Microsoft.OpenApi.Models.OpenApiServer
            {
                Url = "http://localhost:5000",
                Description = "Local Development"
            });
        }

        document.Servers = servers;
        return Task.CompletedTask;
    });
});
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

builder.Services.AddCors(options =>
{
    options.AddPolicy("InternalApiCors", policy =>
    {
        policy
            .WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddScoped<MakePaymentService>();
builder.Services.AddScoped<IRawMaterialService, RawMaterialService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<MakePaymentService, MakePaymentService>();
builder.Services.AddScoped<CommercialBankService, CommercialBankService>();

builder.Services.AddHttpClient();

builder.Services.AddTransient<HttpLoggingHandler>();
builder.Services.AddTransient<GlobalHeaderHandler>();

builder.Services.AddHttpClient("test")
    .AddHttpMessageHandler<HttpLoggingHandler>()
    .AddHttpMessageHandler<GlobalHeaderHandler>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });


Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

new Startup(builder).ConfigureApplication();
var app = builder.Build();


app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "MiniConomy Recycler API v1");
});


app.Use(async (context, next) =>
{
    // Enable buffering so the body can be read multiple times
    context.Request.EnableBuffering();

    // Leave the stream open after reading
    using var reader = new StreamReader(
        context.Request.Body,
        encoding: Encoding.UTF8,
        detectEncodingFromByteOrderMarks: false,
        bufferSize: 1024,
        leaveOpen: true);

    var body = await reader.ReadToEndAsync();

    // Reset the stream position so the next middleware can read it
    context.Request.Body.Position = 0;
    if (context.Request.Path != "/logs")
    {
        String clientid = "";
        foreach (var header in context.Request.Headers)
        {
            if (header.Key == "Client-Id") clientid = $"{header.Value}";
        }
        if (clientid != "idPutSomethingToBlockHere")
        {
            Console.WriteLine($"HTTP {context.Request.Method} {context.Request.Path}");
            Console.WriteLine($"FROM:\t{clientid}");
            Console.WriteLine($"\nBODY:\n{body}\n");
        }
    }


    await next();
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");


//Migrations to run
bool Migrations = false;
if (Migrations)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    // Folder where SQL files live
    var migrationFolder = Path.Combine(AppContext.BaseDirectory, "dbMigrations");
    // Run each file
    foreach (var file in Directory.GetFiles(migrationFolder, "*.sql").OrderBy(f => f))
    {
        var sql = await File.ReadAllTextAsync(file);
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine($"Executed migration: {Path.GetFileName(file)}");
    }
}

//Get logs constantly
app.MapGet("/logs", async (HttpContext context) =>
{
    var logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "app.log");

    if (!File.Exists(logFilePath))
    {
        context.Response.StatusCode = 404;
        return Results.NotFound("Log file not found");
    }

    var logContent = await File.ReadAllTextAsync(logFilePath);
    return Results.Text(logContent, "text/plain");
});
await app.RunAsync();
