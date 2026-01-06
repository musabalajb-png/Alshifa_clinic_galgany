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
        private readonly ILogger<ClinicController> _logger;

        public ClinicController(ClinicDbContext context, ILogger<ClinicController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region === إدارة المرضى ===

        [HttpPost("patients")]
        public async Task<IActionResult> AddPatient([FromBody] Patient patient)
        {
            try
            {
                patient.RegistrationDate = DateTime.Now;
                patient.Status = "waiting_doctor";
                
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"تم إضافة مريض جديد: {patient.Name} (ID: {patient.Id})");
                return Ok(new { success = true, data = patient });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في إضافة مريض: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            try
            {
                var patients = await _context.Patients
                    .OrderByDescending(p => p.RegistrationDate)
                    .ToListAsync();
                    
                return Ok(new { success = true, data = patients });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب المرضى: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("patients/{id}")]
        public async Task<IActionResult> GetPatient(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.Visits)
                    .Include(p => p.LabTests)
                    .Include(p => p.Prescriptions)
                    .Include(p => p.NursingRecords)
                    .FirstOrDefaultAsync(p => p.Id == id);
                    
                if (patient == null)
                    return NotFound(new { success = false, error = "المريض غير موجود" });
                    
                return Ok(new { success = true, data = patient });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب بيانات المريض: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("patients/waiting")]
        public async Task<IActionResult> GetWaitingPatients()
        {
            try
            {
                var patients = await _context.Patients
                    .Where(p => p.Status == "waiting_doctor")
                    .OrderBy(p => p.RegistrationDate)
                    .ToListAsync();
                    
                return Ok(new { success = true, data = patients });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب المرضى المنتظرين: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPut("patients/{id}/status")]
        public async Task<IActionResult> UpdatePatientStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null)
                    return NotFound(new { success = false, error = "المريض غير موجود" });
                    
                patient.Status = dto.Status;
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"تم تحديث حالة المريض {patient.Name} إلى: {dto.Status}");
                return Ok(new { success = true, data = patient });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في تحديث حالة المريض: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region === إدارة الزيارات ===

        [HttpPost("visits")]
        public async Task<IActionResult> AddVisit([FromBody] Visit visit)
        {
            try
            {
                visit.VisitDate = DateTime.Now;
                _context.Visits.Add(visit);
                
                // تحديث حالة المريض
                var patient = await _context.Patients.FindAsync(visit.PatientId);
                if (patient != null)
                {
                    patient.Status = "completed_visit";
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"تم إضافة زيارة للمريض ID: {visit.PatientId}");
                return Ok(new { success = true, data = visit });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في إضافة زيارة: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("patients/{patientId}/visits")]
        public async Task<IActionResult> GetPatientVisits(int patientId)
        {
            try
            {
                var visits = await _context.Visits
                    .Where(v => v.PatientId == patientId)
                    .OrderByDescending(v => v.VisitDate)
                    .ToListAsync();
                    
                return Ok(new { success = true, data = visits });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب زيارات المريض: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region === إدارة المعمل ===

        [HttpPost("lab/tests")]
        public async Task<IActionResult> RequestLabTest([FromBody] LabTest test)
        {
            try
            {
                test.RequestDate = DateTime.Now;
                test.Status = "Pending";
                
                _context.LabTests.Add(test);
                
                // تحديث حالة المريض
                var patient = await _context.Patients.FindAsync(test.PatientId);
                if (patient != null)
                {
                    patient.Status = "waiting_lab";
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"تم طلب فحص معمل للمريض ID: {test.PatientId}");
                return Ok(new { success = true, data = test });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في طلب فحص معمل: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("lab/tests/pending")]
        public async Task<IActionResult> GetPendingLabTests()
        {
            try
            {
                var tests = await _context.LabTests
                    .Include(t => t.Patient)
                    .Where(t => t.Status == "Pending")
                    .OrderBy(t => t.RequestDate)
                    .ToListAsync();
                    
                return Ok(new { success = true, data = tests });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب الفحوصات المعلقة: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPut("lab/tests/{id}/results")]
        public async Task<IActionResult> UpdateLabTestResult(int id, [FromBody] LabTestResultDto dto)
        {
            try
            {
                var test = await _context.LabTests
                    .Include(t => t.Patient)
                    .FirstOrDefaultAsync(t => t.Id == id);
                    
                if (test == null)
                    return NotFound(new { success = false, error = "الفحص غير موجود" });
                    
                test.Result = dto.Result;
                test.NormalRange = dto.NormalRange;
                test.Status = dto.Status;
                test.ResultDate = DateTime.Now;
                test.TechnicianName = dto.TechnicianName;
                
                // تحديث حالة المريض إذا اكتمل الفحص
                if (dto.Status == "Completed" && test.Patient != null)
                {
                    test.Patient.Status = "waiting_pharmacy";
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"تم تحديث نتيجة الفحص ID: {id}");
                return Ok(new { success = true, data = test });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في تحديث نتيجة الفحص: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region === إدارة الصيدلية ===

        [HttpGet("medications")]
        public async Task<IActionResult> GetMedications()
        {
            try
            {
                var medications = await _context.Medications
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
                    
                return Ok(new { success = true, data = medications });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب الأدوية: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("medications")]
        public async Task<IActionResult> AddMedication([FromBody] Medication medication)
        {
            try
            {
                medication.CreatedAt = DateTime.Now;
                medication.IsActive = true;
                
                _context.Medications.Add(medication);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"تم إضافة دواء جديد: {medication.Name}");
                return Ok(new { success = true, data = medication });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في إضافة دواء: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPut("medications/{id}/stock")]
        public async Task<IActionResult> UpdateMedicationStock(int id, [FromBody] StockUpdateDto dto)
        {
            try
            {
                var medication = await _context.Medications.FindAsync(id);
                if (medication == null)
                    return NotFound(new { success = false, error = "الدواء غير موجود" });
                    
                medication.Quantity += dto.Quantity; // يمكن أن تكون القيمة موجبة أو سالبة
                
                if (medication.Quantity < 0)
                    medication.Quantity = 0;
                    
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"تم تحديث مخزون الدواء {medication.Name}: {dto.Quantity}");
                return Ok(new { success = true, data = medication });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في تحديث المخزون: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("pharmacy/transactions")]
        public async Task<IActionResult> AddPharmacyTransaction([FromBody] PharmacyTransaction transaction)
        {
            try
            {
                transaction.TransactionDate = DateTime.Now;
                
                // التحقق من وجود الدواء والمخزون الكافي
                var medication = await _context.Medications.FindAsync(transaction.MedicationId);
                if (medication == null)
                    return BadRequest(new { success = false, error = "الدواء غير موجود" });
                    
                if (medication.Quantity < transaction.Quantity)
                    return BadRequest(new { success = false, error = "المخزون غير كافي" });
                
                // حساب الأرباح
                transaction.Profit = (medication.SellingPrice - medication.CostPrice) * transaction.Quantity;
                transaction.TotalAmount = medication.SellingPrice * transaction.Quantity;
                
                // خصم الكمية من المخزون
                medication.Quantity -= transaction.Quantity;
                
                // تحديث حالة المريض
                var patient = await _context.Patients.FindAsync(transaction.PatientId);
                if (patient != null)
                {
                    patient.Status = "completed";
                }
                
                _context.PharmacyTransactions.Add(transaction);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"تم صرف دواء للمريض ID: {transaction.PatientId}");
                return Ok(new { success = true, data = transaction });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في صرف الدواء: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region === إدارة التمريض ===

        [HttpPost("nursing/records")]
        public async Task<IActionResult> AddNursingRecord([FromBody] NursingRecord record)
        {
            try
            {
                record.RecordDate = DateTime.Now;
                
                _context.NursingRecords.Add(record);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"تم إضافة سجل تمريض للمريض ID: {record.PatientId}");
                return Ok(new { success = true, data = record });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في إضافة سجل تمريض: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("patients/{patientId}/nursing-records")]
        public async Task<IActionResult> GetPatientNursingRecords(int patientId)
        {
            try
            {
                var records = await _context.NursingRecords
                    .Where(n => n.PatientId == patientId)
                    .OrderByDescending(n => n.RecordDate)
                    .ToListAsync();
                    
                return Ok(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب سجلات التمريض: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region === إدارة المستخدمين والمصادقة ===

        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.Username == dto.Username && u.Password == dto.Password);
                    
                if (user == null)
                    return Unauthorized(new { success = false, error = "اسم المستخدم أو كلمة المرور غير صحيحة" });
                    
                if (!user.IsActive)
                    return Unauthorized(new { success = false, error = "الحساب معطل" });
                
                // تحديث وقت آخر دخول
                user.LastLogin = DateTime.Now;
                await _context.SaveChangesAsync();
                
                var userData = new
                {
                    user.Id,
                    user.Username,
                    user.FullName,
                    user.Role,
                    user.DepartmentId,
                    DepartmentName = user.Department?.Name,
                    user.Email,
                    user.Phone
                };
                
                _logger.LogInformation($"تم تسجيل دخول المستخدم: {user.Username}");
                return Ok(new { success = true, data = userData });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في تسجيل الدخول: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Department)
                    .Where(u => u.IsActive)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.FullName,
                        u.Role,
                        u.DepartmentId,
                        DepartmentName = u.Department.Name,
                        u.Email,
                        u.Phone,
                        u.LastLogin
                    })
                    .ToListAsync();
                    
                return Ok(new { success = true, data = users });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب المستخدمين: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region === الإحصائيات والتقارير ===

        [HttpGet("stats/daily")]
        public async Task<IActionResult> GetDailyStats()
        {
            try
            {
                var today = DateTime.Today;
                
                var stats = new
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
                    CompletedLabTests = await _context.LabTests
                        .Where(l => l.RequestDate.Date == today && l.Status == "Completed")
                        .CountAsync(),
                    PharmacyTransactions = await _context.PharmacyTransactions
                        .Where(t => t.TransactionDate.Date == today)
                        .CountAsync(),
                    PharmacyIncome = await _context.PharmacyTransactions
                        .Where(t => t.TransactionDate.Date == today)
                        .SumAsync(t => t.TotalAmount),
                    PharmacyProfit = await _context.PharmacyTransactions
                        .Where(t => t.TransactionDate.Date == today)
                        .SumAsync(t => t.Profit)
                };
                
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب الإحصائيات: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("stats/summary")]
        public async Task<IActionResult> GetSystemSummary()
        {
            try
            {
                var summary = new
                {
                    TotalPatients = await _context.Patients.CountAsync(),
                    TotalVisits = await _context.Visits.CountAsync(),
                    TotalMedications = await _context.Medications.CountAsync(),
                    ActiveUsers = await _context.Users.Where(u => u.IsActive).CountAsync(),
                    TotalTransactions = await _context.PharmacyTransactions.CountAsync(),
                    TotalRevenue = await _context.PharmacyTransactions.SumAsync(t => t.TotalAmount)
                };
                
                return Ok(new { success = true, data = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError($"خطأ في جلب ملخص النظام: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        #endregion
    }

    #region === DTOs ===

    public class UpdateStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class LabTestResultDto
    {
        public string Result { get; set; } = string.Empty;
        public string NormalRange { get; set; } = string.Empty;
        public string Status { get; set; } = "Completed";
        public string TechnicianName { get; set; } = string.Empty;
    }

    public class StockUpdateDto
    {
        public int Quantity { get; set; }
    }

    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    #endregion
}
