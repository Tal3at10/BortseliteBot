using CustomsBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use PORT from environment (for Railway/Cloud hosting)
var port = Environment.GetEnvironmentVariable("PORT") ?? "5085";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// 1. إضافة دعم الـ Controllers (هذا السطر كان ناقصاً)
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddOpenApi();

// تسجيل الـ Services
builder.Services.AddSingleton<SessionManager>();
builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddHttpClient<WhatsAppService>();
builder.Services.AddHttpClient<TelegramService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// تعطيل HTTPS redirect في Production (Railway بيوفر HTTPS تلقائياً)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 2. تشغيل الـ Controllers لكي يرى ملف الواتساب (هذا السطر كان ناقصاً)
app.MapControllers();

app.Run();