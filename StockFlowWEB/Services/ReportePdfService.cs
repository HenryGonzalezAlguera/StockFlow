using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace StockFlowWEB.Services
{
    public class ReportePdfService
    {
        public byte[] GenerarReporte(
            string empresa,
            string titulo,
            string? periodo,
            string[] encabezados,
            IEnumerable<string[]> filas,
            string? resumen = null)
        {
            var datos =
                filas.ToList();


            // =====================================================
            // IMPORTANTE
            //
            // Usamos el namespace completo para evitar:
            //
            // QuestPDF.Fluent.Document
            // VS
            // System.Reflection.Metadata.Document
            // =====================================================

            var documento =
                QuestPDF.Fluent.Document.Create(
                    container =>
                    {
                        container.Page(
                            page =>
                            {
                                // =================================
                                // PÁGINA
                                // =================================

                                page.Size(
                                    PageSizes.A4.Landscape()
                                );


                                page.Margin(25);


                                page.PageColor(
                                    Colors.White
                                );


                                page.DefaultTextStyle(
                                    x =>
                                        x.FontSize(9)
                                );


                                // =================================
                                // ENCABEZADO
                                // =================================

                                page.Header()
                                    .Column(
                                        column =>
                                        {
                                            column.Item()
                                                .Text(
                                                    "StockFlow"
                                                )
                                                .Bold()
                                                .FontSize(20);


                                            column.Item()
                                                .Text(
                                                    empresa
                                                )
                                                .FontSize(11);


                                            column.Item()
                                                .PaddingTop(6)
                                                .Text(
                                                    titulo
                                                )
                                                .Bold()
                                                .FontSize(15);


                                            if (
                                                !string.IsNullOrWhiteSpace(
                                                    periodo
                                                )
                                            )
                                            {
                                                column.Item()
                                                    .Text(
                                                        periodo
                                                    )
                                                    .FontSize(9);
                                            }


                                            column.Item()
                                                .PaddingTop(2)
                                                .Text(
                                                    $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}"
                                                )
                                                .FontSize(8)
                                                .FontColor(
                                                    Colors
                                                        .Grey
                                                        .Darken1
                                                );
                                        }
                                    );


                                // =================================
                                // CONTENIDO
                                // =================================

                                page.Content()
                                    .PaddingVertical(15)
                                    .Column(
                                        column =>
                                        {
                                            column.Spacing(8);


                                            column.Item()
                                                .Text(
                                                    $"Registros: {datos.Count}"
                                                )
                                                .SemiBold();


                                            if (
                                                !string.IsNullOrWhiteSpace(
                                                    resumen
                                                )
                                            )
                                            {
                                                column.Item()
                                                    .Text(
                                                        resumen
                                                    )
                                                    .SemiBold();
                                            }


                                            column.Item()
                                                .Table(
                                                    table =>
                                                    {
                                                        // =================
                                                        // COLUMNAS
                                                        // =================

                                                        table.ColumnsDefinition(
                                                            columns =>
                                                            {
                                                                foreach (
                                                                    var _
                                                                    in encabezados
                                                                )
                                                                {
                                                                    columns
                                                                        .RelativeColumn();
                                                                }
                                                            }
                                                        );


                                                        // =================
                                                        // ENCABEZADOS
                                                        // =================

                                                        table.Header(
                                                            header =>
                                                            {
                                                                foreach (
                                                                    var encabezado
                                                                    in encabezados
                                                                )
                                                                {
                                                                    header.Cell()
                                                                        .Background(
                                                                            Colors
                                                                                .Grey
                                                                                .Lighten2
                                                                        )
                                                                        .Padding(5)
                                                                        .Text(
                                                                            encabezado
                                                                        )
                                                                        .SemiBold();
                                                                }
                                                            }
                                                        );


                                                        // =================
                                                        // SIN REGISTROS
                                                        // =================

                                                        if (
                                                            datos.Count == 0
                                                        )
                                                        {
                                                            table.Cell()
                                                                .ColumnSpan(
                                                                    (uint)
                                                                    encabezados.Length
                                                                )
                                                                .Padding(10)
                                                                .AlignCenter()
                                                                .Text(
                                                                    "No existen registros para mostrar."
                                                                );
                                                        }
                                                        else
                                                        {
                                                            // =================
                                                            // REGISTROS
                                                            // =================

                                                            foreach (
                                                                var fila
                                                                in datos
                                                            )
                                                            {
                                                                foreach (
                                                                    var celda
                                                                    in fila
                                                                )
                                                                {
                                                                    table.Cell()
                                                                        .BorderBottom(1)
                                                                        .BorderColor(
                                                                            Colors
                                                                                .Grey
                                                                                .Lighten3
                                                                        )
                                                                        .Padding(5)
                                                                        .Text(
                                                                            celda
                                                                            ??
                                                                            string.Empty
                                                                        );
                                                                }
                                                            }
                                                        }
                                                    }
                                                );
                                        }
                                    );


                                // =================================
                                // PIE
                                // =================================

                                page.Footer()
                                    .AlignCenter()
                                    .Text(
                                        text =>
                                        {
                                            text.Span(
                                                "Página "
                                            );


                                            text.CurrentPageNumber();


                                            text.Span(
                                                " de "
                                            );


                                            text.TotalPages();
                                        }
                                    );
                            }
                        );
                    }
                );


            return documento
                .GeneratePdf();
        }
    }
}