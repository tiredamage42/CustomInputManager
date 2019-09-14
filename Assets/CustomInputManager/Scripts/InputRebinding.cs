using System.Collections;
using UnityEngine;

namespace CustomInputManager {
    public static class InputRebinding
    {
        
        public static void SaveRebinds () {
            InputManager.SaveCustomBindings();
        }
    
        public static bool OnStartRebind(InputBinding inputBinding, bool changingPositiveDigitalAxis, System.Action onStopScan) {
			if (InputManager.IsScanning) {
                return false;
            }
            InputManager.m_instance.StartCoroutine(StartInputScanDelayedNegativeOrDefault(inputBinding, changingPositiveDigitalAxis, onStopScan));
            return true;
		}
		
        static bool changingPositive;
        static InputBinding inputBinding;

        static IEnumerator StartInputScanDelayedNegativeOrDefault(InputBinding inputBinding, bool changingPositiveDigitalAxis, System.Action onStopScan)
		{
			
			yield return null; // delay before scanning
            if (inputBinding.Type == InputType.MouseAxis) {
                Debug.LogError("Error, cant rebind Mouse Axes...");
            }
            InputRebinding.inputBinding = inputBinding;

            if (inputBinding.Type == InputType.DigitalAxis) {
                InputRebinding.changingPositive = changingPositiveDigitalAxis;
                InputManager.StartInputScan(ScanFlags.Key, HandleKeyScan, onStopScan);	
            }
            if (inputBinding.Type == InputType.GamepadAxis) {
                InputRebinding.changingPositive = true;
                InputManager.StartInputScan(ScanFlags.JoystickAxis, HandleJoystickAxisScan, onStopScan);
            }
            if (inputBinding.Type == InputType.KeyButton) {
                InputRebinding.changingPositive = true;
                InputManager.StartInputScan(ScanFlags.Key, HandleKeyScan, onStopScan);
            }
            if (inputBinding.Type == InputType.GamepadButton) {
                InputRebinding.changingPositive = true;
                ScanFlags flags = ScanFlags.JoystickButton;
				flags |= ScanFlags.JoystickAxis;
				InputManager.StartInputScan(flags, HandleJoystickButtonScan, onStopScan);	
            }
            if (inputBinding.Type == InputType.GamepadAnalogButton) {
                InputRebinding.changingPositive = true;
                ScanFlags flags = ScanFlags.JoystickButton;
				flags |= ScanFlags.JoystickAxis;
				InputManager.StartInputScan(flags, HandleJoystickButtonScan, onStopScan);	
            }	
		}

		//	When you return false you tell the InputManager that it should keep scaning for other keys
		static bool HandleKeyScan(ScanResult result)
		{
			//	If the key is KeyCode.Backspace clear the current binding
			KeyCode Key = (result.keyCode == KeyCode.Backspace) ? KeyCode.None : result.keyCode;
			if(changingPositive)
				inputBinding.Positive = Key;
			else
				inputBinding.Negative = Key;
			return true;
		
		}
				
		//	When you return false you tell the InputManager that it should keep scaning for other keys
		static bool HandleJoystickButtonScan(ScanResult result)
		{
			
			if(result.ScanFlags == ScanFlags.JoystickButton)
			{
				if (result.gamepadButton != GamepadButton.None)
				{
					inputBinding.Type = InputType.GamepadButton;
					inputBinding.GamepadButton = result.gamepadButton;
					return true;
				}
			}
			else
			{
				if(result.gamepadAxis != GamepadAxis.None)
				{
					inputBinding.Type = InputType.GamepadAnalogButton;
					inputBinding.useNegativeAxisForButton = result.axisValue < 0.0f;
					inputBinding.GamepadAxis = result.gamepadAxis;
					return true;
				}
			}
			return false;
		}
		//	When you return false you tell the InputManager that it should keep scaning for other keys
		static bool HandleJoystickAxisScan(ScanResult result)
		{
			if (result.gamepadAxis != GamepadAxis.None) {
				inputBinding.GamepadAxis = result.gamepadAxis;
				return true;
			}
			return false;
		}	
    }
}
