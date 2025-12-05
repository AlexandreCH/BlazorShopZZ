namespace BlazorShop.Infrastructure.Data
{
    using BlazorShop.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                // Check if we already have data
                if (await context.Categories.AnyAsync() || await context.Products.AnyAsync())
                {
                    logger.LogInformation("Database already contains seed data. Skipping seed.");
                    return;
                }

                logger.LogInformation("Seeding database with real estate property data...");

                // Seed Property Categories
                var categories = new[]
                {
                    new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"), Name = "Urban Apartments" },
                    new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"), Name = "Townhouses" },
                    new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"), Name = "Luxury Villas" },
                    new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440004"), Name = "Penthouses" },
                    new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440005"), Name = "Studio Apartments" },
                    new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440006"), Name = "Commercial Properties" },
                    new Category { Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440007"), Name = "Countryside Houses" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Seed Properties (Products)
                var properties = new[]
                {
                    // Urban Apartments
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440001"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                        Name = "Modern Downtown Loft",
                        Description = "Stunning 2-bedroom loft in the heart of downtown. Open concept design, floor-to-ceiling windows, hardwood floors, and access to rooftop terrace. Walking distance to restaurants, shops, and public transit.",
                        Price = 485000m,
                        Quantity = 1,
                        Image = "/uploads/properties/downtown-loft.jpg",
                        CreatedOn = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440002"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                        Name = "City View Apartment",
                        Description = "Spacious 3-bedroom apartment with breathtaking city views. Features include modern kitchen, in-suite laundry, balcony, and secure underground parking. Close to schools and parks.",
                        Price = 625000m,
                        Quantity = 1,
                        Image = "/uploads/properties/city-view-apt.jpg",
                        CreatedOn = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440003"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                        Name = "Riverside Condominium",
                        Description = "Elegant 2-bedroom condo overlooking the river. Granite countertops, stainless steel appliances, gym, and concierge service. Pet-friendly building with green spaces.",
                        Price = 545000m,
                        Quantity = 1,
                        Image = "/uploads/properties/riverside-condo.jpg",
                        CreatedOn = DateTime.UtcNow
                    },

                    // Townhouses
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440004"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                        Name = "Contemporary Suburban Townhouse",
                        Description = "Beautiful 3-bedroom, 2.5-bath townhouse in family-friendly neighborhood. Features include finished basement, attached garage, private backyard, and modern finishes throughout.",
                        Price = 565000m,
                        Quantity = 1,
                        Image = "/uploads/properties/suburban-townhouse.jpg",
                        CreatedOn = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440005"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                        Name = "Executive Townhouse with Rooftop",
                        Description = "Luxurious 4-bedroom executive townhouse with rooftop terrace. Open-concept main floor, chef's kitchen with quartz counters, high ceilings, and smart home features.",
                        Price = 785000m,
                        Quantity = 1,
                        Image = "/uploads/properties/executive-townhouse.jpg",
                        CreatedOn = DateTime.UtcNow
                    },

                    // Luxury Villas
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440006"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"),
                        Name = "Waterfront Luxury Villa",
                        Description = "Spectacular 5-bedroom waterfront villa with private dock. Features infinity pool, home theater, wine cellar, gourmet kitchen, and panoramic lake views. 4-car garage and smart home automation.",
                        Price = 2850000m,
                        Quantity = 1,
                        Image = "/uploads/properties/waterfront-villa.jpg",
                        CreatedOn = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440007"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"),
                        Name = "Mediterranean Estate",
                        Description = "Exquisite 6-bedroom Mediterranean-style estate on 2 acres. Custom architecture, marble floors, outdoor kitchen, saltwater pool, guest house, and landscaped gardens. Gated community.",
                        Price = 3250000m,
                        Quantity = 1,
                        Image = "/uploads/properties/mediterranean-estate.jpg",
                        CreatedOn = DateTime.UtcNow
                    },

                    // Penthouses
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440008"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440004"),
                        Name = "Sky Tower Penthouse",
                        Description = "Luxurious 3-bedroom penthouse on 35th floor. 360-degree city views, 2,500 sq ft private terrace, floor-to-ceiling windows, Italian marble bathrooms, and premium appliances. 24/7 concierge.",
                        Price = 1950000m,
                        Quantity = 1,
                        Image = "/uploads/properties/sky-tower-penthouse.jpg",
                        CreatedOn = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440009"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440004"),
                        Name = "Downtown Executive Penthouse",
                        Description = "Ultra-modern 4-bedroom penthouse in prestigious building. Smart home technology, private elevator access, spa-like bathrooms, chef's kitchen, and wraparound terrace with outdoor fireplace.",
                        Price = 2450000m,
                        Quantity = 1,
                        Image = "/uploads/properties/executive-penthouse.jpg",
                        CreatedOn = DateTime.UtcNow
                    },

                    // Studio Apartments
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440010"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440005"),
                        Name = "Downtown Studio Loft",
                        Description = "Efficient and stylish studio apartment perfect for urban living. Murphy bed, modern kitchenette, high ceilings, large windows. Ideal for first-time buyers or investors.",
                        Price = 285000m,
                        Quantity = 1,
                        Image = "/uploads/properties/studio-loft.jpg",
                        CreatedOn = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440011"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440005"),
                        Name = "Compact Urban Studio",
                        Description = "Cozy studio with smart storage solutions. Recently renovated with new flooring, updated kitchen, and bathroom. Walking distance to transit and entertainment district.",
                        Price = 245000m,
                        Quantity = 1,
                        Image = "/uploads/properties/compact-studio.jpg",
                        CreatedOn = DateTime.UtcNow
                    },

                    // Commercial Properties
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440012"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440006"),
                        Name = "Prime Retail Space",
                        Description = "High-traffic retail storefront in bustling shopping district. 2,200 sq ft, excellent visibility, ample parking. Perfect for restaurant, boutique, or professional office. Lease-to-own available.",
                        Price = 895000m,
                        Quantity = 1,
                        Image = "/uploads/properties/retail-space.jpg",
                        CreatedOn = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440013"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440006"),
                        Name = "Modern Office Building",
                        Description = "Contemporary 3-story office building in business district. 8,500 sq ft total, elevator access, parking for 30 vehicles, conference facilities. Currently 80% occupied with stable tenants.",
                        Price = 2750000m,
                        Quantity = 1,
                        Image = "/uploads/properties/office-building.jpg",
                        CreatedOn = DateTime.UtcNow
                    },

                    // Countryside Houses
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440014"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440007"),
                        Name = "Charming Country Farmhouse",
                        Description = "Picturesque 4-bedroom farmhouse on 5 acres with barn and stables. Original hardwood floors, stone fireplace, wraparound porch, and modern updates. Ideal for hobby farm or equestrian lifestyle.",
                        Price = 725000m,
                        Quantity = 1,
                        Image = "/uploads/properties/country-farmhouse.jpg",
                        CreatedOn = DateTime.UtcNow
                    },
                    new Product
                    {
                        Id = Guid.Parse("650e8400-e29b-41d4-a716-446655440015"),
                        CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440007"),
                        Name = "Mountain View Retreat",
                        Description = "Serene 3-bedroom countryside home with stunning mountain views. Vaulted ceilings, large deck, wood-burning stove, and 3-car garage. Situated on 10 private acres with hiking trails.",
                        Price = 865000m,
                        Quantity = 1,
                        Image = "/uploads/properties/mountain-retreat.jpg",
                        CreatedOn = DateTime.UtcNow
                    }
                };

                await context.Products.AddRangeAsync(properties);
                await context.SaveChangesAsync();

                // Seed Property Features (ProductVariants - representing different features/packages)
                var propertyFeatures = new[]
                {
                    // Modern Downtown Loft - Upgrade Packages
                    new ProductVariant
                    {
                        Id = Guid.Parse("750e8400-e29b-41d4-a716-446655440001"),
                        ProductId = Guid.Parse("650e8400-e29b-41d4-a716-446655440001"),
                        SizeScale = 0, // 0 = Feature Package
                        SizeValue = "Base",
                        Color = "Standard",
                        Stock = 1,
                        IsDefault = true,
                        Price = 485000m,
                        Sku = "DT-LOFT-BASE"
                    },
                    new ProductVariant
                    {
                        Id = Guid.Parse("750e8400-e29b-41d4-a716-446655440002"),
                        ProductId = Guid.Parse("650e8400-e29b-41d4-a716-446655440001"),
                        SizeScale = 0,
                        SizeValue = "Premium",
                        Color = "Upgraded",
                        Stock = 1,
                        IsDefault = false,
                        Price = 515000m,
                        Sku = "DT-LOFT-PREM"
                    },

                    // Contemporary Suburban Townhouse - Finishing Options
                    new ProductVariant
                    {
                        Id = Guid.Parse("750e8400-e29b-41d4-a716-446655440003"),
                        ProductId = Guid.Parse("650e8400-e29b-41d4-a716-446655440004"),
                        SizeScale = 0,
                        SizeValue = "Standard",
                        Color = "Light Oak",
                        Stock = 1,
                        IsDefault = true,
                        Price = 565000m,
                        Sku = "SUB-TH-STD"
                    },
                    new ProductVariant
                    {
                        Id = Guid.Parse("750e8400-e29b-41d4-a716-446655440004"),
                        ProductId = Guid.Parse("650e8400-e29b-41d4-a716-446655440004"),
                        SizeScale = 0,
                        SizeValue = "Deluxe",
                        Color = "Dark Walnut",
                        Stock = 1,
                        IsDefault = false,
                        Price = 595000m,
                        Sku = "SUB-TH-DLX"
                    },

                    // Waterfront Luxury Villa - Customization Levels
                    new ProductVariant
                    {
                        Id = Guid.Parse("750e8400-e29b-41d4-a716-446655440005"),
                        ProductId = Guid.Parse("650e8400-e29b-41d4-a716-446655440006"),
                        SizeScale = 0,
                        SizeValue = "Signature",
                        Color = "Designer",
                        Stock = 1,
                        IsDefault = true,
                        Price = 2850000m,
                        Sku = "WF-VILLA-SIG"
                    },
                    new ProductVariant
                    {
                        Id = Guid.Parse("750e8400-e29b-41d4-a716-446655440006"),
                        ProductId = Guid.Parse("650e8400-e29b-41d4-a716-446655440006"),
                        SizeScale = 0,
                        SizeValue = "Ultra-Luxury",
                        Color = "Bespoke",
                        Stock = 1,
                        IsDefault = false,
                        Price = 3150000m,
                        Sku = "WF-VILLA-ULX"
                    },

                    // Downtown Studio Loft - Package Options
                    new ProductVariant
                    {
                        Id = Guid.Parse("750e8400-e29b-41d4-a716-446655440007"),
                        ProductId = Guid.Parse("650e8400-e29b-41d4-a716-446655440010"),
                        SizeScale = 0,
                        SizeValue = "Furnished",
                        Color = "Modern",
                        Stock = 1,
                        IsDefault = false,
                        Price = 295000m,
                        Sku = "STU-LOFT-FURN"
                    },
                    new ProductVariant
                    {
                        Id = Guid.Parse("750e8400-e29b-41d4-a716-446655440008"),
                        ProductId = Guid.Parse("650e8400-e29b-41d4-a716-446655440010"),
                        SizeScale = 0,
                        SizeValue = "Unfurnished",
                        Color = "Standard",
                        Stock = 1,
                        IsDefault = true,
                        Price = 285000m,
                        Sku = "STU-LOFT-UNFURN"
                    }
                };

                await context.ProductVariants.AddRangeAsync(propertyFeatures);
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeding completed successfully with {CategoryCount} categories and {PropertyCount} properties!", 
                    categories.Length, properties.Length);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
