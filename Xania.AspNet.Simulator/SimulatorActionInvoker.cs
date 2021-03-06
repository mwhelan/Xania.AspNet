using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Xania.AspNet.Core;

namespace Xania.AspNet.Simulator
{
    internal class SimulatorActionInvoker : ControllerActionInvoker
    {
        private readonly IMvcApplication _mvcApplication;
        private readonly ActionExecutionContext _actionExecutionContext;
        private readonly FilterInfo _filterInfo;
        private IDictionary<string, object> _parameters;

        public SimulatorActionInvoker(IMvcApplication mvcApplication, ActionExecutionContext actionExecutionContext)
        {
            _filterInfo = mvcApplication.GetFilterInfo(actionExecutionContext.ControllerContext,
                actionExecutionContext.ActionDescriptor);

            _mvcApplication = mvcApplication;
            _actionExecutionContext = actionExecutionContext;
        }

        public virtual ActionResult GetAuthorizationResult()
        {
            var authorizationContext = InvokeAuthorizationFilters(_actionExecutionContext.ControllerContext,
                _filterInfo.AuthorizationFilters, _actionExecutionContext.ActionDescriptor);

            return authorizationContext.Result;
        }

        protected override AuthorizationContext InvokeAuthorizationFilters(ControllerContext controllerContext, IList<IAuthorizationFilter> filters,
            ActionDescriptor actionDescriptor)
        {
            var context = new AuthorizationContext(controllerContext, actionDescriptor);
            foreach (var filter in filters)
            {
                filter.OnAuthorization(context);
                // short-circuit evaluation when an error occurs
                if (context.Result == null) 
                    continue;

                SimulatorHelper.InitizializeActionResults(context.Result, _mvcApplication.Routes);
                break;
            }

            return context;
        }

        public virtual ActionResult GetActionResult()
        {
            var controllerContext = _actionExecutionContext.ControllerContext;
            var actionDescriptor = _actionExecutionContext.ActionDescriptor;

            var parameters = _parameters ?? GetParameterValues(controllerContext, actionDescriptor);
            var actionExecutedContext = InvokeActionMethodWithFilters(controllerContext, _filterInfo.ActionFilters,
                actionDescriptor, parameters);

            if (actionExecutedContext == null)
                throw new Exception("GetActionResult returned null");

            SimulatorHelper.InitizializeActionResults(actionExecutedContext.Result, _mvcApplication.Routes);
            return actionExecutedContext.Result;
        }

        public virtual ModelStateDictionary ValidateRequest()
        {
            var controllerContext = _actionExecutionContext.ControllerContext;
            var actionDescriptor = _actionExecutionContext.ActionDescriptor;

            _parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var modelState = controllerContext.Controller.ViewData.ModelState;
            foreach (ParameterDescriptor parameterDescriptor in actionDescriptor.GetParameters())
            {
                var parameterName = parameterDescriptor.ParameterName;
                var parameterType = parameterDescriptor.ParameterType;
                var value = GetParameterValue(controllerContext, parameterDescriptor);

                _parameters[parameterName] = value;

                if (value == null)
                    continue;

                var validationResults = _mvcApplication.ValidateModel(parameterType, value, controllerContext);

                Func<ModelValidationResult, bool> isValidField =
                    res => modelState.IsValidField(String.Format("{0}.{1}", parameterName, res.MemberName));

                foreach (var validationResult in validationResults.Where(isValidField).ToArray())
                {
                    var subPropertyName = String.Format("{0}.{1}", parameterName, validationResult.MemberName);
                    modelState.AddModelError(subPropertyName, validationResult.Message);
                }
            }
            return modelState;
        }
    }
}