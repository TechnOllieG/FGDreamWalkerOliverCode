using System;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace GP2_Team7.EditorScripts
{
	// ===============================================================================================
	// =============================== GridEditorLib.cs by TechnOllieG ===============================
	// ============ Purpose: Provides methods for creating custom inspector fields easily ============
	// ===============================================================================================
    
	// Inherit custom Editors from this class
	public class EditorLib<TMono> : EditorLib where TMono : MonoBehaviour
	{
		protected TMono scriptInstance;

		protected GUIStyle titleStyle;

		/// <summary>
		/// Initializes the custom editor with a scriptInstance and titleStyle
		/// </summary>
		public void Init()
		{
			titleStyle = new GUIStyle {richText = true, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold};
            
			if(scriptInstance == null)
				scriptInstance = (TMono) target;
		}

		/// <summary>
		/// Checks if the serializedObject was modified and if true it applies the modified properties
		/// </summary>
		protected void ApplyProperties()
		{
			if (serializedObject.hasModifiedProperties)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
    
	// And use this class if you only want to access the static methods
	public class EditorLib : Editor
	{
		public enum FieldMode
		{
			Instant,
			Delayed
		}

		public static EditorGUILayout.HorizontalScope Horizontal => new EditorGUILayout.HorizontalScope();
		public static EditorGUILayout.VerticalScope Vertical => new EditorGUILayout.VerticalScope();
		public static void Indent(int numberOfIndents = 1)
		{
			for (int i = 0; i < numberOfIndents; i++)
				EditorGUILayout.LabelField("", GUILayout.Width(MarginInsideFoldout));
		}
        
		public const int MinLabelWidth = 100;
		public const int MaxLabelWidth = 250;
		public const int MarginInsideFoldout = 20;

		public static void Space(int multiplier = 1)
		{
			for (int i = 0; i < multiplier; i++)
			{
				EditorGUILayout.Space();
			}
		}

		public static void FlexibleSpace() => GUILayout.FlexibleSpace();

		public static void Foldout(GUIContent label, ref bool isOpen, int numberOfIndents = 0, bool spaceBefore = false)
		{
			if (spaceBefore) Space();

			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);
                
			isOpen = EditorGUILayout.Foldout(isOpen, label);
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}
        
		public static void Foldout(GUIContent label, SerializedProperty foldoutBoolProperty, int numberOfIndents = 0, bool spaceBefore = false)
		{
			bool tempBool = foldoutBoolProperty.boolValue;
			Foldout(label, ref tempBool, numberOfIndents, spaceBefore);
			foldoutBoolProperty.boolValue = tempBool;
		}

		public static void EnumField<T>(GUIContent label, SerializedProperty property, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, params GUILayoutOption[] options) where T : struct, Enum
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);

			using (new LabelWidth(labelWidth))
			{
				property.enumValueIndex = Convert.ToInt32((T) EditorGUILayout.EnumPopup(label, (T) Enum.ToObject(typeof(T), property.enumValueIndex), options));
			}
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}

		public static void FloatField(GUIContent label, SerializedProperty property, FieldMode mode, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, params GUILayoutOption[] options)
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);
                
			using (new LabelWidth(labelWidth))
			{
				if(mode == FieldMode.Instant) property.floatValue = EditorGUILayout.FloatField(label, property.floatValue, options);
				else property.floatValue = EditorGUILayout.DelayedFloatField(label, property.floatValue, options);
			}
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}
        
		public static void Vector3Field(GUIContent label, SerializedProperty property, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, params GUILayoutOption[] options)
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);
                
			using (new LabelWidth(labelWidth))
			{
				property.vector3Value = EditorGUILayout.Vector3Field(label, property.vector3Value, options);
			}
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}
		
		public static void ObjectField<T>(GUIContent label, SerializedProperty property, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, bool allowSceneObjects = false, params GUILayoutOption[] options) where T : Object
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);

			using (new LabelWidth(labelWidth))
			{
				property.objectReferenceValue = (T) EditorGUILayout.ObjectField(label, property.objectReferenceValue, typeof(T), allowSceneObjects, options);
			}
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}
        
		public static void ReadOnlyObjectField<T>(string label, Object obj, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, bool allowSceneObjects = false, params GUILayoutOption[] options) where T : Object
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);
			
			using (new LabelWidth(labelWidth))
			{
				EditorGUILayout.ObjectField(label, obj, typeof(T), allowSceneObjects, options);
			}
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}

		public static void ColorField(GUIContent label, SerializedProperty property, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, params GUILayoutOption[] options)
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);

			using (new LabelWidth(labelWidth))
			{
				property.colorValue = EditorGUILayout.ColorField(label, property.colorValue, options);
			}
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}

		private delegate float FloatFieldDelegate(GUIContent content, float value, params GUILayoutOption[] options);

		public static void IntField(GUIContent label, SerializedProperty property, FieldMode mode, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, params GUILayoutOption[] options)
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);

			using (new LabelWidth(labelWidth))
			{
				if(mode == FieldMode.Instant) property.intValue = EditorGUILayout.IntField(label, property.intValue, options);
				else property.intValue = EditorGUILayout.DelayedIntField(label, property.intValue, options);
			}
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}

		private delegate int IntFieldDelegate(GUIContent content, int value, params GUILayoutOption[] options);

		public static void BoolField(GUIContent label, SerializedProperty property, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, params GUILayoutOption[] options)
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);

			using (new LabelWidth(labelWidth))
			{
				property.boolValue = EditorGUILayout.Toggle(label, property.boolValue, options);
			}
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}

		public static void Title(string text, GUIStyle style = null, bool spaceBefore = false, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, string color = "white", params GUILayoutOption[] options)
		{
			if (style == null)
				style = new GUIStyle() {richText = true, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold};
            
			if (spaceBefore) Space();
			
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();

			Indent(numberOfIndents);
            
			EditorGUILayout.LabelField($"<color={color}>{text}</color>", style, options);
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}

		public static void Label(string text, int numberOfIndents = 0, GUIStyle style = null, params GUILayoutOption[] options) => Label(new GUIContent(text), numberOfIndents, style, options);

		public static void Label(GUIContent label, int numberOfIndents = 0, GUIStyle style = null, params GUILayoutOption[] options)
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);
                
			if(style == null) EditorGUILayout.LabelField(label, options);
			else EditorGUILayout.LabelField(label, style, options);
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}
        
		public static void StringField(GUIContent label, SerializedProperty property, FieldMode mode, int numberOfIndents = 0, int labelWidth = MaxLabelWidth, params GUILayoutOption[] options)
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);

			using (new LabelWidth(labelWidth))
			{
				if(mode == FieldMode.Instant) property.stringValue = EditorGUILayout.TextField(label, property.stringValue, options);
				else property.stringValue = EditorGUILayout.DelayedTextField(label, property.stringValue, options);
			}
			
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();
		}

		public static bool Button(string buttonText, int width = 0, int numberOfIndents = 0, params GUILayoutOption[] options) => Button(new GUIContent(buttonText), width, numberOfIndents, options);

		public static bool Button(GUIContent buttonLabel, int width = 0, int numberOfIndents = 0, params GUILayoutOption[] options)
		{
			if (numberOfIndents > 0)
				EditorGUILayout.BeginHorizontal();
			
			Indent(numberOfIndents);

			bool returnValue;
				
			if(width > 0) returnValue = GUILayout.Button(buttonLabel, GUILayout.Width(width));
			else returnValue = GUILayout.Button(buttonLabel);
			 
			if (numberOfIndents > 0)
				EditorGUILayout.EndHorizontal();

			return returnValue;
		}

		public class LabelWidth : IDisposable
		{
			private float _oldLabelWidth;
			
			public LabelWidth(float width)
			{
				_oldLabelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = width;
			}

			public void Dispose()
			{
				EditorGUIUtility.labelWidth = _oldLabelWidth;
			}
		}
	}
}