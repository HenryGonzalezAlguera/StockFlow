/*
    StockFlowDB - Azure SQL
    BLOQUE 03A: Datos base obligatorios

    Ejecutar después de:
    - Bloque 01: estructura
    - Bloque 02B: índices/defaults/FK/checks

    Este script es idempotente:
    si los registros ya existen, no los duplica.
*/

SET NOCOUNT ON;

-- =========================================================
-- 1. ROLES
-- =========================================================

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Roles
    WHERE NombreRol = 'Administrador'
)
BEGIN
    INSERT INTO dbo.Roles (NombreRol, Descripcion, Estado)
    VALUES ('Administrador', NULL, 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Roles
    WHERE NombreRol = 'Empleado'
)
BEGIN
    INSERT INTO dbo.Roles (NombreRol, Descripcion, Estado)
    VALUES ('Empleado', NULL, 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.Roles
    WHERE NombreRol = 'Consulta'
)
BEGIN
    INSERT INTO dbo.Roles (NombreRol, Descripcion, Estado)
    VALUES ('Consulta', NULL, 1);
END;

-- =========================================================
-- 2. TIPOS DE MOVIMIENTO
-- =========================================================

IF NOT EXISTS (
    SELECT 1
    FROM dbo.TiposMovimiento
    WHERE Nombre = 'Compra'
)
BEGIN
    INSERT INTO dbo.TiposMovimiento (Nombre, Naturaleza, Estado)
    VALUES ('Compra', 'ENTRADA', 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.TiposMovimiento
    WHERE Nombre = 'Venta'
)
BEGIN
    INSERT INTO dbo.TiposMovimiento (Nombre, Naturaleza, Estado)
    VALUES ('Venta', 'SALIDA', 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.TiposMovimiento
    WHERE Nombre = 'Ajuste Entrada'
)
BEGIN
    INSERT INTO dbo.TiposMovimiento (Nombre, Naturaleza, Estado)
    VALUES ('Ajuste Entrada', 'ENTRADA', 1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM dbo.TiposMovimiento
    WHERE Nombre = 'Ajuste Salida'
)
BEGIN
    INSERT INTO dbo.TiposMovimiento (Nombre, Naturaleza, Estado)
    VALUES ('Ajuste Salida', 'SALIDA', 1);
END;

-- =========================================================
-- 3. VERIFICACIÓN
-- =========================================================

SELECT
    (SELECT COUNT(*)
       FROM dbo.Roles
      WHERE Estado = 1
        AND NombreRol IN ('Administrador', 'Empleado', 'Consulta')) AS RolesBase,

    (SELECT COUNT(*)
       FROM dbo.TiposMovimiento
      WHERE Estado = 1
        AND (
             (Nombre = 'Compra' AND Naturaleza = 'ENTRADA')
          OR (Nombre = 'Venta' AND Naturaleza = 'SALIDA')
          OR (Nombre = 'Ajuste Entrada' AND Naturaleza = 'ENTRADA')
          OR (Nombre = 'Ajuste Salida' AND Naturaleza = 'SALIDA')
        )) AS TiposMovimientoBase;
