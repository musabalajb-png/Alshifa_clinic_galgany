namespace Alshifa_clinic_galgany.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
