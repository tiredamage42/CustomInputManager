using UnityEngine;
using System.Collections.Generic;
using System.IO;
namespace CustomInputManager
{
	
	public partial class InputManager : MonoBehaviour
	{
		public GamepadHandler gamepad;
		public static GamepadHandler Gamepad { get { return m_instance.gamepad; } }

		[HideInInspector] [SerializeField] private List<ControlScheme> m_controlSchemes = new List<ControlScheme>();
		public TextAsset defaultInputsXML;
		ControlScheme[] playerSchemes;
		
		private ScanService m_scanService;
		public static InputManager m_instance;
		

		private Dictionary<string, ControlScheme> m_schemeLookup;

		public List<ControlScheme> ControlSchemes { get { return m_controlSchemes; } }

		public int maxJoysticks  { get { return numPlayers; } }
		public int numPlayers = 2;


		private void Awake()
		{
			if(m_instance == null)
			{
				m_instance = this;
				m_scanService = new ScanService();

				m_schemeLookup = new Dictionary<string, ControlScheme>();
				
				Initialize();

				gamepad.Awake(this, maxJoysticks);

				// try and load custom runtime bindings
				if (!LoadOverridenControls()) {
					ResetSchemes(defaultInputsXML);
				}
			}
			else
			{
				Debug.LogWarning("You have multiple InputManager instances in the scene!", gameObject);
				Destroy(this);
			}
		}



		private void OnDestroy()
		{
			if(m_instance == this)
			{
				m_instance = null;
			}
		}

		private void Initialize()
		{
			m_schemeLookup.Clear();
			
			playerSchemes = new ControlScheme[numPlayers];
			for (int i = 0; i < numPlayers; i++) playerSchemes[i] = null;
		
			if(m_controlSchemes.Count == 0) {
				Debug.LogWarning("No Control Schemes Loaded...");
				return;
			}

			for (int i = 0; i < numPlayers; i++) {
				playerSchemes[i] = m_controlSchemes[0];
			}

			PopulateLookupTables();

			foreach(ControlScheme scheme in m_controlSchemes)
				scheme.Initialize(maxJoysticks);
			
			Input.ResetInputAxes();
		}

		private void PopulateLookupTables()
		{

			Debug.Log("Initializing scheme lookup " + m_controlSchemes.Count);

			m_schemeLookup.Clear();
			foreach(ControlScheme scheme in m_controlSchemes)
			{
				m_schemeLookup[scheme.Name] = scheme;
			}
		}

		private void Update()
		{
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			gamepad.OnUpdate(unscaledDeltaTime);


			for (int i = 0; i < m_controlSchemes.Count; i++) {
				m_controlSchemes[i].Update(unscaledDeltaTime);
			}

			if(m_scanService.IsScanning)
			{
				m_scanService.Update(Time.unscaledTime, KeyCode.Escape, 5.0f, numPlayers);
			}

		}
		private int? IsControlSchemeInUse(string name)
		{
			for (int i = 0; i < numPlayers; i++) {
				if(playerSchemes[i] != null && playerSchemes[i].Name == name)
					return i;
			}
			return null;
		}

		public void SetSaveData(List<ControlScheme> controlSchemes)
		{
			if (controlSchemes != null) 
				m_controlSchemes = controlSchemes;
		}

		
		#region [Static Interface]
		

		public static bool IsScanning { get { return m_instance.m_scanService.IsScanning; } }

		public static ControlScheme GetControlScheme(int playerID) {
			return m_instance.playerSchemes[playerID];
		}

		/// <summary>
		/// Returns true if any axis of any active control scheme is receiving input.
		/// </summary>
		public static bool AnyInput()
		{

			int c = m_instance.m_controlSchemes.Count;
			for (int i = 0; i < c; i++) {
				if (AnyInput(m_instance.m_controlSchemes[i], i)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns true if any axis of the control scheme is receiving input.
		/// </summary>
		public static bool AnyInput(int playerID)
		{
			return AnyInput(m_instance.playerSchemes[playerID], playerID);
		}

		/// <summary>
		/// Returns true if any axis of the specified control scheme is receiving input.
		/// </summary>
		public static bool AnyInput(string schemeName, int playerID)
		{
			ControlScheme scheme;
			if(m_instance.m_schemeLookup.TryGetValue(schemeName, out scheme)) {
				return scheme.AnyInput(playerID);
			}
			return false;
		}

		private static bool AnyInput(ControlScheme scheme, int playerID)
		{
			if(scheme != null)
				return scheme.AnyInput(playerID);
			return false;
		}

		/// <summary>
		/// Resets the internal state of the input manager.
		/// </summary>
		public static void Reinitialize()
		{
			m_instance.Initialize();
		}

		/// <summary>
		/// Changes the active control scheme.
		/// </summary>
		public static void SetControlScheme(string name, int playerID)
		{
			int? playerWhoUsesControlScheme = m_instance.IsControlSchemeInUse(name);

			// this assumes only one player per control scheme, which only works for strictly
			// keyboard only games...

			// if(playerWhoUsesControlScheme.HasValue && playerWhoUsesControlScheme.Value != playerID)
			// {
			// 	Debug.LogErrorFormat("The control scheme named \'{0}\' is already being used by player {1}", name, playerWhoUsesControlScheme.Value.ToString());
			// 	return;
			// }

			if(playerWhoUsesControlScheme.HasValue && playerWhoUsesControlScheme.Value == playerID) {
				Debug.LogWarning("player " + playerID + " is already using scheme: " + name);
				return;
			}

			ControlScheme controlScheme = null;
			if(m_instance.m_schemeLookup.TryGetValue(name, out controlScheme))
			{
				controlScheme.Initialize(m_instance.maxJoysticks);
				m_instance.playerSchemes[playerID] = controlScheme;
			}
			else
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", name));
			}
		}

		public static ControlScheme GetControlScheme(string name)
		{
			ControlScheme scheme;
			if(m_instance.m_schemeLookup.TryGetValue(name, out scheme)) {
				return scheme;
			}
			Debug.LogError("Scheme " + name + " does not exist");
			return null;
		}

		public static InputAction GetAction(int playerID, int actionKey)
		{
			ControlScheme scheme = m_instance.playerSchemes[playerID];
			if(scheme == null) return null;
			return scheme.GetAction(actionKey);
		}


		public static int Name2Key (string name) {
			bool nameValid = false;
			for (int i = 0; i < m_instance.ControlSchemes.Count; i++) {
				if (m_instance.ControlSchemes[i].HasActionName(name)) {
					nameValid = true;
					break;
				}
			}
			if (!nameValid) {
				Debug.LogError(string.Format("An action named \'{0}\' does not exist in the active input configuration", name));
				return -1;
			}
			return Shader.PropertyToID(name);
		}
		public static int Name2Key (string name, int playerID) {
			ControlScheme scheme = m_instance.playerSchemes[playerID];
			if (!scheme.HasActionName(name)) {
				Debug.LogError(string.Format("An action named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
				return -1;
			}
			return Shader.PropertyToID(name);
		}


		public static void StartInputScan(ScanFlags scanFlags, ScanHandler scanHandler, System.Action onScanEnd)
		{
			m_instance.m_scanService.Start(Time.unscaledTime, scanFlags, scanHandler, onScanEnd);
		}
		public static void StopInputScan()
		{
			m_instance.m_scanService.Stop();
		}

		

		
		/// <summary>
		/// Saves the control schemes in an XML file, in Application.persistentDataPath.
		/// </summary>
		public static void SaveCustomBindings()
		{
			Save(Application.persistentDataPath + "/InputManagerOverride.xml");
		}

		/// <summary>
		/// Saves the control schemes in the XML format, at the specified location.
		/// </summary>
		public static void Save(string filePath)
		{
			Save(new InputSaverXML(filePath));
		}

		public static void Save(InputSaverXML inputSaver)
		{
			if(inputSaver != null)
			{
				inputSaver.Save(m_instance.ControlSchemes);//.GetSaveData());
			}
			else
			{
				Debug.LogError("InputSaver is null. Cannot save control schemes.");
			}
		}



		/// <summary>
		/// Loads the control schemes from an XML file, from Application.persistentDataPath.
		/// </summary>
		public static bool LoadOverridenControls()
		{
			return Load(Application.persistentDataPath + "/InputManagerOverride.xml");
		}

		/// <summary>
		/// Loads the control schemes saved in the XML format, from the specified location.
		/// </summary>
		public static bool Load(string filePath)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			if(UnityEngine.Windows.File.Exists(filePath))
#else
			if(System.IO.File.Exists(filePath))
#endif
			{
				Load(new InputLoaderXML(filePath));
				return true;
			}
			return false;
		}

		public static void ResetSchemes( TextAsset defaultInputSchemesXML )
		{
			using(StringReader reader = new StringReader(defaultInputSchemesXML.text))
			{
                Load(new InputLoaderXML(reader));
			}
		}
		public static bool ResetScheme( string m_controlSchemeName, int bindingIndex)
		{
			ControlScheme defControlScheme = null;
			using(StringReader reader = new StringReader(InputManager.m_instance.defaultInputsXML.text))
			{
				defControlScheme = new InputLoaderXML(reader).Load(m_controlSchemeName);
			}

			if(defControlScheme != null)
			{
				ControlScheme controlScheme = InputManager.GetControlScheme(m_controlSchemeName);
				if(defControlScheme.Actions.Count == controlScheme.Actions.Count)
				{
					for(int i = 0; i < defControlScheme.Actions.Count; i++)
					{
						controlScheme.Actions[i].GetBinding(bindingIndex).Copy(defControlScheme.Actions[i].GetBinding(bindingIndex));
					}

					InputManager.Reinitialize();

                    return true;
				}
				else
				{
					Debug.LogError("Current and default control scheme don't have the same number of actions");
				}
			}
			else
			{
				Debug.LogErrorFormat("Default input profile doesn't contain a control scheme named '{0}'", m_controlSchemeName);
			}

            return false;
		}


		public static void Load(InputLoaderXML inputLoader)
		{
			if(inputLoader != null)
			{
				// Debug.Log("loading input override... " + Application.persistentDataPath);
				m_instance.SetSaveData(inputLoader.Load());
				m_instance.Initialize();
			}
			else
			{
				Debug.LogError("InputLoader is null. Cannot load control schemes.");
			}
		}

		#endregion
	}
}
