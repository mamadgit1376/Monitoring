using Pack.Common.Bale;
using Pack.Common.Connecting;
using Monitoring.Support.Services;
using Monitoring_Support_Server.MiddleWare;
using Monitoring_Support_Server.Models.DatabaseModels.Users;
using Monitoring_Support_Server.Models.ViewModels;
using Monitoring_Support_Server.Services;
using Monitoring_Support_Server.Services.Interfaces;
using Monitoring_Support_Server.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddDbContext<MonitoringDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<TblUser, TblRole>()
    .AddEntityFrameworkStores<MonitoringDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var jwtSecretKey = Encoding.UTF8.GetBytes(jwtSettings.Secret);
// Configure Identity options if needed
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 15;
    options.User.RequireUniqueEmail = false;
});

// Configure JWT Authentication

// In Program.cs, replace your current JWT configuration with this:
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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "defaultIssuer",
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "defaultAudience",
        IssuerSigningKey = new SymmetricSecurityKey(jwtSecretKey)
    };

    // This helps with debugging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HeadAdmin", policy =>
            policy.RequireRole("HeadAdmin"));
    options.AddPolicy("Admin", policy =>
            policy.RequireRole("Admin"));
    options.AddPolicy("User", policy =>
            policy.RequireRole("User"));
});

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Optional, but safe to define
    options.AccessDeniedPath = "/Account/AccessDenied";

    // These lines are CRUCIAL for APIs:
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401; // 🔥 Return 401 instead of redirecting
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = 403; // 🔥 Return 403 instead of redirecting
        return Task.CompletedTask;
    };
});
// Configure Swagger more explicitly
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Monitoring Support API",
        Version = "v1",
        Description = "API for Monitoring Support System"
    });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    c.EnableAnnotations(); // Enable Swagger annotations
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    // افزودن کامنت‌های XML به Swagger
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSiteRequests", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .WithMethods("GET", "POST")
               .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var sinkOptions = new MSSqlServerSinkOptions
{
    TableName = "TblLogs",
    AutoCreateSqlTable = true
};

Log.Logger = new LoggerConfiguration()
    .WriteTo.MSSqlServer(
        connectionString,
        sinkOptions: sinkOptions,
        columnOptions: new ColumnOptions())
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();
builder.Services.AddHttpClient("MainService", client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});


// Register JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<MessagingToken>(builder.Configuration.GetSection("MessagingToken"));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("SecuritySettings"));

builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IMonitorService, MonitorService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRestJsonService, RestJsonService>();
builder.Services.AddScoped<IMessagingService, MessagingService>();
builder.Services.AddScoped<IBaleService, BaleService>();
builder.Services.AddScoped<IBaleWebService, BaleWebService>();
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<BackGroundJobService>();
}
var app = builder.Build();
app.UseDeveloperExceptionPage(); // Enables detailed error logs

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("AllowSiteRequests");

// Enable middleware to serve generated Swagger as a JSON endpoint
app.UseSwagger();

// Enable middleware to serve Swagger UI (HTML interface)
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");

    // Allow access without requiring JWT token
    c.RoutePrefix = string.Empty;
});
app.UseAuthentication(); // Ensure this is before UseAuthorization
app.UseAuthorization();

app.UseMiddleware<BodyHashValidationMiddleware>();

app.MapControllers();
app.Run();
