using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StateMachine))]
public class StateMachinePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var boolValue = property.FindPropertyRelative("debugStateChanges");

        float height = EditorGUI.GetPropertyHeight(boolValue);
        
        var boolRect = new Rect(position.x, position.y, 250f, height);
        
        EditorGUI.PropertyField(boolRect, boolValue, new GUIContent("Debug State Changes"));

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + 25f;
    }
}