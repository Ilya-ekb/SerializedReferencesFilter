using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class TypeFilterAttribute : PropertyAttribute
{
    public Type BaseType { get; }

    public TypeFilterAttribute(Type baseType)
    {
        BaseType = baseType;
    }
}