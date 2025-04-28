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

// Validate required environment variables
var requiredEnvVars = new[] 
{ 
    "JWT_SECRET",
    "JWT_ISSUER",
    "JWT_AUDIENCE"
};

foreach (var envVar in requiredEnvVars)
{
    var value = Environment.GetEnvironmentVariable(envVar);
    if (string.IsNullOrEmpty(value))
    {
        throw new InvalidOperationException($"Required environment variable {envVar} is not set");
    }
    Log.Information("Environment variable {EnvVar} is configured", envVar);
}


//services setup .
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
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
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
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
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
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

Log.Information("Configuring web server to listen on port {Port}", port);

// build the app
var app = builder.Build();

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
//             Port = Environment.GetEnvironmentVariable("PORT") ?? "8080",
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

