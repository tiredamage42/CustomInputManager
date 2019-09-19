
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CustomEditorTools {
    
    #if UNITY_EDITOR
    /*
        more intuitive array/list handling for gui 
    */

    [CustomPropertyDrawer(typeof(NeatArrayAttribute))] 
    public class NeatArrayAttributeDrawer : PropertyDrawer
    {
        static readonly Color backgroundColor = new Color(0,0,0,.1f);

        void DrawBackgroundBox (float indentedX, float y, float width, float height) {
            GUI.backgroundColor = backgroundColor;
            GUI.Box( new Rect(indentedX, y, width, height ),"");
            GUI.backgroundColor = Color.white;
        }


        GUIContent deleteGUI = new GUIContent("", "Delete Element");
        GUIContent addGUI = new GUIContent("", "Add New Element");
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            SerializedProperty displayed = property.FindPropertyRelative("displayed");

            // the property we want to draw is the list child
            property = property.FindPropertyRelative("list");
            
            float indentedX = GUITools.CalculateIndent(EditorGUI.indentLevel, position.x);

            float y = position.y;

            bool displayedValue = displayed.boolValue;
            if (GUITools.LittleButton(new Rect(indentedX, y, 0, 0), new GUIContent("", displayedValue ? "Hide" : "Show"), displayedValue ? GUITools.blueColor : new Color32(255,255,255,255))){
                displayed.boolValue = !displayedValue;
                displayedValue = !displayedValue;
            }

            GUI.enabled = displayedValue;
            if (GUITools.LittleButton(new Rect(indentedX + GUITools.indentWidth, y, 0, 0), addGUI, GUITools.greenColor)) {
                property.InsertArrayElementAtIndex(property.arraySize);
            }
            GUI.enabled = true;

            GUI.Label(new Rect(indentedX + GUITools.indentWidth + GUITools.littleButtonSize, y, EditorStyles.label.CalcSize(label).x, EditorGUIUtility.singleLineHeight), label);
            
            float h = CalculateHeight(property, displayedValue, 1.25f);
            
            DrawBackgroundBox ( indentedX,  y, position.width, h);

            if (displayedValue) {
                y += EditorGUIUtility.singleLineHeight;

                int indexToDelete = -1;
                EditorGUI.indentLevel ++;

                indentedX = GUITools.CalculateIndent(EditorGUI.indentLevel, position.x);
                
                for (int i = 0; i < property.arraySize; i++) {
                    
                    if (GUITools.LittleButton(new Rect(indentedX, y, 0, 0), deleteGUI, GUITools.redColor)) {
                        indexToDelete = i;
                    }
                    
                    SerializedProperty p = property.GetArrayElementAtIndex(i);
                    EditorGUI.PropertyField(new Rect(position.x + GUITools.littleButtonSize + 5, y, position.width, EditorGUIUtility.singleLineHeight), p);
                    y += EditorGUI.GetPropertyHeight(p, true);
                }
                EditorGUI.indentLevel--;

                if (indexToDelete != -1) property.DeleteArrayElementAtIndex(indexToDelete);
            }

            EditorGUI.EndProperty();
        }

        float CalculateHeight (SerializedProperty actualProp, bool displayedValue, float usedExtraOffset) {
            if (!displayedValue)
                return EditorGUIUtility.singleLineHeight;
                
            int arraySize = actualProp.arraySize;
            float h = EditorGUIUtility.singleLineHeight * (arraySize == 0 ? 1 : usedExtraOffset);
            for (int i = 0; i < arraySize; i++) h += EditorGUI.GetPropertyHeight(actualProp.GetArrayElementAtIndex(i), true);
            return h;
        }
    
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label) {
            return CalculateHeight(prop.FindPropertyRelative("list"), prop.FindPropertyRelative("displayed").boolValue, 1.5f);
        }
    }
    #endif
    
    //the actual attribute
    public class NeatArrayAttribute : PropertyAttribute { }
    
    [Serializable] public class NeatBoolList : NeatListWrapper<bool> {}
    [Serializable] public class NeatBoolArray : NeatArrayWrapper<bool> {}
    
    [Serializable] public class NeatStringList : NeatListWrapper<string> {}
    [Serializable] public class NeatStringArray : NeatArrayWrapper<string> {}
    
    [Serializable] public class NeatIntList : NeatListWrapper<int> {}
    [Serializable] public class NeatIntArray : NeatArrayWrapper<int> {}
    
    [Serializable] public class NeatFloatList : NeatListWrapper<float> {}
    [Serializable] public class NeatFloatArray : NeatArrayWrapper<float> {}

    public class NeatArrayWrapper<T> {
        public T[] list;
        public int Length { get { return list.Length; } }
        public T this[int index] { get { return list[index]; } }
        public bool displayed;
        public static implicit operator T[](NeatArrayWrapper<T> c) { return c.list; }
        // public static implicit operator NeatArrayWrapper<T>(T[] l) { return new NeatArrayWrapper<T>(){ list = l }; }
    }
    public class NeatListWrapper<T> {
        public List<T> list;
        public int Count { get { return list.Count; } }
        public T this[int index] { get { return list[index]; } }
        public bool displayed;
        public static implicit operator List<T>(NeatListWrapper<T> c) { return c.list; }
        // public static implicit operator NeatListWrapper<T>(List<T> l) { return new NeatListWrapper<T>(){ list = l }; }
    }
}


