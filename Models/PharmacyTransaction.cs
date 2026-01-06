namespace Alshifa_clinic_galgany.Models
{
    public class PharmacyTransaction
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int MedicationId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Profit { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string PharmacistName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        
        public Patient Patient { get; set; }
        public Medication Medication { get; set; }
    }
}
