# ISW-312 — ERP modules (Inventory, Sales & Purchases)

A small restaurant ERP split into **three independent .NET 10 APIs** that share one
PostgreSQL database (`inventory_db`, with schemas `inventory`, `sales`, `purchases`)
and feed a single **React/Vite** admin UI.

| Module | Folder | Default port | Swagger |
|---|---|---|---|
| Inventory | [Inventory.API/](Inventory.API/) | `5001` | <http://localhost:5001/swagger> |
| Sales (POS) | [Sales.API/](Sales.API/) | `5002` | <http://localhost:5002/swagger> |
| Purchases | [Purchases.API/](Purchases.API/) | `5003` | <http://localhost:5003/swagger> |
| Frontend (admin UI) | [External repository](https://github.com/DanyElAlgo/slash-admin.git) | `3001` | --- |

Shared cross-service DTOs live in [Shared.Contracts/](Shared.Contracts/). The
database scripts live in [database/](database/). The frontend repository can be found [here](https://github.com/DanyElAlgo/slash-admin.git).

---

## 1. Modules

### Inventory
The catalog and stock source of truth. Owns companies, warehouses, categories,
units, products, stock levels, the kardex (stock movements) and inventory documents.
It is the **dependency** the other two modules call.

- Catalog & masters: products, categories, units, warehouses, companies.
- Stock: query levels, `increase`, `adjustments`, `validate`, `consume`.
- Kardex per product, documents, and a `restock-events` SSE stream.
- Route prefix: `GET/POST /api/inventory/companies/{companyCen}/...`

### Sales
The point-of-sale module: tickets, kitchen display (KDS), payments, tax config and
daily dashboards. It **calls Inventory** to validate stock when items are added and
to consume stock on payment, and freezes the unit price on each ticket line.

- Tickets (open, add/edit items, send to kitchen, cancel, print, totals).
- KDS teams & item status, waiters, catalog (proxied sellable products).
- Payments + payment methods, tax configuration, default warehouse, dashboards.
- Route prefix: `GET/POST /api/sales/companies/{companyCen}/...`

### Purchases
Supplier and purchase-order management. On **order confirmation** it calls Inventory
to increase stock, which in turn triggers the restock notification to the frontend.

- Suppliers (read).
- Purchase orders: list, create, get, **confirm**.
- Route prefix: `GET/POST /api/purchases/companies/{companyCen}/...`

---

## 2. How the modules integrate

All three speak HTTP over the same `companyCen` tenant key and agree on shared JSON
shapes (camelCase, RFC 7807 `ProblemDetails` for errors).

```
                 ┌────────────┐   confirm order → stock/increase
   Purchases ───▶│  Inventory │◀─── validate / consume stock ──── Sales
       (5003)    │   (5001)   │                                   (5002)
                 └─────┬──────┘
                       │ restock-events (SSE, text/event-stream)
                       ▼
                  Frontend (3001) ── shows toast on restock
```

Three integration cases the system is built around:

| Case | Flow | Mechanism |
|---|---|---|
| **Restock notification** | Purchases → Inventory → Frontend | Inventory publishes Server-Sent Events on `/restock-events`; the UI consumes them. |
| **Resilience** | Sales → Inventory (down) | Sales wraps Inventory calls in **Polly** retry + circuit breaker and returns **503** with a readable message instead of failing hard. |
| **Price history** | Sales | The unit price is **frozen** on each ticket line so later catalog price changes don't rewrite past sales. |

`InventoryApi__BaseUrl` (set on Sales and Purchases) is how each caller finds
Inventory.

---

## 3. Interaction examples

> Replace `{companyCen}` with a real value from the seed data.

**Purchase confirmation raises stock (Purchases → Inventory):**
```bash
curl -X POST http://localhost:5003/api/purchases/companies/{companyCen}/orders/{orderCen}/confirm
# Inventory stock goes up and a restock event is emitted on the SSE stream.
```

**Frontend / debug listens for restock events (Inventory SSE):**
```bash
curl -N http://localhost:5001/api/inventory/companies/{companyCen}/restock-events
# data: {"companyCen":"...","warehouseCen":"...","items":[{"productCen":"...","quantity":12}]}
```

**Sell items, which consumes stock (Sales → Inventory):**
```bash
# 1. open a ticket
curl -X POST http://localhost:5002/api/sales/companies/{companyCen}/tickets
# 2. add an item (Sales validates stock against Inventory)
curl -X POST http://localhost:5002/api/sales/companies/{companyCen}/tickets/{ticketCen}/items \
  -H "Content-Type: application/json" -d '{"productCen":"...","quantity":2}'
# 3. pay (Sales consumes stock in Inventory; if Inventory is down → 503)
curl -X POST http://localhost:5002/api/sales/companies/{companyCen}/tickets/{ticketCen}/payment \
  -H "Content-Type: application/json" -d '{"paymentMethodCen":"...","amount":1000}'
```

---

## 4. Configuration / `.env` & appsettings

### Backend (per API — `appsettings.json` or environment overrides)

| Variable | Effect |
|---|---|
| `ASPNETCORE_URLS` | Listen URL(s), e.g. `http://*:5001`. Wins over `appsettings.json`. |
| `ConnectionStrings__DefaultConnection` | Postgres connection string, e.g. `Host=localhost;Port=5432;Database=inventory_db;Username=postgres;Password=<your-local-password>`. For local dev set it in the gitignored `appsettings.Development.json` or `.env`; in the cloud it is injected from AWS Secrets Manager. |
| `InventoryApi__BaseUrl` *(Sales & Purchases only)* | Where the caller reaches Inventory, e.g. `http://localhost:5001`. |

### Frontend

| Variable | Default | Change when… |
|---|---|---|
| `VITE_DEV_PORT` | `3001` | you want a different UI port. |
| `VITE_INVENTORY_PROXY_TARGET` | `http://localhost:5001` | Inventory runs on another host/port. |
| `VITE_SALES_PROXY_TARGET` | `http://localhost:5002` | Sales runs on another host/port. |
| `VITE_PURCHASES_PROXY_TARGET` | `http://localhost:5003` | Purchases runs on another host/port. |
| `VITE_INVENTORY_API_URL` / `VITE_SALES_API_URL` / `VITE_PURCHASES_API_URL` | `/inventory-api` · `/sales-api` · `/purchases-api` | the browser-facing API path changes (rarely). |

The browser calls the `*_API_URL` paths; Vite proxies them to the `*_PROXY_TARGET`
hosts, so there is no CORS in development.

---

## 5. How to run the project

**Prerequisites:** .NET SDK 10+, PostgreSQL 15+, Node 20.19+, the frontend repository code, pnpm (or npm)

1. **Create the database** and load schema + seed (from the repo root):
   ```bash
   createdb -U postgres inventory_db
   psql -U postgres -d inventory_db -f backend/database/schema.sql
   psql -U postgres -d inventory_db -f backend/database/seed_data.sql
   ```

2. **Point each API at the database** (skip if the default connection string is fine):
   ```bash
   export ConnectionStrings__DefaultConnection="Host=<db-host>;Port=5432;Database=inventory_db;Username=postgres;Password=<pwd>"
   ```

3. **Start the backends.** Either run all three from `backend/` with `make all`, or
   each in its own terminal:
   ```bash
   cd backend/Inventory.API && ASPNETCORE_URLS=http://*:5001 dotnet run
   cd backend/Sales.API     && ASPNETCORE_URLS=http://*:5002 InventoryApi__BaseUrl=http://<inventory-host>:5001 dotnet run
   cd backend/Purchases.API && ASPNETCORE_URLS=http://*:5003 InventoryApi__BaseUrl=http://<inventory-host>:5001 dotnet run
   ```
   Verify each `/swagger` page loads (ports 5001/5002/5003).

4. **Configure and start the frontend:**
   ```bash
   cd slash-admin
   cp .env.example .env.local        # edit the *_PROXY_TARGET vars if APIs aren't on localhost
   pnpm install                       # or: npm install
   pnpm dev                           # dev server on http://localhost:3001
   # production build instead: pnpm build && pnpm preview
   ```

5. **Smoke test:** open <http://localhost:3001>, pick a company, browse inventory,
   open a ticket in Sales, confirm a purchase order, and watch the restock toast appear.

> **LAN demo:** every host/port is config-driven. Set `ASPNETCORE_URLS=http://*:<port>`
> on each API host, set `InventoryApi__BaseUrl` on Sales/Purchases to Inventory's LAN
> IP, and set the `VITE_*_PROXY_TARGET` vars on the frontend host. CORS is open
> (`AllowAnyOrigin`) for local testing, lock it down before any public deployment.
