CREATE TABLE IF NOT EXISTS Log (
   id SERIAL CONSTRAINT pk_log PRIMARY KEY,
   request_source VARCHAR NOT NULL,
   request_endpoint VARCHAR NOT NULL,
   request_body VARCHAR,
   response VARCHAR
);