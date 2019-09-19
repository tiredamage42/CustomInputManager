using UnityEngine;

#if UNITY_EDITOR    
using UnityEditor;
#endif

namespace CustomEditorTools {

    public class EnumFlagsAttribute : PropertyAttribute { public EnumFlagsAttribute() { } }
    
    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))] public class EnumFlagsDrawer : PropertyDrawer {
        public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
            _property.intValue = EditorGUI.MaskField( _position, _label, _property.intValue, _property.enumNames );
        }
    }
    #endif
}
