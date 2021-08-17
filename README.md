Command Handling
============
[![NuGet](https://img.shields.io/nuget/v/CommandHandling.AspNetCore.DependencyInjection.svg)](https://www.nuget.org/packages/CommandHandling.AspNetCore.DependencyInjection/)


CommandHandling is set of an [IServiceCollection](https://github.com/aspnet/DependencyInjection/blob/master/src/DI.Abstractions/IServiceCollection.cs) extensions for [AspNet](https://github.com/dotnet/aspnetcore) framework. It allows you to focus on your business logic, and safe time for generating controller wrappers. 

## Get Started

Having:

```csharp
public class MathCommand
{
    public string SquareRoot(int size)
    {
        return $"{Math.Sqrt(size)}";
    }
}
```

Simply do:
* Install package [CommandHandling.AspNetCore.DependencyInjection](https://www.nuget.org/packages/CommandHandling.AspNetCore.DependencyInjection/) 
* Register handlers:
```csharp
public class YourStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.RegisterHandler<MathCommand, int, string>((math, size) => math.SquareRoot(size)); // Register Handler
        services.AddHandlers(); // Generate and register Controllers

        // Optionally Include xml documentation from your Commands to Swagger
        services.AddSwaggerGen(c =>
        {
            c.AddHandlerDocs();
        }
    }
}
```
This will generate dynamic assembly containing your controllers.