
#if UNITY_EDITOR
// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CustomEditorTools {

    public static class GUITools
    {

        public static readonly Color32 redColor = new Color32(200, 75, 75, 255);
        public static readonly Color32 blueColor = new Color32(125, 125, 200, 255);
        public static readonly Color32 lightBlue = new Color32(168, 175, 218, 255);
        public static readonly Color32 white = new Color32(255, 255, 255, 255);

        
        public static readonly Color32 greenColor = new Color32(72, 172, 84, 255);

        public const int indentWidth = 20;
        public static float CalculateIndent (int level, float startX) {
            return startX + indentWidth * level;
        }

        public static void Space (int spacing=1) {
            for (int i = 0; i < spacing; i++) EditorGUILayout.Space();
        }


        static GUIStyle _bs;
        static GUIStyle buttonStyle {
            get {
                if (_bs == null) _bs = EditorStyles.miniButton;
                return _bs;
            }
        }

        static GUILayoutOption[] _littleButtonOptions;
        static GUILayoutOption[] littleButtonOptions {
            get {
                if (_littleButtonOptions == null || _littleButtonOptions.Length != 2) {
                    _littleButtonOptions = new GUILayoutOption[] { 
                        GUILayout.Width(littleButtonSize), GUILayout.Height(littleButtonSize)
                    };
                }
                return _littleButtonOptions;
            }
        }

        public const int littleButtonSize = 12;
        
        public static bool LittleButton (Rect pos, GUIContent content, Color32 color) {
            pos.width = pos.height = littleButtonSize;
            bool pressed = false;
            GUI.backgroundColor = color;
            if (GUI.Button(pos, content, buttonStyle)) {
                pressed = true;
            }
            GUI.backgroundColor = Color.white;
            return pressed;
        }
        public static bool LittleButton (GUIContent content, Color32 color) {
            bool pressed = false;
            GUI.backgroundColor = color;
            if (GUILayout.Button(content, buttonStyle, littleButtonOptions)) {
                pressed = true;
            }
            GUI.backgroundColor = Color.white;
            return pressed;
        }
    }
}


#endif
