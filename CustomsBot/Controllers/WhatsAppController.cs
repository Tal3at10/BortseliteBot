using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CustomsBot.Models;
using CustomsBot.Services;

namespace CustomsBot.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SessionManager _sessionManager;
        private readonly MessageHandler _messageHandler;
        private readonly WhatsAppService _whatsAppService;

        public WhatsAppController(
            IConfiguration configuration,
            SessionManager sessionManager,
            MessageHandler messageHandler,
            WhatsAppService whatsAppService)
        {
            _configuration = configuration;
            _sessionManager = sessionManager;
            _messageHandler = messageHandler;
            _whatsAppService = whatsAppService;
        }

        // 1. دالة التحقق (GET)
        // Meta ستستخدم هذا الرابط للتأكد من أن السيرفر يعمل
        [HttpGet("webhook")]
        public IActionResult VerifyWebhook(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string token,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            // 🔍 طباعة القيم للتأكد من وصولها صح
            Console.WriteLine($"📥 Mode: {mode}");
            Console.WriteLine($"📥 Token: {token}");
            Console.WriteLine($"📥 Challenge: {challenge}");

            // قراءة الرمز السري من ملف appsettings.json
            var mySecretToken = _configuration["WhatsAppSettings:VerifyToken"];
            Console.WriteLine($"🔑 My Token: {mySecretToken}");

            // التحقق من أن القيم مش فاضية
            if (string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(challenge))
            {
                Console.WriteLine("❌ أحد القيم فاضي!");
                return BadRequest("Missing parameters");
            }

            // التحقق من المطابقة
            if (mode == "subscribe" && token == mySecretToken)
            {
                Console.WriteLine($"✅ التحقق نجح! سأرجع: {challenge}");
                
                // ✅ إرجاع الـ challenge كنص خام مع تحديد Content-Type بوضوح
                return Content(challenge, "text/plain");
            }

            // إذا كان الرمز خطأ
            Console.WriteLine("❌ الرمز السري غير مطابق!");
            return Unauthorized();
        }

        // 2. دالة استقبال الرسائل (POST)
        // هنا ستصل رسائل العملاء
        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveMessage([FromBody] WhatsAppWebhook webhook)
        {
            try
            {
                Console.WriteLine("📩 تم استقبال رسالة جديدة من واتساب!");

                // التحقق من وجود رسائل
                if (webhook?.Entry == null || !webhook.Entry.Any())
                {
                    return Ok();
                }

                foreach (var entry in webhook.Entry)
                {
                    foreach (var change in entry.Changes)
                    {
                        if (change.Value?.Messages == null || !change.Value.Messages.Any())
                            continue;

                        foreach (var message in change.Value.Messages)
                        {
                            // معالجة الرسائل النصية فقط
                            if (message.Type == "text" && message.Text != null)
                            {
                                var phoneNumber = message.From;
                                var messageText = message.Text.Body;

                                Console.WriteLine($"📱 من: {phoneNumber}");
                                Console.WriteLine($"💬 الرسالة: {messageText}");

                                // معالجة الرسالة والحصول على الرد
                                var responseText = _messageHandler.ProcessMessage(phoneNumber, messageText);

                                // إرسال الرد
                                await _whatsAppService.SendTextMessage(phoneNumber, responseText);
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ في معالجة الرسالة: {ex.Message}");
                Console.WriteLine($"❌ التفاصيل: {ex.StackTrace}");
                return Ok(); // نرجع OK حتى لو في خطأ عشان Meta ميعيدش الطلب
            }
        }
    }
}