/*
    StockFlowDB - Azure SQL
    BLOQUE 02: Índices + Defaults + Foreign Keys + CHECK Constraints

    Basado en BDFinalStock.ipynb y en las correcciones finales de QA.

    IMPORTANTE:
    - Ejecutar DESPUÉS del Bloque 01.
    - No volver a ejecutar si termina correctamente.
    - Mantiene DECIMAL(18,3) en BD.
    - Refuerza la regla final: cantidades y stock son enteros.
*/



/* =========================================================
   1. ÍNDICES
========================================================= */


/****** Objeto: Index [IX_Categorias_Empresa_Estado] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_Categorias_Empresa_Estado] ON [dbo].[Categorias]
(
	[IdEmpresa] ASC,
	[Estado] ASC
)
INCLUDE([Nombre],[Descripcion],[FechaRegistro]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


SET ANSI_PADDING ON

/****** Objeto: Index [UX_Categorias_Empresa_Nombre] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Categorias_Empresa_Nombre] ON [dbo].[Categorias]
(
	[IdEmpresa] ASC,
	[Nombre] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


/****** Objeto: Index [IX_Clientes_Empresa_Estado] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_Clientes_Empresa_Estado] ON [dbo].[Clientes]
(
	[IdEmpresa] ASC,
	[Estado] ASC
)
INCLUDE([Identificacion],[Nombre],[Correo],[Telefono],[FechaRegistro]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


SET ANSI_PADDING ON

/****** Objeto: Index [UX_Clientes_Empresa_Identificacion] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Clientes_Empresa_Identificacion] ON [dbo].[Clientes]
(
	[IdEmpresa] ASC,
	[Identificacion] ASC
)
WHERE ([Identificacion] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


/****** Objeto: Index [IX_Compras_Empresa_Fecha] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_Compras_Empresa_Fecha] ON [dbo].[Compras]
(
	[IdEmpresa] ASC,
	[FechaCompra] DESC,
	[IdCompra] DESC
)
INCLUDE([IdProveedor],[IdUsuario],[NumeroCompra],[Subtotal],[PorcentajeImpuesto],[Impuesto],[Total],[Estado]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


SET ANSI_PADDING ON

/****** Objeto: Index [UX_Compras_NumeroCompra] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Compras_NumeroCompra] ON [dbo].[Compras]
(
	[NumeroCompra] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


/****** Objeto: Index [IX_DetalleCompra_Compra] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_DetalleCompra_Compra] ON [dbo].[DetalleCompra]
(
	[IdCompra] ASC
)
INCLUDE([IdProducto],[Cantidad],[PrecioUnitario],[Subtotal]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


/****** Objeto: Index [IX_DetalleVenta_Venta] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_DetalleVenta_Venta] ON [dbo].[DetalleVenta]
(
	[IdVenta] ASC
)
INCLUDE([IdProducto],[Cantidad],[PrecioUnitario],[CostoUnitario],[Subtotal]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


SET ANSI_PADDING ON

/****** Objeto: Index [UX_Empresas_Identificacion] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Empresas_Identificacion] ON [dbo].[Empresas]
(
	[Identificacion] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


/****** Objeto: Index [IX_Movimientos_Empresa_Fecha] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_Movimientos_Empresa_Fecha] ON [dbo].[MovimientosInventario]
(
	[IdEmpresa] ASC,
	[FechaMovimiento] DESC,
	[IdMovimiento] DESC
)
INCLUDE([IdProducto],[IdTipoMovimiento],[IdUsuario],[IdCompra],[IdVenta],[Cantidad],[StockAnterior],[StockNuevo]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


/****** Objeto: Index [IX_Productos_Empresa_Estado_Categoria] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_Productos_Empresa_Estado_Categoria] ON [dbo].[Productos]
(
	[IdEmpresa] ASC,
	[Estado] ASC,
	[IdCategoria] ASC
)
INCLUDE([Codigo],[Nombre],[PrecioCompra],[PrecioVenta],[StockActual],[StockMinimo],[FechaRegistro]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


SET ANSI_PADDING ON

/****** Objeto: Index [UX_Productos_Empresa_Codigo] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Productos_Empresa_Codigo] ON [dbo].[Productos]
(
	[IdEmpresa] ASC,
	[Codigo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


/****** Objeto: Index [IX_Proveedores_Empresa_Estado] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_Proveedores_Empresa_Estado] ON [dbo].[Proveedores]
(
	[IdEmpresa] ASC,
	[Estado] ASC
)
INCLUDE([Identificacion],[Nombre],[Correo],[Telefono],[FechaRegistro]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


SET ANSI_PADDING ON

/****** Objeto: Index [UX_Proveedores_Empresa_Identificacion] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Proveedores_Empresa_Identificacion] ON [dbo].[Proveedores]
(
	[IdEmpresa] ASC,
	[Identificacion] ASC
)
WHERE ([Identificacion] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


/****** Objeto: Index [IX_Usuarios_Empresa_Estado_Rol] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_Usuarios_Empresa_Estado_Rol] ON [dbo].[Usuarios]
(
	[IdEmpresa] ASC,
	[Estado] ASC,
	[IdRol] ASC
)
INCLUDE([Nombre],[Correo],[FechaRegistro]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


SET ANSI_PADDING ON

/****** Objeto: Index [UX_Usuarios_Empresa_Correo] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Usuarios_Empresa_Correo] ON [dbo].[Usuarios]
(
	[IdEmpresa] ASC,
	[Correo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


/****** Objeto: Index [IX_Ventas_Empresa_Fecha] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE NONCLUSTERED INDEX [IX_Ventas_Empresa_Fecha] ON [dbo].[Ventas]
(
	[IdEmpresa] ASC,
	[FechaVenta] DESC,
	[IdVenta] DESC
)
INCLUDE([IdCliente],[IdUsuario],[NumeroVenta],[Subtotal],[PorcentajeImpuesto],[Impuesto],[Total],[Estado]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)


SET ANSI_PADDING ON

/****** Objeto: Index [UX_Ventas_NumeroVenta] Fecha de script: 28/10/2026 18:05:05 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_Ventas_NumeroVenta] ON [dbo].[Ventas]
(
	[NumeroVenta] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)



/* =========================================================
   2. DEFAULT CONSTRAINTS
========================================================= */


ALTER TABLE [dbo].[Categorias] ADD  CONSTRAINT [DF_Categorias_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[Categorias] ADD  CONSTRAINT [DF_Categorias_FechaRegistro]  DEFAULT (sysdatetime()) FOR [FechaRegistro]


ALTER TABLE [dbo].[Clientes] ADD  CONSTRAINT [DF_Clientes_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[Clientes] ADD  CONSTRAINT [DF_Clientes_FechaRegistro]  DEFAULT (sysdatetime()) FOR [FechaRegistro]


ALTER TABLE [dbo].[Compras] ADD  CONSTRAINT [DF_Compras_FechaCompra]  DEFAULT (sysdatetime()) FOR [FechaCompra]


ALTER TABLE [dbo].[Compras] ADD  CONSTRAINT [DF_Compras_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[Compras] ADD  CONSTRAINT [DF_Compras_PorcentajeImpuesto]  DEFAULT ((0)) FOR [PorcentajeImpuesto]


ALTER TABLE [dbo].[DetalleVenta] ADD  CONSTRAINT [DF_DetalleVenta_CostoUnitario]  DEFAULT ((0)) FOR [CostoUnitario]


ALTER TABLE [dbo].[Empresas] ADD  CONSTRAINT [DF_Empresas_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[Empresas] ADD  CONSTRAINT [DF_Empresas_FechaRegistro]  DEFAULT (sysdatetime()) FOR [FechaRegistro]


ALTER TABLE [dbo].[MovimientosInventario] ADD  CONSTRAINT [DF_Movimientos_Fecha]  DEFAULT (sysdatetime()) FOR [FechaMovimiento]


ALTER TABLE [dbo].[Productos] ADD  CONSTRAINT [DF_Productos_StockActual]  DEFAULT ((0)) FOR [StockActual]


ALTER TABLE [dbo].[Productos] ADD  CONSTRAINT [DF_Productos_StockMinimo]  DEFAULT ((0)) FOR [StockMinimo]


ALTER TABLE [dbo].[Productos] ADD  CONSTRAINT [DF_Productos_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[Productos] ADD  CONSTRAINT [DF_Productos_FechaRegistro]  DEFAULT (sysdatetime()) FOR [FechaRegistro]


ALTER TABLE [dbo].[Proveedores] ADD  CONSTRAINT [DF_Proveedores_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[Proveedores] ADD  CONSTRAINT [DF_Proveedores_FechaRegistro]  DEFAULT (sysdatetime()) FOR [FechaRegistro]


ALTER TABLE [dbo].[Roles] ADD  CONSTRAINT [DF_Roles_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[TiposMovimiento] ADD  CONSTRAINT [DF_TiposMovimiento_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[Usuarios] ADD  CONSTRAINT [DF_Usuarios_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[Usuarios] ADD  CONSTRAINT [DF_Usuarios_FechaRegistro]  DEFAULT (sysdatetime()) FOR [FechaRegistro]


ALTER TABLE [dbo].[Ventas] ADD  CONSTRAINT [DF_Ventas_FechaVenta]  DEFAULT (sysdatetime()) FOR [FechaVenta]


ALTER TABLE [dbo].[Ventas] ADD  CONSTRAINT [DF_Ventas_Estado]  DEFAULT ((1)) FOR [Estado]


ALTER TABLE [dbo].[Ventas] ADD  CONSTRAINT [DF_Ventas_PorcentajeImpuesto]  DEFAULT ((0)) FOR [PorcentajeImpuesto]



/* =========================================================
   3. FOREIGN KEYS
========================================================= */


ALTER TABLE [dbo].[Categorias]  WITH CHECK ADD  CONSTRAINT [FK_Categorias_Empresas] FOREIGN KEY([IdEmpresa])
REFERENCES [dbo].[Empresas] ([IdEmpresa])

ALTER TABLE [dbo].[Categorias] CHECK CONSTRAINT [FK_Categorias_Empresas]


ALTER TABLE [dbo].[Clientes]  WITH CHECK ADD  CONSTRAINT [FK_Clientes_Empresas] FOREIGN KEY([IdEmpresa])
REFERENCES [dbo].[Empresas] ([IdEmpresa])

ALTER TABLE [dbo].[Clientes] CHECK CONSTRAINT [FK_Clientes_Empresas]


ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [FK_Compras_Empresas] FOREIGN KEY([IdEmpresa])
REFERENCES [dbo].[Empresas] ([IdEmpresa])

ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [FK_Compras_Empresas]


ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [FK_Compras_Proveedores] FOREIGN KEY([IdProveedor])
REFERENCES [dbo].[Proveedores] ([IdProveedor])

ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [FK_Compras_Proveedores]


ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [FK_Compras_Usuarios] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuarios] ([IdUsuario])

ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [FK_Compras_Usuarios]


ALTER TABLE [dbo].[DetalleCompra]  WITH CHECK ADD  CONSTRAINT [FK_DetalleCompra_Compras] FOREIGN KEY([IdCompra])
REFERENCES [dbo].[Compras] ([IdCompra])

ALTER TABLE [dbo].[DetalleCompra] CHECK CONSTRAINT [FK_DetalleCompra_Compras]


ALTER TABLE [dbo].[DetalleCompra]  WITH CHECK ADD  CONSTRAINT [FK_DetalleCompra_Productos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[Productos] ([IdProducto])

ALTER TABLE [dbo].[DetalleCompra] CHECK CONSTRAINT [FK_DetalleCompra_Productos]


ALTER TABLE [dbo].[DetalleVenta]  WITH CHECK ADD  CONSTRAINT [FK_DetalleVenta_Productos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[Productos] ([IdProducto])

ALTER TABLE [dbo].[DetalleVenta] CHECK CONSTRAINT [FK_DetalleVenta_Productos]


ALTER TABLE [dbo].[DetalleVenta]  WITH CHECK ADD  CONSTRAINT [FK_DetalleVenta_Ventas] FOREIGN KEY([IdVenta])
REFERENCES [dbo].[Ventas] ([IdVenta])

ALTER TABLE [dbo].[DetalleVenta] CHECK CONSTRAINT [FK_DetalleVenta_Ventas]


ALTER TABLE [dbo].[MovimientosInventario]  WITH CHECK ADD  CONSTRAINT [FK_Movimientos_Compras] FOREIGN KEY([IdCompra])
REFERENCES [dbo].[Compras] ([IdCompra])

ALTER TABLE [dbo].[MovimientosInventario] CHECK CONSTRAINT [FK_Movimientos_Compras]


ALTER TABLE [dbo].[MovimientosInventario]  WITH CHECK ADD  CONSTRAINT [FK_Movimientos_Empresas] FOREIGN KEY([IdEmpresa])
REFERENCES [dbo].[Empresas] ([IdEmpresa])

ALTER TABLE [dbo].[MovimientosInventario] CHECK CONSTRAINT [FK_Movimientos_Empresas]


ALTER TABLE [dbo].[MovimientosInventario]  WITH CHECK ADD  CONSTRAINT [FK_Movimientos_Productos] FOREIGN KEY([IdProducto])
REFERENCES [dbo].[Productos] ([IdProducto])

ALTER TABLE [dbo].[MovimientosInventario] CHECK CONSTRAINT [FK_Movimientos_Productos]


ALTER TABLE [dbo].[MovimientosInventario]  WITH CHECK ADD  CONSTRAINT [FK_Movimientos_Tipos] FOREIGN KEY([IdTipoMovimiento])
REFERENCES [dbo].[TiposMovimiento] ([IdTipoMovimiento])

ALTER TABLE [dbo].[MovimientosInventario] CHECK CONSTRAINT [FK_Movimientos_Tipos]


ALTER TABLE [dbo].[MovimientosInventario]  WITH CHECK ADD  CONSTRAINT [FK_Movimientos_Usuarios] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuarios] ([IdUsuario])

ALTER TABLE [dbo].[MovimientosInventario] CHECK CONSTRAINT [FK_Movimientos_Usuarios]


ALTER TABLE [dbo].[MovimientosInventario]  WITH CHECK ADD  CONSTRAINT [FK_Movimientos_Ventas] FOREIGN KEY([IdVenta])
REFERENCES [dbo].[Ventas] ([IdVenta])

ALTER TABLE [dbo].[MovimientosInventario] CHECK CONSTRAINT [FK_Movimientos_Ventas]


ALTER TABLE [dbo].[Productos]  WITH CHECK ADD  CONSTRAINT [FK_Productos_Categorias] FOREIGN KEY([IdCategoria])
REFERENCES [dbo].[Categorias] ([IdCategoria])

ALTER TABLE [dbo].[Productos] CHECK CONSTRAINT [FK_Productos_Categorias]


ALTER TABLE [dbo].[Productos]  WITH CHECK ADD  CONSTRAINT [FK_Productos_Empresas] FOREIGN KEY([IdEmpresa])
REFERENCES [dbo].[Empresas] ([IdEmpresa])

ALTER TABLE [dbo].[Productos] CHECK CONSTRAINT [FK_Productos_Empresas]


ALTER TABLE [dbo].[Proveedores]  WITH CHECK ADD  CONSTRAINT [FK_Proveedores_Empresas] FOREIGN KEY([IdEmpresa])
REFERENCES [dbo].[Empresas] ([IdEmpresa])

ALTER TABLE [dbo].[Proveedores] CHECK CONSTRAINT [FK_Proveedores_Empresas]


ALTER TABLE [dbo].[Usuarios]  WITH CHECK ADD  CONSTRAINT [FK_Usuarios_Empresas] FOREIGN KEY([IdEmpresa])
REFERENCES [dbo].[Empresas] ([IdEmpresa])

ALTER TABLE [dbo].[Usuarios] CHECK CONSTRAINT [FK_Usuarios_Empresas]


ALTER TABLE [dbo].[Usuarios]  WITH CHECK ADD  CONSTRAINT [FK_Usuarios_Roles] FOREIGN KEY([IdRol])
REFERENCES [dbo].[Roles] ([IdRol])

ALTER TABLE [dbo].[Usuarios] CHECK CONSTRAINT [FK_Usuarios_Roles]


ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Clientes] FOREIGN KEY([IdCliente])
REFERENCES [dbo].[Clientes] ([IdCliente])

ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Clientes]


ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Empresas] FOREIGN KEY([IdEmpresa])
REFERENCES [dbo].[Empresas] ([IdEmpresa])

ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Empresas]


ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [FK_Ventas_Usuarios] FOREIGN KEY([IdUsuario])
REFERENCES [dbo].[Usuarios] ([IdUsuario])

ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [FK_Ventas_Usuarios]



/* =========================================================
   4. CHECK CONSTRAINTS
========================================================= */


ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [CK_Compras_Impuesto] CHECK  (([Impuesto]>=(0)))

ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [CK_Compras_Impuesto]


ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [CK_Compras_PorcentajeImpuesto] CHECK  (([PorcentajeImpuesto]>=(0) AND [PorcentajeImpuesto]<=(100)))

ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [CK_Compras_PorcentajeImpuesto]


ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [CK_Compras_Subtotal] CHECK  (([Subtotal]>=(0)))

ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [CK_Compras_Subtotal]


ALTER TABLE [dbo].[Compras]  WITH CHECK ADD  CONSTRAINT [CK_Compras_Total] CHECK  (([Total]>=(0)))

ALTER TABLE [dbo].[Compras] CHECK CONSTRAINT [CK_Compras_Total]


ALTER TABLE dbo.DetalleCompra
    WITH CHECK ADD CONSTRAINT CK_DetalleCompra_Cantidad
    CHECK (
        Cantidad > 0
        AND Cantidad = FLOOR(Cantidad)
    );
ALTER TABLE dbo.DetalleCompra
    CHECK CONSTRAINT CK_DetalleCompra_Cantidad;


ALTER TABLE [dbo].[DetalleCompra]  WITH CHECK ADD  CONSTRAINT [CK_DetalleCompra_Precio] CHECK  (([PrecioUnitario]>=(0)))

ALTER TABLE [dbo].[DetalleCompra] CHECK CONSTRAINT [CK_DetalleCompra_Precio]


ALTER TABLE [dbo].[DetalleCompra]  WITH CHECK ADD  CONSTRAINT [CK_DetalleCompra_Subtotal] CHECK  (([Subtotal]>=(0)))

ALTER TABLE [dbo].[DetalleCompra] CHECK CONSTRAINT [CK_DetalleCompra_Subtotal]


ALTER TABLE dbo.DetalleVenta
    WITH CHECK ADD CONSTRAINT CK_DetalleVenta_Cantidad
    CHECK (
        Cantidad > 0
        AND Cantidad = FLOOR(Cantidad)
    );
ALTER TABLE dbo.DetalleVenta
    CHECK CONSTRAINT CK_DetalleVenta_Cantidad;


ALTER TABLE [dbo].[DetalleVenta]  WITH CHECK ADD  CONSTRAINT [CK_DetalleVenta_CostoUnitario] CHECK  (([CostoUnitario]>=(0)))

ALTER TABLE [dbo].[DetalleVenta] CHECK CONSTRAINT [CK_DetalleVenta_CostoUnitario]


ALTER TABLE [dbo].[DetalleVenta]  WITH CHECK ADD  CONSTRAINT [CK_DetalleVenta_Precio] CHECK  (([PrecioUnitario]>=(0)))

ALTER TABLE [dbo].[DetalleVenta] CHECK CONSTRAINT [CK_DetalleVenta_Precio]


ALTER TABLE [dbo].[DetalleVenta]  WITH CHECK ADD  CONSTRAINT [CK_DetalleVenta_Subtotal] CHECK  (([Subtotal]>=(0)))

ALTER TABLE [dbo].[DetalleVenta] CHECK CONSTRAINT [CK_DetalleVenta_Subtotal]


ALTER TABLE dbo.MovimientosInventario
    WITH CHECK ADD CONSTRAINT CK_Movimientos_Cantidad
    CHECK (
        Cantidad > 0
        AND Cantidad = FLOOR(Cantidad)
    );
ALTER TABLE dbo.MovimientosInventario
    CHECK CONSTRAINT CK_Movimientos_Cantidad;


ALTER TABLE dbo.MovimientosInventario
    WITH CHECK ADD CONSTRAINT CK_Movimientos_StockAnterior
    CHECK (
        StockAnterior >= 0
        AND StockAnterior = FLOOR(StockAnterior)
    );
ALTER TABLE dbo.MovimientosInventario
    CHECK CONSTRAINT CK_Movimientos_StockAnterior;


ALTER TABLE dbo.MovimientosInventario
    WITH CHECK ADD CONSTRAINT CK_Movimientos_StockNuevo
    CHECK (
        StockNuevo >= 0
        AND StockNuevo = FLOOR(StockNuevo)
    );
ALTER TABLE dbo.MovimientosInventario
    CHECK CONSTRAINT CK_Movimientos_StockNuevo;


ALTER TABLE [dbo].[Productos]  WITH CHECK ADD  CONSTRAINT [CK_Productos_PrecioCompra] CHECK  (([PrecioCompra]>=(0)))

ALTER TABLE [dbo].[Productos] CHECK CONSTRAINT [CK_Productos_PrecioCompra]


ALTER TABLE [dbo].[Productos]  WITH CHECK ADD  CONSTRAINT [CK_Productos_PrecioVenta] CHECK  (([PrecioVenta]>=(0)))

ALTER TABLE [dbo].[Productos] CHECK CONSTRAINT [CK_Productos_PrecioVenta]


ALTER TABLE dbo.Productos
    WITH CHECK ADD CONSTRAINT CK_Productos_StockActual
    CHECK (
        StockActual >= 0
        AND StockActual = FLOOR(StockActual)
    );
ALTER TABLE dbo.Productos
    CHECK CONSTRAINT CK_Productos_StockActual;


ALTER TABLE dbo.Productos
    WITH CHECK ADD CONSTRAINT CK_Productos_StockMinimo
    CHECK (
        StockMinimo >= 0
        AND StockMinimo = FLOOR(StockMinimo)
    );
ALTER TABLE dbo.Productos
    CHECK CONSTRAINT CK_Productos_StockMinimo;


ALTER TABLE [dbo].[TiposMovimiento]  WITH CHECK ADD  CONSTRAINT [CK_TiposMovimiento_Naturaleza] CHECK  (([Naturaleza]='SALIDA' OR [Naturaleza]='ENTRADA'))

ALTER TABLE [dbo].[TiposMovimiento] CHECK CONSTRAINT [CK_TiposMovimiento_Naturaleza]


ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [CK_Ventas_Impuesto] CHECK  (([Impuesto]>=(0)))

ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [CK_Ventas_Impuesto]


ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [CK_Ventas_PorcentajeImpuesto] CHECK  (([PorcentajeImpuesto]>=(0) AND [PorcentajeImpuesto]<=(100)))

ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [CK_Ventas_PorcentajeImpuesto]


ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [CK_Ventas_Subtotal] CHECK  (([Subtotal]>=(0)))

ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [CK_Ventas_Subtotal]


ALTER TABLE [dbo].[Ventas]  WITH CHECK ADD  CONSTRAINT [CK_Ventas_Total] CHECK  (([Total]>=(0)))

ALTER TABLE [dbo].[Ventas] CHECK CONSTRAINT [CK_Ventas_Total]



/* =========================================================
   5. COMPROBACIÓN
========================================================= */

SELECT
    (SELECT COUNT(*)
       FROM sys.indexes
      WHERE object_id IN (
            SELECT object_id
              FROM sys.tables
             WHERE schema_id = SCHEMA_ID('dbo')
      )
        AND name IS NOT NULL
        AND is_primary_key = 0
        AND is_unique_constraint = 0) AS IndicesAplicacion,

    (SELECT COUNT(*)
       FROM sys.default_constraints
      WHERE parent_object_id IN (
            SELECT object_id
              FROM sys.tables
             WHERE schema_id = SCHEMA_ID('dbo')
      )) AS Defaults,

    (SELECT COUNT(*)
       FROM sys.foreign_keys
      WHERE parent_object_id IN (
            SELECT object_id
              FROM sys.tables
             WHERE schema_id = SCHEMA_ID('dbo')
      )) AS ForeignKeys,

    (SELECT COUNT(*)
       FROM sys.check_constraints
      WHERE parent_object_id IN (
            SELECT object_id
              FROM sys.tables
             WHERE schema_id = SCHEMA_ID('dbo')
      )) AS Checks;
