#if UNITY_EDITOR
using System;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

internal sealed class SerializeReferenceTypeDropdown : AdvancedDropdown
{
    private readonly SerializedProperty mProperty;
    private readonly Type mBaseType;

    private const float minWidth = 300f;

    public SerializeReferenceTypeDropdown(
        AdvancedDropdownState state,
        SerializedProperty property,
        Type baseType)
        : base(state)
    {
        mProperty = property;
        mBaseType = baseType;
        minimumSize = new Vector2(minWidth, 0f);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Select Type");

        var types = TypeCache.GetTypesDerivedFrom(mBaseType)
            .Where(IsValidCandidate)
            .OrderBy(t => t.Name);

        foreach (var type in types)
            root.AddChild(new TypeItem(type));

        if (!root.children.Any())
            root.AddChild(new AdvancedDropdownItem("No compatible types"));

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        if (item is TypeItem ti)
            Assign(ti.Type);
    }

    private void Assign(Type type)
    {
        // финальная страховка
        if (!IsValidCandidate(type))
        {
            Debug.LogError(
                $"[SerializeReference] Invalid type selected: {type.FullName}");
            return;
        }

        var instance = FormatterServices.GetUninitializedObject(type);
        mProperty.managedReferenceValue = instance;
        mProperty.serializedObject.ApplyModifiedProperties();
    }

    private static bool IsValidCandidate(Type type)
    {
        if (type == null) return false;
        if (type.IsAbstract) return false;
        if (type.IsInterface) return false;
        if (type.IsValueType) return false;
        if (type.IsGenericTypeDefinition) return false;
        if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return false;
        return true;
    }

    private sealed class TypeItem : AdvancedDropdownItem
    {
        public readonly Type Type;
        public TypeItem(Type type) : base(type.Name)
        {
            Type = type;
        }
    }
}
#endif