
using UnityEngine;
using UnityEditor;
// using UnityEditor.IMGUI.Controls;
// using System.Linq;
using System.Collections.Generic;
using CustomInputManager;
// using UnityInputConverter;
using UnityObject = UnityEngine.Object;

namespace CustomInputManagerEditor.IO
{
		
	public partial class InputEditor : EditorWindow
	{
		#region [Fields]
		[SerializeField] private InputManager m_inputManager;

		
		
		[SerializeField] private Selection m_selection;
		[SerializeField] private Vector2 m_hierarchyScrollPos = Vector2.zero;
		[SerializeField] private Vector2 m_mainPanelScrollPos = Vector2.zero;
		[SerializeField] private float m_hierarchyPanelWidth = MENU_WIDTH * 2;
		[SerializeField] private Texture2D m_highlightTexture;
		
		private GUIContent m_gravityInfo;
		private GUIContent m_sensitivityInfo;
		private GUIContent m_snapInfo;
		private GUIContent m_deadZoneInfo;
		private GUIContent m_plusButtonContent;
		private GUIContent m_minusButtonContent;
		private GUIContent m_upButtonContent;
		private GUIContent m_downButtonContent;
		private InputAction m_copySource;
		private KeyCodeField[] m_keyFields;
		private GUIStyle m_whiteLabel;
		private GUIStyle m_whiteFoldout;
		private GUIStyle m_warningLabel;
		private bool m_isResizingHierarchy = false;
		private bool m_tryedToFindInputManagerInScene = false;
		private bool m_isDisposed = false;
		#endregion

		#region [Startup]
		private void OnEnable()
		{
			m_gravityInfo = new GUIContent("Gravity When Axis Query", "The speed(in units/sec) at which a digital axis falls towards neutral.");
			m_sensitivityInfo = new GUIContent("Sensitivity When Axis Query", "The speed(in units/sec) at which an axis moves towards the target value.");
			m_snapInfo = new GUIContent("Snap When Axis Query", "If input switches direction, do we snap to neutral and continue from there?");// For digital axes only.");
			m_deadZoneInfo = new GUIContent("Dead Zone", "Size of analog dead zone. Values within this range map to neutral.");
			m_plusButtonContent = new GUIContent(EditorToolbox.GetUnityIcon("ol plus"));
			m_minusButtonContent = new GUIContent(EditorToolbox.GetUnityIcon("ol minus"));
			m_upButtonContent = new GUIContent(EditorToolbox.GetCustomIcon("input_editor_arrow_up"));
			m_downButtonContent = new GUIContent(EditorToolbox.GetCustomIcon("input_editor_arrow_down"));

			
			CreateKeyFields();

			EditorToolbox.ShowStartupWarning();
			IsOpen = true;

			m_tryedToFindInputManagerInScene = false;
			m_inputManager = UnityObject.FindObjectOfType<InputManager>();
			
			if(m_selection == null)
				m_selection = new Selection();
			if(m_highlightTexture == null)
				CreateHighlightTexture();

			EditorApplication.playModeStateChanged += HandlePlayModeChanged;
			m_isDisposed = false;
		}

		private void OnDisable()
		{
			Dispose();
		}

		private void OnDestroy()
		{
			Dispose();
		}

		private void Dispose()
		{
			if(!m_isDisposed)
			{
				IsOpen = false;
				Texture2D.DestroyImmediate(m_highlightTexture);
				m_highlightTexture = null;
				m_copySource = null;

				EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
				m_isDisposed = true;
			}
		}
		#endregion

		#region [Menus]
		private void CreateFileMenu(Rect position)
		{
			GenericMenu fileMenu = new GenericMenu();
			fileMenu.AddItem(new GUIContent("Overwrite Project Settings"), false, HandleFileMenuOption, FileMenuOptions.OverwriteProjectSettings);
			
			fileMenu.AddSeparator("");
			if(m_inputManager.ControlSchemes.Count > 0)
				fileMenu.AddItem(new GUIContent("Create Snapshot"), false, HandleFileMenuOption, FileMenuOptions.CreateSnapshot);
			else
				fileMenu.AddDisabledItem(new GUIContent("Create Snapshot"));

			if(EditorToolbox.CanLoadSnapshot())
				fileMenu.AddItem(new GUIContent("Restore Snapshot"), false, HandleFileMenuOption, FileMenuOptions.LoadSnapshot);
			else
				fileMenu.AddDisabledItem(new GUIContent("Restore Snapshot"));
			fileMenu.AddSeparator("");

			if(m_inputManager.ControlSchemes.Count > 0)
				fileMenu.AddItem(new GUIContent("Export"), false, HandleFileMenuOption, FileMenuOptions.Export);
			else
				fileMenu.AddDisabledItem(new GUIContent("Export"));

			fileMenu.AddItem(new GUIContent("Import"), false, HandleFileMenuOption, FileMenuOptions.Import);
			
			fileMenu.DropDown(position);
		}

		private void HandleFileMenuOption(object arg)
		{
			FileMenuOptions option = (FileMenuOptions)arg;
			switch(option)
			{
			case FileMenuOptions.OverwriteProjectSettings:
				EditorToolbox.OverwriteProjectSettings();
				break;
			case FileMenuOptions.CreateSnapshot:
				EditorToolbox.CreateSnapshot(m_inputManager);
				break;
			case FileMenuOptions.LoadSnapshot:
				EditorToolbox.LoadSnapshot(m_inputManager);
				break;
			case FileMenuOptions.Export:
				ExportInputProfile();
				break;
			case FileMenuOptions.Import:
				ImportInputProfile();
				break;
			}
		}

		private void CreateEditMenu(Rect position)
		{
			GenericMenu editMenu = new GenericMenu();
			editMenu.AddItem(new GUIContent("New Control Scheme"), false, HandleEditMenuOption, EditMenuOptions.NewControlScheme);
			if(m_selection.IsControlSchemeSelected)
				editMenu.AddItem(new GUIContent("New Action"), false, HandleEditMenuOption, EditMenuOptions.NewInputAction);
			else
				editMenu.AddDisabledItem(new GUIContent("New Action"));
			editMenu.AddSeparator("");

			if(!m_selection.IsEmpty)
				editMenu.AddItem(new GUIContent("Duplicate"), false, HandleEditMenuOption, EditMenuOptions.Duplicate);
			else
				editMenu.AddDisabledItem(new GUIContent("Duplicate"));

			if(!m_selection.IsEmpty)
				editMenu.AddItem(new GUIContent("Delete"), false, HandleEditMenuOption, EditMenuOptions.Delete);
			else
				editMenu.AddDisabledItem(new GUIContent("Delete"));

			if(m_inputManager.ControlSchemes.Count > 0)
				editMenu.AddItem(new GUIContent("Delete All"), false, HandleEditMenuOption, EditMenuOptions.DeleteAll);
			else
				editMenu.AddDisabledItem(new GUIContent("Delete All"));

			if(m_selection.IsActionSelected)
				editMenu.AddItem(new GUIContent("Copy"), false, HandleEditMenuOption, EditMenuOptions.Copy);
			else
				editMenu.AddDisabledItem(new GUIContent("Copy"));

			if(m_copySource != null && m_selection.IsActionSelected)
				editMenu.AddItem(new GUIContent("Paste"), false, HandleEditMenuOption, EditMenuOptions.Paste);
			else
				editMenu.AddDisabledItem(new GUIContent("Paste"));

			
			editMenu.DropDown(position);
		}

		private void HandleEditMenuOption(object arg)
		{
			EditMenuOptions option = (EditMenuOptions)arg;
			switch(option)
			{
			case EditMenuOptions.NewControlScheme:
				CreateNewControlScheme();
				break;
			case EditMenuOptions.NewInputAction:
				CreateNewInputAction();
				break;
			case EditMenuOptions.Duplicate:
				Duplicate();
				break;
			case EditMenuOptions.Delete:
				Delete();
				break;
			case EditMenuOptions.DeleteAll:
				DeleteAll();
				break;
			case EditMenuOptions.Copy:
				CopyInputAction();
				break;
			case EditMenuOptions.Paste:
				PasteInputAction();
				break;
			}
		}

		private void CreateControlSchemeContextMenu(Rect position)
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("New Action"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.NewInputAction);
			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.Duplicate);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.Delete);
			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Move Up"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.MoveUp);
			contextMenu.AddItem(new GUIContent("Move Down"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.MoveDown);

			contextMenu.DropDown(position);
		}

		private void HandleControlSchemeContextMenuOption(object arg)
		{
			ControlSchemeContextMenuOptions option = (ControlSchemeContextMenuOptions)arg;
			switch(option)
			{
			case ControlSchemeContextMenuOptions.NewInputAction:
				CreateNewInputAction();
				break;
			case ControlSchemeContextMenuOptions.Duplicate:
				Duplicate();
				break;
			case ControlSchemeContextMenuOptions.Delete:
				Delete();
				break;
			case ControlSchemeContextMenuOptions.MoveUp:
				ReorderControlScheme(MoveDirection.Up);
				break;
			case ControlSchemeContextMenuOptions.MoveDown:
				ReorderControlScheme(MoveDirection.Down);
				break;
			}
		}

		private void CreateInputActionContextMenu(Rect position)
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.Duplicate);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.Delete);
			contextMenu.AddItem(new GUIContent("Copy"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.Copy);
			contextMenu.AddItem(new GUIContent("Paste"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.Paste);
			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Move Up"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.MoveUp);
			contextMenu.AddItem(new GUIContent("Move Down"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.MoveDown);

			contextMenu.DropDown(position);
		}

		private void HandleInputActionContextMenuOption(object arg)
		{
			InputActionContextMenuOptions option = (InputActionContextMenuOptions)arg;
			switch(option)
			{
			case InputActionContextMenuOptions.Duplicate:
				Duplicate();
				break;
			case InputActionContextMenuOptions.Delete:
				Delete();
				break;
			case InputActionContextMenuOptions.Copy:
				CopyInputAction();
				break;
			case InputActionContextMenuOptions.Paste:
				PasteInputAction();
				break;
			case InputActionContextMenuOptions.MoveUp:
				ReorderInputAction(MoveDirection.Up);
				break;
			case InputActionContextMenuOptions.MoveDown:
				ReorderInputAction(MoveDirection.Down);
				break;
			}
		}

		private void CreateNewControlScheme()
		{
			m_inputManager.ControlSchemes.Add(new ControlScheme());
			m_selection.Reset();
			m_selection.ControlScheme = m_inputManager.ControlSchemes.Count - 1;
			Repaint();
		}

		private void CreateNewInputAction()
		{
			if(m_selection.IsControlSchemeSelected)
			{
				ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
				scheme.CreateNewAction("New Action", "New Action Display Name");
				scheme.IsExpanded = true;
				m_selection.Action = scheme.Actions.Count - 1;
				ResetKeyFields();
				Repaint();
			}
		}

		private void Duplicate()
		{
			if(m_selection.IsActionSelected)
			{
				DuplicateInputAction();
			}
			else if(m_selection.IsControlSchemeSelected)
			{
				DuplicateControlScheme();
			}
		}

		public static InputAction DuplicateInputAction(string name, InputAction source)
		{
			InputAction duplicate = new InputAction("_");
			CopyInputAction(duplicate, source);
			return duplicate;
		}


		public InputAction InsertNewActionScheme(ControlScheme scheme, int index, string name, InputAction source)
		{
			InputAction action = DuplicateInputAction(name, source);
			scheme.Actions.Insert(index, action);
			return action;
		}

		private void DuplicateInputAction()
		{
			ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
			InputAction source = scheme.Actions[m_selection.Action];

			InsertNewActionScheme(scheme, m_selection.Action + 1, source.Name + " Copy", source);
			m_selection.Action++;

			Repaint();
		}


		public static ControlScheme DuplicateControlScheme(string name, ControlScheme source)
		{
			ControlScheme duplicate = new ControlScheme();
			duplicate.Name = name;
			duplicate.UniqueID = ControlScheme.GenerateUniqueID(); 
			duplicate.Actions = new List<InputAction>();
			foreach(var action in source.Actions)
			{
				duplicate.Actions.Add(DuplicateInputAction(action));
			}
			return duplicate;
		}

		private void DuplicateControlScheme()
		{
			ControlScheme source = m_inputManager.ControlSchemes[m_selection.ControlScheme];

			m_inputManager.ControlSchemes.Insert(m_selection.ControlScheme + 1, DuplicateControlScheme(source.Name + " Copy", source));
			m_selection.ControlScheme++;

			Repaint();
		}


		public void DeleteSchemeAction(ControlScheme scheme, InputAction action)
		{
			scheme.Actions.Remove(action);
		}

		public void DeleteSchemeAction(ControlScheme scheme, int index)
		{
			if(index >= 0 && index < scheme.Actions.Count)
				scheme.Actions.RemoveAt(index);
		}

		private void Delete()
		{
			if(m_selection.IsActionSelected)
			{
				ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];

				DeleteSchemeAction(scheme, m_selection.Action);
				
			}
			else if(m_selection.IsControlSchemeSelected)
			{
				m_inputManager.ControlSchemes.RemoveAt(m_selection.ControlScheme);
			}

			m_selection.Reset();
			Repaint();
		}

		private void DeleteAll()
		{
			m_inputManager.ControlSchemes.Clear();

			m_selection.Reset();
			Repaint();
		}

		public static InputAction DuplicateInputAction(InputAction source)
		{
			return DuplicateInputAction(source.Name, source);
		}

		private void CopyInputAction()
		{
			ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
			m_copySource = DuplicateInputAction(scheme.Actions[m_selection.Action]);
		}

		public static void CopyInputAction(InputAction a, InputAction source)
		{

			a.Name = source.Name;
			a.displayName = source.displayName;
			a.bindings.Clear();
			foreach(var binding in source.bindings)
			{
				a.bindings.Add(DuplicateInputBinding(binding));
			}
		}
		public static InputBinding DuplicateInputBinding(InputBinding source)
		{
			InputBinding duplicate = new InputBinding();
			duplicate.Copy(source);
			return duplicate;
		}
		

		private void PasteInputAction()
		{
			ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
			InputAction action = scheme.Actions[m_selection.Action];

			CopyInputAction(action, m_copySource);
			
		}

		private void ReorderControlScheme(MoveDirection dir)
		{
			if(m_selection.IsControlSchemeSelected)
			{
				var index = m_selection.ControlScheme;

				if(dir == MoveDirection.Up && index > 0)
				{
					var temp = m_inputManager.ControlSchemes[index];
					m_inputManager.ControlSchemes[index] = m_inputManager.ControlSchemes[index - 1];
					m_inputManager.ControlSchemes[index - 1] = temp;

					m_selection.Reset();
					m_selection.ControlScheme = index - 1;
				}
				else if(dir == MoveDirection.Down && index < m_inputManager.ControlSchemes.Count - 1)
				{
					var temp = m_inputManager.ControlSchemes[index];
					m_inputManager.ControlSchemes[index] = m_inputManager.ControlSchemes[index + 1];
					m_inputManager.ControlSchemes[index + 1] = temp;

					m_selection.Reset();
					m_selection.ControlScheme = index + 1;
				}
			}
		}

		public void SwapActions(ControlScheme scheme, int fromIndex, int toIndex)
		{
			if(fromIndex >= 0 && fromIndex < scheme.Actions.Count && toIndex >= 0 && toIndex < scheme.Actions.Count)
			{
				var temp = scheme.Actions[toIndex];
				scheme.Actions[toIndex] = scheme.Actions[fromIndex];
				scheme.Actions[fromIndex] = temp;
			}
		}

		private void ReorderInputAction(MoveDirection dir)
		{
			if(m_selection.IsActionSelected)
			{
				var scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
				var schemeIndex = m_selection.ControlScheme;
				var actionIndex = m_selection.Action;

				if(dir == MoveDirection.Up && actionIndex > 0)
				{
					SwapActions(scheme, actionIndex, actionIndex - 1);
					
					m_selection.Reset();
					m_selection.ControlScheme = schemeIndex;
					m_selection.Action = actionIndex - 1;
				}
				else if(dir == MoveDirection.Down && actionIndex < scheme.Actions.Count - 1)
				{
					SwapActions(scheme, actionIndex, actionIndex + 1);

					m_selection.Reset();
					m_selection.ControlScheme = schemeIndex;
					m_selection.Action = actionIndex + 1;
				}
			}
		}

		
		#endregion

		#region [OnGUI]
		private void OnGUI()
		{
			EnsureGUIStyles();

			if(m_inputManager == null && !m_tryedToFindInputManagerInScene)
				TryToFindInputManagerInScene();

			if(m_inputManager == null)
			{
				DrawMissingInputManagerWarning();
				return;
			}

			ValidateSelection();

			Undo.RecordObject(m_inputManager, "InputManager");
			if(m_selection.IsControlSchemeSelected)
			{
				DrawMainPanel();
			}

			UpdateHierarchyPanelWidth();


			DrawHierarchyPanel();


			DrawMainToolbar();
			if(GUI.changed)
			{
				EditorUtility.SetDirty(m_inputManager);
			}
			// UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(m_inputManager.gameObject.scene);
		}

		private void ValidateSelection()
		{
			if(m_inputManager == null || m_inputManager.ControlSchemes.Count <= 0)
				m_selection.Reset();
			else if(m_selection.IsControlSchemeSelected && m_selection.ControlScheme >= m_inputManager.ControlSchemes.Count)
				m_selection.Reset();
			else if(m_selection.IsActionSelected && m_selection.Action >= m_inputManager.ControlSchemes[m_selection.ControlScheme].Actions.Count)
				m_selection.Reset();
		}

		private void DrawMissingInputManagerWarning()
		{
			Rect warningRect = new Rect(0.0f, 20.0f, position.width, 40.0f);
			Rect buttonRect = new Rect(position.width / 2 - 100.0f, warningRect.yMax, 200.0f, 25.0f);

			EditorGUI.LabelField(warningRect, "Could not find an input manager instance in the scene!", m_warningLabel);
			if(GUI.Button(buttonRect, "Try Again"))
			{
				TryToFindInputManagerInScene();
			}
		}

		private void DrawMainToolbar()
		{
			Rect screenRect = new Rect(0.0f, 0.0f, position.width, TOOLBAR_HEIGHT);
			Rect fileMenuRect = new Rect(0.0f, 0.0f, MENU_WIDTH, screenRect.height);
			Rect editMenuRect = new Rect(fileMenuRect.xMax, 0.0f, MENU_WIDTH, screenRect.height);
			Rect paddingLabelRect = new Rect(editMenuRect.xMax, 0.0f, screenRect.width - MENU_WIDTH * 2, screenRect.height);
			Rect searchFieldRect = new Rect(screenRect.width - (MENU_WIDTH * 1.5f + 5.0f), 2.0f, MENU_WIDTH * 1.5f, screenRect.height - 2.0f);
			
			GUI.BeginGroup(screenRect);
			DrawFileMenu(fileMenuRect);
			DrawEditMenu(editMenuRect);
			EditorGUI.LabelField(paddingLabelRect, "", EditorStyles.toolbarButton);
			
			GUI.EndGroup();
		}

		private void DrawFileMenu(Rect screenRect)
		{
			EditorGUI.LabelField(screenRect, "File", EditorStyles.toolbarDropDown);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
				screenRect.Contains(Event.current.mousePosition))
			{
				CreateFileMenu(new Rect(screenRect.x, screenRect.yMax, 0.0f, 0.0f));
			}
		}

		private void DrawEditMenu(Rect screenRect)
		{
			EditorGUI.LabelField(screenRect, "Edit", EditorStyles.toolbarDropDown);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
				screenRect.Contains(Event.current.mousePosition))
			{
				CreateEditMenu(new Rect(screenRect.x, screenRect.yMax, 0.0f, 0.0f));
			}
		}

		private void UpdateHierarchyPanelWidth()
		{
			float cursorRectWidth = m_isResizingHierarchy ? MAX_CURSOR_RECT_WIDTH : MIN_CURSOR_RECT_WIDTH;
			Rect cursorRect = new Rect(m_hierarchyPanelWidth - cursorRectWidth / 2, TOOLBAR_HEIGHT, cursorRectWidth,
										position.height - TOOLBAR_HEIGHT);
			Rect resizeRect = new Rect(m_hierarchyPanelWidth - MIN_CURSOR_RECT_WIDTH / 2, 0.0f,
										MIN_CURSOR_RECT_WIDTH, position.height);

			EditorGUIUtility.AddCursorRect(cursorRect, MouseCursor.ResizeHorizontal);
			switch(Event.current.type)
			{
			case EventType.MouseDown:
				if(Event.current.button == 0 && resizeRect.Contains(Event.current.mousePosition))
				{
					m_isResizingHierarchy = true;
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if(Event.current.button == 0 && m_isResizingHierarchy)
				{
					m_isResizingHierarchy = false;
					Event.current.Use();
				}
				break;
			case EventType.MouseDrag:
				if(m_isResizingHierarchy)
				{
					m_hierarchyPanelWidth = Mathf.Clamp(m_hierarchyPanelWidth + Event.current.delta.x,
													 MIN_HIERARCHY_PANEL_WIDTH, position.width / 2);
					Event.current.Use();
					Repaint();
				}
				break;
			default:
				break;
			}
		}

		private void DrawHierarchyPanel()
		{
			Rect screenRect = new Rect(0.0f, TOOLBAR_HEIGHT - 5.0f, m_hierarchyPanelWidth, position.height - TOOLBAR_HEIGHT + 10.0f);
			Rect scrollView = new Rect(screenRect.x, screenRect.y + 5.0f, screenRect.width, position.height - screenRect.y);
			Rect viewRect = new Rect(0.0f, 0.0f, scrollView.width, CalculateHierarchyViewRectHeight());
			float itemPosY = 0.0f;

			GUI.Box(screenRect, "");
			m_hierarchyScrollPos = GUI.BeginScrollView(scrollView, m_hierarchyScrollPos, viewRect);
			for(int i = 0; i < m_inputManager.ControlSchemes.Count; i++)
			{
				Rect csRect = new Rect(1.0f, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
				DrawHierarchyControlSchemeItem(csRect, i);
				itemPosY += HIERARCHY_ITEM_HEIGHT;

				if(m_inputManager.ControlSchemes[i].IsExpanded)
				{
					for(int j = 0; j < m_inputManager.ControlSchemes[i].Actions.Count; j++)
					{
						Rect iaRect = new Rect(1.0f, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
						DrawHierarchyInputActionItem(iaRect, i, j);
						itemPosY += HIERARCHY_ITEM_HEIGHT;
					}
				}
			}
			GUI.EndScrollView();
		}

		private void DrawHierarchyControlSchemeItem(Rect position, int index)
		{
			ControlScheme scheme = m_inputManager.ControlSchemes[index];
			Rect foldoutRect = new Rect(5.0f, 1.0f, 10, position.height - 1.0f);
			Rect nameRect = new Rect(foldoutRect.xMax + 5.0f, 1.0f, position.width - (foldoutRect.xMax + 5.0f), position.height - 1.0f);

			if(Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
			{
				if(position.Contains(Event.current.mousePosition))
				{
					m_selection.Reset();
					m_selection.ControlScheme = index;
					GUI.FocusControl(null);
					Repaint();

					if(Event.current.button == 1)
					{
						CreateControlSchemeContextMenu(new Rect(Event.current.mousePosition, Vector2.zero));
					}
				}
			}

			GUI.BeginGroup(position);
			if(m_selection.IsControlSchemeSelected && !m_selection.IsActionSelected && m_selection.ControlScheme == index)
			{
				GUI.DrawTexture(new Rect(0, 0, position.width, position.height), m_highlightTexture, ScaleMode.StretchToFill);
				scheme.IsExpanded = EditorGUI.Foldout(foldoutRect, scheme.IsExpanded, GUIContent.none);
				EditorGUI.LabelField(nameRect, scheme.Name, m_whiteLabel);
			}
			else
			{
				scheme.IsExpanded = EditorGUI.Foldout(foldoutRect, scheme.IsExpanded, GUIContent.none);
				EditorGUI.LabelField(nameRect, scheme.Name);
			}
			GUI.EndGroup();
		}

		private void DrawHierarchyInputActionItem(Rect position, int controlSchemeIndex, int index)
		{
			InputAction action = m_inputManager.ControlSchemes[controlSchemeIndex].Actions[index];
			Rect nameRect = new Rect(HIERARCHY_INDENT_SIZE, 1.0f, position.width - HIERARCHY_INDENT_SIZE, position.height - 1.0f);

			if(Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
			{
				if(position.Contains(Event.current.mousePosition))
				{
					m_selection.Reset();
					m_selection.ControlScheme = controlSchemeIndex;
					m_selection.Action = index;
					ResetKeyFields();
					Event.current.Use();
					GUI.FocusControl(null);
					Repaint();

					if(Event.current.button == 1)
					{
						CreateInputActionContextMenu(new Rect(Event.current.mousePosition, Vector2.zero));
					}
				}
			}

			GUI.BeginGroup(position);
			if(m_selection.IsActionSelected && m_selection.ControlScheme == controlSchemeIndex && m_selection.Action == index)
			{
				GUI.DrawTexture(new Rect(0, 0, position.width, position.height), m_highlightTexture, ScaleMode.StretchToFill);
				EditorGUI.LabelField(nameRect, action.Name, m_whiteLabel);
			}
			else
			{
				EditorGUI.LabelField(nameRect, action.Name);
			}
			GUI.EndGroup();
		}

		private void DrawMainPanel()
		{
			Rect position = new Rect(m_hierarchyPanelWidth, TOOLBAR_HEIGHT,
										this.position.width - m_hierarchyPanelWidth,
										this.position.height - TOOLBAR_HEIGHT);
			ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];

			if(m_selection.IsActionSelected)
			{
				InputAction action = scheme.Actions[m_selection.Action];
				DrawInputActionFields(position, action);
			}
			else
			{
				DrawControlSchemeFields(position, scheme);
			}

			if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if(position.Contains(Event.current.mousePosition))
				{
					Event.current.Use();
					GUI.FocusControl(null);
					Repaint();
				}
			}
		}

		private void DrawControlSchemeFields(Rect position, ControlScheme controlScheme)
		{
			position.x += 5;
			position.y += 5;
			position.width -= 10;

			GUILayout.BeginArea(position);
			controlScheme.Name = EditorGUILayout.TextField("Name", controlScheme.Name);
			EditorGUILayout.LabelField("Description");
			
			GUILayout.EndArea();
		}

		private void DrawInputActionFields(Rect position, InputAction action)
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

			m_mainPanelScrollPos = GUI.BeginScrollView(position, m_mainPanelScrollPos, viewRect);
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
			{
				Repaint();
			}
		}

		public InputBinding InsertNewBinding(InputAction action, int index)
		{
			if(action.bindings.Count < InputAction.MAX_BINDINGS)
			{
				InputBinding binding = new InputBinding();
				action.bindings.Insert(index, binding);

				return binding;
			}

			return null;
		}

		public void SwapBindings(InputAction action, int fromIndex, int toIndex)
		{
			if(fromIndex >= 0 && fromIndex < action.bindings.Count && toIndex >= 0 && toIndex < action.bindings.Count)
			{
				var temp = action.bindings[toIndex];
				action.bindings[toIndex] = action.bindings[fromIndex];
				action.bindings[fromIndex] = temp;
			}
		}

		private CollectionAction DrawInputBindingFields(Rect position, string label, InputAction action, int bindingIndex)
		{
			Rect headerRect = new Rect(position.x + 5.0f, position.y, position.width, INPUT_FIELD_HEIGHT);
			Rect removeButtonRect = new Rect(position.width - 25.0f, position.y + 2, 20.0f, 20.0f);
			Rect addButtonRect = new Rect(removeButtonRect.x - 20.0f, position.y + 2, 20.0f, 20.0f);
			Rect downButtonRect = new Rect(addButtonRect.x - 20.0f, position.y + 2, 20.0f, 20.0f);
			Rect upButtonRect = new Rect(downButtonRect.x - 20.0f, position.y + 2, 20.0f, 20.0f);
			Rect layoutArea = new Rect(position.x + 10.0f, position.y + INPUT_FIELD_HEIGHT + FIELD_SPACING + 5.0f, position.width - 12.5f, position.height - (INPUT_FIELD_HEIGHT + FIELD_SPACING + 5.0f));
			InputBinding binding = action.bindings[bindingIndex];
			KeyCode positive = binding.Positive, negative = binding.Negative;
			CollectionAction result = CollectionAction.None;

			//GUI.Box(position, "");
			EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);
			
			GUILayout.BeginArea(layoutArea);
			binding.Type = (InputType)EditorGUILayout.EnumPopup("Type", binding.Type);

			if(binding.Type == InputType.KeyButton || binding.Type == InputType.DigitalAxis) {
				DrawKeyCodeField(action, bindingIndex, KeyType.Positive);
			}

			if(binding.Type == InputType.DigitalAxis) {
				DrawKeyCodeField(action, bindingIndex, KeyType.Negative);
			}

			if(binding.Type == InputType.MouseAxis) {
				binding.MouseAxis = EditorGUILayout.Popup("Axis", binding.MouseAxis, InputBinding.mouseAxisNames);// m_axisOptions);
			}

			if(binding.Type == InputType.GamepadButton) {
				binding.GamepadButton = (GamepadButton)EditorGUILayout.EnumPopup("Button", binding.GamepadButton);
			}

			if(binding.Type == InputType.GamepadAnalogButton || binding.Type == InputType.GamepadAxis) {
				binding.GamepadAxis = (GamepadAxis)EditorGUILayout.EnumPopup("Axis", binding.GamepadAxis);
			}



			if (
				binding.Type == InputType.DigitalAxis ||
				binding.Type == InputType.KeyButton ||
				binding.Type == InputType.GamepadButton ||
				binding.Type == InputType.GamepadAnalogButton
			) {
				binding.Gravity = EditorGUILayout.FloatField(m_gravityInfo, binding.Gravity);
			}

			if (
				binding.Type == InputType.DigitalAxis ||
				binding.Type == InputType.KeyButton ||
				binding.Type == InputType.GamepadButton ||
				binding.Type == InputType.GamepadAnalogButton ||
				binding.Type == InputType.MouseAxis
			) {
				binding.Sensitivity = EditorGUILayout.FloatField(m_sensitivityInfo, binding.Sensitivity);
			}

			if(binding.Type == InputType.GamepadAxis || binding.Type == InputType.GamepadAnalogButton || binding.Type == InputType.MouseAxis) {
				binding.DeadZone = EditorGUILayout.FloatField(m_deadZoneInfo, binding.DeadZone);
			}

			binding.SnapWhenReadAsAxis = EditorGUILayout.Toggle(m_snapInfo, binding.SnapWhenReadAsAxis);

			binding.InvertWhenReadAsAxis = EditorGUILayout.Toggle("Invert When Axis Query", binding.InvertWhenReadAsAxis);


			if( binding.Type == InputType.DigitalAxis || binding.Type == InputType.GamepadAnalogButton || binding.Type == InputType.GamepadAxis || binding.Type == InputType.MouseAxis) {
				binding.useNegativeAxisForButton = EditorGUILayout.Toggle("Use Negative Axis For Button Query", binding.useNegativeAxisForButton);
			}



			binding.rebindable = EditorGUILayout.Toggle("Rebindable", binding.rebindable);
			binding.sensitivityEditable = EditorGUILayout.Toggle("Sensitivity Editable", binding.sensitivityEditable);
			binding.invertEditable = EditorGUILayout.Toggle("Invert Editable", binding.invertEditable);


			GUILayout.EndArea();

			if(action.bindings.Count < InputAction.MAX_BINDINGS)
			{
				if(GUI.Button(addButtonRect, m_plusButtonContent, EditorStyles.label))
				{
					result = CollectionAction.Add;
				}
			}
			if(GUI.Button(removeButtonRect, m_minusButtonContent, EditorStyles.label))
			{
				result = CollectionAction.Remove;
			}
			if(GUI.Button(upButtonRect, m_upButtonContent, EditorStyles.label))
			{
				result = CollectionAction.MoveUp;
			}
			if(GUI.Button(downButtonRect, m_downButtonContent, EditorStyles.label))
			{
				result = CollectionAction.MoveDown;
			}
			return result;
		}

		private void DrawKeyCodeField(InputAction action, int bindingIndex, KeyType keyType)
		{
			InputBinding binding = action.bindings[bindingIndex];
			int kfIndex = bindingIndex * 2;

			if(keyType == KeyType.Positive)
			{
				binding.Positive = m_keyFields[kfIndex].OnGUI("Positive", binding.Positive);
			}
			else
			{
				binding.Negative = m_keyFields[kfIndex + 1].OnGUI("Negative", binding.Negative);
			}
		}
		#endregion

		#region [Utility]
		private void CreateKeyFields()
		{
			m_keyFields = new KeyCodeField[InputAction.MAX_BINDINGS * 2];
			for(int i = 0; i < m_keyFields.Length; i++)
			{
				m_keyFields[i] = new KeyCodeField();
			}
		}

		private void ResetKeyFields()
		{
			for(int i = 0; i < m_keyFields.Length; i++)
			{
				m_keyFields[i].Reset();
			}
		}

		private void CreateHighlightTexture()
		{
			m_highlightTexture = new Texture2D(1, 1);
			m_highlightTexture.SetPixel(0, 0, HIGHLIGHT_COLOR);
			m_highlightTexture.Apply();
		}

		private void EnsureGUIStyles()
		{
			if(m_highlightTexture == null)
			{
				CreateHighlightTexture();
			}
			if(m_whiteLabel == null)
			{
				m_whiteLabel = new GUIStyle(EditorStyles.label);
				m_whiteLabel.normal.textColor = Color.white;
			}
			if(m_whiteFoldout == null)
			{
				m_whiteFoldout = new GUIStyle(EditorStyles.foldout);
				m_whiteFoldout.normal.textColor = Color.white;
				m_whiteFoldout.onNormal.textColor = Color.white;
				m_whiteFoldout.active.textColor = Color.white;
				m_whiteFoldout.onActive.textColor = Color.white;
				m_whiteFoldout.focused.textColor = Color.white;
				m_whiteFoldout.onFocused.textColor = Color.white;
			}
			if(m_warningLabel == null)
			{
				m_warningLabel = new GUIStyle(EditorStyles.largeLabel)
				{
					alignment = TextAnchor.MiddleCenter,
					fontStyle = FontStyle.Bold,
					fontSize = 14
				};
			}
		}

		private void ExportInputProfile()
		{
			string file = EditorUtility.SaveFilePanel("Export input profile", "", "profile.xml", "xml");
			if(string.IsNullOrEmpty(file))
				return;

			InputSaverXML inputSaver = new InputSaverXML(file);
			inputSaver.Save(m_inputManager.ControlSchemes);//.GetSaveData());
			if(file.StartsWith(Application.dataPath))
			{
				AssetDatabase.Refresh();
			}
		}

		private void ImportInputProfile()
		{
			string file = EditorUtility.OpenFilePanel("Import input profile", "", "xml");
			if(string.IsNullOrEmpty(file))
				return;

			bool replace = EditorUtility.DisplayDialog("Replace or Append", "Do you want to replace the current control schemes?", "Replace", "Append");
			if(replace)
			{
				InputLoaderXML inputLoader = new InputLoaderXML(file);
				m_inputManager.SetSaveData(inputLoader.Load());
				m_selection.Reset();
			}
			else
			{
				InputLoaderXML inputLoader = new InputLoaderXML(file);
				var saveData = inputLoader.Load();
				if(saveData != null && saveData.Count > 0)
				
				{
					foreach(var scheme in saveData)
					{
						m_inputManager.ControlSchemes.Add(scheme);
					}
				}
			}

			Repaint();
		}

		private void TryToFindInputManagerInScene()
		{
			m_inputManager = UnityObject.FindObjectOfType<InputManager>();
			m_tryedToFindInputManagerInScene = true;
		}

		private void HandlePlayModeChanged(PlayModeStateChange state)
		{
			if(state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
			{
				TryToFindInputManagerInScene();
			}
		}

		private float CalculateHierarchyViewRectHeight()
		{
			float height = 0.0f;
			foreach(var scheme in m_inputManager.ControlSchemes)
			{
				height += HIERARCHY_ITEM_HEIGHT;
				if(scheme.IsExpanded)
				{
					height += scheme.Actions.Count * HIERARCHY_ITEM_HEIGHT;
				}
			}

			return height;
		}

		private float CalculateInputActionViewRectHeight(InputAction action)
		{
			float height = INPUT_FIELD_HEIGHT * 2 + FIELD_SPACING * 2 + INPUT_ACTION_SPACING;
			if(action.bindings.Count > 0)
			{
				foreach(var binding in action.bindings)
				{
					height += CalculateInputBindingViewRectHeight(binding) + INPUT_BINDING_SPACING;
				}

				height += 15.0f;
			}
			else
			{
				height += BUTTON_HEIGHT;
			}

			return height;
		}

		private float CalculateInputBindingViewRectHeight(InputBinding binding)
		{
			int numberOfFields = 12;
			switch(binding.Type)
			{
			case InputType.KeyButton:
				numberOfFields = 5;
				break;
			case InputType.MouseAxis:
				numberOfFields = 6;
				break;
			case InputType.DigitalAxis:
				numberOfFields = 7;
				break;
			case InputType.GamepadButton:
				numberOfFields = 5;
				break;
			case InputType.GamepadAnalogButton:
				numberOfFields = 7;
				break;
			case InputType.GamepadAxis:
				numberOfFields = 5;
				break;
			}

			numberOfFields += 2;    //	Header and type

			numberOfFields += 3; //public bool rebindable, sensitivityEditable, invertEditable;


			float height = INPUT_FIELD_HEIGHT * numberOfFields + FIELD_SPACING * numberOfFields + 10.0f;
			if(binding.Type == InputType.KeyButton && (Event.current == null || Event.current.type != EventType.KeyUp))
			{
				if(IsGenericJoystickButton(binding.Positive))
					height += JOYSTICK_WARNING_SPACING + JOYSTICK_WARNING_HEIGHT;
			}

			return height;
		}

		private float ValuePP(ref float height, float amount)
		{
			float value = height;
			height += amount;
			return value;
		}

		private bool IsGenericJoystickButton(KeyCode keyCode)
		{
			return (int)keyCode >= (int)KeyCode.JoystickButton0 && (int)keyCode <= (int)KeyCode.JoystickButton19;
		}

		#endregion

		#region [Static Interface]
		public static bool IsOpen { get; private set; }

		[MenuItem("CustomInputManager/Input Manager/Open Input Editor", false, 0)]
		public static void OpenWindow()
		{
			if(!IsOpen)
			{
				if(UnityObject.FindObjectOfType(typeof(InputManager)) == null)
				{
					bool create = EditorUtility.DisplayDialog("Warning", "There is no InputManager instance in the scene. Do you want to create one?", "Yes", "No");
					if(create)
					{
						GameObject gameObject = new GameObject("InputManager");
						gameObject.AddComponent<InputManager>();
					}
					else
					{
						return;
					}
				}
				var window = EditorWindow.GetWindow<InputEditor>("Input Editor");
				window.minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
			}
		}


		public static void CloseWindow()
		{
			if(IsOpen)
			{
				var window = EditorWindow.GetWindow<InputEditor>("Input Editor");
				window.Close();
			}
		}
		#endregion
	}
}
