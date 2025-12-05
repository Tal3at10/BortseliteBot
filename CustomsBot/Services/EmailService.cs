using System.Net;
using System.Net.Mail;

namespace CustomsBot.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendOrderNotification(string customerPhone, string serviceName, Dictionary<string, string> orderData)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var receiverEmail = _configuration["EmailSettings:ReceiverEmail"];

                using var smtpClient = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "نخبة المنافذ - CustomsBot"),
                    Subject = $"🔔 طلب جديد - {serviceName}",
                    Body = BuildEmailBody(customerPhone, serviceName, orderData),
                    IsBodyHtml = false
                };

                mailMessage.To.Add(receiverEmail);

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"✅ تم إرسال إشعار البريد الإلكتروني بنجاح");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ في إرسال البريد الإلكتروني: {ex.Message}");
                return false;
            }
        }

        private string BuildEmailBody(string customerPhone, string serviceName, Dictionary<string, string> orderData)
        {
            var body = $@"🔔 طلب جديد من CustomsBot

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📱 رقم العميل: {customerPhone}
🔹 الخدمة: {serviceName}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

📋 تفاصيل الطلب:
";

            foreach (var item in orderData)
            {
                body += $"• {GetArabicLabel(item.Key)}: {item.Value}\n";
            }

            body += $@"
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

⏰ التاريخ والوقت: {DateTime.Now:dd/MM/yyyy hh:mm tt}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

💬 للرد على العميل:
افتح WhatsApp وابعت رسالة على الرقم: {customerPhone}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

تم الإرسال تلقائياً من CustomsBot 🤖
نخبة المنافذ - خدمات لوجستية متكاملة
";

            return body;
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
