
using UnityEngine;
using UnityEditor;
using System;
namespace CustomEditorTools.Modules {
    
    public class ModulesWindow : EditorWindow
    {
        [MenuItem(ProjectTools.defaultMenuItemSection + "Modules %#m", false, ProjectTools.defaultMenuItemPriority)]
		static void OpenWindow () {
            EditorWindowTools.OpenWindowNextToInspector<ModulesWindow>("Modules");
		}

        public ImportModules importModules = new ImportModules();
        public ExportModules exportModules = new ExportModules();
        // Func<bool>[] guiCallbacks;
        int chosenTab;
        
        void OnEnable () {
            // guiCallbacks = new Func<bool>[] { importModules.OnGUI, exportModules.OnGUI };
            Initialization();
        }
        void Initialization () {
            exportModules.InitializeModuleExporting();
            importModules.InitializeModuleImporting(exportModules.projectSpecifierModuleName);
        }
        void OnGUI () {
            GUITools.Space(3);
            
            GUI.color = GUITools.greenColor;
            if (GUILayout.Button("Refresh Modules Window")) Initialization();
            GUI.color = Color.white;
            
            GUITools.Space(2);
            
            chosenTab = GUILayout.Toolbar(chosenTab, new string[] { "Import", "Export" });
            
            GUITools.Space(2);

            if (chosenTab == 0) {
                if (importModules.OnGUI(exportModules.projectSpecifierModule)) {
                    Initialization();
                }
            }
            else {
                if (exportModules.OnGUI()) {
                    Initialization();
                }
            }
            
            // if (guiCallbacks[chosenTab]())
            //     Initialization();
        }
    }    
}