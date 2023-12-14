using System;

namespace StronglyTypedIds;

[System.AttributeUsage(System.AttributeTargets.Class)]
public sealed class StronglyTypedIdAttribute : Attribute
{
    public StronglyTypedIdAttribute(StringTransformation transformation = StringTransformation.None, Type? validator = default)
    {
        
    }
}