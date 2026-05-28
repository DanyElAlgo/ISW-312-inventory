# API de inventario

## Descripción

Este API es el módulo de Inventario perteneciente a un proyecto de manejo ERP (Inventario, Ventas, Compras).

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

Este comando creará la base de datos, los esquemas y poblará ambos esquemas (inventory y sales) al mismo tiempo.

## Configuración

El archivo `appsettings.Development.json` contiene las rutas y puertos que se abrirán tanto para Inventory como para Sales.

## Ejecución

En la raíz del repositorio, ejecutar `make` para lanzar los dos módulos al mismo tiempo, opcionalmente también se puede acceder a la carpeta de cada módulo y ahí ejecutar `make` o `dotnet run`

## Mapa de endpoints

El mapa de endpoints se encuentra en: `salesAPI.json`
