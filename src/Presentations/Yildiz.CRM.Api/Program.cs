using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text;
using Yildiz.CRM.Applications.Interfaces;
using Yildz.CRM.Infrastructures;
using Yildz.CRM.Infrastructures.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add OpenAPI with enhanced configuration for Scalar and JWT
builder.Services.AddOpenApi("v1", options =>
{
    // Configure document-level settings
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Yildiz CRM API",
            Version = "v1",
            Description = "A modern CRM API built with .NET 10, CQRS, and MediatR. Provides customer management and JWT-based authentication."
        };

        // Add security scheme for JWT Bearer tokens
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme. Enter your token in the text input below.",
                In = ParameterLocation.Header,
                Name = "Authorization"
            }
        };

        return Task.CompletedTask;
    });

    // Configure operation-level settings - automatically add Bearer auth to protected endpoints
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        // Check if the endpoint has [Authorize] attribute
        bool hasAuthorize = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAuthorizeData>()
            .Any();

        if (!hasAuthorize)
        {
            return Task.CompletedTask;
        }

        // Add Bearer security requirement to this operation
        operation.Security ??= new List<OpenApiSecurityRequirement>();

        var securityRequirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
        };

        operation.Security.Add(securityRequirement);
        return Task.CompletedTask;
    });
});

// Database
builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register ICrmDbContext
builder.Services.AddScoped<ICrmDbContext>(provider => provider.GetRequiredService<CrmDbContext>());

// Register Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IIdentityContext, IdentityContextService>();

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Yildiz.CRM.Applications.Interfaces.ICrmDbContext).Assembly);
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Apply migrations and seed data (only if not in test environment)
if (!app.Environment.IsEnvironment("Test"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the implicit Program class public for integration tests and Scalar
public partial class Program { }
