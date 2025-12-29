using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Alshifa_clinic_galgany.Data;
using Alshifa_clinic_galgany.Models;

namespace Alshifa_clinic_galgany.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PharmacyController : ControllerBase
    {
        private readonly ClinicDbContext _context;

        public PharmacyController(ClinicDbContext context)
        {
            _context = context;
        }

        // 1. عرض كل الأدوية في المخزن
        [HttpGet("inventory")]
        public async Task<ActionResult<IEnumerable<Medication>>> GetInventory()
        {
            return await _context.Medications.ToListAsync();
        }

        // 2. إضافة دواء جديد للمخزن
        [HttpPost("add-medication")]
        public async Task<ActionResult<Medication>> AddMedication(Medication medication)
        {
            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم إضافة الدواء بنجاح", data = medication });
        }

        // 3. عملية صرف دواء (بيع)
        [HttpPost("sell")]
        public async Task<IActionResult> SellMedication(int id, int quantity)
        {
            var med = await _context.Medications.FindAsync(id);
            if (med == null || med.StockQuantity < quantity)
                return BadRequest("الكمية غير كافية أو الدواء غير موجود");

            // خصم من المخزن
            med.StockQuantity -= quantity;

            // حساب الأرباح والعملية
            var transaction = new PharmacyTransaction
            {
                MedicationId = id,
                QuantitySold = quantity,
                TotalAmount = quantity * med.SellingPrice,
                Profit = (med.SellingPrice - med.CostPrice) * quantity,
                TransactionDate = DateTime.Now
            };

            _context.PharmacyTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تمت عملية الصرف بنجاح", total = transaction.TotalAmount });
        }

        // 4. التقرير المالي (يومي وشهري)
        [HttpGet("reports")]
        public async Task<IActionResult> GetFinancialReport()
        {
            var today = DateTime.Today;
            var month = DateTime.Today.Month;

            var dailySales = await _context.PharmacyTransactions
                .Where(t => t.TransactionDate.Date == today)
                .SumAsync(t => t.TotalAmount);

            var dailyProfit = await _context.PharmacyTransactions
                .Where(t => t.TransactionDate.Date == today)
                .SumAsync(t => t.Profit);

            var monthlySales = await _context.PharmacyTransactions
                .Where(t => t.TransactionDate.Month == month)
                .SumAsync(t => t.TotalAmount);

            return Ok(new
            {
                TodaySales = dailySales,
                TodayProfit = dailyProfit,
                ThisMonthSales = monthlySales
            });
        }
    }
}
