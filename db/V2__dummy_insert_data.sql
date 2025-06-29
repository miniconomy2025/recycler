-- Roles
INSERT INTO Role (name)
VALUES ('Supplier'),
       ('Logistics');

-- Order Statuses
INSERT INTO OrderStatus (name)
VALUES ('Pending'),
       ('Paid'),
       ('Cancelled Due To Non-Payment'),
       ('Cancelled and Refunded Due To No Stock'),
       ('Out For Delivery'),
       ('Delivered'),
       ('Completed');

-- PhoneBrand
INSERT INTO PhoneBrand (brand_name)
VALUES ('Pear'),
       ('Sumsang');

-- Phones
INSERT INTO Phone (phone_brand_id, model)
VALUES (1, 'ePhone'),
       (1, 'ePhone plus'),
       (1, 'ePhone pro max'),
       (2, 'Cosmos Z25'),
       (2, 'Cosmos Z25 ultra'),
       (2, 'Cosmos Z25 FE');

-- Raw Materials
INSERT INTO RawMaterial (name, price)
VALUES ('Aluminum', 15.50),
       ('Copper', 25.75),
       ('Silicone', 35.00),
       ('Sand', 45.55),
       ('Plastic', 5.75);

-- Phone Parts
INSERT INTO PhoneParts (name)
VALUES ('Case'),
       ('Screen'),
       ('Electronics');

-- Phone to Phone Part Ratio
INSERT INTO PhoneToPhonePartRatio (phone_id, phone_part_id, phone_part_quantity_per_phone)
VALUES (1, 1, 1),
       (1, 2, 1),
       (1, 3, 5),
       (2, 1, 1),
       (2, 2, 2),
       (2, 3, 10);

-- Phone Part to Raw Material Ratio
INSERT INTO PhonePartToRawMaterialRatio (phone_part_id, raw_material_id, raw_material_quantity_per_phone_part)
VALUES (1, 2, 4),
       (1, 3, 3),
       (2, 5, 4),
       (2, 1, 7),
       (3, 2, 2),
       (3, 4, 7);

-- Companies
INSERT INTO Companies (company_number, role_id, name, key_id)
VALUES ('7a8930d0-7f0f-4142-b51f-51cba40144db', 1, 'EcoSupply Inc.', 1001),
       ('5e59a150-3044-44ac-a52e-1afeaaf15719', 2, 'GreenCycle Ltd.', 1002);

-- Phone Inventory
INSERT INTO PhoneInventory (phone_id, quantity_in_thousands)
VALUES (1, 10.00),
       (2, 5.00);

-- Material Inventory
INSERT INTO MaterialInventory (material_id, available_quantity_in_tonnes)
VALUES (1, 100.00),
       (2, 50.00),
       (3, 150.00),
       (4, 25.00),
       (5, 60.00);

-- Orders
INSERT INTO Orders (order_number, order_status_id, created_At, company_id)
VALUES ('6d71f7b0-a18d-4c6d-83d0-d417be41e51c', 1, '2025-06-29 20:00:43', 1),
       ('d105b904-e58f-4baa-b4c3-f1b2aef0a0e1', 2, '2025-06-29 20:00:43', 2);

-- Order Items
INSERT INTO OrderItems (order_id, material_id, quantity_in_tonnes, price)
VALUES (1, 1, 20, 310.00),
       (2, 1, 10, 155.00),
       (2, 2, 10, 257.50),
       (2, 3, 20, 700.00);