using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using flea_WebProj.Helpers;

namespace flea_WebProj.Middleware;

public class RequireAuthAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Session.IsAuthenticated())
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult(
                "Login", 
                "Account", 
                new { returnUrl = returnUrl }
            );
        }
        base.OnActionExecuting(context);
    }
}

public class RequireAdminAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Session.IsAuthenticated())
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }
        if (!context.HttpContext.Session.IsAdmin())
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);

        base.OnActionExecuting(context);
    }
}

public class RequireModeratorAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Session.IsAuthenticated())
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        if (!context.HttpContext.Session.IsAdmin() && !context.HttpContext.Session.IsModerator())
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);

        base.OnActionExecuting(context);
    }
}