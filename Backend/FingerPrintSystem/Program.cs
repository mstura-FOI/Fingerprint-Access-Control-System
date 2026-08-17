using FingerPrintSystem.Application.Auth.AuthService;
using FingerPrintSystem.Application.Auth.TokenService;
using FingerPrintSystem.Application.Security;
using FingerPrintSystem.Application.Services.AccessCodeService;
using FingerPrintSystem.Application.Services.AccessRightService;
using FingerPrintSystem.Application.Services.AccessSessionService;
using FingerPrintSystem.Application.Services.AccountService;
using FingerPrintSystem.Application.Services.AttendanceService;
using FingerPrintSystem.Application.Services.DeviceService;
using FingerPrintSystem.Application.Services.EnrollmentService;
using FingerPrintSystem.Application.Services.FingerprintScanService;
using FingerPrintSystem.Application.Services.RoomService;
using FingerPrintSystem.Application.Services.RoomShiftService;
using FingerPrintSystem.Application.Services.ShiftService;
using FingerPrintSystem.Core.Entities.Identity;
using FingerPrintSystem.Core.Options;
using FingerPrintSystem.Infrastructure.EFCore;
using FingerPrintSystem.Infrastructure.Security.ITemplateCipher;
using FingerPrintSystem.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Unesi SAMO access token (bez 'Bearer ' prefiksa)."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

// OPTIONS 
builder.Services.Configure<FingerPrintSystemOptions>(
    builder.Configuration.GetSection(FingerPrintSystemOptions.SectionName));
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
builder.Services.Configure<JwtOptions>(jwtSection);
var jwt = jwtSection.Get<JwtOptions>()!;
builder.Services.Configure<EnvelopeCipherOptions>(builder.Configuration.GetSection(EnvelopeCipherOptions.SectionName));
builder.Services.Configure<AccessCodeOptions>(builder.Configuration.GetSection(AccessCodeOptions.SectionName));
builder.Services.Configure<TotpProtectorOptions>(
    builder.Configuration.GetSection(TotpProtectorOptions.SectionName));

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; 
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30) 
        };
        options.Events = new JwtBearerEvents {
            OnAuthenticationFailed = ctx =>
            {
                return Task.CompletedTask;
            }
        };
    });

const string DevicePolicy = "device-code-attempts";

builder.Services.AddRateLimiter(options => {
    options.AddPolicy(DevicePolicy, httpContext => {
        var thumbprint = httpContext.Connection.ClientCertificate?.Thumbprint
                         ?? "no-cert";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: thumbprint,
            factory: _ => new FixedWindowRateLimiterOptions {
                PermitLimit = 7,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });
    });

    options.AddPolicy("totp-attempts", httpContext => {
        var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: key,
            factory: _ => new FixedWindowRateLimiterOptions {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
            });
    });

    options.OnRejected = async (context, ct) => {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Previše pokušaja. Pokušaj kasnije.", ct);
    };
});

builder.WebHost.ConfigureKestrel(k => {
    k.ListenAnyIP(7137, listen => {
        listen.UseHttps(https => {
            https.ServerCertificate = X509CertificateLoader.LoadPkcs12FromFile(
                Path.Combine(AppContext.BaseDirectory, "server.pfx"), "test");
            https.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
            https.ClientCertificateValidation = (cert, chain, errors) => true;
        });
    });
});

builder.Services.AddCors(options => {
    options.AddPolicy("Frontend", policy => {
        policy
            .WithOrigins(
                "http://localhost:8081",
                "http://localhost:8082"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

//SERVICES 
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<ITotpSecretProtector, AesTotpSecretProtector>();
builder.Services.AddScoped<ITotpService, TotpService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<ShiftService>();
builder.Services.AddScoped<RoomShiftService>();
builder.Services.AddScoped<AccessRightService>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<FingerprintScanService>();
builder.Services.AddScoped<ITemplateCipher, EnvelopeTemplateCipher>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IAccessCodeService, AccessCodeService>();
builder.Services.AddScoped<IAccessService, AccessService>();

var app = builder.Build();

// migracije + seed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await services.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    await IdentitySeeder.SeedAsync(services);
    await ConfigurationSeeder.SeedAsync(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseMiddleware<DeviceCertificateMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();