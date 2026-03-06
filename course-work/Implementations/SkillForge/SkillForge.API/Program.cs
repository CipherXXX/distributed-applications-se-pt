using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkillForge.API.Middleware;
using SkillForge.Application.Interfaces;
using SkillForge.Application.Services;
using SkillForge.Application.Validators;
using SkillForge.Infrastructure;
using SkillForge.Infrastructure.Data;
using SkillForge.Infrastructure.Services;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SkillForge API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header. Example: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
    o.ApiVersionReader = new UrlSegmentApiVersionReader();
});
builder.Services.AddVersionedApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});

builder.Services.Configure<SkillForge.Infrastructure.Services.JwtSettings>(o =>
{
    o.Secret = builder.Configuration["Jwt:Secret"] ?? "SkillForgeSuperSecretKeyThatIsAtLeast32CharactersLong!";
    o.Issuer = builder.Configuration["Jwt:Issuer"] ?? "SkillForge";
    o.Audience = builder.Configuration["Jwt:Audience"] ?? "SkillForge";
    o.ExpirationMinutes = int.TryParse(builder.Configuration["Jwt:ExpirationMinutes"], out var min) ? min : 60;
});
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "SkillForgeSuperSecretKeyThatIsAtLeast32CharactersLong!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SkillForge",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SkillForge",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=SkillForgeDb;Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddInfrastructure(connectionString);

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath ?? Directory.GetCurrentDirectory(), "uploads");
builder.Services.Configure<FileStorageOptions>(o => o.BasePath = uploadsPath);

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddAutoMapper(typeof(SkillForge.Application.Mapping.MappingProfile).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentDtoValidator>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseIpRateLimiting();
app.UseHttpsRedirection();

// Uploads directory (files served only via authorized GET /api/v1/files/download)
Directory.CreateDirectory(uploadsPath);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"SkillForge API {description.GroupName.ToUpperInvariant()}");
        }
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SkillForge.Infrastructure.Data.SkillForgeDbContext>();
    await SkillForge.Infrastructure.Data.DbSeeder.SeedAsync(db);
}

app.Run();
