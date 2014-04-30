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
            if (Disabled()) return;
            if(AppHarbor.IsHosting())
            {
                DynamicModuleUtility.RegisterModule(typeof(AppHarborModule));    
            }
            RegisterPipelineModules();
			RunPreStartMethods();
        }

	    public static void PostAppInit()
	    {
            if (Disabled()) return;
			RunPostStartMethods();
	    }

	    public static bool Disabled()
	    {
            var disableValue = ConfigurationManager.AppSettings["minirack_Bypass"];
            bool disable;
            bool.TryParse(disableValue, out disable);
		    return disable;
	    }

		/// <summary>
		/// returns all assemblies in the current AppDomain that don't start
		/// with an obvious ms name (mscorlib, system, microsoft)
		/// </summary>
	    public static Assembly[] GetNonSystemTypes()
	    {
		    var assemblies = AppDomain.CurrentDomain.GetAssemblies()
			    .Where(asm => !asm.FullName.StartsWith("mscorlib")
								&& !asm.FullName.StartsWith("System.")
								&& !asm.FullName.StartsWith("Microsoft."))
				.ToArray();
		    return assemblies;
	    }

	    public static Type[] GetUserTypes(Predicate<Type> where = null)
	    {
			Assembly[] asms = GetNonSystemTypes();
			var flatTypes = new List<Type>();
			foreach (var asm in asms)
			{
				try
				{
					var types = asm.GetTypes()
						.Where(t => where == null || where(t));
					flatTypes.AddRange(types);
				}
				catch
				{
					// Seems that some assemblies might throw exceptions when you call .GetTypes() (Allocate.Net.dll...)
					// Those types aren't going to contain what we're looking for here anyway, so we can safely skip them
				}
			}
			return flatTypes.ToArray();
	    }

        private static void RegisterPipelineModules()
        {
	        var types = GetUserTypes();
            var modules = types.Where(t => t.HasAttribute<PipelineAttribute>(inherit: false) && t.Implements<IHttpModule>());
            var moduleOrder = modules.Select(m => new {m, i = m.GetAttribute<PipelineAttribute>(inherit: false).Order}).OrderBy(o => o.i);
            foreach (var order in moduleOrder)
            {
				// TODO: When/why is BuildManager.AddReferencedAssembly(asm); required?
                DynamicModuleUtility.RegisterModule(order.m);
            }
        }

        private static void RunPreStartMethods()
        {
	        var types = GetUserTypes();
            var preStartTypes = types.Where(t => t.HasAttribute<PreAppStartAttribute>(inherit: false));
	        var moduleOrder = preStartTypes
				.Select(t => new { t, att = t.GetAttribute<PreAppStartAttribute>(inherit: false) })
				.OrderBy(o => o.att.Order);
            foreach (var m in moduleOrder)
            {
	            MethodInfo initMethod = m.t.GetMethod(m.att.InitMethodName);
	            if (initMethod == null)
		            throw new Exception("No method found " + m.att.InitMethodName + " in type " + m.t.FullName + ","
										+ " (it's being called due to the [PreAppStart] attribute on the type");
	            initMethod.Invoke(null, null);
            }
        }

        private static void RunPostStartMethods()
        {
	        var types = GetUserTypes();
            var postStartTypes = types.Where(t => t.HasAttribute<PostAppStartAttribute>(inherit: false));
	        var moduleOrder = postStartTypes
				.Select(t => new { t, att = t.GetAttribute<PostAppStartAttribute>(inherit: false) })
				.OrderBy(o => o.att.Order);
            foreach (var m in moduleOrder)
            {
	            MethodInfo initMethod = m.t.GetMethod(m.att.InitMethodName);
				if (initMethod == null)
		            throw new Exception("No method found " + m.att.InitMethodName + " in type " + m.t.FullName + ","
										+ " (it's being called due to the [PostAppStart] attribute on the type");
	            initMethod.Invoke(null, null);
            }
        }

    }
}