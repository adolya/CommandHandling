using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Linq;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public static class TypeExtensions
    {
        public static TypeInfo GetGenericController(this Type type, Type[] args)
        {
            return type.MakeGenericType(args).GetTypeInfo();
        }

        public static TypeInfo GetGenericController(this Type baseType, Type[] args, RouteInfo routeInfo)
        {   
            var typeBuilder = GetTypeBuilder(baseType);
            SetupConstructor(typeBuilder, baseType);
            var httpMethodAttributeBuilder = GetHttpMethodAttribute(routeInfo.Method, routeInfo.Path);
            typeBuilder.ConfigureHttpMethod(baseType, httpMethodAttributeBuilder, args[2], args[1]);
            var controllerType = typeBuilder.CreateType();
            return controllerType.GetTypeInfo();
        }

        private static CustomAttributeBuilder GetHttpMethodAttribute(HttpMethod method, string path)
        {
            Type[] ctorParams = new Type[] { typeof(string) };
            ConstructorInfo classCtorInfo = GetHttpAttribute(method).GetConstructor(ctorParams);

            return new CustomAttributeBuilder(
                                classCtorInfo,
                                new object[] { path });
        }

        private static void SetupConstructor(TypeBuilder typeBuilder, Type baseType)
        {
            var baseConstructor = baseType.GetConstructors().First();

            // Create a parameterless (default) constructor.
            var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null); 
            var ilGenerator = constructor.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);                // push "this"
            ilGenerator.Emit(OpCodes.Ldarg_1);                // push the 1. parameter
            ilGenerator.Emit(OpCodes.Call, baseConstructor);
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static TypeBuilder GetTypeBuilder(Type baseType)
        {
            AssemblyName myAsmName = new AssemblyName() { Name = "MyAssembly" };
            AssemblyBuilder myAsmBuilder = AssemblyBuilder.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.Run);
            ModuleBuilder myModBuilder = myAsmBuilder.DefineDynamicModule("MyModule");
            TypeBuilder myTypeBuilder = myModBuilder.DefineType("MyType", TypeAttributes.Public);
            myTypeBuilder.SetParent(baseType);
            return myTypeBuilder;
        }

        private static Type GetHttpAttribute(HttpMethod method)
        {
            if (method == HttpMethod.Delete)
                return typeof(HttpDeleteAttribute);
            
            if (method == HttpMethod.Get)
                return typeof(HttpGetAttribute);

            if (method == HttpMethod.Head)
                return typeof(HttpHeadAttribute);

            if (method == HttpMethod.Options)
                return typeof(HttpOptionsAttribute);

            if (method == HttpMethod.Patch)
                return typeof(HttpPatchAttribute);

            if (method == HttpMethod.Put)
                return typeof(HttpPutAttribute);
                
            if (method == HttpMethod.Trace)
                return typeof(HttpHeadAttribute);

            return typeof(HttpPostAttribute);
        }
    }
}