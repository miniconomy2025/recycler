CREATE TABLE IF NOT EXISTS PhoneToPhonePartRatioAuditLogs (
    id SERIAL PRIMARY KEY,
    audit_action_id INTEGER NOT NULL,
    phone_id INTEGER,
    phone_part_id INTEGER,
    phone_part_quantity_per_phone INTEGER,
    last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_ptppr_audit_action_id FOREIGN KEY (audit_action_id) REFERENCES AuditActions(id) ON DELETE RESTRICT,
    CONSTRAINT fk_ptppr_phone_phone_id FOREIGN KEY (phone_id) REFERENCES Phone(id) ON DELETE RESTRICT,
    CONSTRAINT fk_ptppr_phone_part_id FOREIGN KEY (phone_part_id)REFERENCES PhoneParts(id) ON DELETE RESTRICT
);


CREATE OR REPLACE FUNCTION phone_to_phone_part_ratio_audit_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO PhoneToPhonePartRatioAuditLogs (
        audit_action_id,
        phone_id,
        phone_part_id,
        phone_part_quantity_per_phone,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'INSERT'),
        NEW.phone_id,
        NEW.phone_part_id,
        NEW.phone_part_quantity_per_phone,
        NOW()
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_phone_to_phone_part_ratio_audit_insert
    AFTER INSERT ON PhoneToPhonePartRatio
    FOR EACH ROW
    EXECUTE FUNCTION phone_to_phone_part_ratio_audit_insert();


CREATE OR REPLACE FUNCTION phone_to_phone_part_ratio_audit_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO PhoneToPhonePartRatioAuditLogs (
        audit_action_id,
        phone_id,
        phone_part_id,
        phone_part_quantity_per_phone,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'UPDATE'),
        NEW.phone_id,
        NEW.phone_part_id,
        NEW.phone_part_quantity_per_phone,
        NOW()
        );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_phone_to_phone_part_ratio_audit_update
    AFTER UPDATE ON PhoneToPhonePartRatio
    FOR EACH ROW
    EXECUTE FUNCTION phone_to_phone_part_ratio_audit_update();
