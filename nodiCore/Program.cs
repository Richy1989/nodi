using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nodiCore.Data;
using nodiCore.Services;

var builder = WebApplication.CreateBuilder(args);

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
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("SQLite") ?? "Data Source=nodi.db"));
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
// EnsureCreated creates the schema on first run but does not run migrations.
// The ALTER TABLE below is a manual migration to add the Theme column to
// existing SQLite databases that were created before this column was introduced.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // Gracefully add new columns to existing SQLite databases (EnsureCreated won't migrate)
    try { db.Database.ExecuteSqlRaw("ALTER TABLE Users ADD COLUMN Theme TEXT NOT NULL DEFAULT 'Dark'"); }
    catch { /* column already exists */ }
    await DbSeeder.SeedAsync(db, builder.Configuration);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
