using System.Globalization;
using System.Text;
using Npgsql;
using Recycler.API;
using Recycler.API.Services;
using Recycler.API.Utils;

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

builder.Services.AddOpenApi();

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
    .AddHttpMessageHandler<GlobalHeaderHandler>();


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

    Console.WriteLine($"HTTP {context.Request.Method} {context.Request.Path}");
    Console.WriteLine($"\nBODY:\n{body}\n");

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

await app.RunAsync();
