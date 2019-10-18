using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;

using UnityEngine;



using UnityTools;

namespace CustomInputManager
{
	public class InputLoaderXML 
	{
		string fileName;
		TextReader textReader;

		public InputLoaderXML(string filename)
		{
			if(filename == null) throw new ArgumentNullException("filename");
			fileName = filename;
			textReader = null;
		}
		
		public InputLoaderXML(TextReader reader)
		{
			if(reader == null) throw new ArgumentNullException("reader");
			textReader = reader;
			fileName = null;
		}

		public List<ControlScheme> Load()
		{
			XmlDocument doc = fileName != null ? XMLTools.LoadXML(fileName) : XMLTools.LoadXML(textReader);
			if(doc != null) return LoadControlSchemes(doc);
			Debug.LogError("couldnt load xml doc");
			return null;
		}

		public ControlScheme Load(string schemeName)
		{
			if(string.IsNullOrEmpty(schemeName)) return null;
			XmlDocument doc = fileName != null ? XMLTools.LoadXML(fileName) : XMLTools.LoadXML(textReader);
			if(doc != null) return LoadControlScheme(doc, schemeName);
			Debug.LogError("couldnt load xml doc");
			return null;
		}

		#region [V2]
		List<ControlScheme> LoadControlSchemes(XmlDocument doc)
		{
			List<ControlScheme> saveData = new List<ControlScheme>();
			foreach(XmlNode n in XMLTools.SelectSubNodesByName(doc.DocumentElement, "ControlScheme")) saveData.Add(ReadControlScheme(n));
			return saveData;
		}

		ControlScheme LoadControlScheme(XmlDocument doc, string schemeName)
		{
			foreach(XmlNode controlSchemeNode in XMLTools.SelectSubNodesByName(doc.DocumentElement, "ControlScheme")) {
				if(XMLTools.ReadAttribute(controlSchemeNode, "name") == schemeName) 
					return ReadControlScheme(controlSchemeNode);
			}
			return null;
		}

		ControlScheme ReadControlScheme(XmlNode controlSchemeNode)
		{
			ControlScheme scheme = new ControlScheme(XMLTools.ReadAttribute(controlSchemeNode, "name", "Unnamed Control Scheme"));
			foreach(XmlNode inputActionNode in XMLTools.SelectSubNodesByName(controlSchemeNode, "Action")) 
				ReadInputAction(scheme, inputActionNode);
			return scheme;
		}

		void ReadInputAction(ControlScheme scheme, XmlNode inputActionNode)
		{
			string name = XMLTools.ReadAttribute(inputActionNode, "name", "Unnamed Action");
			InputAction action = scheme.CreateNewAction(name, XMLTools.ReadAttribute(inputActionNode, "displayName", name));
			foreach(XmlNode inputBindingNode in XMLTools.SelectSubNodesByName(inputActionNode, "Binding")) 
				ReadInputBinding(action, inputBindingNode);
		}

		void ReadInputBinding(InputAction action, XmlNode inputBindingNode)
		{
			InputBinding binding = action.CreateNewBinding();
			foreach(XmlNode n in inputBindingNode.ChildNodes)
			{
				switch(n.LocalName)
				{
					case "Positive"				: binding.Positive 					= XMLTools.ReadAsEnum(n, KeyCode.None); break;
					case "Negative"				: binding.Negative 					= XMLTools.ReadAsEnum(n, KeyCode.None); break;
					case "DeadZone"				: binding.DeadZone 					= XMLTools.ReadAsFloat(n); break;
					case "Gravity"				: binding.Gravity 					= XMLTools.ReadAsFloat(n, 1.0f); break;
					case "Sensitivity"			: binding.Sensitivity 				= XMLTools.ReadAsFloat(n, 1.0f); break;
					case "Snap"					: binding.SnapWhenReadAsAxis 		= XMLTools.ReadAsBool(n); break;
					case "Invert"				: binding.InvertWhenReadAsAxis 		= XMLTools.ReadAsBool(n); break;
					case "UseNeg"				: binding.useNegativeAxisForButton 	= XMLTools.ReadAsBool(n); break;
					case "Rebindable"			: binding.rebindable 				= XMLTools.ReadAsBool(n); break;
					case "SensitivityEditable"	: binding.sensitivityEditable 		= XMLTools.ReadAsBool(n); break;
					case "InvertEditable"		: binding.invertEditable 			= XMLTools.ReadAsBool(n); break;
					case "Type"					: binding.Type 						= XMLTools.ReadAsEnum(n, InputType.KeyButton); break;
					case "Axis"					: binding.MouseAxis 				= XMLTools.ReadAsInt(n); break;
					case "GamepadButton"		: binding.GamepadButton 			= XMLTools.ReadAsEnum(n, GamepadButton.None); break;
					case "GamepadAxis"			: binding.GamepadAxis 				= XMLTools.ReadAsEnum(n, GamepadAxis.None); break;
					case "UpdateAsAxis"			: binding.updateAsAxis 				= XMLTools.ReadAsBool(n); break;
					case "UpdateAsButton"		: binding.updateAsButton 			= XMLTools.ReadAsBool(n); break;
				}

			}
		}
		#endregion

	}
}
