using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CustomInputManager
{

    public enum GamepadAxis
	{
		LeftThumbstickX, LeftThumbstickY,
		RightThumbstickX, RightThumbstickY,
		DPadX, DPadY,
		LeftTrigger, RightTrigger,
		None
	}

    public enum GamepadButton
	{
		LeftStick, RightStick,
		LeftBumper, RightBumper,
		DPadUp, DPadDown, DPadLeft, DPadRight,
		Back, Start,
		ActionBottom, ActionRight, ActionLeft, ActionTop,
		None
	}

    public enum GamepadDPadType { Axis = 0, Button }

    public enum GamePadPossiblePlatform { Linux, OSX, Windows, PS4, XboxOne };
    
    [System.Serializable] public class GamepadHandler
    {   

        static bool CurrentPlatformInGamepadPlatforms (List<GamePadPossiblePlatform> platforms) {
            RuntimePlatform platform = Application.platform;
            if (platform == RuntimePlatform.LinuxEditor || platform == RuntimePlatform.LinuxPlayer) 
                return platforms.Contains(GamePadPossiblePlatform.Linux);
            else if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer) 
                return platforms.Contains(GamePadPossiblePlatform.OSX);
            else if (platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsPlayer) 
                return platforms.Contains(GamePadPossiblePlatform.Windows);
            else if (platform == RuntimePlatform.PS4) 
                return platforms.Contains(GamePadPossiblePlatform.PS4);
            else if (platform == RuntimePlatform.XboxOne) 
                return platforms.Contains(GamePadPossiblePlatform.XboxOne);

            Debug.LogError("Platform: " + platform + " not supported by gamepads via Custom Input Manager");
            return false;
        }

        static GenericGamepadProfile GetProfileForJoystickName(string joystickName, List<GenericGamepadProfile> originalList) {
            for (int i = 0; i < originalList.Count; i++) {
                if (joystickName == originalList[i].unityJoystickName) {
                    if (CurrentPlatformInGamepadPlatforms(originalList[i].platforms)) {
                        return originalList[i];
                    }
                }
            }
            Debug.LogError("Cant Find Profile for joystick: " + joystickName);
            return null;
        }


        struct DPadState
        {
            public Vector2 axes;
            public ButtonState Up, Down, Left, Right;
            public static DPadState Empty => new DPadState() {
                Up = ButtonState.Released,
                Down = ButtonState.Released,
                Right = ButtonState.Released,
                Left = ButtonState.Released
            };

            public void UpdateButtonStates (bool upPressed, bool downPressed, bool leftPressed, bool rightPressed) {
                Up = GetNewDPadButtonState(upPressed, Up);
                Down = GetNewDPadButtonState(downPressed, Down);
                Left = GetNewDPadButtonState(leftPressed, Left);
                Right = GetNewDPadButtonState(rightPressed, Right);
            }

            static ButtonState GetNewDPadButtonState(bool isPressed, ButtonState oldState)
            {
                ButtonState newState = isPressed ? ButtonState.Pressed : ButtonState.Released;
                if (oldState == ButtonState.Pressed || oldState == ButtonState.JustPressed)
                    newState = isPressed ? ButtonState.Pressed : ButtonState.JustReleased;
                else if (oldState == ButtonState.Released || oldState == ButtonState.JustReleased)
                    newState = isPressed ? ButtonState.JustPressed : ButtonState.Released;
                return newState;
            }
        }

        public List<GenericGamepadProfile> allGamepadProfiles = new List<GenericGamepadProfile>();
        GenericGamepadProfile[] gamepadProfilesPerGamepad;
        
        [Tooltip("At what interval(in sec) to check how many joysticks are connected.")]
        [SerializeField] private float m_joystickCheckFrequency = 1.0f;
        [SerializeField] private float m_dpadGravity = 3.0f;
        [SerializeField] private float m_dpadSensitivity = 3.0f;
        [SerializeField] private bool m_dpadSnap = true;

        DPadState[] m_dpadState;
        string[] m_axisNameLookupTable;
        
        int maxJoysticks;

        public void Awake(InputManager inputManager, int maxJoysticks)
        {
            this.maxJoysticks = maxJoysticks;

            gamepadProfilesPerGamepad = new GenericGamepadProfile[maxJoysticks];
            m_dpadState = new DPadState[maxJoysticks];
            gamepadNames = new string[maxJoysticks];

            for (int i = 0; i < maxJoysticks; i++) {
                m_dpadState[i] = DPadState.Empty;
            }

            GenerateAxisNameLookupTable();
            inputManager.StartCoroutine(CheckForGamepads());
        }

        string[] gamepadNames;

        public string GamepadName (int gamepad) {
            return gamepadNames[gamepad];
        }

        private IEnumerator CheckForGamepads()
        {
            while(true)
            {
                string[] joystickNames = InputManager.GetJoystickNames();

                int ln = joystickNames.Length;
                for(int i = 0; i < maxJoysticks; i++) {
                    bool connected = ln > i && !string.IsNullOrEmpty(joystickNames[i]);

                    gamepadNames[i] = connected ? joystickNames[i] : "Not Connected";

                    if (connected) {
                        if (gamepadProfilesPerGamepad[i] == null){
                            gamepadProfilesPerGamepad[i] = GetProfileForJoystickName(joystickNames[i], allGamepadProfiles);
                            if (gamepadProfilesPerGamepad[i] == null) {
                                gamepadNames[i] = joystickNames[i] + " [No Profile]";
                            }
                            else {
                                Debug.Log("Assigned profile " + gamepadProfilesPerGamepad[i].name + " for joystick: " + joystickNames[i]);
                            }
                        }
                    }
                    else {
                        gamepadProfilesPerGamepad[i] = null;
                    }
                }
                
                yield return new WaitForSecondsRealtime(m_joystickCheckFrequency);
            }
        }

        void GenerateAxisNameLookupTable() {
            string template = "joy_{0}_axis_{1}";
            m_axisNameLookupTable = new string[maxJoysticks * InputBinding.MAX_JOYSTICK_AXES];
            for(int j = 0; j < maxJoysticks; j++) {
                for(int a = 0; a < InputBinding.MAX_JOYSTICK_AXES; a++) {
                    m_axisNameLookupTable[j * InputBinding.MAX_JOYSTICK_AXES + a] = string.Format(template, j, a);
                }
            }
        }

        public bool GamepadAvailable (int gamepad, out GenericGamepadProfile profile) {
            if (!CheckForGamepadProfile(gamepad, out profile)) return false;
            return true;
        }

        public void OnUpdate(float deltaTime)
        {
            for(int i = 0; i < maxJoysticks; i++)
            {
                GenericGamepadProfile profile;
                if (!GamepadAvailable(i, out profile)) continue;
                
                if(profile.DPadType == GamepadDPadType.Button)
                {
                    // mimic axis values
                    UpdateDPadAxis(i, deltaTime, 0, profile.DPadRightButton, profile.DPadLeftButton);
                    UpdateDPadAxis(i, deltaTime, 1, profile.DPadUpButton, profile.DPadDownButton);
                }
                else
                {
                    // mimic button values
                    UpdateDPadButton(i, profile);
                }
                
            }
        }

        private void UpdateDPadAxis(int gamepad, float deltaTime, int axis, int posButton, int negButton)
        {
            bool posPressed = GetButton(posButton, gamepad);
            bool negPressed = GetButton(negButton, gamepad);

            float ax = m_dpadState[gamepad].axes[axis];

            if(posPressed)
            {
                if(ax < InputBinding.AXIS_NEUTRAL && m_dpadSnap) ax = InputBinding.AXIS_NEUTRAL;
                ax += m_dpadSensitivity * deltaTime;
                if(ax > InputBinding.AXIS_POSITIVE) ax = InputBinding.AXIS_POSITIVE;
            }
            else if(negPressed)
            {
                if(ax > InputBinding.AXIS_NEUTRAL && m_dpadSnap) ax = InputBinding.AXIS_NEUTRAL;
                ax -= m_dpadSensitivity * deltaTime;
                if(ax < InputBinding.AXIS_NEGATIVE) ax = InputBinding.AXIS_NEGATIVE;
            }
            else
            {
                if(ax < InputBinding.AXIS_NEUTRAL)
                {
                    ax += m_dpadGravity * deltaTime;
                    if(ax > InputBinding.AXIS_NEUTRAL) ax = InputBinding.AXIS_NEUTRAL;
                }
                else if(ax > InputBinding.AXIS_NEUTRAL)
                {
                    ax -= m_dpadGravity * deltaTime;
                    if(ax < InputBinding.AXIS_NEUTRAL) ax = InputBinding.AXIS_NEUTRAL;
                }
            }

            m_dpadState[gamepad].axes[axis] = ax;    
        }

        
        static readonly float dpadThreshold = 0.9f;
        static readonly float negDpadThreshold = -dpadThreshold;
        void UpdateDPadButton(int gamepad, GenericGamepadProfile profile)
        {
            int jOffset = gamepad * InputBinding.MAX_JOYSTICK_AXES;
            float x = Input.GetAxis(m_axisNameLookupTable[jOffset + profile.DPadXAxis]);
            float y = Input.GetAxis(m_axisNameLookupTable[jOffset + profile.DPadYAxis]);
            m_dpadState[gamepad].UpdateButtonStates (y >= dpadThreshold, y <= negDpadThreshold, x <= negDpadThreshold, x >= dpadThreshold);
        }

    
        HashSet<int> checkedGamepadLTriggersForInitialization = new HashSet<int>();
        HashSet<int> checkedGamepadRTriggersForInitialization = new HashSet<int>();
        
        // xbox controller triggers on OSX initialize at 0 but have range -1, 1
        float AdjustOSXAxis (float rawAxis, int gamepad, ref HashSet<int> checkSet) {
            float adjustedAxis = 0.0f;
            bool checkedTrigger = checkSet.Contains(gamepad);
            if (!checkedTrigger) {
                if (rawAxis > -0.9f && rawAxis < -0.0001f){
                    checkSet.Add(gamepad);
                    checkedTrigger = true;
                }
            }
            if(checkedTrigger) {
                adjustedAxis = (rawAxis + 1.0f) * 0.5f;
            }
            return adjustedAxis;
        }


        public float GetAxis(GamepadAxis axis, int gamepad)
        {

            GenericGamepadProfile profile;
            if (!GamepadAvailable(gamepad, out profile)) return 0.0f;
            
            int axisID = -1;

            switch(axis)
            {
                case GamepadAxis.LeftThumbstickX:  axisID = profile.LeftStickXAxis; break;
                case GamepadAxis.LeftThumbstickY:  axisID = profile.LeftStickYAxis; break;
                case GamepadAxis.RightThumbstickX: axisID = profile.RightStickXAxis; break;
                case GamepadAxis.RightThumbstickY: axisID = profile.RightStickYAxis; break;
                
                case GamepadAxis.DPadX: 
                    if (profile.DPadType == GamepadDPadType.Button) return m_dpadState[gamepad].axes[0];
                    axisID = profile.DPadXAxis; 
                    break;
                case GamepadAxis.DPadY: 
                    if (profile.DPadType == GamepadDPadType.Button) return m_dpadState[gamepad].axes[1];
                    axisID = profile.DPadYAxis; 
                    break;
                
                case GamepadAxis.LeftTrigger:  return AdjustOSXAxis (Input.GetAxis(m_axisNameLookupTable[gamepad * InputBinding.MAX_JOYSTICK_AXES + profile.LeftTriggerAxis]), gamepad, ref checkedGamepadLTriggersForInitialization);
                case GamepadAxis.RightTrigger: return AdjustOSXAxis (Input.GetAxis(m_axisNameLookupTable[gamepad * InputBinding.MAX_JOYSTICK_AXES + profile.RightTriggerAxis]), gamepad, ref checkedGamepadRTriggersForInitialization);
            }

            return axisID >= 0 ? Input.GetAxis(m_axisNameLookupTable[gamepad * InputBinding.MAX_JOYSTICK_AXES + axisID]) : 0.0f;
        }

        public float GetAxisRaw(GamepadAxis axis, int gamepad, float deadZone)
        {
            float value = GetAxis(axis, gamepad);
            if ((value < 0 && value > -deadZone) || (value > 0 && value < deadZone) || value == 0) return 0;
            return value > 0 ? 1 : -1;
        }

        bool CheckForGamepadProfile (int gamepad, out GenericGamepadProfile profile) {
            profile = gamepadProfilesPerGamepad[gamepad];
            if (profile == null) {
                Debug.LogError("No Gamepad Profile supplied For Input Manager...");
                return false;
            }
            return true;
        }

        bool ButtonQuery (GamepadButton button, int gamepad, System.Func<int, int, bool> callback, ButtonState dpadButtonStateCheck) {
            GenericGamepadProfile profile;
            if (!GamepadAvailable(gamepad, out profile)) return false;
            
            switch(button)
            {
                case GamepadButton.LeftStick:       return callback(profile.LeftStickButton, gamepad);
                case GamepadButton.RightStick:      return callback(profile.RightStickButton, gamepad);
                case GamepadButton.LeftBumper:      return callback(profile.LeftBumperButton, gamepad);
                case GamepadButton.RightBumper:     return callback(profile.RightBumperButton, gamepad);
                
                case GamepadButton.Back:            return callback(profile.BackButton, gamepad);
                case GamepadButton.Start:           return callback(profile.StartButton, gamepad);
                case GamepadButton.ActionBottom:    return callback(profile.ActionBottomButton, gamepad);
                case GamepadButton.ActionRight:     return callback(profile.ActionRightButton, gamepad);
                case GamepadButton.ActionLeft:      return callback(profile.ActionLeftButton, gamepad);
                case GamepadButton.ActionTop:       return callback(profile.ActionTopButton, gamepad);
                
                case GamepadButton.DPadUp:          return profile.DPadType == GamepadDPadType.Button ? callback(profile.DPadUpButton, gamepad) : m_dpadState[gamepad].Up == dpadButtonStateCheck;
                case GamepadButton.DPadDown:        return profile.DPadType == GamepadDPadType.Button ? callback(profile.DPadDownButton, gamepad) : m_dpadState[gamepad].Down == dpadButtonStateCheck;
                case GamepadButton.DPadLeft:        return profile.DPadType == GamepadDPadType.Button ? callback(profile.DPadLeftButton, gamepad) : m_dpadState[gamepad].Left == dpadButtonStateCheck;
                case GamepadButton.DPadRight:       return profile.DPadType == GamepadDPadType.Button ? callback(profile.DPadRightButton, gamepad) : m_dpadState[gamepad].Right == dpadButtonStateCheck;
                
                default:
                    return false;
            }
        }

        public bool GetButton(GamepadButton button, int gamepad) { return ButtonQuery(button, gamepad, GetButton, ButtonState.Pressed); }
        public bool GetButtonDown(GamepadButton button, int gamepad) { return ButtonQuery(button, gamepad, GetButtonDown, ButtonState.JustPressed); }
        public bool GetButtonUp(GamepadButton button, int gamepad) { return ButtonQuery(button, gamepad, GetButtonUp, ButtonState.JustReleased); }

        static readonly int firstJoyButton = (int)KeyCode.Joystick1Button0;

        bool GetButton(int button, int gamepad) { return InputManager.GetKey((KeyCode)(firstJoyButton + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button)); }
        bool GetButtonDown(int button, int gamepad) { return InputManager.GetKeyDown((KeyCode)(firstJoyButton + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button)); }
        bool GetButtonUp(int button, int gamepad) { return InputManager.GetKeyUp((KeyCode)(firstJoyButton + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button)); }
    }
}