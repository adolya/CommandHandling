using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Linq;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public static class ReflectionExtensions
    {
        public static void ConfigureHttpMethod(this TypeBuilder typeBuilder, Type baseType, CustomAttributeBuilder httpMethodAttributeBuilder, Type request, Type? response)
        {
            MethodBuilder myMethodBuilder = typeBuilder.DefineMethod("Go", MethodAttributes.Public);
            myMethodBuilder.SetCustomAttribute(httpMethodAttributeBuilder);
            myMethodBuilder.SetParameters(request);
            myMethodBuilder.SetReturnType(response);
            var ilGenerator = myMethodBuilder.GetILGenerator();
            ilGenerator.Emit(OpCodes.Nop);              
            ilGenerator.Emit(OpCodes.Ldarg_0);                          
            ilGenerator.Emit(OpCodes.Ldarg_1);                
            
            var byteFlagLabel = ilGenerator.DefineLabel();
            var baseMethod = baseType.GetMethod("Process", BindingFlags.Instance | BindingFlags.NonPublic);;
            ilGenerator.Emit(OpCodes.Call, baseMethod);
            ilGenerator.Emit(OpCodes.Stloc_0);  
            ilGenerator.Emit(OpCodes.Br_S, byteFlagLabel);  
            ilGenerator.Emit(OpCodes.Ldloc_0);  
            ilGenerator.MarkLabel(byteFlagLabel);
            ilGenerator.Emit(OpCodes.Ret);  
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
    }
}