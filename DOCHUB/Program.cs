using Microsoft.EntityFrameworkCore;
using DOCHUB.APP.Data;
using DOCHUB.APP.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DOCHUB.APP.JWT;
using Microsoft.OpenApi.Models;
using Amazon;
using DOCHUB.Infrastructure.Storage;

System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
System.Net.ServicePointManager.SecurityProtocol =
    System.Net.SecurityProtocolType.Tls12 |
    System.Net.SecurityProtocolType.Tls13;
System.Net.ServicePointManager.CheckCertificateRevocationList = false;
System.Net.ServicePointManager.Expect100Continue = false;
System.Net.ServicePointManager.UseNagleAlgorithm = false;
System.Net.ServicePointManager.DefaultConnectionLimit = 100;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "DOCHUB API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("postgresql")).EnableSensitiveDataLogging());
builder.Services.AddControllers();

// Registrar servicios JWT
builder.Services.AddScoped<AccessToken>();
builder.Services.AddScoped<RefreshToken>();

// Registrar repositories
builder.Services.AddScoped<UsuariosRepository>();
builder.Services.AddScoped<JWTRepository>();
builder.Services.AddScoped<DocumentosRepository>();
builder.Services.AddScoped<InformacionPerfilRepository>();

//  CAMBIO: Agregar HttpClient y usar el servicio simple
builder.Services.AddHttpClient();
builder.Services.AddScoped<CloudflareR2MinioService>();

// Configuración JWT
var secretKey = builder.Configuration["JWT:SecretKey"];
var issuer = builder.Configuration["JWT:Issuer"];
var audience = builder.Configuration["JWT:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    var allowed = new[] { "https://app.dochub.stream", "https://api.dochub.stream" };

    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
      policy.WithOrigins(allowed)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});




builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();       // 1️⃣ primero redirección segura
app.UseCookiePolicy();           // 2️⃣ luego política de cookies
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();         // 4️⃣ JWT
app.UseAuthorization();          // 5️⃣ autorización
app.MapControllers();

app.Run();
