var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddControllers();

builder.Services.AddOpenApi();


// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Miniconomy Recycler", Version = "v1" });
//     c.EnableAnnotations();
//     c.CustomSchemaIds(type => type.ToString());
// });


// await new Startup(builder).ConfigureApplication();

// builder.Services.RegisterAllTypes(typeof(IRepository<>), ServiceLifetime.Scoped);
// builder.Services.AddScoped<IClaimValidationService, ClaimValidationService>();
// builder.Services.AddMediatR(mediatRServiceConfiguration =>
//     mediatRServiceConfiguration.RegisterServicesFromAssembly(typeof(GetClaimLineItemsForClaimQuery).Assembly));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "MiniConomy Recycler API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/healthz");

await app.RunAsync();