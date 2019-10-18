using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEditor;
using UnityTools.EditorTools;

namespace CustomInputManager.Editor {

    public class HieararchyGUIElement {
        public GUIContent name;
        public List<HieararchyGUIElement> subElements;
        public Action<Rect> createContextMenu;
        public HieararchyGUIElement (string name, List<HieararchyGUIElement> subElements, Action<Rect> createContextMenu) {
            this.name = new GUIContent(name);
            this.subElements = subElements;
            this.createContextMenu = createContextMenu;
        }
    }

    public class HierarchyGUI
    {

        public void OnEnable () {
            ResetSelections();
        }

        public int[] selections = new int[2];
        public void ResetSelections () {
            for (int i = 0; i < selections.Length; i++) {
                selections[i] = -1;
            }
        }

        public bool Draw (EditorWindow window, Rect pos, bool expandable, List<HieararchyGUIElement> elements, Action<Rect> drawSelected, Action<Rect> drawFileMenu, float hierarchyPanelWidth=150) {//, int[] selections) {
            if (selections == null) {
                selections = new int[2];
                ResetSelections();
            }
            
            
            if ((elements.Count <= 0) || (selections[0] >= elements.Count) || (expandable && selections[1] != -1 && selections[1] >= elements[selections[0]].subElements.Count))
				ResetSelections();					

            DrawMainToolbar(new Rect(pos.x, pos.y, pos.width, EditorGUIUtility.singleLineHeight), drawFileMenu);
            Rect withoutToolbar = new Rect(pos.x, pos.y + EditorGUIUtility.singleLineHeight + 1, pos.width, pos.height - EditorGUIUtility.singleLineHeight);

            bool clicked = DrawHierarchyPanel(window, new Rect(pos.x, withoutToolbar.y, hierarchyPanelWidth, withoutToolbar.height), expandable, elements);
            if (!clicked) {
                DrawMainPanel(window, new Rect(pos.x + hierarchyPanelWidth, withoutToolbar.y, pos.width - hierarchyPanelWidth, withoutToolbar.height), drawSelected);
            }
            return clicked;
        }

        void DrawMainToolbar(Rect pos, Action<Rect> drawFileMenu)
        {
            const float menuWidth = 100.0f;
            if (GUI.Button(new Rect(pos.x, pos.y, menuWidth, pos.height), "Options", GUITools.toolbarDropDown)) 
                drawFileMenu(new Rect(pos.x, pos.y + EditorGUIUtility.singleLineHeight, 0.0f, 0.0f));

            EditorGUI.LabelField(new Rect(pos.x + menuWidth, pos.y, pos.width - menuWidth, pos.height), string.Empty, GUITools.toolbarButton);
		}
        
        float CalculateHeight(List<HieararchyGUIElement> elements, bool expandable) {
            float h = 0;
            for (int i = 0; i < elements.Count; i++) {
                h += EditorGUIUtility.singleLineHeight;
                if (expandable && Expanded(i)) {
                    h += elements[i].subElements.Count * EditorGUIUtility.singleLineHeight;
                }
            }
            return h;
        }
		
        Vector2 m_hierarchyScrollPos = Vector2.zero;
		
        bool DrawHierarchyPanel(EditorWindow window, Rect pos, bool expandable, List<HieararchyGUIElement> elements)
        {

            CreateHighlightTexture();
            
            bool clicked  = false;

			GUI.Box(pos, "");

			m_hierarchyScrollPos = GUI.BeginScrollView(pos, m_hierarchyScrollPos, new Rect(0, 0, pos.width, CalculateHeight(elements, expandable)));
			
            Rect itemRect = new Rect(0, 5, pos.width, EditorGUIUtility.singleLineHeight);
			
            for(int i = 0; i < elements.Count; i++)
			{
				if (DrawBaseHiearchyItem(window, itemRect, i, expandable, elements[i])) clicked = true;
                
				itemRect.y += EditorGUIUtility.singleLineHeight;

                if (expandable && Expanded(i))
				{
					for(int j = 0; j < elements[i].subElements.Count; j++)
					{
					    if (DrawSubElement(window, itemRect, i, j, elements[i].subElements[j])) clicked = true;
                        
						itemRect.y += EditorGUIUtility.singleLineHeight;
					}
				}
			}
			GUI.EndScrollView();

            return clicked;
		}

        bool[] itemsExpanded;
		bool Expanded (int i) {
			if (itemsExpanded == null) itemsExpanded = new bool[i+1];
			if (itemsExpanded.Length <= i) System.Array.Resize(ref itemsExpanded, i+1);
			return !itemsExpanded[i]; // backwards so it starts out open
		}

        bool DrawHiearchyItem(EditorWindow window, Rect pos, int i, int j, bool expandable, HieararchyGUIElement element, float indent)
		{
            bool clicked = false;
			
			if(Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
			{
				if(pos.Contains(Event.current.mousePosition))
				{
					selections[0] = i;
                    selections[1] = j;

                    clicked = true;

					GUI.FocusControl(null);
                    window.Repaint();

					if(Event.current.button == 1)
						element.createContextMenu(new Rect(Event.current.mousePosition, Vector2.zero));
				}
			}

            float foldoutWidth = 10;
            
            if (expandable) itemsExpanded[i] = !EditorGUI.Foldout(new Rect(pos.x + indent, pos.y, foldoutWidth, pos.height), Expanded(i), GUITools.noContent);

            float offset = expandable ? foldoutWidth + indent : indent;
            Rect nameRect = new Rect(pos.x + offset, pos.y, pos.width - offset, pos.height);
            
            bool isSelected = selections[0] == i && selections[1] == j;

            if (isSelected) GUI.DrawTexture(pos, m_highlightTexture, ScaleMode.StretchToFill);    
                
            GUITools.Label(nameRect, element.name, isSelected ? GUITools.white : GUITools.black, GUITools.label);         

			
            return clicked;
		}

        
        bool DrawBaseHiearchyItem(EditorWindow window, Rect position, int index, bool expandable, HieararchyGUIElement element)
		{
            return DrawHiearchyItem(window, position, index, -1, expandable, element, 5);
		}
        bool DrawSubElement(EditorWindow window, Rect position, int i, int j, HieararchyGUIElement subElement)
		{
            return DrawHiearchyItem(window, position, i, j, false, subElement, 30);   
		}

		
        void DrawMainPanel(EditorWindow window, Rect pos, Action<Rect> drawSelected)
        {
            drawSelected(pos);
			
            if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if(pos.Contains(Event.current.mousePosition))
				{
					Event.current.Use();
					GUI.FocusControl(null);
                    window.Repaint();
                }
			}			
		}

        static readonly Color32 HIGHLIGHT_COLOR = new Color32(62, 125, 231, 200);
        static Texture2D m_highlightTexture;
        static void CreateHighlightTexture()
		{
            if (m_highlightTexture == null) {
                m_highlightTexture = new Texture2D(1, 1);
                m_highlightTexture.SetPixel(0, 0, HIGHLIGHT_COLOR);
                m_highlightTexture.Apply();
            }
		}
        static void DisposeHighlightTexture () {
            if (m_highlightTexture != null) {
                Texture2D.DestroyImmediate(m_highlightTexture);
                m_highlightTexture = null;	
            }
        }

        public void Dispose () {
            DisposeHighlightTexture();
        }
        
    }
}


