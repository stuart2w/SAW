using System.Collections.Generic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SAW
{
	public partial class cfgKeys : ConfigPage
	{
		public cfgKeys()
		{
			InitializeComponent();
			Strings.Translate(this);
		}

		public override void OnDisplay()
		{
			base.OnDisplay();
			if (tvActions.IsFilled == false)
				FillKeyActions();
			tvActions_AfterSelect(this, null);
			btnKeysReset.Visible = Level != Config.Levels.System;
			m_DisplayedKey = Keys.None;
			txtKey.Text = "";
			PerformLayout();
		}

		public override void OnHide()
		{
			base.OnHide();
			CheckKeyNotAssigned();
		}

		public ImageList ImageList
		{ get { return tvActions.ActionImageList; } } // This is used by the form for some other lists

		public void EnsureFilled()
		{
			// Called by form because some other lists use the same image list
			if (tvActions.IsFilled == false)
				FillKeyActions();
		}

		#region Keys
		private Keys m_DisplayedKey = Keys.None;
		/// <summary>if true the key is passed to the focal control and ignored by the form's config processing</summary>
		private bool m_KeyIsFocal ;
											  // Remember which parts the user has selected, and warn them if they close the screen having selected both parts but not assigned:
		private bool m_KeyIsChanged;
		private bool m_KeyActionChanged;

		private void FillKeyActions()
		{
			tvActions.Fill(m_Applied);
			m_KeyActionChanged = false;
		}

		public void txtKey_GotFocus(object sender, EventArgs e)
		{
			if (m_DisplayedKey == Keys.None)
				txtKey.SelectionStart = txtKey.TextLength;
		}

		public void txtKey_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.ShiftKey:
				case Keys.LMenu:
				case Keys.ControlKey:
				case Keys.RMenu:
				case Keys.RShiftKey:
				case Keys.RWin:
				case Keys.LWin:
				case Keys.LControlKey:
				case Keys.RControlKey:
					break;
				case Keys.RButton:
				case Keys.Menu:
					break;
				default:
					CombinedKeyEvent combined = new CombinedKeyEvent(e); // on Mac this will modify the key as needed
					DisplayKey(combined.KeyData);
					m_KeyIsChanged = true;
					break;
			}
			e.SuppressKeyPress = true;
			e.Handled = true;
		}

		public void txtKey_TextChanged(object sender, EventArgs e)
		{
			if (!m_Filling)
			{
				// If the system has changed the key, we must change it back again
				DisplayKey(m_DisplayedKey);
				m_KeyIsChanged = true;
			}
		}

		private void DisplayKey(Keys key)
		{
			m_DisplayedKey = key;
			bool wasFilling = m_Filling;
			m_Filling = true;
			try
			{
				pnlKeys.SuspendLayout();
				if (key == Keys.None)
				{
					txtKey.ForeColor = Color.Silver;
					txtKey.Text = Strings.Item("Config_PressKey");
					pnlCurrentKey.Visible = false;
				}
				else
				{
					txtKey.ForeColor = Color.Black;
					txtKey.Text = GUIUtilities.KeyDescription(key);
					pnlCurrentKey.Visible = true;
				}
				m_KeyIsFocal = (key & Keys.KeyCode) == Keys.Tab; // (frmMain.KeyType(eKey) = frmMain.KeyTypes.Focal)

				Functions.Action objAction = m_SubsequentApplied.KeyAction(m_DisplayedKey); 
				// m_objApplied.KeyAction(m_eDisplayedKey)
				btnClearKey.Visible = m_DisplayedKey != Keys.None && m_Config.HasValue(Config.KeyKey(m_DisplayedKey)) && !m_KeyIsFocal;
				// note only displayed if setting is in THIS config
				lblKeyCurrent.Text = objAction.DescriptionWithoutAccelerator();
				if (m_KeyIsFocal)
				{
					lblKeyWarning.Text = Strings.Item("Config_KeyFocal");
					lblKeyWarning.Visible = true;
				}
				else
					lblKeyWarning.Visible = false;
				btnKeysReset.Visible = Level != Config.Levels.System;
				EnableKeyAssign();
			}
			finally
			{
				pnlKeys.ResumeLayout();
				m_Filling = wasFilling;
			}
		}

		public void tvActions_AfterSelect(object sender, EventArgs e)
		{
			Functions.Action action = tvActions.SelectedAction;
			if (action.IsEmpty)
			{
				lblActionCurrentKeys.Visible = false; // no action selected
				lblKeyAction.Text = Strings.Item("Config_KeyNoAction");
				lblKeyAction.ForeColor = Color.Silver;
			}
			else
			{
				lblKeyAction.ForeColor = Color.Black;
				lblKeyAction.Text = action.DescriptionWithoutAccelerator();
				List<Keys> keys = m_Applied.GetKeysForAction(action);
				if (keys == null || keys.Count == 0)
					lblActionCurrentKeys.Visible = false;
				else
				{
					System.Text.StringBuilder output = new System.Text.StringBuilder(Strings.Item("Config_ActionCurrentKeys"));
					output.Append(" ");
					bool first = true;
					foreach (Keys key in keys)
					{
						if (!first)
							output.Append(", ");
						output.Append(GUIUtilities.KeyDescription(key));
						first = false;
					}
					lblActionCurrentKeys.Text = output.ToString();
					lblActionCurrentKeys.Visible = true;
				}
				m_KeyActionChanged = true;
			}
			EnableKeyAssign();
		}

		private void EnableKeyAssign()
		{
			btnSetKey.Enabled = !tvActions.SelectedAction.IsEmpty && m_DisplayedKey != Keys.None && !m_KeyIsFocal;
		}

		public void btnSetKey_Click(object sender, EventArgs e)
		{
			if (tvActions.SelectedAction.IsEmpty || m_DisplayedKey == Keys.None)
				return;
			Functions.Action action = tvActions.SelectedAction;
			Functions.Action existing = m_Applied.KeyAction(m_DisplayedKey);
			if (action.Equals(existing))
				return;
			if (!existing.IsEmpty)
			{
				if (GUIUtilities.QuestionBox(Strings.Item("Config_KeyReplaceWarning").Replace("%0", lblKeyCurrent.Text), MessageBoxButtons.OKCancel) != DialogResult.OK)
					return;
			}
			List<Keys> keys = m_Applied.GetKeysForAction(action);
			if (keys != null && keys.Count == 1 && keys[0] != m_DisplayedKey)
			{
				if (GUIUtilities.QuestionBox(Strings.Item("Config_KeyRemoveExisting").Replace("%0", GUIUtilities.KeyDescription(keys[0])), MessageBoxButtons.YesNo) == DialogResult.Yes)
					m_Config.Write(Config.KeyKey(keys[0]), "");
			}
			m_Config.Write(Config.KeyKey(m_DisplayedKey), action.ToString());
			WrittenToCurrent();
			m_Applied.DiscardKeyLookup();
			DisplayKey(m_DisplayedKey);
			tvActions_AfterSelect(this, null);
			m_KeyActionChanged = false;
			m_KeyIsChanged = false;
		}

		public void btnClearKey_Click(object sender, EventArgs e)
		{
			m_Config.RemoveValue(Config.KeyKey(m_DisplayedKey));
			WrittenToCurrent();
			m_Applied.DiscardKeyLookup();
			DisplayKey(m_DisplayedKey);
			tvActions_AfterSelect(this, null);
			m_KeyIsChanged = false;
		}

		public void btnKeysReset_Click(object sender, EventArgs e)
		{
			if (GUIUtilities.QuestionBox(Strings.Item("Config_Keys_ResetConfirm"),  MessageBoxButtons.OKCancel) !=  DialogResult.OK)
				return;
			List<string> remove = new List<string>(); // the configuration keys to be removed
			foreach (string key in m_Config.Values.Keys)
			{
				if (key.StartsWith("Key_"))
					remove.Add(key);
			}
			foreach (string key in remove)
			{
				m_Config.Values.Remove(key);
			}
			DisplayKey(m_DisplayedKey);
			tvActions_AfterSelect(this, null);
			WrittenToCurrent();
			m_KeyIsChanged = false;
		}

		private void CheckKeyNotAssigned()
		{
			if (m_KeyActionChanged && m_KeyIsChanged && m_DisplayedKey != Keys.None 
				&& !tvActions.SelectedAction.IsEmpty && btnSetKey.Enabled 
				&& !tvActions.SelectedAction.Equals(m_Applied.KeyAction(m_DisplayedKey))) // second-line prevents a spurious warning if the user has selected a matching key and action (in which case clicking the button is ignored)
			{
				// the user has made a selection from key assignment but not actually pressed the assign button
				if (GUIUtilities.QuestionBox(Strings.Item("Config_KeyNotAssigned").Replace("%0", txtKey.Text).Replace("%1", tvActions.SelectedAction.DescriptionWithoutAccelerator()),  MessageBoxButtons.YesNo) ==  DialogResult.Yes)
					btnSetKey_Click(this, EventArgs.Empty);
				else
				{
					m_KeyIsChanged = false;
					m_KeyActionChanged = false;
				}
			}
		}

		#endregion

		public void pnlActions_Resize(object sender, EventArgs e)
		{
			// http://stackoverflow.com/questions/1204804/word-wrap-for-a-label-in-windows-forms
			lblActionCurrentKeys.MaximumSize = new Size(pnlActions.Width, 100);
		}
	}

}
