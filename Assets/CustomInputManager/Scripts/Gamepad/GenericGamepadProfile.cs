using UnityEngine;
using System.Collections.Generic;

namespace CustomInputManager
{
    [CreateAssetMenu(fileName = "New Gamepad Profile", menuName = "CustomInputManager/Input Manager/Gamepad Profile")]
    public class GenericGamepadProfile : ScriptableObject
    {
        public string unityJoystickName;
        public List<GamePadPossiblePlatform> platforms = new List<GamePadPossiblePlatform>();

        [SerializeField] private GamepadDPadType m_dpadType = GamepadDPadType.Axis;
        
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_leftStickButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_rightStickButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_leftBumperButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_rightBumperButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_dpadUpButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_dpadDownButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_dpadLeftButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_dpadRightButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_backButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_startButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_actionTopButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_actionBottomButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_actionLeftButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_actionRightButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_leftStickXAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_leftStickYAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_rightStickXAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_rightStickYAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_dpadXAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_dpadYAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_leftTriggerAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_rightTriggerAxis = 0;

        public GamepadDPadType DPadType { get { return m_dpadType; } }
        public int LeftStickButton { get { return m_leftStickButton; } }
        public int RightStickButton { get { return m_rightStickButton; } }
        public int LeftBumperButton { get { return m_leftBumperButton; } }
        public int RightBumperButton { get { return m_rightBumperButton; } }
        public int DPadUpButton { get { return m_dpadUpButton; } }
        public int DPadDownButton { get { return m_dpadDownButton; } }
        public int DPadLeftButton { get { return m_dpadLeftButton; } }
        public int DPadRightButton { get { return m_dpadRightButton; } }
        public int BackButton { get { return m_backButton; } }
        public int StartButton { get { return m_startButton; } }
        public int ActionTopButton { get { return m_actionTopButton; } }
        public int ActionBottomButton { get { return m_actionBottomButton; } }
        public int ActionLeftButton { get { return m_actionLeftButton; } }
        public int ActionRightButton { get { return m_actionRightButton; } }
        public int LeftStickXAxis { get { return m_leftStickXAxis; } }
        public int LeftStickYAxis { get { return m_leftStickYAxis; } }
        public int RightStickXAxis { get { return m_rightStickXAxis; } }
        public int RightStickYAxis { get { return m_rightStickYAxis; } }
        public int DPadXAxis { get { return m_dpadXAxis; } }
        public int DPadYAxis { get { return m_dpadYAxis; } }
        public int LeftTriggerAxis { get { return m_leftTriggerAxis; } }
        public int RightTriggerAxis { get { return m_rightTriggerAxis; } }
    }
}
