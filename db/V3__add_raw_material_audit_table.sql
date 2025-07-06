CREATE TABLE IF NOT EXISTS RawMaterialAuditLogs (
    id SERIAL PRIMARY KEY,
    audit_action_id INTEGER NOT NULL,
    name VARCHAR(8) NOT NULL,
    price DECIMAL(10,2),
    last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_raw_material_audit_audit_action_id FOREIGN KEY (audit_action_id) REFERENCES AuditActions(id) ON DELETE RESTRICT
);


CREATE OR REPLACE FUNCTION raw_material_audit_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO RawMaterialAuditLogs (
        audit_action_id,
        name,
        price,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'INSERT'),
        NEW.name,
        NEW.price,
        NOW()
     );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_raw_material_audit_insert
    AFTER INSERT ON RawMaterial
    FOR EACH ROW
    EXECUTE FUNCTION raw_material_audit_insert();


CREATE OR REPLACE FUNCTION raw_material_audit_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO RawMaterialAuditLogs (
        audit_action_id,
        name,
        price,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'UPDATE'),
        NEW.name,
        NEW.price,
        NOW()
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trigger_raw_material_audit_update
    AFTER UPDATE ON RawMaterial
    FOR EACH ROW
    EXECUTE FUNCTION raw_material_audit_update();