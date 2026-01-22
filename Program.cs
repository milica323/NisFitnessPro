using StackExchange.Redis;
using NisFitnessPro.Services;

var builder = WebApplication.CreateBuilder(args);

// Dodaj CORS
builder.Services.AddCors(options => options.AddPolicy("AllowAll", 
    p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Konfiguracija Redisa
var redisConnectionString = "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddScoped<RedisService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();

// --- OVE DVE LINIJE DODAJ OVDE ---
app.UseDefaultFiles(); // Omogućava da index.html bude početna strana
app.UseStaticFiles();  // Omogućava serviranje CSS i JS fajlova iz wwwroot
// --------------------------------

app.MapControllers();

app.Run();