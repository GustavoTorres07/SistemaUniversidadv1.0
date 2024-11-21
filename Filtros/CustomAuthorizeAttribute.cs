using SistemaUniversidadv1._0.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SistemaUniversidadv1._0.Filtros
{
    // Define un atributo de autorización personalizado que hereda de AuthorizeAttribute
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        // Array de roles permitidos para el acceso
        private readonly string[] rolesPermitidos;

        // Constructor que recibe los roles permitidos como parámetros
        public CustomAuthorizeAttribute(params string[] roles)
        {
            // Inicializa el array de rolesPermitidos con los roles proporcionados
            this.rolesPermitidos = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Variable para determinar si el usuario está autorizado
            bool autorizado = false;

            // Obtiene el nombre de usuario del contexto HTTP (se asume que es el identificador del usuario)
            var id_usuario = httpContext.User.Identity.Name;

            // Verifica si el nombre de usuario no es nulo o vacío
            if (!string.IsNullOrEmpty(id_usuario))
            {
                using (var db = new UniversidadContext())
                {
                    // Busca al usuario en la base de datos usando el nombre de usuario
                    var usuario = db.USUARIO.FirstOrDefault(u => u.usuario_usuario == id_usuario);

                    // Verifica si el usuario fue encontrado y tiene un rol válido (mayor que 0)
                    if (usuario != null && usuario.ROL != null)
                    {
                        // Verifica si el rol del usuario está en la lista de roles permitidos
                        if (this.rolesPermitidos.Contains(usuario.ROL.nombre_rol))
                        {
                            autorizado = true; // Si el rol está permitido, autoriza el acceso
                        }
                    }
                }
            }

            // Devuelve el resultado de la autorización
            return autorizado; // Siempre debe devolver un valor booleano
        }



        // Método que maneja las solicitudes no autorizadas
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Redirige al usuario a la página de acceso denegado si no está autorizado
            filterContext.Result = new RedirectResult("~/Acceso/Login");
        }
    }
}