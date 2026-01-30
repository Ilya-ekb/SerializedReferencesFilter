#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

internal static class SerializeReferenceAdvancedDropdown
{
    private static AdvancedDropdownState sState;

    public static void Show(
        Rect anchorRect,
        SerializedProperty property,
        Type baseType)
    {
        if (property == null)
            return;

        sState ??= new AdvancedDropdownState();

        var dropdown = new SerializeReferenceTypeDropdown(
            sState,
            property,
            baseType);

        dropdown.Show(anchorRect);
    }
}
#endif