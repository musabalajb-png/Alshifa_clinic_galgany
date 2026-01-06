namespace Alshifa_clinic_galgany.Models
{
    public class Visit
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime VisitDate { get; set; } = DateTime.Now;
        public string Complaint { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Prescription { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public string DoctorNotes { get; set; } = string.Empty;
    }
}
