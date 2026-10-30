/*
=========================================================
StockFlow - Fase L.2 v2
Pulido visual robusto
=========================================================
- Detecta automáticamente cards de Reportes.
- Convierte alertas generales en Toasts.
- No toca validaciones dentro de formularios.
- Usa textContent: no inyecta HTML.
=========================================================
*/

(function () {
    "use strict";

    document.addEventListener(
        "DOMContentLoaded",
        function () {

            document.documentElement.classList.add(
                "sf-polish-ready"
            );

            marcarTarjetasReportes();
            convertirAlertasGeneralesEnToasts();

        }
    );


    // =====================================================
    // REPORTES
    // =====================================================

    function marcarTarjetasReportes() {

        const path =
            window.location.pathname
                .toLowerCase();


        const esIndiceReportes =
            path === "/reporte"
            ||
            path === "/reporte/"
            ||
            document.title
                .toLowerCase()
                .includes("reportes");


        if (!esIndiceReportes) {
            return;
        }


        const tarjetas =
            document.querySelectorAll(
                ".content-card"
            );


        tarjetas.forEach(
            function (tarjeta) {

                const texto =
                    normalizarTexto(
                        tarjeta.textContent
                    )
                    .toLowerCase();


                const tieneEnlaceReporte =
                    Array.from(
                        tarjeta.querySelectorAll("a")
                    )
                    .some(
                        function (enlace) {

                            return normalizarTexto(
                                enlace.textContent
                            )
                                .toLowerCase()
                                .includes("ver reporte");

                        }
                    );


                if (
                    tieneEnlaceReporte
                    ||
                    texto.includes("reporte de ventas")
                    ||
                    texto.includes("reporte de compras")
                    ||
                    texto.includes("inventario actual")
                    ||
                    texto.includes("stock bajo")
                    ||
                    texto.includes("productos más vendidos")
                ) {
                    tarjeta.classList.add(
                        "sf-report-card"
                    );
                }

            }
        );
    }


    // =====================================================
    // TOASTS
    // =====================================================

    function convertirAlertasGeneralesEnToasts() {

        const alertas =
            document.querySelectorAll(
                ".alert-success, .alert-danger, .alert-warning, .alert-info"
            );


        if (!alertas.length) {
            return;
        }


        let stack = null;


        alertas.forEach(
            function (alerta) {

                if (!debeSerToast(alerta)) {
                    return;
                }


                const mensaje =
                    normalizarTexto(
                        alerta.textContent
                    );


                if (!mensaje) {
                    return;
                }


                if (!stack) {
                    stack =
                        obtenerOCrearStack();
                }


                const tipo =
                    obtenerTipo(
                        alerta
                    );


                const toast =
                    crearToast(
                        tipo,
                        mensaje
                    );


                stack.appendChild(
                    toast
                );


                alerta.remove();


                mostrarToast(
                    toast,
                    tipo
                );

            }
        );
    }


    function debeSerToast(
        alerta) {

        if (
            alerta.classList.contains(
                "validation-summary-errors"
            )
        ) {
            return false;
        }


        if (
            alerta.hasAttribute(
                "data-no-toast"
            )
        ) {
            return false;
        }


        if (
            alerta.closest("form")
        ) {
            return false;
        }


        const texto =
            normalizarTexto(
                alerta.textContent
            );


        if (!texto) {
            return false;
        }


        return true;
    }


    function obtenerTipo(
        alerta) {

        if (
            alerta.classList.contains(
                "alert-success"
            )
        ) {
            return "success";
        }


        if (
            alerta.classList.contains(
                "alert-danger"
            )
        ) {
            return "danger";
        }


        if (
            alerta.classList.contains(
                "alert-warning"
            )
        ) {
            return "warning";
        }


        return "info";
    }


    function obtenerOCrearStack() {

        let stack =
            document.getElementById(
                "stockFlowToastStack"
            );


        if (stack) {
            return stack;
        }


        stack =
            document.createElement(
                "div"
            );


        stack.id =
            "stockFlowToastStack";


        stack.className =
            "sf-toast-stack";


        stack.setAttribute(
            "aria-live",
            "polite"
        );


        document.body.appendChild(
            stack
        );


        return stack;
    }


    function crearToast(
        tipo,
        mensaje) {

        const config =
            obtenerConfigToast(
                tipo
            );


        const toast =
            document.createElement(
                "div"
            );


        toast.className =
            `sf-toast sf-toast--${tipo}`;


        toast.setAttribute(
            "role",
            tipo === "danger"
                ? "alert"
                : "status"
        );


        const cajaIcono =
            document.createElement(
                "div"
            );


        cajaIcono.className =
            "sf-toast-icon";


        const icono =
            document.createElement(
                "i"
            );


        icono.className =
            config.icono;


        cajaIcono.appendChild(
            icono
        );


        const contenido =
            document.createElement(
                "div"
            );


        contenido.className =
            "sf-toast-content";


        const titulo =
            document.createElement(
                "p"
            );


        titulo.className =
            "sf-toast-title";


        titulo.textContent =
            config.titulo;


        const texto =
            document.createElement(
                "p"
            );


        texto.className =
            "sf-toast-message";


        texto.textContent =
            mensaje;


        contenido.appendChild(
            titulo
        );


        contenido.appendChild(
            texto
        );


        const cerrar =
            document.createElement(
                "button"
            );


        cerrar.type =
            "button";


        cerrar.className =
            "sf-toast-close";


        cerrar.setAttribute(
            "aria-label",
            "Cerrar notificación"
        );


        const cerrarIcono =
            document.createElement(
                "i"
            );


        cerrarIcono.className =
            "bi bi-x-lg";


        cerrar.appendChild(
            cerrarIcono
        );


        cerrar.addEventListener(
            "click",
            function () {

                ocultarToast(
                    toast
                );

            }
        );


        toast.appendChild(
            cajaIcono
        );


        toast.appendChild(
            contenido
        );


        toast.appendChild(
            cerrar
        );


        return toast;
    }


    function mostrarToast(
        toast,
        tipo) {

        const config =
            obtenerConfigToast(
                tipo
            );


        let timeoutId = null;


        requestAnimationFrame(
            function () {

                requestAnimationFrame(
                    function () {

                        toast.classList.add(
                            "sf-toast-show"
                        );

                    }
                );

            }
        );


        const iniciar =
            function () {

                window.clearTimeout(
                    timeoutId
                );


                timeoutId =
                    window.setTimeout(
                        function () {

                            ocultarToast(
                                toast
                            );

                        },
                        config.duracion
                    );

            };


        toast.addEventListener(
            "mouseenter",
            function () {

                window.clearTimeout(
                    timeoutId
                );

            }
        );


        toast.addEventListener(
            "mouseleave",
            iniciar
        );


        iniciar();
    }


    function ocultarToast(
        toast) {

        if (
            !toast
            ||
            toast.dataset.closing === "true"
        ) {
            return;
        }


        toast.dataset.closing =
            "true";


        toast.classList.remove(
            "sf-toast-show"
        );


        toast.classList.add(
            "sf-toast-hide"
        );


        window.setTimeout(
            function () {

                toast.remove();


                const stack =
                    document.getElementById(
                        "stockFlowToastStack"
                    );


                if (
                    stack
                    &&
                    stack.children.length === 0
                ) {
                    stack.remove();
                }

            },
            230
        );
    }


    function obtenerConfigToast(
        tipo) {

        switch (tipo) {

            case "success":
                return {
                    titulo:
                        "Operación completada",

                    icono:
                        "bi bi-check-circle-fill",

                    duracion:
                        4800
                };


            case "danger":
                return {
                    titulo:
                        "No se pudo completar",

                    icono:
                        "bi bi-exclamation-circle-fill",

                    duracion:
                        7200
                };


            case "warning":
                return {
                    titulo:
                        "Atención",

                    icono:
                        "bi bi-exclamation-triangle-fill",

                    duracion:
                        6200
                };


            default:
                return {
                    titulo:
                        "Información",

                    icono:
                        "bi bi-info-circle-fill",

                    duracion:
                        4800
                };

        }
    }


    function normalizarTexto(
        valor) {

        return String(
            valor ?? ""
        )
            .replace(/\s+/g, " ")
            .trim();
    }

})();
