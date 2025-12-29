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

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<LabTest> LabTests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // تأكيد تسمية الجداول عشان ما يحصل تعارض مع قاعدة بيانات Somee
            modelBuilder.Entity<Patient>().ToTable("Patients");
            modelBuilder.Entity<Visit>().ToTable("Visits");
            modelBuilder.Entity<LabTest>().ToTable("LabTests");
        }
    }
}
// أضف السطر ده تحت الـ DbSets القديمة
public DbSet<Medication> Medications { get; set; }
