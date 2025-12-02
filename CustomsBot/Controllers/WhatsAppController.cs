using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CustomsBot.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WhatsAppController(IConfiguration configuration)
        {
            _configuration = configuration;
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
        // هنا ستصل رسائل العملاء لاحقاً
        [HttpPost("webhook")]
        public IActionResult ReceiveMessage([FromBody] object payload)
        {
            // طباعة الرسالة في الشاشة السوداء للتأكد من وصولها
            Console.WriteLine("📩 تم استقبال رسالة جديدة من واتساب!");

            // الرد بـ 200 OK ضروري جداً لكي لا يعتبر فيسبوك أن الرسالة فشلت
            return Ok();
        }
    }
}