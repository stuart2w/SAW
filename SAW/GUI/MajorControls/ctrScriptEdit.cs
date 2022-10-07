using System;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using SAW.Shapes;

namespace SAW
{
	internal partial class ctrScriptEdit : UserControl
	{
		private Script m_Script;
		private bool m_Filling;
		/// <summary>Whether the UI is displayed as a list or text.  Stored here so that the same setting is used when the next editor is shown</summary>
		private static bool g_AsText;
		/// <summary>The page on which the edited item appears</summary>
		private Page m_Page;
		/// <summary>True if the text in the item ID field was valid when the user last edited it.  Blank text is counted as 0 and accepted</summary>
		private bool m_ItemIDValid;
		/// <summary>The default script from the page matching this one</summary>
		private Script m_DefaultScript;
		/// <summary>The item that the script is attached to, if known.  MAY BE NULL</summary>
		private Scriptable m_Scriptable;
		private bool m_TextChanged;

		public ctrScriptEdit()
		{
			m_Filling = true;
			InitializeComponent();

			for (var i = Script.VisitTarget.VisitTypes.None; i <= Script.VisitTarget.VisitTypes.Item; i++)
			{
				cmbVisitType.Items.Add(Strings.Item("SAW_Visit_" + i)); // translation entries of form SAW_Visit_None
			}

			// In the designer neither option is ticked; we start with the one text that was previously ticked by the user (defaults to list when the application is opened)
			if (g_AsText)
				rdoText.Checked = true;
			else
				rdoList.Checked = true;
			m_Filling = false;
		}

		#region Top level data read/write
		/// <summary>Display the given script for editing.</summary>
		public void Edit(Scriptable scriptable, int which)
		{
			m_Scriptable = scriptable;
			m_Script = scriptable.Scripts[which];
			m_Page = scriptable.FindPage();
			m_DefaultScript = m_Scriptable.GetDefaultScript((Scriptable.ScriptTypes)which, Globals.Root.CurrentDocument);
			FillUI();
		}

		/// <summary>Use overloaded version (Scriptable, which) normally.  This version is intended for the default scripts, and will not support calling thru to default</summary>
		public void Edit(Script script, Page page)
		{
			m_Scriptable = null;
			m_Script = script;
			m_Page = page;
			m_DefaultScript = null;
			FillUI();
		}

		private void FillUI()
		{
			this.Enabled = m_Script != null;
			if (m_Script == null)
				return;

			m_Filling = true;
			try
			{
				chkRunDefault.Checked = m_Script.RunDefault;
				cmbVisitType.SelectedIndex = (int)m_Script.Visit.VisitType;
				if (cmbVisitType.SelectedIndex == (int)Script.VisitTarget.VisitTypes.Item)
					txtVisitItemID.Text = m_Script.Visit.ItemID.ToString();
				else
					txtVisitItemID.Text = "";
				m_ItemIDValid = true;
				txtScript.Text = m_Script.GenerateScript();
				FillCommandList();
				UpdateVisitDefaultInfo();

				chkRunDefault.Visible = m_DefaultScript != null;
				btnDefaultInfo.Visible = m_DefaultScript != null;

				m_TextChanged = false;
			}
			finally { m_Filling = false; }
		}

		/// <summary>Refills just the command list control</summary>
		private void FillCommandList()
		{
			lstCommands.Items.Clear();
			foreach (var o in m_Script.CommandList)
			{
				lstCommands.Items.Add(o.GetScriptWithComment());
			}
			// clear isn't triggering SelectedIndex changed
			lstCommands_SelectedIndexChanged(lstCommands, EventArgs.Empty);
		}

		/// <summary>Called when the user has made changes; the changes have already been written to the data object</summary>
		private void Edited()
		{
		}

		/// <summary>Returns an error message, or null if none</summary>
		public string GetValidationError()
		{ // not using the in-built validation as it mostly works on LostFocus on internal controls, and is a pain
			if (!m_ItemIDValid)
				return Strings.Item("SAW_Edit_VisitItemInvalid");
			string error = StoreTextChanges();
			if (rdoList.Checked)
				error = error ?? ctrCommandEditor.GetValidationError();
			return error;
		}

		/// <summary>Stores any text changes, returning null on success or error message</summary>
		private string StoreTextChanges()
		{
			if (m_TextChanged)
			{
				string error = m_Script.ParseFromScript(txtScript.Text, false);
				if (error != null)
				{
					AnimationColourChange.CreateStart(btnAbandonText, Color.LightYellow, 8, 1);
					return error;
				}
				FillCommandList();
				m_TextChanged = false;
			}
			return null;
		}

		private void UpdateVisitDefaultInfo()
		{
			bool useDefault = chkRunDefault.Checked && m_Script.Visit.VisitType == Script.VisitTarget.VisitTypes.None && m_DefaultScript != null;
			lblVisitDefaultInfo.Visible = useDefault;
			if (m_DefaultScript == null)
				return;
			if (m_DefaultScript.Visit.VisitType == Script.VisitTarget.VisitTypes.None)
				lblVisitDefaultInfo.Text = Strings.Item("SAW_Edit_NoDefaultVisit");
			else
				lblVisitDefaultInfo.Text = Strings.Item("SAW_Edit_DefaultVisitIs").Replace("%0", m_DefaultScript.Visit.GetDescription());
		}

		#endregion

		#region Control events
		// list buttons are made invisible in text mode, and disabled in list mode when not applicable
		private void rdoListOrText_CheckedChanged(object sender, EventArgs e)
		{ // Fires if EITHER radio button changes (this will fire twice when both change, but that doesn't matter)
			if (txtScript.Visible && rdoList.Checked && !m_Filling && m_TextChanged)
			{// need to parse the text back into the list
				string error = StoreTextChanges();
				if (error != null)
				{
					rdoText.Checked = true;
					MessageBox.Show(error);
					return;
				}
			}
			else if (ctrCommandEditor.Visible && rdoText.Checked)
			{
				if (!DoControlError())
				{
					rdoList.Checked = true;
					return;
				}
			}
			lstCommands.Visible = rdoList.Checked;
			ctrCommandEditor.Visible = rdoList.Checked;
			txtScript.Visible = rdoText.Checked;
			g_AsText = !rdoList.Checked;
			btnDown.Visible = rdoList.Checked;
			btnUp.Visible = rdoList.Checked;
			btnAdd.Visible = rdoList.Checked;
			btnDelete.Visible = rdoList.Checked;
			btnAbandonText.Visible = rdoText.Checked;
			btnAbandonText.Enabled = m_TextChanged; // should be false presumably?
		}

		/// <summary>Checks for error in control editor, and returns true if OK to proceed.  Reports error if any to user</summary>
		private bool DoControlError()
		{
			string error = ctrCommandEditor.GetValidationError();
			if (error != null)
				MessageBox.Show(error);
			return error == null;
		}

		private void cmbVisitType_SelectedIndexChanged(object sender, EventArgs e)
		{
			// this part runs both in response to user change and internal changes
			bool isItem = (cmbVisitType.SelectedIndex == (int)Script.VisitTarget.VisitTypes.Item);
			lblVisitItemID.Visible = isItem;
			txtVisitItemID.Visible = isItem;

			// this part only runs in response to user changes
			if (m_Filling)
				return;
			m_Script.Visit.VisitType = (Script.VisitTarget.VisitTypes)cmbVisitType.SelectedIndex;
			Edited();
			UpdateVisitDefaultInfo();
		}

		private bool m_IgnoreListSelectionChange; // used to help prevent ctrCommandEditor_UserChanged triggering this unhelpfully
												  /// <summary>Item in the command list that is currently being edited </summary>
		private int m_CurrentListIndex;
		private void lstCommands_SelectedIndexChanged(object sender, EventArgs e)
		{ // The buttons are enabled or disabled depending on selection.  The Visible property is changed if editing in text mode, so that will not interfere with this
			if (m_IgnoreListSelectionChange)
				return;
			if (!m_Filling && m_CurrentListIndex >= 0 && !DoControlError())
			{
				m_IgnoreListSelectionChange = true;
				lstCommands.SelectedIndex = m_CurrentListIndex;
				m_IgnoreListSelectionChange = false;
				return;
			}
			m_CurrentListIndex = lstCommands.SelectedIndex;
			btnUp.Enabled = lstCommands.SelectedIndex > 0;
			btnDown.Enabled = lstCommands.SelectedIndex >= 0 && lstCommands.SelectedIndex < m_Script.CommandList.Count - 1;
			ctrCommandEditor.EditCommand(lstCommands.SelectedIndex >= 0 ? m_Script.CommandList[lstCommands.SelectedIndex] : null, m_Scriptable);
			btnDelete.Enabled = lstCommands.SelectedIndex >= 0;
		}

		private void btnUp_Click(object sender, EventArgs e)
		{
			int index = lstCommands.SelectedIndex;
			Command move = m_Script.CommandList[index];
			m_Script.CommandList.RemoveAt(index);
			m_Script.CommandList.Insert(index - 1, move);
			FillUI();
			lstCommands.SelectedIndex = index - 1;
			Edited();
		}

		private void btnDown_Click(object sender, EventArgs e)
		{
			int index = lstCommands.SelectedIndex;
			Command move = m_Script.CommandList[index];
			m_Script.CommandList.RemoveAt(index);
			m_Script.CommandList.Insert(index + 1, move);
			FillUI();
			lstCommands.SelectedIndex = index + 1;
			Edited();
		}

		private void txtVisitItemID_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			int id;
			if (txtVisitItemID.Text == "")
			{
				m_ItemIDValid = true;
				m_Script.Visit.ItemID = 0;
			}
			if (!int.TryParse(txtVisitItemID.Text, out id))
				m_ItemIDValid = false;
			else
				m_ItemIDValid = m_Page.FindScriptableByID(id) != null;
			if (m_ItemIDValid)
				m_Script.Visit.ItemID = id;
			txtVisitItemID.BackColor = m_ItemIDValid ? Color.White : Color.Tomato;
		}

		/// <summary>Other code changing the text should set m_TextChanged = false AFTER changing text</summary>
		private void txtScript_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_TextChanged = true;
			btnAbandonText.Enabled = true;
		}

		private void btnAbandonText_Click(object sender, EventArgs e)
		{
			txtScript.Text = m_Script.GenerateScript();
			m_TextChanged = false;
			btnAbandonText.Enabled = false;
			// without the following it tends to highlight the entire text (since the focus is pushed off this button when it is disabled)
			txtScript.Select(txtScript.Text.Length, 0);
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			Command command = frmAddCommand.CreateCommand(FindForm());
			if (command == null)
				return; // cancelled (or maybe just failed)
			m_Script.CommandList.Add(command);
			FillUI();
			lstCommands.SelectedIndex = lstCommands.Items.Count - 1;
			Edited();
		}

		private void btnDelete_Click(object sender, EventArgs e)
		{
			if (lstCommands.SelectedIndex < 0)
				return;
			m_CurrentListIndex = -1;  // required to prevent lstCommands_SelectedIndexChanged trying to reselect lstCommands_SelectedIndexChanged
			m_Script.CommandList.RemoveAt(lstCommands.SelectedIndex);
			lstCommands.Items.RemoveAt(lstCommands.SelectedIndex); // should trigger deselection, triggering UI changes
		}

		private void btnReset_Click(object sender, EventArgs e)
		{
			if (GUIUtilities.QuestionBox("[SAW_Edit_ResetConfirm]", MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			m_Filling = true;
			m_Script.CommandList.Clear();
			lstCommands.Items.Clear();
			txtScript.Clear();
			chkRunDefault.Checked = true;
			m_Script.RunDefault = true;
			m_Filling = false;
		}
		
		private void ctrCommandEditor_UserChanged(object sender, EventArgs e)
		{
			Debug.Assert(!m_Filling && lstCommands.SelectedIndex >= 0 && rdoList.Checked);
			int index = lstCommands.SelectedIndex;
			if (index < 0)
				return;
			m_IgnoreListSelectionChange = true; // otherwise the assignment below fires SelectedIndexChanged
			m_Filling = true;
			try
			{
				lstCommands.Items[index] = m_Script.CommandList[index].GetScriptWithComment();
				txtScript.Text = m_Script.GenerateScript();
			}
			catch (Exception ex) // best to suppress errors here?
			{
				Utilities.LogSubError(ex);
			}
			finally
			{
				m_Filling = false;
				m_IgnoreListSelectionChange = false;
			}
		}

		private void chkRunDefault_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (m_TextChanged) // finalise the text first, otherwise editing will make a mess
			{
				string error = StoreTextChanges();
				if (error != null) // block the change until the text is fixed:
				{
					MessageBox.Show(error);
					m_Filling = true;
					chkRunDefault.Checked = !chkRunDefault.Checked;
					m_Filling = false;
					return;
				}
			}
			m_Script.RunDefault = chkRunDefault.Checked;
			// then do some checks to help user
			if (m_DefaultScript != null)
			{
				if (!chkRunDefault.Checked && m_DefaultScript.CommandList.Any()) // && !m_Script.CommandList.Any()) - better to always ask, not just if list empty.  If user added one custom command and unticks, they still very much want the defaults
				{ // only exception is if the default is empty
					if (GUIUtilities.QuestionBox("[SAW_Edit_CopyDefaults]", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						// only copies any commands that are not already present.  Inserts at start of list
						m_Script.CommandList.InsertRange(0, from c in m_DefaultScript.CommandList where !m_Script.CommandList.Any(user => user.IdenticalTo(c)) select c.Clone());
						if (m_Script.Visit.VisitType == Script.VisitTarget.VisitTypes.None)
							m_Script.Visit = m_DefaultScript.Visit;
						FillUI();
					}
				}
				else if (chkRunDefault.Checked && m_Script.CommandList.Any(user => m_DefaultScript.CommandList.Any(def => def.IdenticalTo(user))))
				{
					// second condition checks if any user command matches any default command
					if (GUIUtilities.QuestionBox("[SAW_Edit_RemoveDefaults]", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						m_Script.CommandList.RemoveAll(user => m_DefaultScript.CommandList.Any(def => def.IdenticalTo(user)));
						if (m_Script.Visit.Equals(m_DefaultScript.Visit))
							m_Script.Visit.VisitType = Script.VisitTarget.VisitTypes.None;
						FillUI();
					}
				}
			}
			Edited();
		}

		private void btnDefaultInfo_Click(object sender, EventArgs e)
		{
			MessageBox.Show(Strings.Item("SAW_Edit_DefaultInfo") + "\r\n" + (m_DefaultScript?.GenerateScript() ?? ""));
		}

		#endregion

	}
}
