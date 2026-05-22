# API de ventas

## Descripción

Este API pertenece a un proyecto de manejo ERP (Inventario, Ventas, Compras) con un submódulo de PoS (Point of Sale).

## Pre-requisitos
- SDK de .NET 10
- PostgreSQL 14 o superior
- Una instancia del módulo de Inventario

## Base de datos

Antes de iniciar el proyecto, es necesario tener una base de datos, este proyecto en particular usa dos esquemas en la misma base de datos `inventory_db`. Para crear la base de datos, ejecute este comando en la raíz del repositorio (backend/):

```bash
psql -U postgres -d inventory_db -f backend/database/schema.sql
psql -U postgres -d inventory_db -f backend/database/seed_data.sql
```

