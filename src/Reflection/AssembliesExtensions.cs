using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infrastructure.Utils.Reflection;

public static class AssembliesExtensions
{
    public static Assembly[] GetAllReferencedAssemblies(this Assembly assembly, bool ignoreSystemAssemblies = true)
    {
        var firstReferences = assembly.GetReferencedAssemblies()
                .Select(Assembly.Load);

        if (ignoreSystemAssemblies)
        {
            firstReferences = firstReferences.GetNonSystemAssembliesOnly();
        }

        var result = new List<Assembly>(firstReferences);

        foreach (var reference in firstReferences)
            result.AddRange(GetAllReferencedAssemblies(reference));

        return result.Distinct().ToArray();
    }

    private static Assembly[] GetNonSystemAssembliesOnly(this IEnumerable<Assembly> assemblies)
    {
        return assemblies
                .Where(x => (x.Location == null ||
                    (!x.Location.Contains("C:/Program Files/dotnet/shared") &&
                    !x.Location.Contains("C:\\Program Files\\dotnet\\shared"))) &&
                    !x.FullName!.StartsWith("Microsoft.") &&
                    !x.FullName.StartsWith("System."))
                .ToArray();
    }
}
