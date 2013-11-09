# MiniRack
## Tiny aggregator for self-registering HttpModules

```
PM> Install-Package MiniRack
```

This micro library leans on WebActivator to simplify self-registering HttpModules. All it adds on top is an attribute
that you can apply to IHttpModules to have them automatically registered at runtime by the participating ASP.NET 
application.

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

### [PostAppStart] usage

```csharp
using minirack;

[PostAppStart]
public class AnyClass
{
	public static void PostAppStart()
	{
		// Will be called once after the HttpApplication is started
		// (one time static init, warmup, caching, whatever)
	}
}
```

