# ?? Quick Test Guide - Payment Integration

## ? Fast Start (5 minutes)

### 1. Start the Application

**Terminal 1 - API**:
```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ
dotnet run --project BlazorShop.Presentation\BlazorShop.API
```

**Terminal 2 - Web**:
```bash
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ
dotnet run --project BlazorShop.Presentation\BlazorShop.Web
```

**Wait for**:
```
? API: Now listening on: https://localhost:7094
? Web: Now listening on: https://localhost:7258
```

---

### 2. Test Mock Payment (Default in Development)

1. Open browser: `https://localhost:7258`
2. Register/Login
3. Browse products ? Add to cart
4. Navigate to `/cart`
5. Click "Proceed to Checkout"
6. Select **"Credit Card"**
7. **Observe**:
   - ?? 1-second loading indicator
   - ? Redirect to `/payment-success?mock=true&order_id={guid}`
   - ? Success message shown

**Check Database**:
```sql
\c blazorshop
SELECT "Id", "Reference", "Status", "PaymentMethod", "PaymentReference", "TotalAmount" 
FROM "Orders" 
ORDER BY "CreatedOn" DESC 
LIMIT 5;
```

**Expected**:
- Status: `Paid`
- PaymentMethod: `Credit Card`
- PaymentReference: `mock_pi_xxxxx`

**Check Logs**:
```bash
cd BlazorShop.Presentation\BlazorShop.API
tail -n 20 log/log.txt
```

**Expected**:
```
[INF] MOCK: Payment initiated for order {guid}, Amount XX.XX
[INF] MOCK: Payment successful for order {guid}, PaymentId mock_pi_xxxxx (random: 45 < 90)
```

---

### 3. Test Cash on Delivery

1. Add products to cart again
2. Click "Proceed to Checkout"
3. Select **"Cash on Delivery"**
4. **Observe**:
   - ? Success message: "Order placed successfully! You will pay upon delivery."
   - ? Redirect to `/payment-success`

**Check Database**:
```sql
SELECT "Id", "Reference", "Status", "PaymentMethod" 
FROM "Orders" 
ORDER BY "CreatedOn" DESC 
LIMIT 1;
```

**Expected**:
- Status: `AwaitingShipment`
- PaymentMethod: `CashOnDelivery`

**Check Email**:
- ? Email sent to your registered email
- ? Subject: "Order Confirmation - Cash on Delivery"
- ? Content includes order reference and total

---

### 4. Test Bank Transfer

1. Add products to cart again
2. Click "Proceed to Checkout"
3. Select **"Bank Transfer"**
4. **Observe**:
   - ? Bank transfer info dialog appears with:
     - Bank name: "Unicredit Bulbank (DEV)"
     - IBAN: "BG00UNCR70001512345678_DEV"
     - Payment reference: `BT-20250130-XXXXXXXX`
   - ? Click "I have noted the details"
   - ? Redirect to `/payment-success?bt=1`

**Check Database**:
```sql
SELECT "Id", "Reference", "Status", "PaymentMethod", "PaymentReference" 
FROM "Orders" 
ORDER BY "CreatedOn" DESC 
LIMIT 1;
```

**Expected**:
- Status: `AwaitingPayment`
- PaymentMethod: `BankTransfer`
- PaymentReference: `BT-20250130-XXXXXXXX`

**Check Email**:
- ? Email sent with subject: "Bank Transfer Instructions"
- ? Content includes bank details and payment reference

---

### 5. Test PayPal (Disabled - Expected to Fail)

1. Add products to cart again
2. Click "Proceed to Checkout"
3. Select **"PayPal"**
4. **Observe**:
   - ? Error toast: "PayPal is currently unavailable. Please use another payment method."

**Check Database**:
```sql
SELECT "Id", "Reference", "Status", "PaymentMethod" 
FROM "Orders" 
ORDER BY "CreatedOn" DESC 
LIMIT 1;
```

**Expected**:
- Status: `PaymentFailed`

---

## ?? Success Criteria

After all tests, you should have **4 orders**:

| Order Reference | Payment Method | Status | PaymentReference |
|-----------------|----------------|--------|------------------|
| ORD-20250130-XXXX | Credit Card | Paid | mock_pi_xxxxx |
| ORD-20250130-YYYY | Cash on Delivery | AwaitingShipment | null |
| ORD-20250130-ZZZZ | Bank Transfer | AwaitingPayment | BT-20250130-AAAA |
| ORD-20250130-WWWW | PayPal | PaymentFailed | null |

---

## ?? Database Quick Checks

**All Orders**:
```sql
SELECT "Id", "Reference", "Status", "PaymentMethod", "PaymentReference", "TotalAmount", "CreatedOn"
FROM "Orders"
ORDER BY "CreatedOn" DESC;
```

**Paid Orders Only**:
```sql
SELECT "Id", "Reference", "TotalAmount"
FROM "Orders"
WHERE "Status" = 'Paid'
ORDER BY "CreatedOn" DESC;
```

**Pending Payments**:
```sql
SELECT "Id", "Reference", "Status", "PaymentMethod", "TotalAmount"
FROM "Orders"
WHERE "Status" IN ('Created', 'AwaitingPayment', 'PaymentInitiated')
ORDER BY "CreatedOn" DESC;
```

**Failed Payments**:
```sql
SELECT "Id", "Reference", "PaymentMethod", "CreatedOn"
FROM "Orders"
WHERE "Status" = 'PaymentFailed'
ORDER BY "CreatedOn" DESC;
```

---

## ?? Log Quick Checks

**View Last 50 Lines**:
```bash
cd BlazorShop.Presentation\BlazorShop.API
tail -n 50 log/log.txt
```

**Search for Errors**:
```bash
cat log/log.txt | Select-String "\[ERR\]"
```

**Search for Payment Logs**:
```bash
cat log/log.txt | Select-String "Payment"
```

**Search for Email Logs**:
```bash
cat log/log.txt | Select-String "Email sent"
```

---

## ?? Troubleshooting

### Issue: Mock Payment Takes Too Long

**Problem**: Mock payment delay is too slow for testing

**Solution**: Reduce delay in `appsettings.Development.json`:
```json
{
  "Payment": {
    "MockDelayMs": 100
  }
}
```

---

### Issue: Mock Payment Always Fails

**Problem**: You keep getting payment failures

**Solution**: Increase success rate in `appsettings.Development.json`:
```json
{
  "Payment": {
    "MockSuccessRate": 100
  }
}
```

---

### Issue: Emails Not Sending

**Check Configuration**:
```bash
cd BlazorShop.Presentation\BlazorShop.API
dotnet user-secrets list | Select-String "EmailSettings"
```

**Expected Output**:
```
EmailSettings:From = your-email@gmail.com
EmailSettings:Password = ****
EmailSettings:Username = your-email@gmail.com
```

**If Empty**: Configure secrets:
```bash
dotnet user-secrets set "EmailSettings:From" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Username" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
```

---

### Issue: Order Not Created

**Check Logs**:
```bash
cat log/log.txt | Select-String "\[ERR\]" | tail -n 20
```

**Common Causes**:
1. Empty cart
2. Invalid product IDs
3. No payment methods in database

**Verify Payment Methods**:
```sql
SELECT * FROM "PaymentMethods";
```

**Expected**:
```
Id                                   | Name
-------------------------------------+-------------------
3604fc1d-cd6a-46ad-ace4-9b5f8e03f43b | Credit Card
6f2c2a7e-9f9b-4a0d-9f7f-2a1b3c4d5e6f | Cash on Delivery
b2e5c1d4-7a9f-4d2c-8f1e-3a4b5c6d7e8f | Bank Transfer
a3bb23e6-6a7c-4b7d-9c73-7d5f2bc2f7b1 | PayPal
```

**If Empty**: Run seeder or insert manually:
```sql
INSERT INTO "PaymentMethods" ("Id", "Name") VALUES
  ('3604fc1d-cd6a-46ad-ace4-9b5f8e03f43b', 'Credit Card'),
  ('6f2c2a7e-9f9b-4a0d-9f7f-2a1b3c4d5e6f', 'Cash on Delivery'),
  ('b2e5c1d4-7a9f-4d2c-8f1e-3a4b5c6d7e8f', 'Bank Transfer'),
  ('a3bb23e6-6a7c-4b7d-9c73-7d5f2bc2f7b1', 'PayPal');
```

---

## ?? Advanced Testing (Optional)

### Test Mock Payment Failure

**Modify Configuration**:
```json
{
  "Payment": {
    "MockSuccessRate": 10
  }
}
```

**Restart API** and try checkout again. You should see:
- ? Error toast: "Mock payment declined (simulated failure for testing)"
- Database: Order status = `PaymentFailed`

---

### Test Real Stripe (Requires Keys)

**Configure Stripe Test Keys**:
```bash
cd BlazorShop.Presentation\BlazorShop.API
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR_KEY"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR_SECRET"
dotnet user-secrets set "Payment:UseMockPayments" "false"
```

**Restart API** and try checkout. You should:
- ? Redirect to Stripe checkout page
- ? Use test card: `4242 4242 4242 4242`
- ? Complete payment
- ? Redirect back to success page
- ?? Order status = `PaymentInitiated` (webhook needed for `Paid`)

**Webhook Testing** (requires Stripe CLI):
```bash
stripe listen --forward-to https://localhost:7094/api/payment/stripe/webhook
```

---

## ? Test Summary

After completing all tests, you should verify:

1. ? **Mock Payment**: Works, order marked as "Paid"
2. ? **Cash on Delivery**: Works, order marked as "AwaitingShipment", email sent
3. ? **Bank Transfer**: Works, order marked as "AwaitingPayment", email sent, reference generated
4. ? **PayPal**: Gracefully fails with error message
5. ? **Database**: All columns populated correctly
6. ? **Logs**: No errors, payment operations logged
7. ? **Emails**: Delivered successfully

---

## ?? All Tests Pass?

**Congratulations!** Your payment integration is working correctly.

**Next Steps**:
1. Review `PAYMENT_VALIDATION_CHECKLIST.md` for detailed validation
2. Configure Stripe test keys for real integration testing
3. Test webhook handling with Stripe CLI
4. Deploy to staging environment
5. Set up production Stripe keys in Azure Key Vault

---

## ?? Need Help?

**Check**:
1. `PAYMENT_IMPLEMENTATION_GUIDE.md` - Complete implementation details
2. `PAYMENT_VALIDATION_CHECKLIST.md` - Comprehensive checklist
3. `EMAIL_CONFIGURATION_SECURITY_REVIEW.md` - Email setup
4. API logs: `BlazorShop.Presentation\BlazorShop.API\log\log.txt`

**Common Issues**:
- Build errors ? Run `dotnet build` and check output
- Email errors ? Verify User Secrets configured
- Database errors ? Check migration applied
- Payment errors ? Check logs for details

---

**Happy Testing!** ??
