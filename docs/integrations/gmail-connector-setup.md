# Private Gmail connector setup

1. Create or select a private Google Cloud project.
2. Enable **Gmail API** under APIs & Services / Library.
3. Open **Google Auth Platform** and configure Branding.
4. Set Audience to **External**, keep the app in **Testing**, and add the Gmail account under **Test users**.
5. Under Data Access add only `https://www.googleapis.com/auth/gmail.readonly`.
6. Under Clients create an OAuth client with application type **Desktop app**.
7. Launch LifeOS once and open Email Radar, then click **Open Gmail configuration folder**.
8. Edit `%LOCALAPPDATA%\LifeOS\connectors\gmail\configuration.json`:

```json
{
  "ClientId": "YOUR_DESKTOP_CLIENT_ID.apps.googleusercontent.com",
  "ClientSecret": "YOUR_DESKTOP_CLIENT_SECRET",
  "RedirectUri": "http://127.0.0.1:53683/"
}
```

9. Save, return to LifeOS, click **Connect Gmail read-only**, and complete browser consent.
10. Use **Preview bounded Gmail search** before **Confirm manual Gmail search**.

Keep the project private/testing-mode. Do not commit the configuration or token cache.
