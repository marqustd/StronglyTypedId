using System;

namespace StronglyTypedIds;

[System.AttributeUsage(System.AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class StronglyTypedIdAttribute : Attribute
{
    public StronglyTypedIdAttribute(StringTransformation transformation = StringTransformation.None, Type? validator = default, params string[] serializers)
    {
        
    }
}