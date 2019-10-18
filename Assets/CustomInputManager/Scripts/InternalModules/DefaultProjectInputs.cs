using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

/*
    Handle the developer specified project inputs
    
    (default game input schemes)
*/

namespace CustomInputManager.Internal {
    public static class DefaultProjectInputs
    {
        const string XMLName = "DefaultProjectInputsXML";
        static TextAsset _xmlAsset {
            get {
                if (InputManager.instance != null)
                    return InputManager.instance.defaultProjectInputs;
                return null;
            }
            set {
                if (InputManager.instance != null)
                    InputManager.instance.defaultProjectInputs = value;
            }
        }
        static TextAsset xmlAsset {
            get {

                #if UNITY_EDITOR
                if (_xmlAsset == null) {
                    InitializeXMLAsset();
                }
                #endif   
                return _xmlAsset;
            }
        }
        
        public static ControlScheme LoadDefaultScheme (string controlSchemeName) {
            ControlScheme r = null;
            using(StringReader reader = new StringReader(xmlAsset.text)) {
				r = new InputLoaderXML(reader).Load(controlSchemeName);
			}
            return r;
        }
    
        public static List<ControlScheme> LoadDefaultSchemes () {
            if (xmlAsset == null)
                return null;
            
            List<ControlScheme> r = null;
            using(StringReader reader = new StringReader(xmlAsset.text)) {
                r = new InputLoaderXML(reader).Load();
            }
            return r;
        }
           

#if UNITY_EDITOR
        static void InitializeXMLAsset () {
            if (InputManager.instance != null)
                return;

            if (_xmlAsset != null)
                return;
            
            if (Application.isPlaying) {
                Debug.LogError("Default Project Inputs XML not found! Open up the Custom Input Manager Window in Editor Mode to initialize it.");
            }
            else {
                _xmlAsset = CreateNewXML ();
            }
        }
        static TextAsset CreateNewXML () {
            SaveSchemesAsDefault("Creating", new List<ControlScheme>());
            return AssetDatabase.LoadAssetAtPath<TextAsset>(InputManager.fullResourcesDirectory + XMLName + ".xml");
        }
        public static void SaveSchemesAsDefault (string msg, List<ControlScheme> controlSchemes) {
            if (InputManager.instance == null)
                return;

            if (controlSchemes == null)
                return;
            
            string path = InputManager.fullResourcesDirectory + XMLName + ".xml";
            
            Debug.Log(msg + " Default Project Inputs XML at path :: " + path);
            new InputSaverXML(path).Save(controlSchemes);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }
#endif

    }
}
