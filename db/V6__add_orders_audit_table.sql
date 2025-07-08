CREATE TABLE IF NOT EXISTS OrdersAuditLogs (
    id SERIAL PRIMARY KEY,
    audit_action_id INTEGER NOT NULL,
    order_number UUID NOT NULL,
    order_status_id INTEGER,
    created_at TIMESTAMP,
    company_id INTEGER,
    last_modified_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_orders_audit_action_id FOREIGN KEY (audit_action_id) REFERENCES AuditActions(id) ON DELETE RESTRICT,
    CONSTRAINT fk_orders_audit_order_status_id FOREIGN KEY (order_status_id) REFERENCES OrderStatus(id) ON DELETE RESTRICT,
    CONSTRAINT fk_orders_audit_company_id FOREIGN KEY (company_id) REFERENCES Companies(id) ON DELETE RESTRICT
);


CREATE OR REPLACE FUNCTION orders_audit_insert()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO OrdersAuditLogs (
        audit_action_id,
        order_number,
        order_status_id,
        created_at,
        company_id,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'INSERT'),
        NEW.order_number,
        NEW.order_status_id,
        NEW.created_at,
        NEW.company_id,
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
        order_number,
        order_status_id,
        created_at,
        company_id,
        last_modified_at
    ) VALUES (
        (SELECT id FROM AuditActions WHERE action_name = 'UPDATE'),
        NEW.order_number,
        NEW.order_status_id,
        NEW.created_at,
        NEW.company_id,
        NOW()
    );
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE TRIGGER trigger_orders_audit_update
AFTER UPDATE ON Orders
    FOR EACH ROW
    EXECUTE FUNCTION orders_audit_update();
