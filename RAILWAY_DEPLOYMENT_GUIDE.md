# 🚀 دليل نشر المشروع على Railway

## ✅ الإعداد خلص! المشروع جاهز للنشر

---

## 📋 الخطوات (10 دقايق):

### الخطوة 1️⃣: إنشاء Git Repository

#### أ) افتح Terminal في مجلد المشروع واكتب:
```bash
git init
git add .
git commit -m "Initial commit - WhatsApp Bot"
```

#### ب) إنشاء repository على GitHub:
1. روح على: https://github.com/new
2. اسم الـ repo: `CustomsBot` (أو أي اسم تحبه)
3. اختار **Private** (مهم عشان الـ tokens)
4. اضغط **Create repository**

#### ج) ارفع الكود على GitHub:
```bash
git remote add origin https://github.com/YOUR_USERNAME/CustomsBot.git
git branch -M main
git push -u origin main
```

---

### الخطوة 2️⃣: إنشاء حساب على Railway

1. روح على: https://railway.app
2. اضغط **Login**
3. سجل دخول بـ **GitHub** (أسهل طريقة)
4. اضغط **Authorize Railway**

---

### الخطوة 3️⃣: إنشاء Project جديد

1. من Dashboard، اضغط **New Project**
2. اختار **Deploy from GitHub repo**
3. اختار الـ repository: `CustomsBot`
4. اضغط **Deploy Now**

Railway هيبدأ يبني المشروع تلقائياً! ⏳

---

### الخطوة 4️⃣: إضافة Environment Variables

بعد ما الـ deployment يخلص:

1. اضغط على الـ **service** اللي اتعمل
2. روح على تاب **Variables**
3. اضغط **+ New Variable** وضيف:

```
WhatsAppSettings__AccessToken=EAA5um1kSkh8BQMRoaMiA39P79WknLEHOroZAX8GxsNmhBz9UrZATyXLb8O6ECXJMaIrywp2vHEGRST0DYB6NAjFBxD7M53fEyDp56b7ZCtujZANA0GZAlArNom59vReyXhZAFv7CW4iazltNV3rZBfqkEZBBgM0CUcCr7bNALLdM5qK1WrmBjuZA87PZCACLKK3oo1M8LEZAzYG0jhqZAhhyfq08KREramOb1Prv9bPrzsekOQZAYwY3iDR7x6zT3tR406OZBRksl6sydZBRaRpmr8DnNDG

WhatsAppSettings__PhoneNumberId=948397325016135

WhatsAppSettings__VerifyToken=my_secret_token_123
```

4. اضغط **Add** لكل variable

---

### الخطوة 5️⃣: الحصول على الـ URL

1. روح على تاب **Settings**
2. في قسم **Networking**، اضغط **Generate Domain**
3. Railway هيديك URL شكله: `https://customsbot-production-xxxx.up.railway.app`

**انسخ اللينك ده! 📋**

---

### الخطوة 6️⃣: ربط الـ Webhook مع Meta

1. روح على: https://developers.facebook.com/apps
2. اختار الـ App بتاعك
3. روح **WhatsApp > Configuration**
4. في قسم **Webhook**:

**Callback URL:**
```
https://your-app.up.railway.app/api/whatsapp/webhook
```

**Verify Token:**
```
my_secret_token_123
```

5. اضغط **Verify and Save** ✅

---

## 🎉 تم! البوت شغال 24/7

### ✅ المميزات:
- البوت شغال 24/7 بدون ما تشغل جهازك
- HTTPS URL ثابت (مش هيتغير)
- Automatic deployments (لو عدلت الكود وعملت push)
- Logs متاحة في Railway Dashboard

---

## 💰 التكلفة:

- **أول 500 ساعة**: مجاني
- **بعد كده**: $5/شهر تقريباً
- تقدر تشوف الاستهلاك من Dashboard

---

## 🔍 مراقبة البوت:

### في Railway Dashboard:
- **Deployments**: شوف الـ deployment history
- **Logs**: شوف الـ console output
- **Metrics**: شوف الـ CPU/Memory usage

---

## 🆘 لو حصلت مشكلة:

### البوت مش شغال:
1. شوف الـ **Logs** في Railway
2. تأكد إن الـ **Environment Variables** مضبوطة
3. تأكد إن الـ **Domain** متولد

### Meta مش قادرة تتحقق:
1. تأكد إن الـ URL صحيح
2. تأكد إن الـ Verify Token مطابق
3. جرب الـ URL في المتصفح

---

## 📞 محتاج مساعدة؟
قولي في أي خطوة وأنا معاك! 🚀
