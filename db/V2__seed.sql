INSERT INTO Role (id, name) VALUES
  (1, 'Recycler'),
  (2, 'Supplier'),
  (4, 'Logistics'),
  (5, 'Bank');

INSERT INTO RawMaterial (id, name, price) VALUES
  (1, 'Copper', 45.00),
  (2, 'Silicon', 55.00),
  (3, 'Sand', 10.00),
  (4, 'Plastic', 15.00),
  (5, 'Aluminum', 25.00);

INSERT INTO OrderStatus (id, name) VALUES
  (1, 'Pending'),
  (2, 'Approved'),
  (3, 'Rejected'),
  (4, 'Shipped'),
  (5, 'Delivered'),
  (6, 'Refunded');

INSERT INTO PhoneParts (id, name) VALUES
  (1, 'Screen'),
  (2, 'Case'),
  (3, 'Battery');
