using Microsoft.EntityFrameworkCore;
using Alshifa_clinic_galgany.Data;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. إضافة الخدمات الأساسية
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// 2. إعداد Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "مجمع الشفاء الطبي - API", 
        Version = "v1",
        Description = "واجهة برمجة التطبيقات لنظام إدارة العيادة المتكامل",
        Contact = new OpenApiContact
        {
            Name = "دعم فني",
            Email = "support@alshifaclinic.com"
        }
    });
});

// 3. إعداد قاعدة البيانات
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddDbContext<ClinicDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
        .CommandTimeout(60)));

// 4. إعداد الـ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
    
    // سياسة محددة للإنتاج
    options.AddPolicy("ProductionCors", builder =>
    {
        builder.WithOrigins(
            "https://alshifa-clinic-galgany.vercel.app",
            "https://www.alshifaclinic.com",
            "http://localhost:3000",
            "http://localhost:8080"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// 5. إضافة Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "SQL Server");

var app = builder.Build();

// --- التكوين ---

// 1. بيئة التطوير
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alshifa Clinic API v1");
        c.RoutePrefix = "api-docs";
        c.DocumentTitle = "وثائق API - مجمع الشفاء";
    });
    
    app.UseCors("AllowAll");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseCors("ProductionCors");
}

// 2. تفعيل ملفات الواجهة من مجلد wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// 3. تفعيل Swagger في الإنتاج إذا كان مطلوباً
if (bool.Parse(builder.Configuration["AppSettings:EnableSwagger"] ?? "false"))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alshifa Clinic API v1");
        c.RoutePrefix = "api";
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// 4. Health Check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});

// 5. اختبار قاعدة البيانات
app.MapGet("/test-db", async (ClinicDbContext dbContext) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        return Results.Ok(new 
        { 
            success = canConnect, 
            message = canConnect ? "✅ قاعدة البيانات متصلة بنجاح" : "❌ فشل الاتصال بقاعدة البيانات",
            server = "AlshifaDb.mssql.somee.com",
            timestamp = DateTime.Now
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"❌ خطأ في الاتصال: {ex.Message}");
    }
});

// 6. معلومات النظام
app.MapGet("/api/info", () =>
{
    return Results.Ok(new
    {
        name = "مجمع الشفاء الطبي",
        version = "3.0.0",
        environment = app.Environment.EnvironmentName,
        database = "AlshifaDb.mssql.somee.com",
        timestamp = DateTime.Now
    });
});

// 7. تعيين الـ Controllers
app.MapControllers();

// 8. صفحة افتراضية للخطأ 404
app.MapFallbackToFile("index.html");

// 9. التحقق من اتصال قاعدة البيانات عند بدء التشغيل
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    logger.LogInformation("🔍 جاري التحقق من اتصال قاعدة البيانات...");
    
    var canConnect = await dbContext.Database.CanConnectAsync();
    if (canConnect)
    {
        logger.LogInformation("✅ قاعدة البيانات متصلة بنجاح!");
        
        // إنشاء الجداول إذا لم تكن موجودة
        await dbContext.Database.EnsureCreatedAsync();
        logger.LogInformation("✅ تم التأكد من إنشاء الجداول");
        
        // عرض بعض الإحصائيات
        var patientsCount = await dbContext.Patients.CountAsync();
        var medicationsCount = await dbContext.Medications.CountAsync();
        var usersCount = await dbContext.Users.CountAsync();
        
        logger.LogInformation("📊 إحصائيات قاعدة البيانات:");
        logger.LogInformation($"   👥 المرضى: {patientsCount}");
        logger.LogInformation($"   💊 الأدوية: {medicationsCount}");
        logger.LogInformation($"   👤 المستخدمين: {usersCount}");
    }
    else
    {
        logger.LogError("❌ فشل الاتصال بقاعدة البيانات!");
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "❌ حدث خطأ أثناء التحقق من قاعدة البيانات");
}

// 10. معلومات بدء التشغيل
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("🚀 تم بدء تشغيل نظام مجمع الشفاء بنجاح!");
    logger.LogInformation("📡 قاعدة البيانات: AlshifaDb.mssql.somee.com");
    logger.LogInformation("🌐 الواجهة الأمامية: https://alshifa-clinic-galgany.vercel.app");
    logger.LogInformation("🔧 API: https://alshifa-clinic-galgany.vercel.app/api");
    logger.LogInformation("🏥 Health Check: https://alshifa-clinic-galgany.vercel.app/health");
});

// 11. تحديد المنفذ
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
