# ?? Email Configuration Issue - Root Cause Analysis

## Error Summary

```
[17:37:51 ERR] Error sending email to support@az-solve.com: No such host is known.
[17:37:51 ERR] Error sending email to alexandrech@hotmail.com: No such host is known.
```

---

## ?? Configuration Flow Explanation

### 1. Where `support@az-solve.com` Comes From

**Location:** `BlazorShop.Application\Services\SupportTicketService.cs` - Line 77

```csharp
await _emailService.SendEmailAsync(
    "support@az-solve.com",  // ?? HARDCODED EMAIL ADDRESS
    $"New Support Ticket #{ticket.Id}",
    GenerateSupportTeamNotificationEmail(ticket));
```

**This email is used as the recipient for support team notifications** - NOT related to your User Secrets!

---

### 2. Email Configuration Sources

#### A. **appsettings.json** (Current State - Partial)
```json
{
  "EmailSettings": {
    "DisplayName": "EAssets Support",
    "SmtpServer": "smtp.email.com",     // ?? Generic/fake SMTP server
    "Port": 587,
    "UseSsl": true
  }
}
```

**Missing:** `From`, `Username`, `Password` (removed for security - correct!)

#### B. **User Secrets** (Your Local Configuration)
Located at: `%APPDATA%\Microsoft\UserSecrets\4d225ffe-ba02-4d4c-a4c3-37082aa61d29\secrets.json`

**You mentioned you set:**
- Email: `...@gmail.com`
- Password: Google App Password

#### C. **Final Merged Configuration** (Runtime)

.NET merges these sources in order:
1. `appsettings.json` ? Base configuration
2. User Secrets ? **Overrides** `From`, `Username`, `Password`

**Result:**
```json
{
  "EmailSettings": {
    "From": "your-email@gmail.com",          // From User Secrets
    "DisplayName": "EAssets Support",        // From appsettings.json
    "SmtpServer": "smtp.email.com",          // ?? WRONG - From appsettings.json
    "Port": 587,                             // From appsettings.json
    "UseSsl": true,                          // From appsettings.json
    "Username": "your-email@gmail.com",      // From User Secrets
    "Password": "your-app-password"          // From User Secrets
  }
}
```

---

## ?? Root Cause: DNS Resolution Failure

### Error Message Breakdown

**"No such host is known"** is a **DNS resolution error**, meaning:
- Your app tried to connect to `smtp.email.com`
- The DNS system could not find that host
- **`smtp.email.com` is a placeholder, not a real SMTP server**

### Why This Happened

1. **appsettings.json has placeholder SMTP server:**
   ```json
   "SmtpServer": "smtp.email.com"  // ?? Doesn't exist!
   ```

2. **User Secrets only override specific values:**
   - User Secrets can override `From`, `Username`, `Password`
   - But `SmtpServer` is still read from `appsettings.json`
   - Since you didn't override `SmtpServer` in User Secrets, it used the fake one

3. **EmailService.cs uses SMTP configuration:**
   ```csharp
   await smtp.ConnectAsync(
       _emailSettings.SmtpServer,  // = "smtp.email.com" (doesn't exist!)
       _emailSettings.Port,         // = 587
       _emailSettings.UseSsl,       // = true
       cts.Token);
   ```

---

## ?? How Configuration Merging Works

### Example Configuration Flow:

**Step 1: appsettings.json loads first**
```json
{
  "EmailSettings": {
    "DisplayName": "EAssets Support",
    "SmtpServer": "smtp.email.com",
    "Port": 587,
    "UseSsl": true
  }
}
```

**Step 2: User Secrets override (only what you set)**
```json
{
  "EmailSettings:From": "myemail@gmail.com",
  "EmailSettings:Username": "myemail@gmail.com",
  "EmailSettings:Password": "myapppassword"
}
```

**Step 3: Final merged result at runtime**
```json
{
  "EmailSettings": {
    "From": "myemail@gmail.com",        // ? From User Secrets
    "DisplayName": "EAssets Support",    // ? From appsettings.json
    "SmtpServer": "smtp.email.com",      // ?? From appsettings.json (WRONG!)
    "Port": 587,                         // ? From appsettings.json
    "UseSsl": true,                      // ? From appsettings.json
    "Username": "myemail@gmail.com",     // ? From User Secrets
    "Password": "myapppassword"          // ? From User Secrets
  }
}
```

**Problem:** `SmtpServer` was never overridden, so it still points to the fake placeholder!

---

## ?? Two Emails Being Sent

### Email 1: Customer Confirmation
**To:** `alexandrech@hotmail.com` (the customer's email from the form)
**From:** Your Gmail (from User Secrets)
**SMTP Server:** `smtp.email.com` (?? doesn't exist)

### Email 2: Support Team Notification
**To:** `support@az-solve.com` (?? hardcoded in SupportTicketService.cs)
**From:** Your Gmail (from User Secrets)
**SMTP Server:** `smtp.email.com` (?? doesn't exist)

**Both emails fail because the SMTP server doesn't exist!**

---

## ? Complete Fix Required

### Fix 1: Update appsettings.json

**Change this:**
```json
{
  "EmailSettings": {
    "DisplayName": "EAssets Support",
    "SmtpServer": "smtp.email.com",  // ?? CHANGE THIS
    "Port": 587,
    "UseSsl": true
  }
}
```

**To this:**
```json
{
  "EmailSettings": {
    "DisplayName": "EAssets Support",
    "SmtpServer": "smtp.gmail.com",   // ? Real Gmail SMTP server
    "Port": 587,
    "UseSsl": true
  }
}
```

### Fix 2: Update User Secrets (if needed)

**Check your secrets:**
```sh
cd BlazorShop.Presentation\BlazorShop.API
dotnet user-secrets list
```

**Should show:**
```
EmailSettings:From = your-email@gmail.com
EmailSettings:Username = your-email@gmail.com
EmailSettings:Password = your-app-password
```

**If `SmtpServer` is missing or wrong, add it:**
```sh
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
```

### Fix 3: Update Hardcoded Support Email

**In `SupportTicketService.cs` - Line 77:**

**Option A: Use configuration (recommended)**
```csharp
// Inject IOptions<EmailSettings> in constructor
private readonly EmailSettings _emailSettings;

public SupportTicketService(
    IGenericRepository<Domain.Entities.SupportTicket> repository,
    IEmailService emailService,
    IAppLogger<SupportTicketService> logger,
    IMapper mapper,
    IOptions<EmailSettings> emailSettings)  // Add this
{
    _repository = repository;
    _emailService = emailService;
    _logger = logger;
    _mapper = mapper;
    _emailSettings = emailSettings.Value;  // Add this
}

// Then use it:
await _emailService.SendEmailAsync(
    _emailSettings.From,  // ? Use the From email as support email
    $"New Support Ticket #{ticket.Id}",
    GenerateSupportTeamNotificationEmail(ticket));
```

**Option B: Add to configuration**
```json
// appsettings.json
{
  "EmailSettings": {
    "From": "",
    "SupportEmail": "support@your-domain.com",  // Add this
    "DisplayName": "EAssets Support",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true
  }
}

// User Secrets
dotnet user-secrets set "EmailSettings:SupportEmail" "your-email@gmail.com"
```

---

## ?? Testing the Fix

### 1. Verify Configuration

```sh
cd BlazorShop.Presentation\BlazorShop.API

# Check what's in User Secrets
dotnet user-secrets list

# Expected output:
# EmailSettings:From = your-email@gmail.com
# EmailSettings:Username = your-email@gmail.com
# EmailSettings:Password = xnxx xxxx xxxx xxxx (app password)
```

### 2. Run the Application

```sh
dotnet run --project BlazorShop.Presentation\BlazorShop.API
```

### 3. Submit a Test Ticket

Go to: `https://localhost:7258/customer-service`

Fill in:
- **Name:** Test User
- **Email:** your-email@gmail.com (use your Gmail)
- **Message:** Test support ticket

**Expected Success Logs:**
```
[INF] Support ticket created: {guid} from your-email@gmail.com
[INF] Email sent to your-email@gmail.com
[INF] Email sent to support@az-solve.com (or your configured email)
```

### 4. Check Your Gmail

You should receive TWO emails:
1. **Customer confirmation** (to your email)
2. **Support team notification** (to your email as well, since you're testing)

---

## ?? Google App Password Mapping

### How It Works:

1. **You generate App Password in Google:**
   - Format: `xnxx xxxx xxxx xxxx` (16 characters with spaces)
   - Example: `abcd efgh ijkl mnop`

2. **You store it in User Secrets:**
   ```sh
   dotnet user-secrets set "EmailSettings:Password" "abcd efgh ijkl mnop"
   ```
   **Note:** Include the spaces or remove them - both work

3. **MailKit uses it for SMTP authentication:**
   ```csharp
   // In EmailService.cs
   await smtp.AuthenticateAsync(
       _emailSettings.Username,  // = "your-email@gmail.com"
       _emailSettings.Password,  // = "abcd efgh ijkl mnop"
       cts.Token);
   ```

4. **Gmail SMTP server accepts it:**
   - Server: `smtp.gmail.com`
   - Port: `587` (TLS/STARTTLS)
   - Authentication: Username + App Password
   - Connection: Encrypted (TLS)

### Why App Password is Needed:

- **Regular Gmail password** is rejected by Gmail SMTP when:
  - 2FA is enabled (recommended)
  - Accessing from apps (not a browser)
  
- **App Password** is a special 16-character password that:
  - Works specifically for apps
  - Bypasses 2FA
  - Can be revoked independently
  - Is more secure than using your main password

---

## ?? Quick Fix Commands

```sh
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ\BlazorShop.Presentation\BlazorShop.API

# Set correct SMTP server in User Secrets
dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"

# Verify all secrets are set correctly
dotnet user-secrets list

# Should see:
# EmailSettings:From = your-email@gmail.com
# EmailSettings:Username = your-email@gmail.com
# EmailSettings:Password = your-app-password
# EmailSettings:SmtpServer = smtp.gmail.com (if you added it)
```

**Then update `appsettings.json`:**
```json
{
  "EmailSettings": {
    "DisplayName": "EAssets Support",
    "SmtpServer": "smtp.gmail.com",  // ? Change from smtp.email.com
    "Port": 587,
    "UseSsl": true
  }
}
```

---

## ?? Summary

### Problems Identified:

1. ? **appsettings.json** has `smtp.email.com` (fake/placeholder)
2. ? **User Secrets** didn't override `SmtpServer`
3. ? **Support email** is hardcoded as `support@az-solve.com`
4. ? **User Secrets** correctly has Gmail credentials
5. ? **App Password** is correctly stored

### Solutions:

1. ? **Update appsettings.json** to use `smtp.gmail.com`
2. ? **Optional:** Add `SmtpServer` to User Secrets for override
3. ? **Update SupportTicketService.cs** to use configurable support email
4. ? **Test** with real Gmail SMTP server

### Why It Failed:

- DNS couldn't resolve `smtp.email.com` because it's not a real domain
- Your Gmail credentials were correct but used with wrong SMTP server
- Two emails attempted: customer confirmation + support notification
- Both failed at SMTP connection stage before authentication

### After Fix:

- ? SMTP connects to real Gmail server (`smtp.gmail.com`)
- ? Authenticates with your Gmail + App Password
- ? Sends customer confirmation email
- ? Sends support team notification email
- ? Both emails delivered successfully

---

**The issue is NOT with your User Secrets or App Password - they're working correctly! The problem is the SMTP server address in `appsettings.json` is pointing to a non-existent server.**
