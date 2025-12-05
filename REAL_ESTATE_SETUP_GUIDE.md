# BlazorShop Real Estate Platform - Setup Guide

## ? Build Issues Fixed

### 1. Program.cs Async/Await Error - FIXED ?
**Error:** `CS4033: The 'await' operator can only be used within an async method`

**Solution Applied:**
- Changed `Main` method signature from `void` to `async Task`
- Changed `app.Run()` to `await app.RunAsync()`
- Changed `Log.CloseAndFlush()` to `await Log.CloseAndFlushAsync()`

### 2. GenericRepository Null Reference Warning - FIXED ?
**Warning:** `CS8603: Possible null reference return`

**Solution Applied:**
- Updated `GetByIdAsync` return type to `Task<TEntity?>` (nullable)
- Updated `IGenericRepository<TEntity>` interface to match
- Removed unnecessary null-forgiving operator `!`

## ?? Real Estate Data Structure

### Property Categories (7 types)
1. **Urban Apartments** - City living properties
2. **Townhouses** - Suburban family homes
3. **Luxury Villas** - High-end estates
4. **Penthouses** - Premium high-rise units
5. **Studio Apartments** - Compact urban living
6. **Commercial Properties** - Business real estate
7. **Countryside Houses** - Rural/countryside properties

### Sample Properties (15 listings)
- **Price Range:** $245,000 - $3,250,000
- **Market Segments:**
  - Entry Level: < $300,000
  - Mid Range: $300,000 - $700,000
  - Premium: $700,000 - $1,500,000
  - Luxury: > $1,500,000

### Property Features/Packages
Each property can have multiple variants representing:
- Upgrade packages (Base, Premium, Deluxe, Ultra-Luxury)
- Finishing options (Light Oak, Dark Walnut, Designer, Bespoke)
- Furnished/Unfurnished options
- Customization levels

## ?? How to Run

### Option 1: Using C# Seeder (Recommended)
```powershell
cd C:\Users\alexa\source\repos\Blazor-gRpc\E-cmmrc\BlazorShopZZ
dotnet run --project BlazorShop.Presentation\BlazorShop.API
```

The seeder will automatically:
1. Apply EF Core migrations
2. Create all tables
3. Seed property categories
4. Seed 15 property listings
5. Seed property feature packages

### Option 2: Direct SQL Seeding
```powershell
psql -U postgres -d blazorshop -f BlazorShop.Infrastructure/Data/RealEstateSeedData.sql
```

## ?? Database Verification

### Check Seeded Data
```sql
\c blazorshop

-- Count records
SELECT 
    'Categories' as table_name, COUNT(*) FROM "Categories"
UNION ALL
SELECT 'Properties', COUNT(*) FROM "Products"
UNION ALL
SELECT 'Property Features', COUNT(*) FROM "ProductVariants"
UNION ALL
SELECT 'Payment Methods', COUNT(*) FROM "PaymentMethods"
UNION ALL
SELECT 'Roles', COUNT(*) FROM "AspNetRoles";

-- View all properties by category
SELECT 
    c."Name" as "Category",
    p."Name" as "Property",
    p."Price",
    p."Quantity" as "Available"
FROM "Products" p
JOIN "Categories" c ON p."CategoryId" = c."Id"
ORDER BY c."Name", p."Price";
```

### Expected Results
```
Categories: 7 property types
Properties: 15 listings
Property Features: 8 variants
Payment Methods: 4 options
Roles: 2 roles (Admin, User)
```

## ?? Testing Your Blazor WebAssembly App

1. **Start the API:**
   ```powershell
   dotnet run --project BlazorShop.Presentation\BlazorShop.API
   ```
   - API runs at: `https://localhost:7094`
   - Swagger UI: `https://localhost:7094/swagger`

2. **Start the Web App:**
   ```powershell
   dotnet run --project BlazorShop.Presentation\BlazorShop.Web
   ```
   - Web runs at: `https://localhost:7258`

3. **Or use AppHost (runs both):**
   ```powershell
   dotnet run --project BlazorShop.AppHost
   ```

## ?? User Registration

### First User (Becomes Admin)
1. Navigate to `/authentication/register`
2. Register with:
   - Full Name
   - Email
   - Password (8+ chars, uppercase, lowercase, digit, special char)
3. Confirm email (check console logs for confirmation link)
4. First registered user automatically gets **Admin** role

### Subsequent Users
- Get **User** role by default
- Same registration process

## ?? Real Estate Features

### Browse Properties
- Home page shows all property listings
- Filter by category
- Search by property name
- View property details

### Property Details
- Full description
- Price information
- Available upgrade packages
- Property features

### Admin Features (First User)
- Add new properties
- Edit existing properties
- Manage property categories
- Add property variants/packages
- View and manage orders

### User Features
- Browse properties
- Add to cart (interest/inquiry)
- View order history
- Update profile

## ??? Database Structure

### Core Tables
- `Categories` - Property types
- `Products` - Property listings
- `ProductVariants` - Property features/packages
- `PaymentMethods` - Payment options (seeded)
- `AspNetRoles` - User roles (seeded)
- `AspNetUsers` - Registered users
- `Orders` - Property inquiries/orders
- `OrderLines` - Order details
- `NewsletterSubscribers` - Email subscriptions

## ?? Troubleshooting

### Migration Issues
```powershell
# Reset database
DROP DATABASE blazorshop;
CREATE DATABASE blazorshop;

# Reapply migrations
cd BlazorShop.Infrastructure
dotnet ef database update --startup-project ..\BlazorShop.Presentation\BlazorShop.API
```

### Connection Issues
Check `appsettings.json` in `BlazorShop.API`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=blazorshop;Username=postgres;Password=postgres"
  }
}
```

### View Logs
Logs are written to:
- Console output
- `BlazorShop.API/log/log.txt` (daily rolling)

## ?? Next Steps

1. ? Build successful
2. ? Database initialized
3. ? Real estate data seeded
4. ?? Run the application
5. ?? Register first admin user
6. ?? Start browsing properties!

## ?? Success Indicators

When everything is working correctly, you should see:
- ? No build errors
- ? Database tables created
- ? 7 property categories
- ? 15 property listings
- ? 8 property feature packages
- ? 4 payment methods
- ? 2 user roles (Admin, User)
- ? API running on port 7094
- ? Web app running on port 7258

---

**Your BlazorShop is now a Real Estate Property Platform!** ????
