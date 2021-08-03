using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Net.Http;
using CommandHandling.Mvc.DependencyInjection.Extensions;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class BaseControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly IDictionary<RouteInfo, Type> _commandHandlerTypes;
        public BaseControllerFeatureProvider(IDictionary<RouteInfo, Type> commandHandlerTypes)
        {
            _commandHandlerTypes = commandHandlerTypes;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var handlers = _commandHandlerTypes.Select(_ => CreateController(_)).Distinct();
            foreach (var candidate in handlers)
            {
                feature.Controllers.Add(candidate);
            }
        }

        private TypeInfo CreateController(KeyValuePair<RouteInfo, Type> kvp)
        {   
            Type controllerType = typeof(BaseController<,,>); 

            return controllerType.GetGenericController(kvp.Value.GetGenericArguments(), kvp.Key);
        }
    }
}
