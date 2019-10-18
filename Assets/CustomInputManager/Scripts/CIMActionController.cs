using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityTools.EditorTools;
using UnityTools;
namespace CustomInputManager {

    public class CIMActionController : ActionsInterfaceController
    {
        

        
        public string MouseX = "Mouse X";
        public string MouseY = "Mouse Y";

        [NeatArray] public NeatStringArray actionButtons = new NeatStringArray(
            new string[] {
                "Run", "Jump", "Crouch", 
                "Interact", "Reload", "Fire", "Aim", 
                "Secondary Fire", "QuickInventory", "Pause", "Inventory", 
                "Submit", "Cancel",
                "FreeCamUp", "FreeCamDown"
            }
        );
        [NeatArray] public NeatStringArray axisNames = new NeatStringArray(
            new string[] {
                "Horizontal", "Vertical", "Mouse X", "Mouse Y", "Throttle", "Brake"
            }
        );

        public override string ConstructTooltip () {
            string r = GetType().Name + "\n\nActions:\n";
            for (int i = 0; i < actionButtons.Length; i++) r += i.ToString() + ": " + actionButtons[i] + "\n";
            r += "Axes:\n";
            for (int i = 0; i < axisNames.Length; i++) r += i.ToString() + ": " + axisNames[i] + "\n";
            return r;
        }
        
        int _MouseX, _MouseY;
        int[] _actions, _axes;
        void OnEnable () {
            _MouseX = CustomInput.Name2Key( MouseX );
            _MouseY = CustomInput.Name2Key( MouseY );

            _actions = new int[actionButtons.Length];
            for (int i = 0; i < actionButtons.Length; i++) _actions[i] = CustomInput.Name2Key( actionButtons[i] );
            
            _axes = new int[axisNames.Length];
            for (int i = 0; i < axisNames.Length; i++) _axes[i] = CustomInput.Name2Key( axisNames[i] );
            
        }

        protected override Vector2 GetMousePos (int controller) {
            return new Vector2( CustomInput.GetAxis( _MouseX ), CustomInput.GetAxis( _MouseY ) ) ;
        }
        
        protected override bool GetActionDown (int action, int controller) {
            if (!CheckActionIndex("Action", action, _actions.Length)) return false;
            return CustomInput.GetButtonDown(_actions[action]);
        }
        protected override bool GetAction (int action, int controller) {
            if (!CheckActionIndex("Action", action, _actions.Length)) return false;
            return CustomInput.GetButton(_actions[action]);
        }
        protected override bool GetActionUp (int action, int controller) {
            if (!CheckActionIndex("Action", action, _actions.Length)) return false;
            return CustomInput.GetButtonUp(_actions[action]);
        }
        protected override float GetAxis (int axis, int controller) {
            if (!CheckActionIndex("Axis", axis, _axes.Length)) return 0;
            return CustomInput.GetAxis(_axes[axis]);
        }

        bool CheckActionIndex (string type, int action, int length) {
            if (action < 0 || action >= length) {
                Debug.LogWarning(type + ": " + action + " is out of range [" + length + "]");
                return false;
            }
            return true;
        }        
    }
}
