﻿using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace GameSettingsSystem {

    /*
        game settings holder keeps track of all the
        game settings objects in the project, the holder is located in a Resources folder, so we dont
        have to worry about having references to any of our game settings objects during builds
    */
    [CreateAssetMenu(menuName="Game Settings System/Replacement Game Settings Object", fileName=GameSettings.fileName)]
    public class GameSettings : ScriptableObject
    {
        const string resourcesDirectory = "GameSettingsSystem/";
        const string fileName = "GameSettings";
        static GameSettings _gameSettings;
        public static GameSettings gameSettings {
            get {
                if (_gameSettings == null) {
                    _gameSettings = Resources.Load<GameSettings>(resourcesDirectory + fileName);
                    
                    if (_gameSettings == null) {
                        Debug.LogError(string.Format("Couldnt find GameSettings object in the project, create a replacement at path: '{0}'.\nRight-click -> Create -> Game Settings System -> Replacement Game Settings Object", "Resources/" + resourcesDirectory + fileName));
                    }
                    else {
                        if (Application.isPlaying) {
                            InitializeRuntimeSettingsLookups ();
                        }
                    }
                }
                return _gameSettings;
            }
        }
        public static GameSettingsObject[] settings {
            get {
                if (gameSettings == null)
                    return null;
                
                return gameSettings._settings;
            }
            set {
                if (Application.isPlaying) {
                    Debug.LogWarning("Cant modify settings holder settings array at runtime");
                    return;
                }
                gameSettings._settings = value;
            }
        }

        // the actual settings objects
        [SerializeField] GameSettingsObject[] _settings;
        

        // use a dictionary to lookups at run time for performance
        static Dictionary<Type, Dictionary<string, GameSettingsObject>> typesLookups;

        static void InitializeRuntimeSettingsLookups () {

            typesLookups = new Dictionary<Type, Dictionary<string, GameSettingsObject>>();
            for (int i = 0; i < settings.Length; i++) {
                GameSettingsObject gs = settings[i];
                Type t = gs.GetType();
                
                Dictionary<string, GameSettingsObject> namesLookup;
                if (typesLookups.TryGetValue(t, out namesLookup)) {
                    namesLookup.Add(gs.name, gs);
                }
                else {
                    typesLookups.Add(t, new Dictionary<string, GameSettingsObject>() { {gs.name, gs } });
                }
            }
        }

        /*
            if the name parameter is null, we jsut return the first of type(T)
            we find
        */
        public static T GetSettings<T> (string name=null) where T : GameSettingsObject {
            if (settings == null)
                return null;

            // during editor, just search by for loop
            if (!Application.isPlaying) {
                for (int i = 0; i < settings.Length; i++) {
                    GameSettingsObject gs = settings[i];
                    T t = gs as T;
                    if (t != null) {
                        if (name == null || t.name == name) {
                            return t;
                        }
                    }
                }
            }
            else {
                Dictionary<string, GameSettingsObject> namesLookup;
                if (typesLookups.TryGetValue(typeof(T), out namesLookup)) {

                    if (name == null) {
                        if (namesLookup.Count > 1) 
                            Debug.LogWarning("Multiple Settings Objects of type " + typeof(T).FullName + " only returning first. Use 'GetSettings<T> (string name)' for more specific object");
                        return namesLookup.First().Value as T;
                    }
                    else {
                        GameSettingsObject gs;
                        if (namesLookup.TryGetValue(name, out gs)) {
                            return gs as T;
                        }
                    }
                }
            }

            Debug.LogError("Couldnt find Settings Object of type " + typeof(T).FullName + " named '" + name + "'.");
            return null;
        }
    }

    // base class for game settings to keep track of within the framework
    public abstract class GameSettingsObject : ScriptableObject {

        // only used for editor
        public virtual string DisplayName () {
            return name;
        }
        // only used for editor
        public virtual bool ShowInGameSettingsWindow () {
            return true;
        }
    }
}

        
