# MiniRack
## Tiny aggregator for self-registering HttpModules and pre/post app start code.

```
PM> Install-Package MiniRack
```

This micro library leans on **WebActivatorEx** to simplify self-registering HttpModules and pre/post app start code.
All it adds on top is an attribute that you can apply to IHttpModules to have them automatically
registered at runtime by the participating ASP.NET application.


### Usage

```csharp
using minirack;

[Pipeline]
public class MyModule : IHttpModule
{
	// This module will be dynamically registered...
}
```

_This library works on AppHarbor._

### Bypass in web.config

```xml
<configuration>
	<appSettings>
		<add key="minirack_Bypass" value="true" />
	</appSettings>
</configuration>
```

### [PreAppStart] usage

```csharp
using minirack;

[PreAppStart]
public class AnyClass
{
	public static void PreAppStart()
	{
		// Will be called _before_ the HttpApplication is created, for one time static
		// init, warmup, caching, whatever.
		
		// If there's an IHttpModule you don't control the source of, you can
		// still have it registered dynamically here:
		DynamicModuleUtility.RegisterModule(typeof(ThirdPartyNonDynamicModule));
		
		// If you have control over the source, you could just put a [Pipeline] attribute
		// on your IHttpModule class, and it will be self registering (using the same method above)
		
		// PreApp also is useful for querying types with attributes,
		// in order to do [x] to/with them. Minirack helps query types easily:
		Type[] mytypes = minirack.PipelineInstaller
			// non system/MS types in the current asm, flattened
			.GetUserTypes(t => // where predicate will filter by attribute and implements in this example
				t.HasAttribute<YourAttribute>(inherit: false)
				&& t.Implements<YourBaseClass>)
			.ToArray();
		foreach(Type mytype in mytypes)
		{
			// whatever
		}
	}
}
```

### [PostAppStart] usage

```csharp
using minirack;

[PostAppStart]
public class AnyClass
{
	public static void PostAppStart()
	{
		// Will be called once _after_ the HttpApplication is started
		// (one time static init, warmup, caching, whatever)
	}
}
```
