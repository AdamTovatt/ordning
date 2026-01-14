CREATE TABLE IF NOT EXISTS locations (
    id VARCHAR(255) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(1000),
    parent_location_id VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_locations_parent_location FOREIGN KEY (parent_location_id) 
        REFERENCES locations(id) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS idx_locations_parent_location_id ON locations(parent_location_id);
