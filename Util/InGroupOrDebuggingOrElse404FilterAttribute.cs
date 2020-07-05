using System.Diagnostics;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace PinkUmbrella.Util
{
    public class InGroupOrDebuggingOrElse404FilterAttribute : ActionFilterAttribute
    {
        public InGroupOrDebuggingOrElse404FilterAttribute(string group)
        {
            Group = group;
        }

        public string Group { get; set; }
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Result != null)
            {
                return;
            }

            if (Debugger.IsAttached || (context.HttpContext.User != null && context.HttpContext.User.IsInRole(Group)))
            {
                base.OnActionExecuting(context);
            }
            else
            {
                var controller = "Home";
                var action = "Error";
                var code = "404";
                var urlHelper = new UrlHelper(context);
                context.Result = new RedirectToRouteResult(new RouteValueDictionary( new { area = string.Empty, controller, action, code }));
            }
        }
    }

    public class IsDevOrDebuggingOrElse404FilterAttribute : InGroupOrDebuggingOrElse404FilterAttribute
    {
        public IsDevOrDebuggingOrElse404FilterAttribute() : base("dev")
        {
        }
    }

    public class IsAdminOrDebuggingOrElse404FilterAttribute : InGroupOrDebuggingOrElse404FilterAttribute
    {
        public IsAdminOrDebuggingOrElse404FilterAttribute() : base("admin")
        {
        }
    }
}