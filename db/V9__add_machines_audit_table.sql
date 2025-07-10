CREATE TABLE IF NOT EXISTS MachinesAuditLogs (
    machine_id INTEGER NOT NULL,
    received_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_operational BOOL NOT NULL,
    last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_machines_audit_audit_action_id FOREIGN KEY (audit_action_id) REFERENCES AuditActions(id) ON DELETE RESTRICT
);


CREATE OR REPLACE FUNCTION machines_audit_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO MachinesAuditLogs (
        audit_action_id,
        machine_id,
        received_at,
        is_operational,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'INSERT'),
        NEW.machine_id,
        NEW.received_at,
        NEW.is_operational,
        NOW()
     );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_machines_audit_insert
    AFTER INSERT ON Machines
    FOR EACH ROW
    EXECUTE FUNCTION machines_audit_insert();


CREATE OR REPLACE FUNCTION machines_audit_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO MachinesAuditLogs (
        audit_action_id,
        machine_id,
        received_at,
        is_operational,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'UPDATE'),
        NEW.machine_id,
        NEW.received_at,
        NEW.is_operational,
        NOW()
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trigger_machines_audit_update
    AFTER UPDATE ON Machines
    FOR EACH ROW
    EXECUTE FUNCTION machines_audit_update();