using System.Collections.Generic;
using System.Reflection;
using CommandHandling.CommandHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CommandHandling.Mvc.DependencyInjection.Convensions
{
    public class BaseControllerRouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerType.IsGenericType)
            {
                var genericType = controller.ControllerType.GenericTypeArguments[0];
                var route = genericType.Name.ToLower(); 
                controller.Selectors.Add(new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel(new RouteAttribute(route)),
                });
            }
        }
    }
}
