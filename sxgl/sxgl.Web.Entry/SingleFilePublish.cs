using Furion;
using System.Reflection;

namespace sxgl.Web.Entry;

public class SingleFilePublish : ISingleFilePublish
{
    public Assembly[] IncludeAssemblies()
    {
        return Array.Empty<Assembly>();
    }

    public string[] IncludeAssemblyNames()
    {
        return new[]
        {
            "sxgl.Application",
            "sxgl.Core",
            "sxgl.EntityFramework.Core",
            "sxgl.Web.Core"
        };
    }
}