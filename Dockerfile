FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ كل الملفات الموجودة في GitHub للسيرفر
COPY . .

# تنفيذ الـ Restore (حيفتش على أي ملف .csproj براهو)
RUN dotnet restore

# بناء المشروع
RUN dotnet publish -c Release -o /app/publish

# مرحلة التشغيل النهائية
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# السطر ده هو "المحرك" - تأكد إن الاسم يطابق ملف الـ csproj
ENTRYPOINT ["dotnet", "Alshifa_clinic_galgany.dll"]
