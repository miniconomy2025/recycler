DROP TABLE IF EXISTS OrderItems CASCADE;
DROP TABLE IF EXISTS Orders CASCADE;
DROP TABLE IF EXISTS MaterialInventory CASCADE;
DROP TABLE IF EXISTS PhonePartsInventory CASCADE;
DROP TABLE IF EXISTS PhoneInventory CASCADE;
DROP TABLE IF EXISTS PhoneParts CASCADE;
DROP TABLE IF EXISTS RawMaterial CASCADE;
DROP TABLE IF EXISTS Phone CASCADE;
DROP TABLE IF EXISTS Companies CASCADE;
DROP TABLE IF EXISTS Role CASCADE;
DROP TABLE IF EXISTS Consumer CASCADE;
DROP TABLE IF EXISTS Supplier CASCADE;
DROP TABLE IF EXISTS OrderStatus CASCADE;


CREATE TABLE Consumer (
    id INTEGER PRIMARY KEY,
    name VARCHAR(55) NOT NULL,
    email VARCHAR(50) UNIQUE NOT NULL
);

CREATE TABLE Role (
    id INTEGER PRIMARY KEY,
    name VARCHAR(55) NOT NULL UNIQUE
);

CREATE TABLE Companies (
    id INTEGER PRIMARY KEY,
    role_id INTEGER NOT NULL,
    name VARCHAR(55) NOT NULL,
    key_id INTEGER,
    CONSTRAINT fk_role
        FOREIGN KEY (role_id)
        REFERENCES Role(id)
        ON DELETE RESTRICT
);

CREATE TABLE Phone (
    id INTEGER PRIMARY KEY,
    model VARCHAR(50) NOT NULL,
    brand VARCHAR(50) NOT NULL,
    condition VARCHAR(3) NOT NULL,
    returned_date TIMESTAMP
);

CREATE TABLE RawMaterial (
    id INTEGER PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL,
    price DECIMAL(10,2) NOT NULL
);

CREATE TABLE PhoneInventory (
    id INTEGER PRIMARY KEY,
    phone_id INTEGER UNIQUE NOT NULL,
    quantity DECIMAL(10,2) NOT NULL DEFAULT 0,
    CONSTRAINT fk_phone
        FOREIGN KEY (phone_id)
        REFERENCES Phone(id)
        ON DELETE CASCADE
);

CREATE TABLE PhoneParts (
    id INTEGER PRIMARY KEY,
    name VARCHAR(55) NOT NULL UNIQUE
);

CREATE TABLE PhonePartsInventory (
    id INTEGER PRIMARY KEY,
    phone_part_id INTEGER UNIQUE NOT NULL,
    quantity DECIMAL(10,2) NOT NULL DEFAULT 0,
    CONSTRAINT fk_phone_part
        FOREIGN KEY (phone_part_id)
        REFERENCES PhoneParts(id)
        ON DELETE CASCADE
);

CREATE TABLE MaterialInventory (
    id INTEGER PRIMARY KEY,
    material_id INTEGER UNIQUE NOT NULL,
    available_quantity_in_kg DECIMAL(10,2) NOT NULL DEFAULT 0,
    CONSTRAINT fk_material
        FOREIGN KEY (material_id)
        REFERENCES RawMaterial(id)
        ON DELETE CASCADE
);

CREATE TABLE Supplier (
    id INTEGER PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    industry_type VARCHAR(50)
);

CREATE TABLE OrderStatus (
    id INTEGER PRIMARY KEY,
    name VARCHAR(55) NOT NULL UNIQUE
);

CREATE TABLE Orders (
    id INTEGER PRIMARY KEY,
    order_status_id INTEGER NOT NULL,
    created_At TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    supplier_id INTEGER NOT NULL,
    CONSTRAINT fk_order_status
        FOREIGN KEY (order_status_id)
        REFERENCES OrderStatus(id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_supplier
        FOREIGN KEY (supplier_id)
        REFERENCES Supplier(id)
        ON DELETE RESTRICT
);

CREATE TABLE OrderItems (
    id INTEGER PRIMARY KEY,
    order_info_id INTEGER NOT NULL,
    material_id INTEGER NOT NULL,
    quantity VARCHAR(55) NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_order_info
        FOREIGN KEY (order_info_id)
        REFERENCES Orders(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_material_item
        FOREIGN KEY (material_id)
        REFERENCES RawMaterial(id)
        ON DELETE RESTRICT
);

CREATE INDEX idx_companies_role_id ON Companies (role_id);
CREATE INDEX idx_phoneinventory_phone_id ON PhoneInventory (phone_id);
CREATE INDEX idx_phonepartsinventory_phone_part_id ON PhonePartsInventory (phone_part_id);
CREATE INDEX idx_materialinventory_material_id ON MaterialInventory (material_id);
CREATE INDEX idx_orders_order_status_id ON Orders (order_status_id);
CREATE INDEX idx_orders_supplier_id ON Orders (supplier_id);
CREATE INDEX idx_orderitems_order_info_id ON OrderItems (order_info_id);
CREATE INDEX idx_orderitems_material_id ON OrderItems (material_id);
