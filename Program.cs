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
using System.Text;
using SocialMediaAPI.Services;
using SocialMediaAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);


DotNetEnv.Env.Load();

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;


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
    
// Configure configuration sources
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

foreach (var envVar in requiredEnvVars)
{
    // Check JWT section first
    var configValue = Environment.GetEnvironmentVariable(envVar);

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

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IFollowService, FollowService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<FeedScoreCalculator>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();

//Redis cache configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Environment.GetEnvironmentVariable("REDIS");
    options.InstanceName = "SocialMediaAPI:";
});


// Add Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Social Media API", 
        Version = "v1",
        Description = "A Social Media API built with ASP.NET Core",
        Contact = new OpenApiContact
        {
            Name = "galaxia",
            Email = "omopekunmichael@gmail.com",
            Url = new Uri("https://socialmediaapi-production-74e1.up.railway.app")
        }
    });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.AddServer(new OpenApiServer
    {
        Url = "https://socialmediaapi-production-74e1.up.railway.app"
    });
});


// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});


// Configure DbContext
var connectionString = $"Server={Environment.GetEnvironmentVariable("POSTGRES_HOST")};" +
                      $"Port={Environment.GetEnvironmentVariable("POSTGRES_PORT")};" +
                      $"Database={Environment.GetEnvironmentVariable("POSTGRES_DB")};" +
                      $"Username={Environment.GetEnvironmentVariable("POSTGRES_USER")};" +
                      $"Password={Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")};" +
                      "SSL Mode=Require;Trust Server Certificate=true";
                      
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

        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");

        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        if (string.IsNullOrEmpty(jwtSecret))
        {
            Log.Error("JWT_SECRET environment variable is not set");
            throw new InvalidOperationException("JWT_SECRET environment variable is not set");
        }

        Log.Information("Configuring JWT authentication");

         options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Error(context.Exception, "JWT validation failed");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("JWT token validated successfully for: {Name}", context.Principal.Identity?.Name);
                return Task.CompletedTask;
            }
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

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Social Media API v1");
        c.RoutePrefix = "swagger";
    });

app.UseCors("AllowAll");
app.UseHsts();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.Run();

