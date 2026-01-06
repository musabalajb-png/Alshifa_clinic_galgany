using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alshifa_clinic_galgany.Data;
using Alshifa_clinic_galgany.Models;

namespace Alshifa_clinic_galgany.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicController : ControllerBase
    {
        private readonly ClinicDbContext _context;
        public ClinicController(ClinicDbContext context) { _context = context; }

        // 1. إدارة المرضى
        [HttpPost("patients")]
        public async Task<IActionResult> AddPatient(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return Ok(patient);
        }

        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var patients = await _context.Patients.ToListAsync();
            return Ok(patients);
        }

        [HttpGet("patients/waiting")]
        public async Task<IActionResult> GetWaitingPatients()
        {
            var patients = await _context.Patients
                .Where(p => p.Status == "waiting_doctor")
                .ToListAsync();
            return Ok(patients);
        }

        // 2. إدارة الزيارات
        [HttpPost("visits")]
        public async Task<IActionResult> CreateVisit(Visit visit)
        {
            _context.Visits.Add(visit);
            
            // تحديث حالة المريض
            var patient = await _context.Patients.FindAsync(visit.PatientId);
            if (patient != null)
            {
                patient.Status = "completed_doctor";
                _context.Patients.Update(patient);
            }
            
            await _context.SaveChangesAsync();
            return Ok(visit);
        }

        // 3. إدارة المعمل
        [HttpGet("lab/pending")]
        public async Task<IActionResult> GetPendingTests()
        {
            var tests = await _context.LabTests
                .Where(t => t.Status == "Pending")
                .ToListAsync();
            return Ok(tests);
        }

        [HttpPost("lab/request")]
        public async Task<IActionResult> RequestLabTest(LabTest test)
        {
            _context.LabTests.Add(test);
            
            // تحديث حالة المريض
            var patient = await _context.Patients.FindAsync(test.PatientId);
            if (patient != null)
            {
                patient.Status = "waiting_lab";
                _context.Patients.Update(patient);
            }
            
            await _context.SaveChangesAsync();
            return Ok(test);
        }

        [HttpPut("lab/results/{id}")]
        public async Task<IActionResult> UpdateLabResult(int id, [FromBody] LabTestUpdateDto update)
        {
            var test = await _context.LabTests.FindAsync(id);
            if (test == null) return NotFound();
            
            test.Result = update.Result;
            test.Status = update.Status;
            test.ResultDate = DateTime.Now;
            
            // إذا اكتمل الفحص، إعادة المريض للطبيب
            if (update.Status == "Completed")
            {
                var patient = await _context.Patients.FindAsync(test.PatientId);
                if (patient != null)
                {
                    patient.Status = "waiting_pharmacy";
                    _context.Patients.Update(patient);
                }
            }
            
            await _context.SaveChangesAsync();
            return Ok(test);
        }

        // 4. إدارة الصيدلية
        [HttpGet("medications")]
        public async Task<IActionResult> GetMedications()
        {
            var medications = await _context.Medications.ToListAsync();
            return Ok(medications);
        }

        [HttpPost("medications")]
        public async Task<IActionResult> AddMedication(Medication medication)
        {
            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();
            return Ok(medication);
        }

        [HttpPost("pharmacy/transaction")]
        public async Task<IActionResult> AddPharmacyTransaction(PharmacyTransaction transaction)
        {
            _context.PharmacyTransactions.Add(transaction);
            
            // تحديث كمية الدواء
            var medication = await _context.Medications.FindAsync(transaction.MedicationId);
            if (medication != null)
            {
                medication.Quantity -= transaction.Quantity;
                _context.Medications.Update(medication);
            }
            
            // تحديث حالة المريض
            var patient = await _context.Patients.FindAsync(transaction.PatientId);
            if (patient != null)
            {
                patient.Status = "completed";
                _context.Patients.Update(patient);
            }
            
            await _context.SaveChangesAsync();
            return Ok(transaction);
        }

        // 5. التقارير والإحصائيات
        [HttpGet("stats/daily")]
        public async Task<IActionResult> GetDailyStats()
        {
            var today = DateTime.Today;
            
            var dailyStats = new
            {
                TotalPatients = await _context.Patients
                    .Where(p => p.RegistrationDate.Date == today)
                    .CountAsync(),
                TotalVisits = await _context.Visits
                    .Where(v => v.VisitDate.Date == today)
                    .CountAsync(),
                TotalLabTests = await _context.LabTests
                    .Where(l => l.RequestDate.Date == today)
                    .CountAsync(),
                PharmacyIncome = await _context.PharmacyTransactions
                    .Where(t => t.TransactionDate.Date == today)
                    .SumAsync(t => t.TotalAmount)
            };
            
            return Ok(dailyStats);
        }
    }

    // DTO لتحديث نتائج المعمل
    public class LabTestUpdateDto
    {
        public string Result { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
    }
}
