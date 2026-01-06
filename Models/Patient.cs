namespace Alshifa_clinic_galgany.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "waiting_doctor";
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public decimal TicketPrice { get; set; } = 2000;
        public string Notes { get; set; } = string.Empty;
        
        // التنقل
        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
        public ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<NursingRecord> NursingRecords { get; set; } = new List<NursingRecord>();
    }
}
