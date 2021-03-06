﻿using UnityEngine;
using UnityEditor;


using UnityTools;
using UnityTools.EditorTools;
using CustomInputManager.Internal;
namespace CustomInputManager.Editor {
    public class GamepadProfilesEditor 
    {

        void NewProfile (GenericGamepadProfile profile) {
            if (!InputManager.instance._gamepads.Contains(profile)) {
                InputManager.instance._gamepads.Add(profile);
                EditorUtility.SetDirty(InputManager.instance);
                ProjectTools.RefreshAndSave();
            }
        }

        public GenericGamepadProfile CreateNewGamepadProfile (string name) {
            string dir = InputManager.fullResourcesDirectory + GamepadHandler.gamepadProfilesResourcesDirectory;
            string path = dir + "/" + name + ".asset";

            int x = 0;
            while (System.IO.File.Exists(path)) {
                path = dir + "/" + name + x.ToString() + ".asset";
                x++;
            }

            GenericGamepadProfile profile = AssetTools.CreateScriptableObject<GenericGamepadProfile>(path, true);
            NewProfile(profile);
        
            return profile;
        }
	
        public void DuplicateProfile (GenericGamepadProfile source) {
            string sourcePath = AssetDatabase.GetAssetPath(source);
            if (sourcePath.EndsWith(".asset")) sourcePath = sourcePath.Substring(0, sourcePath.Length - 6);
            GenericGamepadProfile newProfile = CreateNewGamepadProfile(System.IO.Path.GetFileName(sourcePath) + "_Duplicate");
            DuplicateGamepadProfile(source, newProfile);
            NewProfile(newProfile);
        }

		void DuplicateGamepadProfile (GenericGamepadProfile source, GenericGamepadProfile target) {
            
            SerializedObject s = new SerializedObject(source);
            SerializedObject t = new SerializedObject(target);

            for (int i = 0; i < buttons.Length; i++) t.FindProperty(buttons[i]).intValue = s.FindProperty(buttons[i]).intValue;
            for (int i = 0; i < axes.Length; i++) t.FindProperty(axes[i]).intValue = s.FindProperty(axes[i]).intValue;
            
            t.ApplyModifiedProperties();
            target.m_dpadType = source.m_dpadType;
            target.platforms.list = source.platforms.list.MakeCopy();
            EditorUtility.SetDirty(target);
        }
        
        string[] axes = new string[] {
            "m_leftStickXAxis", "m_leftStickYAxis",
            "m_rightStickXAxis", "m_rightStickYAxis",
            "m_leftTriggerAxis", "m_rightTriggerAxis",
            "m_dpadXAxis", "m_dpadYAxis"
        };

        string[] buttons = new string[] {
            "m_backButton", "m_startButton",
            "m_actionTopButton", "m_actionBottomButton", "m_actionLeftButton", "m_actionRightButton",
            "m_leftStickButton", "m_rightStickButton",
            "m_leftBumperButton", "m_rightBumperButton",
            "m_dpadUpButton", "m_dpadDownButton", "m_dpadLeftButton", "m_dpadRightButton",
        };

		public void DeleteProfile(GenericGamepadProfile toDelete) {
            if (EditorUtility.DisplayDialog("Delete Gamepad Profile", "Are you sure you want to delete profile: " + toDelete.name + "?", "Yes", "No")) {
                InputManager.instance._gamepads.Remove(toDelete);
                EditorUtility.SetDirty(InputManager.instance);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(toDelete));
				ProjectTools.RefreshAndSave();
			}
        }

        public void RenameProfile(GenericGamepadProfile profile, string newName) {
			AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(profile), newName);
			ProjectTools.RefreshAndSave();
        }

        string[] buttonDisplayNames {
            get {
                if (_bNames == null || _bNames.Length != InputBinding.MAX_JOYSTICK_BUTTONS) _bNames = GenerateJoystickButtonNames();
                return _bNames;
            }
        }

        string[] axisDisplayNames {
            get {
                if (_aNames == null || _aNames.Length != InputBinding.MAX_JOYSTICK_AXES) _aNames = GenerateJoystickAxisNames();
                return _aNames;
            }
        }

        string[] _bNames, _aNames;
        
        public static string[] GenerateJoystickButtonNames()
        {
            string[] _buttonNames = new string[InputBinding.MAX_JOYSTICK_BUTTONS];
            for(int i = 0; i < InputBinding.MAX_JOYSTICK_BUTTONS; i++)
                _buttonNames[i] = "Joystick Button " + i;

            return _buttonNames;
        }

		public static string[] GenerateJoystickAxisNames()
		{
	         string[] _axisNames = new string[InputBinding.MAX_JOYSTICK_AXES];
            for(int i = 0; i < InputBinding.MAX_JOYSTICK_AXES; i++)
            {
                if(i == 0)       _axisNames[i] = "X";
                else if(i == 1)  _axisNames[i] = "Y";
                else if(i == 2)  _axisNames[i] = "3rd axis (Joysticks and Scrollwheel)";
                else if(i == 21) _axisNames[i] = "21st axis (Joysticks)";
                else if(i == 22) _axisNames[i] = "22nd axis (Joysticks)";
                else if(i == 23) _axisNames[i] = "23rd axis (Joysticks)";
                else             _axisNames[i] = string.Format("{0}th axis (Joysticks)", i + 1);
            }
			return _axisNames;
		}

        public void DrawProfile(SerializedObject profileSO)
        {
			if (profileSO == null)
				return;
			
            profileSO.Update();

			EditorGUILayout.PropertyField(profileSO.FindProperty("unityJoystickNames"), true);
            EditorGUILayout.PropertyField(profileSO.FindProperty("joystickAliases"), true);
            EditorGUILayout.PropertyField(profileSO.FindProperty("platforms"), true);

            
			SerializedProperty m_dpadType = profileSO.FindProperty("m_dpadType");
            EditorGUILayout.PropertyField(m_dpadType);

			DrawFields (profileSO, "Buttons", m_dpadType.enumValueIndex, (int)GamepadDPadType.Button, 4, buttons, buttonDisplayNames);
			DrawFields (profileSO, "Axes", m_dpadType.enumValueIndex, (int)GamepadDPadType.Axis, 2, axes, axisDisplayNames);

            profileSO.ApplyModifiedProperties();
        }

		void DrawFields (SerializedObject profileSO, string header, int dpadValue, int dpadCheck, int diff, string[] props, string[] names) {
			// header
			EditorGUILayout.Space();
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);

            int l = dpadValue == dpadCheck ? props.Length : props.Length - diff;
            for (int i = 0; i < l; i++) {
				SerializedProperty prop = profileSO.FindProperty(props[i]);
				prop.intValue = EditorGUILayout.Popup(prop.displayName, prop.intValue, names);
			}
		}
    }
}