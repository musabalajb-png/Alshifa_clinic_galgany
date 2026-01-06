#!/bin/bash
echo "🛠️  إعداد بيئة تطوير مجمع الشفاء..."

# تثبيت .NET SDK إذا لم يكن مثبتاً
if ! command -v dotnet &> /dev/null; then
    echo "📥 تثبيت .NET SDK 8.0..."
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --version 8.0.100
    export PATH="$HOME/.dotnet:$PATH"
fi

# التحقق من التثبيت
echo "🔍 التحقق من التثبيت..."
dotnet --version

# استعادة الحزم
echo "📦 استعادة حزم المشروع..."
dotnet restore

# إنشاء قاعدة البيانات
echo "🗄️  إنشاء قاعدة البيانات..."
dotnet ef database update

echo "✅ تم إعداد البيئة بنجاح!"
echo "🚀 لبدء التشغيل: dotnet run"
echo "🌐 المتصفح: http://localhost:5000"
echo "📚 Swagger: http://localhost:5000/api-docs"
