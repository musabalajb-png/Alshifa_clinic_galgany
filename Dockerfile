FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ كل ملفات المشروع
COPY . .

# تنفيذ الـ Restore والـ Publish
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# مرحلة التشغيل
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# تشغيل الـ DLL النهائي
ENTRYPOINT ["dotnet", "Alshifa_clinic_galgany.dll"]
