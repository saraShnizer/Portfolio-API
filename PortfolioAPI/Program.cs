using GitHubService.Settings;
using Microsoft.Extensions.Caching.Memory; // הוספת תמיכה ב-IMemoryCache

var builder = WebApplication.CreateBuilder(args);

// טעינת ה-Token מה-User Secrets
builder.Configuration.AddUserSecrets<Program>();
var token = builder.Configuration["GitHub:Token"];

if (string.IsNullOrEmpty(token))
{
    throw new InvalidOperationException("GitHub token is missing or empty.");
}

// הוספת שירותי ה-Cache ל-DI
builder.Services.AddMemoryCache();

// הוספת שירותים ל-Container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// קונפיגורציה ל-GitHub Settings
builder.Services.Configure<GitHubSettings>(builder.Configuration.GetSection("GitHub"));

// רישום ה-Client Service כ-Singleton
builder.Services.AddSingleton<GitHubClientService>();

var app = builder.Build();

// קונפיגורציה של ה-Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
