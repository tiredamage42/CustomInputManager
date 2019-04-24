﻿using UnityEngine;
using UnityEngine.UI;

namespace CustomInputManager.Examples
{
    public class GenericGamepadAdapterTest : MonoBehaviour
    {
        [SerializeField] [Range(0, InputManager.maxPlayers)] int playerID = -1;

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
            for(int i = 0; i < m_gamepadStateText.Length; i++)
                m_gamepadStateText[i].text = InputManager.Gamepad.GamepadIsConnected(i) ? "Connected" : "Not Connected";

            for(int i = 0; i < m_gamepadButtonText.Length; i++)
                m_gamepadButtonText[i].text = InputManager.Gamepad.GetButton((GamepadButton)i, playerID).ToString();

            for(int i = 0; i < m_gamepadAxisText.Length; i++)
                m_gamepadAxisText[i].text = InputManager.Gamepad.GetAxis((GamepadAxis)i, playerID).ToString();
                
            GamepadHandler adapter = InputManager.Gamepad;//.Adapter as GenericGamepadStateAdapter;
            if(adapter.GamepadProfile.DPadType == GamepadDPadType.Axis)
            {
                if(InputManager.Gamepad.GetButtonDown(GamepadButton.DPadUp, playerID))
                    Debug.Log("DPadUp was pressed!");
                if(InputManager.Gamepad.GetButtonDown(GamepadButton.DPadDown, playerID))
                    Debug.Log("DPadDown was pressed!");
                if(InputManager.Gamepad.GetButtonDown(GamepadButton.DPadLeft, playerID))
                    Debug.Log("DPadLeft was pressed!");
                if(InputManager.Gamepad.GetButtonDown(GamepadButton.DPadRight, playerID))
                    Debug.Log("DPadRight was pressed!");

                if(InputManager.Gamepad.GetButtonUp(GamepadButton.DPadUp, playerID))
                    Debug.Log("DPadUp was released!");
                if(InputManager.Gamepad.GetButtonUp(GamepadButton.DPadDown, playerID))
                    Debug.Log("DPadDown was released!");
                if(InputManager.Gamepad.GetButtonUp(GamepadButton.DPadLeft, playerID))
                    Debug.Log("DPadLeft was released!");
                if(InputManager.Gamepad.GetButtonUp(GamepadButton.DPadRight, playerID))
                    Debug.Log("DPadRight was released!");
            }
        }

        private void CreateGamepadStateFields()
        {
            m_gamepadStateText = new Text[4];

            for(int i = 0; i < m_gamepadStateText.Length; i++)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(m_gamepadStateTemplate);
                obj.SetActive(true);
                obj.transform.SetParent(m_gamepadStateRoot);

                Text label = obj.transform.Find("label").GetComponent<Text>();
                label.text = "Gamepad " + (i + 1) + ":";

                m_gamepadStateText[i] = obj.transform.Find("value").GetComponent<Text>();
                m_gamepadStateText[i].text = "Not Connected";
            }
        }

        private void CreateGamepadButtonFields()
        {
            m_gamepadButtonText = new Text[14];

            for(int i = 0; i < m_gamepadButtonText.Length; i++)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(m_gamepadButtonTemplate);
                obj.SetActive(true);
                obj.transform.SetParent(m_gamepadButtonRoot);

                Text label = obj.transform.Find("label").GetComponent<Text>();
                label.text = ((GamepadButton)i) + ":";

                m_gamepadButtonText[i] = obj.transform.Find("value").GetComponent<Text>();
                m_gamepadButtonText[i].text = "False";
            }
        }

        private void CreateGamepadAxisFields()
        {

            m_gamepadAxisText = new Text[8];
            for(int i = 0; i < m_gamepadAxisText.Length; i++)
            {
                GameObject obj = GameObject.Instantiate<GameObject>(m_gamepadAxisTemplate);
                obj.SetActive(true);
                obj.transform.SetParent(m_gamepadAxisRoot);

                Text label = obj.transform.Find("label").GetComponent<Text>();
                label.text = ((GamepadAxis)i) + ":";

                m_gamepadAxisText[i] = obj.transform.Find("value").GetComponent<Text>();
                m_gamepadAxisText[i].text = "0";
            }
        }
    }
}