-- ==========================================
-- REAL ESTATE SEED DATA FOR BLAZORSHOP
-- ==========================================
-- Connect to blazorshop database
\c blazorshop

-- ==========================================
-- 1. INSERT PROPERTY CATEGORIES
-- ==========================================
INSERT INTO "Categories" ("Id", "Name") VALUES
('550e8400-e29b-41d4-a716-446655440001', 'Urban Apartments'),
('550e8400-e29b-41d4-a716-446655440002', 'Townhouses'),
('550e8400-e29b-41d4-a716-446655440003', 'Luxury Villas'),
('550e8400-e29b-41d4-a716-446655440004', 'Penthouses'),
('550e8400-e29b-41d4-a716-446655440005', 'Studio Apartments'),
('550e8400-e29b-41d4-a716-446655440006', 'Commercial Properties'),
('550e8400-e29b-41d4-a716-446655440007', 'Countryside Houses')
ON CONFLICT ("Id") DO NOTHING;

-- ==========================================
-- 2. INSERT PROPERTY LISTINGS (PRODUCTS)
-- ==========================================

-- Urban Apartments
INSERT INTO "Products" ("Id", "CategoryId", "Name", "Description", "Price", "Quantity", "Image", "CreatedOn") VALUES
('650e8400-e29b-41d4-a716-446655440001', '550e8400-e29b-41d4-a716-446655440001', 
 'Modern Downtown Loft', 
 'Stunning 2-bedroom loft in the heart of downtown. Open concept design, floor-to-ceiling windows, hardwood floors, and access to rooftop terrace. Walking distance to restaurants, shops, and public transit.', 
 485000, 1, '/uploads/properties/downtown-loft.jpg', NOW()),

('650e8400-e29b-41d4-a716-446655440002', '550e8400-e29b-41d4-a716-446655440001', 
 'City View Apartment', 
 'Spacious 3-bedroom apartment with breathtaking city views. Features include modern kitchen, in-suite laundry, balcony, and secure underground parking. Close to schools and parks.', 
 625000, 1, '/uploads/properties/city-view-apt.jpg', NOW()),

('650e8400-e29b-41d4-a716-446655440003', '550e8400-e29b-41d4-a716-446655440001', 
 'Riverside Condominium', 
 'Elegant 2-bedroom condo overlooking the river. Granite countertops, stainless steel appliances, gym, and concierge service. Pet-friendly building with green spaces.', 
 545000, 1, '/uploads/properties/riverside-condo.jpg', NOW()),

-- Townhouses
('650e8400-e29b-41d4-a716-446655440004', '550e8400-e29b-41d4-a716-446655440002', 
 'Contemporary Suburban Townhouse', 
 'Beautiful 3-bedroom, 2.5-bath townhouse in family-friendly neighborhood. Features include finished basement, attached garage, private backyard, and modern finishes throughout.', 
 565000, 1, '/uploads/properties/suburban-townhouse.jpg', NOW()),

('650e8400-e29b-41d4-a716-446655440005', '550e8400-e29b-41d4-a716-446655440002', 
 'Executive Townhouse with Rooftop', 
 'Luxurious 4-bedroom executive townhouse with rooftop terrace. Open-concept main floor, chef''s kitchen with quartz counters, high ceilings, and smart home features.', 
 785000, 1, '/uploads/properties/executive-townhouse.jpg', NOW()),

-- Luxury Villas
('650e8400-e29b-41d4-a716-446655440006', '550e8400-e29b-41d4-a716-446655440003', 
 'Waterfront Luxury Villa', 
 'Spectacular 5-bedroom waterfront villa with private dock. Features infinity pool, home theater, wine cellar, gourmet kitchen, and panoramic lake views. 4-car garage and smart home automation.', 
 2850000, 1, '/uploads/properties/waterfront-villa.jpg', NOW()),

('650e8400-e29b-41d4-a716-446655440007', '550e8400-e29b-41d4-a716-446655440003', 
 'Mediterranean Estate', 
 'Exquisite 6-bedroom Mediterranean-style estate on 2 acres. Custom architecture, marble floors, outdoor kitchen, saltwater pool, guest house, and landscaped gardens. Gated community.', 
 3250000, 1, '/uploads/properties/mediterranean-estate.jpg', NOW()),

-- Penthouses
('650e8400-e29b-41d4-a716-446655440008', '550e8400-e29b-41d4-a716-446655440004', 
 'Sky Tower Penthouse', 
 'Luxurious 3-bedroom penthouse on 35th floor. 360-degree city views, 2,500 sq ft private terrace, floor-to-ceiling windows, Italian marble bathrooms, and premium appliances. 24/7 concierge.', 
 1950000, 1, '/uploads/properties/sky-tower-penthouse.jpg', NOW()),

('650e8400-e29b-41d4-a716-446655440009', '550e8400-e29b-41d4-a716-446655440004', 
 'Downtown Executive Penthouse', 
 'Ultra-modern 4-bedroom penthouse in prestigious building. Smart home technology, private elevator access, spa-like bathrooms, chef''s kitchen, and wraparound terrace with outdoor fireplace.', 
 2450000, 1, '/uploads/properties/executive-penthouse.jpg', NOW()),

-- Studio Apartments
('650e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440005', 
 'Downtown Studio Loft', 
 'Efficient and stylish studio apartment perfect for urban living. Murphy bed, modern kitchenette, high ceilings, large windows. Ideal for first-time buyers or investors.', 
 285000, 1, '/uploads/properties/studio-loft.jpg', NOW()),

('650e8400-e29b-41d4-a716-446655440011', '550e8400-e29b-41d4-a716-446655440005', 
 'Compact Urban Studio', 
 'Cozy studio with smart storage solutions. Recently renovated with new flooring, updated kitchen, and bathroom. Walking distance to transit and entertainment district.', 
 245000, 1, '/uploads/properties/compact-studio.jpg', NOW()),

-- Commercial Properties
('650e8400-e29b-41d4-a716-446655440012', '550e8400-e29b-41d4-a716-446655440006', 
 'Prime Retail Space', 
 'High-traffic retail storefront in bustling shopping district. 2,200 sq ft, excellent visibility, ample parking. Perfect for restaurant, boutique, or professional office. Lease-to-own available.', 
 895000, 1, '/uploads/properties/retail-space.jpg', NOW()),

('650e8400-e29b-41d4-a716-446655440013', '550e8400-e29b-41d4-a716-446655440006', 
 'Modern Office Building', 
 'Contemporary 3-story office building in business district. 8,500 sq ft total, elevator access, parking for 30 vehicles, conference facilities. Currently 80% occupied with stable tenants.', 
 2750000, 1, '/uploads/properties/office-building.jpg', NOW()),

-- Countryside Houses
('650e8400-e29b-41d4-a716-446655440014', '550e8400-e29b-41d4-a716-446655440007', 
 'Charming Country Farmhouse', 
 'Picturesque 4-bedroom farmhouse on 5 acres with barn and stables. Original hardwood floors, stone fireplace, wraparound porch, and modern updates. Ideal for hobby farm or equestrian lifestyle.', 
 725000, 1, '/uploads/properties/country-farmhouse.jpg', NOW()),

('650e8400-e29b-41d4-a716-446655440015', '550e8400-e29b-41d4-a716-446655440007', 
 'Mountain View Retreat', 
 'Serene 3-bedroom countryside home with stunning mountain views. Vaulted ceilings, large deck, wood-burning stove, and 3-car garage. Situated on 10 private acres with hiking trails.', 
 865000, 1, '/uploads/properties/mountain-retreat.jpg', NOW())

ON CONFLICT ("Id") DO NOTHING;

-- ==========================================
-- 3. INSERT PROPERTY FEATURES/PACKAGES (PRODUCT VARIANTS)
-- ==========================================

INSERT INTO "ProductVariants" ("Id", "ProductId", "SizeScale", "SizeValue", "Color", "Stock", "IsDefault", "Price", "Sku") VALUES
-- Modern Downtown Loft - Upgrade Packages
('750e8400-e29b-41d4-a716-446655440001', '650e8400-e29b-41d4-a716-446655440001', 0, 'Base', 'Standard', 1, true, 485000, 'DT-LOFT-BASE'),
('750e8400-e29b-41d4-a716-446655440002', '650e8400-e29b-41d4-a716-446655440001', 0, 'Premium', 'Upgraded', 1, false, 515000, 'DT-LOFT-PREM'),

-- Contemporary Suburban Townhouse - Finishing Options
('750e8400-e29b-41d4-a716-446655440003', '650e8400-e29b-41d4-a716-446655440004', 0, 'Standard', 'Light Oak', 1, true, 565000, 'SUB-TH-STD'),
('750e8400-e29b-41d4-a716-446655440004', '650e8400-e29b-41d4-a716-446655440004', 0, 'Deluxe', 'Dark Walnut', 1, false, 595000, 'SUB-TH-DLX'),

-- Waterfront Luxury Villa - Customization Levels
('750e8400-e29b-41d4-a716-446655440005', '650e8400-e29b-41d4-a716-446655440006', 0, 'Signature', 'Designer', 1, true, 2850000, 'WF-VILLA-SIG'),
('750e8400-e29b-41d4-a716-446655440006', '650e8400-e29b-41d4-a716-446655440006', 0, 'Ultra-Luxury', 'Bespoke', 1, false, 3150000, 'WF-VILLA-ULX'),

-- Downtown Studio Loft - Package Options
('750e8400-e29b-41d4-a716-446655440007', '650e8400-e29b-41d4-a716-446655440010', 0, 'Furnished', 'Modern', 1, false, 295000, 'STU-LOFT-FURN'),
('750e8400-e29b-41d4-a716-446655440008', '650e8400-e29b-41d4-a716-446655440010', 0, 'Unfurnished', 'Standard', 1, true, 285000, 'STU-LOFT-UNFURN')

ON CONFLICT ("Id", "ProductId", "SizeScale", "SizeValue") DO NOTHING;

-- ==========================================
-- 4. VERIFY SEEDED DATA
-- ==========================================
SELECT 
    '========== REAL ESTATE DATABASE SUMMARY ==========' as info
UNION ALL
SELECT CONCAT('Categories: ', COUNT(*), ' property types') FROM "Categories"
UNION ALL
SELECT CONCAT('Properties: ', COUNT(*), ' listings') FROM "Products"
UNION ALL
SELECT CONCAT('Property Features: ', COUNT(*), ' variants') FROM "ProductVariants"
UNION ALL
SELECT CONCAT('Payment Methods: ', COUNT(*), ' options') FROM "PaymentMethods"
UNION ALL
SELECT CONCAT('User Roles: ', COUNT(*), ' roles') FROM "AspNetRoles"
UNION ALL
SELECT '==================================================';

-- View all property categories with listing count
SELECT 
    c."Name" as "Property Type",
    COUNT(p."Id") as "Available Listings",
    TO_CHAR(MIN(p."Price"), 'FM$999,999,999') as "Starting From",
    TO_CHAR(MAX(p."Price"), 'FM$999,999,999') as "Up To"
FROM "Categories" c
LEFT JOIN "Products" p ON c."Id" = p."CategoryId"
GROUP BY c."Id", c."Name"
ORDER BY MIN(p."Price") NULLS LAST;

-- View sample properties by price range
SELECT 
    c."Name" as "Category",
    p."Name" as "Property",
    TO_CHAR(p."Price", 'FM$999,999,999') as "Price",
    CASE 
        WHEN p."Price" < 300000 THEN 'Entry Level'
        WHEN p."Price" < 700000 THEN 'Mid Range'
        WHEN p."Price" < 1500000 THEN 'Premium'
        ELSE 'Luxury'
    END as "Market Segment"
FROM "Products" p
JOIN "Categories" c ON p."CategoryId" = c."Id"
ORDER BY p."Price" ASC;
