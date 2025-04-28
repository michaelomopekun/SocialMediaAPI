using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SocialMediaAPI.Data;
using SocialMediaAPI.Models.Domain.User;
using Serilog;
using Serilog.Events;
using SocialMediaAPI.Constants;
using SocialMediaAPI.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Configure configuration sources
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

// Replace the environment variable validation section
var requiredEnvVars = new[] 
{ 
    "JWT_ISSUER",
    "JWT_SECRET",
    "JWT_AUDIENCE"
};

// Initialize Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

foreach (var envVar in requiredEnvVars)
{
    // Check JWT section first
    var configValue = builder.Configuration[$"JWT:{envVar.Replace("JWT_", "")}"] ?? 
                     builder.Configuration[envVar] ?? 
                     Environment.GetEnvironmentVariable(envVar);

    if (!string.IsNullOrEmpty(configValue))
    {
        Log.Information("Found {EnvVar} in configuration", envVar);
        continue;
    }

    var message = $"Required variable {envVar} is not set in either configuration or environment variables";
    Log.Error(message);
    throw new InvalidOperationException(message);
}

//services setup.
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Social Media API", Version = "v1" });
});

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, 
        x => x.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

// Identity setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>( option =>
{
    option.Password.RequireDigit = true;
    option.Password.RequiredLength = 8;
    option.Password.RequireNonAlphanumeric = true;
    option.Password.RequireUppercase = true;
    option.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add these services before AddAuthentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// JWT configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    try 
    {
        var jwtSecret = builder.Configuration["JWT:Secret"] ?? 
                        builder.Configuration["JWT_SECRET"] ?? 
                        Environment.GetEnvironmentVariable("JWT_SECRET");

        var jwtIssuer = builder.Configuration["JWT:ValidIssuer"] ?? 
                        builder.Configuration["JWT_ISSUER"] ?? 
                        Environment.GetEnvironmentVariable("JWT_ISSUER");

        var jwtAudience = builder.Configuration["JWT:ValidAudience"] ?? 
                         builder.Configuration["JWT_AUDIENCE"] ?? 
                         Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        if (string.IsNullOrEmpty(jwtSecret))
        {
            Log.Error("JWT_SECRET environment variable is not set");
            throw new InvalidOperationException("JWT_SECRET environment variable is not set");
        }

        Log.Information("Configuring JWT authentication");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Failed to configure JWT authentication");
        throw;
    }
});

// Serilog setup
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: 1024 * 1024,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
    
builder.Host.UseSerilog();

Log.Information("Starting web application");

// Configure host and port for Railway
var port = builder.Configuration["PORT"] ?? Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

Log.Information("Configuring web server to listen on port {Port}", port);

// build the app
var app = builder.Build();

Log.Information("Web application built successfully");
if (app.Environment.IsDevelopment())
{
    foreach (var config in builder.Configuration.AsEnumerable())
    {
        Log.Debug("Config: {Key} = {Value}", config.Key, config.Key.Contains("SECRET") ? "[REDACTED]" : config.Value);
    }
}

// setup Identity Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { UserRoles.Admin, UserRoles.User };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// app.Use(async (context, next) =>
// {
//     try
//     {
//         await next();
//     }
//     catch (Exception ex)
//     {
//         Log.Error(ex, "An unhandled exception occurred.");
//         throw;
//     }
// });

app.MapControllers();

// Uncomment and update the status endpoint
// app.MapGet("/status", () => 
// {
//     try 
//     {
//         using var scope = app.Services.CreateScope();
//         var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//         var canConnect = context.Database.CanConnect();

//         var status = new
//         {
//             Status = "Running",
//             Timestamp = DateTime.UtcNow,
//             Port = builder.Configuration["PORT"] ?? Environment.GetEnvironmentVariable("PORT") ?? "8080",
//             Database = canConnect ? "Connected" : "Disconnected",
//             Environment = app.Environment.EnvironmentName
//         };
        
//         Log.Information("Status check: {@Status}", status);
//         return Results.Ok(status);
//     }
//     catch (Exception ex)
//     {
//         Log.Error(ex, "Status check failed");
//         return Results.Problem(
//             title: "Status Check Failed",
//             detail: ex.Message,
//             statusCode: 500
//         );
//     }
// });

app.Run();

