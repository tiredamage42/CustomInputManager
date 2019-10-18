
using System;
using System.Xml;
using System.Globalization;
using System.Collections.Generic;

using UnityTools;
namespace CustomInputManager
{
	public class InputSaverXML 
	{
		
		private string m_filename;
		
		public InputSaverXML(string filename)
		{
			if(filename == null) throw new ArgumentNullException("filename");
			m_filename = filename;
		}

		List<ControlScheme> controlSchemes;
		public void Save(List<ControlScheme> controlSchemes)
		{
			if (controlSchemes == null) return;
			this.controlSchemes = controlSchemes;
			XMLTools.CreateXML(m_filename, WriteInputsDocument);
		}

		void WriteInputsDocument(XmlWriter writer) {
			XMLTools.WriteSection( 
				"Input", writer, 
				(w) => { foreach(ControlScheme scheme in controlSchemes) WriteControlScheme(scheme, writer); }, 
				null, null 
			);
		}
		void WriteControlScheme(ControlScheme scheme, XmlWriter writer) {
			XMLTools.WriteSection( 
				"ControlScheme", writer, 
				(w) => { foreach(var action in scheme.Actions) WriteInputAction(action, writer); }, 
				new string[] { "name" }, new string[] { scheme.Name } 
			);
		}
		void WriteInputAction(InputAction action, XmlWriter writer) {
			XMLTools.WriteSection( 
				"Action", writer, 
				(w) => { foreach(var binding in action.bindings) WriteInputBinding(binding, writer); }, 
				new string[] { "name", "displayName" }, new string[] { action.Name, action.displayName } 
			);
		}

		private void WriteInputBinding(InputBinding binding, XmlWriter writer)
		{
			writer.WriteStartElement("Binding");

			writer.WriteElementString("Positive", binding.Positive.ToString());
			writer.WriteElementString("Negative", binding.Negative.ToString());

			writer.WriteElementString("DeadZone", binding.DeadZone.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Gravity", binding.Gravity.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Sensitivity", binding.Sensitivity.ToString(CultureInfo.InvariantCulture));

			writer.WriteElementString("Snap", binding.SnapWhenReadAsAxis.ToString());
			writer.WriteElementString("Invert", binding.InvertWhenReadAsAxis.ToString());

			writer.WriteElementString("UseNeg", binding.useNegativeAxisForButton.ToString());
			writer.WriteElementString("Rebindable", binding.rebindable.ToString());
			writer.WriteElementString("SensitivityEditable", binding.sensitivityEditable.ToString());
			writer.WriteElementString("InvertEditable", binding.invertEditable.ToString());

			writer.WriteElementString("Type", binding.Type.ToString());
			writer.WriteElementString("Axis", binding.MouseAxis.ToString());

			writer.WriteElementString("GamepadButton", binding.GamepadButton.ToString());
			writer.WriteElementString("GamepadAxis", binding.GamepadAxis.ToString());

			writer.WriteElementString("UpdateAsAxis", binding.updateAsAxis.ToString());
			writer.WriteElementString("UpdateAsButton", binding.updateAsButton.ToString());
			

			writer.WriteEndElement();
		}
	}
}
