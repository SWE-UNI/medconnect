using System.Text;
using MedConnect.Data;
using MedConnect.Domain;
using MedConnect.Features.Facilities;
using MedConnect.Features.Divisions;
using MedConnect.Features.Appointments;
using MedConnect.Features.FieldVisits;
using MedConnect.Features.Identity;
using MedConnect.Features.LabResults;
using MedConnect.Features.Patients;
using MedConnect.Features.Prescriptions;
using MedConnect.Features.Referrals;
using MedConnect.Features.Visits;
using MedConnect.Features.Vitals;
using MedConnect.Hubs;
using MedConnect.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!))
        };

        // SignalR can't set an Authorization header on WebSocket/long-polling
        // transports, so the client sends the JWT as an `access_token` query
        // string parameter. Read it here and feed it into the handler.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken)
                    && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Required in .NET 10 for the parameterless UseExceptionHandler() below:
// without it, calling UseExceptionHandler() in non-Development environments
// throws at startup. Converts unhandled API errors into JSON ProblemDetails.
builder.Services.AddProblemDetails();

// The Blazor SPA is deployed separately (Cloudflare Pages) and talks to this API
// cross-origin. Named origins are required because SignalR + AllowCredentials
// forbid AllowAnyOrigin. Config: Cors__Origins = a comma-separated allowlist.
var corsOrigins = builder.Configuration["Cors:Origins"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy("MedConnectSpa", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<TotpService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<IFacilityRepository, FacilityRepository>();
builder.Services.AddScoped<FacilityService>();

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<PatientService>();

builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<VisitService>();

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<AppointmentService>();

builder.Services.AddScoped<IReferralRepository, ReferralRepository>();
builder.Services.AddScoped<ReferralService>();

builder.Services.AddScoped<IFieldVisitLogRepository, FieldVisitLogRepository>();
builder.Services.AddScoped<FieldVisitLogService>();

builder.Services.AddScoped<IVitalSignRepository, VitalSignRepository>();
builder.Services.AddScoped<VitalSignService>();

builder.Services.AddScoped<ILabResultRepository, LabResultRepository>();
builder.Services.AddScoped<LabResultService>();

builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<PrescriptionService>();

builder.Services.AddScoped<IDivisionRepository, DivisionRepository>();
builder.Services.AddScoped<DivisionService>();

builder.Services.AddHttpClient("SmsApi");
builder.Services.AddScoped<SmsService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<AuditLogger>();

builder.Services.AddSingleton<LoginRateLimiter>();
builder.Services.AddScoped<LoginThrottleFilter>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    // Applies any pending migrations on startup so a fresh hosted database
    // (e.g. a newly provisioned MSSQL database on your host) gets its schema
    // without needing a separate `dotnet ef database update` run from a machine
    // that can reach it. Safe to run every boot: EF Core tracks applied
    // migrations and this is a no-op once the schema is current.
    //
    // Deliberately guarded: on a misconfigured host we must not kill the process
    // (crash loop / app-pool recycle). Log loudly and keep serving; requests
    // surface real failures as 500 ProblemDetails once the DB is reachable.
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        await RoleSeeder.SeedAsync(scope.ServiceProvider);
        await DivisionSeeder.SeedAsync(scope.ServiceProvider);
        await AdminAccountSeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);
    }
    catch (Exception ex)
    {
        app.Logger.LogCritical(ex, "Database migration/seeding failed during startup. The API will boot anyway; requests hitting the database will fail until this is resolved.");
    }
}

// Configure the HTTP request pipeline. This is now a pure Web API host — the
// Blazor WASM SPA lives in its own deployable and is not served from here.
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    // Explicit, deterministic handler: never depends on service registration
    // (the parameterless UseExceptionHandler() throws at startup in .NET 10
    // without AddProblemDetails). Keeps a misconfigured host from crash-looping.
    app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 500,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "An unexpected error occurred."
        });
    }));
    app.UseHsts();
}

// Must call UseCors BEFORE UseHttpsRedirection, UseAuthentication, and UseAuthorization
// so preflight OPTIONS requests and HTTP->HTTPS redirects include CORS headers.
app.UseCors("MedConnectSpa");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ReferralHub>("/hubs/referrals");
app.MapHub<ChatHub>("/hubs/chats");

app.Run();