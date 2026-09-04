using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace StockFlowWEB.Attributes
{
    public class SessionAuthorizeAttribute
        : ActionFilterAttribute
    {
        private readonly HashSet<string>
            _rolesPermitidos;


        // =========================================================
        // CONSTRUCTOR
        //
        // [SessionAuthorize]
        //      -> cualquier usuario autenticado
        //
        // [SessionAuthorize("Administrador")]
        //      -> solo administrador
        //
        // [SessionAuthorize("Administrador", "Empleado")]
        //      -> administrador o empleado
        // =========================================================

        public SessionAuthorizeAttribute(
            params string[] roles)
        {
            _rolesPermitidos =
                new HashSet<string>(
                    roles
                    ??
                    Array.Empty<string>(),
                    StringComparer.OrdinalIgnoreCase
                );
        }


        // =========================================================
        // VALIDAR SESION Y ROL
        // =========================================================

        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            var session =
                context.HttpContext.Session;


            var autenticado =
                session.GetString(
                    "Autenticado"
                );


            var token =
                session.GetString(
                    "Token"
                );


            // =====================================================
            // 1. VALIDAR SESION
            // =====================================================

            if (autenticado != "true" ||
                string.IsNullOrWhiteSpace(
                    token
                ))
            {
                session.Clear();


                context.Result =
                    new RedirectToActionResult(
                        "Index",
                        "Home",
                        null
                    );


                return;
            }


            // =====================================================
            // 2. SIN ROLES ESPECIFICOS:
            //    SOLO REQUIERE AUTENTICACION
            // =====================================================

            if (_rolesPermitidos.Count == 0)
            {
                base.OnActionExecuting(
                    context
                );


                return;
            }


            // =====================================================
            // 3. OBTENER ROL DE LA SESION
            // =====================================================

            var rol =
                session.GetString(
                    "Rol"
                );


            // =====================================================
            // 4. VALIDAR ROL
            // =====================================================

            if (string.IsNullOrWhiteSpace(
                    rol
                )
                ||
                !_rolesPermitidos.Contains(
                    rol
                ))
            {
                context.Result =
                    new RedirectToActionResult(
                        "Estado",
                        "Home",
                        new
                        {
                            codigo =
                                StatusCodes
                                    .Status403Forbidden
                        }
                    );


                return;
            }


            base.OnActionExecuting(
                context
            );
        }
    }
}

