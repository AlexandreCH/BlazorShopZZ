# ?? Payment System - Command Reference

Quick reference for all commands needed to manage the payment system.

---

## ?? Application Start/Stop

### Start API
```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ
dotnet run --project BlazorShop.Presentation\BlazorShop.API
```

### Start Web
```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ
dotnet run --project BlazorShop.Presentation\BlazorShop.Web
```

### Stop (Ctrl+C in terminal)

---

## ?? User Secrets Management

### Navigate to API Project
```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ\BlazorShop.Presentation\BlazorShop.API
```

### Initialize User Secrets
```bash
dotnet user-secrets init
```

### Set Secrets

**Email Settings**:
```bash
dotnet user-secrets set "EmailSettings:From" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Username" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
```

**Stripe Settings**:
```bash
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_KEY"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR_SECRET"
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_YOUR_KEY"
```

**Mock Payment Toggle**:
```bash
# Enable mock (development)
dotnet user-secrets set "Payment:UseMockPayments" "true"

# Disable mock (use real Stripe)
dotnet user-secrets set "Payment:UseMockPayments" "false"
```

### List All Secrets
```bash
dotnet user-secrets list
```

### Remove a Secret
```bash
dotnet user-secrets remove "EmailSettings:Password"
```

### Clear All Secrets
```bash
dotnet user-secrets clear
```

---

## ??? Database Management

### Navigate to Infrastructure Project
```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ\BlazorShop.Infrastructure
```

### Create Migration
```bash
dotnet ef migrations add MigrationName --startup-project ../BlazorShop.Presentation/BlazorShop.API
```

### Apply Migrations
```bash
dotnet ef database update --startup-project ../BlazorShop.Presentation/BlazorShop.API
```

### List Migrations
```bash
dotnet ef migrations list --startup-project ../BlazorShop.Presentation/BlazorShop.API
```

### Rollback Migration
```bash
dotnet ef database update PreviousMigrationName --startup-project ../BlazorShop.Presentation/BlazorShop.API
```

### Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove --startup-project ../BlazorShop.Presentation/BlazorShop.API
```

---

## ?? PostgreSQL Database Queries

### Connect to Database
```bash
psql -U postgres
```

### Switch to BlazorShop Database
```sql
\c blazorshop
```

### List All Tables
```sql
\dt
```

### Describe Orders Table
```sql
\d "Orders"
```

### View Recent Orders
```sql
SELECT 
    "Id", 
    "Reference", 
    "Status", 
    "PaymentMethod", 
    "PaymentReference", 
    "TotalAmount", 
    "CreatedOn"
FROM "Orders"
ORDER BY "CreatedOn" DESC
LIMIT 10;
```

### View Orders by Status
```sql
SELECT "Reference", "Status", "PaymentMethod", "TotalAmount"
FROM "Orders"
WHERE "Status" = 'Paid'
ORDER BY "CreatedOn" DESC;
```

### View Paid Orders
```sql
SELECT COUNT(*) as TotalPaid, SUM("TotalAmount") as TotalRevenue
FROM "Orders"
WHERE "Status" = 'Paid';
```

### View Orders by Payment Method
```sql
SELECT 
    "PaymentMethod", 
    COUNT(*) as Count, 
    SUM("TotalAmount") as Total
FROM "Orders"
GROUP BY "PaymentMethod"
ORDER BY Count DESC;
```

### View Failed Payments
```sql
SELECT "Id", "Reference", "PaymentMethod", "CreatedOn"
FROM "Orders"
WHERE "Status" = 'PaymentFailed'
ORDER BY "CreatedOn" DESC;
```

### View Payment Methods
```sql
SELECT * FROM "PaymentMethods";
```

### View Refresh Tokens
```sql
-- View all refresh tokens
SELECT * FROM "RefreshTokens";

-- Count tokens per user
SELECT "UserId", COUNT(*) as TokenCount
FROM "RefreshTokens"
GROUP BY "UserId"
ORDER BY TokenCount DESC;

-- Find users with multiple tokens (should be empty after fix!)
SELECT "UserId", COUNT(*) as TokenCount
FROM "RefreshTokens"
GROUP BY "UserId"
HAVING COUNT(*) > 1;

-- View tokens with expiration info
SELECT 
    rt."UserId",
    rt."Token",
    rt."CreatedAt",
    rt."ExpiresAt",
    CASE 
        WHEN rt."ExpiresAt" < NOW() THEN 'EXPIRED'
        ELSE 'VALID'
    END as Status
FROM "RefreshTokens" rt
ORDER BY rt."ExpiresAt" ASC;

-- Count expired tokens
SELECT COUNT(*) as ExpiredTokens
FROM "RefreshTokens"
WHERE "ExpiresAt" < NOW();
```

### Cleanup Duplicate Refresh Tokens
```sql
-- ?? WARNING: This will force all users to re-login!

-- View tokens before cleanup
SELECT COUNT(*) as TotalTokens FROM "RefreshTokens";

-- Delete ALL refresh tokens
DELETE FROM "RefreshTokens";

-- Verify cleanup
SELECT COUNT(*) as TotalTokens FROM "RefreshTokens";
-- Expected: 0
```

### Delete All Orders (Testing Only)
```sql
DELETE FROM "OrderLines";
DELETE FROM "Orders";
```

---

## ?? Log Management

### View Last 50 Lines
```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ\BlazorShop.Presentation\BlazorShop.API
tail -n 50 log/log.txt
```

### Follow Logs in Real-Time
```bash
Get-Content log/log.txt -Wait -Tail 50
```

### Search for Errors
```bash
cat log/log.txt | Select-String "\[ERR\]"
```

### Search for Payment Logs
```bash
cat log/log.txt | Select-String "Payment"
```

### Search for Mock Payment Logs
```bash
cat log/log.txt | Select-String "MOCK:"
```

### Search for Stripe Logs
```bash
cat log/log.txt | Select-String "Stripe"
```

### Search for Email Logs
```bash
cat log/log.txt | Select-String "Email sent"
```

### Clear Logs (Be Careful!)
```bash
Remove-Item log/log*.txt
```

---

## ??? Build Commands

### Build Solution
```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ
dotnet build
```

### Build Specific Project
```bash
dotnet build BlazorShop.Presentation\BlazorShop.API
```

### Clean Solution
```bash
dotnet clean
```

### Restore NuGet Packages
```bash
dotnet restore
```

### Rebuild Solution
```bash
dotnet clean && dotnet restore && dotnet build
```

---

## ?? Testing Commands

### Run All Tests (if exists)
```bash
dotnet test
```

### Run Tests with Detailed Output
```bash
dotnet test --verbosity detailed
```

---

## ?? Stripe CLI (Optional)

### Install Stripe CLI
```bash
winget install stripe.stripe-cli
```

### Login to Stripe
```bash
stripe login
```

### Listen to Webhooks (Forward to Local API)
```bash
stripe listen --forward-to https://localhost:7094/api/payment/stripe/webhook
```

### Trigger Test Webhook Events
```bash
# Successful payment
stripe trigger checkout.session.completed

# Expired session
stripe trigger checkout.session.expired

# Refund
stripe trigger charge.refunded
```

### View Recent Stripe Events
```bash
stripe events list
```

---

## ?? Git Commands

### Check Status
```bash
git status
```

### View Uncommitted Changes
```bash
git diff
```

### Search for Secrets in Git History (Security Check)
```bash
git log -S "sk_test_" --oneline
git log -S "sk_live_" --oneline
git log -S "whsec_" --oneline
```

### Stage Files
```bash
git add .
```

### Commit Changes
```bash
git commit -m "Implement payment integration"
```

### Push to Remote
```bash
git push origin master
```

---

## ?? NuGet Package Management

### Add Stripe Package (Already Installed)
```bash
dotnet add package Stripe.net --version 46.4.0
```

### Add MailKit Package (Already Installed)
```bash
dotnet add package MailKit --version 4.13.0
```

### List Installed Packages
```bash
dotnet list package
```

### Update Package
```bash
dotnet add package PackageName --version x.x.x
```

---

## ?? Deployment Commands (Azure)

### Login to Azure CLI
```bash
az login
```

### Set App Settings
```bash
az webapp config appsettings set `
  --name blazorshop-api `
  --resource-group blazorshop-rg `
  --settings `
  Stripe__SecretKey="sk_live_xxxxx" `
  Stripe__WebhookSecret="whsec_xxxxx" `
  Payment__UseMockPayments="false" `
  App__FrontendUrl="https://yourapp.azurewebsites.net" `
  App__ApiUrl="https://yourapi.azurewebsites.net"
```

### View App Settings
```bash
az webapp config appsettings list `
  --name blazorshop-api `
  --resource-group blazorshop-rg
```

### Deploy to Azure
```bash
dotnet publish -c Release
az webapp deployment source config-zip `
  --resource-group blazorshop-rg `
  --name blazorshop-api `
  --src ./bin/Release/net9.0/publish.zip
```

### View Logs
```bash
az webapp log tail `
  --name blazorshop-api `
  --resource-group blazorshop-rg
```

---

## ?? Configuration Quick Changes

### Switch to Mock Payments (Development)
**File**: `appsettings.Development.json`
```json
{
  "Payment": {
    "UseMockPayments": true,
    "MockSuccessRate": 90,
    "MockDelayMs": 1000
  }
}
```

### Switch to Stripe Payments (Production)
**File**: `appsettings.json`
```json
{
  "Payment": {
    "UseMockPayments": false
  }
}
```

**AND** Set User Secrets:
```bash
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_KEY"
dotnet user-secrets set "Payment:UseMockPayments" "false"
```

### Adjust Mock Success Rate
**File**: `appsettings.Development.json`
```json
{
  "Payment": {
    "MockSuccessRate": 100  // 100% success (no failures)
  }
}
```

### Adjust Mock Delay
**File**: `appsettings.Development.json`
```json
{
  "Payment": {
    "MockDelayMs": 100  // Faster testing (0.1s delay)
  }
}
```

---

## ?? Cleanup Commands

### Clear Cart Cookies (Browser)
**Browser Console** (F12):
```javascript
document.cookie.split(";").forEach(c => {
  document.cookie = c.replace(/^ +/, "").replace(/=.*/, "=;expires=" + new Date().toUTCString() + ";path=/");
});
```

### Clear Browser Local Storage
```javascript
localStorage.clear();
```

### Reset Test Database
```sql
\c blazorshop
DELETE FROM "OrderLines";
DELETE FROM "Orders";
DELETE FROM "CheckoutOrderItems";
```

---

## ?? Monitoring Commands

### Check API Health
```bash
curl https://localhost:7094/health
```

### Check Database Connection
```bash
psql -U postgres -d blazorshop -c "SELECT 1;"
```

### Check Disk Space
```bash
Get-PSDrive C
```

### Check Memory Usage
```bash
Get-Process -Name dotnet | Select-Object CPU, WorkingSet
```

---

## ?? Quick Test Sequence

```bash
# 1. Start API
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ
dotnet run --project BlazorShop.Presentation\BlazorShop.API

# 2. In new terminal, start Web
dotnet run --project BlazorShop.Presentation\BlazorShop.Web

# 3. In new terminal, connect to database
psql -U postgres -d blazorshop

# 4. View orders (run this after testing)
SELECT "Reference", "Status", "PaymentMethod", "TotalAmount" 
FROM "Orders" 
ORDER BY "CreatedOn" DESC;

# 5. View logs
cd BlazorShop.Presentation\BlazorShop.API
Get-Content log/log.txt -Wait -Tail 50
```

---

## ?? Emergency Commands

### Kill Stuck dotnet Process
```bash
Get-Process -Name dotnet | Stop-Process -Force
```

### Reset Database (DESTRUCTIVE!)
```bash
dotnet ef database drop --force --startup-project BlazorShop.Presentation/BlazorShop.API
dotnet ef database update --startup-project BlazorShop.Presentation/BlazorShop.API
```

### Clear All Secrets (Start Fresh)
```bash
cd BlazorShop.Presentation\BlazorShop.API
dotnet user-secrets clear
```

---

## ?? Documentation Commands

### View Documentation
```bash
code PAYMENT_IMPLEMENTATION_GUIDE.md
code PAYMENT_VALIDATION_CHECKLIST.md
code QUICK_TEST_GUIDE.md
code PAYMENT_IMPLEMENTATION_SUMMARY.md
code PAYMENT_STATUS_BOARD.md
code PAYMENT_COMMANDS_REFERENCE.md
```

---

## ? Pre-Flight Checklist

Before testing, run these commands:

```bash
# 1. Check build
dotnet build
# Expected: Build succeeded. 0 Error(s)

# 2. Check migration status
cd BlazorShop.Infrastructure
dotnet ef migrations list --startup-project ../BlazorShop.Presentation/BlazorShop.API
# Expected: AddPaymentReferenceToOrder (Applied)

# 3. Check User Secrets
cd ../BlazorShop.Presentation/BlazorShop.API
dotnet user-secrets list
# Expected: EmailSettings configured

# 4. Check database connection
psql -U postgres -d blazorshop -c "SELECT COUNT(*) FROM \"PaymentMethods\";"
# Expected: 4 payment methods

# 5. Check configuration
cat appsettings.Development.json | Select-String "UseMockPayments"
# Expected: "UseMockPayments": true
```

---

**All systems ready!** Use this reference anytime you need to manage the payment system.
