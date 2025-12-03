using CustomsBot.Models;
using System.Text.RegularExpressions;

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
            var input = messageText.Trim().ToLower();

            // التحقق من طلب العودة للقائمة الرئيسية
            if (input.Contains("قائمة") || input == "0")
            {
                _sessionManager.ResetSession(phoneNumber);
                return GetWelcomeMessage();
            }

            // التحقق من طلب الرجوع خطوة للخلف
            if ((input.Contains("رجوع") || input.Contains("السابق") || input == "back") && session.CurrentService != 0)
            {
                if (session.CurrentStep > 1)
                {
                    session.CurrentStep--;
                    _sessionManager.UpdateSession(session);
                    return "⬅️ تم الرجوع للخطوة السابقة\n\n" + GetCurrentStepQuestion(session);
                }
                else
                {
                    // إذا كان في الخطوة الأولى، يرجع للقائمة الرئيسية
                    _sessionManager.ResetSession(phoneNumber);
                    return "⬅️ تم الرجوع للقائمة الرئيسية\n\n" + GetWelcomeMessage();
                }
            }

            // لو في القائمة الرئيسية
            if (session.CurrentService == 0)
            {
                return HandleMainMenu(session, messageText);
            }

            // لو في خدمة معينة
            return HandleServiceFlow(session, messageText);
        }

        // دالة مساعدة لإرجاع السؤال الحالي بناءً على الخطوة
        private string GetCurrentStepQuestion(UserSession session)
        {
            return session.CurrentService switch
            {
                1 => GetCustomsClearanceQuestion(session.CurrentStep),
                2 => GetSaberCertificateQuestion(session.CurrentStep),
                3 => GetInternationalShippingQuestion(session.CurrentStep),
                4 => GetExportQuestion(session.CurrentStep),
                5 => GetLocalTransportQuestion(session.CurrentStep),
                6 => GetStorageQuestion(session.CurrentStep),
                7 => GetPackagingQuestion(session.CurrentStep),
                8 => GetEventsExhibitionsQuestion(session.CurrentStep),
                _ => "حدث خطأ"
            };
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
                1 => "✅ تم اختيار خدمة التخليص الجمركي\n\n📋 نحتاج منك:\n\n1️⃣ أرفق صورة بوليصة الشحن أو رقمها\n\n" + GetNavigationFooter(),
                2 => "✅ تم اختيار خدمة إصدار شهادة سابر\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع المنتج؟\n(يرجى ذكر النوع بشكل محدد. مثال: لمبات LED، خلاط كهربائي)\n\n" + GetNavigationFooter(),
                3 => "✅ تم اختيار خدمة الشحن الدولي\n\n📋 نحتاج منك:\n\n1️⃣ من أي مدينة سيتم الشحن؟\n\n" + GetNavigationFooter(),
                4 => "✅ تم اختيار خدمة التصدير\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع المنتج؟\n\n" + GetNavigationFooter(),
                5 => "✅ تم اختيار خدمة النقل المحلي\n\n📋 نحتاج منك:\n\n1️⃣ حدد موقع الاستلام (اسم مدينة + حي)\n\n" + GetNavigationFooter(),
                6 => "✅ تم اختيار خدمة التخزين\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع البضاعة؟\n\n" + GetNavigationFooter(),
                7 => "✅ تم اختيار خدمة التعبئة والتغليف\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع البضاعة؟\n\n" + GetNavigationFooter(),
                8 => "✅ تم اختيار خدمة الفعاليات والمعارض\n\n📋 نحتاج منك:\n\n1️⃣ ما نوع الفعالية أو المعرض؟\n\n" + GetNavigationFooter(),
                _ => "حدث خطأ"
            };
        }

        private string GetNavigationFooter()
        {
            return "━━━━━━━━━━━━━━━━\n💡 اكتب \"رجوع\" للخطوة السابقة\n💡 اكتب \"قائمة\" للقائمة الرئيسية";
        }

        // دوال الأسئلة لكل خدمة
        private string GetCustomsClearanceQuestion(int step)
        {
            return step switch
            {
                1 => "1️⃣ أرفق صورة بوليصة الشحن أو رقمها\n\n" + GetNavigationFooter(),
                2 => "2️⃣ حدد المنفذ:\n• مطار\n• ميناء بحري\n• منفذ بري\n\n" + GetNavigationFooter(),
                _ => "حدث خطأ"
            };
        }

        private string GetSaberCertificateQuestion(int step)
        {
            return step switch
            {
                1 => "1️⃣ ما نوع المنتج؟\n(يرجى ذكر النوع بشكل محدد. مثال: لمبات LED، خلاط كهربائي)\n\n" + GetNavigationFooter(),
                2 => "2️⃣ هل يتوفر رمز HS؟\nأرسل الرقم أو اكتب: لا\n\n" + GetNavigationFooter(),
                3 => "3️⃣ أرسل اسم المصنع أو المورد\n\n" + GetNavigationFooter(),
                4 => "4️⃣ أرسل الفاتورة أو عرض السعر\n(يمكنك إرسال صورة أو ملف أو كتابة \"تم الإرسال\")\n\n" + GetNavigationFooter(),
                _ => "حدث خطأ"
            };
        }

        private string GetInternationalShippingQuestion(int step)
        {
            return step switch
            {
                1 => "1️⃣ من أي مدينة سيتم الشحن؟\n\n" + GetNavigationFooter(),
                2 => "2️⃣ ما نوع البضاعة؟\n\n" + GetNavigationFooter(),
                3 => "3️⃣ كم الوزن التقريبي بالكيلو؟\n\n" + GetNavigationFooter(),
                4 => "4️⃣ تفضل الشحن البحري أم الجوي؟\n• بحري\n• جوي\n\n" + GetNavigationFooter(),
                _ => "حدث خطأ"
            };
        }

        private string GetExportQuestion(int step)
        {
            return step switch
            {
                1 => "1️⃣ ما نوع المنتج؟\n\n" + GetNavigationFooter(),
                2 => "2️⃣ ما الدولة المستوردة؟\n(حدد اسم الدولة بالضبط)\n\n" + GetNavigationFooter(),
                3 => "3️⃣ ما الكمية والوزن؟\n(مثال: 500 كيلو أو 100 قطعة)\n\n" + GetNavigationFooter(),
                4 => "4️⃣ هل تحتاج شهادة منشأ؟\n• نعم\n• لا\n\n" + GetNavigationFooter(),
                _ => "حدث خطأ"
            };
        }

        private string GetLocalTransportQuestion(int step)
        {
            return step switch
            {
                1 => "1️⃣ حدد موقع الاستلام (مدينة + حي)\n\n" + GetNavigationFooter(),
                2 => "2️⃣ حدد موقع التسليم (مدينة + حي)\n\n" + GetNavigationFooter(),
                3 => "3️⃣ ما نوع الحمولة؟\n\n" + GetNavigationFooter(),
                4 => "4️⃣ ما الوقت المطلوب للتحميل؟\n(مثال: اليوم الساعة 3 مساءً، غداً صباحاً)\n\n" + GetNavigationFooter(),
                _ => "حدث خطأ"
            };
        }

        private string GetStorageQuestion(int step)
        {
            return step switch
            {
                1 => "1️⃣ ما نوع البضاعة؟\n\n" + GetNavigationFooter(),
                2 => "2️⃣ ما الحجم أو عدد الطبليات؟\n(مثال: 10 طبليات، 50 متر مكعب)\n\n" + GetNavigationFooter(),
                3 => "3️⃣ ما مدة التخزين؟\n(حدد المدة بالأيام أو الأسابيع أو الأشهر)\n\n" + GetNavigationFooter(),
                _ => "حدث خطأ"
            };
        }

        private string GetPackagingQuestion(int step)
        {
            return step switch
            {
                1 => "1️⃣ ما نوع البضاعة؟\n\n" + GetNavigationFooter(),
                2 => "2️⃣ كم عدد القطع؟\n\n" + GetNavigationFooter(),
                3 => "3️⃣ ما نوع التغليف المطلوب؟\n• أساسي\n• شحن\n• حماية إضافية\n\n" + GetNavigationFooter(),
                _ => "حدث خطأ"
            };
        }

        private string GetEventsExhibitionsQuestion(int step)
        {
            return step switch
            {
                1 => "1️⃣ ما نوع الفعالية أو المعرض؟\n\n" + GetNavigationFooter(),
                _ => "حدث خطأ"
            };
        }

        // ==================== خدمة 1: التخليص الجمركي ====================
        private string HandleCustomsClearance(UserSession session, string messageText)
        {
            switch (session.CurrentStep)
            {
                case 1: // بوليصة الشحن
                    if (string.IsNullOrWhiteSpace(messageText) || messageText.Trim().Length < 3)
                    {
                        return "❌ الرجاء إرسال بوليصة الشحن أو رقمها بشكل واضح.\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["bill_of_lading"] = messageText.Trim();
                    session.CurrentStep = 2;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام بوليصة الشحن\n\n2️⃣ حدد المنفذ:\n• مطار\n• ميناء بحري\n• منفذ بري\n\n" + GetNavigationFooter();

                case 2: // اسم المنفذ
                    var portType = messageText.Trim().ToLower();
                    if (!portType.Contains("مطار") && !portType.Contains("ميناء") && !portType.Contains("منفذ") && !portType.Contains("بري"))
                    {
                        return "❌ لتجهيز الطلب نحتاج تحديد نوع المنفذ:\n• مطار\n• ميناء بحري\n• منفذ بري\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["port_type"] = messageText.Trim();

                    var summary1 = $@"✅ تم استلام جميع البيانات بنجاح!

📋 ملخص طلبك:
━━━━━━━━━━━━━━━━
🔹 الخدمة: التخليص الجمركي
🔹 بوليصة الشحن: {session.CollectedData["bill_of_lading"]}
🔹 المنفذ: {session.CollectedData["port_type"]}

⏳ سيتم حساب الرسوم والمدة والبدء بالإجراءات فوراً.
📞 سيتم التواصل معك قريباً.

شكراً لاختيارك نخبة المنافذ 🌟

للعودة للقائمة الرئيسية، اكتب: قائمة";

                    _sessionManager.ResetSession(session.PhoneNumber);
                    return summary1;

                default:
                    return "حدث خطأ. الرجاء البدء من جديد.\n\n" + GetNavigationFooter();
            }
        }

        // ==================== خدمة 2: إصدار شهادة سابر ====================
        private string HandleSaberCertificate(UserSession session, string messageText)
        {
            switch (session.CurrentStep)
            {
                case 1: // نوع المنتج
                    var productType = messageText.Trim();
                    // منع الكلمات العامة
                    if (productType.Length < 3 || 
                        productType == "أجهزة" || productType == "منتجات" || 
                        productType == "بضاعة" || productType == "شي")
                    {
                        return "❌ يرجى ذكر النوع بشكل محدد.\n\nمثال: لمبات LED، خلاط كهربائي، كابلات USB\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["product_type"] = productType;
                    session.CurrentStep = 2;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام نوع المنتج\n\n2️⃣ هل يتوفر رمز HS؟\n\nأرسل الرقم أو اكتب: لا\n\n" + GetNavigationFooter();

                case 2: // رمز HS
                    var hsCode = messageText.Trim();
                    if (hsCode.ToLower() == "لا" || hsCode.ToLower() == "لايوجد" || hsCode.ToLower() == "ما عندي")
                    {
                        session.CollectedData["hs_code"] = "غير متوفر";
                    }
                    else if (Regex.IsMatch(hsCode, @"^\d{4,10}$"))
                    {
                        session.CollectedData["hs_code"] = hsCode;
                    }
                    else
                    {
                        return "❌ رمز HS يجب أن يكون رقمي (4-10 أرقام) أو اكتب: لا\n\n" + GetNavigationFooter();
                    }
                    session.CurrentStep = 3;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم الاستلام\n\n3️⃣ أرسل اسم المصنع أو المورد\n\n" + GetNavigationFooter();

                case 3: // اسم المصنع
                    var manufacturer = messageText.Trim();
                    if (manufacturer.Length < 2 || manufacturer == "شركة" || manufacturer == "مصنع")
                    {
                        return "❌ الرجاء إرسال اسم المصنع أو المورد بشكل كامل\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["manufacturer"] = manufacturer;
                    session.CurrentStep = 4;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام اسم المصنع\n\n4️⃣ أرسل الفاتورة أو عرض السعر\n\n(يمكنك إرسال صورة أو ملف أو كتابة \"تم الإرسال\")";

                case 4: // الفاتورة
                    if (string.IsNullOrWhiteSpace(messageText) || messageText.Trim().Length < 2)
                    {
                        return "❌ الرجاء إرسال الفاتورة أو عرض السعر أو اكتب \"تم الإرسال\"";
                    }
                    session.CollectedData["invoice"] = "تم الاستلام";

                    var summary2 = $@"✅ تم استلام جميع البيانات بنجاح!

📋 ملخص طلبك:
━━━━━━━━━━━━━━━━
🔹 الخدمة: إصدار شهادة سابر
🔹 نوع المنتج: {session.CollectedData["product_type"]}
🔹 رمز HS: {session.CollectedData["hs_code"]}
🔹 المصنع: {session.CollectedData["manufacturer"]}
🔹 الفاتورة: تم الاستلام

⏳ ستصدر الشهادة بعد المراجعة والمطابقة من الجهة المختصة.
📞 سيتم التواصل معك قريباً.

شكراً لاختيارك نخبة المنافذ 🌟

للعودة للقائمة الرئيسية، اكتب: قائمة";

                    _sessionManager.ResetSession(session.PhoneNumber);
                    return summary2;

                default:
                    return "حدث خطأ. الرجاء البدء من جديد.\n\n" + GetNavigationFooter();
            }
        }

        // ==================== خدمة 3: الشحن الدولي ====================
        private string HandleInternationalShipping(UserSession session, string messageText)
        {
            switch (session.CurrentStep)
            {
                case 1: // مدينة الشحن
                    var city = messageText.Trim();
                    if (city.Length < 3)
                    {
                        return "❌ الرجاء كتابة اسم المدينة بشكل واضح\n\n" + GetNavigationFooter();
                    }
                    // التحقق من أنه ليس دولة فقط
                    if (city.ToLower() == "السعودية" || city.ToLower() == "سعودية" || 
                        city.ToLower() == "مصر" || city.ToLower() == "الإمارات")
                    {
                        return "❌ الرجاء تحديد اسم المدينة وليس الدولة فقط\n\nمثال: الرياض، جدة، دبي، القاهرة\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["shipping_city"] = city;
                    session.CurrentStep = 2;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام المدينة\n\n2️⃣ ما نوع البضاعة؟\n\n" + GetNavigationFooter();

                case 2: // نوع البضاعة
                    var goodsType = messageText.Trim();
                    if (goodsType.Length < 3)
                    {
                        return "❌ الرجاء تحديد نوع البضاعة بشكل واضح\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["goods_type"] = goodsType;
                    session.CurrentStep = 3;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام نوع البضاعة\n\n3️⃣ كم الوزن التقريبي بالكيلو؟\n\n" + GetNavigationFooter();

                case 3: // الوزن
                    var weight = messageText.Trim().Replace("كيلو", "").Replace("كجم", "").Replace("kg", "").Trim();
                    if (!Regex.IsMatch(weight, @"^\d+(\.\d+)?$"))
                    {
                        return "❌ الرجاء إدخال الوزن بالأرقام فقط\n\nمثال: 100 أو 50.5\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["weight"] = weight + " كيلو";
                    session.CurrentStep = 4;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام الوزن\n\n4️⃣ تفضل الشحن البحري أم الجوي؟\n\n• بحري\n• جوي\n\n" + GetNavigationFooter();

                case 4: // نوع الشحن
                    var shippingType = messageText.Trim().ToLower();
                    if (!shippingType.Contains("بحري") && !shippingType.Contains("جوي") && 
                        !shippingType.Contains("بحر") && !shippingType.Contains("جو"))
                    {
                        return "❌ الرجاء الاختيار بين:\n• بحري\n• جوي\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["shipping_type"] = shippingType.Contains("بحر") ? "بحري" : "جوي";

                    var summary3 = $@"✅ تم استلام جميع البيانات بنجاح!

📋 ملخص طلبك:
━━━━━━━━━━━━━━━━
🔹 الخدمة: الشحن الدولي
🔹 مدينة الشحن: {session.CollectedData["shipping_city"]}
🔹 نوع البضاعة: {session.CollectedData["goods_type"]}
🔹 الوزن: {session.CollectedData["weight"]}
🔹 نوع الشحن: {session.CollectedData["shipping_type"]}

⏳ سيتم إرسال عرض السعر وخيارات المدة مباشرة.
📞 سيتم التواصل معك قريباً.

شكراً لاختيارك نخبة المنافذ 🌟

للعودة للقائمة الرئيسية، اكتب: قائمة";

                    _sessionManager.ResetSession(session.PhoneNumber);
                    return summary3;

                default:
                    return "حدث خطأ. الرجاء البدء من جديد.\n\n" + GetNavigationFooter();
            }
        }

        // ==================== خدمة 4: التصدير ====================
        private string HandleExport(UserSession session, string messageText)
        {
            switch (session.CurrentStep)
            {
                case 1: // نوع المنتج
                    var productType = messageText.Trim();
                    if (productType.Length < 3)
                    {
                        return "❌ الرجاء تحديد نوع المنتج بشكل واضح\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["product_type"] = productType;
                    session.CurrentStep = 2;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام نوع المنتج\n\n2️⃣ ما الدولة المستوردة؟\n\n(حدد اسم الدولة بالضبط)\n\n" + GetNavigationFooter();

                case 2: // الدولة المستوردة
                    var country = messageText.Trim();
                    // منع الإجابات العامة
                    if (country.ToLower() == "أوروبا" || country.ToLower() == "آسيا" || 
                        country.ToLower() == "أفريقيا" || country.ToLower() == "الخليج")
                    {
                        return "❌ الرجاء تحديد اسم الدولة بالضبط\n\nمثال: الإمارات، مصر، تركيا، ألمانيا\n\n" + GetNavigationFooter();
                    }
                    if (country.Length < 3)
                    {
                        return "❌ الرجاء كتابة اسم الدولة بشكل واضح\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["destination_country"] = country;
                    session.CurrentStep = 3;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام الدولة\n\n3️⃣ ما الكمية والوزن؟\n\n(مثال: 500 كيلو أو 100 قطعة)\n\n" + GetNavigationFooter();

                case 3: // الكمية والوزن
                    var quantity = messageText.Trim();
                    if (quantity.Length < 2 || !Regex.IsMatch(quantity, @"\d+"))
                    {
                        return "❌ الرجاء تحديد الكمية أو الوزن بشكل واضح\n\nمثال: 500 كيلو أو 100 قطعة\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["quantity"] = quantity;
                    session.CurrentStep = 4;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام الكمية\n\n4️⃣ هل تحتاج شهادة منشأ؟\n\n• نعم\n• لا\n\n" + GetNavigationFooter();

                case 4: // شهادة المنشأ
                    var needsCertificate = messageText.Trim().ToLower();
                    if (!needsCertificate.Contains("نعم") && !needsCertificate.Contains("لا"))
                    {
                        return "❌ الرجاء الإجابة بـ:\n• نعم\n• لا\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["origin_certificate"] = needsCertificate.Contains("نعم") ? "نعم" : "لا";

                    var summary4 = $@"✅ تم استلام جميع البيانات بنجاح!

📋 ملخص طلبك:
━━━━━━━━━━━━━━━━
🔹 الخدمة: التصدير
🔹 نوع المنتج: {session.CollectedData["product_type"]}
🔹 الدولة المستوردة: {session.CollectedData["destination_country"]}
🔹 الكمية: {session.CollectedData["quantity"]}
🔹 شهادة منشأ: {session.CollectedData["origin_certificate"]}

⏳ سنجهز إجراءات التصدير والشحن بالكامل.
📞 سيتم التواصل معك قريباً.

شكراً لاختيارك نخبة المنافذ 🌟

للعودة للقائمة الرئيسية، اكتب: قائمة";

                    _sessionManager.ResetSession(session.PhoneNumber);
                    return summary4;

                default:
                    return "حدث خطأ. الرجاء البدء من جديد.\n\n" + GetNavigationFooter();
            }
        }

        // ==================== خدمة 5: النقل المحلي ====================
        private string HandleLocalTransport(UserSession session, string messageText)
        {
            switch (session.CurrentStep)
            {
                case 1: // موقع الاستلام
                    var pickupLocation = messageText.Trim();
                    // التحقق من وجود مدينة + حي
                    if (pickupLocation.Length < 5 || !pickupLocation.Contains(" "))
                    {
                        return "❌ الرجاء تحديد الموقع بشكل كامل (مدينة + حي)\n\nمثال: الرياض - حي النخيل\nأو: جدة - حي الروضة\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["pickup_location"] = pickupLocation;
                    session.CurrentStep = 2;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام موقع الاستلام\n\n2️⃣ حدد موقع التسليم (مدينة + حي)\n\n" + GetNavigationFooter();

                case 2: // موقع التسليم
                    var deliveryLocation = messageText.Trim();
                    if (deliveryLocation.Length < 5 || !deliveryLocation.Contains(" "))
                    {
                        return "❌ الرجاء تحديد الموقع بشكل كامل (مدينة + حي)\n\nمثال: الرياض - حي العليا\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["delivery_location"] = deliveryLocation;
                    session.CurrentStep = 3;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام موقع التسليم\n\n3️⃣ ما نوع الحمولة؟\n\n" + GetNavigationFooter();

                case 3: // نوع الحمولة
                    var cargoType = messageText.Trim();
                    if (cargoType.Length < 3)
                    {
                        return "❌ الرجاء تحديد نوع الحمولة بشكل واضح\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["cargo_type"] = cargoType;
                    session.CurrentStep = 4;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام نوع الحمولة\n\n4️⃣ ما الوقت المطلوب للتحميل؟\n\n(مثال: اليوم الساعة 3 مساءً، غداً صباحاً، الأحد 10 صباحاً)\n\n" + GetNavigationFooter();

                case 4: // وقت التحميل
                    var loadingTime = messageText.Trim();
                    // منع الإجابات غير الواضحة
                    if (loadingTime.Length < 4 || 
                        loadingTime.ToLower() == "بعد شوي" || 
                        loadingTime.ToLower() == "قريب" ||
                        loadingTime.ToLower() == "الحين")
                    {
                        return "❌ الرجاء تحديد الوقت بشكل واضح\n\nمثال:\n• اليوم الساعة 3 مساءً\n• غداً صباحاً\n• الأحد 10 صباحاً\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["loading_time"] = loadingTime;

                    var summary5 = $@"✅ تم استلام جميع البيانات بنجاح!

📋 ملخص طلبك:
━━━━━━━━━━━━━━━━
🔹 الخدمة: النقل المحلي
🔹 موقع الاستلام: {session.CollectedData["pickup_location"]}
🔹 موقع التسليم: {session.CollectedData["delivery_location"]}
🔹 نوع الحمولة: {session.CollectedData["cargo_type"]}
🔹 وقت التحميل: {session.CollectedData["loading_time"]}

⏳ سنوفر الشاحنة المناسبة ونرتب الحركة.
📞 سيتم التواصل معك قريباً.

شكراً لاختيارك نخبة المنافذ 🌟

للعودة للقائمة الرئيسية، اكتب: قائمة";

                    _sessionManager.ResetSession(session.PhoneNumber);
                    return summary5;

                default:
                    return "حدث خطأ. الرجاء البدء من جديد.\n\n" + GetNavigationFooter();
            }
        }

        // ==================== خدمة 6: التخزين ====================
        private string HandleStorage(UserSession session, string messageText)
        {
            switch (session.CurrentStep)
            {
                case 1: // نوع البضاعة
                    var goodsType = messageText.Trim();
                    if (goodsType.Length < 3)
                    {
                        return "❌ الرجاء تحديد نوع البضاعة بشكل واضح\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["goods_type"] = goodsType;
                    session.CurrentStep = 2;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام نوع البضاعة\n\n2️⃣ ما الحجم أو عدد الطبليات؟\n\n(مثال: 10 طبليات، 50 متر مكعب)\n\n" + GetNavigationFooter();

                case 2: // الحجم
                    var size = messageText.Trim();
                    if (!Regex.IsMatch(size, @"\d+"))
                    {
                        return "❌ الرجاء تحديد الحجم أو العدد بشكل واضح\n\nمثال: 10 طبليات، 50 متر مكعب\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["size"] = size;
                    session.CurrentStep = 3;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام الحجم\n\n3️⃣ ما مدة التخزين؟\n\n(حدد المدة بالأيام أو الأسابيع أو الأشهر)\n\n" + GetNavigationFooter();

                case 3: // مدة التخزين
                    var duration = messageText.Trim();
                    if (!Regex.IsMatch(duration, @"\d+") || duration.Length < 2)
                    {
                        return "❌ الرجاء تحديد المدة بشكل واضح\n\nمثال:\n• 7 أيام\n• أسبوعين\n• شهر\n• 3 أشهر\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["duration"] = duration;

                    var summary6 = $@"✅ تم استلام جميع البيانات بنجاح!

📋 ملخص طلبك:
━━━━━━━━━━━━━━━━
🔹 الخدمة: التخزين
🔹 نوع البضاعة: {session.CollectedData["goods_type"]}
🔹 الحجم: {session.CollectedData["size"]}
🔹 مدة التخزين: {session.CollectedData["duration"]}

⏳ سنوفر مخازن آمنة ومتابعة يومية.
📞 سيتم التواصل معك قريباً.

شكراً لاختيارك نخبة المنافذ 🌟

للعودة للقائمة الرئيسية، اكتب: قائمة";

                    _sessionManager.ResetSession(session.PhoneNumber);
                    return summary6;

                default:
                    return "حدث خطأ. الرجاء البدء من جديد.\n\n" + GetNavigationFooter();
            }
        }

        // ==================== خدمة 7: التعبئة والتغليف ====================
        private string HandlePackaging(UserSession session, string messageText)
        {
            switch (session.CurrentStep)
            {
                case 1: // نوع البضاعة
                    var goodsType = messageText.Trim();
                    if (goodsType.Length < 3)
                    {
                        return "❌ الرجاء تحديد نوع البضاعة بشكل واضح\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["goods_type"] = goodsType;
                    session.CurrentStep = 2;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام نوع البضاعة\n\n2️⃣ كم عدد القطع؟\n\n" + GetNavigationFooter();

                case 2: // عدد القطع
                    var quantity = messageText.Trim().Replace("قطعة", "").Replace("قطع", "").Trim();
                    if (!Regex.IsMatch(quantity, @"^\d+$"))
                    {
                        return "❌ الرجاء إدخال عدد القطع بالأرقام فقط\n\nمثال: 100\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["quantity"] = quantity + " قطعة";
                    session.CurrentStep = 3;
                    _sessionManager.UpdateSession(session);
                    return "✅ تم استلام العدد\n\n3️⃣ ما نوع التغليف المطلوب؟\n\n• أساسي\n• شحن\n• حماية إضافية\n\n" + GetNavigationFooter();

                case 3: // نوع التغليف
                    var packagingType = messageText.Trim().ToLower();
                    if (!packagingType.Contains("أساسي") && !packagingType.Contains("شحن") && 
                        !packagingType.Contains("حماية") && !packagingType.Contains("إضافية"))
                    {
                        return "❌ الرجاء الاختيار من:\n• أساسي\n• شحن\n• حماية إضافية\n\n" + GetNavigationFooter();
                    }
                    
                    string selectedType = "أساسي";
                    if (packagingType.Contains("شحن")) selectedType = "شحن";
                    else if (packagingType.Contains("حماية") || packagingType.Contains("إضافية")) selectedType = "حماية إضافية";
                    
                    session.CollectedData["packaging_type"] = selectedType;

                    var summary7 = $@"✅ تم استلام جميع البيانات بنجاح!

📋 ملخص طلبك:
━━━━━━━━━━━━━━━━
🔹 الخدمة: التعبئة والتغليف
🔹 نوع البضاعة: {session.CollectedData["goods_type"]}
🔹 عدد القطع: {session.CollectedData["quantity"]}
🔹 نوع التغليف: {session.CollectedData["packaging_type"]}

⏳ سنقدم عرض السعر ونبدأ التنفيذ.
📞 سيتم التواصل معك قريباً.

شكراً لاختيارك نخبة المنافذ 🌟

للعودة للقائمة الرئيسية، اكتب: قائمة";

                    _sessionManager.ResetSession(session.PhoneNumber);
                    return summary7;

                default:
                    return "حدث خطأ. الرجاء البدء من جديد.\n\n" + GetNavigationFooter();
            }
        }

        // ==================== خدمة 8: الفعاليات والمعارض ====================
        private string HandleEventsExhibitions(UserSession session, string messageText)
        {
            switch (session.CurrentStep)
            {
                case 1: // نوع الفعالية
                    var eventType = messageText.Trim();
                    if (eventType.Length < 3)
                    {
                        return "❌ الرجاء تحديد نوع الفعالية أو المعرض بشكل واضح\n\nمثال: معرض تجاري، مؤتمر، حفل افتتاح\n\n" + GetNavigationFooter();
                    }
                    session.CollectedData["event_type"] = eventType;

                    var summary8 = $@"✅ تم استلام البيانات بنجاح!

📋 ملخص طلبك:
━━━━━━━━━━━━━━━━
🔹 الخدمة: الفعاليات والمعارض
🔹 نوع الفعالية: {session.CollectedData["event_type"]}

⏳ سنتواصل معك لتفاصيل الخدمة المطلوبة.
📞 سيتم التواصل معك قريباً.

شكراً لاختيارك نخبة المنافذ 🌟

للعودة للقائمة الرئيسية، اكتب: قائمة";

                    _sessionManager.ResetSession(session.PhoneNumber);
                    return summary8;

                default:
                    return "حدث خطأ. الرجاء البدء من جديد.\n\n" + GetNavigationFooter();
            }
        }
    }
}

