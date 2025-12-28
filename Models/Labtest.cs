using System.ComponentModel.DataAnnotations;

namespace Alshifa_clinic_galgany.Models
{
    public class LabTest
    {
        [Key]
        public int Id { get; set; }
        public int VisitId { get; set; }
        public string TestName { get; set; }
        public string Result { get; set; }
        public string Status { get; set; } // Pending, Completed
    }
}
