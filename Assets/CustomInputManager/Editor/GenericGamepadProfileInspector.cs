using UnityEngine;
using UnityEditor;
using CustomInputManager;

namespace CustomInputManagerEditor.IO
{
    [CustomEditor(typeof(GenericGamepadProfile))]
    public class GenericGamepadProfileInspector : Editor
    {
        private static string[] m_buttonNames, m_axisNames;
		
        public static string[] GenerateJoystickButtonNames()
        {
            if(m_buttonNames == null || m_buttonNames.Length != InputBinding.MAX_JOYSTICK_BUTTONS)
            {
                m_buttonNames = new string[InputBinding.MAX_JOYSTICK_BUTTONS];
                for(int i = 0; i < InputBinding.MAX_JOYSTICK_BUTTONS; i++)
                {
                    m_buttonNames[i] = "Joystick Button " + i;
                }
            }

            return m_buttonNames;
        }

		public static string[] GenerateJoystickAxisNames()
		{
			if(m_axisNames == null || m_axisNames.Length != InputBinding.MAX_JOYSTICK_AXES)
			{
				m_axisNames = new string[InputBinding.MAX_JOYSTICK_AXES];
				for(int i = 0; i < InputBinding.MAX_JOYSTICK_AXES; i++)
				{
					if(i == 0)
						m_axisNames[i] = "X";
					else if(i == 1)
						m_axisNames[i] = "Y";
					else if(i == 2)
						m_axisNames[i] = "3rd axis (Joysticks and Scrollwheel)";
					else if(i == 21)
						m_axisNames[i] = "21st axis (Joysticks)";
					else if(i == 22)
						m_axisNames[i] = "22nd axis (Joysticks)";
					else if(i == 23)
						m_axisNames[i] = "23rd axis (Joysticks)";
					else
						m_axisNames[i] = string.Format("{0}th axis (Joysticks)", i + 1);
				}
			}

			return m_axisNames;
		}


        private SerializedProperty m_dpadType;
        private SerializedProperty m_leftStickButton;
        private SerializedProperty m_rightStickButton;
        private SerializedProperty m_leftBumperButton;
        private SerializedProperty m_rightBumperButton;
        private SerializedProperty m_dpadUpButton;
        private SerializedProperty m_dpadDownButton;
        private SerializedProperty m_dpadLeftButton;
        private SerializedProperty m_dpadRightButton;
        private SerializedProperty m_backButton;
        private SerializedProperty m_startButton;
        private SerializedProperty m_actionTopButton;
        private SerializedProperty m_actionBottomButton;
        private SerializedProperty m_actionLeftButton;
        private SerializedProperty m_actionRightButton;
        private SerializedProperty m_leftStickXAxis;
        private SerializedProperty m_leftStickYAxis;
        private SerializedProperty m_rightStickXAxis;
        private SerializedProperty m_rightStickYAxis;
        private SerializedProperty m_dpadXAxis;
        private SerializedProperty m_dpadYAxis;
        private SerializedProperty m_leftTriggerAxis;
        private SerializedProperty m_rightTriggerAxis;
        
        SerializedProperty unityJoystickName;
        SerializedProperty platforms;
        

        private void OnEnable()
        {
            m_dpadType = serializedObject.FindProperty("m_dpadType");
            m_leftStickButton = serializedObject.FindProperty("m_leftStickButton");
            m_rightStickButton = serializedObject.FindProperty("m_rightStickButton");
            m_leftBumperButton = serializedObject.FindProperty("m_leftBumperButton");
            m_rightBumperButton = serializedObject.FindProperty("m_rightBumperButton");
            m_dpadUpButton = serializedObject.FindProperty("m_dpadUpButton");
            m_dpadDownButton = serializedObject.FindProperty("m_dpadDownButton");
            m_dpadLeftButton = serializedObject.FindProperty("m_dpadLeftButton");
            m_dpadRightButton = serializedObject.FindProperty("m_dpadRightButton");
            m_backButton = serializedObject.FindProperty("m_backButton");
            m_startButton = serializedObject.FindProperty("m_startButton");
            m_actionTopButton = serializedObject.FindProperty("m_actionTopButton");
            m_actionBottomButton = serializedObject.FindProperty("m_actionBottomButton");
            m_actionLeftButton = serializedObject.FindProperty("m_actionLeftButton");
            m_actionRightButton = serializedObject.FindProperty("m_actionRightButton");
            m_leftStickXAxis = serializedObject.FindProperty("m_leftStickXAxis");
            m_leftStickYAxis = serializedObject.FindProperty("m_leftStickYAxis");
            m_rightStickXAxis = serializedObject.FindProperty("m_rightStickXAxis");
            m_rightStickYAxis = serializedObject.FindProperty("m_rightStickYAxis");
            m_dpadXAxis = serializedObject.FindProperty("m_dpadXAxis");
            m_dpadYAxis = serializedObject.FindProperty("m_dpadYAxis");
            m_leftTriggerAxis = serializedObject.FindProperty("m_leftTriggerAxis");
            m_rightTriggerAxis = serializedObject.FindProperty("m_rightTriggerAxis");

            unityJoystickName = serializedObject.FindProperty("unityJoystickName");
            platforms = serializedObject.FindProperty("platforms");

            m_buttonNames = GenerateJoystickButtonNames();
            m_axisNames = GenerateJoystickAxisNames();
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(unityJoystickName);
            EditorGUILayout.PropertyField(platforms, true);

            //  SETTINGS
            DrawHeader("Settings");
            EditorGUILayout.PropertyField(m_dpadType);
            
            //  BUTTONS
            DrawHeader("Buttons");
            DrawButtonField(m_leftStickButton);
            DrawButtonField(m_rightStickButton);
            DrawButtonField(m_leftBumperButton);
            DrawButtonField(m_rightBumperButton);

            if(m_dpadType.enumValueIndex == (int)GamepadDPadType.Button)
            {
                DrawButtonField(m_dpadUpButton);
                DrawButtonField(m_dpadDownButton);
                DrawButtonField(m_dpadLeftButton);
                DrawButtonField(m_dpadRightButton);
            }

            DrawButtonField(m_backButton);
            DrawButtonField(m_startButton);
            DrawButtonField(m_actionTopButton);
            DrawButtonField(m_actionBottomButton);
            DrawButtonField(m_actionLeftButton);
            DrawButtonField(m_actionRightButton);

            //  AXES
            DrawHeader("Axes");
            DrawAxisField(m_leftStickXAxis);
            DrawAxisField(m_leftStickYAxis);
            DrawAxisField(m_rightStickXAxis);
            DrawAxisField(m_rightStickYAxis);

            DrawAxisField(m_leftTriggerAxis);
            DrawAxisField(m_rightTriggerAxis);
            
            if(m_dpadType.enumValueIndex == (int)GamepadDPadType.Axis)
            {
                DrawAxisField(m_dpadXAxis);
                DrawAxisField(m_dpadYAxis);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader(string label)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private void DrawButtonField(SerializedProperty button)
        {
            button.intValue = EditorGUILayout.Popup(button.displayName, button.intValue, m_buttonNames);
        }

        private void DrawAxisField(SerializedProperty axis)
        {
            axis.intValue = EditorGUILayout.Popup(axis.displayName, axis.intValue, m_axisNames);
        }
    }
}