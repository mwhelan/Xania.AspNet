using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Xania.AspNet.Simulator
{
    internal class SimpleActionInvoker : ControllerActionInvoker
    {
        private readonly ControllerContext _controllerContext;
        private readonly ActionDescriptor _actionDescriptor;
        private readonly FilterInfo _filterInfo;

        public SimpleActionInvoker(ControllerContext controllerContext, ActionDescriptor actionDescriptor, IEnumerable<Filter> filters)
        {
            _controllerContext = controllerContext;
            _actionDescriptor = actionDescriptor;
            _filterInfo = new FilterInfo(filters);
        }

        public virtual ActionResult AuthorizeAction()
        {
            var authorizationContext = InvokeAuthorizationFilters(_controllerContext,
                _filterInfo.AuthorizationFilters, _actionDescriptor);

            return authorizationContext.Result;
        }

        public virtual ActionResult InvokeAction()
        {
            return AuthorizeAction() ?? InvokeActionMethodWithFilters();
        }

        private ActionResult InvokeActionMethodWithFilters()
        {
            var controller = _controllerContext.Controller as Controller;
            if (controller != null)
            {
                controller.ViewEngineCollection = new ViewEngineCollection();
            }

            var parameters = GetParameterValues(_controllerContext, _actionDescriptor);
            var actionExecutedContext = InvokeActionMethodWithFilters(_controllerContext,
                _filterInfo.ActionFilters, _actionDescriptor, parameters);

            if (actionExecutedContext == null)
                throw new Exception("InvokeActionMethodWithFilters returned null");

            return actionExecutedContext.Result;
        }
    }
}