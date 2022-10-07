using System;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using SAW.Commands;

namespace SAW.CommandEditors
{
	[ToolboxItem(false)]
	public partial class KeysEditor : UserControl, ICommandEditor
	{
		private bool m_Filling;
		/// <summary>The parameter number at which the text is stored</summary>
		private int m_TextParam;

		public KeysEditor()
		{
			m_Filling = true;
			InitializeComponent();
			Strings.Translate(this);
			m_Filling = false;
		}

		private void FillUI()
		{
			m_Filling = true;
			try
			{
				m_TextParam = 0;
				if (m_Command is CmdSlowKeys)
				{
					txtDelay.Value = m_Command.GetParamAsInt(0, true);
					m_TextParam = 1;
				}
				else
				{
					txtDelay.Visible = false; // converse not needed as this control will not be re-used and it starts visible
					lblDelay.Visible = false;
				}

				bool newFormat = false;
				string text = m_Command.GetParamAsString(m_TextParam, true);
				if (text.Contains(KeySend.NEWFORMATINDICATOR))
				{
					newFormat = true;
					text = text.Replace(KeySend.NEWFORMATINDICATOR, "");
				}
				else if (text.StartsWith("'") && text.EndsWith("'"))
				{
					newFormat = true;
					text = text.Substring(1, text.Length - 2);
				}
				else if (!text.Contains("<")) // any text which doesn't contain any command codes of either type will be treated as new format (i.e. commands will be added in new format)
					newFormat = true;
				chkGIDEI.Checked = !newFormat;
				txtKeys.Text = text;

			}
			finally { m_Filling = false; }
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
			if (m_Command is CmdSlowKeys && !txtDelay.IsValid)
				return Strings.Item("SAW_CommandEdit_BadDelay");
			return null;
		}

		#endregion

		#region Events

		private void txtDelay_TextChanged(object sender, EventArgs e)
		{ // will only fire if CmdSlowKeys
			if (m_Filling)
				return;
			if (!txtDelay.IsValid || txtDelay.Value < 0 || txtDelay.Value > CmdSlowKeys.MAXDELAY)
				return;
			m_Command.ParamList[0] = new IntegerParam(txtDelay.Value);
			UserChanged?.Invoke(this, EventArgs.Empty);
		}

		private void chkGIDEI_CheckedChanged(object sender, EventArgs e)
		{ // this runs even when filling the control
			btnAdd.Enabled = !chkGIDEI.Checked;
			chkCapture.Enabled = !chkGIDEI.Checked;
			if (chkGIDEI.Checked)
				chkCapture.Checked = false;
		}

		private void chkCapture_CheckedChanged(object sender, EventArgs e)
		{
			txtKeys.CaptureAllKeys = chkCapture.Checked;
			//if (!m_Filling)
			//	txtKeys.Focus(); // put focus back so user doesn't need to click again - but doing this selects entire text.  Drat
		}

		private void txtKeys_KeyDown(object sender, KeyEventArgs e)
		{
			// enter is always captured to prevent new lines in the script
			if (!chkCapture.Checked && e.KeyCode != Keys.Enter)
				return;
			InsertCommand(KeySend.Instance.GetKeyNewCode(e.KeyCode).ToUpper());
			e.Handled = true;
		}

		private void txtKeys_KeyPress(object sender, KeyPressEventArgs e)
		{ // Must suppress these as well in capture mode.  Even with KeyDown suppressed keys will still type without this.
			if (chkCapture.Checked && e.KeyChar != '\n')
				e.Handled = true;
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			BuildMenu(); // ignored if already done
			mnuKeys.Show(btnAdd, Point.Empty, ToolStripDropDownDirection.Right);
		}

		private void txtKeys_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			string param = txtKeys.Text;
			if (!chkGIDEI.Checked)
				param = "'" + param + "'";
			m_Command.ParamList[m_TextParam] = new StringParam(param);
			UserChanged?.Invoke(this, EventArgs.Empty);
			tmrCheckScript.Enabled = true;
		}

		/// <summary>Inserts the given key command at the typing point</summary>
		private void InsertCommand(string code)
		{
			Debug.Assert(code.StartsWith("<"));
			//m_Filling = true;
			// this SHOULD trigger TextChanged so propagating the changes out
			int location = txtKeys.SelectionStart;
			int end = txtKeys.SelectionStart + txtKeys.SelectionLength;
			txtKeys.Text = txtKeys.Text.Substring(0, location) + code + txtKeys.Text.Substring(end);
			txtKeys.SelectionStart = location + code.Length;
			txtKeys.SelectionLength = 0;
			//m_Filling = false;
		}


		#endregion

		#region Menu

		private void BuildMenu()
		{
			if (mnuKeys.Items.Count > 0)
				return;
			ToolStripMenuItem mnuSub = (ToolStripMenuItem)mnuKeys.Items.Add(Strings.Item("SAW_CommandEdit_KeyGroup_Modifiers"));
			AddListOfKeys(mnuSub, new[] { Keys.ControlKey, Keys.Menu, Keys.ShiftKey });
			AddKeyCombination(mnuSub, new[] { Keys.ControlKey, Keys.Menu });
			AddKeyCombination(mnuSub, new[] { Keys.ControlKey, Keys.ShiftKey });
			AddKeyCombination(mnuSub, new[] { Keys.Menu, Keys.ShiftKey });
			AddKeyCombination(mnuSub, new[] { Keys.ControlKey, Keys.Menu, Keys.ShiftKey });

			mnuSub = (ToolStripMenuItem)mnuKeys.Items.Add(Strings.Item("SAW_CommandEdit_KeyGroup_Navigation"));
			AddListOfKeys(mnuSub, new[] { Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.PageUp, Keys.PageDown });

			mnuSub = (ToolStripMenuItem)mnuKeys.Items.Add(Strings.Item("SAW_CommandEdit_KeyGroup_Common"));
			AddListOfKeys(mnuSub, new[] { Keys.Enter, Keys.Space, Keys.Escape, Keys.Tab, Keys.Back, Keys.Insert, Keys.Delete, Keys.Home, Keys.End });
			AddKey(mnuSub, "<", "<less>");

			mnuSub = (ToolStripMenuItem)mnuKeys.Items.Add(Strings.Item("SAW_CommandEdit_KeyGroup_FN"));
			AddListOfKeys(mnuSub, new[] { Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12 });

			mnuSub = (ToolStripMenuItem)mnuKeys.Items.Add(Strings.Item("SAW_CommandEdit_KeyGroup_LocksSpecial"));
			AddListOfKeys(mnuSub, new[] { Keys.CapsLock, Keys.NumLock, Keys.Scroll, Keys.Pause, Keys.PrintScreen, Keys.LWin, Keys.RWin, Keys.Apps });

			mnuSub = (ToolStripMenuItem)mnuKeys.Items.Add(Strings.Item("SAW_CommandEdit_KeyGroup_NumPad"));
			AddListOfKeys(mnuSub, new[] { Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3, Keys.NumPad4, Keys.NumPad5, Keys.NumPad6,
				Keys.NumPad7,Keys.NumPad8,Keys.NumPad9,Keys.Add,Keys.Subtract,Keys.Multiply,Keys.Divide,Keys.Decimal});

			mnuSub = (ToolStripMenuItem)mnuKeys.Items.Add(Strings.Item("SAW_CommandEdit_KeyGroup_Actions"));
			AddKeyAction(mnuSub, Keys.ControlKey, "Lock");
			AddKeyAction(mnuSub, Keys.ControlKey, "Unlock");
			AddKeyAction(mnuSub, Keys.Menu, "Lock");
			AddKeyAction(mnuSub, Keys.Menu, "Unlock");
			AddKeyAction(mnuSub, Keys.ShiftKey, "Lock");
			AddKeyAction(mnuSub, Keys.ShiftKey, "Unlock");
			AddKeyAction(mnuSub, Keys.Capital, "On");
			AddKeyAction(mnuSub, Keys.Capital, "Off");
			AddKeyAction(mnuSub, Keys.NumLock, "On");
			AddKeyAction(mnuSub, Keys.NumLock, "Off");
			AddKeyAction(mnuSub, Keys.Scroll, "On");
			AddKeyAction(mnuSub, Keys.Scroll, "Off");
			AddKey(mnuSub, Strings.Item("SAW_CommandEdit_KeyAction_UnlockAll"), "<unlock>");
			AddKey(mnuSub, Strings.Item("SAW_CommandEdit_KeyAction_Press").Replace("%0", KeySend.Instance.GetKeyDescription(Keys.ControlKey)), "<ctrl_press>");
			AddKey(mnuSub, Strings.Item("SAW_CommandEdit_KeyAction_Press").Replace("%0", KeySend.Instance.GetKeyDescription(Keys.ShiftKey)), "<ctrl_shift>");
			AddKey(mnuSub, Strings.Item("SAW_CommandEdit_KeyAction_Press").Replace("%0", KeySend.Instance.GetKeyDescription(Keys.Menu)), "<ctrl_alt>");
			AddKey(mnuSub, Strings.Item("SAW_CommandEdit_KeyAction_HoldWin"), "<LWIN_hold>");
			AddKey(mnuSub, Strings.Item("SAW_CommandEdit_KeyAction_Repeat"), "<" + KeySend.Instance.RepeatCode + " 2><" + KeySend.Instance.EndRepeatCode + ">");
			AddKey(mnuSub, Strings.Item("SAW_CommandEdit_KeyAction_Delay"), "<" + KeySend.Instance.DelayCode + " 500>");
		}

		private void AddKey(ToolStripMenuItem parent, string name, string code)
		{
			var added = parent.DropDownItems.Add(name);
			added.Tag = code;
			added.Click += KeyMenu_Click;
		}

		private void AddListOfKeys(ToolStripMenuItem parent, Keys[] keys)
		{
			foreach (Keys key in keys)
			{
				AddKey(parent, KeySend.Instance.GetKeyDescription(key), KeySend.Instance.GetKeyNewCode(key));
			}
		}

		/// <summary>Adds a single menu entry referencing multiple keys (eg &lt;Shift+Alt&gt;)</summary>
		private void AddKeyCombination(ToolStripMenuItem parent, Keys[] keys)
		{
			StringBuilder code = new StringBuilder();
			StringBuilder name = new StringBuilder();
			code.Append("<");
			foreach (Keys key in keys)
			{
				if (name.Length > 0)
				{ // not first item, so add joiner:
					code.Append('+');
					name.Append('+');
				}
				code.Append(KeySend.Instance.GetKeyNewCode(key).Trim('<', '>'));
				name.Append(KeySend.Instance.GetKeyDescription(key));
			}
			code.Append(">");
			AddKey(parent, name.ToString(), code.ToString());
		}

		/// <summary>Adds a single key perform a Lock or On/Off type action</summary>
		private void AddKeyAction(ToolStripMenuItem parent, Keys key, string action)
		{ // action should have initial capital to match the translations
			string name = Strings.Item("SAW_CommandEdit_KeyAction_" + action).Replace("%0", KeySend.Instance.GetKeyDescription(key));
			string code = KeySend.Instance.GetKeyNewCode(key).TrimEnd('>') + "_" + action + ">";
			AddKey(parent, name, code);
		}

		private void KeyMenu_Click(object sender, EventArgs e)
		{
			string code = (sender as ToolStripItem).Tag.ToString();
			InsertCommand(code.ToUpper());
		}

		#endregion

		private void tmrCheckScript_Tick(object sender, EventArgs e)
		{
			tmrCheckScript.Enabled = false;
			// check if script is valid
			if (txtKeys.Text != "")
			{
				try
				{
					KeySend.TestInstance.ReleaseAll();
					KeySend.TestInstance.SendStringKeys(txtKeys.Text, 0);
				}
				catch (Exception ex)
				{
					lblWarning.Text = Strings.Item("SAW_CommandEdit_KeyFailed").Replace("%0", ex.Message);
					return;
				}
				if (KeySend.TestInstance.HasHeldKeys)
					lblWarning.Text = Strings.Item("SAW_CommandEdit_KeyHeldOpen");
				else
					lblWarning.Text = "";
			}
		}
	}

}
