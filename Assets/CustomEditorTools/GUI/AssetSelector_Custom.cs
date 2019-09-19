using System.Collections.Generic;
using UnityEngine;

namespace CustomEditorTools {
    /*
        custom template for extending asset selection fields, to only show certain
        assets / change display names
    */    
    public class CustomAssetSelectionAttribute : AssetSelectionAttribute
    {
        public override List<AssetSelectorElement> OnAssetsLoaded(List<AssetSelectorElement> originals) {
            for (int i = 0; i < originals.Count; i++) 
                originals[i].displayName = originals[i].displayName + " !!!";

            return originals;
        }
        public CustomAssetSelectionAttribute (  ) : base(typeof(float)) { }
    }

    
#if UNITY_EDITOR
    [UnityEditor.CustomPropertyDrawer(typeof(CustomAssetSelectionAttribute))] 
    public class CustomAssetSelectionDrawer : AssetSelectionDrawer { }   
#endif
}
