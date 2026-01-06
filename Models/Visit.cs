namespace Alshifa_clinic_galgany.Models
{
    public class Visit
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime VisitDate { get; set; } = DateTime.Now;
        public string Complaint { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public decimal Fee { get; set; } = 2000;
        public string DoctorNotes { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        
        public Patient Patient { get; set; }
    }
}
