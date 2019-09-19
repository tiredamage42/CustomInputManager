using UnityEngine;
using UnityEditor;
using CustomInputManager.Internal;
using CustomEditorTools;
namespace CustomInputManager.Editor {
	public class InputSettingsTab
	{

		SerializedObject serializedObject;
        string [] props = new string[] {
            "_maxPlayers", "_dpadGravity", "_dpadSensitivity", "_dpadSnap", "_joystickCheckFrequency",
        };

        void DrawInputManagerSettings () {
            if (Application.isPlaying) {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Scene already initialized... any changes will take place the next time you enter play mode.", MessageType.Info);
            }

            GUITools.Space(2);
            for (int i = 0; i < props.Length; i++) {
                GUI.enabled = i > 0 || !Application.isPlaying;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(props[i]), true);
            }
            GUI.enabled = true;
        }
        public void OnGUI()
		{
            if (serializedObject == null) serializedObject = new SerializedObject( InputManager.instance );
            serializedObject.Update();
            DrawInputManagerSettings();
            serializedObject.ApplyModifiedProperties();
		}
	}
}
