using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;

namespace SAW
{
	public sealed partial class frmEditConfig
	{// copied from Splash, but much reduced.
	 // this maintains the possibility of other config levels by using an AppliedConfig( which also has better read methods)
	 // but with no subsequent/previous logic, in practice editing should be for 1 level only

		internal readonly Document m_Document;
		internal Config m_Config;
		internal AppliedConfig m_Applied; // an applied configuration containing the list above, which sometimes allows easier look up of values
		private readonly List<ConfigPage> m_SectionPanels = new List<ConfigPage>(); // the panels corresponding to the entries in the sections list on the left
		internal Transaction m_Transaction;
		internal bool m_Filling;
		/// <summary>Not really applicable - whether editing the user mode or editor mode settings</summary>
		internal Users m_User = Globals.Root.User;
		internal Config.Levels m_Level;
		private Document m_OpenPaletteOnExit; // if creating or editing a palette this is a reference to the object which needs to be opened for editing (in teacher mode) on exit
		internal bool m_PalettePurposeChanged = false; // true if any existing palettes had their purpose changed; in which case we need to update the registrations on exit
														  // mode is now sticky between opening and closing screen...
		private cfgKeys m_pnlKeys;

		private frmEditConfig(Config config, Config.Levels level, Document document)
		{
			m_Filling = true;
			InitializeComponent();
			m_Level = level;
			m_Config = config;
			m_Applied = new AppliedConfig(new[] { config, Config.SystemConfig }.ToList());
			// we don't scale (most controls are auto-size), except for the list on the left:
			pnlSections.Width = pnlSections.Width * GUIUtilities.SystemDPI / 96;

			m_Document = document;
			CreatePanels();

			Strings.Translate(this, ttMain);
			m_Transaction = new Transaction();
			m_Transaction.DocumentNotChanged = true; // will be cleared again if any actual edits made to doc config
			m_Filling = false;

			pnlSections.SelectedIndex = 0;
			pnlSections_SelectedIndexChanged(this, null);

			Switches.Switching.PhysicalSwitch.MasterSwitchChanged += PhysicalSwitch_MasterSwitchChanged;
		}

		private void CreatePanels()
		{
			//pnlSections.AddLabel(Strings.Item("Config_Introduction"));
			//m_colSectionPanels.Add(null); // actual value inserted during SetLevels
			pnlSections.AddLabel(Strings.Item("SAW_Settings_Switches")).Font = new Font(pnlSections.Font, FontStyle.Bold);
			m_SectionPanels.Add(AddPanel(new cfgSwitching()));
			pnlSections.AddLabel(Strings.Item("SAW_Settings_Input"));
			m_SectionPanels.Add(AddPanel(new cfgInput()));
			pnlSections.AddLabel(Strings.Item("SAW_Settings_Output"));
			m_SectionPanels.Add(AddPanel(new cfgOutput()));
			pnlSections.AddLabel(Strings.Item("SAW_Settings_MouseMovement"));
			m_SectionPanels.Add(AddPanel(new cfgMouseOutput()));
			pnlSections.AddLabel(Strings.Item("SAW_Settings_Styles"));
			m_SectionPanels.Add(AddPanel(new cfgStyles()));
			pnlSections.AddLabel(Strings.Item("Config_Speech"));
			m_SectionPanels.Add(AddPanel(new cfgSpeech()));
			pnlSections.AddLabel(Strings.Item("SAW_Settings_EditorView"));
			m_SectionPanels.Add(AddPanel(new cfgShowInEditing()));
			pnlSections.AddLabel(Strings.Item("Config_Keys"));
			m_pnlKeys = (cfgKeys)AddPanel(new cfgKeys());
			m_SectionPanels.Add(m_pnlKeys);
#if DEBUG
			pnlSections.AddLabel(Strings.Item("Config_Palettes"));
			m_SectionPanels.Add(AddPanel(new cfgCustomPalettes()));
			pnlSections.AddLabel("(List values)"); // is before speech
			m_SectionPanels.Add(AddPanel(new cfgListValues()));
#endif

		}

		/// <summary>Edits the config.  Document should be the document being edited, not the config container document</summary>
		/// <param name="level"></param>
		/// <param name="document"></param>
		/// <param name="config"></param>
		/// <param name="userOnly"></param>
		/// <param name="initialSection"></param>
		/// <returns></returns>
		public static DialogResult EditConfig(Config.Levels level, Document document, Config config = null, bool userOnly = false, Sections initialSection = (Sections)(-1))
		{
			// returns DialogResult = Yes if OK, but has stored doc settings transaction which will have triggered refresh of main screen settings already
			Config.UserCurrent.StorePalettePositions();
			if (config == null)
				config = Globals.Root.GetCurrentConfig(level, document);
			frmEditConfig frm = new frmEditConfig(config, level, document);
			if (initialSection >= 0)
			{
				frm.pnlSections.SelectedIndex = (int)initialSection;
			}
			DialogResult result = frm.ShowDialog();
			if (result == DialogResult.OK)
			{
				bool usersChanged = false;
				bool documentChanged = false;
				bool activityChanged = false;
				foreach (Config changed in frm.m_Changed.Values)
				{
					if (changed == Config.UserUser || changed == Config.UserEditor)
						usersChanged = true;
					else if (changed == Config.SystemConfig)
						Globals.Root.SaveSystemConfig();
					else if (changed == document.UserSettings || changed == document.BothSettings)
						documentChanged = true;
					else
						// must be from current activity; could be either part
						activityChanged = true;
				}
				if (documentChanged)
					Globals.Root.StoreNewTransaction(frm.m_Transaction);
				if (usersChanged)
					Globals.Root.SaveUserConfigs();
				if (activityChanged)
					Globals.Root.SaveActivityConfig();
				if (frm.m_PalettePurposeChanged)
				{
					// One or more user-defined palettes have had their purpose changed
					foreach (Palette palette in Palette.List.Values)
					{
						if (palette.UserDefined)
							palette.PalettePurpose = palette.CustomDocument.PalettePurpose;
					}
				}
				if (frm.m_OpenPaletteOnExit != null)
				{
					if (Globals.Root.User == Users.User)
						Globals.Root.User = Users.Editor;
					Globals.Root.SelectDocument(frm.m_OpenPaletteOnExit);
				}
				Globals.OnSettingsChanged();
			}
			else
				frm.m_Transaction.Cancel();
			return result;
		}


		#region Sections
		public enum Sections
		{
			// These should not be stored in data, and can be renumbered at any time; These need to match the list in the GUI
			Intro = 0,
			Switching
		}

		/// <summary>To be called for any page which appears here.  For convenience returns the same object</summary>
		private ConfigPage AddPanel(ConfigPage pnl)
		{
			Strings.Translate(pnl, ttMain);
			pnl.Form = this;
			Controls.Add(pnl);
			pnl.Visible = false;
			pnl.Dock = DockStyle.Fill;
			pnl.BringToFront();
			if (pnl.Controls.Count == 1)
				pnl.Controls[0].Visible = true; // because they were designed with Visible = false when the panels were directly on this page
			return pnl;
		}

		public void pnlSections_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (pnlSections.SelectedIndex >= 0)
				m_SectionPanels[pnlSections.SelectedIndex].OnHide();
			for (int index = 0; index <= m_SectionPanels.Count - 1; index++)
			{
				ConfigPage section = m_SectionPanels[index];
				if (index == pnlSections.SelectedIndex)
				{
					// In the editor be sections are not placed correctly so that I can get at them all to edit them
					section.Dock = DockStyle.Fill;
					section.OnDisplay();
					section.Visible = true;
					section.Focus();
				}
				else
					section.Visible = false;
			}
		}

		internal Sections Section
		{
			get { return (Sections)pnlSections.SelectedIndex; }
			set { pnlSections.SelectedIndex = (int)value; }
		}

		#endregion

		#region Configuration utilities, auto change, change tracking

		private DatumList m_Changed = new DatumList(); // list of the ones which have changed

		internal void WrittenToCurrent()
		{
			if (!m_Changed.Contains(m_Config))
				m_Changed.Add(m_Config);
		}

		#endregion

		#region Prompts
		// The prompts are updated from a timer.  We can't use MouseMove on the form because this is not raised since the form is covered in controls
		// (and I think capturing would fail eventually due to pop up forms and combo boxes).  We could in theory attach Enter and Leave events to every control
		// but this seems inefficient and complicated

		private object m_ctrPromptDisplayed; // the control for which prompts are currently displayed' For tree views this will be the Node
		public void tmrPrompt_Tick(object sender, EventArgs e)
		{
			List<Shape.Prompt> list = new List<Shape.Prompt>();
			if (!this.ContainsFocus)
			{
				// This in case of modal dialogs appearing over this window.  Looks very strange if the prompts still respond to this window (responding to control which might be hidden behind the modal dialog)
				if (m_ctrPromptDisplayed != null)
				{
					Globals.SetHover(list);
					m_ctrPromptDisplayed = null;
				}
				return;
			}
			Control ctr = GUIUtilities.YoungestChildUnderMouse(this);
			if (ctr == m_ctrPromptDisplayed)
				return;
			if (ctr?.Parent is ctrActionSelect)
				ctr = ctr.Parent;
			if (ctr is ctrActionSelect)
			{
				ctrActionSelect action = (ctrActionSelect)ctr;
				TreeNode node = action.GetNodeAt(action.PointToClient(Cursor.Position));
				if (m_ctrPromptDisplayed == node)
					return;
				m_ctrPromptDisplayed = node;
				if (node?.Tag != null)
				{
					Functions.Action objAction = (Functions.Action)node.Tag;
					list.Add(objAction.GetPrompt());
				}
			}
			else if (ctr is TreeView) // other Treeviews are all in the toolbar section
			{
				TreeNode node = ((TreeView)ctr).GetNodeAt(ctr.PointToClient(Cursor.Position));
				if (m_ctrPromptDisplayed == node)
					return;
				m_ctrPromptDisplayed = node;
				if (node != null)
				{
					Functions.Action objAction = Functions.Verb.Find((Functions.Codes)node.Tag);
					list.Add(objAction.GetPrompt());
				}
			}
			else if (ctr?.Tag == null || !(ctr.Tag is string) || ctr.Tag.ToString() == "")
				m_ctrPromptDisplayed = null;
			else
			{
				m_ctrPromptDisplayed = ctr;
				string text = "ConfigPrompt_" + ctr.Tag;
				if (Strings.Exists(text))
					list.Add(new Shape.Prompt(Shape.ShapeVerbs.Info, text));
				if (Strings.Exists(text + "_2"))
					list.Add(new Shape.Prompt(Shape.ShapeVerbs.Info, text + "_2"));
#if DEBUG
				if (list.Count == 0)
					list.Add(new Shape.Prompt(Shape.ShapeVerbs.Info, "No prompts for: " + ctr.Tag, (Image)null));
#endif
			}
			Globals.SetHover(list); // this can deliberately set an empty list; we don't want to do ClearHover because this would allow any previous prompt from the current tool - it's better to clear the prompts area completely
									  //If objList.Count > 0 Then Globals.SetHover(objList)
		}

		#endregion

		#region Search
		private Control m_CurrentSearchControl; // last control highlighted by the search.  Reset by SetLevel
		private string m_LastSearchText = ""; // Always lower case

		public void tmrSearch_Tick(object sender, EventArgs e)
		{
			tmrSearch.Enabled = false;
			Search();
		}

		public void txtSearch_KeyDown(object sender, KeyEventArgs e)
		{
			if (Switches.Switching.PhysicalSwitch.MasterSwitch == Switches.Switching.PhysicalSwitch.MasterModes.Detect)
			{
				m_IgnoreNextKeyDueToDetection = true;
				e.Handled = true;
				return;
			}
			switch (e.KeyCode)
			{
				case Keys.Enter:
					tmrSearch.Enabled = false;
					e.Handled = true;
					e.SuppressKeyPress = true;
					if (txtSearch.TextLength > 0)
						Search();
					break;
				case Keys.Escape:
					if (txtSearch.TextLength > 0)
						txtSearch.Clear();
					else
						DialogResult = DialogResult.Cancel;
					break;
			}
		}

		public void txtSearch_TextChanged(object sender, EventArgs e)
		{
			tmrSearch.Enabled = false;
			if (txtSearch.TextLength > 0)
				tmrSearch.Enabled = true;
		}

		private void Search()
		{
			if (txtSearch.TextLength == 0)
			{
				m_CurrentSearchControl = null;
				return;
			}
			if (txtSearch.TextLength < m_LastSearchText.Length)
				m_CurrentSearchControl = null; // if the user backspaces the search always restarts from the beginning
			m_LastSearchText = txtSearch.Text;
			bool reachedCurrent = m_CurrentSearchControl == null; // true when we have passed the current item, and the next one should be selected
			for (int section = 0; section <= m_SectionPanels.Count - 1; section++)
			{
				if (!pnlSections.Controls[section].Visible)
				{
					continue; // Checks that the panel is applicable
				}
				foreach (Control ctr in m_SectionPanels[section].Controls)
				{
					var ctrMatches = ControlMatchesSearch(ctr, ref reachedCurrent);
					if (ctrMatches != null)
					{
						//	intFound += 1
						pnlSections.SelectedIndex = section; // NB this will likely clear m_ctrCurrentSearch
						m_CurrentSearchControl = ctrMatches;
						Debug.WriteLine("Search found: " + ctrMatches.GetType() + "/" + ctrMatches.Name);
						Rectangle rct = new Rectangle(ctrMatches.PointToScreen(Point.Empty), ctrMatches.Size); // Me.PointToClient
						m_frmHightlight.Owner = this;
						m_frmHightlight.Display();
						m_frmHightlight.SetBounds(rct.X, rct.Y, rct.Width, rct.Height);
						AnimationLinear.CreateStart(m_frmHightlight, 80, 2, 1);
						return;
					}
				}
			}
			if (m_CurrentSearchControl != null) // AndAlso intFound > 1 Then
			{
				// The currently highlighted item must be in the last one; need to start again from the beginning
				m_CurrentSearchControl = null;
				Search();
			}
			else
			{
				Console.Beep();
				m_CurrentSearchControl = null; // will allow re-find if there was a single item already found
			}
		}

		#region Search highlight form
		private readonly HighlightForm m_frmHightlight = new HighlightForm();
		private sealed class HighlightForm : Form, ILinearAnimated, IAnimationNotifyComplete
		{

			public HighlightForm()
			{
				this.FormBorderStyle = FormBorderStyle.None;
				this.ShowInTaskbar = false;
				this.ControlBox = false;
				this.BackColor = Color.LightGreen;
				this.SetStyle(ControlStyles.Selectable, false);
				this.Opacity = 0;
				this.Click += HighlightForm_Click;
			}


			/// <summary>Shows without gaining focus</summary>
			public void Display()
			{
				Windows.ShowWindow(this.Handle, Windows.SW_SHOWNOACTIVATE);
			}

			public void Complete()
			{
				this.Hide();
			}

			public int VariableValue
			{
				get { return (int)(this.Opacity * 100); }
				set { this.Opacity = value / 100f; }
			}

			private void HighlightForm_Click(object sender, EventArgs e)
			{
				this.Hide();
			}
		}

		#endregion

		/// <summary>Returns the actual control matching the search, which may be the parameter or any sub control</summary>
		private Control ControlMatchesSearch(Control ctr, ref bool reachedCurrent)
		{
			if (reachedCurrent) // Current control only eligible once searching properly; but we will still iterate the sub controls below
			{
				if (ctr.Text.ToLower().Contains(m_LastSearchText))
					return ctr;
				if (ttMain.GetToolTip(ctr).ToLower().Contains(m_LastSearchText))
					return ctr;
				if (ctr is ComboBox)
				{
					if (((ComboBox)ctr).Items.OfType<string>().Any(x => x.ToLower().Contains(m_LastSearchText)))
						return ctr;
				}
			}
			if (ctr == m_CurrentSearchControl) // Genuine searching only really happens AFTER we reach the current item
				reachedCurrent = true;
			// search sub-controls:
			foreach (Control sub in ctr.Controls)
			{
				var ctrResult = ControlMatchesSearch(sub, ref reachedCurrent);
				if (ctrResult != null)
					return ctrResult;
			}
			return null;
		}

		#endregion

		public void btnOK_Click(object sender, EventArgs e)
		{
			if (pnlSections.SelectedIndex >= 0)
				m_SectionPanels[pnlSections.SelectedIndex].OnHide();
			DialogResult = DialogResult.OK;
		}

		internal void CloseAndEditPalette(Document palette)
		{
			m_OpenPaletteOnExit = palette;
			this.DialogResult = DialogResult.OK;
		}

		#region Key suppression
		// when doing switch detection force cursor into search box - it's the only safe place to swallow ALL switchs
		private bool m_IgnoreNextKeyDueToDetection;// will ignore next press (ie which went with a given key down)
		private void PhysicalSwitch_MasterSwitchChanged()
		{
			if (Switches.Switching.PhysicalSwitch.MasterSwitch == Switches.Switching.PhysicalSwitch.MasterModes.Detect)
				txtSearch.Focus();
			else
			{
				txtSearch.Clear();
				tmrSearch.Enabled = false;
				m_IgnoreNextKeyDueToDetection = true;
			}
		}

		private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (m_IgnoreNextKeyDueToDetection || Switches.Switching.PhysicalSwitch.MasterSwitch == Switches.Switching.PhysicalSwitch.MasterModes.Detect)
				e.Handled = true;
			m_IgnoreNextKeyDueToDetection = false;
		}

		private void txtSearch_KeyUp(object sender, KeyEventArgs e)
		{
			if (Switches.Switching.PhysicalSwitch.MasterSwitch == Switches.Switching.PhysicalSwitch.MasterModes.Detect)
				e.Handled = true;

		}


		#endregion

	}

}
