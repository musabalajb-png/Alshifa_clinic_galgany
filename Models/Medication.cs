namespace Alshifa_clinic_galgany.Models
{
    public class Medication
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GenericName { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public decimal CostPrice { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; } = DateTime.Now.AddYears(2);
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
