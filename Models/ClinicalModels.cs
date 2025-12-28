using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alshifa_clinic_galgany.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<Visit> Visits { get; set; } = new();
    }

    public class Visit
    {
        [Key]
        public int VisitId { get; set; }
        public int PatientId { get; set; }
        public DateTime VisitDate { get; set; } = DateTime.Now;
        public string Complaint { get; set; } 
        public string Diagnosis { get; set; } 
        public Patient? Patient { get; set; }
        public List<LabTest> LabTests { get; set; } = new();
        public List<Prescription> Prescriptions { get; set; } = new();
    }

    public class LabTest
    {
        [Key]
        public int TestId { get; set; }
        public int VisitId { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string? Result { get; set; }
        public string Status { get; set; } = "Pending"; 
    }

    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }
        public int VisitId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public bool IsDispensed { get; set; } = false;
    }
}
