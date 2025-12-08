# 🔍 Payment System Diagnostic & Fix Guide

## 📊 Issue Summary

**Problem:** User gets 403 Forbidden when trying to checkout with any payment method.

**Root Cause:** The checkout endpoint requires `User` role, but the logged-in user has `Admin` role.

## ✅ Fix Applied

Changed `CartController.cs` checkout endpoints to allow both roles:

```csharp
[HttpPost("checkout")]
[Authorize(Roles = "User, Admin")]  // ✅ Now allows Admin too
public async Task<IActionResult> Checkout(Checkout checkout)

[HttpPost("save-checkout")]
[Authorize(Roles = "User, Admin")]  // ✅ Now allows Admin too
public async Task<IActionResult> SaveCheckout(IEnumerable<CreateOrderItem> orderItems)
```

---

## 🔍 Step-by-Step Diagnostic

### 1. Verify Database Has Payment Methods

**Run this SQL query:**

```sql
-- Check if payment methods exist
SELECT * FROM "PaymentMethods";

-- Expected output:
-- Id                                   | Name
-- ------------------------------------+-------------------
-- 3604fc1d-cd6a-46ad-ace4-9b5f8e03f43b | Credit Card
-- a3bb23e6-6a7c-4b7d-9c73-7d5f2bc2f7b1 | PayPal
-- 6f2c2a7e-9f9b-4a0d-9f7f-2a1b3c4d5e6f | Cash on Delivery
-- b2e5c1d4-7a9f-4d2c-8f1e-3a4b5c6d7e8f | Bank Transfer
```

**If empty, seed the data:**

```bash
# Delete the database
dropdb -h localhost -U postgres blazorshop

# Recreate it
createdb -h localhost -U postgres blazorshop

# Run migrations and seed
cd BlazorShop.Presentation\BlazorShop.API
dotnet ef database update
dotnet run
```

---

### 2. Check Payment Configuration in Secrets

**List all secrets:**

```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ\BlazorShop.Presentation\BlazorShop.API

dotnet user-secrets list
```

**Expected output should include:**

```
Stripe:SecretKey = sk_test_xxxxxxxxxxxxx
BankTransfer:Iban = BG00UNCR70001512345678
BankTransfer:Beneficiary = BlazorShop Ltd.
BankTransfer:BankName = Unicredit Bulbank
```

**If missing, set them:**

```bash
# Stripe (get from: https://dashboard.stripe.com/test/apikeys)
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_STRIPE_KEY"

# Bank Transfer (already in appsettings.json, no need to set in secrets)
```

---

### 3. Verify API Can Load Payment Methods

**Test the endpoint:**

```bash
# Test payment methods endpoint (public, no auth required)
curl -X GET "https://localhost:7094/api/payment/methods" -k

# Expected response:
# [
#   {"id":"3604fc1d-cd6a-46ad-ace4-9b5f8e03f43b","name":"Credit Card"},
#   {"id":"a3bb23e6-6a7c-4b7d-9c73-7d5f2bc2f7b1","name":"PayPal"},
#   {"id":"6f2c2a7e-9f9b-4a0d-9f7f-2a1b3c4d5e6f","name":"Cash on Delivery"},
#   {"id":"b2e5c1d4-7a9f-4d2c-8f1e-3a4b5c6d7e8f","name":"Bank Transfer"}
# ]
```

---

### 4. Test User Roles

**Check what role the current user has:**

From the logs, decode the JWT token:

```
Token expiry: 06/12/2025 17:52:51 (EXPIRED)
Role: Admin ✅
Email: alexa....@.tmail.com
```

**Solution:** Login again to get a fresh token:

```bash
# Login via Blazor Web UI
# OR use curl:

curl -X POST "https://localhost:7094/api/authentication/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"alexandr....@.otmail.com","password":"F....#123"}' -k
```

---

### 5. Test Checkout with Fixed Role

**Now test checkout with Admin role:**

```bash
# Get fresh token from login response
$TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Test checkout
curl -X POST "https://localhost:7094/api/cart/checkout" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "items": [
      {
        "productId": "650e8400-e29b-41d4-a716-446655440001",
        "quantity": 1,
        "unitPrice": 485000
      }
    ],
    "paymentMethodId": "3604fc1d-cd6a-46ad-ace4-9b5f8e03f43b",
    "totalAmount": 485000
  }' -k

# Expected: 200 OK (instead of 403 Forbidden)
```

---

## 🧪 Complete Testing Workflow

### Test 1: Payment Methods Endpoint

```bash
# Run the API
cd BlazorShop.Presentation\BlazorShop.API
dotnet run

# In another terminal:
curl -X GET "https://localhost:7094/api/payment/methods" -k | jq
```

**✅ Expected:** JSON array with 4 payment methods

**❌ If fails:** Check database has payment methods (see Step 1)

---

### Test 2: Login and Get Token

```bash
curl -X POST "https://localhost:7094/api/authentication/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"alexandr....@.tmail.com","password":"F......#123"}' -k | jq

# Copy the "token" field from response
```

**✅ Expected:** JSON with `token`, `refreshToken`, and `success: true`

**❌ If fails:** Check user exists and password is correct

---

### Test 3: Checkout with Credit Card

```bash
# Replace $TOKEN with actual token from Test 2
curl -X POST "https://localhost:7094/api/cart/checkout" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "items": [
      {
        "productId": "650e8400-e29b-41d4-a716-446655440012",
        "quantity": 1,
        "unitPrice": 895000
      }
    ],
    "paymentMethodId": "3604fc1d-cd6a-46ad-ace4-9b5f8e03f43b",
    "totalAmount": 895000
  }' -k | jq
```

**✅ Expected:** 200 OK with payment result

**❌ If fails:**
- 401: Token expired or invalid
- 403: Role issue (should be fixed now!)
- 400: Check request body format

---

### Test 4: Checkout with PayPal

```bash
curl -X POST "https://localhost:7094/api/cart/checkout" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "items": [
      {
        "productId": "650e8400-e29b-41d4-a716-446655440010",
        "quantity": 1,
        "unitPrice": 285000
      }
    ],
    "paymentMethodId": "a3bb23e6-6a7c-4b7d-9c73-7d5f2bc2f7b1",
    "totalAmount": 285000
  }' -k | jq
```

**✅ Expected:** 200 OK with PayPal checkout URL

---

### Test 5: Checkout with Bank Transfer

```bash
curl -X POST "https://localhost:7094/api/cart/checkout" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "items": [
      {
        "productId": "650e8400-e29b-41d4-a716-446655440014",
        "quantity": 1,
        "unitPrice": 725000
      }
    ],
    "paymentMethodId": "b2e5c1d4-7a9f-4d2c-8f1e-3a4b5c6d7e8f",
    "totalAmount": 725000
  }' -k | jq
```

**✅ Expected:** 200 OK with bank transfer instructions

---

## 🐛 Common Issues & Solutions

### Issue 1: "401 Unauthorized"

**Cause:** Token expired or missing

**Solution:**
```bash
# Login again to get fresh token
curl -X POST "https://localhost:7094/api/authentication/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"alex....@.tmail.com","password":"F..#123"}' -k
```

---

### Issue 2: "403 Forbidden"

**Cause:** User doesn't have required role (FIXED!)

**Solution:**
- ✅ Already fixed by allowing both User and Admin roles
- If still happens, check the role in JWT token at https://jwt.io

---

### Issue 3: "Payment methods not found"

**Cause:** Database not seeded

**Solution:**
```bash
# Recreate database
cd BlazorShop.Presentation\BlazorShop.API
dotnet ef database drop -f
dotnet ef database update

# Run API to seed data
dotnet run
```

---

### Issue 4: "Stripe configuration missing"

**Cause:** Stripe secret key not in user secrets

**Solution:**
```bash
cd BlazorShop.Presentation\BlazorShop.API
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_KEY"

# Get test key from: https://dashboard.stripe.com/test/apikeys
```

---

## 📊 Verify Payment Method Configuration

### Check AppDbContext Seeding

**File:** `BlazorShop.Infrastructure\Data\AppDbContext.cs`

```csharp
builder.Entity<PaymentMethod>().HasData(
    new PaymentMethod
    {
        Id = Guid.Parse("3604fc1d-cd6a-46ad-ace4-9b5f8e03f43b"),
        Name = "Credit Card",
    },
    new PaymentMethod
    {
        Id = Guid.Parse("a3bb23e6-6a7c-4b7d-9c73-7d5f2bc2f7b1"),
        Name = "PayPal",
    },
    new PaymentMethod
    {
        Id = Guid.Parse("6f2c2a7e-9f9b-4a0d-9f7f-2a1b3c4d5e6f"),
        Name = "Cash on Delivery",
    },
    new PaymentMethod
    {
        Id = Guid.Parse("b2e5c1d4-7a9f-4d2c-8f1e-3a4b5c6d7e8f"),
        Name = "Bank Transfer",
    });
```

✅ This is correct and already in place!

---

### Check Payment Processors Registration

**File:** `BlazorShop.Presentation\BlazorShop.API\Program.cs`

Should have:

```csharp
// Payment processors
builder.Services.AddSingleton<IPaymentProcessor, StripePaymentProcessor>();
builder.Services.AddSingleton<IPaymentProcessor, PayPalMockProcessor>();
builder.Services.AddSingleton<IPaymentProcessor, CashOnDeliveryProcessor>();
builder.Services.AddSingleton<IPaymentProcessor, BankTransferProcessor>();

builder.Services.AddSingleton<PaymentServiceFactory>();
```

✅ Already configured!

---

## 🎯 Summary Checklist

Before testing, verify:

- [ ] **API is running** - `dotnet run` in BlazorShop.API
- [ ] **Database has payment methods** - Query `SELECT * FROM "PaymentMethods"`
- [ ] **User secrets configured** - `dotnet user-secrets list`
- [ ] **Role authorization fixed** - `[Authorize(Roles = "User, Admin")]` ✅
- [ ] **Fresh JWT token** - Login to get new token
- [ ] **Test endpoint responds** - `GET /api/payment/methods` returns 4 methods

---

## 🚀 Quick Test Script

```bash
# 1. Start API
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ\BlazorShop.Presentation\BlazorShop.API
dotnet run

# 2. In new terminal: Get payment methods
curl -X GET "https://localhost:7094/api/payment/methods" -k

# 3. Login
$loginResponse = Invoke-RestMethod -Method Post `
  -Uri "https://localhost:7094/api/authentication/login" `
  -ContentType "application/json" `
  -Body '{"email":"ale....@.mail.com","password":"F..#123"}' `
  -SkipCertificateCheck

$token = $loginResponse.token

# 4. Test checkout
$checkoutBody = @{
  items = @(
    @{
      productId = "650e8400-e29b-41d4-a716-446655440012"
      quantity = 1
      unitPrice = 895000
    }
  )
  paymentMethodId = "3604fc1d-cd6a-46ad-ace4-9b5f8e03f43b"
  totalAmount = 895000
} | ConvertTo-Json

Invoke-RestMethod -Method Post `
  -Uri "https://localhost:7094/api/cart/checkout" `
  -ContentType "application/json" `
  -Headers @{ Authorization = "Bearer $token" } `
  -Body $checkoutBody `
  -SkipCertificateCheck
```

---

## 📝 Logs to Monitor

**File:** `BlazorShop.Presentation\BlazorShop.API\log\log20251207.txt`

**Success indicators:**
```
[INF] Payment processor resolved: StripePaymentProcessor
[INF] Checkout session created successfully
[INF] Order created: {OrderReference}
```

**Failure indicators:**
```
[ERR] Payment processor not found for method: {MethodName}
[ERR] Checkout failed: {Error}
[INF] Authorization failed: User.IsInRole must be true for one of the following roles: (User)
```

---

## ✅ Expected Results After Fix

### 1. Checkout with Credit Card (Stripe)
**Status:** ✅ Should work
**Response:** Stripe checkout session URL

### 2. Checkout with PayPal
**Status:** ✅ Should work
**Response:** PayPal checkout URL (mock)

### 3. Checkout with Cash on Delivery
**Status:** ✅ Should work
**Response:** Success message

### 4. Checkout with Bank Transfer
**Status:** ✅ Should work
**Response:** Bank transfer instructions (IBAN, beneficiary, etc.)

---

## 🎓 Understanding the Fix

### Before Fix:
```csharp
[Authorize(Roles = "User")]  // ❌ Only User role allowed
public async Task<IActionResult> Checkout(Checkout checkout)
```

**Problem:** Admin users got 403 Forbidden

### After Fix:
```csharp
[Authorize(Roles = "User, Admin")]  // ✅ Both roles allowed
public async Task<IActionResult> Checkout(Checkout checkout)
```

**Result:** Both User and Admin can checkout! 🎉

---

## 🔄 Next Steps

1. ✅ **Role authorization fixed** - Both User and Admin can checkout
2. ⚠️ **Test all payment methods** - Follow testing workflow above
3. 📊 **Monitor logs** - Check for any errors during checkout
4. 🎯 **Create test user** - Optionally create a User-only account for testing

---

## 📞 Need Help?

If checkout still fails:

1. **Check logs:** `BlazorShop.API\log\log20251207.txt`
2. **Verify token:** Paste JWT at https://jwt.io and check role claim
3. **Test database:** Run SQL queries from Step 1
4. **Check secrets:** `dotnet user-secrets list`

**The fix is applied! Just need to:**
1. ✅ Rebuild and restart API
2. ✅ Login to get fresh token
3. ✅ Test checkout - should now work for Admin role! 🎉
