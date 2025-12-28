using Microsoft.EntityFrameworkCore;
using Alshifa_clinic_galgany.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. إضافة الـ CORS للسماح بالاتصال من المتصفحات والموبايل
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", b => b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 2. إعداد قاعدة البيانات
builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. تفعيل الـ Swagger حتى في وضع الـ Production عشان تقدر تجرب الـ API أونلاين
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alshifa API v1"));

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
