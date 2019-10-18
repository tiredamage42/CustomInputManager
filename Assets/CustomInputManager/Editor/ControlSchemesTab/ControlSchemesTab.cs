
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using CustomInputManager.Internal;


using UnityTools.EditorTools;
namespace CustomInputManager.Editor {
		
	public class ControlSchemesTab 
	{
		public enum MoveDirection { Up, Down }
		public enum CollectionAction { None, Remove, Add, MoveUp, MoveDown }
		public enum KeyType { Positive = 0, Negative }
		
		#region [Fields]
		public const float INPUT_ACTION_SPACING = 20.0f;
		public const float INPUT_BINDING_SPACING = 10.0f;
		public const float JOYSTICK_WARNING_SPACING = 10.0f;
		public const float JOYSTICK_WARNING_HEIGHT = 40.0f;
		public const float INPUT_FIELD_HEIGHT = 16.0f;
		public const float FIELD_SPACING = 2.0f;
		public const float BUTTON_HEIGHT = 24.0f;
		public const float SCROLL_BAR_WIDTH = 15.0f;
		public const float MIN_MAIN_PANEL_WIDTH = 300.0f;
		
		List<ControlScheme> loadedElements;

		void InitializeLoadedElements () {
			if (!Application.isPlaying) loadedElements = DefaultProjectInputs.LoadDefaultSchemes();
		}

		List<ControlScheme> schemes {
			get {
				if (Application.isPlaying) return CustomInput.ControlSchemes;
				if (loadedElements == null) InitializeLoadedElements();
				return loadedElements;
			}
		}

		bool selectionEmpty { get { return !IsActionSelected && !IsControlSchemeSelected; } }
		GUIContent m_gravityInfo;
		GUIContent m_sensitivityInfo;
		GUIContent m_snapInfo;
		GUIContent m_deadZoneInfo;
		GUIContent m_plusButtonContent;
		GUIContent m_minusButtonContent;
		GUIContent m_upButtonContent;
		GUIContent m_downButtonContent;
		InputAction m_copySource;
		GUIStyle m_warningLabel;
		#endregion

		#region [Startup]
		public static Texture2D GetUnityIcon(string name) {
			return EditorGUIUtility.Load(name + ".png") as Texture2D;
		}
		public static Texture2D GetCustomIcon(string name) {
			return Resources.Load<Texture2D>(InputManager.resourcesFolder + name) as Texture2D;
		}
		
		HierarchyGUI hierarchyGUI;
		public void OnEnable(HierarchyGUI hierarchyGUI)
		{
			this.hierarchyGUI = hierarchyGUI;

			m_gravityInfo = new GUIContent ("Gravity When Axis Query", "The speed(in units/sec) at which a digital axis falls towards neutral.");
			m_sensitivityInfo = new GUIContent ("Sensitivity When Axis Query", "The speed(in units/sec) at which an axis moves towards the target value.");
			m_snapInfo = new GUIContent ("Snap When Axis Query", "If input switches direction, do we snap to neutral and continue from there?");// For digital axes only.");
			m_deadZoneInfo = new GUIContent ("Dead Zone", "Size of analog dead zone. Values within this range map to neutral.");
			m_plusButtonContent = new GUIContent (GetUnityIcon("ol plus"));
			m_minusButtonContent = new GUIContent (GetUnityIcon("ol minus"));
			m_upButtonContent = new GUIContent (GetCustomIcon("input_editor_arrow_up"));
			m_downButtonContent = new GUIContent (GetCustomIcon("input_editor_arrow_down"));

			InitializeLoadedElements();
		}

		void SaveDefaultProjectInputsXML() {
			DefaultProjectInputs.SaveSchemesAsDefault("Saving", schemes);
			guiChanged = false;
		}
		
		void DisplaySaveDialogue () {
			if (!guiChanged)
				return;

			if (EditorUtility.DisplayDialog("Input Manager", "Would you like to save changes made to the input schemes?", "Yes", "No"))
				SaveDefaultProjectInputsXML();
		}
		public void Dispose(bool repeat)
		{
			if (!repeat) {
				DisplaySaveDialogue();
				m_copySource = null;
			}
		}
		#endregion

		#region [Menus]
		void CreateFileMenu(Rect position) {
			GenericMenu fileMenu = new GenericMenu();
			fileMenu.AddItem(new GUIContent("Overwrite Project Settings"), false, HandleFileMenuOption, 0);
			
			fileMenu.AddSeparator("");
			fileMenu.AddItem(new GUIContent("Save Project Inputs"), false, HandleFileMenuOption, 1);
			
			fileMenu.AddSeparator("");
			
			fileMenu.AddItem(new GUIContent("New Control Scheme"), false, HandleFileMenuOption, 2);
			if(IsControlSchemeSelected)
				fileMenu.AddItem(new GUIContent("New Action"), false, HandleFileMenuOption, 3);
			else
				fileMenu.AddDisabledItem(new GUIContent("New Action"));

			fileMenu.AddSeparator("");

			if (!selectionEmpty)
				fileMenu.AddItem(new GUIContent("Duplicate"), false, HandleFileMenuOption, 4);
			else
				fileMenu.AddDisabledItem(new GUIContent("Duplicate"));

			if (!selectionEmpty)
				fileMenu.AddItem(new GUIContent("Delete"), false, HandleFileMenuOption, 5);
			else
				fileMenu.AddDisabledItem(new GUIContent("Delete"));

			if(IsActionSelected)
				fileMenu.AddItem(new GUIContent("Copy"), false, HandleFileMenuOption, 6);
			else
				fileMenu.AddDisabledItem(new GUIContent("Copy"));

			if(m_copySource != null && IsActionSelected)
				fileMenu.AddItem(new GUIContent("Paste"), false, HandleFileMenuOption, 7);
			else
				fileMenu.AddDisabledItem(new GUIContent("Paste"));

			fileMenu.DropDown(position);
		}
		void HandleFileMenuOption(object arg) {
			switch((int)arg) {
				case 0: ConvertUnityInputManager.OverwriteProjectSettings(); break;
				case 1: SaveDefaultProjectInputsXML(); break;
				case 2: CreateNewControlScheme(); break;
				case 3: CreateNewInputAction(); break; 
				case 4: Duplicate(); break;
				case 5: Delete(); break;
				case 6: CopyInputAction(); break;
				case 7: PasteInputAction(); break;
			}
		}
		void CreateControlSchemeContextMenu(Rect position) {
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("New Action"), false, HandleControlSchemeContextMenuOption, 0);
			contextMenu.AddSeparator("");
			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleControlSchemeContextMenuOption, 1);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleControlSchemeContextMenuOption, 2);
			contextMenu.AddSeparator("");
			contextMenu.AddItem(new GUIContent("Move Up"), false, HandleControlSchemeContextMenuOption, 3);
			contextMenu.AddItem(new GUIContent("Move Down"), false, HandleControlSchemeContextMenuOption, 4);
			contextMenu.DropDown(position);
		}
		void HandleControlSchemeContextMenuOption(object arg) {
			switch((int)arg) {
				case 0: CreateNewInputAction(); break;
				case 1: Duplicate(); break;
				case 2: Delete(); break;
				case 3: ReorderControlScheme(MoveDirection.Up); break;
				case 4: ReorderControlScheme(MoveDirection.Down); break;
			}
		}
		void CreateInputActionContextMenu(Rect position) {
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleInputActionContextMenuOption, 0);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleInputActionContextMenuOption, 1);
			contextMenu.AddItem(new GUIContent("Copy"), false, HandleInputActionContextMenuOption, 2);
			contextMenu.AddItem(new GUIContent("Paste"), false, HandleInputActionContextMenuOption, 3);
			contextMenu.AddSeparator("");
			contextMenu.AddItem(new GUIContent("Move Up"), false, HandleInputActionContextMenuOption, 4);
			contextMenu.AddItem(new GUIContent("Move Down"), false, HandleInputActionContextMenuOption, 5);
			contextMenu.DropDown(position);
		}

		void HandleInputActionContextMenuOption (object arg)
		{
			switch((int)arg) {
				case 0: Duplicate(); break;
				case 1: Delete(); break;
				case 2: CopyInputAction(); break;
				case 3: PasteInputAction(); break;
				case 4: ReorderInputAction(MoveDirection.Up); break;
				case 5: ReorderInputAction(MoveDirection.Down); break;
			}
		}

		void CreateNewControlScheme () {
			schemes.Add(new ControlScheme());
			hierarchyGUI.ResetSelections();
			hierarchyGUI.selections[0] = schemes.Count - 1;
			InputManagerWindow.instance.Repaint();
		}

		bool IsControlSchemeSelected { get { return hierarchyGUI.selections[0] >= 0; } }
		bool IsActionSelected { get { return hierarchyGUI.selections[1] >= 0; } }
		
		void CreateNewInputAction() {
			if(IsControlSchemeSelected) {
				ControlScheme scheme = schemes[hierarchyGUI.selections[0]];
				scheme.CreateNewAction("New Action", "New Action Display Name");
				hierarchyGUI.selections[1] = scheme.Actions.Count - 1;
				InputManagerWindow.instance.Repaint();
			}
		}

		void Duplicate() {
			if (IsActionSelected)
				DuplicateInputAction();
			else if (IsControlSchemeSelected)
				DuplicateControlScheme();
		}

		InputAction DuplicateInputAction(InputAction source) {
			return DuplicateInputAction(source.Name, source);
		}
		InputAction DuplicateInputAction(string name, InputAction source) {
			InputAction a = new InputAction("_");
			CopyInputAction(a, source);
			a.Name = name;
			return a;
		}

		void DuplicateInputAction () {
			ControlScheme scheme = schemes[hierarchyGUI.selections[0]];
			InputAction source = scheme.Actions[hierarchyGUI.selections[1]];
			InputAction action = DuplicateInputAction(source.Name + " Copy", source);
			scheme.Actions.Insert(hierarchyGUI.selections[1] + 1, action);
			hierarchyGUI.selections[1]++;
			InputManagerWindow.instance.Repaint();
		}

		void DuplicateControlScheme () {
			ControlScheme source = schemes[hierarchyGUI.selections[0]];

			ControlScheme duplicate = new ControlScheme();
			duplicate.Name = source.Name + " Copy";
			duplicate.Actions = new List<InputAction>();
			
			foreach(var action in source.Actions) duplicate.Actions.Add(DuplicateInputAction(action));
			
			schemes.Insert(hierarchyGUI.selections[0] + 1, duplicate);
			hierarchyGUI.selections[0]++;

			InputManagerWindow.instance.Repaint();
		}

		void Delete()
		{
			if(IsActionSelected) {
				ControlScheme scheme = schemes[hierarchyGUI.selections[0]];
				if(hierarchyGUI.selections[1] >= 0 && hierarchyGUI.selections[1] < scheme.Actions.Count)
					scheme.Actions.RemoveAt(hierarchyGUI.selections[1]);
			}
			else if(IsControlSchemeSelected)
				schemes.RemoveAt(hierarchyGUI.selections[0]);
			
			hierarchyGUI.ResetSelections();
			InputManagerWindow.instance.Repaint();
		}

		void CopyInputAction()
		{
			m_copySource = DuplicateInputAction(schemes[hierarchyGUI.selections[0]].Actions[hierarchyGUI.selections[1]]);
		}
		void PasteInputAction()
		{
			CopyInputAction(schemes[hierarchyGUI.selections[0]].Actions[hierarchyGUI.selections[1]], m_copySource);
		}
			
		void ReorderControlScheme(MoveDirection dir) {
			if(IsControlSchemeSelected) {
				var index = hierarchyGUI.selections[0];
				if (dir == MoveDirection.Up && index > 0) {
					var temp = schemes[index];
					schemes[index] = schemes[index - 1];
					schemes[index - 1] = temp;
					hierarchyGUI.ResetSelections();
					hierarchyGUI.selections[0] = index - 1;
				}
				else if(dir == MoveDirection.Down && index < schemes.Count - 1) {
					var temp = schemes[index];
					schemes[index] = schemes[index + 1];
					schemes[index + 1] = temp;
					hierarchyGUI.ResetSelections();
					hierarchyGUI.selections[0] = index + 1;
				}
			}
		}
		void SwapActions(ControlScheme scheme, int fromIndex, int toIndex) {
			if (fromIndex >= 0 && fromIndex < scheme.Actions.Count && toIndex >= 0 && toIndex < scheme.Actions.Count) {
				var temp = scheme.Actions[toIndex];
				scheme.Actions[toIndex] = scheme.Actions[fromIndex];
				scheme.Actions[fromIndex] = temp;
			}
		}

		void ReorderInputAction(MoveDirection dir)
		{
			if(IsActionSelected)
			{
				var scheme = schemes[hierarchyGUI.selections[0]];
				var actionIndex = hierarchyGUI.selections[1];

				if(dir == MoveDirection.Up && actionIndex > 0)
				{
					SwapActions(scheme, actionIndex, actionIndex - 1);
					hierarchyGUI.selections[1] = actionIndex - 1;
				}
				else if(dir == MoveDirection.Down && actionIndex < scheme.Actions.Count - 1)
				{
					SwapActions(scheme, actionIndex, actionIndex + 1);
					hierarchyGUI.selections[1] = actionIndex + 1;
				}
			}
		}
		#endregion

		public static void CopyInputAction(InputAction a, InputAction source)
		{
			a.Name = source.Name;
			a.displayName = source.displayName;
			a.bindings.Clear();
			foreach(var binding in source.bindings)
			{
				InputBinding duplicate = new InputBinding();
				duplicate.Copy(binding);
				a.bindings.Add(duplicate);
			}
		}

		

		#region [OnGUI]

		static bool guiChanged;
		public void OnGUI(Rect position)
		{
			EditorGUI.BeginChangeCheck();
			
			EnsureGUIStyles();

			if (hierarchyGUI.Draw(InputManagerWindow.instance, position, true, BuildHierarchyElementsList(), DrawSelected, CreateFileMenu)) { }
				
			if (EditorGUI.EndChangeCheck()) {
				guiChanged = true;
			}
		}

		List<HieararchyGUIElement> BuildSubElements (ControlScheme scheme) {
			List<HieararchyGUIElement> r = new List<HieararchyGUIElement>();
			for (int i = 0; i < scheme.Actions.Count; i++) {
				r.Add(new HieararchyGUIElement(scheme.Actions[i].Name, null, CreateInputActionContextMenu));
			}
			return r;
		}
		List<HieararchyGUIElement> BuildHierarchyElementsList () {
			List<HieararchyGUIElement> r = new List<HieararchyGUIElement>();
			for (int i = 0; i < schemes.Count; i++) {
				r.Add(new HieararchyGUIElement(schemes[i].Name, BuildSubElements(schemes[i]), CreateControlSchemeContextMenu));
			}
			return r;
		}

		void DrawSelected (Rect position) {
			if (IsControlSchemeSelected) {

				if(IsActionSelected)
					DrawInputActionFields(position, schemes[hierarchyGUI.selections[0]].Actions[hierarchyGUI.selections[1]]);
				else
					DrawControlSchemeFields(position, schemes[hierarchyGUI.selections[0]]);
			}
		}


		 void DrawControlSchemeFields(Rect position, ControlScheme controlScheme)
		{
			position.x += 5;
			position.y += 5;
			position.width -= 10;

			GUILayout.BeginArea(position);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Name", GUILayout.Width(50));
			controlScheme.Name = EditorGUILayout.TextField(controlScheme.Name);
			EditorGUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		void DrawInputActionFields(Rect position, InputAction action)
		{
			bool collectionChanged = false;
			float viewRectHeight = CalculateInputActionViewRectHeight(action);
			float itemPosY = 0.0f;
			float contentWidth = position.width - 10.0f;
			Rect viewRect = new Rect(-5.0f, -5.0f, position.width - 10.0f, viewRectHeight - 10.0f);
			
			if(viewRect.width < MIN_MAIN_PANEL_WIDTH)
			{
				viewRect.width = MIN_MAIN_PANEL_WIDTH;
				contentWidth = viewRect.width - 10.0f;
			}

			if(viewRectHeight - 10.0f > position.height)
			{
				viewRect.width -= SCROLL_BAR_WIDTH;
				contentWidth -= SCROLL_BAR_WIDTH;
			}

			InputManagerWindow.m_mainPanelScrollPos = GUI.BeginScrollView(position, InputManagerWindow.m_mainPanelScrollPos, viewRect);
			Rect nameRect = new Rect(0.0f, ValuePP(ref itemPosY, INPUT_FIELD_HEIGHT + FIELD_SPACING), contentWidth, INPUT_FIELD_HEIGHT);
			Rect displayNameRect = new Rect(0.0f, ValuePP(ref itemPosY, INPUT_FIELD_HEIGHT + FIELD_SPACING), contentWidth, INPUT_FIELD_HEIGHT);

			string name = EditorGUI.TextField(nameRect, "Name", action.Name);
			if(name != action.Name) action.Name = name;		//	This prevents the warning at runtime

			action.displayName = EditorGUI.TextField(displayNameRect, "Display Name", action.displayName);

			if(action.bindings.Count > 0)
			{
				itemPosY += INPUT_ACTION_SPACING;

				for(int i = 0; i < action.bindings.Count; i++)
				{
					float bindingRectHeight = CalculateInputBindingViewRectHeight(action.bindings[i]);
					Rect bindingRect = new Rect(-4.0f, ValuePP(ref itemPosY, bindingRectHeight + INPUT_BINDING_SPACING), contentWidth + 8.0f, bindingRectHeight);

					var res = DrawInputBindingFields(bindingRect, "Binding " + (i + 1).ToString("D2"), action, i);
					if(res == CollectionAction.Add)
					{
						InsertNewBinding(action, i+1);
						collectionChanged = true;
					}
					else if(res == CollectionAction.Remove)
					{

						 int index = i--;

						 if(index >= 0 && index < action.bindings.Count) {

							action.bindings.RemoveAt(index);
						 }

						
						collectionChanged = true;
					}
					else if(res == CollectionAction.MoveUp)
					{
						SwapBindings(action, i, i-1);
						collectionChanged = true;
					}
					else if(res == CollectionAction.MoveDown)
					{
						
						SwapBindings(action, i, i+1);
						collectionChanged = true;
					}
				}
			}
			else
			{
				Rect buttonRect = new Rect(contentWidth / 2 - 125.0f, itemPosY + INPUT_ACTION_SPACING, 250.0f, BUTTON_HEIGHT);
				if(GUI.Button(buttonRect, "Add New Binding"))
				{
					action.CreateNewBinding();
					collectionChanged = true;
				}
			}

			GUI.EndScrollView();

			if(collectionChanged)
				InputManagerWindow.instance.Repaint();
			
		}

		InputBinding InsertNewBinding(InputAction action, int index)
		{
			if(action.bindings.Count < InputAction.MAX_BINDINGS)
			{
				InputBinding binding = new InputBinding();
				action.bindings.Insert(index, binding);

				return binding;
			}

			return null;
		}

		 void SwapBindings(InputAction action, int fromIndex, int toIndex)
		{
			if(fromIndex >= 0 && fromIndex < action.bindings.Count && toIndex >= 0 && toIndex < action.bindings.Count)
			{
				var temp = action.bindings[toIndex];
				action.bindings[toIndex] = action.bindings[fromIndex];
				action.bindings[fromIndex] = temp;
			}
		}

		CollectionAction DrawInputBindingFields(Rect position, string label, InputAction action, int bindingIndex)
		{
			Rect headerRect = new Rect(position.x + 5.0f, position.y, position.width, INPUT_FIELD_HEIGHT);
			Rect layoutArea = new Rect(position.x + 10.0f, position.y + INPUT_FIELD_HEIGHT + FIELD_SPACING + 5.0f, position.width - 12.5f, position.height - (INPUT_FIELD_HEIGHT + FIELD_SPACING + 5.0f));
			InputBinding binding = action.bindings[bindingIndex];
			CollectionAction result = CollectionAction.None;

			EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);
			
			GUILayout.BeginArea(layoutArea);
			binding.Type = (InputType)EditorGUILayout.EnumPopup("Type", binding.Type);
			InputType t = binding.Type;

			if (t== InputType.KeyButton || t == InputType.DigitalAxis) 
				binding.Positive = (KeyCode)EditorGUILayout.EnumPopup("Positive", binding.Positive);
			if(t == InputType.DigitalAxis) 
				binding.Negative = (KeyCode)EditorGUILayout.EnumPopup("Negative", binding.Negative);
			if(t == InputType.MouseAxis) 
				binding.MouseAxis = EditorGUILayout.Popup("Axis", binding.MouseAxis, InputBinding.mouseAxisNames);
			if(t == InputType.GamepadButton) 
				binding.GamepadButton = (GamepadButton)EditorGUILayout.EnumPopup("Button", binding.GamepadButton);
			if(t == InputType.GamepadAnalogButton || t == InputType.GamepadAxis) 
				binding.GamepadAxis = (GamepadAxis)EditorGUILayout.EnumPopup("Axis", binding.GamepadAxis);
			
			bool isButton = t == InputType.GamepadButton || t == InputType.KeyButton;
			bool isAxis = t == InputType.GamepadAxis || t == InputType.MouseAxis;
			bool isFakeAxis = t == InputType.GamepadAnalogButton || t == InputType.DigitalAxis;

			if (isAxis) binding.updateAsButton = EditorGUILayout.Toggle("Update As Button", binding.updateAsButton);
			if (isButton) binding.updateAsAxis = EditorGUILayout.Toggle("Update As Axis", binding.updateAsAxis);
			
			bool buttonAsAxis = isButton && binding.updateAsAxis;
			bool axisAsButton = isAxis && binding.updateAsButton;

			if (isAxis || t == InputType.GamepadAnalogButton) {
				binding.DeadZone = EditorGUILayout.FloatField(m_deadZoneInfo, binding.DeadZone);
			}

			if (t == InputType.DigitalAxis || buttonAsAxis)  {
				binding.Gravity = EditorGUILayout.FloatField(m_gravityInfo, binding.Gravity);
				binding.SnapWhenReadAsAxis = EditorGUILayout.Toggle(m_snapInfo, binding.SnapWhenReadAsAxis);
			}

			if (isFakeAxis || axisAsButton) {
				binding.useNegativeAxisForButton = EditorGUILayout.Toggle("Use Negative Axis For Button Query", binding.useNegativeAxisForButton);
			}

			binding.rebindable = EditorGUILayout.Toggle("Rebindable", binding.rebindable);
			
			if (isFakeAxis || isAxis || buttonAsAxis) {
				binding.Sensitivity = EditorGUILayout.FloatField(m_sensitivityInfo, binding.Sensitivity);
				binding.sensitivityEditable = EditorGUILayout.Toggle("Sensitivity Editable", binding.sensitivityEditable);
				binding.InvertWhenReadAsAxis = EditorGUILayout.Toggle("Invert When Axis Query", binding.InvertWhenReadAsAxis);
				binding.invertEditable = EditorGUILayout.Toggle("Invert Editable", binding.invertEditable);
			}
			
			GUILayout.EndArea();

			Rect r = new Rect(position.width - 25.0f, position.y + 2, 20.0f, 20.0f);
			if(action.bindings.Count < InputAction.MAX_BINDINGS) if(GUI.Button(r, m_plusButtonContent, GUITools.label)) result = CollectionAction.Add;
			r.x -= r.width;
			if(GUI.Button(r, m_minusButtonContent, GUITools.label)) result = CollectionAction.Remove;
			r.x -= r.width;
			if(GUI.Button(r, m_upButtonContent, GUITools.label)) result = CollectionAction.MoveUp;
			r.x -= r.width;
			if(GUI.Button(r, m_downButtonContent, GUITools.label)) result = CollectionAction.MoveDown;
			r.x -= r.width;
			
			return result;
		}

		#endregion

		#region [Utility]
		
		void EnsureGUIStyles()
		{
			if(m_warningLabel == null) {
				m_warningLabel = new GUIStyle(EditorStyles.largeLabel) {
					alignment = TextAnchor.MiddleCenter,
					fontStyle = FontStyle.Bold,
					fontSize = 14
				};
			}
		}
		
		public void OnPlayStateChanged (PlayModeStateChange state) {
			if (state == PlayModeStateChange.ExitingPlayMode)
				DisplaySaveDialogue();	
			if (state == PlayModeStateChange.EnteredEditMode)
				InitializeLoadedElements();
		}


		float CalculateInputActionViewRectHeight(InputAction action)
		{
			float height = INPUT_FIELD_HEIGHT * 2 + FIELD_SPACING * 2 + INPUT_ACTION_SPACING;
			if(action.bindings.Count > 0) {
				foreach(var binding in action.bindings) height += CalculateInputBindingViewRectHeight(binding) + INPUT_BINDING_SPACING;
				height += 15.0f;
			}
			else height += BUTTON_HEIGHT;
			return height;
		}

		float CalculateInputBindingViewRectHeight(InputBinding binding)
		{

			int numberOfFields = 3;

			InputType t = binding.Type;
			if (t== InputType.KeyButton || t == InputType.DigitalAxis) numberOfFields++;
			if(t == InputType.DigitalAxis) numberOfFields++;
			if(t == InputType.MouseAxis) numberOfFields++;
			if(t == InputType.GamepadButton) numberOfFields++;
			if(t == InputType.GamepadAnalogButton || t == InputType.GamepadAxis) numberOfFields++;
			
			bool isButton = t == InputType.GamepadButton || t == InputType.KeyButton;
			bool isAxis = t == InputType.GamepadAxis || t == InputType.MouseAxis;
			bool isFakeAxis = t == InputType.GamepadAnalogButton || t == InputType.DigitalAxis;

			if (isAxis) numberOfFields++;
			if (isButton) numberOfFields++;
			
			bool buttonAsAxis = isButton && binding.updateAsAxis;
			bool axisAsButton = isAxis && binding.updateAsButton;

			if (t == InputType.DigitalAxis || buttonAsAxis) numberOfFields+=2;
			if (isAxis || t == InputType.GamepadAnalogButton) numberOfFields++;
			if (isFakeAxis || axisAsButton) numberOfFields++;
			if (isFakeAxis || isAxis || buttonAsAxis) numberOfFields+=4;
			

			float height = INPUT_FIELD_HEIGHT * numberOfFields + FIELD_SPACING * numberOfFields + 10.0f;
			if(t == InputType.KeyButton && (Event.current == null || Event.current.type != EventType.KeyUp)) {
				if(IsGenericJoystickButton(binding.Positive)) height += JOYSTICK_WARNING_SPACING + JOYSTICK_WARNING_HEIGHT;
			}

			return height;
		}

		float ValuePP(ref float height, float amount)
		{
			float value = height;
			height += amount;
			return value;
		}

		bool IsGenericJoystickButton(KeyCode keyCode)
		{
			return (int)keyCode >= (int)KeyCode.JoystickButton0 && (int)keyCode <= (int)KeyCode.JoystickButton19;
		}

		#endregion

		
	}
}
