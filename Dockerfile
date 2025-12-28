# استخدم نسخة الـ SDK لبناء المشروع
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات المشروع وعمل Restore
COPY . .
RUN dotnet restore "Alshifa_clinic_galgany.csproj"

# بناء المشروع بنسخة الـ Release
RUN dotnet publish "Alshifa_clinic_galgany.csproj" -c Release -o /app/publish

# استخدم نسخة الـ Runtime لتشغيل المشروع
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# تشغيل التطبيق (تأكد من اسم الـ DLL)
ENTRYPOINT ["dotnet", "Alshifa_clinic_galgany.dll"]
