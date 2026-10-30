/*
    StockFlowDB - Azure SQL
    BLOQUE 01: Estructura base
    Generado desde BDFinalStock.ipynb

    Incluye:
    - Secuencias
    - Tipos de tabla (TVP)
    - Tablas

    NO incluye:
    - CREATE DATABASE
    - rutas MDF/LDF
    - índices adicionales
    - defaults/FK/CHECK
    - procedimientos almacenados
*/
/****** Objeto: Sequence [dbo].[SeqNumeroCompra] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE SEQUENCE [dbo].[SeqNumeroCompra] 
 AS [bigint]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -9223372036854775808
 MAXVALUE 9223372036854775807
 CACHE


/****** Objeto: Sequence [dbo].[SeqNumeroVenta] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE SEQUENCE [dbo].[SeqNumeroVenta] 
 AS [bigint]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -9223372036854775808
 MAXVALUE 9223372036854775807
 CACHE


/****** Objeto: UserDefinedTableType [dbo].[CompraDetalleType] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE TYPE [dbo].[CompraDetalleType] AS TABLE(
	[IdProducto] [bigint] NOT NULL,
	[Cantidad] [decimal](18, 3) NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[IdProducto] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)


/****** Objeto: UserDefinedTableType [dbo].[VentaDetalleType] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE TYPE [dbo].[VentaDetalleType] AS TABLE(
	[IdProducto] [bigint] NOT NULL,
	[Cantidad] [decimal](18, 3) NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[IdProducto] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)


/****** Objeto: Table [dbo].[Categorias] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Categorias](
	[IdCategoria] [bigint] IDENTITY(1,1) NOT NULL,
	[IdEmpresa] [bigint] NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[Descripcion] [varchar](250) NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Categorias] PRIMARY KEY CLUSTERED 
(
	[IdCategoria] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[Clientes] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Clientes](
	[IdCliente] [bigint] IDENTITY(1,1) NOT NULL,
	[IdEmpresa] [bigint] NOT NULL,
	[Identificacion] [varchar](30) NULL,
	[Nombre] [varchar](150) NOT NULL,
	[Correo] [varchar](150) NULL,
	[Telefono] [varchar](30) NULL,
	[Direccion] [varchar](300) NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Clientes] PRIMARY KEY CLUSTERED 
(
	[IdCliente] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[Compras] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Compras](
	[IdCompra] [bigint] IDENTITY(1,1) NOT NULL,
	[IdEmpresa] [bigint] NOT NULL,
	[IdProveedor] [bigint] NOT NULL,
	[IdUsuario] [bigint] NOT NULL,
	[NumeroCompra] [varchar](50) NOT NULL,
	[FechaCompra] [datetime2](7) NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
	[Impuesto] [decimal](18, 2) NOT NULL,
	[Total] [decimal](18, 2) NOT NULL,
	[Estado] [bit] NOT NULL,
	[PorcentajeImpuesto] [decimal](5, 2) NOT NULL,
 CONSTRAINT [PK_Compras] PRIMARY KEY CLUSTERED 
(
	[IdCompra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[DetalleCompra] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[DetalleCompra](
	[IdDetalleCompra] [bigint] IDENTITY(1,1) NOT NULL,
	[IdCompra] [bigint] NOT NULL,
	[IdProducto] [bigint] NOT NULL,
	[Cantidad] [decimal](18, 3) NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_DetalleCompra] PRIMARY KEY CLUSTERED 
(
	[IdDetalleCompra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[DetalleVenta] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[DetalleVenta](
	[IdDetalleVenta] [bigint] IDENTITY(1,1) NOT NULL,
	[IdVenta] [bigint] NOT NULL,
	[IdProducto] [bigint] NOT NULL,
	[Cantidad] [decimal](18, 3) NOT NULL,
	[PrecioUnitario] [decimal](18, 2) NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
	[CostoUnitario] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_DetalleVenta] PRIMARY KEY CLUSTERED 
(
	[IdDetalleVenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[Empresas] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Empresas](
	[IdEmpresa] [bigint] IDENTITY(1,1) NOT NULL,
	[Identificacion] [varchar](30) NOT NULL,
	[Nombre] [varchar](150) NOT NULL,
	[Correo] [varchar](150) NULL,
	[Telefono] [varchar](30) NULL,
	[Direccion] [varchar](300) NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Empresas] PRIMARY KEY CLUSTERED 
(
	[IdEmpresa] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[MovimientosInventario] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[MovimientosInventario](
	[IdMovimiento] [bigint] IDENTITY(1,1) NOT NULL,
	[IdEmpresa] [bigint] NOT NULL,
	[IdProducto] [bigint] NOT NULL,
	[IdUsuario] [bigint] NOT NULL,
	[IdTipoMovimiento] [bigint] NOT NULL,
	[IdCompra] [bigint] NULL,
	[IdVenta] [bigint] NULL,
	[Cantidad] [decimal](18, 3) NOT NULL,
	[StockAnterior] [decimal](18, 3) NOT NULL,
	[StockNuevo] [decimal](18, 3) NOT NULL,
	[Motivo] [varchar](300) NULL,
	[FechaMovimiento] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_MovimientosInventario] PRIMARY KEY CLUSTERED 
(
	[IdMovimiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[Productos] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Productos](
	[IdProducto] [bigint] IDENTITY(1,1) NOT NULL,
	[IdEmpresa] [bigint] NOT NULL,
	[IdCategoria] [bigint] NOT NULL,
	[Codigo] [varchar](50) NOT NULL,
	[Nombre] [varchar](150) NOT NULL,
	[Descripcion] [varchar](300) NULL,
	[PrecioCompra] [decimal](18, 2) NOT NULL,
	[PrecioVenta] [decimal](18, 2) NOT NULL,
	[StockActual] [decimal](18, 3) NOT NULL,
	[StockMinimo] [decimal](18, 3) NOT NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Productos] PRIMARY KEY CLUSTERED 
(
	[IdProducto] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[Proveedores] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Proveedores](
	[IdProveedor] [bigint] IDENTITY(1,1) NOT NULL,
	[IdEmpresa] [bigint] NOT NULL,
	[Identificacion] [varchar](30) NULL,
	[Nombre] [varchar](150) NOT NULL,
	[Correo] [varchar](150) NULL,
	[Telefono] [varchar](30) NULL,
	[Direccion] [varchar](300) NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Proveedores] PRIMARY KEY CLUSTERED 
(
	[IdProveedor] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[Roles] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Roles](
	[IdRol] [bigint] IDENTITY(1,1) NOT NULL,
	[NombreRol] [varchar](50) NOT NULL,
	[Descripcion] [varchar](200) NULL,
	[Estado] [bit] NOT NULL,
 CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED 
(
	[IdRol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
 CONSTRAINT [UQ_Roles_NombreRol] UNIQUE NONCLUSTERED 
(
	[NombreRol] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[TiposMovimiento] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[TiposMovimiento](
	[IdTipoMovimiento] [bigint] IDENTITY(1,1) NOT NULL,
	[Nombre] [varchar](100) NOT NULL,
	[Naturaleza] [varchar](10) NOT NULL,
	[Estado] [bit] NOT NULL,
 CONSTRAINT [PK_TiposMovimiento] PRIMARY KEY CLUSTERED 
(
	[IdTipoMovimiento] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF),
 CONSTRAINT [UQ_TiposMovimiento_Nombre] UNIQUE NONCLUSTERED 
(
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[Usuarios] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Usuarios](
	[IdUsuario] [bigint] IDENTITY(1,1) NOT NULL,
	[IdEmpresa] [bigint] NOT NULL,
	[IdRol] [bigint] NOT NULL,
	[Nombre] [varchar](150) NOT NULL,
	[Correo] [varchar](150) NOT NULL,
	[Contrasena] [varchar](250) NOT NULL,
	[Estado] [bit] NOT NULL,
	[FechaRegistro] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED 
(
	[IdUsuario] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)


/****** Objeto: Table [dbo].[Ventas] Fecha de script: 28/10/2026 18:05:05 ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Ventas](
	[IdVenta] [bigint] IDENTITY(1,1) NOT NULL,
	[IdEmpresa] [bigint] NOT NULL,
	[IdCliente] [bigint] NULL,
	[IdUsuario] [bigint] NOT NULL,
	[NumeroVenta] [varchar](50) NOT NULL,
	[FechaVenta] [datetime2](7) NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
	[Impuesto] [decimal](18, 2) NOT NULL,
	[Total] [decimal](18, 2) NOT NULL,
	[Estado] [bit] NOT NULL,
	[PorcentajeImpuesto] [decimal](5, 2) NOT NULL,
 CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED 
(
	[IdVenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
)



-- =========================================================
-- COMPROBACIÓN
-- =========================================================
SELECT
    (SELECT COUNT(*) FROM sys.sequences WHERE schema_id = SCHEMA_ID('dbo')) AS Secuencias,
    (SELECT COUNT(*) FROM sys.table_types WHERE schema_id = SCHEMA_ID('dbo')) AS TiposTabla,
    (SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo')) AS Tablas;
