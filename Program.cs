using Microsoft.EntityFrameworkCore;
using Alshifa_clinic_galgany.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. إضافة الخدمات الأساسية
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. إعداد Swagger بشكل احترافي
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Alshifa Clinic API", Version = "v1" });
});

// 3. إعداد قاعدة البيانات (تأكد من وجود ConnectionString في appsettings.json)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(connectionString));

// 4. إعداد الـ CORS (ضروري جداً عشان تربط الفرونت-إند بالباك-إند)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", b => b
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

// 5. تفعيل Swagger في كل البيئات (مهم للتجربة على Render)
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alshifa Clinic API v1");
    c.RoutePrefix = string.Empty; // ده بيخلي Swagger يفتح أول ما تدخل رابط الموقع
});

// 6. الترتيب الصحيح للـ Middleware
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// 7. تحديد المنفذ (Port) بشكل آلي عشان Render ما يرفض الاتصال
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
