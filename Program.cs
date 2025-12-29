using Microsoft.EntityFrameworkCore;
using Alshifa_clinic_galgany.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. إضافة الخدمات الأساسية
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. إعداد Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Alshifa Clinic API", Version = "v1" });
});

// 3. إعداد قاعدة البيانات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(connectionString));

// 4. إعداد الـ CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", b => b
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

// --- التعديل الجوهري هنا يا مصعب ---

// 5. تفعيل ملفات الواجهة (HTML, CSS, JS)
app.UseDefaultFiles(); // ضروري عشان يفتح index.html تلقائياً
app.UseStaticFiles(); // ضروري عشان يقرأ الملفات من wwwroot

// 6. تفعيل Swagger (حنخليه في رابط منفصل عشان الواجهة تشتغل)
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alshifa Clinic API v1");
    // شلنا RoutePrefix عشان الموقع يفتح الواجهة الأساسية أولاً
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 7. تحديد المنفذ لـ Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
