CREATE TABLE IF NOT EXISTS AuditActions (
   id SERIAL PRIMARY KEY,
   action_name CHAR(6) UNIQUE NOT NULL
);

INSERT INTO AuditActions (action_name) VALUES
    ('INSERT'),
    ('UPDATE');