using UnityEngine;
using UnityEditor;
using System.IO;
using CustomInputManager;
using System;
using UnityInputConverter;


namespace CustomInputManagerEditor.IO
{
	public static class EditorToolbox
	{
		static string m_snapshotFile { get { return Path.Combine(Application.temporaryCachePath, "input_config.xml"); } }
        


		public static bool CanLoadSnapshot()
		{
			return File.Exists(m_snapshotFile);
		}
		
		public static void CreateSnapshot(InputManager inputManager)
		{
			InputSaverXML inputSaver = new InputSaverXML(m_snapshotFile);
			inputSaver.Save(inputManager.ControlSchemes);//.GetSaveData());
		}
		
		public static void LoadSnapshot(InputManager inputManager)
		{
			if(!CanLoadSnapshot())
				return;

			InputLoaderXML inputLoader = new InputLoaderXML(m_snapshotFile);
			inputManager.SetSaveData(inputLoader.Load());
		}
		
		public static void ShowStartupWarning()
		{
			string key = string.Concat(PlayerSettings.companyName, ".", PlayerSettings.productName, ".InputManager.StartupWarning");
			
			if(!EditorPrefs.GetBool(key, false))
			{
				string message = "In order to use the InputManager plugin you need to overwrite your project's input settings. Your old input axes will be exported to a file which can be imported at a later time from the File menu.\n\nDo you want to overwrite the input settings now?\nYou can always do it later from the File menu.";
				if(EditorUtility.DisplayDialog("Warning", message, "Yes", "No"))
				{
					if(OverwriteProjectSettings())
						EditorPrefs.SetBool(key, true);
				}
			}
		}
		
		public static bool OverwriteProjectSettings()
		{
			int length = Application.dataPath.LastIndexOf('/');
			string projectSettingsFolder = string.Concat(Application.dataPath.Substring(0, length), "/ProjectSettings");
			string inputManagerPath = string.Concat(projectSettingsFolder, "/InputManager.asset");

			if(!Directory.Exists(projectSettingsFolder))
			{
				EditorUtility.DisplayDialog("Error", "Unable to get the correct path to the ProjectSetting folder.", "OK");
				return false;
			}

			if(!EditorUtility.DisplayDialog("Warning", "You chose not to export your old input settings. They will be lost forever. Are you sure you want to continue?", "Yes", "No"))
				return false;
			
			InputConverter inputConverter = new InputConverter();
			inputConverter.GenerateDefaultUnityInputManager(inputManagerPath);
			EditorUtility.DisplayDialog("Success", "The input settings have been successfully replaced.\n\nYou might need to minimize and restore Unity to reimport the new settings.", "OK");
			return true;
		}

		public static Texture2D GetUnityIcon(string name)
		{
			return EditorGUIUtility.Load(name + ".png") as Texture2D;
		}
		public static Texture2D GetCustomIcon(string name)
		{
			return Resources.Load<Texture2D>(name) as Texture2D;
		}
	}

	public class KeyCodeField
	{
		private string m_controlName, m_keyString;
		private bool m_isEditing;

		public KeyCodeField()
		{
			m_controlName = Guid.NewGuid().ToString("N");
			m_keyString = "";
			m_isEditing = false;
		}

		string Key2String (KeyCode key) {
			return key == KeyCode.None ? "" : KeyCodeConverter.KeyToString(key);
		}

		public KeyCode OnGUI(string label, KeyCode key)
		{
			GUI.SetNextControlName(m_controlName);
			bool hasFocus = (GUI.GetNameOfFocusedControl() == m_controlName);
			if(!m_isEditing && hasFocus)
			{
				m_keyString = Key2String(key);
			}

			m_isEditing = hasFocus;
			if(m_isEditing)
			{
				m_keyString = EditorGUILayout.TextField(label, m_keyString);
			}
			else
			{
				EditorGUILayout.TextField(label, Key2String(key));
			}

			if(m_isEditing && Event.current.type == EventType.KeyUp)
			{
				key = KeyCodeConverter.StringToKey(m_keyString);

				m_keyString = Key2String(key);
				m_isEditing = false;
			}

			return key;
		}

		public void Reset()
		{
			m_keyString = "";
			m_isEditing = false;
		}
	}
}
