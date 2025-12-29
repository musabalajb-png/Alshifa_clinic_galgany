using System;
using System.ComponentModel.DataAnnotations;

namespace Alshifa_clinic_galgany.Models
{
    public class Medication
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // اسم الدواء

        public string? Dosage { get; set; } // الجرعة (مثل 500mg)

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } // الكمية الموجودة في الرف

        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; } // سعر الشراء (عشان نحسب الأرباح)

        [Range(0, double.MaxValue)]
        public decimal SellingPrice { get; set; } // سعر البيع للجمهور

        public DateTime ExpiryDate { get; set; } // تاريخ انتهاء الصلاحية

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
