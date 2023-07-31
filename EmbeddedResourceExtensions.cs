using System.Reflection;

internal static class Extensions
{
    internal static string LoadEmbeddedResource(this Assembly assembly, string resourceName)
    {
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            using (var reader = new StreamReader(stream!))
            {
                return reader.ReadToEnd();
            }
        }
    }
}