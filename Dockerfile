FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ كل الملفات
COPY . .

# تنفيذ الـ Restore
RUN dotnet restore

# بناء ونشر المشروع
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# تشغيل المشروع (تأكد إن الاسم يطابق ملف الـ csproj)
ENTRYPOINT ["dotnet", "Alshifa_clinic_galgany.dll"]
