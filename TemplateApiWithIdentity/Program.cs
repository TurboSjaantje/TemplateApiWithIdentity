using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TemplateApiWithIdentity.Authentication;
using TemplateApiWithIdentity.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddDbContext<SecurityDbContext>(options => options.UseSqlite("Data Source=security.db"));
builder.Services.AddScoped<TokenService>();

// Configure Identity
var jwtKey = builder.Configuration["Jwt:Key"] ?? "super_secret_dev_key_123!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TemplateApi";

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
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<SecurityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy
        .RequireAuthenticatedUser()
        .RequireRole("Admin"))
    .AddPolicy("User", policy => policy
        .RequireAuthenticatedUser()
        .RequireRole("User"));

// Configure Endpoints
builder.Services.AddControllers();

var app = builder.Build();

// Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Configure Identity
await app.Services.SeedRolesAndAdminAsync();

// Configure Endpoints
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.Run();