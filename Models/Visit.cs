using System.ComponentModel.DataAnnotations;

namespace Alshifa_clinic_galgany.Models
{
    public class Visit
    {
        [Key]
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime VisitDate { get; set; }
        public string Diagnosis { get; set; }
        public string Prescription { get; set; }
    }
}
