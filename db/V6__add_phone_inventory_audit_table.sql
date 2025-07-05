CREATE TABLE IF NOT EXISTS PhoneInventoryAuditLogs (
    id SERIAL PRIMARY KEY,
    audit_action_id INTEGER NOT NULL,
    phone_id INTEGER NOT NULL,
    quantity INTEGER,
    last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_phone_inventory_audit_action_id FOREIGN KEY (audit_action_id) REFERENCES AuditActions(id) ON DELETE RESTRICT,
    CONSTRAINT fk_phone_inventory_audit_phone_id FOREIGN KEY (phone_id) REFERENCES Phone(id) ON DELETE RESTRICT
);

CREATE OR REPLACE FUNCTION phone_inventory_audit_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO PhoneInventoryAuditLogs (
        audit_action_id,
        phone_id,
        quantity,
        last_modified_at
    ) VALUES (
         (SELECT id FROM AuditActions WHERE action_name = 'INSERT'),
         NEW.phone_id,
         NEW.quantity,
         NOW()
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trigger_phone_inventory_audit_insert
    AFTER INSERT ON PhoneInventory
    FOR EACH ROW
    EXECUTE FUNCTION phone_inventory_audit_insert();


CREATE OR REPLACE FUNCTION phone_inventory_audit_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO PhoneInventoryAuditLogs (
        audit_action_id,
        phone_id,
        quantity,
        last_modified_at
    ) VALUES (
         (SELECT id FROM AuditActions WHERE action_name = 'UPDATE'),
         NEW.phone_id,
         NEW.quantity,
         NOW()
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trigger_phone_inventory_audit_update
    AFTER UPDATE ON PhoneInventory
    FOR EACH ROW
    EXECUTE FUNCTION phone_inventory_audit_update();

