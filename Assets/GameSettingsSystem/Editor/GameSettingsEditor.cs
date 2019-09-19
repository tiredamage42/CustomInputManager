using UnityEngine;
using UnityEditor;
using CustomEditorTools;

namespace GameSettingsSystem.Editor {
    /*
        update the game settings holder every time the project changes, to keep track of all the
        game settings objects in the project, the holder is located in a Resources folder, so we dont
        have to worry about having references to any of our game settings objects during builds
    */
    [InitializeOnLoad] public class GameSettingsEditor {
        static GameSettingsEditor () {
            EditorApplication.projectChanged += CheckForNewGameSettingsObjects;

            CheckForNewGameSettingsObjects();
        }
        static void CheckForNewGameSettingsObjects () {
            // Debug.Log("should chekc");
            // dont update when in play mode or if our game settings object is missing
            if (Application.isPlaying || GameSettings.settings == null) return;
            // Debug.Log("checking");
            
            // update the array of all game settings objects in the project
            GameSettings.settings = AssetTools.FindAssetsByType<GameSettingsObject>(logToConsole: true).ToArray();
            EditorUtility.SetDirty(GameSettings.gameSettings);
        }
    }
}