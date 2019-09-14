using UnityEngine;
using UnityEngine.UI;

namespace CustomInputManager.Examples
{
    public class GenericGamepadAdapterTest : MonoBehaviour
    {
        const int maxGamePads = InputBinding.MAX_UNITY_JOYSTICKS;
        [SerializeField] [Range(0, InputBinding.MAX_UNITY_JOYSTICKS)] int playerID = 0;

        [Header("Templates")]
        [SerializeField] private GameObject m_gamepadStateTemplate = null;
        [SerializeField] private GameObject m_gamepadButtonTemplate = null;
        [SerializeField] private GameObject m_gamepadAxisTemplate = null;

        [Header("Roots")]
        [SerializeField] private RectTransform m_gamepadStateRoot = null;
        [SerializeField] private RectTransform m_gamepadButtonRoot = null;
        [SerializeField] private RectTransform m_gamepadAxisRoot = null;

        private Text[] m_gamepadStateText, m_gamepadButtonText, m_gamepadAxisText;
        
        private void Start()
        {
            CreateGamepadStateFields();
            CreateGamepadButtonFields();
            CreateGamepadAxisFields();
        }

        private void Update()
        {
            for(int i = 0; i < m_gamepadStateText.Length; i++) m_gamepadStateText[i].text = InputManager.Gamepad.GamepadName(i);// InputManager.Gamepad.GamepadAvailable(i, out _) ? "Connected" : "Not Connected";
            for(int i = 0; i < m_gamepadButtonText.Length; i++) m_gamepadButtonText[i].text = InputManager.Gamepad.GetButton((GamepadButton)i, playerID).ToString();
            for(int i = 0; i < m_gamepadAxisText.Length; i++) m_gamepadAxisText[i].text = InputManager.Gamepad.GetAxis((GamepadAxis)i, playerID).ToString();
                
            // if(InputManager.Gamepad.GamepadProfile.DPadType == GamepadDPadType.Axis) {
            //     if(InputManager.Gamepad.GetButtonDown(GamepadButton.DPadUp, playerID)) Debug.Log("DPadUp was pressed!");
            //     if(InputManager.Gamepad.GetButtonDown(GamepadButton.DPadDown, playerID)) Debug.Log("DPadDown was pressed!");
            //     if(InputManager.Gamepad.GetButtonDown(GamepadButton.DPadLeft, playerID)) Debug.Log("DPadLeft was pressed!");
            //     if(InputManager.Gamepad.GetButtonDown(GamepadButton.DPadRight, playerID)) Debug.Log("DPadRight was pressed!");

            //     if(InputManager.Gamepad.GetButtonUp(GamepadButton.DPadUp, playerID)) Debug.Log("DPadUp was released!");
            //     if(InputManager.Gamepad.GetButtonUp(GamepadButton.DPadDown, playerID)) Debug.Log("DPadDown was released!");
            //     if(InputManager.Gamepad.GetButtonUp(GamepadButton.DPadLeft, playerID)) Debug.Log("DPadLeft was released!");
            //     if(InputManager.Gamepad.GetButtonUp(GamepadButton.DPadRight, playerID)) Debug.Log("DPadRight was released!");
            // }
        }

        Text MakeNewTemplate (GameObject template, Transform root, string defaultLabel, string valueText) {
            GameObject obj = GameObject.Instantiate<GameObject>(template);
            obj.SetActive(true);
            obj.transform.SetParent(root);
            obj.transform.localScale = Vector3.one;

            Text label = obj.transform.Find("label").GetComponent<Text>();
            label.text = defaultLabel + ":";

            Text returnText = obj.transform.Find("value").GetComponent<Text>();
            returnText.text = valueText;
            return returnText;
        }

        private void CreateGamepadStateFields() {
            m_gamepadStateText = new Text[maxGamePads];
            for(int i = 0; i < maxGamePads; i++) m_gamepadStateText[i] = MakeNewTemplate(m_gamepadStateTemplate, m_gamepadStateRoot, "Gamepad " + (i + 1), "Not Connected");
        }
        private void CreateGamepadButtonFields() {
            m_gamepadButtonText = new Text[14];
            for(int i = 0; i < m_gamepadButtonText.Length; i++) m_gamepadButtonText[i] = MakeNewTemplate(m_gamepadButtonTemplate, m_gamepadButtonRoot, ((GamepadButton)i).ToString(), "False");
        }
        private void CreateGamepadAxisFields() {
            m_gamepadAxisText = new Text[8];
            for(int i = 0; i < m_gamepadAxisText.Length; i++) m_gamepadAxisText[i] = MakeNewTemplate(m_gamepadAxisTemplate, m_gamepadAxisRoot, ((GamepadAxis)i).ToString(), "0");
        }
    }
}