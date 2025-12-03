namespace CustomsBot.Models
{
    public class UserSession
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public int CurrentService { get; set; } = 0; // 0 = قائمة رئيسية، 1-8 = الخدمات
        public int CurrentStep { get; set; } = 0; // رقم الخطوة في الخدمة
        public Dictionary<string, string> CollectedData { get; set; } = new();
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    }
}
