using System.Text;
using System.Threading.RateLimiting;
using AirlineBooking.Api.Middleware;
using AirlineBooking.Api.Serialization;
using AirlineBooking.Application;
using AirlineBooking.Application.Common.Models;
using AirlineBooking.Infrastructure;
using AirlineBooking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---- Layered composition root: each layer exposes one extension method (AddApplication,
// AddInfrastructure) so this file reads as "what the app is made of," not a pile of
// registration details that belong to those layers. ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers(options =>
{
    // Runs FluentValidation against every action argument that has a registered validator -
    // see ValidationActionFilter for why this beats calling validators by hand in each action.
    options.Filters.Add<ValidationActionFilter>();
})
    // Serialize enums as their string names (e.g. "Confirmed"), not raw ordinals ("1") - the
    // same reasoning as storing them as strings in SQL Server (see BookingConfiguration):
    // self-documenting API responses, and stable across future enum reordering.
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        // See UtcDateTimeConverter: every DateTime this API exposes is a UTC instant by
        // convention (*Utc-suffixed properties throughout), but EF Core hands them back as
        // DateTimeKind.Unspecified, which the default writer serializes without a "Z" - this
        // stamps them as UTC on the wire so clients in non-UTC timezones parse them correctly.
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
    });

// .NET 8+'s IExceptionHandler pipeline: GlobalExceptionHandler maps our Application exceptions
// to ProblemDetails; AddProblemDetails() gives every other unhandled 4xx/5xx (e.g. a 404 from
// routing) the same RFC 7807 JSON shape for free, so API consumers get one consistent error
// format regardless of where the failure came from.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---- JWT bearer authentication ----
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ValidateLifetime = true,
            // Default 5-minute clock-skew allowance is unnecessary slack for a single-service
            // API where the token issuer and validator are the same process/clock.
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ---- CORS: the Vite dev server for the agent-console frontend runs on a different origin
// (localhost:5173) than the API (localhost:5178/7091). Scoped to Development only - a deployed
// frontend would be served from an origin added here explicitly, not a wildcard. ----
const string FrontendDevCorsPolicy = "FrontendDev";
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(FrontendDevCorsPolicy, policy =>
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });
}

// ---- Rate limiting (built into ASP.NET Core since .NET 7 - no extra package needed) ----
// A global per-client-IP limiter on every endpoint, plus a tighter dedicated limiter on
// /api/auth/login so credential-stuffing attempts get throttled harder than normal traffic.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// ---- Swagger / OpenAPI ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Airline Booking API",
        Version = "v1",
        Description = "A portfolio-scoped airline booking & reservation API: flight search, " +
                       "seat maps, and the hold -> confirm/cancel booking lifecycle, built on " +
                       "ASP.NET Core, EF Core, and SQL Server."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT returned from POST /api/auth/login (no \"Bearer \" prefix needed here)."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("AirlineBookingDb")!);

var app = builder.Build();

// ---- Dev-only startup tasks: apply pending migrations and seed reference data. In a real
// deployment, migrations run as a controlled CI/CD step (see .github/workflows), not on
// app boot - auto-migrating in Production risks two app instances racing on schema changes
// during a rolling deploy. ----
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AirlineBookingDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbSeeder.SeedAsync(dbContext);
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Airline Booking API v1");
    });
}

if (!app.Environment.IsDevelopment())
{
    // HSTS tells browsers to only ever talk to this host over HTTPS for the given duration -
    // skipped in Development because it self-signs and runs on localhost, where HSTS caching
    // in a browser causes more friction (stale-cert lockout) than it prevents.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    // A minimal, dependency-free set of defense-in-depth response headers: nosniff stops
    // browsers from MIME-sniffing a response into something more dangerous than its declared
    // Content-Type; DENY stops this API's JSON responses from being framed (not that JSON
    // rendered in a frame does much, but it's a zero-cost guard); no-referrer keeps this
    // internal API's URLs (which can contain booking references) out of Referer headers sent
    // to third parties if a client ever links out from a page built on this data.
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    await next();
});

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseCors(FrontendDevCorsPolicy);
}

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program
{
}
