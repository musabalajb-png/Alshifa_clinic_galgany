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

        [HttpPost("patients")]
        public async Task<IActionResult> AddPatient(Patient patient) {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return Ok(patient);
        }

        [HttpPost("visits")]
        public async Task<IActionResult> CreateVisit(Visit visit) {
            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();
            return Ok(visit);
        }

        [HttpGet("lab/pending")]
        public async Task<IActionResult> GetPendingTests() {
            var tests = await _context.LabTests.Where(t => t.Status == "Pending").ToListAsync();
            return Ok(tests);
        }

        [HttpPut("lab/results/{id}")]
        public async Task<IActionResult> UpdateLabResult(int id, [FromBody] string result) {
            var test = await _context.LabTests.FindAsync(id);
            if (test == null) return NotFound();
            test.Result = result;
            test.Status = "Completed";
            await _context.SaveChangesAsync();
            return Ok(test);
        }
    }
}
