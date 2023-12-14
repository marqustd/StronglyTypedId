using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace StronglyTypedIds;

internal static class EmbeddedSources
{
    private static readonly Assembly ThisAssembly = typeof(EmbeddedSources).Assembly;
    internal static readonly string StronglyTypedIdAttributeSource = LoadAttributeTemplateForEmitting(nameof(StronglyTypedIdAttribute));
    internal static readonly string IValidatorSource = LoadAttributeTemplateForEmitting(nameof(IValidator));
    internal static readonly string StringTransformationSource = LoadAttributeTemplateForEmitting(nameof(StringTransformation));


    private static string LoadEmbeddedResource(string resourceName)
    {
        var resourceStream = ThisAssembly.GetManifestResourceStream(resourceName);
        if (resourceStream is null)
        {
            var existingResources = ThisAssembly.GetManifestResourceNames();
            throw new ArgumentException($"Could not find embedded resource {resourceName}. Available names: {string.Join(", ", existingResources)}");
        }

        using var reader = new StreamReader(resourceStream, Encoding.UTF8);

        return reader.ReadToEnd();
    }

    private static string LoadAttributeTemplateForEmitting(string resourceName)
    {
        var resource = LoadEmbeddedResource($"StronglyTypedIds.Templates.Sources.{resourceName}.cs");

        return resource
            .Replace("public enum", "internal enum")
            .Replace("public sealed", "internal sealed");
    }
}