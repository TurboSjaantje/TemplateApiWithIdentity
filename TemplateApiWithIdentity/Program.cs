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

// Configure EF Core
builder.Services.AddDbContext<SecurityDbContext>(options =>
    // options.UseSqlite("Data Source=security.db"));
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Token service
builder.Services.AddScoped<TokenService>();

// JWT settings
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];

// JWT Authentication only
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

// IdentityCore without cookie auth, with roles
builder.Services.AddIdentityCore<IdentityUser>(options => { options.User.RequireUniqueEmail = true; })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<SecurityDbContext>()
    .AddDefaultTokenProviders();

// Authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy
        .RequireAuthenticatedUser()
        .RequireRole("Admin"))
    .AddPolicy("User", policy => policy
        .RequireAuthenticatedUser()
        .RequireRole("User", "Admin"));

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed roles and admin user
await app.Services.SeedRolesAndAdminAsync();

// Middleware order matters
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();

app.Run();