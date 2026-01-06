namespace Alshifa_clinic_galgany.Models
{
    public class NursingRecord
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime RecordDate { get; set; } = DateTime.Now;
        public decimal Temperature { get; set; }
        public string BloodPressure { get; set; } = string.Empty;
        public decimal Pulse { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string NurseName { get; set; } = string.Empty;
        
        public Patient Patient { get; set; }
    }
}
