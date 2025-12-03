using CustomsBot.Models;

namespace CustomsBot.Services
{
    public class MessageHandler
    {
        private readonly SessionManager _sessionManager;

        public MessageHandler(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        public string ProcessMessage(string phoneNumber, string messageText)
        {
            var session = _sessionManager.GetOrCreateSession(phoneNumber);

            // لو في القائمة الرئيسية
            if (session.CurrentService == 0)
            {
                return HandleMainMenu(session, messageText);
            }

            // لو في خدمة معينة
            return HandleServiceFlow(session, messageText);
        }

        private string HandleMainMenu(UserSession session, string messageText)
        {
            // رسالة الترحيب
            if (string.IsNullOrWhiteSpace(messageText) || messageText.ToLower().Contains("مرحبا") || messageText.ToLower().Contains("start"))
            {
                return GetWelcomeMessage();
            }

            // اختيار الخدمة
            if (int.TryParse(messageText.Trim(), out int serviceNumber) && serviceNumber >= 1 && serviceNumber <= 8)
            {
                session.CurrentService = serviceNumber;
                session.CurrentStep = 1;
                session.CollectedData.Clear();
                _sessionManager.UpdateSession(session);

                return GetServiceFirstQuestion(serviceNumber);
            }

            return "❌ الرجاء اختيار رقم من 1 إلى 8\n\n" + GetWelcomeMessage();
        }

        private string HandleServiceFlow(UserSession session, string messageText)
        {
            return session.CurrentService switch
            {
                1 => HandleCustomsClearance(session, messageText),
                2 => HandleSaberCertificate(session, messageText),
                3 => HandleInternationalShipping(session, messageText),
                4 => HandleExport(session, messageText),
                5 => HandleLocalTransport(session, messageText),
                6 => HandleStorage(session, messageText),
                7 => HandlePackaging(session, messageText),
                8 => HandleEventsExhibitions(session, messageText),
                _ => "حدث خطأ. الرجاء البدء من جديد."
            };
        }

        private string GetWelcomeMessage()
        {
            return @"مرحباً بك في نخبة المنافذ ويسعدنا خدمتك 🌟

اختر رقم الخدمة ليتم توجيهك مباشرة:

1️⃣ التخليص الجمركي
2️⃣ إصدار شهادة سابر
3️⃣ الشحن الدولي
4️⃣ التصدير
5️⃣ النقل المحلي
6️⃣ التخزين
7️⃣ التعبئة والتغليف
8️⃣ الفعاليات والمعارض

اكتب رقم الخدمة فقط.";
        }

        private string GetServiceFirstQuestion(int serviceNumber)
        {
            return serviceNumber switch
            {
                1 => "✅ تم اختيار خدمة التخليص الجمركي\n\n📋 نحتاج منك:\n\n1️⃣ أرفق صورة بوليصة الشحن أو رقمها",
                2 => "✅ تم اختيار خدمة إصدار شهادة سابر\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع المنتج؟\n(يرجى ذكر النوع بشكل محدد. مثال: لمبات LED، خلاط كهربائي)",
                3 => "✅ تم اختيار خدمة الشحن الدولي\n\n📋 نحتاج منك:\n\n1️⃣ من أي مدينة سيتم الشحن؟",
                4 => "✅ تم اختيار خدمة التصدير\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع المنتج؟",
                5 => "✅ تم اختيار خدمة النقل المحلي\n\n📋 نحتاج منك:\n\n1️⃣ حدد موقع الاستلام (اسم مدينة + حي)",
                6 => "✅ تم اختيار خدمة التخزين\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع البضاعة؟",
                7 => "✅ تم اختيار خدمة التعبئة والتغليف\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع البضاعة؟",
                8 => "✅ تم اختيار خدمة الفعاليات والمعارض\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع الفعالية أو المعرض؟",
                _ => "حدث خطأ"
            };
        }

        // خدمة 1: التخليص الجمركي (مثال كامل)
        private string HandleCustomsClearance(UserSession session, string messageText)
        {
            switch (session.CurrentStep)
            {
                case 1: // بوليصة الشحن
                    if (string.IsNullOrWhiteSpace(messageText) || messageText.Length < 3)
                    {
                        return "❌ الرجاء إرسال بوليصة الشحن أو رقمها بشكل واضح.";
                    }
                    session.CollectedData["bill_of_lading"] = messageText;
                    session.CurrentStep = 2;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام بوليصة الشحن\n\n2️⃣ حدد المنفذ:\n• مطار\n• ميناء بحري\n• منفذ بري";

                case 2: // اسم المنفذ
                    var portType = messageText.Trim().ToLower();
                    if (!portType.Contains("مطار") && !portType.Contains("ميناء") && !portType.Contains("منفذ") && !portType.Contains("بري"))
                    {
                        return "❌ لتجهيز الطلب نحتاج تحديد نوع المنفذ:\n• مطار\n• ميناء بحري\n• منفذ بري";
                    }
                    session.CollectedData["port_type"] = messageText;
                    _sessionManager.UpdateSession(session);

                    // إنهاء الخدمة
                    var summary = $@"✅ تم استلام جميع البيانات بنجاح!

📋 ملخص طلبك:
━━━━━━━━━━━━━━━━
🔹 الخدمة: التخليص الجمركي
🔹 بوليصة الشحن: {session.CollectedData["bill_of_lading"]}
🔹 المنفذ: {session.CollectedData["port_type"]}

⏳ سيتم حساب الرسوم والمدة والبدء بالإجراءات فوراً.
📞 سيتم التواصل معك قريباً.

شكراً لاختيارك نخبة المنافذ 🌟

للعودة للقائمة الرئيسية، اكتب: قائمة";

                    // إعادة تعيين الجلسة
                    _sessionManager.ResetSession(session.PhoneNumber);
                    return summary;

                default:
                    return "حدث خطأ. الرجاء البدء من جديد.";
            }
        }

        // باقي الخدمات (نفس النمط)
        private string HandleSaberCertificate(UserSession session, string messageText)
        {
            return "🚧 هذه الخدمة قيد التطوير. سيتم إضافتها قريباً.";
        }

        private string HandleInternationalShipping(UserSession session, string messageText)
        {
            return "🚧 هذه الخدمة قيد التطوير. سيتم إضافتها قريباً.";
        }

        private string HandleExport(UserSession session, string messageText)
        {
            return "🚧 هذه الخدمة قيد التطوير. سيتم إضافتها قريباً.";
        }

        private string HandleLocalTransport(UserSession session, string messageText)
        {
            return "🚧 هذه الخدمة قيد التطوير. سيتم إضافتها قريباً.";
        }

        private string HandleStorage(UserSession session, string messageText)
        {
            return "🚧 هذه الخدمة قيد التطوير. سيتم إضافتها قريباً.";
        }

        private string HandlePackaging(UserSession session, string messageText)
        {
            return "🚧 هذه الخدمة قيد التطوير. سيتم إضافتها قريباً.";
        }

        private string HandleEventsExhibitions(UserSession session, string messageText)
        {
            return "🚧 هذه الخدمة قيد التطوير. سيتم إضافتها قريباً.";
        }
    }
}
