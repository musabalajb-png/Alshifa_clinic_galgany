using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace Alshifa_clinic_galgany.Data
{
    public static class DatabaseTester
    {
        public static async Task TestConnection(string connectionString)
        {
            Console.WriteLine("🔍 جاري اختبار الاتصال بقاعدة البيانات...");
            Console.WriteLine($"📡 السيرفر: AlshifaDb.mssql.somee.com");
            
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("✅ الاتصال ناجح!");
                    
                    // اختبار استعلام بسيط
                    var command = new SqlCommand("SELECT @@VERSION as Version, DB_NAME() as DatabaseName", connection);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Console.WriteLine($"📊 قاعدة البيانات: {reader["DatabaseName"]}");
                            Console.WriteLine($"⚙️  إصدار SQL Server: {reader["Version"]}");
                        }
                    }
                    
                    // اختبار الجداول
                    command = new SqlCommand(@"
                        SELECT 
                            TABLE_NAME,
                            (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) as ColumnsCount
                        FROM INFORMATION_SCHEMA.TABLES t
                        WHERE TABLE_TYPE = 'BASE TABLE'
                        ORDER BY TABLE_NAME", connection);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        Console.WriteLine("\n📋 الجداول الموجودة:");
                        while (await reader.ReadAsync())
                        {
                            Console.WriteLine($"  - {reader["TABLE_NAME"]} ({reader["ColumnsCount"]} أعمدة)");
                        }
                    }
                    
                    connection.Close();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"❌ خطأ SQL: {ex.Message}");
                Console.WriteLine($"🔢 رقم الخطأ: {ex.Number}");
                
                if (ex.Number == 18456)
                {
                    Console.WriteLine("🔐 خطأ في اسم المستخدم أو كلمة المرور!");
                }
                else if (ex.Number == 4060)
                {
                    Console.WriteLine("🗄️ قاعدة البيانات غير موجودة!");
                }
                else if (ex.Number == -1)
                {
                    Console.WriteLine("🌐 لا يمكن الوصول إلى السيرفر!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ عام: {ex.Message}");
            }
        }
    }
}
