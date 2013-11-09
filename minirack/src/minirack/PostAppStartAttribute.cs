using System;

namespace minirack
{
    /// <summary>
    /// Apply this attribute to any class that has a **public static void PostAppStart()** method,
    /// and it will be called via the magic of assembly attributes and WebActivatorEx
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PostAppStartAttribute : Attribute
    {
        public string InitMethodName { get; set; }
        public int Order { get; set; }

	    public PostAppStartAttribute(string initMethodName = "PostAppStart", int order = -1)
	    {
		    InitMethodName = initMethodName;
		    Order = order;
	    }
    }
}
