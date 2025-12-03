using System.Text;
using System.Text.Json;

namespace CustomsBot.Services
{
    public class WhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public WhatsAppService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<bool> SendTextMessage(string toPhoneNumber, string messageText)
        {
            try
            {
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                var phoneNumberId = _configuration["WhatsAppSettings:PhoneNumberId"];

                var url = $"https://graph.facebook.com/v21.0/{phoneNumberId}/messages";

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = toPhoneNumber,
                    type = "text",
                    text = new { body = messageText }
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ تم إرسال الرسالة بنجاح إلى {toPhoneNumber}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ فشل إرسال الرسالة: {response.StatusCode}");
                    Console.WriteLine($"❌ التفاصيل: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ في إرسال الرسالة: {ex.Message}");
                return false;
            }
        }
    }
}
