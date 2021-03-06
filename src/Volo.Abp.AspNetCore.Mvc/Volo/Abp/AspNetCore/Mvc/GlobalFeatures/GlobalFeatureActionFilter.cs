using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.GlobalFeatures;
using Volo.Abp.Reflection;

namespace Volo.Abp.AspNetCore.Mvc.GlobalFeatures
{
    public class GlobalFeatureActionFilter : IAsyncActionFilter, ITransientDependency
    {
        public ILogger<GlobalFeatureActionFilter> Logger { get; set; }

        public GlobalFeatureActionFilter()
        {
            Logger = NullLogger<GlobalFeatureActionFilter>.Instance;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionDescriptor.IsControllerAction())
            {
                await next();
                return;
            }

            if (!IsGlobalFeatureEnabled(context.Controller.GetType(), out var attribute))
            {
                Logger.LogWarning($"The '{context.Controller.GetType().FullName}' controller needs to enable '{attribute.Name}' feature.");
                context.Result = new NotFoundResult();
                return;
            }

            await next();
        }

        protected virtual bool IsGlobalFeatureEnabled(Type controllerType, out RequiresGlobalFeatureAttribute attribute)
        {
            attribute = ReflectionHelper.GetSingleAttributeOrDefault<RequiresGlobalFeatureAttribute>(controllerType);
            return attribute == null || GlobalFeatureManager.Instance.IsEnabled(attribute.GetFeatureName());
        }
    }
}
