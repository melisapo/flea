using flea_WebProj.Helpers;

namespace flea_WebProj.Middleware;

public class AuthenticationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
            
        // Rutas públicas
        var publicPaths = new[]
        {
            "/",
            "/home",
            "/home/index",
            "/home/about",
            "/home/contact",
            "/account/login",
            "/account/register",
            "/account/accessdenied"
        };

        // Si es ruta pública, continuar
        if (publicPaths.Any(p => path.StartsWith(p)))
        {
            await next(context);
            return;
        }
        
        if (!context.Session.IsAuthenticated())
        {
            // Guardar URL a la que intentaba acceder
            var returnUrl = context.Request.Path + context.Request.QueryString;
            context.Response.Redirect($"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            return;
        }

        // Usuario autenticado, continuar
        await next(context);
    }
}