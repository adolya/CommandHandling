using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class BaseControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly Assembly _handlersAssembly;
        public BaseControllerFeatureProvider(Assembly handlersAssembly)
        {
            _handlersAssembly = handlersAssembly;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var handlers = _handlersAssembly.GetTypes();
            foreach (var candidate in handlers)
            {
                feature.Controllers.Add(candidate.GetTypeInfo());
            }
        }

        // private TypeInfo CreateController(KeyValuePair<RouteInfo, Type> kvp)
        // {   
        //     Type controllerType = typeof(BaseController<,,>); 

        //     return controllerType.MakeGenericType(kvp.Value.GetGenericArguments(), kvp.Key);
        // }
    }
}
