CREATE TABLE IF NOT EXISTS MaterialInventoryAuditLogs (
    id SERIAL PRIMARY KEY,
    audit_action_id INTEGER NOT NULL,
    material_id INTEGER NOT NULL,
    available_quantity_in_kg INTEGER,
    last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_material_inventory_audit_action_id FOREIGN KEY (audit_action_id) REFERENCES AuditActions(id) ON DELETE RESTRICT,
    CONSTRAINT fk_material_inventory_audit_material_id FOREIGN KEY (material_id) REFERENCES RawMaterial(id) ON DELETE RESTRICT
);


CREATE OR REPLACE FUNCTION material_inventory_audit_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO MaterialInventoryAuditLogs (
        audit_action_id,
        material_id,
        available_quantity_in_kg,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'INSERT'),
        NEW.material_id,
        NEW.available_quantity_in_kg,
        NOW()
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_material_inventory_audit_insert
    AFTER INSERT ON MaterialInventory
    FOR EACH ROW
    EXECUTE FUNCTION material_inventory_audit_insert();


CREATE OR REPLACE FUNCTION material_inventory_audit_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO MaterialInventoryAuditLogs (
        audit_action_id,
        material_id,
        available_quantity_in_kg,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'UPDATE'),
        NEW.material_id,
        NEW.available_quantity_in_kg,
        NOW()
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_material_inventory_audit_update
    AFTER UPDATE ON MaterialInventory
    FOR EACH ROW
    EXECUTE FUNCTION material_inventory_audit_update();
