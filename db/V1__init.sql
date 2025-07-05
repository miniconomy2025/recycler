DROP TABLE IF EXISTS PhonePartToRawMaterialRatio CASCADE;
DROP TABLE IF EXISTS PhoneToPhonePartRatio CASCADE;
DROP TABLE IF EXISTS OrderItems CASCADE;
DROP TABLE IF EXISTS Orders CASCADE;
DROP TABLE IF EXISTS MaterialInventory CASCADE;
DROP TABLE IF EXISTS PhoneInventory CASCADE;
DROP TABLE IF EXISTS PhoneParts CASCADE;
DROP TABLE IF EXISTS RawMaterial CASCADE;
DROP TABLE IF EXISTS Phone CASCADE;
DROP TABLE IF EXISTS PhoneBrand CASCADE;
DROP TABLE IF EXISTS Companies CASCADE;
DROP TABLE IF EXISTS Role CASCADE;
DROP TABLE IF EXISTS OrderStatus CASCADE;

CREATE TABLE Role (
    id SERIAL CONSTRAINT pk_role PRIMARY KEY,
    name VARCHAR(55) NOT NULL UNIQUE
);

CREATE TABLE Companies (
    id SERIAL CONSTRAINT pk_companies PRIMARY KEY,
    company_number UUID UNIQUE NOT NULL,
    role_id INTEGER NOT NULL,
    name VARCHAR(55) NOT NULL,
    key_id INTEGER,
    CONSTRAINT fk_role
        FOREIGN KEY (role_id)
        REFERENCES Role(id)
        ON DELETE RESTRICT
);

CREATE TABLE PhoneBrand (
    id SERIAL CONSTRAINT pk_phone_brand PRIMARY KEY,
    brand_name VARCHAR(7) NOT NULL
);

CREATE TABLE Phone (
    id SERIAL CONSTRAINT pk_phone PRIMARY KEY,
    phone_brand_id INTEGER NOT NULL,
    model VARCHAR(50) UNIQUE NOT NULL,
    CONSTRAINT fk_phone_phone_brand_id
        FOREIGN KEY (phone_brand_id)
            REFERENCES PhoneBrand(id)
            ON DELETE CASCADE
);

CREATE TABLE RawMaterial (
    id SERIAL CONSTRAINT pk_raw_material PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL,
    price DECIMAL(10,2) NOT NULL
);

CREATE TABLE PhoneInventory (
    id SERIAL CONSTRAINT pk_phone_inventory PRIMARY KEY,
    phone_id INTEGER UNIQUE NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT fk_phone
        FOREIGN KEY (phone_id)
        REFERENCES Phone(id)
        ON DELETE CASCADE
);

CREATE TABLE PhoneParts (
    id SERIAL CONSTRAINT pk_phone_parts PRIMARY KEY,
    name VARCHAR(55) NOT NULL UNIQUE
);

CREATE TABLE MaterialInventory (
    id SERIAL CONSTRAINT pk_material_inventory PRIMARY KEY,
    material_id INTEGER UNIQUE NOT NULL,
    available_quantity_in_kg INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT fk_material
        FOREIGN KEY (material_id)
        REFERENCES RawMaterial(id)
        ON DELETE CASCADE
);

CREATE TABLE OrderStatus (
    id SERIAL CONSTRAINT pk_order_status PRIMARY KEY,
    name VARCHAR(55) NOT NULL UNIQUE
);

CREATE TABLE Orders (
    id SERIAL CONSTRAINT pk_order PRIMARY KEY,
    order_number UUID UNIQUE NOT NULL,
    order_status_id INTEGER NOT NULL,
    created_At TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    company_id INTEGER NOT NULL,
    CONSTRAINT fk_order_status
        FOREIGN KEY (order_status_id)
        REFERENCES OrderStatus(id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_company
        FOREIGN KEY (company_id)
        REFERENCES Companies(id)
        ON DELETE RESTRICT
);

CREATE TABLE OrderItems (
    id SERIAL CONSTRAINT pk_order_items PRIMARY KEY,
    order_id INTEGER NOT NULL,
    material_id INTEGER NOT NULL,
    quantity_in_kg INTEGER NOT NULL,
    price DECIMAL(10,2) NOT NULL,
    CONSTRAINT fk_order
        FOREIGN KEY (order_id)
        REFERENCES Orders(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_material_item
        FOREIGN KEY (material_id)
        REFERENCES RawMaterial(id)
        ON DELETE RESTRICT
);

CREATE TABLE PhoneToPhonePartRatio (
    id SERIAL CONSTRAINT pk_phone_to_phone_part_ratio PRIMARY KEY,
    phone_id INTEGER NOT NULL,
    phone_part_id INTEGER NOT NULL,
    phone_part_quantity_per_phone INTEGER NOT NULL,
    CONSTRAINT fk_phone_to_phone_part_ratio_phone
        FOREIGN KEY (phone_id)
            REFERENCES Phone(id)
            ON DELETE CASCADE,
    CONSTRAINT fk_phone_to_phone_part_ratio_phone_part
        FOREIGN KEY (phone_part_id)
            REFERENCES PhoneParts(id)
            ON DELETE CASCADE
);

CREATE TABLE PhonePartToRawMaterialRatio (
    id SERIAL CONSTRAINT pk_phone_part_to_raw_material_ratio PRIMARY KEY,
    phone_part_id INTEGER NOT NULL,
    raw_material_id INTEGER NOT NULL,
    raw_material_quantity_per_phone_part INTEGER NOT NULL,
    CONSTRAINT fk_phone_part_to_raw_material_ratio_phone_part
        FOREIGN KEY (phone_part_id)
            REFERENCES PhoneParts(id)
            ON DELETE CASCADE,
    CONSTRAINT fk_phone_part_to_raw_material_ratio_raw_material
        FOREIGN KEY (raw_material_id)
            REFERENCES RawMaterial(id)
            ON DELETE CASCADE
);

CREATE TABLE Machines (
    id SERIAL PRIMARY KEY,
    machine_id INTEGER NOT NULL,
    received_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) NOT NULL 
);

CREATE INDEX idx_companies_role_id ON Companies (role_id);
CREATE INDEX idx_phoneinventory_phone_id ON PhoneInventory (phone_id);
CREATE INDEX idx_materialinventory_material_id ON MaterialInventory (material_id);
CREATE INDEX idx_orders_order_status_id ON Orders (order_status_id);
CREATE INDEX idx_orders_company_id ON Orders (company_id);
CREATE INDEX idx_orderitems_order_id ON OrderItems (order_id);
CREATE INDEX idx_orderitems_material_id ON OrderItems (material_id);
CREATE INDEX idx_phone_to_phone_part_ratio ON PhoneToPhonePartRatio (phone_id, phone_part_id);
CREATE INDEX idx_phone_part_to_raw_material_ratio ON PhonePartToRawMaterialRatio (phone_part_id, raw_material_id);
