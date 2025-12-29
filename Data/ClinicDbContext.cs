using Microsoft.EntityFrameworkCore;
using Alshifa_clinic_galgany.Models;

namespace Alshifa_clinic_galgany.Data
{
    public class ClinicDbContext : DbContext
    {
        public ClinicDbContext(DbContextOptions<ClinicDbContext> options)
            : base(options)
        {
        }

        // جداول العيادة والمعمل
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<LabTest> LabTests { get; set; }

        // جداول الصيدلية والتقارير المالية (التحديث الجديد)
        public DbSet<Medication> Medications { get; set; }
        public DbSet<PharmacyTransaction> PharmacyTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // تأكيد تسمية الجداول في قاعدة بيانات Somee
            modelBuilder.Entity<Patient>().ToTable("Patients");
            modelBuilder.Entity<Visit>().ToTable("Visits");
            modelBuilder.Entity<LabTest>().ToTable("LabTests");
            modelBuilder.Entity<Medication>().ToTable("Medications");
            modelBuilder.Entity<PharmacyTransaction>().ToTable("PharmacyTransactions");

            // ضبط دقة الأسعار والمبالغ المالية (Decimal)
            modelBuilder.Entity<Medication>()
                .Property(m => m.SellingPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Medication>()
                .Property(m => m.CostPrice).HasColumnType("decimal(18,2)");
                
            modelBuilder.Entity<PharmacyTransaction>()
                .Property(t => t.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<PharmacyTransaction>()
                .Property(t => t.Profit).HasColumnType("decimal(18,2)");
        }
    }
}
