# BlazorShop Payment System - Complete Analysis & Recommendations

## ?? Table of Contents
1. [Current Payment System Overview](#current-payment-system-overview)
2. [Payment Methods Supported](#payment-methods-supported)
3. [Payment Flow Architecture](#payment-flow-architecture)
4. [Configuration Details](#configuration-details)
5. [Security Analysis](#security-analysis)
6. [Issues & Vulnerabilities](#issues--vulnerabilities)
7. [Recommended Improvements](#recommended-improvements)
8. [Mock Payment Implementation](#mock-payment-implementation)
9. [Testing Strategy](#testing-strategy)

---

## ??? Current Payment System Overview

### Architecture Components

```
???????????????????????????????????????????????????????????????
?                   Blazor WebAssembly Client                  ?
?  ????????????????????????????????????????????????????????   ?
?  ? Cart.razor.cs ? SelectPaymentMethod()                 ?   ?
?  ?  - Displays payment options dialog                    ?   ?
?  ?  - Handles payment selection                          ?   ?
?  ?  - Processes payment redirects                        ?   ?
?  ????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????
                              ?
                              ?
???????????????????????????????????????????????????????????????
?                       ASP.NET Core API                       ?
?  ????????????????????????????????????????????????????????   ?
?  ? CartController.Checkout(Checkout checkout)            ?   ?
?  ?   ? CartService.CheckoutAsync()                       ?   ?
?  ?      ?? Validates cart items                          ?   ?
?  ?      ?? Calculates total amount                       ?   ?
?  ?      ?? Routes to payment provider                    ?   ?
?  ????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????
                              ?
                              ?
???????????????????????????????????????????????????????????????
?                    Payment Service Layer                     ?
?  ????????????????????????????????????????????????????????   ?
?  ? Stripe       ? PayPal       ? Cash on      ? Bank    ?   ?
?  ? Service      ? Service      ? Delivery     ? Transfer?   ?
?  ????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????
                              ?
                              ?
???????????????????????????????????????????????????????????????
?                    External Payment Gateway                  ?
?  ????????????????????????????????????????????????????????   ?
?  ? Stripe Checkout Session                               ?   ?
?  ? PayPal Order Creation (Demo)                          ?   ?
?  ????????????????????????????????????????????????????????   ?
???????????????????????????????????????????????????????????????
```

---

## ?? Payment Methods Supported

The system supports **4 payment methods** seeded into the database:

### 1. **Credit Card (Stripe)**
- **ID**: `3604fc1d-cd6a-46ad-ace4-9b5f8e03f43b`
- **Implementation**: `StripePaymentService.cs`
- **Flow**: 
  - Creates Stripe Checkout Session
  - Redirects to Stripe hosted checkout page
  - Returns to success/cancel URL

```csharp
// BlazorShop.Infrastructure\Services\StripePaymentService.cs
public async Task<ServiceResponse> Pay(decimal totalAmount, 
    IEnumerable<Product> cartProducts, 
    IEnumerable<ProcessCart> carts)
{
    var lineItems = new List<SessionLineItemOptions>();
    
    foreach (var item in cartProducts)
    {
        var pQuantity = carts.FirstOrDefault(_ => _.ProductId == item.Id);
        
        lineItems.Add(new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = "eur",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = item.Name,
                    Description = item.Description
                },
                UnitAmount = (long)(item.Price * 100), // Convert to cents
            },
            Quantity = pQuantity!.Quantity,
        });
    }
    
    var opt = new SessionCreateOptions
    {
        PaymentMethodTypes = ["card"],
        LineItems = lineItems,
        Mode = "payment",
        SuccessUrl = "https://localhost:7258/payment-success",
        CancelUrl = "https://localhost:7258/payment-cancel",
    };
    
    var service = new SessionService();
    var session = await service.CreateAsync(opt);
    
    return new ServiceResponse(true, session.Url);
}
```

**Issues**:
- ? Hardcoded success/cancel URLs (not environment-aware)
- ? No order creation before payment (order lost if user doesn't complete)
- ? No webhook handling for payment confirmation
- ? No idempotency key for session creation

---

### 2. **PayPal**
- **ID**: `a3bb23e6-6a7c-4b7d-9c73-7d5f2bc2f7b1`
- **Implementation**: `PayPalPaymentService.cs`
- **Status**: ?? **MOCK IMPLEMENTATION ONLY**

```csharp
// BlazorShop.Infrastructure\Services\PayPalPaymentService.cs
public class PayPalPaymentService : IPayPalPaymentService
{
    public Task<ServiceResponse> Pay(decimal totalAmount, 
        IEnumerable<Product> cartProducts, 
        IEnumerable<ProcessCart> carts)
    {
        // ?? RETURNS FAKE URL
        var url = "https://www.paypal.com/checkoutnow?token=demo-token";
        return Task.FromResult(new ServiceResponse(true, url));
    }
    
    public Task<bool> CaptureAsync(string orderId)
    {
        // ?? ALWAYS RETURNS TRUE
        return Task.FromResult(true);
    }
}
```

**Issues**:
- ? Not a real PayPal integration
- ? Returns fake URL that doesn't work
- ? No actual payment processing
- ? Misleading to users

---

### 3. **Cash on Delivery (COD)**
- **ID**: `6f2c2a7e-9f9b-4a0d-9f7f-2a1b3c4d5e6f`
- **Implementation**: Simple message return
- **Flow**: Order is placed without payment processing

```csharp
// In CartService.CheckoutAsync()
if (codId.HasValue && checkout.PaymentMethodId == codId.Value)
{
    return new ServiceResponse(true, 
        "Order placed with Cash on Delivery. You will pay upon delivery.");
}
```

**Issues**:
- ? No order created in database
- ? No order confirmation email
- ? No record for admin to fulfill
- ? User just gets a message, no order tracking

---

### 4. **Bank Transfer**
- **ID**: `b2e5c1d4-7a9f-4d2c-8f1e-3a4b5c6d7e8f`
- **Implementation**: Creates pending order + sends email instructions
- **Flow**: 
  - Creates order with "Pending" status
  - Generates unique reference code
  - Sends bank details via email
  - Returns instructions to frontend

```csharp
// In CartService.CheckoutAsync()
if (bankId.HasValue && checkout.PaymentMethodId == bankId.Value)
{
    var reference = $"BT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    
    var order = new Order
    {
        UserId = userId ?? string.Empty,
        Status = "Pending",
        Reference = reference,
        TotalAmount = totalAmount,
        Lines = checkout.Carts.Select(ci => new OrderLine
        {
            ProductId = ci.ProductId,
            Quantity = ci.Quantity,
            UnitPrice = products.First(p => p.Id == ci.ProductId).Price
        }).ToList()
    };
    
    var orderId = await _orderRepository.CreateAsync(order);
    
    // Send email with bank details
    var html = $@"<p>Thank you for your order.</p>
<p>Please make a bank transfer to the following account:</p>
<ul>
<li>Bank: <b>{_btSettings.BankName}</b></li>
<li>Beneficiary: <b>{_btSettings.Beneficiary}</b></li>
<li>IBAN: <b>{iban}</b></li>
<li>Amount: <b>{totalAmount:F2} EUR</b></li>
<li>Reference: <b>{reference}</b></li>
</ul>";
    
    await _emailService.SendEmailAsync(user.Email, 
        "Bank Transfer Instructions", html);
}
```

**Best Practice**: ? This is the most complete implementation!

---

## ?? Payment Flow Architecture

### Complete Checkout Flow

```
User adds items to cart
        ?
        ?
User clicks "Checkout"
        ?
        ?
Payment method selection dialog
        ?
        ?
???????????????????????????????????????
?               ?          ?          ?
?               ?          ?          ?
Stripe      PayPal      COD     Bank Transfer
?               ?          ?          ?
?               ?          ?          ?
Redirect     Redirect      ?          ?
to Stripe    to PayPal     ?          ?
checkout     checkout      ?          ?
page         page          ?          ?
?               ?          ?          ?
?               ?          ?          ?
User pays    User pays     ?          ?
on Stripe    on PayPal     ?          ?
?               ?          ?          ?
?               ?          ?          ?
Stripe       PayPal       Show      Create order
webhook      webhook      message   Send email
(MISSING!)   (MISSING!)   (NO ORDER!) Show instructions
?               ?                     ?
?               ?                     ?
Update       Update                Order created
order        order                 Status: Pending
status       status                Admin reviews
?               ?                     ?
?               ?                     ?
Redirect     Redirect              Manual
to success   to success            confirmation
page         page                  by admin
```

---

## ?? Configuration Details

### Current Configuration Structure

**File**: `BlazorShop.Presentation\BlazorShop.API\appsettings.json`

```json
{
  "Stripe": {
    "SecretKey": "super secret key"  // ?? PLACEHOLDER - INSECURE!
  },
  "BankTransfer": {
    "Iban": "BG00UNCR70001512345678_999",
    "Beneficiary": "XX_BlazorShop Ltd.",
    "BankName": "XX_Unicredit Bulbank",
    "AdditionalInfo": "Include the reference in the transfer reason."
  },
  "EmailSettings": {
    "DisplayName": "EAsset",
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true
    // Note: From, Username, Password should be in User Secrets
  }
}
```

### Dependency Injection Setup

**File**: `BlazorShop.Infrastructure\DependencyInjection.cs`

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, 
    IConfiguration config)
{
    // Payment services
    services.AddScoped<IPaymentMethod, PaymentMethodRepository>();
    services.AddScoped<IPaymentService, StripePaymentService>();
    services.AddScoped<IPayPalPaymentService, PayPalPaymentService>();
    services.AddScoped<IOrderRepository, OrderRepository>();
    
    // Stripe configuration
    Stripe.StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
    
    // Bank transfer settings
    services.Configure<BankTransferSettings>(
        config.GetSection("BankTransfer"));
    
    return services;
}
```

### Database Seeding

**File**: `BlazorShop.Infrastructure\Data\AppDbContext.cs`

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
    }
);
```

---

## ?? Security Analysis

### ? Security Strengths

1. **User Authentication Required**
   ```csharp
   [HttpPost("checkout")]
   [Authorize(Roles = "User")]
   public async Task<IActionResult> Checkout(Checkout checkout)
   {
       var userId = this.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
       // ...
   }
   ```

2. **Server-Side Price Calculation**
   - Prices are fetched from database, not trusted from client
   - Cart totals recalculated on server
   ```csharp
   private async Task<(IEnumerable<Product>, decimal)> GetCartTotalAmount(
       IEnumerable<ProcessCart> carts)
   {
       var products = await _productRepository.GetAllAsync();
       // Calculate from database prices
   }
   ```

3. **Bank Transfer - Good Practices**
   - Unique reference codes generated
   - Order created before payment
   - Email confirmation sent

### ? Security Vulnerabilities

#### 1. **Hardcoded Credentials in Configuration**
```json
// ? BAD - In appsettings.json
{
  "Stripe": {
    "SecretKey": "super secret key"  // Committed to Git!
  }
}
```

**Fix**: Use User Secrets / Environment Variables / Azure Key Vault

```bash
# Local Development
dotnet user-secrets set "Stripe:SecretKey" "sk_test_xxxxxxxxxxxx"

# Production (Environment Variable)
export Stripe__SecretKey="sk_live_xxxxxxxxxxxx"
```

#### 2. **No Webhook Signature Verification**

Stripe webhooks should verify signatures to prevent fraud:

```csharp
// ? MISSING - Webhook handler
[HttpPost("stripe/webhook")]
public async Task<IActionResult> StripeWebhook()
{
    var json = await new StreamReader(HttpContext.Request.Body)
        .ReadToEndAsync();
    
    // ? NO SIGNATURE VERIFICATION!
    // Anyone can POST fake payment confirmations
}
```

**Fix**: Add signature verification

```csharp
[HttpPost("stripe/webhook")]
public async Task<IActionResult> StripeWebhook()
{
    var json = await new StreamReader(HttpContext.Request.Body)
        .ReadToEndAsync();
    
    try
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            _webhookSecret  // From configuration
        );
        
        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Session;
            await _orderService.CompleteOrderAsync(session.ClientReferenceId);
        }
        
        return Ok();
    }
    catch (StripeException)
    {
        return BadRequest();
    }
}
```

#### 3. **Hardcoded Redirect URLs**

```csharp
// ? BAD - Hardcoded URLs
var opt = new SessionCreateOptions
{
    SuccessUrl = "https://localhost:7258/payment-success",
    CancelUrl = "https://localhost:7258/payment-cancel",
};
```

**Fix**: Use configuration-based URLs

```csharp
// ? GOOD - Configuration-based
var opt = new SessionCreateOptions
{
    SuccessUrl = $"{_appSettings.FrontendUrl}/payment-success",
    CancelUrl = $"{_appSettings.FrontendUrl}/payment-cancel",
};
```

#### 4. **No Idempotency Keys**

Stripe sessions should use idempotency keys to prevent duplicate charges:

```csharp
// ? GOOD - Add idempotency key
var requestOptions = new RequestOptions
{
    IdempotencyKey = $"{userId}-{DateTime.UtcNow.Ticks}"
};

var session = await service.CreateAsync(opt, requestOptions);
```

#### 5. **Missing Order Creation for Stripe/PayPal**

Orders are only created for Bank Transfer. Stripe/PayPal have no order record:

```csharp
// ? BAD - No order created
if (creditCardId.HasValue && checkout.PaymentMethodId == creditCardId.Value)
{
    return await _paymentService.Pay(totalAmount, products, checkout.Carts);
    // Payment redirects, but no Order entity exists!
}
```

**Fix**: Create order first, then pass order ID to payment provider

#### 6. **No CSRF Protection on Payment Endpoints**

Payment endpoints should have anti-CSRF tokens or use secure patterns.

---

## ?? Issues & Vulnerabilities

### Critical Issues

| Issue | Severity | Impact | Fix Priority |
|-------|----------|--------|--------------|
| Hardcoded Stripe secret key | ?? Critical | Anyone with Git access can charge cards | Immediate |
| No webhook signature verification | ?? Critical | Attackers can mark orders as paid | Immediate |
| PayPal is fake implementation | ?? Critical | Users think they can pay with PayPal | Immediate |
| No order creation for Stripe/PayPal | ?? High | Orders lost if user doesn't complete payment | High |
| Hardcoded redirect URLs | ?? High | Won't work in production | High |
| No idempotency keys | ?? Medium | Duplicate payments possible | Medium |
| COD creates no order | ?? Medium | No record for fulfillment | Medium |

### Architecture Issues

1. **No Payment State Machine**
   - Orders should track: Created ? Authorized ? Captured ? Fulfilled
   - Currently: Only "Pending" status

2. **No Refund Support**
   - No way to process refunds through API

3. **No Payment Audit Log**
   - No record of payment attempts, failures, successes

4. **No Cart Validation**
   - No check if products are still in stock
   - No check if prices changed since cart was created

---

## ?? Recommended Improvements

### 1. **Create Comprehensive Payment Service**

Create a unified payment orchestration service:

```csharp
public interface IPaymentOrchestrationService
{
    Task<PaymentResult> InitiatePaymentAsync(
        Guid orderId, 
        PaymentMethodType method);
    
    Task<OrderStatus> HandlePaymentCallbackAsync(
        string provider, 
        string paymentId);
    
    Task<bool> RefundPaymentAsync(Guid orderId, decimal amount);
    
    Task<PaymentStatus> GetPaymentStatusAsync(Guid orderId);
}

public class PaymentOrchestrationService : IPaymentOrchestrationService
{
    private readonly IOrderRepository _orders;
    private readonly IStripePaymentService _stripe;
    private readonly IPayPalPaymentService _payPal;
    private readonly IPaymentAuditRepository _audit;
    
    public async Task<PaymentResult> InitiatePaymentAsync(
        Guid orderId, 
        PaymentMethodType method)
    {
        // 1. Load order
        var order = await _orders.GetByIdAsync(orderId);
        if (order == null) throw new OrderNotFoundException();
        
        // 2. Validate order state
        if (order.Status != "Created") 
            throw new InvalidOrderStateException();
        
        // 3. Update order status
        await _orders.UpdateStatusAsync(orderId, "PaymentInitiated");
        
        // 4. Log payment attempt
        await _audit.LogPaymentAttemptAsync(orderId, method);
        
        // 5. Call payment provider
        PaymentResult result = method switch
        {
            PaymentMethodType.CreditCard => 
                await _stripe.CreateCheckoutSessionAsync(order),
            PaymentMethodType.PayPal => 
                await _payPal.CreateOrderAsync(order),
            PaymentMethodType.BankTransfer => 
                await HandleBankTransferAsync(order),
            PaymentMethodType.CashOnDelivery => 
                await HandleCashOnDeliveryAsync(order),
            _ => throw new UnsupportedPaymentMethodException()
        };
        
        // 6. Update order with payment reference
        await _orders.UpdatePaymentReferenceAsync(orderId, result.PaymentId);
        
        return result;
    }
}
```

### 2. **Implement Proper Stripe Integration**

```csharp
public class StripePaymentService : IStripePaymentService
{
    private readonly IConfiguration _config;
    private readonly IOrderRepository _orders;
    private readonly ILogger<StripePaymentService> _logger;
    
    public async Task<PaymentResult> CreateCheckoutSessionAsync(Order order)
    {
        try
        {
            var lineItems = order.Lines.Select(line => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "eur",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = line.Product.Name,
                        Description = line.Product.Description
                    },
                    UnitAmount = (long)(line.UnitPrice * 100),
                },
                Quantity = line.Quantity,
            }).ToList();
            
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                ClientReferenceId = order.Id.ToString(), // ? Link to order
                SuccessUrl = $"{_config["App:FrontendUrl"]}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_config["App:FrontendUrl"]}/payment-cancel?order_id={order.Id}",
                Metadata = new Dictionary<string, string> // ? Add metadata
                {
                    { "order_id", order.Id.ToString() },
                    { "user_id", order.UserId }
                }
            };
            
            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"order-{order.Id}-{DateTime.UtcNow.Ticks}" // ? Prevent duplicates
            };
            
            var service = new SessionService();
            var session = await service.CreateAsync(options, requestOptions);
            
            _logger.LogInformation(
                "Stripe session created: {SessionId} for order {OrderId}", 
                session.Id, order.Id);
            
            return new PaymentResult
            {
                Success = true,
                PaymentId = session.Id,
                RedirectUrl = session.Url,
                Provider = "Stripe"
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe error for order {OrderId}", order.Id);
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
    
    public async Task<bool> HandleWebhookAsync(string json, string signature)
    {
        try
        {
            var webhookSecret = _config["Stripe:WebhookSecret"];
            var stripeEvent = EventUtility.ConstructEvent(
                json, signature, webhookSecret, throwOnApiVersionMismatch: false);
            
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    var session = stripeEvent.Data.Object as Session;
                    await HandlePaymentSuccessAsync(session);
                    break;
                    
                case "charge.refunded":
                    var charge = stripeEvent.Data.Object as Charge;
                    await HandleRefundAsync(charge);
                    break;
                    
                default:
                    _logger.LogInformation("Unhandled event type: {Type}", stripeEvent.Type);
                    break;
            }
            
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Webhook verification failed");
            return false;
        }
    }
    
    private async Task HandlePaymentSuccessAsync(Session session)
    {
        var orderId = Guid.Parse(session.ClientReferenceId);
        await _orders.UpdateStatusAsync(orderId, "Paid");
        await _orders.UpdatePaymentReferenceAsync(orderId, session.PaymentIntentId);
        
        _logger.LogInformation(
            "Payment completed for order {OrderId}, PaymentIntent {PaymentIntentId}", 
            orderId, session.PaymentIntentId);
        
        // Send confirmation email
        // Trigger fulfillment workflow
    }
}
```

### 3. **Implement Real PayPal Integration**

```csharp
public class PayPalPaymentService : IPayPalPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<PayPalPaymentService> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiry;
    
    public async Task<PaymentResult> CreateOrderAsync(Order order)
    {
        await EnsureAuthenticatedAsync();
        
        var paypalOrder = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = order.Id.ToString(),
                    amount = new
                    {
                        currency_code = "EUR",
                        value = order.TotalAmount.ToString("F2")
                    },
                    items = order.Lines.Select(line => new
                    {
                        name = line.Product.Name,
                        quantity = line.Quantity.ToString(),
                        unit_amount = new
                        {
                            currency_code = "EUR",
                            value = line.UnitPrice.ToString("F2")
                        }
                    })
                }
            },
            application_context = new
            {
                return_url = $"{_config["App:FrontendUrl"]}/payment-success",
                cancel_url = $"{_config["App:FrontendUrl"]}/payment-cancel"
            }
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, 
            $"{_config["PayPal:ApiUrl"]}/v2/checkout/orders");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        request.Content = JsonContent.Create(paypalOrder);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<PayPalOrderResponse>();
        var approveLink = result.Links.First(l => l.Rel == "approve").Href;
        
        return new PaymentResult
        {
            Success = true,
            PaymentId = result.Id,
            RedirectUrl = approveLink,
            Provider = "PayPal"
        };
    }
    
    private async Task EnsureAuthenticatedAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            return;
        
        var clientId = _config["PayPal:ClientId"];
        var secret = _config["PayPal:Secret"];
        var credentials = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{clientId}:{secret}"));
        
        var request = new HttpRequestMessage(HttpMethod.Post, 
            $"{_config["PayPal:ApiUrl"]}/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var tokenResponse = await response.Content
            .ReadFromJsonAsync<PayPalTokenResponse>();
        
        _accessToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);
    }
}
```

### 4. **Fix Cash on Delivery**

```csharp
private async Task<PaymentResult> HandleCashOnDeliveryAsync(Order order)
{
    // Update order status
    await _orders.UpdateStatusAsync(order.Id, "AwaitingShipment");
    await _orders.UpdatePaymentMethodAsync(order.Id, "CashOnDelivery");
    
    // Send confirmation email
    var user = await _userManager.GetUserByIdAsync(order.UserId);
    await _emailService.SendEmailAsync(
        user.Email,
        "Order Confirmation - Cash on Delivery",
        $@"<p>Thank you for your order!</p>
           <p>Order Reference: <b>{order.Reference}</b></p>
           <p>Total Amount: <b>{order.TotalAmount:F2} EUR</b></p>
           <p>Payment Method: <b>Cash on Delivery</b></p>
           <p>You will pay when your order is delivered.</p>");
    
    return new PaymentResult
    {
        Success = true,
        Message = "Order placed successfully. You will pay upon delivery.",
        OrderId = order.Id
    };
}
```

### 5. **Add Payment Configuration Class**

```csharp
// BlazorShop.Application\DTOs\Payment\PaymentSettings.cs
public class PaymentSettings
{
    public StripeSettings Stripe { get; set; } = new();
    public PayPalSettings PayPal { get; set; } = new();
    public BankTransferSettings BankTransfer { get; set; } = new();
    public AppSettings App { get; set; } = new();
}

public class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
}

public class PayPalSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = "https://api-m.sandbox.paypal.com"; // Sandbox by default
    public string WebhookId { get; set; } = string.Empty;
}

public class AppSettings
{
    public string FrontendUrl { get; set; } = "https://localhost:7258";
    public string ApiUrl { get; set; } = "https://localhost:7094";
}
```

**Updated appsettings.json**:
```json
{
  "Payment": {
    "App": {
      "FrontendUrl": "https://localhost:7258",
      "ApiUrl": "https://localhost:7094"
    },
    "Stripe": {
      "PublishableKey": ""  // Can be public
    },
    "PayPal": {
      "ApiUrl": "https://api-m.sandbox.paypal.com"
    },
    "BankTransfer": {
      "Iban": "BG00UNCR70001512345678",
      "Beneficiary": "BlazorShop Ltd.",
      "BankName": "Unicredit Bulbank",
      "AdditionalInfo": "Include the reference in the transfer reason."
    }
  }
}
```

**User Secrets** (development):
```bash
dotnet user-secrets set "Payment:Stripe:SecretKey" "sk_test_xxxxx"
dotnet user-secrets set "Payment:Stripe:WebhookSecret" "whsec_xxxxx"
dotnet user-secrets set "Payment:PayPal:ClientId" "xxxxx"
dotnet user-secrets set "Payment:PayPal:Secret" "xxxxx"
```

---

## ?? Mock Payment Implementation

For testing/development, create a mock payment service:

```csharp
// BlazorShop.Infrastructure\Services\MockPaymentService.cs
public class MockPaymentService : IPaymentService
{
    private readonly IOrderRepository _orders;
    private readonly ILogger<MockPaymentService> _logger;
    private static readonly Random _random = new();
    
    public async Task<ServiceResponse> Pay(
        decimal totalAmount, 
        IEnumerable<Product> cartProducts, 
        IEnumerable<ProcessCart> carts)
    {
        // Simulate payment processing delay
        await Task.Delay(1000);
        
        // Random success/failure for testing
        var success = _random.Next(100) < 90; // 90% success rate
        
        if (success)
        {
            _logger.LogInformation(
                "Mock payment successful for amount {Amount}", totalAmount);
            
            // In real scenario, this would be done by webhook
            // For mock, we do it immediately
            return new ServiceResponse(true, 
                "https://localhost:7258/payment-success?mock=true");
        }
        else
        {
            _logger.LogWarning(
                "Mock payment failed for amount {Amount}", totalAmount);
            return new ServiceResponse(false, "Mock payment declined");
        }
    }
}

// Register in DependencyInjection.cs
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, 
    IConfiguration config)
{
    var useMockPayments = config.GetValue<bool>("Payment:UseMockPayments");
    
    if (useMockPayments)
    {
        services.AddScoped<IPaymentService, MockPaymentService>();
        services.AddScoped<IPayPalPaymentService, MockPayPalPaymentService>();
    }
    else
    {
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IPayPalPaymentService, PayPalPaymentService>();
    }
    
    return services;
}
```

**Configuration**:
```json
{
  "Payment": {
    "UseMockPayments": true  // Enable mock for development
  }
}
```

---

## ?? Testing Strategy

### Unit Tests

```csharp
// BlazorShop.Tests\Application\Services\Payment\StripePaymentServiceTests.cs
public class StripePaymentServiceTests
{
    [Fact]
    public async Task CreateCheckoutSession_ShouldReturnValidUrl()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            TotalAmount = 100m,
            Lines = new List<OrderLine>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100m }
            }
        };
        
        var service = new StripePaymentService(_config, _orders, _logger);
        
        // Act
        var result = await service.CreateCheckoutSessionAsync(order);
        
        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.PaymentId);
        Assert.StartsWith("https://checkout.stripe.com", result.RedirectUrl);
    }
    
    [Fact]
    public async Task HandleWebhook_WithInvalidSignature_ShouldReturnFalse()
    {
        // Arrange
        var service = new StripePaymentService(_config, _orders, _logger);
        var json = "{}";
        var invalidSignature = "invalid_signature";
        
        // Act
        var result = await service.HandleWebhookAsync(json, invalidSignature);
        
        // Assert
        Assert.False(result);
    }
}
```

### Integration Tests

```csharp
// BlazorShop.Tests\Integration\PaymentFlowTests.cs
public class PaymentFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    [Fact]
    public async Task CompleteStripePayment_ShouldUpdateOrderStatus()
    {
        // 1. Create order
        var order = await CreateTestOrderAsync();
        
        // 2. Initiate payment
        var paymentResult = await InitiatePaymentAsync(order.Id, "CreditCard");
        Assert.True(paymentResult.Success);
        
        // 3. Simulate Stripe webhook
        await SimulateStripeWebhookAsync(paymentResult.PaymentId, "checkout.session.completed");
        
        // 4. Verify order status updated
        var updatedOrder = await GetOrderAsync(order.Id);
        Assert.Equal("Paid", updatedOrder.Status);
    }
    
    [Fact]
    public async Task BankTransferPayment_ShouldCreatePendingOrder()
    {
        // 1. Create order
        var order = await CreateTestOrderAsync();
        
        // 2. Select bank transfer
        var result = await InitiatePaymentAsync(order.Id, "BankTransfer");
        
        // 3. Verify order created with pending status
        var createdOrder = await GetOrderAsync(order.Id);
        Assert.Equal("Pending", createdOrder.Status);
        Assert.NotNull(createdOrder.Reference);
        Assert.StartsWith("BT-", createdOrder.Reference);
    }
}
```

---

## ?? Summary of Recommendations

### Immediate Actions (Do First)

1. ? **Move secrets to User Secrets / Environment Variables**
   - Remove hardcoded Stripe key from appsettings.json
   - Use `dotnet user-secrets` for local development

2. ? **Implement Stripe webhook handler with signature verification**
   - Add endpoint: `POST /api/payment/stripe/webhook`
   - Verify webhook signatures
   - Update order status on payment success

3. ? **Fix PayPal integration or disable it**
   - Either implement real PayPal integration
   - Or remove from payment methods table

4. ? **Create orders before payment for all methods**
   - Create Order entity first
   - Pass order ID to payment provider
   - Update order status based on payment result

5. ? **Use configuration-based URLs**
   - Add `App:FrontendUrl` to configuration
   - Use in Stripe success/cancel URLs

### Short-Term Improvements

6. ? **Implement Payment Orchestration Service**
   - Centralize payment logic
   - Add payment state machine
   - Implement payment audit logging

7. ? **Add idempotency keys**
   - Prevent duplicate payments
   - Use order ID + timestamp

8. ? **Fix Cash on Delivery**
   - Create order in database
   - Send confirmation email
   - Provide order tracking

### Long-Term Enhancements

9. ? **Add refund support**
   - Stripe refund API integration
   - Admin interface for refunds
   - Partial refund support

10. ? **Implement payment retry logic**
    - Handle transient failures
    - Exponential backoff
    - Maximum retry attempts

11. ? **Add comprehensive testing**
    - Unit tests for all payment services
    - Integration tests for payment flows
    - Mock payment service for development

12. ? **Monitoring and alerting**
    - Log payment attempts/failures
    - Alert on high failure rates
    - Track payment conversion rates

---

## ?? Configuration Checklist

### Development Environment

```bash
# Initialize user secrets
cd BlazorShop.Presentation/BlazorShop.API
dotnet user-secrets init

# Add Stripe secrets
dotnet user-secrets set "Payment:Stripe:SecretKey" "sk_test_xxxxxxxxxxxxxx"
dotnet user-secrets set "Payment:Stripe:WebhookSecret" "whsec_xxxxxxxxxxxxxx"
dotnet user-secrets set "Payment:Stripe:PublishableKey" "pk_test_xxxxxxxxxxxxxx"

# Add PayPal secrets
dotnet user-secrets set "Payment:PayPal:ClientId" "xxxxxxxxxxxxxx"
dotnet user-secrets set "Payment:PayPal:Secret" "xxxxxxxxxxxxxx"

# Add email secrets (if not already done)
dotnet user-secrets set "EmailSettings:From" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Username" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
```

### Production Environment (Azure)

```bash
# Set as Azure App Service Application Settings
az webapp config appsettings set --name blazorshop-api \
  --settings \
  Payment__Stripe__SecretKey="sk_live_xxxxx" \
  Payment__Stripe__WebhookSecret="whsec_xxxxx" \
  Payment__PayPal__ClientId="xxxxx" \
  Payment__PayPal__Secret="xxxxx"
```

---

## ?? Conclusion

The current payment system has:
- ? **Good foundation** with multiple payment methods
- ? **Best practice** in Bank Transfer implementation
- ? **Critical security issues** with hardcoded secrets
- ? **Incomplete implementations** for Stripe/PayPal
- ? **Missing webhooks** for payment confirmation

**Priority**: Fix security issues first, then implement proper Stripe integration with webhooks, then decide on PayPal (implement properly or remove).

The provided recommendations will transform this from a proof-of-concept to a production-ready payment system.
