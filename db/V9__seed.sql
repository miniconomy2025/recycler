INSERT INTO Role (name) VALUES
  ('Supplier'),
  ('Logistics'),
  ('Bank');


INSERT INTO RawMaterial (name, price) VALUES
  ('Copper', 45.00),
  ('Silicon', 55.00),
  ('Sand', 10.00),
  ('Plastic', 15.00),
  ('Aluminum', 25.00);


INSERT INTO OrderStatus (name) VALUES
  ('Pending'),
  ('Approved'),
  ('Rejected'),
  ('Shipped'),
  ('Delivered'),
  ('Refunded');


INSERT INTO PhoneParts (name) VALUES
  ('Screen'),
  ('Case'),
  ('Electronics');


INSERT INTO PhoneBrand (brand_name) VALUES
    ('Pear'),
    ('Sumsang');


INSERT INTO Phone (phone_brand_id, model) VALUES
    (1, 'ePhone'),
    (1, 'ePhone plus'),
    (1, 'ePhone pro max'),
    (2, 'Cosmos Z25'),
    (2, 'Cosmos Z25 ultra'),
    (2, 'Cosmos Z25 FE');


INSERT INTO PhoneToPhonePartRatio (phone_id, phone_part_id, phone_part_quantity_per_phone) VALUES (1, 1, 1),
    (1, 2, 1),
    (1, 3, 5),
    (2, 1, 1),
    (2, 2, 2),
    (2, 3, 10);


INSERT INTO PhonePartToRawMaterialRatio (phone_part_id, raw_material_id, raw_material_quantity_per_phone_part) VALUES
    (1, 2, 4),
    (1, 3, 3),
    (2, 5, 4),
    (2, 1, 7),
    (3, 2, 2),
    (3, 4, 7);


INSERT INTO Companies (role_id, name, key_id) VALUES
    (1, 'electronics_supplier', 1001),
    (1, 'screen_supplier', 1002),
    (1, 'case_supplier', 1003),
    (2, 'bulk_logistics', 1004),
    (2, 'consumer_logistics', 1005),
    (3, 'commercial_bank', 1006);



