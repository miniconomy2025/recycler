CREATE TABLE IF NOT EXISTS OrdersAuditLogs (
    id SERIAL PRIMARY KEY,
    audit_action_id INTEGER NOT NULL,
    order_id INTEGER NOT NULL,
    order_status_id INTEGER,
    company_id INTEGER,
    created_at TIMESTAMP,
    last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_orders_audit_action_id FOREIGN KEY (audit_action_id) REFERENCES AuditActions(id) ON DELETE RESTRICT,
    CONSTRAINT fk_orders_audit_order_id FOREIGN KEY (order_id) REFERENCES Orders(id) ON DELETE RESTRICT
);


CREATE OR REPLACE FUNCTION orders_audit_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO OrdersAuditLogs (
        audit_action_id,
        order_id,
        order_status_id,
        company_id,
        created_at,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'INSERT'),
        NEW.id,
        NEW.order_status_id,
        NEW.company_id,
        NEW.created_at,
        NOW()
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_orders_audit_insert
    AFTER INSERT ON Orders
    FOR EACH ROW
    EXECUTE FUNCTION orders_audit_insert();


CREATE OR REPLACE FUNCTION orders_audit_update()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO OrdersAuditLogs (
        audit_action_id,
        order_id,
        order_status_id,
        company_id,
        created_at,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'UPDATE'),
        NEW.id,
        NEW.order_status_id,
        NEW.company_id,
        NEW.created_at,
        NOW()
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_orders_audit_update
AFTER UPDATE ON Orders
    FOR EACH ROW
    EXECUTE FUNCTION orders_audit_update();
