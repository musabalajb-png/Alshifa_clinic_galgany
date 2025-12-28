FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ كل الملفات
COPY . .

# تنفيذ الـ Restore لكل المشاريع الموجودة
RUN dotnet restore

# بناء ونشر المشروع
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# تأكد إن اسم الـ DLL هو نفس اسم ملف المشروع
ENTRYPOINT ["dotnet", "Alshifa_clinic_galgany.dll"]
