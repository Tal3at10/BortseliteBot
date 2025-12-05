using System.Text;
using System.Text.Json;

namespace CustomsBot.Services
{
    public class TelegramService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public TelegramService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<bool> SendOrderNotification(string customerPhone, string serviceName, Dictionary<string, string> orderData)
        {
            try
            {
                var botToken = _configuration["TelegramSettings:BotToken"];
                var chatId = _configuration["TelegramSettings:ChatId"];

                var url = $"https://api.telegram.org/bot{botToken}/sendMessage";

                var message = BuildTelegramMessage(customerPhone, serviceName, orderData);

                var payload = new
                {
                    chat_id = chatId,
                    text = message,
                    parse_mode = "HTML"
                };

                var jsonContent = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"✅ تم إرسال إشعار Telegram بنجاح");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ فشل إرسال إشعار Telegram: {response.StatusCode}");
                    Console.WriteLine($"❌ التفاصيل: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ في إرسال إشعار Telegram: {ex.Message}");
                return false;
            }
        }

        private string BuildTelegramMessage(string customerPhone, string serviceName, Dictionary<string, string> orderData)
        {
            var message = $@"🔔 <b>طلب جديد من CustomsBot</b>

━━━━━━━━━━━━━━━━━━━━━━

📱 <b>رقم العميل:</b> <code>{customerPhone}</code>
🔹 <b>الخدمة:</b> {serviceName}

━━━━━━━━━━━━━━━━━━━━━━

📋 <b>تفاصيل الطلب:</b>
";

            foreach (var item in orderData)
            {
                message += $"• <b>{GetArabicLabel(item.Key)}:</b> {item.Value}\n";
            }

            message += $@"
━━━━━━━━━━━━━━━━━━━━━━

⏰ <b>التاريخ والوقت:</b> {DateTime.Now:dd/MM/yyyy hh:mm tt}

━━━━━━━━━━━━━━━━━━━━━━

💬 <b>للرد على العميل:</b>
افتح WhatsApp وابعت رسالة على الرقم:
<code>{customerPhone}</code>

━━━━━━━━━━━━━━━━━━━━━━

🤖 تم الإرسال تلقائياً من CustomsBot
🌟 نخبة المنافذ - خدمات لوجستية متكاملة";

            return message;
        }

        private string GetArabicLabel(string key)
        {
            return key switch
            {
                "bill_of_lading" => "بوليصة الشحن",
                "port_type" => "المنفذ",
                "product_type" => "نوع المنتج",
                "hs_code" => "رمز HS",
                "manufacturer" => "المصنع",
                "invoice" => "الفاتورة",
                "shipping_city" => "مدينة الشحن",
                "goods_type" => "نوع البضاعة",
                "weight" => "الوزن",
                "shipping_type" => "نوع الشحن",
                "destination_country" => "الدولة المستوردة",
                "quantity" => "الكمية",
                "origin_certificate" => "شهادة منشأ",
                "pickup_location" => "موقع الاستلام",
                "delivery_location" => "موقع التسليم",
                "cargo_type" => "نوع الحمولة",
                "loading_time" => "وقت التحميل",
                "size" => "الحجم",
                "duration" => "مدة التخزين",
                "packaging_type" => "نوع التغليف",
                "event_type" => "نوع الفعالية",
                _ => key
            };
        }
    }
}
