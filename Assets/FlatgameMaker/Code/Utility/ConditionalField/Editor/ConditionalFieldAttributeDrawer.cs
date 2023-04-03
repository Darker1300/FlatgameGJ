using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class Helper
{
	/// <summary>
	/// Get string representation of serialized property, even for non-string fields
	/// </summary>
	public static string AsStringValue(this SerializedProperty property)
	{
		switch (property.propertyType)
		{
			case SerializedPropertyType.String:
				return property.stringValue;
				case SerializedPropertyType.Character:
			case SerializedPropertyType.Integer:
				if (property.type == "char") return System.Convert.ToChar(property.intValue).ToString();
				return property.intValue.ToString();
				case SerializedPropertyType.ObjectReference:
				return property.objectReferenceValue != null ? property.objectReferenceValue.ToString() : "null";
				case SerializedPropertyType.Boolean:
				return property.boolValue.ToString();
				case SerializedPropertyType.Enum:
				return property.enumNames[property.enumValueIndex];
				default:
				return string.Empty;
		}
	}

	public static bool IsNullOrEmpty<T>(this IEnumerable<T> sequence)
	{
		if (sequence == null) return true;

		return !sequence.Any();
	}
}

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldAttributeDrawer : PropertyDrawer
{

	private ConditionalFieldAttribute Attribute
	{
		get { return _attribute ?? (_attribute = attribute as ConditionalFieldAttribute); }
	}

	private string PropertyToCheck
	{
		get { return Attribute != null ? _attribute.PropertyToCheck : null; }
	}

	private object CompareValue
	{
		get { return Attribute != null ? _attribute.CompareValue : null; }
	}


	private ConditionalFieldAttribute _attribute;
	private bool _toShow = true;

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return _toShow ? EditorGUI.GetPropertyHeight(property) : 0;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		if (!PropertyToCheck.IsNullOrEmpty())
		{
			var conditionProperty = property.serializedObject.FindProperty(PropertyToCheck);
			if (conditionProperty != null)
			{
				bool isBoolMatch = conditionProperty.propertyType == SerializedPropertyType.Boolean && conditionProperty.boolValue;
				string compareStringValue = CompareValue == null ? "NULL" : CompareValue.ToString().ToUpper();
				if (isBoolMatch && compareStringValue == "FALSE") isBoolMatch = false;
				
				string conditionPropertyStringValue = conditionProperty.AsStringValue().ToUpper();
				bool objectMatch = compareStringValue == conditionPropertyStringValue;

				if (!isBoolMatch && !objectMatch)
				{
					_toShow = false;
					return;
				}
			}
		}

		_toShow = true;
		EditorGUI.PropertyField(position, property, label);
	}

}