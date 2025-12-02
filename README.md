# CustomsBot - WhatsApp Business API Bot

ASP.NET Core Web API bot for WhatsApp Business API integration.

## Features
- WhatsApp webhook verification
- Message receiving endpoint
- Ready for deployment on Railway.app

## Deployment

### Railway.app
1. Push code to GitHub
2. Connect Railway to your repository
3. Add environment variables in Railway dashboard
4. Deploy automatically

### Environment Variables
Set these in Railway dashboard:
- `WhatsAppSettings__AccessToken`: Your WhatsApp access token
- `WhatsAppSettings__PhoneNumberId`: Your phone number ID
- `WhatsAppSettings__VerifyToken`: Your webhook verify token

## Local Development
```bash
cd CustomsBot
dotnet run
```

## Webhook URL
After deployment, your webhook URL will be:
```
https://your-app.railway.app/api/whatsapp/webhook
```
