# StockFlow Database Scripts

These scripts reproduce the SQL Server / Azure SQL structure used by StockFlow v1.0.

Run them against an empty database named `StockFlowDB` in the following order:

```text
1. 01_Estructura.sql
2. 02_Restricciones_Indices.sql
3. 02B_Reparar_Restricciones.sql
4. StockFlow_Azure_03A_DatosBase_v2.sql
5. StockFlow_Azure_03B_Procedimientos.sql
```

## Purpose

| Script | Purpose |
|---|---|
| `01_Estructura.sql` | Core tables, keys and base relational structure |
| `02_Restricciones_Indices.sql` | Indexes, defaults, foreign keys and CHECK constraints |
| `02B_Reparar_Restricciones.sql` | Idempotent hardening/repair pass for constraints and indexes |
| `StockFlow_Azure_03A_DatosBase_v2.sql` | Required base/catalog data |
| `StockFlow_Azure_03B_Procedimientos.sql` | Stored procedures used by StockFlowAPI |

The scripts do not contain production credentials, Azure connection strings, or JWT secrets.
