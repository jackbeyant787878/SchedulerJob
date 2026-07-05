using SchedulerJobCenter.Api.Filters;
using SchedulerJobCenter.Api.Middleware;
using SchedulerJobCenter.Infrastructure.Extensions;
using SchedulerJobCenter.Application.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration() .WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("SchedulerJobCenter starting up...");

    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog Logging ───────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext());

    // ── API Controllers ───────────────────────────────────────────
    builder.Services
        .AddControllers(opts =>
        {
            opts.Filters.Add<GlobalExceptionFilter>();
        })
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // ── Swagger API Documentation ───────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "SchedulerJob API",
            Version = "v1",
            Description = "Scheduled task scheduling center — Built on Quartz.NET & CQRS pattern"
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            c.IncludeXmlComments(xmlPath);
    });

    // ── CORS Policy ──────────────────────────────────────────────────
    builder.Services.AddCors(opts =>
        opts.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader()));

    // ── Business Layer Registration ────────────────────────────────────────────────
    builder.Services.AddApplication();           // Register MediatR & Command/Query Handlers
    builder.Services.AddInfrastructure(builder.Configuration); // EF Core, Quartz, Repositories

    // ── Health Checks ──────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "sqlserver",
            tags: ["db"]);

    var app = builder.Build();

    // ── Middleware Pipeline ────────────────────────────────────────────
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SchedulerJobCenter API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseCors("AllowAll");
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("SchedulerJobCenter started successfully.");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SchedulerJobCenter terminated unexpectedly.");
}
finally
{
    await Log.CloseAndFlushAsync();
}