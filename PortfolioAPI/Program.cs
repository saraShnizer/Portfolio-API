using GitHubService.Settings;
using Microsoft.Extensions.Caching.Memory; // ����� ����� �-IMemoryCache

var builder = WebApplication.CreateBuilder(args);

// ����� �-Token ��-User Secrets
builder.Configuration.AddUserSecrets<Program>();
var token = builder.Configuration["GitHub:Token"];

if (string.IsNullOrEmpty(token))
{
    throw new InvalidOperationException("GitHub token is missing or empty.");
}

// ����� ������ �-Cache �-DI
builder.Services.AddMemoryCache();

// ����� ������� �-Container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ����������� �-GitHub Settings
builder.Services.Configure<GitHubSettings>(builder.Configuration.GetSection("GitHub"));

// ����� �-Client Service �-Singleton
builder.Services.AddSingleton<GitHubClientService>();

var app = builder.Build();

// ����������� �� �-Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
