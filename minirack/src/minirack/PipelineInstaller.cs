using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using minirack;
using minirack.Extensions;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(PipelineInstaller), "PreAppInit")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(PipelineInstaller), "PostAppInit")]

namespace minirack
{
    /// <summary>
    /// A "mini-rack", it acts as a plug for middleware that can intercept and change requests and responses at runtime.
    /// </summary>
    public class PipelineInstaller
    {
        public static void PreAppInit()
        {
            var disableValue = ConfigurationManager.AppSettings["minirack_Bypass"];
            bool disable;
            bool.TryParse(disableValue, out disable);
            if (disable) return;
            if(AppHarbor.IsHosting())
            {
                DynamicModuleUtility.RegisterModule(typeof(AppHarborModule));    
            }
            RegisterPipelineModules();
        }

	    public static void PostAppInit()
	    {
			RunPostStartMethods();
	    }

		/// <summary>
		/// returns all assemblies in the current AppDomain that don't start
		/// with an obvious ms name (mscorlib, system, microsoft)
		/// </summary>
	    public static IEnumerable<Assembly> GetNonSystemTypes()
	    {
		    var assemblies = AppDomain.CurrentDomain.GetAssemblies()
			    .Where(asm => !asm.FullName.StartsWith("mscorlib")
								&& !asm.FullName.StartsWith("System.")
								&& !asm.FullName.StartsWith("Microsoft."));
		    return assemblies;
	    }

	    public static IEnumerable<Type> GetUserTypes(Predicate<Type> where = null)
	    {
		    var types = GetNonSystemTypes()
			    .SelectMany(asm => asm.GetTypes())
				.Where(t => where == null || where(t));
		    return types;
	    }

        private static void RegisterPipelineModules()
        {
	        var types = GetUserTypes();
            var modules = types.Where(t => t.HasAttribute<PipelineAttribute>() && t.Implements<IHttpModule>());
            var moduleOrder = modules.Select(m => new {m, i = m.GetAttribute<PipelineAttribute>().Order}).OrderBy(o => o.i);
            foreach (var order in moduleOrder)
            {
				// TODO: When/why is BuildManager.AddReferencedAssembly(asm); required?
                DynamicModuleUtility.RegisterModule(order.m);
            }
        }

        private static void RunPostStartMethods()
        {
	        var types = GetUserTypes();
            var postStartTypes = types.Where(t => t.HasAttribute<PostAppStartAttribute>());
            var moduleOrder = postStartTypes.Select(m => new {m, i = m.GetAttribute<PostAppStartAttribute>().Order}).OrderBy(o => o.i);
            foreach (var order in moduleOrder)
            {
                DynamicModuleUtility.RegisterModule(order.m);
            }
        }


    }
}