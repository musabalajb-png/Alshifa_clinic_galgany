namespace Alshifa_clinic_galgany.Models
{
    public class FinancialRecord
    {
        public int Id { get; set; }
        public string RecordType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public int? PatientId { get; set; }
        public int? VisitId { get; set; }
        public DateTime RecordDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        
        public Patient Patient { get; set; }
        public Visit Visit { get; set; }
    }
}
