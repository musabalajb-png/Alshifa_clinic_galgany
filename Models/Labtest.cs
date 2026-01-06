namespace Alshifa_clinic_galgany.Models
{
    public class LabTest
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string NormalRange { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime? ResultDate { get; set; }
        public decimal Price { get; set; } = 0;
        public string TechnicianName { get; set; } = string.Empty;
        
        public Patient Patient { get; set; }
    }
}
