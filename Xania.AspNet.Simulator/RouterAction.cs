using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Xania.AspNet.Simulator
{
    public class RouterAction : ControllerAction
    {
        private readonly ControllerContainer _controllerContainer;

        public RouterAction(ControllerContainer controllerContainer)
        {
            _controllerContainer = controllerContainer;
        }

        public override ControllerActionResult Execute(HttpContextBase httpContext)
        {
            var actionContext = GetActionContext(httpContext);
            var actionDescriptor = actionContext.ActionDescriptor;

            if (actionDescriptor == null)
                return null;

            return Execute(actionContext.ControllerContext, actionDescriptor);
        }

        public override ActionContext GetActionContext(HttpContextBase httpContext1)
        {
            var context = httpContext1 ?? AspNetUtility.GetContext(this);
            var routeData = Routes.GetRouteData(context);

            if (routeData == null)
                return null;

            var controllerName = routeData.GetRequiredString("controller");
            var controller = _controllerContainer.CreateController(controllerName);
            var controllerDescriptor = new ReflectedControllerDescriptor(controller.GetType());

            var actionName = routeData.GetRequiredString("action");
            var httpContext = httpContext1 ?? AspNetUtility.GetContext(String.Format("/{0}/{1}", controllerName, actionName), HttpMethod, User ?? AspNetUtility.CreateAnonymousUser());

            var requestContext = new RequestContext(httpContext, routeData);
            var controllerContext = new ControllerContext(requestContext, controller);

            Initialize(controllerContext);
            return new ActionContext
            {
                ControllerContext = controllerContext,
                ActionDescriptor = controllerDescriptor.FindAction(controller.ControllerContext, actionName)
            };
        }

        public override HttpContextBase CreateHttpContext()
        {
            return AspNetUtility.GetContext(this);
        }
    }
}