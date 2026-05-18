using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// تفعيل CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// إعداد Kestrel للاستماع على المنفذ الصحيح
builder.WebHost.ConfigureKestrel(options =>
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

app.UseCors("AllowAll");

// صفحة رئيسية
app.MapGet("/", () => Results.Ok(new 
{ 
    message = "PikaTV Server is Running!",
    version = "1.0",
    status = "online",
    endpoints = new[] 
    {
        "/api/activation/verify/{code}"
    }
}));

// صفحة صحة
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// قاعدة بيانات الأكواد
var userSubscriptions = new Dictionary<string, string>
{
    { "PIKA-112233", "http://server.com:8080/live/user1/pass1/1.m3u8" },
    { "PIKA-998877", "http://server.com:8080/live/user2/pass2/2.m3u8" }
};

// API للتحقق من الأكواد
app.MapGet("/api/activation/verify/{code}", (string code) =>
{
    if (string.IsNullOrWhiteSpace(code))
    {
        return Results.BadRequest(new { success = false, message = "الكود فارغ" });
    }

    if (userSubscriptions.TryGetValue(code.Trim().ToUpper(), out var url))
    {
        return Results.Ok(new 
        { 
            success = true, 
            url = url, 
            message = "Welcome to PikaTV!",
            expires = "2026-12-31"
        });
    }

    return Results.Json(new { success = false, message = "كود التفعيل غير صحيح" }, statusCode: 401);
});

app.Run();
