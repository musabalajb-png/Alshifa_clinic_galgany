using System.ComponentModel.DataAnnotations;

namespace Alshifa_clinic_galgany.Models
{
    public class Medication
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty; // اسم الدواء
        public string Dosage { get; set; } = string.Empty; // الجرعة
        public int StockQuantity { get; set; } // الكمية المتوفرة
        public decimal Price { get; set; } // السعر
    }
}
