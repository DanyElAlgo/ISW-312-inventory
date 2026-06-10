-- =========================
-- Inventory Module
-- =========================

INSERT INTO inventory.business (name, cen) VALUES
    ('TechCorp Solutions', 'BUS-000001'),
    ('Fresh Foods Market', 'BUS-000002'),
    ('Electronics Plus',   'BUS-000003');

INSERT INTO inventory.category (name, description, business_id, cen) VALUES
    ('Electronics',     'Electronic devices and components',  1, 'CAT-000001'),
    ('Fresh Produce',   'Fresh fruits and vegetables',         2, 'CAT-000002'),
    ('Dairy Products',  'Milk, cheese, yogurt',                2, 'CAT-000003'),
    ('Beverages',       'Drinks and juices',                   2, 'CAT-000004'),
    ('Bakery',          'Bread, pastries, cakes',              2, 'CAT-000005'),
    ('Computer Parts',  'Cables, peripherals, components',     3, 'CAT-000006');

INSERT INTO inventory.unit (name, abbreviation, business_id, cen) VALUES
    ('Piece',    'pc',  1, 'UNT-000001'),
    ('Kilogram', 'kg',  2, 'UNT-000002'),
    ('Liter',    'L',   2, 'UNT-000003'),
    ('Box',      'box', 1, 'UNT-000004'),
    ('Dozen',    'dz',  2, 'UNT-000005');

INSERT INTO inventory.product_status (name, description) VALUES
    ('Active',       'Product is available for sale'),
    ('Discontinued', 'Product is no longer sold'),
    ('Low Stock',    'Product stock is below threshold'),
    ('Out of Stock', 'Product is currently unavailable');

-- TechCorp products
INSERT INTO inventory.product (sku, name, description, business_id, category_id, unit_id, cost_price, price, reorder_level, cen, is_active, unit_qty, station_code) VALUES
    ('LAP-001', 'Laptop Computer', 'High-performance business laptop',       1, 1, 1,  800.00, 1200.00, 10, 'PRD-000001', true, 1, NULL),
    ('MSE-001', 'Wireless Mouse',  'USB wireless mouse with 2.4GHz receiver', 1, 1, 1,   15.00,   25.00, 50, 'PRD-000002', true, 1, NULL),
    ('CBL-001', 'USB-C Cable',     'Fast charging USB-C cable 2 meters',     1, 1, 1,    8.00,   15.00, 75, 'PRD-000003', true, 1, NULL);

-- Fresh Foods products
INSERT INTO inventory.product (sku, name, description, business_id, category_id, unit_id, cost_price, price, reorder_level, cen, is_active, unit_qty, station_code) VALUES
    ('APP-001', 'Organic Apples',     'Fresh organic red apples',          2, 2, 2, 2.50, 4.50, 100, 'PRD-000004', true, 1, 'Cocina'),
    ('MLK-001', 'Whole Milk',         '1 liter container of whole milk',   2, 3, 3, 1.50, 2.20,  50, 'PRD-000005', true, 1, 'Bar'),
    ('CHE-001', 'Cheddar Cheese',     'Aged cheddar cheese block',         2, 3, 2, 4.50, 7.80,  20, 'PRD-000006', true, 1, 'Cocina'),
    ('OJC-001', 'Orange Juice',       'Fresh squeezed orange juice',       2, 4, 3, 2.00, 3.40,  30, 'PRD-000007', true, 1, 'Bar'),
    ('BRD-001', 'Whole Wheat Bread',  'Artisan whole wheat bread loaf',    2, 5, 1, 1.50, 2.90,  40, 'PRD-000008', true, 1, 'Cocina');

-- Electronics Plus products
INSERT INTO inventory.product (sku, name, description, business_id, category_id, unit_id, cost_price, price, reorder_level, cen, is_active, unit_qty, station_code) VALUES
    ('KBD-001', 'Mechanical Keyboard', 'RGB Mechanical Keyboard',     3, 6, 1,  35.00,  65.00, 25, 'PRD-000009', true, 1, NULL),
    ('MON-001', '27" Monitor',         'Full HD 27 inch monitor',     3, 6, 1, 150.00, 250.00,  8, 'PRD-000010', true, 1, NULL);

INSERT INTO inventory.warehouse (business_id, name, cen) VALUES
    (1, 'Main Tech Warehouse',         'WAR-000001'),
    (1, 'Tech Distribution Center',    'WAR-000002'),
    (2, 'Fresh Market Warehouse A',    'WAR-000003'),
    (2, 'Fresh Market Cold Storage',   'WAR-000004'),
    (3, 'Electronics Plus Storage',    'WAR-000005');

INSERT INTO inventory.warehouse_product (warehouse_id, product_id, status_id, stock_left, low_stock_qty, price) VALUES
    (1, 1, 1,  45, 10, 800.00),
    (1, 2, 1, 150, 50,  15.00),
    (1, 3, 1, 200, 75,   8.00),
    (2, 1, 1,  30, 10, 800.00),
    (2, 2, 1, 100, 50,  15.00),
    (3, 4, 1, 250, 50,   2.50),
    (3, 5, 1, 120, 30,   1.50),
    (4, 5, 1,  80, 20,   1.50),
    (4, 6, 1,  60, 15,   4.50),
    (4, 7, 1, 100, 25,   2.00),
    (4, 8, 1,  75, 20,   1.50),
    (5, 9, 1,  40, 15,  35.00),
    (5,10, 1,  20,  8, 150.00);

INSERT INTO inventory.inventory_document (business_id, warehouse_id, document_type, document_cen, created_at, status) VALUES
    (1, 1, 'ENTRY', 'DOC-000001', '2025-02-01 08:00:00', 'REGISTERED'),
    (2, 3, 'ENTRY', 'DOC-000002', '2025-02-02 09:00:00', 'REGISTERED');

INSERT INTO inventory.inventory_document_line (document_id, product_id, quantity, unit_cost) VALUES
    (1, 1,  50, 800.00),
    (1, 2, 200,  15.00),
    (2, 4, 300,   2.50);

INSERT INTO inventory.inventory_document (business_id, warehouse_id, document_type, document_cen, created_at, status) VALUES
    (1, 1, 'SALE_EXIT', 'DOC-000003', '2025-02-05 14:30:00', 'REGISTERED'),
    (2, 3, 'SALE_EXIT', 'DOC-000004', '2025-02-06 13:20:00', 'REGISTERED');

INSERT INTO inventory.inventory_document_line (document_id, product_id, quantity, unit_cost) VALUES
    (3, 1,  5, 800.00),
    (4, 4, 50,   2.50);

INSERT INTO inventory.kardex (warehouse_id, product_id, action_type, action_qty, time_stamp, reason, document_id, movement_cen) VALUES
    (1, 1, 'ENTRY', 50,  '2025-02-01 08:00:00', 'Initial stock - Supplier ENT-001',  1, 'KDX-000001'),
    (1, 2, 'ENTRY', 200, '2025-02-02 09:15:00', 'Stock replenishment - ENT-001',     1, 'KDX-000002'),
    (3, 4, 'ENTRY', 300, '2025-02-03 10:00:00', 'Farmer delivery - Supplier ENT-002', 2, 'KDX-000003'),
    (1, 1, 'SALE_EXIT', 5,  '2025-02-05 14:30:00', 'Sale order SO-001', 3, 'KDX-000004'),
    (3, 4, 'SALE_EXIT', 50, '2025-02-06 13:20:00', 'Sale order SO-002', 4, 'KDX-000005');


-- =========================
-- Sales Module
-- =========================

INSERT INTO sales.customer (name, phone) VALUES
    ('John Smith',      '+1-555-0101'),
    ('Maria Garcia',    '+1-555-0102'),
    ('David Chen',      '+1-555-0103'),
    ('Sarah Johnson',   '+1-555-0104'),
    ('Robert Williams', '+1-555-0105');

INSERT INTO sales.order_status (name, description) VALUES
    ('Open',            'Open / active ticket accepting items'),
    ('Pending',         'Item awaiting kitchen pickup'),
    ('Preparing',       'Item being prepared by kitchen/bar'),
    ('Ready',           'Item ready for delivery'),
    ('Paid',            'Paid ticket'),
    ('Cancelled',       'Cancelled ticket');

INSERT INTO sales.payment_type (name, description, code, is_active) VALUES
    ('Cash',          'Payment in physical currency', 'CASH',     true),
    ('Credit Card',   'Payment via credit card',      'CREDIT',   true),
    ('Debit Card',    'Payment via debit card',       'DEBIT',    true),
    ('Bank Transfer', 'Direct bank transfer',         'TRANSFER', true),
    ('Check',         'Payment via check',            'CHECK',    true);

INSERT INTO sales.global_tax_config (id, tax_rate) VALUES (1, 0.18);

INSERT INTO sales.waiter (name, phone) VALUES
    ('Carlos Rodríguez', '+1-555-0201'),
    ('Lucía Martínez',   '+1-555-0202');

INSERT INTO sales.station_type (name, description) VALUES
    ('Kitchen', 'Kitchen station — hot food'),
    ('Bar',     'Bar station — drinks');

INSERT INTO sales.station (name, type_id) VALUES
    ('Main Kitchen',   1),
    ('Central Bar',    2);

INSERT INTO sales.station_coverage (station_type_id, category_id) VALUES
    (1, 2),
    (1, 3),
    (1, 5),
    (2, 4);

-- Default warehouse per company, used when a ticket is created without an explicit warehouseCen.
INSERT INTO sales.default_warehouse (company_cen, warehouse_cen) VALUES
    ('BUS-000001', 'WAR-000001'),
    ('BUS-000002', 'WAR-000003'),
    ('BUS-000003', 'WAR-000005');

-- Sample tickets belong to Fresh Foods Market (BUS-000002) / Fresh Market Warehouse A (WAR-000003).
INSERT INTO sales.order_ticket (company_cen, warehouse_cen, customer_id, status_id, tax_rate_snapshot, created_at, daily_number) VALUES
    ('BUS-000002', 'WAR-000003', 1, 1, 0.18, NOW(), 1),
    ('BUS-000002', 'WAR-000003', 2, 5, 0.18, NOW() - INTERVAL '1 hour',  2),
    ('BUS-000002', 'WAR-000003', 3, 1, 0.18, NOW() - INTERVAL '30 minutes', 3);

INSERT INTO sales.order_item (qty, additional_note, order_id, product_cen, product_name, unit_price, status_id) VALUES
    (2, NULL,                       1, 'PRD-000004', 'Organic Apples',     4.50, 2),
    (1, 'No ice',                   1, 'PRD-000007', 'Orange Juice',       3.40, 2),
    (3, NULL,                       2, 'PRD-000008', 'Whole Wheat Bread',  2.90, 4),
    (1, 'No sugar',                 3, 'PRD-000005', 'Whole Milk',         2.20, 3);

INSERT INTO sales.order_command (order_id, waiter_id) VALUES
    (1, 1),
    (3, 2);

INSERT INTO sales.command_item (command_id, order_item_id, station_id) VALUES
    (1, 1, 1),
    (1, 2, 2),
    (2, 4, 2);

UPDATE sales.order_item SET sent_at = NOW() WHERE id IN (1, 2, 4);

INSERT INTO sales.payment (order_id, payment_type_id, paid_at) VALUES
    (2, 1, NOW() - INTERVAL '50 minutes');

-- =========================
-- Purchases Module
-- =========================

TRUNCATE TABLE
    purchases.purchase_order_item,
    purchases.purchase_order,
    purchases.supplier,
    purchases.purchase_status
    RESTART IDENTITY CASCADE;

INSERT INTO purchases.purchase_status (name, description) VALUES
    ('Pending',   'Order created, not yet confirmed'),
    ('Confirmed', 'Order confirmed and stock received'),
    ('Cancelled', 'Order cancelled before confirmation');

-- Suppliers for the demo company (Fresh Foods, BUS-000002 -> business_id = 2).
INSERT INTO purchases.supplier (business_id, name, cen, contact_email, contact_phone) VALUES
    (2, 'Distribuidora La Pradera', 'SUP-000001', 'ventas@lapradera.test',     '555-0101'),
    (2, 'Lácteos del Valle',        'SUP-000002', 'pedidos@lacteosvalle.test', '555-0102'),
    (2, 'Panadería Don Juan',       'SUP-000003', 'contacto@donjuan.test',     '555-0103');

-- A demo Pending purchase order so the UI shows something on first load.
INSERT INTO purchases.purchase_order (business_id, supplier_id, warehouse_cen, status_id, cen, created_at) VALUES
    (2, 1, 'WAR-000003', 1, 'PO-000001', NOW() - INTERVAL '1 day');

INSERT INTO purchases.purchase_order_item (purchase_order_id, product_cen, product_name, quantity) VALUES
    (1, 'PRD-000004', 'Organic Apples',    50),
    (1, 'PRD-000008', 'Whole Wheat Bread', 30);
