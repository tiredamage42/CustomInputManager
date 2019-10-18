using UnityEngine;

using UnityTools.EditorTools;
namespace CustomInputManager.Internal
{

    [System.Serializable] public class NeatGamepadPlatformArray : NeatArrayWrapper<GamePadPossiblePlatform> {  }
    [CreateAssetMenu(fileName = "New Gamepad Profile", menuName = "Custom Input Manager/Gamepad Profile")]
    public class GenericGamepadProfile : ScriptableObject
    {
        [Header("Names")]
        [NeatArray] public NeatStringArray joystickAliases;
        
        [Header("Platforms")]
        [NeatArray] public NeatGamepadPlatformArray platforms;
        // public GamePadPossiblePlatform[] platforms;


        [Header("Settings")]
        public GamepadDPadType m_dpadType = GamepadDPadType.Axis;
        
        [Header("Buttons")]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_leftStickButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_rightStickButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_leftBumperButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_rightBumperButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_dpadUpButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_dpadDownButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_dpadLeftButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_dpadRightButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_backButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_startButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_actionTopButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_actionBottomButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_actionLeftButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] public int m_actionRightButton = 0;

        [Header("Axes")]
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] public int m_leftStickXAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] public int m_leftStickYAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] public int m_rightStickXAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] public int m_rightStickYAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] public int m_dpadXAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] public int m_dpadYAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] public int m_leftTriggerAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] public int m_rightTriggerAxis = 0;

    }
}
