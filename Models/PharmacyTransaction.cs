using System;

namespace Alshifa_clinic_galgany.Models
{
    public class PharmacyTransaction
    {
        public int Id { get; set; }
        public int MedicationId { get; set; } // رقم الدواء المباع
        public int QuantitySold { get; set; } // الكمية الصرفها الصيدلي
        public decimal TotalAmount { get; set; } // (الكمية * سعر البيع)
        public decimal Profit { get; set; } // (سعر البيع - سعر الشراء) * الكمية
        public DateTime TransactionDate { get; set; } = DateTime.Now; // تاريخ العملية
    }
}
