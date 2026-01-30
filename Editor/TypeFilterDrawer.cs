#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypeFilterAttribute))]
public sealed class TypeFilterDrawer : PropertyDrawer
{
    private const float buttonWidth = 60f;
    private const float spacing = 4f;

    private static readonly GUIStyle _titleStyle =
        new(EditorStyles.boldLabel) { wordWrap = false };

    private static readonly GUIStyle _detailsStyle =
        new(EditorStyles.label) { wordWrap = true, fontSize = 11 };

    private static readonly GUIContent _titleContent = new();
    private static readonly GUIContent _detailsContent = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.HelpBox(
                position,
                "TypeFilter works only with [SerializeReference] fields.",
                MessageType.Error);

            EditorGUI.EndProperty();
            return;
        }

        var state = ResolveState(property, out var problemType);

        if (state == ReferenceState.Unsupported)
        {
            DrawUnsupportedWarning(position, property, problemType);
            EditorGUI.EndProperty();
            return;
        }

        var filter = (TypeFilterAttribute)attribute;

        BuildHeader(property, state, out var title, out var details);

        float labelWidth =
            position.width - (buttonWidth * 2 + spacing * 2);

        _titleContent.text = title;
        _detailsContent.text = details;

        float titleHeight = _titleStyle.CalcHeight(_titleContent, labelWidth);
        float detailsHeight =
            string.IsNullOrEmpty(details)
                ? 0f
                : _detailsStyle.CalcHeight(_detailsContent, labelWidth);

        float headerHeight =
            titleHeight +
            (detailsHeight > 0 ? spacing + detailsHeight : 0);

        DrawHeader(
            new Rect(position.x, position.y, position.width, headerHeight),
            property,
            state,
            labelWidth,
            titleHeight,
            detailsHeight,
            filter.BaseType);

        if (state == ReferenceState.Valid)
        {
            var bodyRect = new Rect(
                position.x,
                position.y + headerHeight + spacing,
                position.width,
                position.height - headerHeight - spacing);

            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(bodyRect, property, GUIContent.none, true);
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
            return EditorGUIUtility.singleLineHeight * 2;

        var state = ResolveState(property, out _);

        if (state == ReferenceState.Unsupported)
            return EditorGUIUtility.singleLineHeight * 2.6f;

        BuildHeader(property, state, out var title, out var details);

        float labelWidth =
            EditorGUIUtility.currentViewWidth -
            EditorGUIUtility.labelWidth -
            (buttonWidth * 2 + spacing * 2);

        _titleContent.text = title;
        _detailsContent.text = details;

        float titleHeight = _titleStyle.CalcHeight(_titleContent, labelWidth);
        float detailsHeight =
            string.IsNullOrEmpty(details)
                ? 0f
                : _detailsStyle.CalcHeight(_detailsContent, labelWidth);

        float headerHeight =
            titleHeight +
            (detailsHeight > 0 ? spacing + detailsHeight : 0);

        if (state != ReferenceState.Valid)
            return headerHeight;

        return headerHeight +
               spacing +
               EditorGUI.GetPropertyHeight(property, label, true);
    }

    // ================= helpers =================

    private static ReferenceState ResolveState(
        SerializedProperty property,
        out Type problemType)
    {
        problemType = null;

        try
        {
            var value = property.managedReferenceValue;
            if (value == null)
                return ReferenceState.Empty;

            var type = value.GetType();
            problemType = type;

            if (type.IsValueType ||
                typeof(UnityEngine.Object).IsAssignableFrom(type))
                return ReferenceState.Unsupported;

            return ReferenceState.Valid;
        }
        catch
        {
            return ReferenceState.Unsupported;
        }
    }

    private static void DrawUnsupportedWarning(
        Rect position,
        SerializedProperty property,
        Type type)
    {
        var helpRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight * 2.4f);

        EditorGUI.HelpBox(
            helpRect,
            "SerializeReference supports reference types only (class).\n" +
            $"Unsupported type: {type?.FullName ?? "unknown"}",
            MessageType.Warning);

        var resetRect = new Rect(
            helpRect.xMax - 60f,
            helpRect.y + EditorGUIUtility.singleLineHeight + 2f,
            60f,
            EditorGUIUtility.singleLineHeight);

        if (GUI.Button(resetRect, "Reset"))
        {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private static void BuildHeader(
        SerializedProperty property,
        ReferenceState state,
        out string title,
        out string details)
    {
        if (state == ReferenceState.Empty)
        {
            title = property.displayName +  " None";
            details = string.Empty;
            return;
        }

        var type = property.managedReferenceValue.GetType();
        title = property.displayName;
        details = type.FullName;
    }

    private static void DrawHeader(
        Rect rect,
        SerializedProperty property,
        ReferenceState state,
        float labelWidth,
        float titleHeight,
        float detailsHeight,
        Type baseType)
    {
        var titleRect = new Rect(rect.x, rect.y, labelWidth, titleHeight);
        var detailsRect = new Rect(
            rect.x,
            rect.y + titleHeight + spacing,
            labelWidth,
            detailsHeight);

        var changeRect = new Rect(
            rect.xMax - (buttonWidth * 2 + spacing),
            rect.y,
            buttonWidth,
            EditorGUIUtility.singleLineHeight);

        var resetRect = new Rect(
            rect.xMax - buttonWidth,
            rect.y,
            buttonWidth,
            EditorGUIUtility.singleLineHeight);

        var prev = GUI.color;

        if (state == ReferenceState.Empty)
            GUI.color = Color.yellow;

        EditorGUI.LabelField(titleRect, _titleContent, _titleStyle);
        GUI.color = prev;

        if (detailsHeight > 0)
            EditorGUI.LabelField(detailsRect, _detailsContent, _detailsStyle);

        if (GUI.Button(changeRect, "Change"))
            SerializeReferenceAdvancedDropdown.Show(
                changeRect,
                property,
                baseType);

        using (new EditorGUI.DisabledScope(state == ReferenceState.Empty))
        {
            if (GUI.Button(resetRect, "Reset"))
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    private enum ReferenceState
    {
        Empty,
        Valid,
        Unsupported
    }
}
#endif