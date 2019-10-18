using UnityEngine;
using UnityEditor;

using CustomInputManager.Internal;

using System.Collections.Generic;

using UnityTools.EditorTools;

namespace CustomInputManager.Editor {

    [System.Serializable] public class GamepadProfilesTab
    {
		GamepadProfilesEditor profilesEditor = new GamepadProfilesEditor();
		[SerializeField] GamepadTestSceneEditor testSceneEditor = new GamepadTestSceneEditor();
		List<GenericGamepadProfile> gamepadProfiles { get { return InputManager.instance._gamepads; } }
		SerializedObject profileSO;
        
		HierarchyGUI hierarchyGUI;
		public void OnEnable (HierarchyGUI hierarchyGUI) {
			this.hierarchyGUI = hierarchyGUI;
		}

        void ReloadProfilesAndRepaint () {
			hierarchyGUI.ResetSelections();
			InputManagerWindow.instance.Repaint();
		}

		
		void CreateEditMenu(Rect position)
		{
			GenericMenu editMenu = new GenericMenu();
			editMenu.AddItem(new GUIContent("New Gamepad Profile"), false, HandleEditMenuOption, 0);
			
            editMenu.AddSeparator("");

			if (hierarchyGUI.selections[0] != -1)
				editMenu.AddItem(new GUIContent("Duplicate"), false, HandleEditMenuOption, 1);
			else
				editMenu.AddDisabledItem(new GUIContent("Duplicate"));

			if (hierarchyGUI.selections[0] != -1)			
				editMenu.AddItem(new GUIContent("Delete"), false, HandleEditMenuOption, 2);
			else
				editMenu.AddDisabledItem(new GUIContent("Delete"));

			editMenu.DropDown(position);
		}
		void CreateControlSchemeContextMenu(Rect position)
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleEditMenuOption, 1);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleEditMenuOption, 2);
			contextMenu.DropDown(position);
		}

		void HandleEditMenuOption(object arg)
		{
			switch((int)arg)
			{
				case 0: profilesEditor.CreateNewGamepadProfile("NewGamepadProfile"); ReloadProfilesAndRepaint(); break;
				case 1: profilesEditor.DuplicateProfile(gamepadProfiles[hierarchyGUI.selections[0]]); ReloadProfilesAndRepaint(); break;
				case 2: profilesEditor.DeleteProfile(gamepadProfiles[hierarchyGUI.selections[0]]); ReloadProfilesAndRepaint(); break;
			}
		}



		List<HieararchyGUIElement> BuildHierarchyElementsList () {
			List<HieararchyGUIElement> r = new List<HieararchyGUIElement>();
			for (int i = 0; i < gamepadProfiles.Count; i++) {
				r.Add(new HieararchyGUIElement(gamepadProfiles[i].name, null, CreateControlSchemeContextMenu));
			}
			return r;
		}

		
		public void OnGUI(Rect pos)
		{
		
			float testButtonHeight = 30; 
			if (Application.isPlaying) testButtonHeight += testSceneEditor.lastScene != null ? 90 : 45;
			

			bool clicked = false;
			clicked = hierarchyGUI.Draw(InputManagerWindow.instance, new Rect(pos.x, pos.y + testButtonHeight, pos.width, pos.height - testButtonHeight), false, BuildHierarchyElementsList(), DrawSelected, CreateEditMenu);
			
			if (clicked) OnNewProfileSelection(gamepadProfiles[hierarchyGUI.selections[0]]);
			else {

				GUILayout.BeginArea(new Rect(pos.x, pos.y, pos.width, testButtonHeight));
				GUITools.Space();

				if (Application.isPlaying) {
					if (testSceneEditor.lastScene != null) {
						EditorGUILayout.HelpBox("Do not close the Input Manager window while in the gamepad testing scene.\n\nOr you will not be taken back to the original scene you were working on...", MessageType.Warning);
					}
					EditorGUILayout.HelpBox("[Play Mode]: Any new Profiles will be active the next time you enter play mode.", MessageType.Info);
				}
				else {
					if (GUILayout.Button("Start Gamepad Inputs Testing Scene")) testSceneEditor.StartTestScene();
				}
				
				GUILayout.EndArea();	
			}
			
		}




        public void OnPlayStateChanged (PlayModeStateChange state) {
			testSceneEditor.OnPlayStateChanged(state);
        }
        
		void DrawSelected(Rect position)
		{
			if (hierarchyGUI.selections[0] < 0)
				return;
			
			position.x += 5;
			position.width -= 5;
			position.y += 5;
			position.height -=5;

			GUILayout.BeginArea(position);
			
			InputManagerWindow.m_mainPanelScrollPos = EditorGUILayout.BeginScrollView( InputManagerWindow.m_mainPanelScrollPos);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Name", GUILayout.Width(50));
            string oldName = gamepadProfiles[hierarchyGUI.selections[0]].name;
			string newName = EditorGUILayout.DelayedTextField("", oldName);
			EditorGUILayout.EndHorizontal();
            if (newName != oldName) profilesEditor.RenameProfile(gamepadProfiles[hierarchyGUI.selections[0]], newName);

            
			profilesEditor.DrawProfile(profileSO);

			GUITools.Space(3);

			EditorGUILayout.EndScrollView();
			
			GUILayout.EndArea();
		}

		void OnNewProfileSelection(GenericGamepadProfile profile) {
            profileSO = new SerializedObject(profile);
        }
	}
}

