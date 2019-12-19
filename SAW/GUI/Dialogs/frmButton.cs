using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Linq;
using SAW.Functions;
using Action = SAW.Functions.Action;

namespace SAW
{
	public partial class frmButton
	{

		private ButtonStyle m_Style;
		private readonly ButtonStyle m_OriginalStyle;
		private readonly ButtonShape m_Button;
		private List<ButtonStyle> m_Shared = new List<ButtonStyle>();
		private bool m_Filling;
		private Transaction m_Transaction;
		private static ButtonStyle m_LastUsedStyle; // used when creating new buttons; they get the same style as the last button where OK was pressed
													   // Note that this might be a shared style, or might be private to the original button (in which case it need to cloning)
		private static Document m_LastUsedDocument; // the document which the above came from.  In case it is shared we need to make sure we're talking about the same document!
		private Action m_Action; // cannot directly update m_Button because the View (on the original document) can have trouble rendering if the action is updated but not the image

		public frmButton(ButtonStyle style, ButtonShape button, Transaction transaction)
		{
			m_Filling = true;
			InitializeComponent();
			Strings.Translate(this); // must not be in Load, as the data occasionally has translatable items
			Strings.Translate(mnuImage);

			m_Button = button;
			m_Style = style; // must be before any parameters are attached
			m_OriginalStyle = style;
			m_Transaction = transaction;
			m_Action = button.Action;

			FillStylesList();
			ReflectCustomState();
			ctrStyle.DisplayStyle(m_Style);

			// content
			txtText.Text = button.LabelText;
			pnlImagePreview.Image = button.GetImageForPreview();
			if (button.TextRatio < 0)
			{
				sldTextRatio.Value = 50;
				rdoTextRatioDefault.Checked = true;
			}
			else
			{
				sldTextRatio.Value = (int)(button.TextRatio * 100);
				rdoTextRatioSet.Checked = true;
			}
			pnlImagePreview.NoImageString = Strings.Item("Button_NoImage");
			PrepareLayout();
			pnlLayout.SelectedIndex = Math.Min((int)m_Button.Layout & ((int)ButtonShape.Layouts.Superimpose - 1), pnlLayout.Controls.Count - 1);
			chkSuperimpose.Checked = (m_Button.Layout & ButtonShape.Layouts.Superimpose) > 0;
			ShowRatioAndLayout();

			// We need the configuration which will (probably) be in effect when the palette is used.  We assume user mode
			// as this is mainly to detect custom shapes.  Any available in user mode will also be available in teacher mode
			Document document = Globals.Root.CurrentDocument; // document being edited; PROBABLY a palette, although buttons can be placed on pages ??
			if (document.PaletteWithin != null)
				document = document.PaletteWithin.Document;
			AppliedConfig applied = new AppliedConfig();
			applied.AddConfigAtEnd(document.UserSettings); // Doesn't matter if any of the items passed to AddConfigAtEnd are nothing
			applied.AddConfigAtEnd(document.BothSettings);
			// Add the activity, if there is one specified in the document...
			if (!document.ActivityID.Equals(Guid.Empty))
			{
				Document activity = Activities.GetActivitySettings(document.ActivityID);
				if (activity != null)
				{
					applied.AddConfigAtEnd(activity.UserSettings);
					applied.AddConfigAtEnd(activity.BothSettings);
				}
			}
			applied.AddConfigAtEnd(Config.UserCurrent); // User mode is assumed.  Any custom shapes only present in teacher mode will be ignored
														   // (Note that the actions list isn't really interested in what is visible; only what is defined)
			applied.AddConfigAtEnd(Config.SystemConfig);
			ctrActions.Fill(applied);

			// Actions data
			chkDisplayFromAction.Checked = button.DisplayFromAction;
			ctrActions.SelectedAction = m_Action;
			if (button.Action.Change == Parameters.Action_Key)
				chkActionKeyAuto.Checked = !string.IsNullOrEmpty(button.LabelText) && button.LabelText[0].ToKeyData() == (button.Action as KeyAction).Key;
			else if (button.Action.Change == Parameters.Action_Character)
				chkActionKeyAuto.Checked = true; // I think the only way of achieving this is using the checkbox
			else if (button.Action.Change == Parameters.Action_Text)
				chkActionKeyAuto.Checked = button.LabelText == (button.Action as TextAction).Text;
			ReflectAction();
			m_Filling = false;
		}

		public void frmButton_Load(object sender, EventArgs e)
		{
			GUIUtilities.ScaleDPI(this);
		}

		public static bool Display(ButtonStyle style, ButtonShape button, Transaction transaction)
		{
			// note that the parameter is changed if the user selects a different style
			using (frmButton frm = new frmButton(style, button, transaction))
			{
				DialogResult result = frm.ShowDialog();
				frm.Dispose(); // Must be disposed immediately because the style panel registers itself with Globals
				if (result == DialogResult.OK)
				{
					if (style != frm.m_Style) // checks if a different style was selected
						if (!style.IsShared)
							transaction.Delete(style); // the old one can be deleted as it is not used anywhere else
															 //style = frm.m_Style
					if (frm.m_Style.IsShared)
					{
						// check if the style was edited; if so the whole document must be refreshed
						ButtonStyle previous = (ButtonStyle)transaction.GetObjectPrevious(frm.m_Style);
						if (previous != null)
						{
							if (!frm.m_Style.ContentEquals(previous))
								transaction.RequiresDocumentRepaint = true;
							// else we can ignore it - if Previous is nothing then this style has been newly created and cannot apply to any other buttons
						}
					}
					return true;
				}
				return false;
			}

		}

		public void btnOK_Click(object sender, EventArgs e)
		{
			// content:
			if (rdoTextRatioSet.Checked)
				m_Button.TextRatio = sldTextRatio.Value / 100f;
			else
				m_Button.TextRatio = -1;
			m_Button.LabelText = txtText.Text;
			if (m_ImageChanged)
				m_Button.SetImage(m_NewImage);
			ButtonShape.Layouts layout = (ButtonShape.Layouts)pnlLayout.SelectedIndex;
			if (chkSuperimpose.Checked)
				layout = layout | ButtonShape.Layouts.Superimpose;
			m_Button.Layout = layout;

			// Style:
			m_Button.SetStyleObject(m_Style);
			m_LastUsedStyle = m_Style;
			m_LastUsedDocument = Globals.Root.CurrentDocument;

			// Actions:
			m_Button.DisplayFromAction = chkDisplayFromAction.Checked;
			m_Button.Action = m_Action;

			this.DialogResult = DialogResult.OK;
		}

		/// <summary>The style to use for a newly created button.  The page can be specified which will use the most prevalent style on the page in preference</summary>
		internal static ButtonStyle GetStyleForNewButton(Page page)
		{
			ButtonStyle mostFrequent =
				(from styleInfo in
					 (from shape in page
					  where shape.ShapeCode == Shape.Shapes.Button
					  group shape by ((ButtonShape)shape).BackgroundStyleObject into g
					  select new { Style = g.Key, Appearances = g.Count() })
				 orderby styleInfo.Appearances descending
				 select styleInfo.Style).FirstOrDefault();
			if (mostFrequent != null && mostFrequent.IsShared) // If not shared it should be only one occurrence anyway!
				return mostFrequent;
			// Uses the last one which was accepted (cloned if necessary):
			ButtonStyle style;
			if (m_LastUsedStyle == null)
				style = ButtonStyle.UserDefaultActionInstance;
			else
			{
				if (m_LastUsedStyle.IsShared && m_LastUsedDocument == Globals.Root.CurrentDocument)
					// can reuse the last one; it is shared
					style = m_LastUsedStyle;
				else
				{
					// want to use the last one, but need to make a copy of it
					style = (ButtonStyle)m_LastUsedStyle.Clone(Mapping.Ignore);
					Debug.Assert(style.IsShared == false);
				}
			}
			return style;
		}

		#region Text ratio
		private void ShowRatioAndLayout()
		{
			lblTextRatio.Visible = pnlImagePreview.Image != null && !string.IsNullOrEmpty(txtText.Text);
			pnlTextRatio.Visible = pnlImagePreview.Image != null && !string.IsNullOrEmpty(txtText.Text);
			pnlLayout.Visible = pnlImagePreview.Image != null && !string.IsNullOrEmpty(txtText.Text);
			chkSuperimpose.Visible = pnlImagePreview.Image != null && !string.IsNullOrEmpty(txtText.Text);
		}

		public void txtText_TextChanged(object sender, EventArgs e)
		{
			ShowRatioAndLayout();
			RefreshPanels();
			if (!m_Filling)
			{
				// was a change by the user - remove the auto option
				chkDisplayFromAction.Checked = false;
			}
			if (m_Action.Change == Parameters.Action_Key || m_Action.Change == Parameters.Action_Character || m_Action.Change == Parameters.Action_Text)
			{
				if (chkActionKeyAuto.Checked)
				{
					SetKeyFromText();
					ReflectAction();
				}
			}
		}

		public void rdoTextRatioSet_CheckedChanged(object sender, EventArgs e)
		{
			sldTextRatio.Enabled = rdoTextRatioSet.Checked;
			RefreshPanels();
		}

		public void sldTextRatio_ValueChanged(object sender, EventArgs e)
		{
			RefreshPanels();
		}
		#endregion

		#region Image
		private SharedImage m_NewImage; // value to store in Button image.  Only defined if the image has been created here
		private bool m_ImageChanged = false;

		// M_NewImage can never be forcibly disposed, because it has always created by Document.AddShared... which use a CRC to check for duplicates
		// therefore it could any time be an external image
		public void btnImageRemove_Click(object sender, EventArgs e)
		{
			m_NewImage = null;
			pnlImagePreview.Image = null;
			ImageChanged();
			chkDisplayFromAction.Checked = false;
		}

		public void btnImageDisc_Click(object sender, EventArgs e)
		{
			string filename = FileDialog.ShowOpen(FileDialog.Context.Image);
			if (string.IsNullOrEmpty(filename))
				return;
			try
			{
				m_NewImage = (SharedImage)Globals.Root.CurrentDocument.AddSharedResourceFromFile(filename, m_Transaction);
				ImageChanged();
				chkDisplayFromAction.Checked = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		public void btnResource_Click(object sender, EventArgs e)
		{
			string ID = frmResource.ChooseImage(this);
			if (!string.IsNullOrEmpty(ID))
			{
				m_NewImage = Globals.Root.CurrentDocument.AddSharedBitmapForResource(ID, m_Transaction);
				ImageChanged();
				chkDisplayFromAction.Checked = false;
			}
		}

		private void ImageChanged()
		{
			m_ImageChanged = true;
			if (m_NewImage != null)
				pnlImagePreview.Image = m_NewImage.GetNetImage();
			ShowRatioAndLayout();
			RefreshPanels();
		}

		public void pnlImagePreview_MouseClick(object sender, MouseEventArgs e)
		{
			if (pnlImagePreview.Image == null)
				return;
			mnuImage.Show(pnlImagePreview, e.Location);
		}

		public void mnuExportImage_Click(object sender, EventArgs e)
		{
			var file = FileDialog.ShowSave(FileDialog.Context.Image, "*.png|*.png");
			if (string.IsNullOrEmpty(file))
				return;
			pnlImagePreview.Image.Save(file);
		}
		#endregion

		#region Layout
		private ButtonShape m_LayoutButton; // dummy used to draw panels

		private void PrepareLayout()
		{
			pnlLayout.Height = pnlLayout.Width / 5;
			int panelSize = pnlLayout.Height - pnlLayout.Padding.Vertical;
			for (int index = 0; index <= 4; index++)
			{
				Panel pnl = new Panel() { Tag = index, Margin = new Padding(0), Size = new Size(panelSize, panelSize) };
				pnl.Paint += DrawLayoutPanel;
				pnlLayout.Controls.Add(pnl);
			}
			m_LayoutButton = new ButtonShape();
			m_LayoutButton.InitialiseFreeStanding();
			m_LayoutButton.StyleObjectForParameter(Parameters.FillColour).SetParameterValue(0, Parameters.FillColour); // empty
			m_LayoutButton.GetTextStyle().SetParameterValue(m_Button.GetTextStyle().ParameterValue(Parameters.FontSize), Parameters.FontSize);
			float size = (float)Math.Sqrt(m_Button.Bounds.Height * m_Button.Bounds.Width);
			m_LayoutButton.SetPosition(new RectangleF(0, 0, size, size));
			// gives button natural looking size, but needs to be square
			RefreshPanels(); // sets some of the other data
		}

		private void DrawLayoutPanel(object sender, PaintEventArgs e)
		{
			Panel pnl = (Panel)sender;
			ButtonShape.Layouts layout = (ButtonShape.Layouts)Convert.ToInt32(pnl.Tag);
			//If eLayout = pnlLayout.SelectedIndex Then e.Graphics.Clear(Color.Orange)
			if (chkSuperimpose.Checked)
				layout = layout | ButtonShape.Layouts.Superimpose;
			m_LayoutButton.Layout = layout;
			//e.Graphics.PageUnit = GraphicsUnit.Millimeter
			float X = pnl.Width - 1; // / e.Graphics.DpiX) * INCH  ' pnl size in mm
			float Y = pnl.Height - 1; // / e.Graphics.DpiY) * INCH
			e.Graphics.ScaleTransform(X / m_LayoutButton.Bounds.Width, Y / m_LayoutButton.Bounds.Height);
			//e.Graphics.ScaleTransform(INCH / e.Graphics.DpiX, INCH / e.Graphics.DpiY)
			m_LayoutButton.Draw(new NetCanvas(e.Graphics), 1, e.Graphics.DpiX / Geometry.INCH, null, StaticView.InvalidationBuffer.Base);
			e.Graphics.ResetTransform();
			e.Graphics.PageUnit = GraphicsUnit.Pixel;
		}

		private void RefreshPanels()
		{
			// update some of the data - save doing this in DrawLayoutPanel which executes *5
			if (m_LayoutButton == null)
				return; // called in InitialiseComponent
			m_LayoutButton.LabelText = txtText.Text;
			m_LayoutButton.TextRatio = rdoTextRatioDefault.Checked ? -1.0f : Math.Max(sldTextRatio.Value / 100f, 0.02f); // max needed otherwise 0 gets counted as default by button
			if (m_ImageChanged)
				m_LayoutButton.SetImage(m_NewImage);
			else
				m_LayoutButton.SetImage(m_Button.GetImage());
			foreach (Panel pnl in pnlLayout.Controls)
			{
				pnl.Refresh();
			}
		}

		public void chkSuperimpose_CheckedChanged(object sender, EventArgs e)
		{
			RefreshPanels();
		}
		#endregion

		#region Style tab
		private void FillStylesList()
		{
			// fills in the drop-down list of standard styles
			bool old = m_Filling;
			m_Filling = true;
			m_Shared.Clear();
			cmbStyle.Items.Clear();
			// lnkDefaultStyle_LinkClicked relies on the default being the first item, and in this order
			m_Shared.Add(ButtonStyle.UserDefaultActionInstance);
			m_Shared.Add(ButtonStyle.UserDefaultSelectionInstance);
			foreach (ButtonStyle shared in Globals.Root.CurrentDocument.SharedButtonStyles())
			{
				Debug.Assert(shared.IsShared);
				m_Shared.Add(shared);
			}
			m_Shared.Sort(); // default is name order
			foreach (ButtonStyle style in m_Shared)
			{
				cmbStyle.Items.Add(Strings.Translate(style.Name));
				if (style == m_Style)
				{
					// don't want to use IndexOf to find it later, as that uses the Equals method - want reference equals here - need to find the actual object, not a matching one
					cmbStyle.SelectedIndex = cmbStyle.Items.Count - 1;
				}
			}
			if (!m_Style.IsShared)
			{
				Debug.Assert(!m_Shared.Contains(m_Style));
				m_Shared.Insert(0, m_Style); // Add(m_Style)	'
				cmbStyle.Items.Insert(0, Strings.Item("Button_CustomName"));
				cmbStyle.SelectedIndex = 0;
			}
			m_Filling = old;
		}

		private void ReflectCustomState()
		{
			if (m_Style.IsShared)
			{
				lblExplain.Text = Strings.Item("Button_IsShared");
				lnkShare.Text = Strings.Item("Button_MakeCustom");
			}
			else
			{
				lblExplain.Text = Strings.Item("Button_IsCustom");
				lnkShare.Text = Strings.Item("Button_MakeShared");
			}
			ctrStyle.Visible = !m_Style.IsUserDefault;
			lnkDefaultStyle.Visible = m_Style.IsUserDefault;
			lblExplain.Visible = !m_Style.IsUserDefault; // message looks a bit daft in the case of the users default
		}

		public void cmbStyle_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Style = m_Shared[cmbStyle.SelectedIndex];
			ctrStyle.DisplayStyle(m_Style);
			// the old custom style is left in the list for the moment.  The caller (ButtonShape) will delete it at the end if needed
			ReflectCustomState();
			m_Transaction.Edit(m_Style); // in case a shared style has been selected - need to make sure it is in the transaction
		}

		public void lnkDefaultStyle_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// Note that thisalways edits the USER one, not the teacher one
			frmUserButtonStyle.Display(Config.UserUser, cmbStyle.SelectedIndex); //
		}

		#endregion

		#region Sharing
		public void lnkShare_Click(object sender, EventArgs e)
		{
			if (m_Style.IsShared)
			{
				// The link is un-sharing. need to detect if any buttons except this one use this style
				m_Button.SetStyleObject(null);
				DatumList list = new DatumList();
				Globals.Root.CurrentDocument.IterateEx(obj => obj.AddRequiredReferences(list.Add, Mapping.Ignore), false, false);
				m_Button.SetStyleObject(m_OriginalStyle); // either way we put the object back in the button itself in case the user cancels this screen
				if (list.Contains(m_Style) || m_Style.IsUserDefault)
				{
					// this style is referenced elsewhere.  Need to make a copy
					ButtonStyle newStyle = (ButtonStyle)m_Style.Clone(Mapping.Ignore);
					newStyle.Name = "";
					newStyle.IsShared = false;
					m_Style = newStyle;
				}
				else
				{
					// this style is not used elsewhere; we can just make it shared
					m_Style.IsShared = false;
					Globals.Root.CurrentDocument.RemoveButtonStyle(m_Style);
					// can keep the name
				}
			}
			else // otherwise we are sharing a previously custom style
			{
				string name = frmButtonStyleName.Display(m_Style); // Name will only be set if this was previously shared
				if (string.IsNullOrEmpty(name))
					return;
				m_Style.IsShared = true;
				m_Style.Name = name;
				m_Transaction.Edit(Globals.Root.CurrentDocument);
				Globals.Root.CurrentDocument.AddButtonStyle(m_Style);
			}
			FillStylesList();
			ReflectCustomState();
			ctrStyle.DisplayStyle(m_Style);
		}

		#endregion

		#region Actions

		public void ctrActions_AfterSelect(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Filling = true;
			try
			{
				Action action = ctrActions.SelectedAction;
				m_Action = action;
				if (action.Change == Parameters.Action_Key)
				{
					// For simulated keys we don't update the text on selecting the action; we will update is the user actually types a key
					// However is there is already text, and there is no key selected yet, then we will fill it in based on the text
					if (txtText.Text.Length > 0 && (action as KeyAction).Key == Keys.None)
					{
						SetKeyFromText();
						// The display in the box will be updated by ReflectAction
					}
				}
				else if (action.Change == Parameters.Action_Character)
				{
					if (txtText.Text.Length > 0 && (action as CharAction).Character == '\0')
						SetKeyFromText();
				}
				else if (action.Change == Parameters.Action_Text)
				{
					if (txtText.Text.Length > 0 && string.IsNullOrEmpty((action as TextAction).Text))
						SetKeyFromText();
				}
				else if (chkDisplayFromAction.Checked && action.Change != Parameters.None)
				{
					string temp;
					ButtonShape.GetDisplayFromAction(m_Action, out temp, out m_NewImage, m_Transaction);
					txtText.Text = temp;
					ImageChanged();
				}
			}
			finally
			{
				m_Filling = false;
			}
			ReflectAction();
		}

		private void SetKeyFromText()
		{
			if (txtText.Text.Length == 0)
				return;
			if (m_Action.Change == Parameters.Action_Character)
			{
				if (txtText.Text.IsTranslatable())
					// Need to store the entire translatable text
					m_Action = new CharAction(txtText.Text);
				else
					m_Action = new CharAction(txtText.Text[0]);
			}
			else if (m_Action.Change == Parameters.Action_Text)
				m_Action = new TextAction(txtText.Text);
			else
			{
				Keys key = txtText.Text[0].ToKeyData();
				if (((int)key & 255) == 255) // seems to mean that there was no valid key
					m_Action = new CharAction(txtText.Text[0]);
				else
					m_Action = new KeyAction(key);
			}
		}

		private void ReflectAction()
		{
			// Displays the details of the action at the bottom
			pnlActionSelected.SuspendLayout();
			ctrActionColour.Visible = false;
			chkActionKeyAuto.Visible = false;
			txtActionKey.Visible = false;
			switch (m_Action.Change)
			{
				case Parameters.LineColour:
				case Parameters.FillColour:
				case Parameters.TextColour:
					ctrActionColour.Visible = true;
					ctrActionColour.CurrentColour = Color.FromArgb((m_Action as ParameterAction).Value);
					lblActionSelected.Text = Strings.Item("Button_SelectedAction") + " " + ParameterSupport.GetParameterTypeName(m_Action.Change) + " = ";
					break;
				case Parameters.Action_Key:
				case Parameters.Action_Character:
				case Parameters.Action_Text:
					chkActionKeyAuto.Visible = true;
					txtActionKey.Visible = true;
					// doesn't use action description because this will include the key as well, which looks odd when it is in the box
					if (m_Action.Change == Parameters.Action_Key)
					{
						txtActionKey.Text = GUIUtilities.KeyDescription((m_Action as KeyAction).Key);
						chkActionKeyAuto.Text = Strings.Item("Button_ActionKeyAuto");
						lblActionSelected.Text = Strings.Item("Button_SelectedAction") + " " + Strings.Item("Action_SimulateKey");
					}
					else
					{
						txtActionKey.Text = (m_Action as TextAction).Text;
						chkActionKeyAuto.Text = Strings.Item("Button_ActionCharacterAuto");
						lblActionSelected.Text = Strings.Item("Button_SelectedAction") + " " + Strings.Item(m_Action.Change == Parameters.Action_Text ? "Action_TypeText" : "Action_TypeCharacter");
					}
					break;
				default:
					lblActionSelected.Text = Strings.Item("Button_SelectedAction") + " " + m_Action.DescriptionWithoutAccelerator();
					break;
			}
			pnlActionSelected.ResumeLayout();
		}

		public void ctrActionColour_UserChangedColour(object sender, EventArgs e)
		{
			m_Filling = true;
			m_Action = new ParameterAction(m_Action.Change, ctrActionColour.CurrentColour.ToArgb());
			ctrActions.SelectedAction = Action.Empty;
			string temp;
			ButtonShape.GetDisplayFromAction(m_Action, out temp, out m_NewImage, m_Transaction);
			txtText.Text = temp;
			ImageChanged();
			m_Filling = false;
		}

		public void txtActionKey_KeyDown(object sender, KeyEventArgs e)
		{
			if (m_Action.Change == Parameters.Action_Character)
			{
				if (e.KeyData == Keys.Back)
				{ }
				else if (e.KeyData == Keys.Delete)
				{
					txtActionKey.Text = "";
					m_Action = new CharAction((char)0);
					e.SuppressKeyPress = true;
					e.Handled = true;
				}
				else if (e.KeyData == (Keys.V | Keys.Control))
				{
					if (Clipboard.ContainsText())
					{
						var strText = Clipboard.GetText();
						if (!string.IsNullOrEmpty(strText))
						{
							txtActionKey.Text = strText.Substring(0, 1);
							m_Action = new CharAction(strText[0]);
							e.SuppressKeyPress = true;
							e.Handled = true;
						}
					}
				}
			}
			if (m_Action.Change != Parameters.Action_Key)
				return;
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
					m_Filling = true;
					CombinedKeyEvent eCombined = new CombinedKeyEvent(e); // on Mac this will modify the key as needed
					txtActionKey.Text = GUIUtilities.KeyDescription(eCombined.KeyData);
					m_Action = new KeyAction(eCombined.KeyData);
					if (chkActionKeyAuto.Checked)
						// update the text
						txtText.Text = eCombined.KeyData.ToCharacter().ToString();
					m_Filling = false;
					break;
			}
			e.SuppressKeyPress = true;
			e.Handled = true;
		}

		public void txtActionKey_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (m_Action.Change != Parameters.Action_Character)
				return;
			e.Handled = true;
			var ch = e.KeyChar;
			if (ch < 32)
				return; // don't type unprintable "characters".  Tab would make some sense, but can probably be done as a key anyway
			m_Filling = true;
			txtActionKey.Text = ch.ToString();
			m_Action = new CharAction(ch);
			if (chkDisplayFromAction.Checked || chkActionKeyAuto.Checked)
			{
				// update the text
				txtText.Text = ch.ToString();
			}
			m_Filling = false;
		}

		public void txtActionKey_TextChanged(object sender, EventArgs e)
		{
			if (m_Action.Change != Parameters.Action_Text)
				return;
			m_Filling = true;
			m_Action = new TextAction(txtActionKey.Text);
			if (chkDisplayFromAction.Checked || chkActionKeyAuto.Checked)
				// update the text
				txtText.Text = txtActionKey.Text;
			m_Filling = false;
		}

		public void chkActionKeyAuto_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			if (chkActionKeyAuto.Checked)
			{
				SetKeyFromText();
				ReflectAction();
			}
		}

		#endregion

	}
}
