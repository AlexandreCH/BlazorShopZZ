# EmailSettings Configuration & Security Review

## ?? Current Configuration Analysis

### ? EmailSettings DTO Structure
**File:** `BlazorShop.Application\DTOs\EmailSettings.cs`

```csharp
public class EmailSettings
{
    public string From { get; set; }          // Sender email address
    public string DisplayName { get; set; }   // Display name in emails
    public string SmtpServer { get; set; }    // SMTP server address
    public int Port { get; set; }             // SMTP port (587 or 465)
    public bool UseSsl { get; set; }          // SSL/TLS encryption
    public string Username { get; set; }      // SMTP authentication username
    public string Password { get; set; }      // SMTP authentication password
}
```

**Status:** ? Structure is correct and complete

---

## ?? Security Issues Found

### Current `appsettings.json` Configuration

```json
"EmailSettings": {
  "From": "support@az-solve.com",
  "DisplayName": "EAssets",
  "SmtpServer": "smtp.email.com",
  "Port": 587,
  "UseSsl": false,                          // ?? Should be true
  "Username": "shop@blazorshop_xx.com",     // ?? Exposed in source control
  "Password": "password"                     // ?? CRITICAL: Hardcoded password!
}
```

### ?? Critical Security Problems:

1. **Hardcoded Password** - Plain text password in `appsettings.json`
2. **SSL Disabled** - `UseSsl: false` means unencrypted connection
3. **Committed to Git** - Sensitive data exposed in version control
4. **Inconsistent Emails** - Multiple email addresses (support@az-solve.com vs shop@blazorshop_xx.com)
5. **No User Secrets** - Project not configured for User Secrets

---

## ? Recommended Configuration

### 1. Structure Overview

```
appsettings.json              ? Non-sensitive defaults (committed to Git)
appsettings.Development.json  ? Development overrides (committed to Git)
secrets.json                  ? Sensitive data (NOT in Git, local only)
Azure Key Vault               ? Production secrets (cloud-based)
```

### 2. What Should Go Where

#### `appsettings.json` (Safe to commit)
```json
{
  "EmailSettings": {
    "DisplayName": "BlazorShop Support",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true
  }
}
```

#### User Secrets (Local development only)
```json
{
  "EmailSettings": {
    "From": "your-email@gmail.com",
    "Username": "your-email@gmail.com",
    "Password": "your-app-specific-password"
  }
}
```

#### Environment Variables (Production)
```bash
EmailSettings__From=noreply@blazorshop.com
EmailSettings__Username=noreply@blazorshop.com
EmailSettings__Password=xxxxx
```

---

## ?? Setup Instructions

### Step 1: Initialize User Secrets

```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ\BlazorShop.Presentation\BlazorShop.API

# Initialize user secrets (this adds UserSecretsId to .csproj)
dotnet user-secrets init
```

**What this does:**
- Adds `<UserSecretsId>` to `BlazorShop.API.csproj`
- Creates a secrets file at: `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json`
- This file is **NEVER committed to Git**

### Step 2: Set Email Secrets

```bash
# Set individual secrets
dotnet user-secrets set "EmailSettings:From" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Username" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
```

### Step 3: Update appsettings.json (Remove Secrets)

**File:** `BlazorShop.Presentation\BlazorShop.API\appsettings.json`

```json
{
  "EmailSettings": {
    "From": "",
    "DisplayName": "BlazorShop Support",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "",
    "Password": ""
  }
}
```

**Note:** Leave `From`, `Username`, and `Password` empty - they'll be populated from User Secrets

### Step 4: Verify User Secrets

```bash
# List all secrets
dotnet user-secrets list

# Should output:
# EmailSettings:From = your-email@gmail.com
# EmailSettings:Username = your-email@gmail.com
# EmailSettings:Password = xxxx
```

### Step 5: Remove Secrets from appsettings.json

```bash
# View what would be changed (dry run)
git diff BlazorShop.Presentation\BlazorShop.API\appsettings.json

# Stage and commit the cleaned file
git add BlazorShop.Presentation\BlazorShop.API\appsettings.json
git commit -m "Remove sensitive email credentials from appsettings.json"
```

---

## ?? Gmail-Specific Setup (Recommended for Testing)

### Why Gmail?
- ? Free and reliable
- ? Easy to set up
- ? Supports app-specific passwords
- ? 2FA compatible

### Setup Steps:

#### 1. Enable 2-Factor Authentication
1. Go to https://myaccount.google.com/security
2. Enable "2-Step Verification"

#### 2. Generate App Password
1. Go to https://myaccount.google.com/apppasswords
2. Select "Mail" and "Other (Custom name)"
3. Enter "BlazorShop API"
4. Click "Generate"
5. Copy the 16-character password

#### 3. Configure User Secrets with Gmail

```bash
cd BlazorShop.Presentation\BlazorShop.API

dotnet user-secrets set "EmailSettings:From" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Username" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "abcd efgh ijkl mnop"  # App password
```

#### 4. Update appsettings.json for Gmail

```json
{
  "EmailSettings": {
    "DisplayName": "BlazorShop Support",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true
  }
}
```

---

## ?? Complete Configuration Example

### Updated `appsettings.json` (Safe to Commit)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=blazorshop;Username=postgres;Password=postgres"
  },

  "Jwt": {
    "Key": "4cc17386da975a90af4c383aad20a77dacc94b86a857ec13150269bbbd8b2df8",
    "Issuer": "https://localhost:7094",
    "Audience": "https://localhost:7094"
  },

  "Stripe": {
    "SecretKey": ""
  },

  "BankTransfer": {
    "Iban": "BG00UNCR70001512345678",
    "Beneficiary": "BlazorShop Ltd.",
    "BankName": "Unicredit Bulbank",
    "AdditionalInfo": "Include the reference in the transfer reason."
  },

  "EmailSettings": {
    "DisplayName": "BlazorShop Support",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true
  },

  "Recommendations": {
    "MaxRecommendations": 6,
    "CacheDurationHours": 1,
    "SlidingExpirationMinutes": 30,
    "EnableOrderBasedRecommendations": true,
    "MinimumOrderCount": 5
  }
}
```

### User Secrets (Local Only - NOT in Git)

Location: `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`

```json
{
  "EmailSettings:From": "your-email@gmail.com",
  "EmailSettings:Username": "your-email@gmail.com",
  "EmailSettings:Password": "your-app-specific-password",
  
  "Stripe:SecretKey": "sk_test_xxxxxxxxxxxxx",
  
  "ConnectionStrings:DefaultConnection": "Host=localhost;Port=5432;Database=blazorshop;Username=postgres;Password=your-db-password"
}
```

---

## ?? How Configuration Merging Works

.NET merges configuration in this order (later overrides earlier):

1. `appsettings.json`
2. `appsettings.{Environment}.json` (e.g., `appsettings.Development.json`)
3. **User Secrets** (Development only)
4. Environment Variables
5. Command-line arguments

**Example:**

```json
// appsettings.json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "From": "",           // Will be overridden
    "Username": "",       // Will be overridden
    "Password": ""        // Will be overridden
  }
}

// User Secrets
{
  "EmailSettings:From": "real@email.com",      // Overrides empty value
  "EmailSettings:Username": "real@email.com",  // Overrides empty value
  "EmailSettings:Password": "real-password"    // Overrides empty value
}

// Final merged configuration:
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "From": "real@email.com",           // From User Secrets
    "Username": "real@email.com",       // From User Secrets
    "Password": "real-password"         // From User Secrets
  }
}
```

---

## ?? Testing Configuration

### 1. Verify User Secrets are Loaded

Add this to `Program.cs` (temporarily):

```csharp
var config = builder.Configuration;
var emailFrom = config["EmailSettings:From"];
var emailPassword = config["EmailSettings:Password"];

Console.WriteLine($"Email From: {emailFrom}");
Console.WriteLine($"Email Password: {(string.IsNullOrEmpty(emailPassword) ? "NOT SET" : "SET (hidden)")}");
```

### 2. Test Email Sending

Run the API and try:
1. Register a new user ? Should receive confirmation email
2. Submit support ticket ? Should receive confirmation email

### 3. Check Logs

**Location:** `BlazorShop.Presentation\BlazorShop.API\log\log.txt`

**Success:**
```
[INF] Email sent to test@example.com
```

**Failure:**
```
[ERR] Error sending email to test@example.com: Authentication failed
```

---

## ?? Production Deployment

### Option 1: Azure App Service

**App Settings (Configuration):**
```
EmailSettings__From = noreply@blazorshop.com
EmailSettings__Username = noreply@blazorshop.com
EmailSettings__Password = xxxxx
EmailSettings__SmtpServer = smtp.gmail.com
EmailSettings__Port = 587
EmailSettings__UseSsl = true
```

### Option 2: Azure Key Vault

**Install package:**
```bash
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
```

**Program.cs:**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

**Store secrets:**
```bash
az keyvault secret set --vault-name "your-keyvault" --name "EmailSettings--From" --value "noreply@blazorshop.com"
az keyvault secret set --vault-name "your-keyvault" --name "EmailSettings--Password" --value "xxxxx"
```

### Option 3: Docker with Environment Variables

**docker-compose.yml:**
```yaml
services:
  api:
    image: blazorshop-api
    environment:
      - EmailSettings__From=noreply@blazorshop.com
      - EmailSettings__Username=noreply@blazorshop.com
      - EmailSettings__Password=${EMAIL_PASSWORD}
      - EmailSettings__SmtpServer=smtp.gmail.com
      - EmailSettings__Port=587
      - EmailSettings__UseSsl=true
```

**.env file (NOT in Git):**
```
EMAIL_PASSWORD=your-app-password
```

---

## ?? Security Checklist

### Before Committing:
- [ ] Remove all passwords from `appsettings.json`
- [ ] Remove all API keys from `appsettings.json`
- [ ] Remove database passwords from `appsettings.json`
- [ ] Set `UseSsl` to `true`
- [ ] Initialize User Secrets for local development
- [ ] Add `.vscode/`, `.vs/`, and `*.user` to `.gitignore`

### For Production:
- [ ] Use Azure Key Vault or similar secret management
- [ ] Never use hardcoded credentials
- [ ] Enable SSL/TLS (`UseSsl: true`)
- [ ] Use app-specific passwords (not account passwords)
- [ ] Rotate secrets regularly
- [ ] Use managed identities when possible

---

## ?? Migration Plan

### Current State Issues:
```json
"EmailSettings": {
  "From": "support@az-solve.com",           // ?? Inconsistent
  "Username": "shop@blazorshop_xx.com",     // ?? Different email
  "Password": "password",                    // ?? EXPOSED
  "UseSsl": false                            // ?? Insecure
}
```

### Step-by-Step Fix:

1. **Initialize User Secrets:**
   ```bash
   cd BlazorShop.Presentation\BlazorShop.API
   dotnet user-secrets init
   ```

2. **Set Actual Credentials:**
   ```bash
   dotnet user-secrets set "EmailSettings:From" "your-real-email@gmail.com"
   dotnet user-secrets set "EmailSettings:Username" "your-real-email@gmail.com"
   dotnet user-secrets set "EmailSettings:Password" "your-app-password"
   ```

3. **Update appsettings.json:**
   - Remove `From`, `Username`, `Password` values (leave empty strings)
   - Change `UseSsl` to `true`
   - Update `SmtpServer` to `smtp.gmail.com` (if using Gmail)

4. **Test:**
   ```bash
   dotnet run --project BlazorShop.Presentation\BlazorShop.API
   # Try sending a test email
   ```

5. **Commit Cleaned Config:**
   ```bash
   git add appsettings.json
   git commit -m "Security: Remove email credentials, enable SSL"
   ```

---

## ? Verification Commands

```bash
# 1. Check User Secrets ID is in .csproj
cat BlazorShop.Presentation\BlazorShop.API\BlazorShop.API.csproj | Select-String "UserSecretsId"

# 2. List all secrets
cd BlazorShop.Presentation\BlazorShop.API
dotnet user-secrets list

# 3. Check a specific secret
dotnet user-secrets list | Select-String "EmailSettings"

# 4. Remove a secret (if needed)
dotnet user-secrets remove "EmailSettings:Password"

# 5. Clear all secrets (careful!)
dotnet user-secrets clear
```

---

## ?? Additional Resources

- [Safe Storage of App Secrets in .NET](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Azure Key Vault Configuration Provider](https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)
- [Gmail SMTP Settings](https://support.google.com/mail/answer/7126229)
- [MailKit Documentation](https://github.com/jstedfast/MailKit)

---

## ?? Summary

### Current Issues:
1. ?? **Critical:** Password exposed in appsettings.json
2. ?? **High:** SSL disabled (insecure connection)
3. ?? **Medium:** No User Secrets configured
4. ?? **Low:** Inconsistent email addresses

### Immediate Actions Required:
1. ? Initialize User Secrets
2. ? Move credentials to User Secrets
3. ? Enable SSL (`UseSsl: true`)
4. ? Clean appsettings.json
5. ? Commit cleaned configuration

### Result:
- ? Secure local development
- ? No secrets in source control
- ? Encrypted SMTP connection
- ? Ready for production deployment

---

**Your email infrastructure is solid, just needs proper secret management!** ??
