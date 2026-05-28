CREATE SCHEMA IF NOT EXISTS inventory;
CREATE SCHEMA IF NOT EXISTS sales;

-- =============================================================================
-- INVENTORY SCHEMA
-- =============================================================================

CREATE TABLE inventory.business (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(255),
    cen         VARCHAR(64),
    is_active   BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE UNIQUE INDEX ux_business_cen ON inventory.business (cen);

CREATE TABLE inventory.category (
    id          SERIAL PRIMARY KEY,
    business_id INT NOT NULL,
    name        VARCHAR(255),
    description VARCHAR(255),
    cen         VARCHAR(64),
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT category_business_id_fkey
        FOREIGN KEY (business_id) REFERENCES inventory.business (id)
);

CREATE UNIQUE INDEX ux_category_business_cen
    ON inventory.category (business_id, cen);

CREATE TABLE inventory.unit (
    id           SERIAL PRIMARY KEY,
    business_id  INT NOT NULL,
    name         VARCHAR(255),
    abbreviation VARCHAR(50),
    description  VARCHAR(255),
    cen          VARCHAR(64),
    is_active    BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT unit_business_id_fkey
        FOREIGN KEY (business_id) REFERENCES inventory.business (id)
);

CREATE UNIQUE INDEX ux_unit_business_cen
    ON inventory.unit (business_id, cen);

CREATE TABLE inventory.product_status (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(255),
    description VARCHAR(255)
);

CREATE TABLE inventory.warehouse (
    id          SERIAL PRIMARY KEY,
    business_id INT NOT NULL,
    name        VARCHAR(255),
    cen         VARCHAR(64),
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT warehouse_business_id_fkey
        FOREIGN KEY (business_id) REFERENCES inventory.business (id)
);

CREATE UNIQUE INDEX ux_warehouse_business_cen
    ON inventory.warehouse (business_id, cen);

CREATE TABLE inventory.product (
    id            SERIAL PRIMARY KEY,
    business_id   INT NOT NULL,
    category_id   INT,
    unit_id       INT,
    name          VARCHAR(255),
    description   VARCHAR(255),
    sku           VARCHAR(100),
    cen           VARCHAR(64),
    price         NUMERIC(12,2),
    cost_price    NUMERIC(12,2),
    reorder_level INT NOT NULL DEFAULT 0,
    station_code  VARCHAR(50),
    unit_qty      INT,
    is_active     BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT product_business_id_fkey
        FOREIGN KEY (business_id) REFERENCES inventory.business (id),
    CONSTRAINT product_category_id_fkey
        FOREIGN KEY (category_id) REFERENCES inventory.category (id),
    CONSTRAINT product_unit_id_fkey
        FOREIGN KEY (unit_id) REFERENCES inventory.unit (id)
);

CREATE UNIQUE INDEX ux_product_business_cen
    ON inventory.product (business_id, cen);
CREATE UNIQUE INDEX ux_product_business_sku
    ON inventory.product (business_id, sku);

CREATE TABLE inventory.warehouse_product (
    id            SERIAL PRIMARY KEY,
    warehouse_id  INT NOT NULL,
    product_id    INT NOT NULL,
    status_id     INT,
    stock_left    INT,
    low_stock_qty INT,
    price         NUMERIC(12,2),
    CONSTRAINT warehouse_product_warehouse_id_fkey
        FOREIGN KEY (warehouse_id) REFERENCES inventory.warehouse (id),
    CONSTRAINT warehouse_product_product_id_fkey
        FOREIGN KEY (product_id) REFERENCES inventory.product (id),
    CONSTRAINT warehouse_product_status_id_fkey
        FOREIGN KEY (status_id) REFERENCES inventory.product_status (id)
);

CREATE TABLE inventory.inventory_document (
    id                 SERIAL PRIMARY KEY,
    business_id        INT NOT NULL,
    warehouse_id       INT NOT NULL,
    document_cen       VARCHAR(64) NOT NULL,
    document_type      VARCHAR(50) NOT NULL,
    status             VARCHAR(50) NOT NULL,
    reason             VARCHAR(255),
    external_reference VARCHAR(255),
    source             VARCHAR(50),
    reference_cen      VARCHAR(100),
    created_at         TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT inventory_document_business_id_fkey
        FOREIGN KEY (business_id) REFERENCES inventory.business (id),
    CONSTRAINT inventory_document_warehouse_id_fkey
        FOREIGN KEY (warehouse_id) REFERENCES inventory.warehouse (id)
);

CREATE UNIQUE INDEX ux_document_business_cen
    ON inventory.inventory_document (business_id, document_cen);

CREATE TABLE inventory.inventory_document_line (
    id          SERIAL PRIMARY KEY,
    document_id INT NOT NULL,
    product_id  INT NOT NULL,
    quantity    DOUBLE PRECISION NOT NULL,
    unit_cost   NUMERIC(12,2),
    CONSTRAINT inventory_document_line_document_id_fkey
        FOREIGN KEY (document_id) REFERENCES inventory.inventory_document (id),
    CONSTRAINT inventory_document_line_product_id_fkey
        FOREIGN KEY (product_id) REFERENCES inventory.product (id)
);

CREATE TABLE inventory.kardex (
    id           SERIAL PRIMARY KEY,
    warehouse_id INT NOT NULL,
    product_id   INT NOT NULL,
    document_id  INT,
    action_type  VARCHAR(50),
    action_qty   DOUBLE PRECISION,
    time_stamp   TIMESTAMP,
    reason       VARCHAR(255),
    movement_cen VARCHAR(64),
    CONSTRAINT kardex_warehouse_id_fkey
        FOREIGN KEY (warehouse_id) REFERENCES inventory.warehouse (id),
    CONSTRAINT kardex_product_id_fkey
        FOREIGN KEY (product_id) REFERENCES inventory.product (id),
    CONSTRAINT kardex_document_id_fkey
        FOREIGN KEY (document_id) REFERENCES inventory.inventory_document (id)
);

CREATE UNIQUE INDEX ux_kardex_movement_cen
    ON inventory.kardex (movement_cen);


-- =============================================================================
-- SALES SCHEMA
-- =============================================================================

CREATE TABLE sales.customer (
    id    SERIAL PRIMARY KEY,
    name  VARCHAR(255),
    phone VARCHAR(50)
);

CREATE TABLE sales.waiter (
    id    SERIAL PRIMARY KEY,
    name  VARCHAR(255),
    phone VARCHAR(50)
);

CREATE TABLE sales.order_status (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(255),
    description VARCHAR(255)
);

CREATE TABLE sales.payment_type (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(255),
    description VARCHAR(255),
    code        VARCHAR(64),
    is_active   BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE sales.global_tax_config (
    id       SERIAL PRIMARY KEY,
    tax_rate NUMERIC(7,4) NOT NULL DEFAULT 0
);

CREATE TABLE sales.order_ticket (
    id                  SERIAL PRIMARY KEY,
    customer_id         INT,
    status_id           INT,
    tax_rate_snapshot   NUMERIC(7,4) NOT NULL DEFAULT 0,
    created_at          TIMESTAMP NOT NULL DEFAULT NOW(),
    daily_number        INT NOT NULL DEFAULT 0,
    cancellation_reason VARCHAR(500),
    CONSTRAINT order_ticket_customer_id_fkey
        FOREIGN KEY (customer_id) REFERENCES sales.customer (id),
    CONSTRAINT order_ticket_status_id_fkey
        FOREIGN KEY (status_id) REFERENCES sales.order_status (id)
);

CREATE TABLE sales.order_item (
    id              SERIAL PRIMARY KEY,
    order_id        INT,
    product_id      INT,
    product_cen     VARCHAR(64),
    product_name    VARCHAR(255),
    unit_price      NUMERIC(12,2),
    qty             DOUBLE PRECISION,
    status_id       INT,
    additional_note VARCHAR(255),
    sent_at         TIMESTAMP,
    resend_count    INT NOT NULL DEFAULT 0,
    CONSTRAINT order_item_order_id_fkey
        FOREIGN KEY (order_id) REFERENCES sales.order_ticket (id),
    CONSTRAINT order_item_status_id_fkey
        FOREIGN KEY (status_id) REFERENCES sales.order_status (id)
);

CREATE TABLE sales.order_command (
    id        SERIAL PRIMARY KEY,
    order_id  INT,
    waiter_id INT,
    CONSTRAINT order_command_order_id_fkey
        FOREIGN KEY (order_id) REFERENCES sales.order_ticket (id),
    CONSTRAINT order_command_waiter_id_fkey
        FOREIGN KEY (waiter_id) REFERENCES sales.waiter (id)
);

CREATE TABLE sales.station_type (
    id          SERIAL PRIMARY KEY,
    name        VARCHAR(255),
    description VARCHAR(255)
);

CREATE TABLE sales.station (
    id      SERIAL PRIMARY KEY,
    name    VARCHAR(255),
    type_id INT,
    CONSTRAINT station_type_id_fkey
        FOREIGN KEY (type_id) REFERENCES sales.station_type (id)
);

CREATE TABLE sales.command_item (
    id            SERIAL PRIMARY KEY,
    command_id    INT,
    order_item_id INT,
    station_id    INT,
    CONSTRAINT command_item_command_id_fkey
        FOREIGN KEY (command_id) REFERENCES sales.order_command (id),
    CONSTRAINT command_item_order_item_id_fkey
        FOREIGN KEY (order_item_id) REFERENCES sales.order_item (id),
    CONSTRAINT command_item_station_id_fkey
        FOREIGN KEY (station_id) REFERENCES sales.station (id)
);

CREATE TABLE sales.station_coverage (
    station_type_id INT NOT NULL,
    category_id     INT NOT NULL,
    PRIMARY KEY (station_type_id, category_id),
    CONSTRAINT station_coverage_station_type_id_fkey
        FOREIGN KEY (station_type_id) REFERENCES sales.station_type (id)
);

CREATE TABLE sales.payment (
    id              SERIAL PRIMARY KEY,
    order_id        INT,
    payment_type_id INT,
    paid_at         TIMESTAMP,
    CONSTRAINT payment_order_id_fkey
        FOREIGN KEY (order_id) REFERENCES sales.order_ticket (id),
    CONSTRAINT payment_payment_type_id_fkey
        FOREIGN KEY (payment_type_id) REFERENCES sales.payment_type (id)
);
