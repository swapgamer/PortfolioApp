using System.Net.Http.Headers;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Portfolio_API.Data;
using Portfolio_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<IRagService, RagService>();

// OpenAI API key: set via `dotnet user-secrets set "OpenAI:ApiKey" "sk-..."` locally,
// environment variable/Key Vault in production -- never committed to appsettings.json.
builder.Services.AddHttpClient<ILlmClient, OpenAiClient>((sp, client) =>
{
    var apiKey = sp.GetRequiredService<IConfiguration>()["OpenAI:ApiKey"];
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.Timeout = TimeSpan.FromSeconds(15);
    if (!string.IsNullOrEmpty(apiKey))
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
});

// Configurable per environment instead of hardcoded, so the production domain can be set via
// Azure App Service configuration (Cors__AllowedOrigins__0=https://your-domain) without a
// code change/redeploy. Falls back to the local Angular dev server if unset.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(o => o.AddPolicy("portfolio", p =>
    p.WithOrigins(allowedOrigins)
     .AllowAnyHeader()
     .WithMethods("GET", "POST")));

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("contact", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
    });
    options.AddFixedWindowLimiter("ask-ai", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
    });
});

var app = builder.Build();

// Applies any pending EF Core migrations on startup -- means deployment never needs a
// separate `dotnet ef database update` step/secret in CI; the app brings its own schema
// up to date against whatever ConnectionStrings:DefaultConnection points to. Safe to run
// every startup since migrations are idempotent (only pending ones are applied).
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("portfolio");

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
