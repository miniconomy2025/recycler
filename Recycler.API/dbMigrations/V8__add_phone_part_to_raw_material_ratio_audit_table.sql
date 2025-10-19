CREATE TABLE IF NOT EXISTS PhonePartToRawMaterialRatioAuditLogs (
    id SERIAL PRIMARY KEY,
    audit_action_id INTEGER NOT NULL,
    phone_part_id INTEGER,
    raw_material_id INTEGER,
    raw_material_quantity_per_phone_part INTEGER,
    last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_pptrmr_audit_action_id FOREIGN KEY (audit_action_id) REFERENCES AuditActions(id) ON DELETE RESTRICT,
    CONSTRAINT fk_pptrmr_phone_part_id FOREIGN KEY (phone_part_id) REFERENCES PhoneParts(id) ON DELETE RESTRICT,
    CONSTRAINT fk_pptrmr_raw_material_id FOREIGN KEY (raw_material_id) REFERENCES RawMaterial(id) ON DELETE RESTRICT
);


CREATE OR REPLACE FUNCTION phone_part_to_raw_material_ratio_audit_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO PhonePartToRawMaterialRatioAuditLogs (
        audit_action_id,
        phone_part_id,
        raw_material_id,
        raw_material_quantity_per_phone_part,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'INSERT'),
        NEW.phone_part_id,
        NEW.raw_material_id,
        NEW.raw_material_quantity_per_phone_part,
        NOW()
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_phone_part_to_raw_material_ratio_audit_insert
    AFTER INSERT ON PhonePartToRawMaterialRatio
    FOR EACH ROW
    EXECUTE FUNCTION phone_part_to_raw_material_ratio_audit_insert();


CREATE OR REPLACE FUNCTION phone_part_to_raw_material_ratio_audit_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO PhonePartToRawMaterialRatioAuditLogs (
        audit_action_id,
        phone_part_id,
        raw_material_id,
        raw_material_quantity_per_phone_part,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'UPDATE'),
        NEW.phone_part_id,
        NEW.raw_material_id,
        NEW.raw_material_quantity_per_phone_part,
        NOW()
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_phone_part_to_raw_material_ratio_audit_update
    AFTER UPDATE ON PhonePartToRawMaterialRatio
    FOR EACH ROW
    EXECUTE FUNCTION phone_part_to_raw_material_ratio_audit_update();
