
using UnityEngine;
using UnityEditor;
using CustomInputManager;

namespace CustomInputManagerEditor.IO
{
	[CustomEditor(typeof(InputManager))]
	public class InputManagerInspector : Editor
	{
		private const int BUTTON_HEIGHT = 35;
		private InputManager m_inputManager;
		private GUIContent m_createSnapshotInfo;
		
		private void OnEnable()
		{
			m_inputManager = target as InputManager;
			m_createSnapshotInfo = new GUIContent("Create\nSnapshot", "Creates a snapshot of your input configurations which can be restored at a later time(when you exit play-mode for example)");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
			
			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			
			GUI.enabled = !InputEditor.IsOpen;
			if(GUILayout.Button("Input\nEditor", GUILayout.Height(BUTTON_HEIGHT)))
			{
				InputEditor.OpenWindow();
			}
			GUI.enabled = true;
			
			if(GUILayout.Button(m_createSnapshotInfo, GUILayout.Height(BUTTON_HEIGHT)))
			{
				EditorToolbox.CreateSnapshot(m_inputManager);
			}
			
			GUI.enabled = EditorToolbox.CanLoadSnapshot();
			if(GUILayout.Button("Restore\nSnapshot", GUILayout.Height(BUTTON_HEIGHT)))
			{
				EditorToolbox.LoadSnapshot(m_inputManager);
			}
			GUI.enabled = true;
			
			GUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(serializedObject.targetObject);
			// UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(m_inputManager.gameObject.scene);
			
		}
	}
}