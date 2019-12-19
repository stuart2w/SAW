using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem("false")]
	public partial class BladeEditor : UserControl, ICommandEditor
	{ // Note that the text in the blade settings string does not translate, although the UI we display to the user does

		private bool m_Filling;
		/// <summary>Any other fields which were not recognised when filling the UI</summary>
		private Dictionary<string, string> m_Unknown;

		public BladeEditor()
		{
			m_Filling = true;
			InitializeComponent();
			Strings.Translate(this);
			for (int index = 0; index < cmbCase.Items.Count; index++)
				cmbCase.Items[index] = Strings.Translate(cmbCase.Items[index].ToString());
			m_Filling = false;
		}

		/// <summary>Possible values for the Case option</summary>
		private string[] Cases = { "Lower", "Normal", "InitialLetter", "Upper" };
		/// <summary>Possible values for the PunctSpace option</summary>
		private string[] PunctSpaces = { "Off", "Single", "Double" };

		private void FillUI()
		{
			m_Filling = true;
			try
			{
				m_Unknown = new Dictionary<string, string>();
				pnlItem.Visible = (m_Command is CmdWordListSet);
				if (m_Command is CmdWordListSet)
					txtItemID.Value = m_Command.GetParamAsInt(1, true);

				// first set the defaults, which will apply for any fields not listed
				chkAlphabeticalResults.CheckState = chkOmitPrevious.CheckState = chkSimpleSingleLetter.CheckState = CheckState.Indeterminate;
				chkMinimumGain.CheckState = chkMinimumLength.CheckState = chkPunctSpace.CheckState = chkCase.CheckState = CheckState.Unchecked;
				nudMinimumGain.Value = nudMinimumLength.Value = 0;
				cmbCase.SelectedIndex = 1;
				cmbPunctSpace.SelectedIndex = 0;
				string settings = m_Command.GetParamAsString(0);
				if (m_Command is CmdWordListSet && settings.Contains("|"))
					settings = settings.Split('|')[1]; // it starts "blade|" which selects the actual engine (fixed here)
				foreach (string field in settings.Split(','))
				{
					if (string.IsNullOrEmpty(field)) // in particular needed if entire settings is ""
						continue;
					string[] halves = field.Split('=');
					if (halves.Length < 2)
					{
						Utilities.LogSubError("Blade settings does not include =");
						continue;
					}
					switch (halves[0].Trim().ToLower())
					{
						case "alphabeticalresults":
							chkAlphabeticalResults.Checked = ParseBoolean(halves[1]);
							break;
						case "omitprevioussuggestions":
							chkOmitPrevious.Checked = ParseBoolean(halves[1]);
							break;
						case "simplesingleletter":
							chkSimpleSingleLetter.Checked = ParseBoolean(halves[1]);
							break;
						case "minimumlength":
							chkMinimumLength.Checked = true;
							nudMinimumLength.Value = Utilities.IntVal(halves[1]);
							break;
						case "minimumgain":
							chkMinimumGain.Checked = true;
							nudMinimumGain.Value = Utilities.IntVal(halves[1]);
							break;
						case "case":
							chkCase.Checked = true;
							for (int index = 0; index < Cases.Length; index++)
								if (halves[1].Equals(Cases[index], StringComparison.CurrentCultureIgnoreCase))
									cmbCase.SelectedIndex = index;
							break;
						case "punctspace":
							chkPunctSpace.Checked = true;
							for (int index = 0; index < PunctSpaces.Length; index++)
								if (halves[1].Equals(PunctSpaces[index], StringComparison.CurrentCultureIgnoreCase))
									cmbPunctSpace.SelectedIndex = index;
							break;
						default:
							m_Unknown.Add(halves[0].Trim(), halves[1]);
							break;
					}
				}
			}
			finally { m_Filling = false; }
		}

		/// <summary>Resets the parameter in the command objects based on the UI.  In most editors individual controls update individual parts of the command,
		/// but in this case the entire lot has to be concatenated into one string </summary>
		private void RefillCommand()
		{
			StringBuilder output = new StringBuilder();
			Dictionary<string, string> settings = new Dictionary<string, string>();
			if (chkAlphabeticalResults.CheckState != CheckState.Indeterminate)
				settings.Add("AlphabeticalResults", chkAlphabeticalResults.Checked.ToString());
			if (chkOmitPrevious.CheckState != CheckState.Indeterminate)
				settings.Add("OmitPreviousSuggestions", chkOmitPrevious.Checked.ToString());
			if (chkSimpleSingleLetter.CheckState != CheckState.Indeterminate)
				settings.Add("SimpleSingleLetter", chkSimpleSingleLetter.Checked.ToString());
			if (chkMinimumLength.Checked)
				settings.Add("MinimumLength", nudMinimumLength.Value.ToString());
			if (chkMinimumGain.Checked)
				settings.Add("MinimumGain", nudMinimumGain.Value.ToString());
			if (chkCase.Checked && cmbCase.SelectedIndex >= 0)
				settings.Add("Case", Cases[cmbCase.SelectedIndex]);
			if (chkPunctSpace.Checked && cmbPunctSpace.SelectedIndex >= 0)
				settings.Add("PunctSpace", PunctSpaces[cmbPunctSpace.SelectedIndex]);
			foreach (var other in m_Unknown)
				settings.Add(other.Key, other.Value);
			foreach (string key in settings.Keys)
			{
				if (output.Length > 0)
					output.Append(",");
				output.Append(key).Append("=").Append(settings[key]);
			}
			m_Command.m_ParamList[0] = new StringParam("blade|" + output);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private static bool ParseBoolean(string value)
		{
			return value.ToLower().Trim() == "true";
		}

		#region ICommandEditor
		private Command m_Command;
		public event EventHandler UserChanged;

		public Command EditCommand
		{
			get { return m_Command; }
			set
			{
				m_Command = value;
				FillUI();
			}
		}

		public string GetValidationError()
		{
			return txtItemID.GetValidationError();
		}

		#endregion

		#region Events

		private void chkMinimumLength_CheckedChanged(object sender, EventArgs e)
		{
			nudMinimumLength.Enabled = chkMinimumLength.Checked;
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void chkMinimumGain_CheckedChanged(object sender, EventArgs e)
		{
			nudMinimumGain.Enabled = chkMinimumGain.Checked;
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void chkCase_CheckedChanged(object sender, EventArgs e)
		{
			cmbCase.Enabled = chkCase.Checked;
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void chkPunctSpace_CheckedChanged(object sender, EventArgs e)
		{
			cmbPunctSpace.Enabled = chkPunctSpace.Checked;
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void chkAlphabeticalResults_CheckStateChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void chkOmitPrevious_CheckStateChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void chkSimpleSingleLetter_CheckStateChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void nudMinimumLength_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void nudMinimumGain_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void cmbCase_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RefillCommand();
		}

		private void cmbPunctSpace_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RefillCommand();
		}
		#endregion

		private void txtItemID_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling || !(m_Command is CmdWordListSet))
				return;
			m_Command.m_ParamList[1] = new IntegerParam(txtItemID.Value);
		}
	}
}
