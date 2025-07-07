
-- Phone Inventory
INSERT INTO PhoneInventory (phone_id, quantity)
VALUES (1, 10),
       (2, 5);

-- Material Inventory
INSERT INTO MaterialInventory (material_id, available_quantity_in_kg)
VALUES (1, 100000),
       (2, 50000),
       (3, 150000),
       (4, 25000),
       (5, 60000);

-- Orders
INSERT INTO Orders (order_number, order_status_id, created_At, company_id)
VALUES ('6d71f7b0-a18d-4c6d-83d0-d417be41e51c', 1, '2025-06-29 20:00:43', 1),
       ('d105b904-e58f-4baa-b4c3-f1b2aef0a0e1', 2, '2025-06-29 20:00:43', 2);

-- Order Items
INSERT INTO OrderItems (order_id, material_id, quantity_in_kg, price_per_kg)
VALUES (1, 1, 20000, 310.00),
       (2, 1, 10000, 155.00),
       (2, 2, 10000, 257.50),
       (2, 3, 20000, 700.00);