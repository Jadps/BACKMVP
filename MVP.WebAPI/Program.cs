using MVP.Application;
using MVP.Infrastructure;
using MVP.Application.Interfaces;
using MVP.WebAPI.Services;
using Serilog;
using Asp.Versioning;
using MVP.Infrastructure.Persistence;
using MVP.WebAPI.Middleware;
using Microsoft.AspNetCore.Authorization;
using Scalar.AspNetCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Hangfire;
using Hangfire.PostgreSql;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddOpenApi();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddMvc();

builder.Services.AddMemoryCache();
builder.Services.AddControllers();

builder.Services.AddAntiforgery(options => 
{
    options.HeaderName = "X-XSRF-TOKEN";
});

builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("StrictPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var frontendUrl = builder.Configuration["Config:FrontendUrl"] ?? "http://localhost:4200";
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddInfrastructureSecurity(builder.Configuration);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(
        options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    await app.Services.InitialiseDatabaseAsync();
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.Use(async (context, next) =>
{
    var antiforgery = context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
    var tokens = antiforgery.GetAndStoreTokens(context);
    
    if (tokens.RequestToken != null)
    {
        context.Response.Cookies.Append(
            "XSRF-TOKEN",
            tokens.RequestToken,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None
            });
    }

    if (HttpMethods.IsPost(context.Request.Method) || 
        HttpMethods.IsPut(context.Request.Method) || 
        HttpMethods.IsDelete(context.Request.Method))
    {
        var endpoint = context.GetEndpoint();
        var ignoreAntiforgery = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute>();

        if (ignoreAntiforgery == null)
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { message = "Antiforgery token validation failed." });
                return;
            }
        }
    }

    await next();
});

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MVP.WebAPI.Middleware.HangfireAuthorizationFilter() }
});

app.MapControllers();

app.MapHealthChecks("/api/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name    = e.Key,
                status  = e.Value.Status.ToString(),
                duration = e.Value.Duration,
                description = e.Value.Description,
                tags    = e.Value.Tags
            })
        });
        await context.Response.WriteAsync(result);
    }
}).AllowAnonymous();

app.Run();