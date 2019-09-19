using UnityEngine;
using UnityEditor;
    
namespace CustomEditorTools {
    [System.Serializable] public class SceneAssetField { [AssetSelection(typeof(SceneAsset))] public SceneAsset scene; }
    
    [CustomPropertyDrawer(typeof(SceneAssetField))]
    public class SceneAssetFieldDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("scene"), GUIContent.none, true);
        }
    }
    
    [System.Serializable] public class SceneList : NeatArrayWrapper<SceneAssetField> {  }

    [System.Serializable] public class BuildWindow : EditorWindow
    {
        //% (ctrl on Windows, cmd on macOS), # (shift), & (alt).

        [MenuItem(ProjectTools.defaultMenuItemSection + "Build Window %#b", false, ProjectTools.defaultMenuItemPriority)]
		static void OpenWindow () {
            EditorWindowTools.OpenWindowNextToInspector<BuildWindow>("Build");
		}

        [NeatArray] public SceneList scenes;
        SerializedObject windowSO;
        int topSpaces = 5;
        

        SerializedProperty scenesList { get { return windowSO.FindProperty("scenes").FindPropertyRelative("list"); } }
        void UpdateToReflectSettings() {

            EditorBuildSettingsScene[] settingsScenes = EditorBuildSettings.scenes;
            int l = settingsScenes.Length;

            SerializedProperty scenes = scenesList;
            for (int i = 0; i < l; i++) {
                if (i >= scenes.arraySize) 
                    scenes.InsertArrayElementAtIndex(scenes.arraySize);

                SerializedProperty scene = scenes.GetArrayElementAtIndex(i).FindPropertyRelative("scene");
                scene.objectReferenceValue = AssetDatabase.LoadAssetAtPath(settingsScenes[i].path, typeof(SceneAsset));
            }
        }


        void UpdateSettings() {
            SerializedProperty scenes = scenesList;
            
            EditorBuildSettingsScene[] settingsScenes = new EditorBuildSettingsScene[scenes.arraySize];
            
            for (int i = 0; i < scenes.arraySize; i++) {
                settingsScenes[i] = new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(scenes.GetArrayElementAtIndex(i).FindPropertyRelative("scene").objectReferenceValue), true);
            }

            EditorBuildSettings.scenes = settingsScenes;
        }

        void OnGUI () {

            if (windowSO == null) windowSO = new SerializedObject(this);
            
            for (int i = 0; i < topSpaces; i++) EditorGUILayout.Space();

            EditorGUILayout.LabelField("Scenes to build:", EditorStyles.boldLabel);

            UpdateToReflectSettings ();
            windowSO.ApplyModifiedProperties();

            EditorGUI.BeginChangeCheck();
            
            windowSO.Update();
            
            EditorGUILayout.PropertyField(windowSO.FindProperty("scenes"), true);
            
            if (EditorGUI.EndChangeCheck()) {
                UpdateSettings();
            }

            windowSO.ApplyModifiedProperties();
        }
    }    
}