using System;
using UnityEngine;
using UnityEditor;

using UnityTools.EditorTools;
namespace CustomInputManager.Editor {
    
    public class InputManagerWindow : EditorWindow {

        [MenuItem(ProjectTools.defaultMenuItemSection + "Input Manager", false, ProjectTools.defaultMenuItemPriority)]
		static void OpenWindow () {
            EditorWindowTools.OpenWindowNextToInspector<InputManagerWindow>("Input Manager");
		}

        public InputSettingsTab inputSettings = new InputSettingsTab();
        public ControlSchemesTab controlSchemes = new ControlSchemesTab();
        public GamepadProfilesTab gamepadProfilesWindow = new GamepadProfilesTab();
        HierarchyGUI hierarchyGUI = new HierarchyGUI();
        
        public static InputManagerWindow instance;
        const float tabsOffYOffset = 45;
        
        bool wasDiabled;
        int selectedTab;
        Action<Rect>[] tabGUIs { get { return new Action<Rect>[] { inputSettings.OnGUI, controlSchemes.OnGUI, gamepadProfilesWindow.OnGUI }; } }
        public static Vector2 m_mainPanelScrollPos = Vector2.zero;

        void DisposeManagerWindow ( bool repeat) {
            instance = null;
            controlSchemes.Dispose( repeat);
            hierarchyGUI.Dispose();
        }
        
        public void OnPlayStateChanged (PlayModeStateChange state) {
            controlSchemes.OnPlayStateChanged(state);
            gamepadProfilesWindow.OnPlayStateChanged(state);
        }
        void OnEnable () {
            instance = this;
            
            hierarchyGUI.OnEnable();
            controlSchemes.OnEnable(hierarchyGUI);
            gamepadProfilesWindow.OnEnable(hierarchyGUI);
            EditorApplication.playModeStateChanged += OnPlayStateChanged;
            
            ConvertUnityInputManager.ShowStartupWarning();
        }

        void OnDisable () {
            EditorApplication.playModeStateChanged -= OnPlayStateChanged;
            wasDiabled = true;
            DisposeManagerWindow(false);
        }
        void OnDestroy () {
            DisposeManagerWindow(wasDiabled);
        }
        void OnGUI() {
            GUITools.Space(3);
            selectedTab = GUILayout.Toolbar (selectedTab, new string[] {"Settings", "Schemes", "Gamepad Profiles"});
            tabGUIs[selectedTab] (new Rect(0, tabsOffYOffset, position.width, position.height - tabsOffYOffset));
		}
    }
}
