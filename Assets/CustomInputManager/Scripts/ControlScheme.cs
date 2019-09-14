
using UnityEngine;
using System;
using System.Collections.Generic;

namespace CustomInputManager
{
	[System.Serializable] public class ControlScheme
	{
		[SerializeField] private bool m_isExpanded;
		public bool IsExpanded
		{
			get { return m_isExpanded; }
			set { m_isExpanded = value; }
		}
		
		[SerializeField] private string m_name;
		[SerializeField] private string m_uniqueID;
		[SerializeField] private List<InputAction> m_actions;



		Dictionary<int, InputAction> key2Action;

		HashSet<string> actionNames;

		public bool HasActionName (string actionName) {
			return actionNames.Contains(actionName);
		}

		public InputAction GetAction(int key) {
			InputAction r;
			if (key2Action.TryGetValue(key, out r))
				return r;
			
			return null;
		}
		

		public List<InputAction> Actions {
			get { return m_actions; }
			set { m_actions = value; }
		}

		public string UniqueID
		{
			get { return m_uniqueID; }
			set { m_uniqueID = value; }
		}

		public string Name
		{
			get { return m_name; }
			set
			{
				if (Application.isPlaying) Debug.LogWarning("You should not change the name of a control scheme at runtime");
				else m_name = value;
			}
		}

		public bool AnyInput (int playerID)
		{
			for (int i = 0; i < actionsCount; i++) {
				if(m_actions[i].AnyInput(playerID)) return true;
			}
			return false;
		}
		
		public ControlScheme() : this("New Scheme") { }
		
		public ControlScheme(string name)
		{
			m_actions = new List<InputAction>();
			m_name = name;
			m_isExpanded = false;
			m_uniqueID = GenerateUniqueID();
		}

		public void Initialize(int maxJoysticks)
		{
			actionsCount = m_actions.Count;

			actionNames = new HashSet<string>();
			
			key2Action = new Dictionary<int, InputAction>();
			for (int i = 0; i < actionsCount; i++) {
				InputAction action = m_actions[i];
				key2Action[_EncodeActionName(action.Name)] = action;
				actionNames.Add(action.Name);

				action.Initialize(maxJoysticks);
			}
		}
		public static int _EncodeActionName (string actionName) {
			return Shader.PropertyToID(actionName);
		}


		int actionsCount;
		public void Update(float deltaTime)
		{
			for (int i = 0; i < actionsCount; i++) m_actions[i].Update(deltaTime);
		}

		public InputAction CreateNewAction(string name, string displayName)
		{
			InputAction action = new InputAction(name, displayName);
			m_actions.Add(action);
			return action;
		}


		public Dictionary<string, InputAction> GetActionLookupTable()
		{
			Dictionary<string, InputAction> table = new Dictionary<string, InputAction>();
			foreach(InputAction action in m_actions)
				table[action.Name] = action;
			
			return table;
		}

		public static string GenerateUniqueID()
		{
			return Guid.NewGuid().ToString("N");
		}
	}
}