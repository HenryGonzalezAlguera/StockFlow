/*
=========================================================
StockFlowDB - Azure SQL
BLOQUE 03B: PROCEDIMIENTOS ALMACENADOS
=========================================================

Incluye los 55 procedimientos almacenados reales
de StockFlow.

Las siguientes 3 rutinas usan la versión final QA:
- SP_RegistrarCompra
- SP_RegistrarVenta
- SP_AjustarInventario

El script usa CREATE OR ALTER y SQL dinámico para
ser compatible con el Editor de consultas de Azure.

Ejecutar después de:
- Bloque 01
- Bloque 02B
- Bloque 03A
=========================================================
*/

SET NOCOUNT ON;

DECLARE @sql nvarchar(max);


/* =========================================================
   01. SP_AjustarInventario
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_AjustarInventario]
    @IdEmpresa BIGINT,
    @IdProducto BIGINT,
    @IdUsuario BIGINT,

    @TipoAjuste VARCHAR(10),
    @Cantidad DECIMAL(18,3),
    @Motivo VARCHAR(300),

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(250) OUTPUT,
    @IdMovimiento BIGINT OUTPUT,
    @StockAnterior DECIMAL(18,3) OUTPUT,
    @StockNuevo DECIMAL(18,3) OUTPUT
AS
BEGIN

    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';
    SET @IdMovimiento = 0;
    SET @StockAnterior = 0;
    SET @StockNuevo = 0;


    ------------------------------------------------------------
    -- NORMALIZAR TIPO
    ------------------------------------------------------------

    SET @TipoAjuste =
        UPPER(LTRIM(RTRIM(@TipoAjuste)));


    ------------------------------------------------------------
    -- 1. VALIDAR TIPO
    ------------------------------------------------------------

    IF @TipoAjuste NOT IN (''ENTRADA'', ''SALIDA'')
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El tipo de ajuste debe ser ENTRADA o SALIDA.'';
        RETURN;
    END;


    ------------------------------------------------------------
    -- 2. VALIDAR CANTIDAD
    ------------------------------------------------------------

    IF @Cantidad <= 0
       OR @Cantidad <> FLOOR(@Cantidad)
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''La cantidad debe ser un número entero mayor a cero.'';
        RETURN;
    END;


    ------------------------------------------------------------
    -- 3. VALIDAR MOTIVO
    ------------------------------------------------------------

    IF @Motivo IS NULL
       OR LTRIM(RTRIM(@Motivo)) = ''''
    BEGIN
        SET @Resultado = -3;
        SET @Mensaje = ''Debe indicar el motivo del ajuste.'';
        RETURN;
    END;


    ------------------------------------------------------------
    -- 4. VALIDAR USUARIO
    ------------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM Usuarios
        WHERE IdUsuario = @IdUsuario
          AND IdEmpresa = @IdEmpresa
          AND Estado = 1
    )
    BEGIN
        SET @Resultado = -4;
        SET @Mensaje = ''El usuario no pertenece a la empresa.'';
        RETURN;
    END;


    BEGIN TRY

        BEGIN TRANSACTION;


        ------------------------------------------------------------
        -- 5. OBTENER PRODUCTO BLOQUEANDO PARA ACTUALIZACION
        ------------------------------------------------------------

        DECLARE @StockActual DECIMAL(18,3);


        SELECT
            @StockActual = StockActual
        FROM Productos WITH (UPDLOCK, HOLDLOCK)
        WHERE IdProducto = @IdProducto
          AND IdEmpresa = @IdEmpresa
          AND Estado = 1;


        IF @StockActual IS NULL
        BEGIN
            SET @Resultado = -5;
       ' AS nvarchar(max))
        + N'     SET @Mensaje =
                ''El producto no existe, está inactivo o pertenece a otra empresa.'';

            ROLLBACK TRANSACTION;
            RETURN;
        END;


        SET @StockAnterior = @StockActual;


        ------------------------------------------------------------
        -- 6. VALIDAR SALIDA
        ------------------------------------------------------------

        IF @TipoAjuste = ''SALIDA''
           AND @StockAnterior < @Cantidad
        BEGIN
            SET @Resultado = -6;
            SET @Mensaje =
                ''No existe stock suficiente para realizar el ajuste de salida.'';

            ROLLBACK TRANSACTION;
            RETURN;
        END;


        ------------------------------------------------------------
        -- 7. BUSCAR TIPO MOVIMIENTO
        ------------------------------------------------------------

        DECLARE @IdTipoMovimiento BIGINT;


        IF @TipoAjuste = ''ENTRADA''
        BEGIN

            SELECT
                @IdTipoMovimiento = IdTipoMovimiento
            FROM TiposMovimiento
            WHERE Nombre = ''Ajuste Entrada''
              AND Naturaleza = ''ENTRADA''
              AND Estado = 1;

        END
        ELSE
        BEGIN

            SELECT
                @IdTipoMovimiento = IdTipoMovimiento
            FROM TiposMovimiento
            WHERE Nombre = ''Ajuste Salida''
              AND Naturaleza = ''SALIDA''
              AND Estado = 1;

        END;


        IF @IdTipoMovimiento IS NULL
        BEGIN
            SET @Resultado = -7;
            SET @Mensaje =
                ''No se encontró el tipo de movimiento para el ajuste.'';

            ROLLBACK TRANSACTION;
            RETURN;
        END;


        ------------------------------------------------------------
        -- 8. CALCULAR NUEVO STOCK
        ------------------------------------------------------------

        IF @TipoAjuste = ''ENTRADA''
        BEGIN

            SET @StockNuevo =
                @StockAnterior + @Cantidad;

        END
        ELSE
        BEGIN

            SET @StockNuevo =
                @StockAnterior - @Cantidad;

        END;


        ------------------------------------------------------------
        -- 9. ACTUALIZAR PRODUCTO
        ------------------------------------------------------------

        UPDATE Productos
        SET StockActual = @StockNuevo
        WHERE IdProducto = @IdProducto
          AND IdEmpresa = @IdEmpresa;


        ------------------------------------------------------------
        -- 10. REGISTRAR MOVIMIENTO
        ------------------------------------------------------------

        INSERT INTO MovimientosInventario
        (
            IdEmpresa,
            IdProducto,
            IdUsuario,
            IdTipoMovimiento,

            IdCompra,
            IdVenta,

            Cantidad,

            StockAnt'
        + N'erior,
            StockNuevo,

            Motivo
        )
        VALUES
        (
            @IdEmpresa,
            @IdProducto,
            @IdUsuario,
            @IdTipoMovimiento,

            NULL,
            NULL,

            @Cantidad,

            @StockAnterior,
            @StockNuevo,

            LTRIM(RTRIM(@Motivo))
        );


        SET @IdMovimiento =
            SCOPE_IDENTITY();


        ------------------------------------------------------------
        -- 11. CONFIRMAR
        ------------------------------------------------------------

        COMMIT TRANSACTION;


        SET @Resultado = 1;
        SET @Mensaje =
            ''Ajuste de inventario realizado correctamente.'';

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;


        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH

END;';

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_AjustarInventario';


/* =========================================================
   02. SP_CambiarContrasenaPropia
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_CambiarContrasenaPropia]
    @IdUsuario BIGINT,
    @IdEmpresa BIGINT,
    @ContrasenaNueva VARCHAR(250),
    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';

    ------------------------------------------------------------
    -- VALIDAR USUARIO / TENANT / ESTADO
    ------------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios U
        INNER JOIN dbo.Empresas E
            ON E.IdEmpresa = U.IdEmpresa
        INNER JOIN dbo.Roles R
            ON R.IdRol = U.IdRol
        WHERE U.IdUsuario = @IdUsuario
          AND U.IdEmpresa = @IdEmpresa
          AND U.Estado = 1
          AND E.Estado = 1
          AND R.Estado = 1
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''La sesión ya no es válida.'';
        RETURN;
    END;

    ------------------------------------------------------------
    -- VALIDAR HASH RECIBIDO
    ------------------------------------------------------------

    IF @ContrasenaNueva IS NULL
       OR LEN(LTRIM(RTRIM(@ContrasenaNueva))) = 0
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''La nueva contraseña no es válida.'';
        RETURN;
    END;

    BEGIN TRY

        UPDATE dbo.Usuarios
        SET Contrasena = @ContrasenaNueva
        WHERE IdUsuario = @IdUsuario
          AND IdEmpresa = @IdEmpresa
          AND Estado = 1;

        SET @Resultado = 1;
        SET @Mensaje = ''Contraseña actualizada correctamente.'';

    END TRY
    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_CambiarContrasenaPropia';


/* =========================================================
   03. SP_CambiarEstadoCategoria
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_CambiarEstadoCategoria]
    @IdCategoria BIGINT,
    @IdEmpresa BIGINT,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';


    IF NOT EXISTS
    (
        SELECT 1
        FROM Categorias
        WHERE IdCategoria = @IdCategoria
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''La categoría no existe.'';
        RETURN;
    END;


    BEGIN TRY

        UPDATE Categorias
        SET Estado =
            CASE
                WHEN Estado = 1 THEN 0
                ELSE 1
            END
        WHERE IdCategoria = @IdCategoria
          AND IdEmpresa = @IdEmpresa;


        SET @Resultado = 1;
        SET @Mensaje = ''Estado de la categoría actualizado correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_CambiarEstadoCategoria';


/* =========================================================
   04. SP_CambiarEstadoCliente
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_CambiarEstadoCliente]
    @IdCliente BIGINT,
    @IdEmpresa BIGINT,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';


    IF NOT EXISTS
    (
        SELECT 1
        FROM Clientes
        WHERE IdCliente = @IdCliente
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El cliente no existe.'';
        RETURN;
    END;


    BEGIN TRY

        UPDATE Clientes
        SET Estado =
            CASE
                WHEN Estado = 1 THEN 0
                ELSE 1
            END
        WHERE IdCliente = @IdCliente
          AND IdEmpresa = @IdEmpresa;


        SET @Resultado = 1;
        SET @Mensaje = ''Estado del cliente actualizado correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_CambiarEstadoCliente';


/* =========================================================
   05. SP_CambiarEstadoProducto
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_CambiarEstadoProducto]
    @IdProducto BIGINT,
    @IdEmpresa BIGINT,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';


    IF NOT EXISTS
    (
        SELECT 1
        FROM Productos
        WHERE IdProducto = @IdProducto
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El producto no existe.'';
        RETURN;
    END;


    BEGIN TRY

        UPDATE Productos
        SET Estado =
            CASE
                WHEN Estado = 1 THEN 0
                ELSE 1
            END
        WHERE IdProducto = @IdProducto
          AND IdEmpresa = @IdEmpresa;


        SET @Resultado = 1;
        SET @Mensaje = ''Estado del producto actualizado correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_CambiarEstadoProducto';


/* =========================================================
   06. SP_CambiarEstadoProveedor
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_CambiarEstadoProveedor]
    @IdProveedor BIGINT,
    @IdEmpresa BIGINT,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';


    IF NOT EXISTS
    (
        SELECT 1
        FROM Proveedores
        WHERE IdProveedor = @IdProveedor
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El proveedor no existe.'';
        RETURN;
    END;


    BEGIN TRY

        UPDATE Proveedores
        SET Estado =
            CASE
                WHEN Estado = 1 THEN 0
                ELSE 1
            END
        WHERE IdProveedor = @IdProveedor
          AND IdEmpresa = @IdEmpresa;


        SET @Resultado = 1;
        SET @Mensaje = ''Estado del proveedor actualizado correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_CambiarEstadoProveedor';


/* =========================================================
   07. SP_CambiarEstadoUsuario
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_CambiarEstadoUsuario]
    @IdUsuario BIGINT,
    @IdEmpresa BIGINT,
    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';

    ------------------------------------------------------------
    -- EXISTENCIA / MULTIEMPRESA
    ------------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios
        WHERE IdUsuario = @IdUsuario
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El usuario no existe.'';
        RETURN;
    END;

    ------------------------------------------------------------
    -- PROTEGER ADMINISTRADOR
    ------------------------------------------------------------

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios U
        INNER JOIN dbo.Roles R
            ON R.IdRol = U.IdRol
        WHERE U.IdUsuario = @IdUsuario
          AND U.IdEmpresa = @IdEmpresa
          AND R.NombreRol = ''Administrador''
    )
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''La cuenta Administrador no puede desactivarse desde este módulo.'';
        RETURN;
    END;

    BEGIN TRY

        UPDATE dbo.Usuarios
        SET Estado =
            CASE
                WHEN Estado = 1 THEN 0
                ELSE 1
            END
        WHERE IdUsuario = @IdUsuario
          AND IdEmpresa = @IdEmpresa;

        SET @Resultado = 1;
        SET @Mensaje = ''Estado del usuario actualizado correctamente.'';

    END TRY
    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_CambiarEstadoUsuario';


/* =========================================================
   08. SP_ConsultarCategoria
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarCategoria]
    @IdCategoria BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdCategoria,
        Nombre,
        Descripcion,
        Estado,
        FechaRegistro
    FROM Categorias
    WHERE IdCategoria = @IdCategoria
      AND IdEmpresa = @IdEmpresa;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarCategoria';


/* =========================================================
   09. SP_ConsultarCategorias
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarCategorias]
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdCategoria,
        Nombre,
        Descripcion,
        Estado,
        FechaRegistro
    FROM Categorias
    WHERE IdEmpresa = @IdEmpresa
    ORDER BY Nombre;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarCategorias';


/* =========================================================
   10. SP_ConsultarCategoriasPaginado
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarCategoriasPaginado]
    @IdEmpresa BIGINT,
    @Busqueda VARCHAR(150) = NULL,
    @Estado BIT = NULL,
    @Pagina INT = 1,
    @CantidadPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;


    IF @Pagina < 1
        SET @Pagina = 1;


    IF @CantidadPagina NOT IN (10, 20, 50)
        SET @CantidadPagina = 10;


    DECLARE @RegistrosOmitir INT =
        (@Pagina - 1) * @CantidadPagina;


    SET @Busqueda =
        NULLIF(LTRIM(RTRIM(@Busqueda)), '''');


    ------------------------------------------------------------
    -- RESULT SET 1
    ------------------------------------------------------------

    SELECT
        C.IdCategoria,
        C.Nombre,
        C.Descripcion,
        C.Estado,
        C.FechaRegistro

    FROM Categorias C

    WHERE C.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL
          OR C.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR C.Descripcion LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @Estado IS NULL
          OR C.Estado = @Estado
      )

    ORDER BY C.Nombre

    OFFSET @RegistrosOmitir ROWS

    FETCH NEXT @CantidadPagina ROWS ONLY;


    ------------------------------------------------------------
    -- RESULT SET 2
    ------------------------------------------------------------

    SELECT
        COUNT(*)

    FROM Categorias C

    WHERE C.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL
          OR C.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR C.Descripcion LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @Estado IS NULL
          OR C.Estado = @Estado
      );

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarCategoriasPaginado';


/* =========================================================
   11. SP_ConsultarCliente
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarCliente]
    @IdCliente BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdCliente,
        Identificacion,
        Nombre,
        Correo,
        Telefono,
        Direccion,
        Estado,
        FechaRegistro
    FROM Clientes
    WHERE IdCliente = @IdCliente
      AND IdEmpresa = @IdEmpresa;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarCliente';


/* =========================================================
   12. SP_ConsultarClientes
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarClientes]
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdCliente,
        Identificacion,
        Nombre,
        Correo,
        Telefono,
        Direccion,
        Estado,
        FechaRegistro
    FROM Clientes
    WHERE IdEmpresa = @IdEmpresa
    ORDER BY Nombre;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarClientes';


/* =========================================================
   13. SP_ConsultarClientesPaginado
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarClientesPaginado]
    @IdEmpresa BIGINT,
    @Busqueda VARCHAR(150) = NULL,
    @Estado BIT = NULL,
    @Pagina INT = 1,
    @CantidadPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;


    IF @Pagina < 1
        SET @Pagina = 1;


    IF @CantidadPagina NOT IN (10, 20, 50)
        SET @CantidadPagina = 10;


    DECLARE @RegistrosOmitir INT =
        (@Pagina - 1) * @CantidadPagina;


    SET @Busqueda =
        NULLIF(LTRIM(RTRIM(@Busqueda)), '''');


    ------------------------------------------------------------
    -- RESULT SET 1
    ------------------------------------------------------------

    SELECT
        C.IdCliente,
        C.Identificacion,
        C.Nombre,
        C.Correo,
        C.Telefono,
        C.Direccion,
        C.Estado,
        C.FechaRegistro

    FROM Clientes C

    WHERE C.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR C.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR C.Identificacion LIKE ''%'' + @Busqueda + ''%''
          OR C.Correo LIKE ''%'' + @Busqueda + ''%''
          OR C.Telefono LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @Estado IS NULL
          OR C.Estado = @Estado
      )

    ORDER BY C.Nombre

    OFFSET @RegistrosOmitir ROWS

    FETCH NEXT @CantidadPagina ROWS ONLY;


    ------------------------------------------------------------
    -- RESULT SET 2
    ------------------------------------------------------------

    SELECT
        COUNT(*)

    FROM Clientes C

    WHERE C.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR C.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR C.Identificacion LIKE ''%'' + @Busqueda + ''%''
          OR C.Correo LIKE ''%'' + @Busqueda + ''%''
          OR C.Telefono LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @Estado IS NULL
          OR C.Estado = @Estado
      );

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarClientesPaginado';


/* =========================================================
   14. SP_ConsultarCompra
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarCompra]
    @IdCompra BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.IdCompra,
        C.NumeroCompra,
        C.FechaCompra,

        C.IdProveedor,
        P.Nombre AS Proveedor,

        C.IdUsuario,
        U.Nombre AS Usuario,

        C.Subtotal,
        C.PorcentajeImpuesto,
        C.Impuesto,
        C.Total,
        C.Estado

    FROM Compras C

    INNER JOIN Proveedores P
        ON C.IdProveedor = P.IdProveedor

    INNER JOIN Usuarios U
        ON C.IdUsuario = U.IdUsuario

    WHERE C.IdCompra = @IdCompra
      AND C.IdEmpresa = @IdEmpresa;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarCompra';


/* =========================================================
   15. SP_ConsultarCompras
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarCompras]
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.IdCompra,
        C.NumeroCompra,
        C.FechaCompra,

        C.IdProveedor,
        P.Nombre AS Proveedor,

        C.IdUsuario,
        U.Nombre AS Usuario,

        C.Subtotal,
        C.PorcentajeImpuesto,
        C.Impuesto,
        C.Total,
        C.Estado

    FROM Compras C

    INNER JOIN Proveedores P
        ON C.IdProveedor = P.IdProveedor

    INNER JOIN Usuarios U
        ON C.IdUsuario = U.IdUsuario

    WHERE C.IdEmpresa = @IdEmpresa

    ORDER BY C.FechaCompra DESC;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarCompras';


/* =========================================================
   16. SP_ConsultarComprasPaginado
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarComprasPaginado]
    @IdEmpresa BIGINT,
    @Busqueda VARCHAR(150) = NULL,
    @IdProveedor BIGINT = NULL,
    @FechaDesde DATE = NULL,
    @FechaHasta DATE = NULL,
    @Pagina INT = 1,
    @CantidadPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;


    ------------------------------------------------------------
    -- NORMALIZAR PAGINACION
    ------------------------------------------------------------

    IF @Pagina < 1
        SET @Pagina = 1;


    IF @CantidadPagina NOT IN (10, 20, 50)
        SET @CantidadPagina = 10;


    DECLARE @RegistrosOmitir INT =
        (@Pagina - 1) * @CantidadPagina;


    SET @Busqueda =
        NULLIF(LTRIM(RTRIM(@Busqueda)), '''');


    ------------------------------------------------------------
    -- RESULT SET 1
    ------------------------------------------------------------

    SELECT
        C.IdCompra,
        C.NumeroCompra,
        C.FechaCompra,

        C.IdProveedor,
        P.Nombre AS Proveedor,

        C.IdUsuario,
        U.Nombre AS Usuario,

        C.Subtotal,
        C.PorcentajeImpuesto,
        C.Impuesto,
        C.Total,

        C.Estado

    FROM Compras C

    INNER JOIN Proveedores P
        ON C.IdProveedor = P.IdProveedor

    INNER JOIN Usuarios U
        ON C.IdUsuario = U.IdUsuario

    WHERE C.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR C.NumeroCompra LIKE ''%'' + @Busqueda + ''%''
          OR P.Nombre LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @IdProveedor IS NULL
          OR C.IdProveedor = @IdProveedor
      )

      AND
      (
          @FechaDesde IS NULL
          OR C.FechaCompra >= @FechaDesde
      )

      AND
      (
          @FechaHasta IS NULL
          OR C.FechaCompra < DATEADD(DAY, 1, @FechaHasta)
      )

    ORDER BY
        C.FechaCompra DESC,
        C.IdCompra DESC

    OFFSET @RegistrosOmitir ROWS

    FETCH NEXT @CantidadPagina ROWS ONLY;


    ------------------------------------------------------------
    -- RESULT SET 2
    ------------------------------------------------------------

    SELECT
        COUNT(*)

    FROM Compras C

    INNER JOIN Proveedores P
        ON C.IdProveedor = P.IdProveedor

    WHERE C.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR C.NumeroCompra LIKE ''%'' + @Busqueda + ''%''
          OR P.Nombre LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @IdProveedor IS NULL
          OR C.IdProveedor = @IdProveedor
      )

      AND
      (
          @FechaDesde IS NULL
          OR C.FechaCompra >= @FechaDesde
      )

      AND
      (
          @FechaHasta IS NULL
          OR C.FechaCompra < DATEADD(DAY, 1, @FechaHasta)
      );

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarComprasPaginado';


/* =========================================================
   17. SP_ConsultarDashboard
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarDashboard]
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;


    DECLARE @InicioMes DATE =
        DATEFROMPARTS
        (
            YEAR(GETDATE()),
            MONTH(GETDATE()),
            1
        );


    DECLARE @InicioMesSiguiente DATE =
        DATEADD
        (
            MONTH,
            1,
            @InicioMes
        );


    ------------------------------------------------------------
    -- RESULT SET 1
    -- INDICADORES PRINCIPALES
    ------------------------------------------------------------

    SELECT

        ISNULL
        (
            (
                SELECT SUM(Total)
                FROM Ventas
                WHERE IdEmpresa = @IdEmpresa
                  AND Estado = 1
                  AND FechaVenta >= @InicioMes
                  AND FechaVenta < @InicioMesSiguiente
            ),
            0
        ) AS VentasMes,


        ISNULL
        (
            (
                SELECT SUM(Total)
                FROM Compras
                WHERE IdEmpresa = @IdEmpresa
                  AND Estado = 1
                  AND FechaCompra >= @InicioMes
                  AND FechaCompra < @InicioMesSiguiente
            ),
            0
        ) AS ComprasMes,


        ISNULL
        (
            (
                SELECT
                    SUM
                    (
                        (DV.PrecioUnitario - DV.CostoUnitario)
                        * DV.Cantidad
                    )

                FROM DetalleVenta DV

                INNER JOIN Ventas V
                    ON DV.IdVenta = V.IdVenta

                WHERE V.IdEmpresa = @IdEmpresa
                  AND V.Estado = 1
                  AND V.FechaVenta >= @InicioMes
                  AND V.FechaVenta < @InicioMesSiguiente
            ),
            0
        ) AS GananciaBrutaMes,


        (
            SELECT COUNT(*)
            FROM Productos
            WHERE IdEmpresa = @IdEmpresa
              AND Estado = 1
        ) AS ProductosActivos,


        (
            SELECT COUNT(*)
            FROM Productos
            WHERE IdEmpresa = @IdEmpresa
              AND Estado = 1
              AND StockActual <= StockMinimo
        ) AS ProductosStockBajo,


        (
            SELECT COUNT(*)
            FROM Clientes
            WHERE IdEmpresa = @IdEmpresa
              AND Estado = 1
        ) AS ClientesActivos,


        (
            SELECT COUNT(*)
            FROM Proveedores
            WHERE IdEmpresa = @IdEmpresa
              AND Estado = 1
        ) AS ProveedoresActivos;



    ------------------------------------------------------------
    -- RESULT SET 2
    -- PRODUCTOS CON STOCK BAJO
    ------------------------------------------------------------

    SELECT TOP 10

        P.IdProducto,
        P.Codigo,
        P.Nombre,
        C.Nombre AS Categ' AS nvarchar(max))
        + N'oria,

        P.StockActual,
        P.StockMinimo

    FROM Productos P

    INNER JOIN Categorias C
        ON P.IdCategoria = C.IdCategoria

    WHERE P.IdEmpresa = @IdEmpresa
      AND P.Estado = 1
      AND P.StockActual <= P.StockMinimo

    ORDER BY
        P.StockActual ASC,
        P.Nombre ASC;



    ------------------------------------------------------------
    -- RESULT SET 3
    -- VENTAS ULTIMOS 6 MESES
    ------------------------------------------------------------

    ;WITH Meses AS
    (
        SELECT
            DATEADD(MONTH, -5, @InicioMes) AS FechaMes

        UNION ALL

        SELECT
            DATEADD(MONTH, 1, FechaMes)
        FROM Meses
        WHERE FechaMes < @InicioMes
    )

    SELECT
        M.FechaMes,

        YEAR(M.FechaMes) AS Anio,

        MONTH(M.FechaMes) AS Mes,

        ISNULL
        (
            SUM(V.Total),
            0
        ) AS TotalVentas

    FROM Meses M

    LEFT JOIN Ventas V
        ON V.IdEmpresa = @IdEmpresa
       AND V.Estado = 1
       AND V.FechaVenta >= M.FechaMes
       AND V.FechaVenta < DATEADD(MONTH, 1, M.FechaMes)

    GROUP BY
        M.FechaMes

    ORDER BY
        M.FechaMes;



    ------------------------------------------------------------
    -- RESULT SET 4
    -- PRODUCTOS MAS VENDIDOS DEL MES
    ------------------------------------------------------------

    SELECT TOP 5

        P.IdProducto,
        P.Codigo,
        P.Nombre,

        SUM(DV.Cantidad) AS CantidadVendida,

        SUM(DV.Subtotal) AS TotalVendido

    FROM DetalleVenta DV

    INNER JOIN Ventas V
        ON DV.IdVenta = V.IdVenta

    INNER JOIN Productos P
        ON DV.IdProducto = P.IdProducto

    WHERE V.IdEmpresa = @IdEmpresa
      AND V.Estado = 1
      AND V.FechaVenta >= @InicioMes
      AND V.FechaVenta < @InicioMesSiguiente

    GROUP BY
        P.IdProducto,
        P.Codigo,
        P.Nombre

    ORDER BY
        CantidadVendida DESC;

END;';

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarDashboard';


/* =========================================================
   18. SP_ConsultarDetalleCompra
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarDetalleCompra]
    @IdCompra BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DC.IdDetalleCompra,
        DC.IdProducto,

        P.Codigo AS CodigoProducto,
        P.Nombre AS Producto,

        DC.Cantidad,
        DC.PrecioUnitario,
        DC.Subtotal

    FROM DetalleCompra DC

    INNER JOIN Compras C
        ON DC.IdCompra = C.IdCompra

    INNER JOIN Productos P
        ON DC.IdProducto = P.IdProducto

    WHERE DC.IdCompra = @IdCompra
      AND C.IdEmpresa = @IdEmpresa

    ORDER BY DC.IdDetalleCompra;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarDetalleCompra';


/* =========================================================
   19. SP_ConsultarDetalleVenta
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarDetalleVenta]
    @IdVenta BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        DV.IdDetalleVenta,
        DV.IdProducto,

        P.Codigo AS CodigoProducto,
        P.Nombre AS Producto,

        DV.Cantidad,
        DV.PrecioUnitario,
        DV.Subtotal

    FROM DetalleVenta DV

    INNER JOIN Ventas V
        ON DV.IdVenta = V.IdVenta

    INNER JOIN Productos P
        ON DV.IdProducto = P.IdProducto

    WHERE DV.IdVenta = @IdVenta
      AND V.IdEmpresa = @IdEmpresa

    ORDER BY DV.IdDetalleVenta;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarDetalleVenta';


/* =========================================================
   20. SP_ConsultarHashUsuario
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarHashUsuario]
    @IdUsuario BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.Contrasena
    FROM dbo.Usuarios U
    INNER JOIN dbo.Empresas E
        ON E.IdEmpresa = U.IdEmpresa
    INNER JOIN dbo.Roles R
        ON R.IdRol = U.IdRol
    WHERE U.IdUsuario = @IdUsuario
      AND U.IdEmpresa = @IdEmpresa
      AND U.Estado = 1
      AND E.Estado = 1
      AND R.Estado = 1;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarHashUsuario';


/* =========================================================
   21. SP_ConsultarMiCuenta
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarMiCuenta]
    @IdUsuario BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.IdUsuario,
        U.Nombre,
        U.Correo,
        R.NombreRol AS Rol,
        E.Nombre AS NombreEmpresa,
        E.Identificacion AS IdentificacionEmpresa,
        U.FechaRegistro
    FROM dbo.Usuarios U
    INNER JOIN dbo.Roles R
        ON R.IdRol = U.IdRol
    INNER JOIN dbo.Empresas E
        ON E.IdEmpresa = U.IdEmpresa
    WHERE U.IdUsuario = @IdUsuario
      AND U.IdEmpresa = @IdEmpresa
      AND U.Estado = 1
      AND R.Estado = 1
      AND E.Estado = 1;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarMiCuenta';


/* =========================================================
   22. SP_ConsultarMovimientosInventario
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarMovimientosInventario]
    @IdEmpresa BIGINT,
    @IdProducto BIGINT = NULL,
    @IdTipoMovimiento BIGINT = NULL,
    @FechaDesde DATE = NULL,
    @FechaHasta DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        M.IdMovimiento,

        M.IdProducto,
        P.Codigo AS CodigoProducto,
        P.Nombre AS Producto,

        M.IdTipoMovimiento,
        T.Nombre AS TipoMovimiento,
        T.Naturaleza,

        M.IdUsuario,
        U.Nombre AS Usuario,

        M.IdCompra,
        C.NumeroCompra,

        M.IdVenta,
        V.NumeroVenta,

        M.Cantidad,
        M.StockAnterior,
        M.StockNuevo,

        M.Motivo,
        M.FechaMovimiento

    FROM MovimientosInventario M

    INNER JOIN Productos P
        ON M.IdProducto = P.IdProducto

    INNER JOIN TiposMovimiento T
        ON M.IdTipoMovimiento = T.IdTipoMovimiento

    INNER JOIN Usuarios U
        ON M.IdUsuario = U.IdUsuario

    LEFT JOIN Compras C
        ON M.IdCompra = C.IdCompra

    LEFT JOIN Ventas V
        ON M.IdVenta = V.IdVenta

    WHERE M.IdEmpresa = @IdEmpresa

      AND
      (
          @IdProducto IS NULL
          OR M.IdProducto = @IdProducto
      )

      AND
      (
          @IdTipoMovimiento IS NULL
          OR M.IdTipoMovimiento = @IdTipoMovimiento
      )

      AND
      (
          @FechaDesde IS NULL
          OR M.FechaMovimiento >= @FechaDesde
      )

      AND
      (
          @FechaHasta IS NULL
          OR M.FechaMovimiento < DATEADD(DAY, 1, @FechaHasta)
      )

    ORDER BY M.FechaMovimiento DESC,
             M.IdMovimiento DESC;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarMovimientosInventario';


/* =========================================================
   23. SP_ConsultarMovimientosInventarioPaginado
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarMovimientosInventarioPaginado]
    @IdEmpresa BIGINT,
    @Busqueda VARCHAR(150) = NULL,
    @IdProducto BIGINT = NULL,
    @IdTipoMovimiento BIGINT = NULL,
    @FechaDesde DATE = NULL,
    @FechaHasta DATE = NULL,
    @Pagina INT = 1,
    @CantidadPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;


    ------------------------------------------------------------
    -- NORMALIZAR PAGINACION
    ------------------------------------------------------------

    IF @Pagina < 1
        SET @Pagina = 1;


    IF @CantidadPagina NOT IN (10, 20, 50)
        SET @CantidadPagina = 10;


    DECLARE @RegistrosOmitir INT =
        (@Pagina - 1) * @CantidadPagina;


    SET @Busqueda =
        NULLIF(LTRIM(RTRIM(@Busqueda)), '''');


    ------------------------------------------------------------
    -- RESULT SET 1
    ------------------------------------------------------------

    SELECT
        M.IdMovimiento,

        M.IdProducto,
        P.Codigo AS CodigoProducto,
        P.Nombre AS Producto,

        M.IdTipoMovimiento,
        T.Nombre AS TipoMovimiento,
        T.Naturaleza,

        M.IdUsuario,
        U.Nombre AS Usuario,

        M.IdCompra,
        C.NumeroCompra,

        M.IdVenta,
        V.NumeroVenta,

        M.Cantidad,
        M.StockAnterior,
        M.StockNuevo,

        M.Motivo,
        M.FechaMovimiento

    FROM MovimientosInventario M

    INNER JOIN Productos P
        ON M.IdProducto = P.IdProducto

    INNER JOIN TiposMovimiento T
        ON M.IdTipoMovimiento = T.IdTipoMovimiento

    INNER JOIN Usuarios U
        ON M.IdUsuario = U.IdUsuario

    LEFT JOIN Compras C
        ON M.IdCompra = C.IdCompra

    LEFT JOIN Ventas V
        ON M.IdVenta = V.IdVenta

    WHERE M.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR P.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR P.Codigo LIKE ''%'' + @Busqueda + ''%''
          OR M.Motivo LIKE ''%'' + @Busqueda + ''%''
          OR C.NumeroCompra LIKE ''%'' + @Busqueda + ''%''
          OR V.NumeroVenta LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @IdProducto IS NULL
          OR M.IdProducto = @IdProducto
      )

      AND
      (
          @IdTipoMovimiento IS NULL
          OR M.IdTipoMovimiento = @IdTipoMovimiento
      )

      AND
      (
          @FechaDesde IS NULL
          OR M.FechaMovimiento >= @FechaDesde
      )

      AND
      (
          @FechaHasta IS NULL
          OR M.FechaMovimiento <
             DATEADD(DAY, 1, @FechaHasta)
      )

    ORDER BY
        M.FechaMovimiento DESC,
        M.IdMovimiento DESC

    OFFSET @RegistrosOmitir ROWS

    FETCH NEXT @CantidadPagina ROWS ONLY;



    ------------------------------------------------------------
    -- RESULT SET 2
    --------------------------------------------------------' AS nvarchar(max))
        + N'----

    SELECT
        COUNT(*)

    FROM MovimientosInventario M

    INNER JOIN Productos P
        ON M.IdProducto = P.IdProducto

    LEFT JOIN Compras C
        ON M.IdCompra = C.IdCompra

    LEFT JOIN Ventas V
        ON M.IdVenta = V.IdVenta

    WHERE M.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR P.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR P.Codigo LIKE ''%'' + @Busqueda + ''%''
          OR M.Motivo LIKE ''%'' + @Busqueda + ''%''
          OR C.NumeroCompra LIKE ''%'' + @Busqueda + ''%''
          OR V.NumeroVenta LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @IdProducto IS NULL
          OR M.IdProducto = @IdProducto
      )

      AND
      (
          @IdTipoMovimiento IS NULL
          OR M.IdTipoMovimiento = @IdTipoMovimiento
      )

      AND
      (
          @FechaDesde IS NULL
          OR M.FechaMovimiento >= @FechaDesde
      )

      AND
      (
          @FechaHasta IS NULL
          OR M.FechaMovimiento <
             DATEADD(DAY, 1, @FechaHasta)
      );

END;';

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarMovimientosInventarioPaginado';


/* =========================================================
   24. SP_ConsultarProducto
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarProducto]
    @IdProducto BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.IdCategoria,
        C.Nombre AS Categoria,
        P.Codigo,
        P.Nombre,
        P.Descripcion,
        P.PrecioCompra,
        P.PrecioVenta,
        P.StockActual,
        P.StockMinimo,
        P.Estado,
        P.FechaRegistro,

        CASE
            WHEN P.StockActual <= P.StockMinimo THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS StockBajo

    FROM Productos P

    INNER JOIN Categorias C
        ON P.IdCategoria = C.IdCategoria

    WHERE P.IdProducto = @IdProducto
      AND P.IdEmpresa = @IdEmpresa;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarProducto';


/* =========================================================
   25. SP_ConsultarProductos
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarProductos]
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.IdCategoria,
        C.Nombre AS Categoria,
        P.Codigo,
        P.Nombre,
        P.Descripcion,
        P.PrecioCompra,
        P.PrecioVenta,
        P.StockActual,
        P.StockMinimo,
        P.Estado,
        P.FechaRegistro,

        CASE
            WHEN P.StockActual <= P.StockMinimo THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS StockBajo

    FROM Productos P

    INNER JOIN Categorias C
        ON P.IdCategoria = C.IdCategoria

    WHERE P.IdEmpresa = @IdEmpresa

    ORDER BY P.Nombre;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarProductos';


/* =========================================================
   26. SP_ConsultarProductosPaginado
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarProductosPaginado]
    @IdEmpresa BIGINT,
    @Busqueda VARCHAR(150) = NULL,
    @IdCategoria BIGINT = NULL,
    @Estado BIT = NULL,
    @Pagina INT = 1,
    @CantidadPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;


    ------------------------------------------------------------
    -- NORMALIZAR PAGINACION
    ------------------------------------------------------------

    IF @Pagina < 1
        SET @Pagina = 1;

    IF @CantidadPagina NOT IN (10, 20, 50)
        SET @CantidadPagina = 10;


    DECLARE @RegistrosOmitir INT =
        (@Pagina - 1) * @CantidadPagina;


    SET @Busqueda =
        NULLIF(LTRIM(RTRIM(@Busqueda)), '''');


    ------------------------------------------------------------
    -- RESULT SET 1
    -- PRODUCTOS
    ------------------------------------------------------------

    SELECT
        P.IdProducto,
        P.IdCategoria,
        C.Nombre AS Categoria,

        P.Codigo,
        P.Nombre,
        P.Descripcion,

        P.PrecioCompra,
        P.PrecioVenta,

        P.StockActual,
        P.StockMinimo,

        CASE
            WHEN P.StockActual <= P.StockMinimo
                THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS StockBajo,

        P.Estado,
        P.FechaRegistro

    FROM Productos P

    INNER JOIN Categorias C
        ON P.IdCategoria = C.IdCategoria

    WHERE P.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR P.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR P.Codigo LIKE ''%'' + @Busqueda + ''%''
          OR P.Descripcion LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @IdCategoria IS NULL
          OR P.IdCategoria = @IdCategoria
      )

      AND
      (
          @Estado IS NULL
          OR P.Estado = @Estado
      )

    ORDER BY
        P.Nombre

    OFFSET @RegistrosOmitir ROWS

    FETCH NEXT @CantidadPagina ROWS ONLY;


    ------------------------------------------------------------
    -- RESULT SET 2
    -- TOTAL REGISTROS
    ------------------------------------------------------------

    SELECT
        COUNT(*) AS TotalRegistros

    FROM Productos P

    WHERE P.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR P.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR P.Codigo LIKE ''%'' + @Busqueda + ''%''
          OR P.Descripcion LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @IdCategoria IS NULL
          OR P.IdCategoria = @IdCategoria
      )

      AND
      (
          @Estado IS NULL
          OR P.Estado = @Estado
      );

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarProductosPaginado';


/* =========================================================
   27. SP_ConsultarProveedor
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarProveedor]
    @IdProveedor BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdProveedor,
        Identificacion,
        Nombre,
        Correo,
        Telefono,
        Direccion,
        Estado,
        FechaRegistro
    FROM Proveedores
    WHERE IdProveedor = @IdProveedor
      AND IdEmpresa = @IdEmpresa;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarProveedor';


/* =========================================================
   28. SP_ConsultarProveedores
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarProveedores]
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdProveedor,
        Identificacion,
        Nombre,
        Correo,
        Telefono,
        Direccion,
        Estado,
        FechaRegistro
    FROM Proveedores
    WHERE IdEmpresa = @IdEmpresa
    ORDER BY Nombre;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarProveedores';


/* =========================================================
   29. SP_ConsultarProveedoresPaginado
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarProveedoresPaginado]
    @IdEmpresa BIGINT,
    @Busqueda VARCHAR(150) = NULL,
    @Estado BIT = NULL,
    @Pagina INT = 1,
    @CantidadPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;


    ------------------------------------------------------------
    -- NORMALIZAR PAGINACION
    ------------------------------------------------------------

    IF @Pagina < 1
        SET @Pagina = 1;


    IF @CantidadPagina NOT IN (10, 20, 50)
        SET @CantidadPagina = 10;


    DECLARE @RegistrosOmitir INT =
        (@Pagina - 1) * @CantidadPagina;


    SET @Busqueda =
        NULLIF(LTRIM(RTRIM(@Busqueda)), '''');


    ------------------------------------------------------------
    -- RESULT SET 1
    ------------------------------------------------------------

    SELECT
        P.IdProveedor,
        P.Identificacion,
        P.Nombre,
        P.Correo,
        P.Telefono,
        P.Direccion,
        P.Estado,
        P.FechaRegistro

    FROM Proveedores P

    WHERE P.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR P.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR P.Identificacion LIKE ''%'' + @Busqueda + ''%''
          OR P.Correo LIKE ''%'' + @Busqueda + ''%''
          OR P.Telefono LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @Estado IS NULL
          OR P.Estado = @Estado
      )

    ORDER BY P.Nombre

    OFFSET @RegistrosOmitir ROWS

    FETCH NEXT @CantidadPagina ROWS ONLY;


    ------------------------------------------------------------
    -- RESULT SET 2
    ------------------------------------------------------------

    SELECT
        COUNT(*)

    FROM Proveedores P

    WHERE P.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR P.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR P.Identificacion LIKE ''%'' + @Busqueda + ''%''
          OR P.Correo LIKE ''%'' + @Busqueda + ''%''
          OR P.Telefono LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @Estado IS NULL
          OR P.Estado = @Estado
      );

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarProveedoresPaginado';


/* =========================================================
   30. SP_ConsultarRolesGestionUsuarios
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarRolesGestionUsuarios]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdRol,
        NombreRol
    FROM dbo.Roles
    WHERE Estado = 1
      AND NombreRol IN (''Empleado'', ''Consulta'')
    ORDER BY
        CASE NombreRol
            WHEN ''Empleado'' THEN 1
            WHEN ''Consulta'' THEN 2
            ELSE 3
        END,
        NombreRol;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarRolesGestionUsuarios';


/* =========================================================
   31. SP_ConsultarTiposMovimiento
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarTiposMovimiento]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdTipoMovimiento,
        Nombre,
        Naturaleza,
        Estado
    FROM TiposMovimiento
    WHERE Estado = 1
    ORDER BY Nombre;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarTiposMovimiento';


/* =========================================================
   32. SP_ConsultarUsuario
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarUsuario]
    @IdUsuario BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.IdUsuario,
        U.IdRol,
        R.NombreRol AS Rol,
        U.Nombre,
        U.Correo,
        U.Estado,
        U.FechaRegistro
    FROM dbo.Usuarios U
    INNER JOIN dbo.Roles R
        ON R.IdRol = U.IdRol
    WHERE U.IdUsuario = @IdUsuario
      AND U.IdEmpresa = @IdEmpresa;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarUsuario';


/* =========================================================
   33. SP_ConsultarUsuarioLogin
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarUsuarioLogin]
    @IdentificacionEmpresa VARCHAR(30),
    @Correo VARCHAR(150)
AS
BEGIN

    SET NOCOUNT ON;

    SELECT
        U.IdUsuario,
        U.IdEmpresa,
        U.Nombre AS NombreUsuario,
        E.Nombre AS NombreEmpresa,
        U.Correo,
        U.Contrasena,
        R.NombreRol AS Rol
    FROM Usuarios U
    INNER JOIN Empresas E
        ON U.IdEmpresa = E.IdEmpresa
    INNER JOIN Roles R
        ON U.IdRol = R.IdRol
    WHERE E.Identificacion = @IdentificacionEmpresa
      AND U.Correo = @Correo
      AND U.Estado = 1
      AND E.Estado = 1
      AND R.Estado = 1;

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarUsuarioLogin';


/* =========================================================
   34. SP_ConsultarUsuariosPaginado
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarUsuariosPaginado]
    @IdEmpresa BIGINT,
    @Busqueda VARCHAR(150) = NULL,
    @IdRol BIGINT = NULL,
    @Estado BIT = NULL,
    @Pagina INT = 1,
    @CantidadPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina < 1
        SET @Pagina = 1;

    IF @CantidadPagina NOT IN (10, 20, 50)
        SET @CantidadPagina = 10;

    DECLARE @RegistrosOmitir INT =
        (@Pagina - 1) * @CantidadPagina;

    SET @Busqueda =
        NULLIF(LTRIM(RTRIM(@Busqueda)), '''');

    ------------------------------------------------------------
    -- RESULT SET 1
    ------------------------------------------------------------

    SELECT
        U.IdUsuario,
        U.IdRol,
        R.NombreRol AS Rol,
        U.Nombre,
        U.Correo,
        U.Estado,
        U.FechaRegistro
    FROM dbo.Usuarios U
    INNER JOIN dbo.Roles R
        ON R.IdRol = U.IdRol
    WHERE U.IdEmpresa = @IdEmpresa
      AND
      (
          @Busqueda IS NULL
          OR U.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR U.Correo LIKE ''%'' + @Busqueda + ''%''
          OR R.NombreRol LIKE ''%'' + @Busqueda + ''%''
      )
      AND
      (
          @IdRol IS NULL
          OR U.IdRol = @IdRol
      )
      AND
      (
          @Estado IS NULL
          OR U.Estado = @Estado
      )
    ORDER BY
        CASE R.NombreRol
            WHEN ''Administrador'' THEN 0
            WHEN ''Empleado'' THEN 1
            WHEN ''Consulta'' THEN 2
            ELSE 3
        END,
        U.Nombre,
        U.IdUsuario
    OFFSET @RegistrosOmitir ROWS
    FETCH NEXT @CantidadPagina ROWS ONLY;

    ------------------------------------------------------------
    -- RESULT SET 2
    ------------------------------------------------------------

    SELECT COUNT(*)
    FROM dbo.Usuarios U
    INNER JOIN dbo.Roles R
        ON R.IdRol = U.IdRol
    WHERE U.IdEmpresa = @IdEmpresa
      AND
      (
          @Busqueda IS NULL
          OR U.Nombre LIKE ''%'' + @Busqueda + ''%''
          OR U.Correo LIKE ''%'' + @Busqueda + ''%''
          OR R.NombreRol LIKE ''%'' + @Busqueda + ''%''
      )
      AND
      (
          @IdRol IS NULL
          OR U.IdRol = @IdRol
      )
      AND
      (
          @Estado IS NULL
          OR U.Estado = @Estado
      );
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarUsuariosPaginado';


/* =========================================================
   35. SP_ConsultarVenta
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarVenta]
    @IdVenta BIGINT,
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.IdVenta,
        V.NumeroVenta,
        V.FechaVenta,

        V.IdCliente,
        ISNULL(C.Nombre, ''Cliente Ocasional'') AS Cliente,

        V.IdUsuario,
        U.Nombre AS Usuario,

        V.Subtotal,
        V.PorcentajeImpuesto,
        V.Impuesto,
        V.Total,
        V.Estado

    FROM Ventas V

    LEFT JOIN Clientes C
        ON V.IdCliente = C.IdCliente

    INNER JOIN Usuarios U
        ON V.IdUsuario = U.IdUsuario

    WHERE V.IdVenta = @IdVenta
      AND V.IdEmpresa = @IdEmpresa;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarVenta';


/* =========================================================
   36. SP_ConsultarVentas
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarVentas]
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.IdVenta,
        V.NumeroVenta,
        V.FechaVenta,

        V.IdCliente,
        ISNULL(C.Nombre, ''Cliente Ocasional'') AS Cliente,

        V.IdUsuario,
        U.Nombre AS Usuario,

        V.Subtotal,
        V.PorcentajeImpuesto,
        V.Impuesto,
        V.Total,
        V.Estado

    FROM Ventas V

    LEFT JOIN Clientes C
        ON V.IdCliente = C.IdCliente

    INNER JOIN Usuarios U
        ON V.IdUsuario = U.IdUsuario

    WHERE V.IdEmpresa = @IdEmpresa

    ORDER BY V.FechaVenta DESC;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarVentas';


/* =========================================================
   37. SP_ConsultarVentasPaginado
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ConsultarVentasPaginado]
    @IdEmpresa BIGINT,
    @Busqueda VARCHAR(150) = NULL,
    @IdCliente BIGINT = NULL,
    @FechaDesde DATE = NULL,
    @FechaHasta DATE = NULL,
    @Pagina INT = 1,
    @CantidadPagina INT = 10
AS
BEGIN

    SET NOCOUNT ON;


    ------------------------------------------------------------
    -- NORMALIZAR PAGINACION
    ------------------------------------------------------------

    IF @Pagina < 1
        SET @Pagina = 1;


    IF @CantidadPagina NOT IN (10, 20, 50)
        SET @CantidadPagina = 10;


    DECLARE @RegistrosOmitir INT =
        (@Pagina - 1) * @CantidadPagina;


    SET @Busqueda =
        NULLIF(LTRIM(RTRIM(@Busqueda)), '''');


    ------------------------------------------------------------
    -- RESULT SET 1
    -- VENTAS
    ------------------------------------------------------------

    SELECT
        V.IdVenta,
        V.NumeroVenta,
        V.FechaVenta,

        V.IdCliente,

        ISNULL(
            C.Nombre,
            ''Cliente Ocasional''
        ) AS Cliente,

        V.IdUsuario,
        U.Nombre AS Usuario,

        V.Subtotal,
        V.PorcentajeImpuesto,
        V.Impuesto,
        V.Total,

        V.Estado

    FROM Ventas V

    LEFT JOIN Clientes C
        ON V.IdCliente = C.IdCliente

    INNER JOIN Usuarios U
        ON V.IdUsuario = U.IdUsuario

    WHERE V.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR V.NumeroVenta LIKE ''%'' + @Busqueda + ''%''

          OR ISNULL(
                C.Nombre,
                ''Cliente Ocasional''
             )
             LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @IdCliente IS NULL
          OR V.IdCliente = @IdCliente
      )

      AND
      (
          @FechaDesde IS NULL
          OR V.FechaVenta >= @FechaDesde
      )

      AND
      (
          @FechaHasta IS NULL
          OR V.FechaVenta <
             DATEADD(DAY, 1, @FechaHasta)
      )

    ORDER BY
        V.FechaVenta DESC,
        V.IdVenta DESC

    OFFSET @RegistrosOmitir ROWS

    FETCH NEXT @CantidadPagina ROWS ONLY;



    ------------------------------------------------------------
    -- RESULT SET 2
    -- TOTAL
    ------------------------------------------------------------

    SELECT
        COUNT(*)

    FROM Ventas V

    LEFT JOIN Clientes C
        ON V.IdCliente = C.IdCliente

    WHERE V.IdEmpresa = @IdEmpresa

      AND
      (
          @Busqueda IS NULL

          OR V.NumeroVenta LIKE ''%'' + @Busqueda + ''%''

          OR ISNULL(
                C.Nombre,
                ''Cliente Ocasional''
             )
             LIKE ''%'' + @Busqueda + ''%''
      )

      AND
      (
          @IdCliente IS NULL
          OR V.IdCliente = @IdCliente
      )

      AND
      (
          @FechaDesde IS NULL' AS nvarchar(max))
        + N'
          OR V.FechaVenta >= @FechaDesde
      )

      AND
      (
          @FechaHasta IS NULL
          OR V.FechaVenta <
             DATEADD(DAY, 1, @FechaHasta)
      );

END;';

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ConsultarVentasPaginado';


/* =========================================================
   38. SP_ModificarCategoria
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ModificarCategoria]
    @IdCategoria BIGINT,
    @IdEmpresa BIGINT,
    @Nombre VARCHAR(100),
    @Descripcion VARCHAR(250) = NULL,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';


    IF NOT EXISTS
    (
        SELECT 1
        FROM Categorias
        WHERE IdCategoria = @IdCategoria
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''La categoría no existe.'';
        RETURN;
    END;


    IF EXISTS
    (
        SELECT 1
        FROM Categorias
        WHERE IdEmpresa = @IdEmpresa
          AND Nombre = @Nombre
          AND IdCategoria <> @IdCategoria
    )
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''Ya existe otra categoría con ese nombre.'';
        RETURN;
    END;


    BEGIN TRY

        UPDATE Categorias
        SET
            Nombre = @Nombre,
            Descripcion = @Descripcion
        WHERE IdCategoria = @IdCategoria
          AND IdEmpresa = @IdEmpresa;


        SET @Resultado = 1;
        SET @Mensaje = ''Categoría modificada correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ModificarCategoria';


/* =========================================================
   39. SP_ModificarCliente
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ModificarCliente]
    @IdCliente BIGINT,
    @IdEmpresa BIGINT,
    @Identificacion VARCHAR(30) = NULL,
    @Nombre VARCHAR(150),
    @Correo VARCHAR(150) = NULL,
    @Telefono VARCHAR(30) = NULL,
    @Direccion VARCHAR(300) = NULL,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';


    ----------------------------------------------------
    -- Validar cliente
    ----------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM Clientes
        WHERE IdCliente = @IdCliente
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El cliente no existe.'';
        RETURN;
    END;


    ----------------------------------------------------
    -- Validar identificación duplicada
    ----------------------------------------------------

    IF @Identificacion IS NOT NULL
       AND LTRIM(RTRIM(@Identificacion)) <> ''''
       AND EXISTS
       (
           SELECT 1
           FROM Clientes
           WHERE IdEmpresa = @IdEmpresa
             AND Identificacion = @Identificacion
             AND IdCliente <> @IdCliente
       )
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''Ya existe otro cliente con esa identificación.'';
        RETURN;
    END;


    BEGIN TRY

        UPDATE Clientes
        SET
            Identificacion =
                NULLIF(LTRIM(RTRIM(@Identificacion)), ''''),
            Nombre = @Nombre,
            Correo = @Correo,
            Telefono = @Telefono,
            Direccion = @Direccion

        WHERE IdCliente = @IdCliente
          AND IdEmpresa = @IdEmpresa;


        SET @Resultado = 1;
        SET @Mensaje = ''Cliente modificado correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ModificarCliente';


/* =========================================================
   40. SP_ModificarProducto
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ModificarProducto]
    @IdProducto BIGINT,
    @IdEmpresa BIGINT,
    @IdCategoria BIGINT,
    @Nombre VARCHAR(150),
    @Descripcion VARCHAR(300) = NULL,
    @PrecioCompra DECIMAL(18,2),
    @PrecioVenta DECIMAL(18,2),
    @StockMinimo DECIMAL(18,3),

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';


    -- =========================================================
    -- VALIDAR PRODUCTO
    -- =========================================================

    IF NOT EXISTS
    (
        SELECT 1
        FROM Productos
        WHERE IdProducto = @IdProducto
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN

        SET @Resultado = -1;
        SET @Mensaje =
            ''El producto no existe.'';

        RETURN;

    END;


    -- =========================================================
    -- VALIDAR CATEGORÍA
    -- =========================================================

    IF NOT EXISTS
    (
        SELECT 1
        FROM Categorias
        WHERE IdCategoria = @IdCategoria
          AND IdEmpresa = @IdEmpresa
          AND Estado = 1
    )
    BEGIN

        SET @Resultado = -2;

        SET @Mensaje =
            ''La categoría no existe o está inactiva.'';

        RETURN;

    END;


    -- =========================================================
    -- VALIDAR VALORES
    -- =========================================================

    IF @PrecioCompra < 0
       OR @PrecioVenta < 0
       OR @StockMinimo < 0
    BEGIN

        SET @Resultado = -3;

        SET @Mensaje =
            ''Los precios y el stock mínimo no pueden ser negativos.'';

        RETURN;

    END;


    IF @StockMinimo <> FLOOR(@StockMinimo)
    BEGIN

        SET @Resultado = -3;

        SET @Mensaje =
            ''El stock mínimo debe ser un número entero.'';

        RETURN;

    END;


    BEGIN TRY

        UPDATE Productos
        SET
            IdCategoria = @IdCategoria,
            Nombre = @Nombre,
            Descripcion = @Descripcion,
            PrecioCompra = @PrecioCompra,
            PrecioVenta = @PrecioVenta,
            StockMinimo = @StockMinimo
        WHERE IdProducto = @IdProducto
          AND IdEmpresa = @IdEmpresa;


        SET @Resultado = 1;

        SET @Mensaje =
            ''Producto modificado correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;

        SET @Mensaje =
            ERROR_MESSAGE();

    END CATCH

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ModificarProducto';


/* =========================================================
   41. SP_ModificarProveedor
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ModificarProveedor]
    @IdProveedor BIGINT,
    @IdEmpresa BIGINT,
    @Identificacion VARCHAR(30) = NULL,
    @Nombre VARCHAR(150),
    @Correo VARCHAR(150) = NULL,
    @Telefono VARCHAR(30) = NULL,
    @Direccion VARCHAR(300) = NULL,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';


    IF NOT EXISTS
    (
        SELECT 1
        FROM Proveedores
        WHERE IdProveedor = @IdProveedor
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El proveedor no existe.'';
        RETURN;
    END;


    IF @Identificacion IS NOT NULL
       AND LTRIM(RTRIM(@Identificacion)) <> ''''
       AND EXISTS
       (
           SELECT 1
           FROM Proveedores
           WHERE IdEmpresa = @IdEmpresa
             AND Identificacion = @Identificacion
             AND IdProveedor <> @IdProveedor
       )
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''Ya existe otro proveedor con esa identificación.'';
        RETURN;
    END;


    BEGIN TRY

        UPDATE Proveedores
        SET
            Identificacion =
                NULLIF(LTRIM(RTRIM(@Identificacion)), ''''),
            Nombre = @Nombre,
            Correo = @Correo,
            Telefono = @Telefono,
            Direccion = @Direccion

        WHERE IdProveedor = @IdProveedor
          AND IdEmpresa = @IdEmpresa;


        SET @Resultado = 1;
        SET @Mensaje = ''Proveedor modificado correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ModificarProveedor';


/* =========================================================
   42. SP_ModificarUsuario
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ModificarUsuario]
    @IdUsuario BIGINT,
    @IdEmpresa BIGINT,
    @IdRol BIGINT,
    @Nombre VARCHAR(150),
    @Correo VARCHAR(150),
    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';

    SET @Nombre = LTRIM(RTRIM(@Nombre));
    SET @Correo = LTRIM(RTRIM(@Correo));

    ------------------------------------------------------------
    -- EXISTENCIA / MULTIEMPRESA
    ------------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios
        WHERE IdUsuario = @IdUsuario
          AND IdEmpresa = @IdEmpresa
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El usuario no existe.'';
        RETURN;
    END;

    ------------------------------------------------------------
    -- PROTEGER ADMINISTRADOR
    ------------------------------------------------------------

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios U
        INNER JOIN dbo.Roles R
            ON R.IdRol = U.IdRol
        WHERE U.IdUsuario = @IdUsuario
          AND U.IdEmpresa = @IdEmpresa
          AND R.NombreRol = ''Administrador''
    )
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''La cuenta Administrador no se modifica desde este módulo.'';
        RETURN;
    END;

    ------------------------------------------------------------
    -- VALIDAR NUEVO ROL
    ------------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Roles
        WHERE IdRol = @IdRol
          AND Estado = 1
          AND NombreRol IN (''Empleado'', ''Consulta'')
    )
    BEGIN
        SET @Resultado = -3;
        SET @Mensaje = ''El rol seleccionado no es válido.'';
        RETURN;
    END;

    ------------------------------------------------------------
    -- CORREO DUPLICADO
    ------------------------------------------------------------

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios
        WHERE IdEmpresa = @IdEmpresa
          AND Correo = @Correo
          AND IdUsuario <> @IdUsuario
    )
    BEGIN
        SET @Resultado = -4;
        SET @Mensaje = ''Ya existe otro usuario con ese correo en la empresa.'';
        RETURN;
    END;

    BEGIN TRY

        UPDATE dbo.Usuarios
        SET
            IdRol = @IdRol,
            Nombre = @Nombre,
            Correo = @Correo
        WHERE IdUsuario = @IdUsuario
          AND IdEmpresa = @IdEmpresa;

        SET @Resultado = 1;
        SET @Mensaje = ''Usuario modificado correctamente.'';

    END TRY
    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ModificarUsuario';


/* =========================================================
   43. SP_RegistrarCategoria
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_RegistrarCategoria]
    @IdEmpresa BIGINT,
    @Nombre VARCHAR(100),
    @Descripcion VARCHAR(250) = NULL,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT,
    @IdCategoria BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';
    SET @IdCategoria = 0;


    IF EXISTS
    (
        SELECT 1
        FROM Categorias
        WHERE IdEmpresa = @IdEmpresa
          AND Nombre = @Nombre
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''Ya existe una categoría con ese nombre.'';
        RETURN;
    END;


    BEGIN TRY

        INSERT INTO Categorias
        (
            IdEmpresa,
            Nombre,
            Descripcion
        )
        VALUES
        (
            @IdEmpresa,
            @Nombre,
            @Descripcion
        );


        SET @IdCategoria = SCOPE_IDENTITY();

        SET @Resultado = 1;
        SET @Mensaje = ''Categoría registrada correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH

END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_RegistrarCategoria';


/* =========================================================
   44. SP_RegistrarCliente
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_RegistrarCliente]
    @IdEmpresa BIGINT,
    @Identificacion VARCHAR(30) = NULL,
    @Nombre VARCHAR(150),
    @Correo VARCHAR(150) = NULL,
    @Telefono VARCHAR(30) = NULL,
    @Direccion VARCHAR(300) = NULL,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT,
    @IdCliente BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';
    SET @IdCliente = 0;


    ----------------------------------------------------
    -- Validar identificación duplicada
    ----------------------------------------------------

    IF @Identificacion IS NOT NULL
       AND LTRIM(RTRIM(@Identificacion)) <> ''''
       AND EXISTS
       (
           SELECT 1
           FROM Clientes
           WHERE IdEmpresa = @IdEmpresa
             AND Identificacion = @Identificacion
       )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''Ya existe un cliente con esa identificación.'';
        RETURN;
    END;


    BEGIN TRY

        INSERT INTO Clientes
        (
            IdEmpresa,
            Identificacion,
            Nombre,
            Correo,
            Telefono,
            Direccion
        )
        VALUES
        (
            @IdEmpresa,
            NULLIF(LTRIM(RTRIM(@Identificacion)), ''''),
            @Nombre,
            @Correo,
            @Telefono,
            @Direccion
        );


        SET @IdCliente = SCOPE_IDENTITY();

        SET @Resultado = 1;
        SET @Mensaje = ''Cliente registrado correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_RegistrarCliente';


/* =========================================================
   45. SP_RegistrarCompra
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_RegistrarCompra]
    @IdEmpresa BIGINT,
    @IdProveedor BIGINT,
    @IdUsuario BIGINT,
    @PorcentajeImpuesto DECIMAL(5,2),
    @Detalles dbo.CompraDetalleType READONLY,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(250) OUTPUT,
    @IdCompra BIGINT OUTPUT,
    @NumeroCompra VARCHAR(50) OUTPUT,
    @Subtotal DECIMAL(18,2) OUTPUT,
    @Impuesto DECIMAL(18,2) OUTPUT,
    @Total DECIMAL(18,2) OUTPUT
AS
BEGIN

    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';
    SET @IdCompra = 0;
    SET @NumeroCompra = '''';
    SET @Subtotal = 0;
    SET @Impuesto = 0;
    SET @Total = 0;


    ---------------------------------------------------------
    -- 1. VALIDAR PROVEEDOR
    ---------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM Proveedores
        WHERE IdProveedor = @IdProveedor
          AND IdEmpresa = @IdEmpresa
          AND Estado = 1
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El proveedor no existe o está inactivo.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 2. VALIDAR USUARIO
    ---------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM Usuarios
        WHERE IdUsuario = @IdUsuario
          AND IdEmpresa = @IdEmpresa
          AND Estado = 1
    )
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''El usuario no pertenece a la empresa.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 3. VALIDAR QUE HAYA PRODUCTOS
    ---------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM @Detalles
    )
    BEGIN
        SET @Resultado = -3;
        SET @Mensaje = ''La compra debe contener al menos un producto.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 4. VALIDAR CANTIDADES Y PRECIOS
    ---------------------------------------------------------

    IF EXISTS
    (
        SELECT 1
        FROM @Detalles
        WHERE Cantidad <= 0
           OR Cantidad <> FLOOR(Cantidad)
           OR PrecioUnitario < 0
    )
    BEGIN
        SET @Resultado = -4;
        SET @Mensaje = ''Las cantidades deben ser enteras mayores a cero y los precios no pueden ser negativos.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 5. VALIDAR PRODUCTOS Y MULTIEMPRESA
    ---------------------------------------------------------

    IF EXISTS
    (
        SELECT 1
        FROM @Detalles D

        LEFT JOIN Productos P
            ON P.IdProducto = D.IdProducto
           AND P.IdEmpresa = @IdEmpresa
           AND P.Estado = 1

        WHERE P.IdProducto IS NULL
    )
    ' AS nvarchar(max))
        + N'BEGIN
        SET @Resultado = -5;
        SET @Mensaje = ''Uno o más productos no existen, están inactivos o pertenecen a otra empresa.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 6. VALIDAR IMPUESTO
    ---------------------------------------------------------

    IF @PorcentajeImpuesto < 0
       OR @PorcentajeImpuesto > 100
    BEGIN
        SET @Resultado = -6;
        SET @Mensaje = ''El porcentaje de impuesto no es válido.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 7. OBTENER TIPO MOVIMIENTO COMPRA
    ---------------------------------------------------------

    DECLARE @IdTipoMovimiento BIGINT;

    SELECT @IdTipoMovimiento = IdTipoMovimiento
    FROM TiposMovimiento
    WHERE Nombre = ''Compra''
      AND Naturaleza = ''ENTRADA''
      AND Estado = 1;


    IF @IdTipoMovimiento IS NULL
    BEGIN
        SET @Resultado = -7;
        SET @Mensaje = ''No existe el tipo de movimiento Compra.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 8. CALCULAR TOTALES
    ---------------------------------------------------------

    SELECT
        @Subtotal =
            ISNULL(
                SUM(
                    ROUND(Cantidad * PrecioUnitario, 2)
                ),
                0
            )
    FROM @Detalles;


    SET @Impuesto =
        ROUND(
            @Subtotal * (@PorcentajeImpuesto / 100),
            2
        );


    SET @Total =
        @Subtotal + @Impuesto;


    ---------------------------------------------------------
    -- 9. GENERAR NUMERO DE COMPRA
    ---------------------------------------------------------

    DECLARE @Consecutivo BIGINT;

    SET @Consecutivo =
        NEXT VALUE FOR dbo.SeqNumeroCompra;


    SET @NumeroCompra =
        ''COMP-'' +
        RIGHT(
            ''00000000'' + CAST(@Consecutivo AS VARCHAR(20)),
            8
        );


    ---------------------------------------------------------
    -- 10. INICIAR TRANSACCION
    ---------------------------------------------------------

    BEGIN TRY

        BEGIN TRANSACTION;


        -----------------------------------------------------
        -- CREAR CABECERA COMPRA
        -----------------------------------------------------

        INSERT INTO Compras
        (
            IdEmpresa,
            IdProveedor,
            IdUsuario,
            NumeroCompra,
            Subtotal,
            PorcentajeImpuesto,
            Impuesto,
            Total
        )
        VALUES
        (
            @IdEmpresa,
            @IdProveedor,
            @IdUsuario,
            @NumeroCompra,
            @Subtotal,
            @PorcentajeImpuesto,
            @Impuesto,
            @Total
        );


        SET @IdCompra = SCOPE_IDENTITY();


        ------'
        + N'-----------------------------------------------
        -- CREAR DETALLE
        -----------------------------------------------------

        INSERT INTO DetalleCompra
        (
            IdCompra,
            IdProducto,
            Cantidad,
            PrecioUnitario,
            Subtotal
        )
        SELECT
            @IdCompra,
            IdProducto,
            Cantidad,
            PrecioUnitario,
            ROUND(Cantidad * PrecioUnitario, 2)
        FROM @Detalles;


        -----------------------------------------------------
        -- GUARDAR STOCK ANTERIOR Y NUEVO
        -----------------------------------------------------

        DECLARE @StockMovimientos TABLE
        (
            IdProducto BIGINT,
            StockAnterior DECIMAL(18,3),
            StockNuevo DECIMAL(18,3)
        );


        -----------------------------------------------------
        -- AUMENTAR INVENTARIO
        -----------------------------------------------------

        UPDATE P
        SET
            P.StockActual =
                P.StockActual + D.Cantidad,

            P.PrecioCompra =
                D.PrecioUnitario

        OUTPUT
            INSERTED.IdProducto,
            DELETED.StockActual,
            INSERTED.StockActual

        INTO @StockMovimientos
        (
            IdProducto,
            StockAnterior,
            StockNuevo
        )

        FROM Productos P

        INNER JOIN @Detalles D
            ON P.IdProducto = D.IdProducto

        WHERE P.IdEmpresa = @IdEmpresa;


        -----------------------------------------------------
        -- REGISTRAR MOVIMIENTOS DE INVENTARIO
        -----------------------------------------------------

        INSERT INTO MovimientosInventario
        (
            IdEmpresa,
            IdProducto,
            IdUsuario,
            IdTipoMovimiento,
            IdCompra,
            IdVenta,
            Cantidad,
            StockAnterior,
            StockNuevo,
            Motivo
        )
        SELECT
            @IdEmpresa,
            M.IdProducto,
            @IdUsuario,
            @IdTipoMovimiento,
            @IdCompra,
            NULL,
            D.Cantidad,
            M.StockAnterior,
            M.StockNuevo,
            ''Compra '' + @NumeroCompra

        FROM @StockMovimientos M

        INNER JOIN @Detalles D
            ON M.IdProducto = D.IdProducto;


        -----------------------------------------------------
        -- CONFIRMAR TODO
        -----------------------------------------------------

        COMMIT TRANSACTION;


        SET @Resultado = 1;
        SET @Mensaje = ''Compra registrada correctamente.'';

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;


        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH

END;';

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_RegistrarCompra';


/* =========================================================
   46. SP_RegistrarEmpresaAdministrador
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_RegistrarEmpresaAdministrador]
    @IdentificacionEmpresa VARCHAR(30),
    @NombreEmpresa VARCHAR(150),
    @CorreoEmpresa VARCHAR(150) = NULL,
    @TelefonoEmpresa VARCHAR(30) = NULL,
    @DireccionEmpresa VARCHAR(300) = NULL,

    @NombreAdministrador VARCHAR(150),
    @CorreoAdministrador VARCHAR(150),
    @Contrasena VARCHAR(250),

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT,
    @IdEmpresa BIGINT OUTPUT,
    @IdUsuario BIGINT OUTPUT
AS
BEGIN

    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';
    SET @IdEmpresa = 0;
    SET @IdUsuario = 0;

    BEGIN TRY

        ----------------------------------------------------
        -- Validar identificación de empresa
        ----------------------------------------------------

        IF EXISTS
        (
            SELECT 1
            FROM Empresas
            WHERE Identificacion = @IdentificacionEmpresa
        )
        BEGIN
            SET @Resultado = -1;
            SET @Mensaje = ''Ya existe una empresa con esa identificación.'';
            RETURN;
        END;


        ----------------------------------------------------
        -- Obtener rol Administrador
        ----------------------------------------------------

        DECLARE @IdRolAdministrador BIGINT;

        SELECT @IdRolAdministrador = IdRol
        FROM Roles
        WHERE NombreRol = ''Administrador''
          AND Estado = 1;


        IF @IdRolAdministrador IS NULL
        BEGIN
            SET @Resultado = -2;
            SET @Mensaje = ''No existe el rol Administrador.'';
            RETURN;
        END;


        ----------------------------------------------------
        -- Iniciar transacción
        ----------------------------------------------------

        BEGIN TRANSACTION;


        ----------------------------------------------------
        -- Registrar empresa
        ----------------------------------------------------

        INSERT INTO Empresas
        (
            Identificacion,
            Nombre,
            Correo,
            Telefono,
            Direccion
        )
        VALUES
        (
            @IdentificacionEmpresa,
            @NombreEmpresa,
            @CorreoEmpresa,
            @TelefonoEmpresa,
            @DireccionEmpresa
        );


        SET @IdEmpresa = SCOPE_IDENTITY();


        ----------------------------------------------------
        -- Registrar administrador
        ----------------------------------------------------

        INSERT INTO Usuarios
        (
            IdEmpresa,
            IdRol,
            Nombre,
            Correo,
            Contrasena
        )
        VALUES
        (
            @IdEmpresa,
            @IdRolAdministrador,
            @NombreAdministrador,
            @CorreoAdministrador,
            @Contrasena
        );' AS nvarchar(max))
        + N'


        SET @IdUsuario = SCOPE_IDENTITY();


        ----------------------------------------------------
        -- Confirmar transacción
        ----------------------------------------------------

        COMMIT TRANSACTION;


        SET @Resultado = 1;
        SET @Mensaje = ''Empresa registrada correctamente.'';

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH

END;';

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_RegistrarEmpresaAdministrador';


/* =========================================================
   47. SP_RegistrarProducto
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_RegistrarProducto]
    @IdEmpresa BIGINT,
    @IdCategoria BIGINT,
    @Nombre VARCHAR(150),
    @Descripcion VARCHAR(300) = NULL,
    @PrecioCompra DECIMAL(18,2),
    @PrecioVenta DECIMAL(18,2),
    @StockMinimo DECIMAL(18,3),

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT,
    @IdProducto BIGINT OUTPUT,
    @Codigo VARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';
    SET @IdProducto = 0;
    SET @Codigo = '''';


    -- =========================================================
    -- VALIDAR CATEGORÍA
    -- =========================================================

    IF NOT EXISTS
    (
        SELECT 1
        FROM Categorias
        WHERE IdCategoria = @IdCategoria
          AND IdEmpresa = @IdEmpresa
          AND Estado = 1
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje =
            ''La categoría no existe o está inactiva.'';

        RETURN;
    END;


    -- =========================================================
    -- VALIDAR VALORES
    -- =========================================================

    IF @PrecioCompra < 0
       OR @PrecioVenta < 0
       OR @StockMinimo < 0
    BEGIN
        SET @Resultado = -3;
        SET @Mensaje =
            ''Los precios y el stock mínimo no pueden ser negativos.'';

        RETURN;
    END;


    -- =========================================================
    -- STOCK MÍNIMO ENTERO
    -- =========================================================

    IF @StockMinimo <> FLOOR(@StockMinimo)
    BEGIN
        SET @Resultado = -3;
        SET @Mensaje =
            ''El stock mínimo debe ser un número entero.'';

        RETURN;
    END;


    BEGIN TRY

        BEGIN TRANSACTION;


        DECLARE @SiguienteNumero INT;


        -- =====================================================
        -- OBTENER SIGUIENTE CONSECUTIVO DE LA EMPRESA
        --
        -- TEC001
        -- TEC002
        -- TEC003
        -- =====================================================

        SELECT
            @SiguienteNumero =
                ISNULL(
                    MAX(
                        TRY_CONVERT(
                            INT,
                            SUBSTRING(
                                Codigo,
                                4,
                                47
                            )
                        )
                    ),
                    0
                ) + 1
        FROM Productos WITH
        (
            UPDLOCK,
            HOLDLOCK
        )
        WHERE IdEmpresa = @IdEmpresa
          AND Codigo LIKE ''TEC%''
          AND TRY_CONVERT(
                INT,
                SUBSTRING(
                    Codigo,
                    4,
                    47
                )
              )' AS nvarchar(max))
        + N' IS NOT NULL;


        -- =====================================================
        -- GENERAR CÓDIGO
        -- =====================================================

        IF @SiguienteNumero < 1000
        BEGIN

            SET @Codigo =
                ''TEC''
                +
                RIGHT(
                    ''000''
                    +
                    CAST(
                        @SiguienteNumero
                        AS VARCHAR(10)
                    ),
                    3
                );

        END
        ELSE
        BEGIN

            SET @Codigo =
                ''TEC''
                +
                CAST(
                    @SiguienteNumero
                    AS VARCHAR(20)
                );

        END;


        -- =====================================================
        -- INSERTAR
        -- =====================================================

        INSERT INTO Productos
        (
            IdEmpresa,
            IdCategoria,
            Codigo,
            Nombre,
            Descripcion,
            PrecioCompra,
            PrecioVenta,
            StockActual,
            StockMinimo
        )
        VALUES
        (
            @IdEmpresa,
            @IdCategoria,
            @Codigo,
            @Nombre,
            @Descripcion,
            @PrecioCompra,
            @PrecioVenta,
            0,
            @StockMinimo
        );


        SET @IdProducto =
            SCOPE_IDENTITY();


        COMMIT TRANSACTION;


        SET @Resultado = 1;

        SET @Mensaje =
            ''Producto registrado correctamente.'';

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;


        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH

END;';

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_RegistrarProducto';


/* =========================================================
   48. SP_RegistrarProveedor
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_RegistrarProveedor]
    @IdEmpresa BIGINT,
    @Identificacion VARCHAR(30) = NULL,
    @Nombre VARCHAR(150),
    @Correo VARCHAR(150) = NULL,
    @Telefono VARCHAR(30) = NULL,
    @Direccion VARCHAR(300) = NULL,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT,
    @IdProveedor BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';
    SET @IdProveedor = 0;


    IF @Identificacion IS NOT NULL
       AND LTRIM(RTRIM(@Identificacion)) <> ''''
       AND EXISTS
       (
           SELECT 1
           FROM Proveedores
           WHERE IdEmpresa = @IdEmpresa
             AND Identificacion = @Identificacion
       )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''Ya existe un proveedor con esa identificación.'';
        RETURN;
    END;


    BEGIN TRY

        INSERT INTO Proveedores
        (
            IdEmpresa,
            Identificacion,
            Nombre,
            Correo,
            Telefono,
            Direccion
        )
        VALUES
        (
            @IdEmpresa,
            NULLIF(LTRIM(RTRIM(@Identificacion)), ''''),
            @Nombre,
            @Correo,
            @Telefono,
            @Direccion
        );


        SET @IdProveedor = SCOPE_IDENTITY();

        SET @Resultado = 1;
        SET @Mensaje = ''Proveedor registrado correctamente.'';

    END TRY

    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_RegistrarProveedor';


/* =========================================================
   49. SP_RegistrarUsuario
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_RegistrarUsuario]
    @IdEmpresa BIGINT,
    @IdRol BIGINT,
    @Nombre VARCHAR(150),
    @Correo VARCHAR(150),
    @Contrasena VARCHAR(250),
    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(200) OUTPUT,
    @IdUsuario BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';
    SET @IdUsuario = 0;

    SET @Nombre = LTRIM(RTRIM(@Nombre));
    SET @Correo = LTRIM(RTRIM(@Correo));

    ------------------------------------------------------------
    -- VALIDAR ROL
    ------------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Roles
        WHERE IdRol = @IdRol
          AND Estado = 1
          AND NombreRol IN (''Empleado'', ''Consulta'')
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El rol seleccionado no es válido.'';
        RETURN;
    END;

    ------------------------------------------------------------
    -- VALIDAR CORREO DENTRO DE LA EMPRESA
    ------------------------------------------------------------

    IF EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios
        WHERE IdEmpresa = @IdEmpresa
          AND Correo = @Correo
    )
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''Ya existe un usuario con ese correo en la empresa.'';
        RETURN;
    END;

    BEGIN TRY

        INSERT INTO dbo.Usuarios
        (
            IdEmpresa,
            IdRol,
            Nombre,
            Correo,
            Contrasena
        )
        VALUES
        (
            @IdEmpresa,
            @IdRol,
            @Nombre,
            @Correo,
            @Contrasena
        );

        SET @IdUsuario = SCOPE_IDENTITY();
        SET @Resultado = 1;
        SET @Mensaje = ''Usuario registrado correctamente.'';

    END TRY
    BEGIN CATCH

        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_RegistrarUsuario';


/* =========================================================
   50. SP_RegistrarVenta
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_RegistrarVenta]
    @IdEmpresa BIGINT,
    @IdCliente BIGINT = NULL,
    @IdUsuario BIGINT,
    @PorcentajeImpuesto DECIMAL(5,2),
    @Detalles dbo.VentaDetalleType READONLY,

    @Resultado INT OUTPUT,
    @Mensaje VARCHAR(250) OUTPUT,
    @IdVenta BIGINT OUTPUT,
    @NumeroVenta VARCHAR(50) OUTPUT,
    @Subtotal DECIMAL(18,2) OUTPUT,
    @Impuesto DECIMAL(18,2) OUTPUT,
    @Total DECIMAL(18,2) OUTPUT
AS
BEGIN

    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Resultado = 0;
    SET @Mensaje = '''';
    SET @IdVenta = 0;
    SET @NumeroVenta = '''';
    SET @Subtotal = 0;
    SET @Impuesto = 0;
    SET @Total = 0;


    ---------------------------------------------------------
    -- 1. VALIDAR USUARIO
    ---------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM Usuarios
        WHERE IdUsuario = @IdUsuario
          AND IdEmpresa = @IdEmpresa
          AND Estado = 1
    )
    BEGIN
        SET @Resultado = -1;
        SET @Mensaje = ''El usuario no pertenece a la empresa.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 2. VALIDAR CLIENTE
    ---------------------------------------------------------

    IF @IdCliente IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM Clientes
           WHERE IdCliente = @IdCliente
             AND IdEmpresa = @IdEmpresa
             AND Estado = 1
       )
    BEGIN
        SET @Resultado = -2;
        SET @Mensaje = ''El cliente no existe o está inactivo.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 3. VALIDAR QUE HAYA DETALLES
    ---------------------------------------------------------

    IF NOT EXISTS
    (
        SELECT 1
        FROM @Detalles
    )
    BEGIN
        SET @Resultado = -3;
        SET @Mensaje = ''La venta debe contener al menos un producto.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 4. VALIDAR CANTIDADES
    ---------------------------------------------------------

    IF EXISTS
    (
        SELECT 1
        FROM @Detalles
        WHERE Cantidad <= 0
           OR Cantidad <> FLOOR(Cantidad)
    )
    BEGIN
        SET @Resultado = -4;
        SET @Mensaje = ''Las cantidades deben ser enteras mayores a cero.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 5. VALIDAR IMPUESTO
    ---------------------------------------------------------

    IF @PorcentajeImpuesto < 0
       OR @PorcentajeImpuesto > 100
    BEGIN
        SET @Resultado = -5;
        SET @Mensaje = ''El porcentaje de impuesto no es válido.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 6. TIPO MOVIMIENTO VENTA
    --------' AS nvarchar(max))
        + N'-------------------------------------------------

    DECLARE @IdTipoMovimiento BIGINT;

    SELECT @IdTipoMovimiento = IdTipoMovimiento
    FROM TiposMovimiento
    WHERE Nombre = ''Venta''
      AND Naturaleza = ''SALIDA''
      AND Estado = 1;


    IF @IdTipoMovimiento IS NULL
    BEGIN
        SET @Resultado = -8;
        SET @Mensaje = ''No existe el tipo de movimiento Venta.'';
        RETURN;
    END;


    ---------------------------------------------------------
    -- 7. TRANSACCION
    ---------------------------------------------------------

    BEGIN TRY

        BEGIN TRANSACTION;


        -----------------------------------------------------
        -- VALIDAR PRODUCTOS
        -----------------------------------------------------

        IF EXISTS
        (
            SELECT 1
            FROM @Detalles D

            LEFT JOIN Productos P WITH (UPDLOCK, HOLDLOCK)
                ON P.IdProducto = D.IdProducto
               AND P.IdEmpresa = @IdEmpresa
               AND P.Estado = 1

            WHERE P.IdProducto IS NULL
        )
        BEGIN

            SET @Resultado = -6;
            SET @Mensaje =
                ''Uno o más productos no existen, están inactivos o pertenecen a otra empresa.'';

            ROLLBACK TRANSACTION;
            RETURN;

        END;


        -----------------------------------------------------
        -- VALIDAR STOCK
        -----------------------------------------------------

        IF EXISTS
        (
            SELECT 1
            FROM @Detalles D

            INNER JOIN Productos P WITH (UPDLOCK, HOLDLOCK)
                ON P.IdProducto = D.IdProducto
               AND P.IdEmpresa = @IdEmpresa

            WHERE P.StockActual < D.Cantidad
        )
        BEGIN

            SET @Resultado = -7;
            SET @Mensaje =
                ''Uno o más productos no tienen stock suficiente.'';

            ROLLBACK TRANSACTION;
            RETURN;

        END;


        -----------------------------------------------------
        -- CALCULAR SUBTOTAL
        -----------------------------------------------------

        SELECT
            @Subtotal =
                ISNULL
                (
                    SUM
                    (
                        ROUND
                        (
                            D.Cantidad * P.PrecioVenta,
                            2
                        )
                    ),
                    0
                )
        FROM @Detalles D

        INNER JOIN Productos P
            ON D.IdProducto = P.IdProducto
           AND P.IdEmpresa = @IdEmpresa;


        -----------------------------------------------------
        -- IMPUESTO Y TOTAL
        -----------------------------------------------------

        SET @Impuesto =
            ROUND
            (
                @Subtotal *
                (@Porcenta'
        + N'jeImpuesto / 100),
                2
            );

        SET @Total =
            @Subtotal + @Impuesto;


        -----------------------------------------------------
        -- GENERAR NUMERO DE VENTA
        -----------------------------------------------------

        DECLARE @Consecutivo BIGINT;

        SET @Consecutivo =
            NEXT VALUE FOR dbo.SeqNumeroVenta;


        SET @NumeroVenta =
            ''VENT-'' +
            RIGHT
            (
                ''00000000''
                + CAST(@Consecutivo AS VARCHAR(20)),
                8
            );


        -----------------------------------------------------
        -- CABECERA DE VENTA
        -----------------------------------------------------

        INSERT INTO Ventas
        (
            IdEmpresa,
            IdCliente,
            IdUsuario,
            NumeroVenta,
            Subtotal,
            PorcentajeImpuesto,
            Impuesto,
            Total
        )
        VALUES
        (
            @IdEmpresa,
            @IdCliente,
            @IdUsuario,
            @NumeroVenta,
            @Subtotal,
            @PorcentajeImpuesto,
            @Impuesto,
            @Total
        );


        SET @IdVenta = SCOPE_IDENTITY();


        -----------------------------------------------------
        -- DETALLE DE VENTA
        -- AQUÍ ESTÁ LA MODIFICACIÓN DE COSTOUNITARIO
        -----------------------------------------------------

        INSERT INTO DetalleVenta
        (
            IdVenta,
            IdProducto,
            Cantidad,
            PrecioUnitario,
            CostoUnitario,
            Subtotal
        )
        SELECT
            @IdVenta,
            D.IdProducto,
            D.Cantidad,
            P.PrecioVenta,
            P.PrecioCompra,
            ROUND
            (
                D.Cantidad * P.PrecioVenta,
                2
            )
        FROM @Detalles D

        INNER JOIN Productos P
            ON D.IdProducto = P.IdProducto
           AND P.IdEmpresa = @IdEmpresa;


        -----------------------------------------------------
        -- TABLA TEMPORAL PARA MOVIMIENTOS
        -----------------------------------------------------

        DECLARE @StockMovimientos TABLE
        (
            IdProducto BIGINT,
            StockAnterior DECIMAL(18,3),
            StockNuevo DECIMAL(18,3)
        );


        -----------------------------------------------------
        -- DESCONTAR STOCK
        -----------------------------------------------------

        UPDATE P
        SET
            P.StockActual =
                P.StockActual - D.Cantidad

        OUTPUT
            INSERTED.IdProducto,
            DELETED.StockActual,
            INSERTED.StockActual

        INTO @StockMovimientos
        (
            IdProducto,
            StockAnterior,
            StockNuevo
        '
        + N')

        FROM Productos P

        INNER JOIN @Detalles D
            ON P.IdProducto = D.IdProducto

        WHERE P.IdEmpresa = @IdEmpresa;


        -----------------------------------------------------
        -- MOVIMIENTO DE INVENTARIO
        -----------------------------------------------------

        INSERT INTO MovimientosInventario
        (
            IdEmpresa,
            IdProducto,
            IdUsuario,
            IdTipoMovimiento,
            IdCompra,
            IdVenta,
            Cantidad,
            StockAnterior,
            StockNuevo,
            Motivo
        )
        SELECT
            @IdEmpresa,
            M.IdProducto,
            @IdUsuario,
            @IdTipoMovimiento,
            NULL,
            @IdVenta,
            D.Cantidad,
            M.StockAnterior,
            M.StockNuevo,
            ''Venta '' + @NumeroVenta

        FROM @StockMovimientos M

        INNER JOIN @Detalles D
            ON M.IdProducto = D.IdProducto;


        -----------------------------------------------------
        -- CONFIRMAR TRANSACCION
        -----------------------------------------------------

        COMMIT TRANSACTION;


        SET @Resultado = 1;
        SET @Mensaje = ''Venta registrada correctamente.'';

    END TRY

    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;


        SET @Resultado = -99;
        SET @Mensaje = ERROR_MESSAGE();

    END CATCH

END;';

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_RegistrarVenta';


/* =========================================================
   51. SP_ReporteCompras
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ReporteCompras]
    @IdEmpresa BIGINT,
    @FechaDesde DATE,
    @FechaHasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        C.IdCompra,
        C.NumeroCompra,
        C.FechaCompra,

        P.Nombre AS Proveedor,
        U.Nombre AS Usuario,

        C.Subtotal,
        C.PorcentajeImpuesto,
        C.Impuesto,
        C.Total,
        C.Estado

    FROM Compras C

    INNER JOIN Proveedores P
        ON C.IdProveedor = P.IdProveedor

    INNER JOIN Usuarios U
        ON C.IdUsuario = U.IdUsuario

    WHERE C.IdEmpresa = @IdEmpresa
      AND C.FechaCompra >= @FechaDesde
      AND C.FechaCompra < DATEADD(DAY, 1, @FechaHasta)

    ORDER BY C.FechaCompra DESC;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ReporteCompras';


/* =========================================================
   52. SP_ReporteInventarioActual
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ReporteInventarioActual]
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Codigo,
        P.Nombre,

        C.Nombre AS Categoria,

        P.PrecioCompra,
        P.PrecioVenta,

        P.StockActual,
        P.StockMinimo,

        P.StockActual * P.PrecioCompra
            AS ValorInventario,

        CASE
            WHEN P.StockActual <= P.StockMinimo
                THEN CAST(1 AS BIT)

            ELSE CAST(0 AS BIT)

        END AS StockBajo,

        P.Estado

    FROM Productos P

    INNER JOIN Categorias C
        ON P.IdCategoria = C.IdCategoria

    WHERE P.IdEmpresa = @IdEmpresa

    ORDER BY P.Nombre;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ReporteInventarioActual';


/* =========================================================
   53. SP_ReporteProductosMasVendidos
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ReporteProductosMasVendidos]
    @IdEmpresa BIGINT,
    @FechaDesde DATE,
    @FechaHasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Codigo,
        P.Nombre,

        SUM(DV.Cantidad)
            AS CantidadVendida,

        SUM(DV.Subtotal)
            AS TotalVendido,

        SUM
        (
            (DV.PrecioUnitario - DV.CostoUnitario)
            * DV.Cantidad
        )
            AS GananciaBruta

    FROM DetalleVenta DV

    INNER JOIN Ventas V
        ON DV.IdVenta = V.IdVenta

    INNER JOIN Productos P
        ON DV.IdProducto = P.IdProducto

    WHERE V.IdEmpresa = @IdEmpresa
      AND V.Estado = 1
      AND V.FechaVenta >= @FechaDesde
      AND V.FechaVenta < DATEADD(DAY, 1, @FechaHasta)

    GROUP BY
        P.IdProducto,
        P.Codigo,
        P.Nombre

    ORDER BY
        CantidadVendida DESC;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ReporteProductosMasVendidos';


/* =========================================================
   54. SP_ReporteStockBajo
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ReporteStockBajo]
    @IdEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Codigo,
        P.Nombre,

        C.Nombre AS Categoria,

        P.StockActual,
        P.StockMinimo,

        CASE
            WHEN P.StockMinimo > P.StockActual
                THEN P.StockMinimo - P.StockActual
            ELSE 0
        END AS CantidadFaltante

    FROM Productos P

    INNER JOIN Categorias C
        ON P.IdCategoria = C.IdCategoria

    WHERE P.IdEmpresa = @IdEmpresa
      AND P.Estado = 1
      AND P.StockActual <= P.StockMinimo

    ORDER BY
        P.StockActual ASC,
        P.Nombre;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ReporteStockBajo';


/* =========================================================
   55. SP_ReporteVentas
========================================================= */

SET @sql =
        CAST(N'CREATE OR ALTER PROCEDURE [dbo].[SP_ReporteVentas]
    @IdEmpresa BIGINT,
    @FechaDesde DATE,
    @FechaHasta DATE
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.IdVenta,
        V.NumeroVenta,
        V.FechaVenta,

        ISNULL(C.Nombre, ''Cliente Ocasional'') AS Cliente,

        U.Nombre AS Usuario,

        V.Subtotal,
        V.PorcentajeImpuesto,
        V.Impuesto,
        V.Total,
        V.Estado

    FROM Ventas V

    LEFT JOIN Clientes C
        ON V.IdCliente = C.IdCliente

    INNER JOIN Usuarios U
        ON V.IdUsuario = U.IdUsuario

    WHERE V.IdEmpresa = @IdEmpresa
      AND V.FechaVenta >= @FechaDesde
      AND V.FechaVenta < DATEADD(DAY, 1, @FechaHasta)

    ORDER BY V.FechaVenta DESC;
END;' AS nvarchar(max));

EXEC sys.sp_executesql @sql;

PRINT 'OK - SP_ReporteVentas';


/* =========================================================
   VERIFICACIÓN FINAL
========================================================= */

SELECT
    COUNT(*) AS ProcedimientosStockFlow
FROM sys.procedures
WHERE schema_id = SCHEMA_ID('dbo')
  AND name IN
  (
        'SP_AjustarInventario',
        'SP_CambiarContrasenaPropia',
        'SP_CambiarEstadoCategoria',
        'SP_CambiarEstadoCliente',
        'SP_CambiarEstadoProducto',
        'SP_CambiarEstadoProveedor',
        'SP_CambiarEstadoUsuario',
        'SP_ConsultarCategoria',
        'SP_ConsultarCategorias',
        'SP_ConsultarCategoriasPaginado',
        'SP_ConsultarCliente',
        'SP_ConsultarClientes',
        'SP_ConsultarClientesPaginado',
        'SP_ConsultarCompra',
        'SP_ConsultarCompras',
        'SP_ConsultarComprasPaginado',
        'SP_ConsultarDashboard',
        'SP_ConsultarDetalleCompra',
        'SP_ConsultarDetalleVenta',
        'SP_ConsultarHashUsuario',
        'SP_ConsultarMiCuenta',
        'SP_ConsultarMovimientosInventario',
        'SP_ConsultarMovimientosInventarioPaginado',
        'SP_ConsultarProducto',
        'SP_ConsultarProductos',
        'SP_ConsultarProductosPaginado',
        'SP_ConsultarProveedor',
        'SP_ConsultarProveedores',
        'SP_ConsultarProveedoresPaginado',
        'SP_ConsultarRolesGestionUsuarios',
        'SP_ConsultarTiposMovimiento',
        'SP_ConsultarUsuario',
        'SP_ConsultarUsuarioLogin',
        'SP_ConsultarUsuariosPaginado',
        'SP_ConsultarVenta',
        'SP_ConsultarVentas',
        'SP_ConsultarVentasPaginado',
        'SP_ModificarCategoria',
        'SP_ModificarCliente',
        'SP_ModificarProducto',
        'SP_ModificarProveedor',
        'SP_ModificarUsuario',
        'SP_RegistrarCategoria',
        'SP_RegistrarCliente',
        'SP_RegistrarCompra',
        'SP_RegistrarEmpresaAdministrador',
        'SP_RegistrarProducto',
        'SP_RegistrarProveedor',
        'SP_RegistrarUsuario',
        'SP_RegistrarVenta',
        'SP_ReporteCompras',
        'SP_ReporteInventarioActual',
        'SP_ReporteProductosMasVendidos',
        'SP_ReporteStockBajo',
        'SP_ReporteVentas'
  );


SELECT
    name AS ProcedimientoCritico
FROM sys.procedures
WHERE schema_id = SCHEMA_ID('dbo')
  AND name IN
  (
      'SP_RegistrarCompra',
      'SP_RegistrarVenta',
      'SP_AjustarInventario',
      'SP_RegistrarEmpresaAdministrador',
      'SP_ConsultarUsuarioLogin',
      'SP_ConsultarMiCuenta',
      'SP_CambiarContrasenaPropia'
  )
ORDER BY name;
