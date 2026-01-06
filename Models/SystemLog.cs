namespace Alshifa_clinic_galgany.Models
{
    public class SystemLog
    {
        public int Id { get; set; }
        public string LogType { get; set; } = string.Empty;
        public string LogData { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public User User { get; set; }
    }
}
