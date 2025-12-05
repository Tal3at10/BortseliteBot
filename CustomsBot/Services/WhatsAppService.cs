using System.Text;
using System.Text.Json;

namespace CustomsBot.Services
{
    public class WhatsAppService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly TelegramService _telegramService;

        public WhatsAppService(IConfiguration configuration, HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            // Get TelegramService from service provider to avoid circular dependency
            _telegramService = serviceProvider.GetRequiredService<TelegramService>();
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

        public async Task<bool> DownloadAndForwardMedia(string mediaId, string customerPhone, string mediaType)
        {
            try
            {
                var accessToken = _configuration["WhatsAppSettings:AccessToken"];
                
                // Step 1: Get media URL
                var mediaInfoUrl = $"https://graph.facebook.com/v21.0/{mediaId}";
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                
                var mediaInfoResponse = await _httpClient.GetAsync(mediaInfoUrl);
                if (!mediaInfoResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ فشل الحصول على معلومات الملف");
                    return false;
                }
                
                var mediaInfoJson = await mediaInfoResponse.Content.ReadAsStringAsync();
                var mediaInfo = JsonSerializer.Deserialize<JsonElement>(mediaInfoJson);
                var mediaUrl = mediaInfo.GetProperty("url").GetString();
                
                if (string.IsNullOrEmpty(mediaUrl))
                {
                    Console.WriteLine($"❌ لم يتم العثور على رابط الملف");
                    return false;
                }
                
                // Step 2: Download media
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                
                var mediaResponse = await _httpClient.GetAsync(mediaUrl);
                if (!mediaResponse.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ فشل تحميل الملف");
                    return false;
                }
                
                var mediaBytes = await mediaResponse.Content.ReadAsByteArrayAsync();
                var mimeType = mediaResponse.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                var extension = GetFileExtension(mimeType);
                
                Console.WriteLine($"✅ تم تحميل الملف - الحجم: {mediaBytes.Length} bytes");
                
                // Step 3: Forward to Telegram
                await _telegramService.SendMediaToTelegram(mediaBytes, customerPhone, mediaType, extension, mimeType);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ في تحميل وإرسال الملف: {ex.Message}");
                return false;
            }
        }

        private string GetFileExtension(string mimeType)
        {
            return mimeType switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "application/pdf" => ".pdf",
                "application/msword" => ".doc",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                "application/vnd.ms-excel" => ".xls",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                _ => ".bin"
            };
        }
    }
}
