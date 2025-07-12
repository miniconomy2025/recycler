using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Recycler.API;
using Recycler.API.Services;

CultureInfo.CurrentCulture = new CultureInfo("en-ZA") { NumberFormat = { NumberDecimalSeparator = "." } };

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
        httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;

        var rootCa = X509CertificateLoader.LoadCertificateFromFile("certs/root-ca.der");

        httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
        {
            var chainPolicy = new X509ChainPolicy
            {
                RevocationMode = X509RevocationMode.NoCheck,
                VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
                                    | X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown
                                    | X509VerificationFlags.IgnoreEndRevocationUnknown,
            };
            chainPolicy.ExtraStore.Add(rootCa);
            bool isValid = chain.Build(cert); chain.ChainPolicy = chainPolicy;
            Console.WriteLine($"Kestrel validation: {isValid}, Subject: {cert.Subject}");

            return chain.Build(cert);
        };
    });
});

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

builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("InternalApiCors", policy =>
    {
        policy
            .WithOrigins("https://recycler.projects.bbdgrad.com")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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

builder.Services
    .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(options =>
    {
        options.AllowedCertificateTypes = CertificateTypes.All;
        options.ChainTrustValidationMode = X509ChainTrustMode.CustomRootTrust;
        options.CustomTrustStore.Add(new X509Certificate2("certs/root-ca.der"));
        options.RevocationMode = X509RevocationMode.NoCheck;

        options.Events = new CertificateAuthenticationEvents
        {
            OnCertificateValidated = context =>
            {
                Console.WriteLine("OnCertificateValidated triggered");
                Console.WriteLine($"Subject: {context.ClientCertificate.Subject}");

                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, context.ClientCertificate.Subject),
                    new Claim(ClaimTypes.Name, context.ClientCertificate.Subject),
                }, context.Scheme.Name));

                context.Success();
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("OnAuthenticationFailed triggered");
                Console.WriteLine($"Reason: {context.Exception?.Message}");
                context.Fail("Invalid certificate");
                return Task.CompletedTask;
            }
        };
    });

// builder.Services.
// AddTransient<HttpClientHandler>(sp =>
// {

// });

builder.Services.AddHttpClient("test")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var cert = new X509Certificate2("certs/client.pfx", "1234",
    X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);

        return new HttpClientHandler
        {
            ClientCertificates = { cert },
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
    Console.WriteLine($"HTTP {context.Request.Method} {context.Request.Path}");
    await next();
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");

await app.RunAsync();
