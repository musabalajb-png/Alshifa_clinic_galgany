using Microsoft.EntityFrameworkCore;
using Alshifa_clinic_galgany.Data;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. تحميل الإعدادات من ملفات متعددة
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// 2. إضافة الخدمات الأساسية
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

// 3. إعداد Swagger مع التوثيق العربي
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
    
    // تضمين تعليقات XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// 4. إعداد قاعدة البيانات
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

// 5. إعداد الـ CORS
var corsSettings = builder.Configuration.GetSection("Cors");
var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? 
    new[] { "https://alshifa-clinic-galgany.vercel.app", "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    // سياسة للواجهة الأمامية
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition", "X-Total-Count");
    });
    
    // سياسة للسماح للجميع (لتطوير)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 6. إضافة Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: connectionString,
        name: "SQL Server",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "database", "sql", "ready" })
    .AddDbContextCheck<ClinicDbContext>(
        name: "Entity Framework",
        tags: new[] { "ef", "orm" });

// 7. إضافة خدمات الجلسات والتخزين المؤقت
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// 8. إضافة خدمات التسجيل
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
});

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
        c.DefaultModelsExpandDepth(-1);
        
        // تخصيص واجهة Swagger العربية
        c.InjectStylesheet("/swagger/custom.css");
    });
    
    // استخدام CORS واسع في التطوير
    app.UseCors("AllowAll");
}
else
{
    // في الإنتاج، استخدم سياسة محددة
    app.UseCors("FrontendPolicy");
    
    // تفعيل Swagger فقط إذا كان ممكناً في الإعدادات
    var enableSwagger = builder.Configuration.GetValue<bool>("AppSettings:EnableSwagger", false);
    if (enableSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Alshifa Clinic API v1");
            c.RoutePrefix = "api";
        });
    }
}

// 2. تفعيل ملفات الواجهة (HTML, CSS, JS)
app.UseDefaultFiles();
app.UseStaticFiles();

// 3. Health Checks
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
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTime.UtcNow
        });
        await context.Response.WriteAsync(result);
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

// 4. نقاط نهاية لاختبار قاعدة البيانات
app.MapGet("/test-db", async (IConfiguration configuration) =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    await DatabaseTester.TestConnection(connectionString);
    return Results.Ok(new { 
        message = "تم اختبار الاتصال بنجاح", 
        timestamp = DateTime.Now,
        server = "AlshifaDb.mssql.somee.com"
    });
});

app.MapGet("/db-info", async (ClinicDbContext dbContext) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (!canConnect)
        {
            return Results.Problem("فشل الاتصال بقاعدة البيانات", statusCode: 503);
        }

        var patients = await dbContext.Patients.CountAsync();
        var medications = await dbContext.Medications.CountAsync();
        var users = await dbContext.Users.CountAsync();
        var visits = await dbContext.Visits.CountAsync();
        var labTests = await dbContext.LabTests.CountAsync();
        
        return Results.Ok(new
        {
            success = true,
            message = "قاعدة البيانات تعمل بنجاح",
            data = new
            {
                patients,
                medications,
                users,
                visits,
                labTests,
                server = "AlshifaDb.mssql.somee.com",
                database = "AlshifaDb",
                provider = "SQL Server",
                timestamp = DateTime.Now
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: $"خطأ في الاتصال بقاعدة البيانات: {ex.Message}",
            statusCode: 500,
            title: "Database Error");
    }
});

// 5. نقاط نهاية إضافية
app.MapGet("/", () => Results.Redirect("/index.html"));
app.MapGet("/api", () => Results.Ok(new { 
    message = "مرحباً بك في API مجمع الشفاء الطبي",
    version = "v1.0.0",
    endpoints = new[] { "/health", "/db-info", "/test-db", "/api-docs" }
}));

// 6. تفعيل الـ HTTPS والترخيص
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseSession();

// 7. تعيين الـ Controllers
app.MapControllers();

// 8. صفحة افتراضية للخطأ 404
app.MapFallbackToFile("index.html");

// 9. التحقق من اتصال قاعدة البيانات عند بدء التشغيل
try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("🔍 جاري التحقق من اتصال قاعدة البيانات...");
        
        var canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            logger.LogInformation("✅ قاعدة البيانات متصلة بنجاح!");
            
            // تنفيذ هجرة قاعدة البيانات
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
            logger.LogInformation($"   🌐 السيرفر: AlshifaDb.mssql.somee.com");
        }
        else
        {
            logger.LogError("❌ فشل الاتصال بقاعدة البيانات!");
            logger.LogWarning("⚠️ النظام سيعمل في الوضع المحلي فقط");
        }
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
    logger.LogInformation($"🌐 الواجهة الأمامية: https://alshifa-clinic-galgany.vercel.app");
    logger.LogInformation($"🔧 API: https://alshifa-clinic-galgany.vercel.app/api-docs");
    logger.LogInformation($"🏥 Health Check: https://alshifa-clinic-galgany.vercel.app/health");
    logger.LogInformation($"📊 Database Info: https://alshifa-clinic-galgany.vercel.app/db-info");
});

// 11. تحديد المنفذ
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
