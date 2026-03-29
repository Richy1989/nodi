using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nodiCore.Data;
using nodiCore.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

// ── Configuration service ─────────────────────────────────────────────────────
// Instantiated early so Program.cs can use DataFolder before the DI container
// is built. The same instance is registered as the singleton below.
var appConfig = new ConfigurationService(builder.Configuration);
Directory.CreateDirectory(appConfig.DataFolder);

// ── Database ──────────────────────────────────────────────────────────────────
// Supports SQLite (default, for local/dev) or PostgreSQL (for production).
// Set Database:Provider = "postgresql" in appsettings to switch providers.
var dbProvider = builder.Configuration.GetValue<string>("Database:Provider") ?? "sqlite";
if (dbProvider.Equals("postgresql", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));
}
else
{
    // Place the SQLite file inside the data folder instead of the working directory.
    var sqlitePath = Path.Combine(appConfig.DataFolder, "nodi.db");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={sqlitePath}"));
}

// ── JWT Authentication ────────────────────────────────────────────────────────
// Requires Jwt:Key in appsettings.json. Throws at startup if missing to avoid
// silent auth failures in production.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured in appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "nodiCore",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "nodiClients",
            ValidateLifetime = true,
            // Zero clock skew means tokens expire exactly at the configured time,
            // preventing a grace window that could allow expired tokens.
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Serialize enums as strings in JSON responses (e.g. "Text" instead of 0)
// so clients don't need to know the underlying integer values.
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

// ── Services (scoped per HTTP request) ───────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<SettingsService>();
builder.Services.AddScoped<UserSettingsService>();

// ── Configuration singleton ───────────────────────────────────────────────────
// Register the instance created above so injected services get the same object.
builder.Services.AddSingleton(appConfig);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(appConfig.DataFolder, "keys")));


// ── CORS ──────────────────────────────────────────────────────────────────────
// Open policy allows nodiWeb and nodiApp to call the API from any origin.
// Tighten this in production if the deployment origin is known.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ── Database initialisation ───────────────────────────────────────────────────
// EnsureCreated creates the schema on first run if the database doesn't exist yet.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    await DbSeeder.SeedAsync(db, builder.Configuration);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
