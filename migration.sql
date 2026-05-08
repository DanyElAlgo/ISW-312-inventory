-- =============================================================================
-- SCHEMA MIGRATION: Split inventory_db into inventory + sales schemas
-- Run this once against the existing inventory_db database.
-- =============================================================================

-- Step 1: Create the two schemas
-- -----------------------------------------------------------------------------
CREATE SCHEMA IF NOT EXISTS inventory;
CREATE SCHEMA IF NOT EXISTS sales;


-- Step 2: Move Inventory tables to the inventory schema
-- -----------------------------------------------------------------------------
ALTER TABLE public.business          SET SCHEMA inventory;
ALTER TABLE public.category          SET SCHEMA inventory;
ALTER TABLE public.unit              SET SCHEMA inventory;
ALTER TABLE public.product           SET SCHEMA inventory;
ALTER TABLE public.product_status    SET SCHEMA inventory;
ALTER TABLE public.warehouse         SET SCHEMA inventory;
ALTER TABLE public.warehouse_product SET SCHEMA inventory;
ALTER TABLE public.kardex            SET SCHEMA inventory;


-- Step 3: Move Sales/POS tables to the sales schema
-- -----------------------------------------------------------------------------
ALTER TABLE public.customer          SET SCHEMA sales;
ALTER TABLE public.waiter            SET SCHEMA sales;
ALTER TABLE public.payment_type      SET SCHEMA sales;
ALTER TABLE public.payment           SET SCHEMA sales;
ALTER TABLE public.order_status      SET SCHEMA sales;
ALTER TABLE public.order_ticket      SET SCHEMA sales;
ALTER TABLE public.order_item        SET SCHEMA sales;
ALTER TABLE public.order_command     SET SCHEMA sales;
ALTER TABLE public.command_item      SET SCHEMA sales;
ALTER TABLE public.station_type      SET SCHEMA sales;
ALTER TABLE public.station           SET SCHEMA sales;
ALTER TABLE public.station_coverage  SET SCHEMA sales;
ALTER TABLE public.global_tax_config SET SCHEMA sales;


-- Step 4: Drop cross-schema FK — order_item.product_id no longer has a DB-level
--         constraint to inventory.product. It becomes a logical reference only.
-- -----------------------------------------------------------------------------
ALTER TABLE sales.order_item
    DROP CONSTRAINT IF EXISTS order_item_product_id_fkey;


-- Step 5: Drop cross-schema FK — station_coverage.category_id no longer has a
--         DB-level constraint to inventory.category.
-- -----------------------------------------------------------------------------
ALTER TABLE sales.station_coverage
    DROP CONSTRAINT IF EXISTS station_coverage_category_id_fkey;


-- Step 6: Add snapshot columns to sales.order_item
--         These are populated by Sales.API when an item is added to an order.
-- -----------------------------------------------------------------------------
ALTER TABLE sales.order_item
    ADD COLUMN IF NOT EXISTS product_name  varchar(255),
    ADD COLUMN IF NOT EXISTS unit_price    numeric(12,2);


-- Step 7: Verify — list all tables in both new schemas
-- -----------------------------------------------------------------------------
-- SELECT table_schema, table_name
-- FROM information_schema.tables
-- WHERE table_schema IN ('inventory', 'sales')
-- ORDER BY table_schema, table_name;
