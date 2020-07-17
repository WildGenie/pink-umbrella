using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PinkUmbrella.Models;

namespace PinkUmbrella.Util
{
    public class RedirectIfNotAnonymous : ActionFilterAttribute
    {
        public RedirectIfNotAnonymous(string action, string controller)
        {
            Action = action;
            Controller = controller;
        }

        public RedirectIfNotAnonymous(): this("Index", "Home") {}

        public string Action { get; set; }

        public string Controller { get; set; }
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Result != null)
            {
                return;
            }
            
            var signin = context.HttpContext.RequestServices.GetRequiredService<SignInManager<UserProfileModel>>();
            if (!signin.IsSignedIn(context.HttpContext.User))
            {
                base.OnActionExecuting(context);
            }
            else
            {
                var controller = Controller;
                var action = Action;
                var urlHelper = new UrlHelper(context);
                context.Result = new RedirectToRouteResult(new RouteValueDictionary( new { area = string.Empty, controller, action }));
            }
        }
    }
}