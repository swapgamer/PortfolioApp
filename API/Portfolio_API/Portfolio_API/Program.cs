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

// TODO: replace the localhost origin with the real production domain once deployed (Docs/02-High-Level-Design.md section 5).
builder.Services.AddCors(o => o.AddPolicy("portfolio", p =>
    p.WithOrigins("http://localhost:4200")
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
