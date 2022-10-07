using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;
using SAW.Shapes;

// I get this when editing text alignment: https://stackoverflow.com/questions/50417999/how-to-opt-in-to-accessibility-improvements
// can't see how it's relevant

namespace SAW
{
	internal partial class frmSAWItem : Form
	{

		private bool m_Filling;
		/// <summary>All items being edited.  Usually one.  Some UI elements can only work on 1 element (eg text, sound, graphic) and will use m_Items.First()</summary>
		private readonly List<Item> m_Items;
		private readonly List<Scriptable> m_Scriptables;
		private static readonly StyleOption[] StyleOptions;
		private readonly Transaction m_Transaction;
		/// <summary>True if data contains Items which can be edited.  If not many parts are disabled/removed </summary>
		private bool EditingItems = true;
		private Page m_Page;

		#region Construct/Dispose

		public frmSAWItem(List<Item> items, List<Scriptable> scriptables, Transaction transaction)
		{
			m_Filling = true; // cleared by Fill
			InitializeComponent();
			Strings.Translate(this);
			// setting these 2 in editor makes their position go nuts while editing
			lblLargerRatio.Anchor = AnchorStyles.Right | AnchorStyles.Top;
			ctrTextRatio.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

			m_Page = (items.FirstOrDefault() as Shape ?? scriptables.FirstOrDefault()).FindPage();
			if (!items.Any())
			{
				items.Add(new Item() { StyleType = Item.ItemDisplayTypes.IDT_Item });
				EditingItems = false;
			}
			m_Items = items;
			m_Scriptables = scriptables;
			m_Transaction = transaction;
			cmbShape.Items.Add(Strings.Item("Button_Rectangular"));
			cmbShape.Items.Add(Strings.Item("Button_Rounded"));
			cmbShape.Items.Add(Strings.Item("Button_Elliptical"));
			foreach (string s in new[] { "SAW_Edit_None", "SAW_Edit_Thin", "SAW_Edit_Medium", "SAW_Edit_Thick", "SAW_Edit_ExtraThick" })
			{
				cmbThicknessNormal.Items.Add(Strings.Item(s));
				cmbThicknessHighlight.Items.Add(Strings.Item(s));
			}
			cmbStyle.DataSource = StyleOptions;
			cmbStyle.DisplayMember = "Text";
			cmbStyle.ValueMember = "Style";
			rdoAlternate.Checked = true;
			Fill();
			tmrPreview.Tick += m_tmrPreview_Tick;
			foreach (RadioButton r in grpGraphicAlignment.Controls)
			{
				r.CheckedChanged += GraphicAlign_CheckedChanged;
			}
			foreach (RadioButton r in grpTextAlignment.Controls)
			{
				r.CheckedChanged += TextAlign_CheckedChanged;
			}
			rdoTextAbove.CheckedChanged += Alignment_CheckedChanged;
			rdoTextBelow.CheckedChanged += Alignment_CheckedChanged;
			rdoTextLeft.CheckedChanged += Alignment_CheckedChanged;
			rdoTextRight.CheckedChanged += Alignment_CheckedChanged;
			rdoTextOverlay.CheckedChanged += Alignment_CheckedChanged;

			ShowStyleWarning();
			if (Globals.Root.CurrentConfig.ReadBoolean(Config.SAW_Prompts, true) == false)
				lblHelpHeader.Text += " " + Strings.Item("SAW_Edit_Help_IsOff");
			else if (Globals.Root.CurrentPage.HelpSAWID <= 0)
				lblHelpHeader.Text += " " + Strings.Item("SAW_Edit_Help_NoItem");

			if (!EditingItems)
			{
				tcMain.TabPages.Remove(tpLayout);
				//tcMain.TabPages.Remove(tpPresentation);
				tcMain.SelectedIndex = 2; // start on scripts

				lblStyleWarning.Enabled = false; // style is left visible to avoid having an odd gap at top of page
				cmbStyle.Enabled = false;
				lblItemStyle.Enabled = false;
				lblBorderShape.Visible = false; // border shape is at bottom and can just be hidden
				cmbShape.Visible = false;

				lblLineSpacing.Visible = txtDisplay.Visible = nudLineSpacing.Visible = chkShowText.Visible = btnFont.Visible = false;
				tablePresentation.RowStyles[2].SizeType = SizeType.AutoSize;
				chkOutputTextSame.Visible = chkSpeechTextSame.Visible = false;
				flowGraphic.Visible = pnlGraphic.Visible = btnImageBrowse.Visible = btnImageClear.Visible = btnCCF.Visible = btnOpenSymbol.Visible = false;
				chkPopup.Visible = chkWordlistCustom.Visible = chkEscapeItem.Visible = false;

				lblDisplayText.Text = Strings.Item("SAW_Edit_GraphicPresentation");
				lblDisplayText.Font = new Font(lblDisplayText.Font.FontFamily, 10);
				lblDisplayText.Padding = new Padding(0, 0, 0, 8);

				// add empty row at bottom to absorb spare space
				tablePresentation.RowCount += 1;
				tablePresentation.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
			}
		}

		protected override void Dispose(bool disposing)
		{
			foreach (RadioButton r in grpGraphicAlignment.Controls)
			{
				r.CheckedChanged -= GraphicAlign_CheckedChanged;
			}
			foreach (RadioButton r in grpTextAlignment.Controls)
			{
				r.CheckedChanged -= TextAlign_CheckedChanged;
			}
			tmrPreview.Tick -= m_tmrPreview_Tick;
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			Player?.Dispose();
			base.Dispose(disposing);
		}

		/// <summary>One of the values in the item style dropdown list</summary>
		public class StyleOption
		{
			public string Text { get; set; }
			public Item.ItemDisplayTypes Style { get; set; }

			public StyleOption(string str, Item.ItemDisplayTypes style)
			{
				Text = Strings.Item(str);
				Style = style;
			}

		}

		static frmSAWItem()
		{
			StyleOptions = new[] {
				new StyleOption("SAW_Style_Output", Item.ItemDisplayTypes.IDT_Output),
					new StyleOption("SAW_Style_Open", Item.ItemDisplayTypes.IDT_Open),
					new StyleOption("SAW_Style_Group", Item.ItemDisplayTypes.IDT_Group),
					new StyleOption("SAW_Style_NotScanned", Item.ItemDisplayTypes.IDT_NotScanned),
					new StyleOption("SAW_Style_Help", Item.ItemDisplayTypes.IDT_Help),
					new StyleOption("SAW_Style_Top", Item.ItemDisplayTypes.IDT_TopItem),
					new StyleOption("SAW_Style_Escape", Item.ItemDisplayTypes.IDT_Escape),
					new StyleOption("SAW_Style_Custom", Item.ItemDisplayTypes.IDT_Item)
			};
		}

		#endregion

		/// <summary>Edits the item(s) currently selected in the view</summary>
		public static DialogResult Display(StaticView view)
		{
			Transaction transaction = new Transaction();
			List<Item> items = new List<Item>();
			List<Scriptable> scriptables = new List<Scriptable>();
			// probably selected items are scriptables, but this is written to cope with all cases (eg naked Items not inside a Scriptable)
			foreach (Shape selected in view.CurrentPage.SelectedShapes)
			{
				if (selected is Item item)
				{
					items.Add(item);
					if (item.Parent is Scriptable scriptable)
						scriptables.Add(scriptable);
				}
				else if (selected is Scriptable s)
				{
					scriptables.Add(s);
					if (s.Element is Item item1)
						items.Add(item1);
				}
			}
			foreach (Scriptable s in scriptables)
				transaction.Edit(s);
			foreach (Item i in items)
			{
				transaction.Edit(i);
				i.BeingEdited = true;
			}
			// there must always be at least one item, but not necessarily scriptables
			Globals.StoreEvent("Log " + items.Count + " items, " + scriptables.Count + " scriptable, first ID=" + (scriptables?.First()?.SAWID ?? 0));

			using (var frm = new frmSAWItem(items, scriptables, transaction))
			{
				// deselect it, so that we get a better preview of what it looks like:
				frm.m_Page.SelectOnly((Shape)null);
				Shape beside = scriptables.Any() ? (Shape)scriptables.First() : items.First();
				GUIUtilities.PositionNewFormBeside(frm, view.ShapeBounds(beside, true));
				bool ok = frm.ShowDialog() == DialogResult.OK;
				foreach (Item i in items)
					i.BeingEdited = false;
				frm.SetPreviewState(ButtonShape.States.Normal);
				view.CurrentPage.SelectOnly(scriptables.Cast<Shape>().ToList());
				if (ok)
				{
					Globals.Root.StoreNewTransaction(transaction);
					return DialogResult.OK;
				}
				transaction.Cancel();
				return DialogResult.Cancel;
			}
		}

		#region Read/write data
		private void Fill()
		{
			m_Filling = true;
			txtDisplay.Text = m_Items.First().LabelText?.Replace("\r", "\r\n") ?? "";
			MergeBooleansIntoCheckbox(chkShowText, from i in m_Items select i.TextShown);
			if (m_Items.Count == 1)
				nudLineSpacing.Value = m_Items.First().LineSpace;
			else
				txtDisplay.Text = Strings.Item("SAW_Edit_MultipleRestrictions");

			txtHelp.Enabled = chkOutputTextSame.Enabled = chkSpeechTextSame.Enabled = txtOutputText.Enabled = txtSpeechText.Enabled = nudLineSpacing.Enabled = txtDisplay.Enabled = m_Items.Count == 1;
			// must be before check boxes are assigned, as they can also disable
			txtOutputText.Text = m_Scriptables.First().OutputText;
			chkOutputTextSame.Checked = m_Scriptables.First().OutputAsDisplay;

			txtSpeechText.Text = m_Scriptables.First().SpeechText;
			chkSpeechTextSame.Checked = m_Scriptables.First().SpeechAsDisplay;
			txtHelp.Text = m_Scriptables.First().PromptText;

			MergeBooleansIntoCheckbox(chkShowGraphic, from i in m_Items select i.GraphicShown);
			MergeBooleansIntoCheckbox(chkGraphicOnlyHighlight, from i in m_Items select i.GraphicOnlyOnHighlight);
			btnImageClear.Enabled = m_Items.Count == 1 && m_Items.Any(i => i.Image != null);
			btnCCF.Enabled = btnImageBrowse.Enabled = m_Items.Count == 1;
			SetGraphicTooltip();

			// attributes:
			MergeBooleansIntoCheckbox(chkPopup, from s in m_Scriptables select s.Popup);
			MergeBooleansIntoCheckbox(chkWordlistCustom, from i in m_Items select i.WordlistDoesntFill);
			MergeBooleansIntoCheckbox(chkResetSwap, from s in m_Scriptables select s.ResetSwap);
			MergeBooleansIntoCheckbox(chkNotScanned, from s in m_Scriptables select s.NotVisited);
			MergeBooleansIntoCheckbox(chkAutoRepeatable, from s in m_Scriptables select s.AutoRepeat);
			MergeBooleansIntoCheckbox(chkEscapeItem, from i in m_Items select i.IsEscape);
			chkEscapeItem.Enabled = !m_Items.Any(i => i.IsGroup); // group items will ignore escape flag

			// colour + style
			Item.ItemDisplayTypes? styletype = MergeStates(from i in m_Items select i.StyleType);
			if (styletype.HasValue)
				cmbStyle.SelectedValue = styletype.Value;
			MergeColoursIntoPicker(clrNormalText, from i in m_Items select i.TextStyle.Colour);
			MergeColoursIntoPicker(clrHighlightText, from s in m_Scriptables select s.HighlightStyle.TextColour);
			MergeBooleansIntoCheckbox(chkNormalFilled, from i in m_Items select i.FillStyle.Pattern != Shape.FillStyleC.Patterns.Empty);
			MergeColoursIntoPicker(clrNormalBack, from i in m_Items select i.FillStyle.Colour);
			MergeColoursIntoPicker(clrNormalBack, Parameters.FillColour);
			//MergeBooleansIntoCheckbox(chkHighlightFilled, from s in m_Scriptables select s.HighlightStyle.FillColour.A != 0);
			MergeColoursIntoPicker(clrHighlightBack, from s in m_Scriptables select s.HighlightStyle.FillColour);
			//MergeColoursIntoPicker(clrNormalBorder, from i in m_Items select i.LineStyle.Colour);
			MergeColoursIntoPicker(clrNormalBorder, Parameters.LineColour);
			MergeColoursIntoPicker(clrHighlightBorder, from s in m_Scriptables select s.HighlightStyle.LineColour);
			try
			{
				// Unlike ALL there is no "none" option.  That is implemented by setting the thickness to 0
				cmbShape.SelectedIndex = MergeStates(from i in m_Items select (int)i.BorderShape) ?? -1;
				cmbThicknessNormal.SelectedIndex = MergeStates(from i in m_Items select IntegerThickness(i.LineStyle.Width)) ?? -1;
				cmbThicknessHighlight.SelectedIndex = MergeStates(from s in m_Scriptables select IntegerThickness(s.HighlightStyle.LineWidth)) ?? -1;
			}
			catch // Any invalid values will leave all at default
			{
				cmbShape.SelectedIndex = 0;
				cmbThicknessNormal.SelectedIndex = 2;
				cmbThicknessHighlight.SelectedIndex = 2;
			}
			// back colour enabled was set by property merge.  Copy to other controls:
			chkNormalFilled.Enabled = chkHighlightFilled.Enabled = clrHighlightBack.Enabled = clrNormalBack.Enabled;
			if (!clrNormalBack.Enabled)
				lblBackColourHeader.Text += " " + Strings.Item("SAW_Edit_NA");

			ButtonLayouts? arrangement = MergeStates(from i in m_Items select i.Arrangement);
			if (arrangement.HasValue) // if not, then no item is selected
				switch (arrangement.Value)
				{ // note sense is different: Layouts enum references graphic position
					case ButtonLayouts.Below: rdoTextAbove.Checked = true; break;
					case ButtonLayouts.Left: rdoTextRight.Checked = true; break;
					case ButtonLayouts.Right: rdoTextLeft.Checked = true; break;
					case ButtonLayouts.CentreBoth: rdoTextOverlay.Checked = true; break;
					//case ButtonShape.Layouts.Above:
					default: rdoTextBelow.Checked = true; break;
				}
			float? textRatio = MergeStates(from i in m_Items select i.TextRatio);
			chkRatioAutomatic.Visible = ctrTextRatio.Visible = lblLargerRatio.Visible = lblSmallerRatio.Visible = lblTextRatioTitle.Visible = textRatio.HasValue;
			if (textRatio.HasValue)
			{
				ctrTextRatio.Value = textRatio >= 0 ? (int)(textRatio * 100) : 50;
				chkRatioAutomatic.Checked = textRatio < 0;
			}

			uint textAlignAsInt = (uint?)MergeStates(from i in m_Items select i.TextAlign) ?? 0xffff;
			RadioButton rdo = (from RadioButton r in grpTextAlignment.Controls where r.Tag.ToString() == textAlignAsInt.ToString() select r).FirstOrDefault();
			if (rdo != null)
				rdo.Checked = true;
			uint graphicAlignAsInt = (uint?)MergeStates(from i in m_Items select i.TextAlign) ?? 0xffff;
			rdo = (from RadioButton r in grpGraphicAlignment.Controls where r.Tag.ToString() == graphicAlignAsInt.ToString() select r).FirstOrDefault();
			if (rdo != null)
				rdo.Checked = true;

			if (m_Scriptables.Count == 1)
			{
				ctrScripts.Edit(m_Scriptables.First());
				Text = Strings.Item("SAW_Edit_Item") + " : ID " + m_Scriptables.First().SAWID;
			}
			else
			{
				tcMain.TabPages.RemoveAt(3);
				Text = Strings.Item("SAW_Edit_Item_Multiple");
			}

			ReflectSoundButtons();
			m_Filling = false;
		}

		/// <summary>Used to decide the check box state if N items are edited, with the given boolean states</summary>
		private static void MergeBooleansIntoCheckbox(CheckBox chk, IEnumerable<bool> states)
		{
			bool? result = MergeStates(states);
			if (!result.HasValue)
				chk.CheckState = CheckState.Indeterminate;
			else
				chk.CheckState = result.Value ? CheckState.Checked : CheckState.Unchecked;
			chk.Enabled = states.Any();
		}

		private static void MergeColoursIntoPicker(ColourPicker ctr, IEnumerable<Color> colors)
		{
			Color? result = MergeStates(colors);
			ctr.DisplayCurrentColour = result.HasValue;
			if (result.HasValue)
				ctr.CurrentColour = result.Value;
			else
				ctr.CurrentColour = colors.First();
		}

		/// <summary>Version of MergeColoursIntoPicker that uses style parameters in case individual items are not in use by some shapes and disables
		/// control if not applicable</summary>
		private void MergeColoursIntoPicker(ColourPicker ctr, Parameters parameter)
		{
			List<Shape.StyleBase> styles = (from s in m_Scriptables select s.StyleObjectForParameter(parameter)).Where(s => s != null).ToList();
			ctr.Enabled = styles.Any();
			if (styles.Any())
				MergeColoursIntoPicker(ctr, from s in styles select Color.FromArgb(s.ParameterValue(parameter)));
		}

		/// <summary>Merges list of states returning value if all the same, or null if differing</summary>
		private static T? MergeStates<T>(IEnumerable<T> states) where T : struct
		{
			if (!states.Any())
				return null;
			T result = states.First();
			foreach (T state in states)
				if (!state.Equals(result))
					return null;
			return result;
		}

		/// <summary>converts to int safely, rounding off (in case non-SAW precise values were used)</summary>
		private static int IntegerThickness(float thickness) => (int)Math.Floor(thickness + 0.5f);

		/// <summary>Called when user makes any UI changes</summary>
		private void Updated()
		{
			foreach (Item i in m_Items)
				i.WasEdited();
			foreach (Scriptable s in m_Scriptables)
				s.WasEdited();
		}

		/// <summary>Returns an error message, or null if none</summary>
		private string GetValidationError()
		{ // not using the in-built validation as it mostly works on LostFocus on internal controls, and is a pain
			if (m_Scriptables.Count == 1)
				return ctrScripts.GetValidationError();
			return null;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			string error = GetValidationError();
			if (error != null)
				MessageBox.Show(error);
			else
				DialogResult = DialogResult.OK;
		}

		#endregion

		#region Text areas

		private void chkOutputTextSame_CheckedChanged(object sender, EventArgs e)
		{ // only for single item
			txtOutputText.Enabled = !chkOutputTextSame.Checked && m_Items.Count == 1;
			if (chkOutputTextSame.Checked)
			{
				txtOutputText.Text = txtDisplay.Text;
				m_Scriptables.First().OutputText = txtDisplay.Text.Replace("\r\n", "\r");
			}
			if (m_Filling)return;
			m_Scriptables.First().OutputAsDisplay = chkOutputTextSame.Checked;
			Updated();
		}

		private void txtOutputText_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)return;
			m_Scriptables.First().OutputText = txtOutputText.Text;
			Updated();
		}

		private void chkShowText_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			foreach (Item i in m_Items)
				i.TextShown = chkShowText.Checked;
			Updated();
		}

		private void chkSpeechTextSame_CheckedChanged(object sender, EventArgs e)
		{
			txtSpeechText.Enabled = !chkSpeechTextSame.Checked && m_Items.Count == 1;
			if (chkSpeechTextSame.Checked)
			{
				txtSpeechText.Text = txtDisplay.Text;
				m_Scriptables.First().SpeechText = txtDisplay.Text.Replace("\r\n", "\r");
			}
			if (m_Filling)return;
			m_Scriptables.First().SpeechAsDisplay = chkSpeechTextSame.Checked;
			Updated();
		}

		private void txtSpeechText_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling) return;
			m_Scriptables.First().SpeechText = txtSpeechText.Text.Replace("\r\n", "\r");
			Updated();
		}

		private void txtDisplay_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling) return;
			string text = txtDisplay.Text.Replace("\r\n", "\r");
			m_Items.First().LabelText = text;
			if (chkOutputTextSame.Checked)
			{
				m_Scriptables.First().OutputText = text;
				txtOutputText.Text = text;
			}
			if (chkSpeechTextSame.Checked)
			{
				m_Scriptables.First().SpeechText = text;
				txtSpeechText.Text = text;
			}
			Updated();
		}

		private void txtHelp_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling) return;
			string text = txtHelp.Text.Replace("\r\n", "\r");
			m_Scriptables.First().PromptText = text;
			Updated();
		}

		private void nudLineSpacing_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			m_Items.First().LineSpace = (uint)nudLineSpacing.Value;
			Updated();
		}

		private void btnFont_Click(object sender, EventArgs e)
		{
			using (frmFont frm = new frmFont())
			{
				Font font = m_Items.First().TextStyle?.CreateFont(m_Items.First().TextStyle.Size);
				if (font != null)
					frm.SelectedFont = font;
				if (frm.ShowDialog() != DialogResult.OK)
					return;
				foreach (Item i in m_Items)
					i.TextStyle.ApplyFont(frm.SelectedFont);
				Updated();
			}
		}

		private void btnTestSpeech_Click(object sender, EventArgs e)
		{
			Globals.Root.Speech.Speak(txtSpeechText.Text);
		}

		#endregion

		#region Media
		private void pnlGraphic_Paint(object sender, PaintEventArgs e)
		{
			if (m_Items.First().Image == null)
				return;
			Rectangle rct = new Rectangle(0, 0, pnlGraphic.Width, pnlGraphic.Height);
			GUIUtilities.CalcDestRect(m_Items.First().Image.Content.GetSize(), ref rct);
			m_Items.First().Image.Content.MemoryImage.Draw(e.Graphics, rct);
			//e.Graphics.DrawImage(m_Items.First().Image.Content.GetNetImage(), rct);
		}

		private void btnImageBrowse_Click(object sender, EventArgs e)
		{
			string path = FileDialog.ShowOpen(FileDialog.Context.Image);
			if (!string.IsNullOrEmpty(path))
				ChangeImage(path);
		}

		private void ChangeImage(string path)
		{
			var image = (SharedImage)Globals.Root.CurrentDocument.AddSharedResourceFromFile(path, m_Transaction);
			m_Items.First().Image = new SharedReference<SharedImage>(image);
			m_Items.First().ImageName = System.IO.Path.GetFileName(path);
			pnlGraphic.Invalidate();
			Updated();
			btnImageClear.Enabled = true;
			SetGraphicTooltip();
		}

		private void btnCCF_Click(object sender, EventArgs e)
		{
			string updateText;
			string conceptID = m_Items.First().ConceptID;
			string imagePath = CCF.frmLookup.DoLookup(txtDisplay.Text, ref conceptID, out updateText);
			if (!string.IsNullOrEmpty(imagePath))
			{
				m_Items.First().ConceptID = conceptID;
				imagePath = System.IO.Path.Combine(CCF.Connection.LocalPath, imagePath);
				ChangeImage(imagePath);
			}
			if (!string.IsNullOrEmpty(updateText))
			{
				// these will trigger the event and react as if the user made the changes:
				txtDisplay.Text = updateText;
				txtOutputText.Text = updateText;
			}
		}

		private void btnOpenSymbol_Click(object sender, EventArgs e)
		{
			string imagePath = frmOpenSymbol.DoSearch(txtDisplay.Text);
			if (!string.IsNullOrEmpty(imagePath))
			{
				imagePath = System.IO.Path.Combine(CCF.Connection.LocalPath, imagePath);
				ChangeImage(imagePath);
			}
		}

		private void chkShowGraphic_CheckedChanged(object sender, EventArgs e)
		{
			chkGraphicOnlyHighlight.Enabled = chkShowGraphic.Checked;
			if (m_Filling)
				return;
			foreach (Item i in m_Items)
				i.GraphicShown = chkShowGraphic.Checked;
			Updated();
		}

		private void chkGraphicOnlyHighlight_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			foreach (Item i in m_Items)
				i.GraphicOnlyOnHighlight = chkGraphicOnlyHighlight.Checked;
			Updated();
		}


		private void btnImageClear_Click(object sender, EventArgs e)
		{
			foreach (Item i in m_Items)
				i.Image = null;
			pnlGraphic.Invalidate();
			Updated();
			btnImageClear.Enabled = false;
			SetGraphicTooltip();
		}

		private void SetGraphicTooltip()
		{
			ttGraphic.SetToolTip(pnlGraphic, m_Items.FirstOrDefault()?.ImageName ?? "");
		}

		private readonly AudioPlayer Player = new AudioPlayer();
		// player doesn't support detecting if it is playing, but we can create the effect by playing sync in background: https://stackoverflow.com/questions/27392396/how-to-know-when-soundplayer-has-finished-playing-a-sound
		private void ReflectSoundButtons()
		{
			SharedReference<SharedResource> sound = m_Scriptables.First().Sound;
			btnSoundPlay.Enabled = !Player.Playing && sound != null && m_Items.Count == 1;
			btnSoundStop.Enabled = Player.Playing;
			btnSoundDelete.Enabled = !Player.Playing && m_Scriptables.Any(s => s.Sound != null);
			btnSoundBrowse.Enabled = !Player.Playing && m_Items.Count == 1;
			lblSoundName.Text = sound?.Content?.Filename ?? "";
		}

		private void btnSoundPlay_Click(object sender, EventArgs e)
		{
			if (m_Scriptables.First().Sound == null)
				return;
			Player.PlayAsync(m_Scriptables.First().Sound.Content.GetStream(), ReflectSoundButtons, this);
			ReflectSoundButtons();
		}

		private void btnSoundStop_Click(object sender, EventArgs e)
		{
			Player.Stop();
			ReflectSoundButtons();
		}

		private void btnSoundDelete_Click(object sender, EventArgs e)
		{
			foreach (Scriptable s in m_Scriptables)
				s.Sound = null;
			ReflectSoundButtons();
			Updated();
		}

		private void btnSoundBrowse_Click(object sender, EventArgs e)
		{
			string path = FileDialog.ShowOpen(FileDialog.Context.Sound);
			if (string.IsNullOrEmpty(path))
				return;
			SharedResource sound = (SharedResource)Globals.Root.CurrentDocument.AddSharedResourceFromFile(path, m_Transaction, false);
			m_Scriptables.First().Sound = new SharedReference<SharedResource>(sound);
			pnlGraphic.Invalidate();
			ReflectSoundButtons();
			Updated();
		}

		#endregion

		#region Other state
		private void chkEscapeItem_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			foreach (Item i in m_Items)
				i.IsEscape = chkEscapeItem.Checked;
			Updated();
		}

		private void chkAutoRepeatable_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			foreach (Scriptable s in m_Scriptables)
				s.AutoRepeat = chkAutoRepeatable.Checked;
			Updated();
		}

		private void chkNotScanned_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			foreach (Scriptable s in m_Scriptables)
				s.NotVisited = chkNotScanned.Checked;
			Updated();
		}

		private void chkPopup_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			foreach (Scriptable s in m_Scriptables)
				s.Popup = chkPopup.Checked;
			Updated();
		}

		private void chkResetSwap_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			foreach (Scriptable s in m_Scriptables)
				s.ResetSwap = chkResetSwap.Checked;
			Updated();
		}

		private void chkWordlistCustom_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			foreach (Scriptable s in m_Scriptables)
				(s.Element as Item).WordlistDoesntFill = chkWordlistCustom.Checked;
			Updated();
		}

		#endregion

		#region Colour page events
		private void chkNormalFilled_CheckedChanged(object sender, EventArgs e)
		{
			clrNormalBack.Visible = chkNormalFilled.Checked;
			if (m_Filling) return;
			foreach (Item i in m_Items)
				i.FillStyle.Pattern = chkNormalFilled.Checked ? Shape.FillStyleC.Patterns.Solid : Shape.FillStyleC.Patterns.Empty;
			Updated();
		}
		private void chkHighlightFilled_CheckedChanged(object sender, EventArgs e)
		{
			clrHighlightBack.Visible = chkHighlightFilled.Checked;
			if (m_Filling) return;
			foreach (Scriptable s in m_Scriptables)
			{
				if (!chkHighlightFilled.Checked)
					s.HighlightStyle.FillColour = Color.Transparent;
				else if (s.HighlightStyle.FillColour.A == 0)
					s.HighlightStyle.FillColour = ((Shape.FillStyleC)s.Element.StyleObjectForParameter(Parameters.FillColour))?.Colour ?? Color.Red;
			}
			Updated();
		}

		private void clrNormalText_UserChangedColour(object sender, EventArgs e)
		{
			foreach (Item i in m_Items)
				i.TextStyle.Colour = clrNormalText.CurrentColour;
			clrNormalText.DisplayCurrentColour = true;
			Updated();
		}

		private void clrHighlightText_UserChangedColour(object sender, EventArgs e)
		{
			foreach (Scriptable s in m_Scriptables)
				s.HighlightStyle.TextColour = clrHighlightText.CurrentColour;
			clrHighlightText.DisplayCurrentColour = true;
			Updated();
		}

		private void clrNormalBack_UserChangedColour(object sender, EventArgs e)
		{
			foreach (Item i in m_Items)
				i.FillStyle.Colour = clrNormalBack.CurrentColour;
			chkNormalFilled.Checked = !clrNormalBack.CurrentColour.IsEmpty;
			clrNormalBack.DisplayCurrentColour = true;
			Updated();
		}

		private void clrHighlightBack_UserChangedColour(object sender, EventArgs e)
		{
			foreach (Scriptable s in m_Scriptables)
				s.HighlightStyle.FillColour = clrHighlightBack.CurrentColour;
			chkHighlightFilled.Checked = !clrHighlightBack.CurrentColour.IsEmpty;
			clrHighlightBack.DisplayCurrentColour = true;
			Updated();
		}

		private void clrNormalBorder_UserChangedColour(object sender, EventArgs e)
		{
			foreach (Item i in m_Items)
				i.LineStyle.Colour = clrNormalBorder.CurrentColour;
			clrNormalBorder.DisplayCurrentColour = true;
			Updated();
		}

		private void clrHighlightBorder_UserChangedColour(object sender, EventArgs e)
		{
			foreach (Scriptable s in m_Scriptables)
				s.HighlightStyle.LineColour = clrHighlightBorder.CurrentColour;
			clrHighlightBack.DisplayCurrentColour = true;
			Updated();
		}

		private void cmbThicknessNormal_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling) return;
			foreach (Item i in m_Items)
				i.LineStyle.Width = cmbThicknessNormal.SelectedIndex;
			Updated();
		}

		private void cmbThicknessHighlight_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling) return;
			foreach (Scriptable s in m_Scriptables)
				s.HighlightStyle.LineWidth = cmbThicknessHighlight.SelectedIndex;
			Updated();
		}

		private void cmbShape_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling) return;
			foreach (Item i in m_Items)
				i.BorderShape = (Container.Borders)cmbShape.SelectedIndex;
			Updated();
		}

		private void cmbStyle_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (m_Filling) return;
			foreach (Item i in m_Items)
				i.StyleType = (Item.ItemDisplayTypes)cmbStyle.SelectedValue;
			ShowStyleWarning();
			Updated();
		}

		private void ShowStyleWarning()
		{
			if (!Globals.Root.CurrentConfig.ReadBoolean(Config.SAW_EnableItemStyles))
			{
				lblStyleWarning.Visible = true;
				lblStyleWarning.Text = Strings.Item("SAW_Edit_StylesDisabled");
			}
			else if ((Item.ItemDisplayTypes)cmbStyle.SelectedValue != Item.ItemDisplayTypes.IDT_Item
					&& Globals.Root.CurrentConfig.ReadBoolean(Config.SAW_EnableItemStyles))
			{
				lblStyleWarning.Visible = true;
				lblStyleWarning.Text = Strings.Item("SAW_Edit_StyleOverrides");
			}
			else
				lblStyleWarning.Visible = false;
		}


		#endregion

		#region Layout
		private void TextAlign_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RadioButton r = (RadioButton)sender;
			if (r.Checked)
			{
				foreach (Item i in m_Items)
					i.TextAlign = (Item.Alignment)int.Parse(r.Tag.ToString());
				Updated();
			}
		}

		private void GraphicAlign_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RadioButton r = (RadioButton)sender;
			if (r.Checked)
			{
				foreach (Item i in m_Items)
					i.GraphicAlign = (Item.Alignment)int.Parse(r.Tag.ToString());
				Updated();
			}
		}

		private void Alignment_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			RadioButton r = (RadioButton)sender;
			foreach (Item i in m_Items)
			{
				if (r == rdoTextAbove)
					i.Arrangement = ButtonLayouts.Below;
				else if (r == rdoTextBelow)
					i.Arrangement = ButtonLayouts.Above;
				else if (r == rdoTextLeft)
					i.Arrangement = ButtonLayouts.Right;
				else if (r == rdoTextRight)
					i.Arrangement = ButtonLayouts.Left;
				else if (r == rdoTextOverlay)
					i.Arrangement = ButtonLayouts.CentreBoth;
			}
			Updated();
		}

		private void ctrTextRatio_ValueChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			foreach (Item i in m_Items)
				i.TextRatio = chkRatioAutomatic.Checked ? -1 : (float)ctrTextRatio.Value / 100;
			Updated();
		}

		private void chkRatioAutomatic_CheckedChanged(object sender, EventArgs e)
		{
			ctrTextRatio.Enabled = !chkRatioAutomatic.Checked;
			if (m_Filling)
				return;
			foreach (Item i in m_Items)
				i.TextRatio = chkRatioAutomatic.Checked ? -1 : (float)ctrTextRatio.Value / 100;
			Updated();
		}

		#endregion

		#region Preview state

		private readonly Timer tmrPreview = new Timer() { Interval = 1500, Enabled = false };
		private void rdoNormal_CheckedChanged(object sender, EventArgs e)
		{
			if (rdoNormal.Checked)
				SetPreviewState(ButtonShape.States.Normal);
			//if (!m_Filling)
			//	Updated();
		}

		private void rdoHighlight_CheckedChanged(object sender, EventArgs e)
		{
			if (rdoHighlight.Checked)
				SetPreviewState(ButtonShape.States.Highlight);
			//if (!m_Filling)
			//	Updated();
		}

		private void rdoAlternate_CheckedChanged(object sender, EventArgs e)
		{
			tmrPreview.Enabled = rdoAlternate.Checked;
			//if (!m_Filling)
			//	Updated();
		}

		private void m_tmrPreview_Tick(object sender, EventArgs e)
		{
			if (m_Scriptables.First().State == ButtonShape.States.Normal)
				SetPreviewState(ButtonShape.States.Highlight);
			else
				SetPreviewState(ButtonShape.States.Normal);
		}

		private void SetPreviewState(ButtonShape.States state)
		{
			foreach (Scriptable s in m_Scriptables)
			{
				s.State = state;
				s.NotifyIndirectChange(s, ChangeAffects.RepaintNeeded);
			}
		}


		#endregion

		#region Nudge
		private void btnNudgeLeft_Click(object sender, EventArgs e) => Nudge(-1, 0);
		private void btnNudgeRight_Click(object sender, EventArgs e) => Nudge(1, 0);
		private void btnNudgeUp_Click(object sender, EventArgs e) => Nudge(0, -1);
		private void btnNudgeDown_Click(object sender, EventArgs e) => Nudge(0, 1);

		private void Nudge(int X, int Y)
		{
			TransformMove transform = new TransformMove(X, Y);
			foreach (Scriptable s in m_Scriptables)
				s.ApplyTransformation(transform);
			Updated();
		}

		private void btnNudgeNarrower_Click(object sender, EventArgs e) => NudgeSize(-1, 0);
		private void btnNudgeWider_Click(object sender, EventArgs e) => NudgeSize(1, 0);
		private void btnNudgeShorter_Click(object sender, EventArgs e) => NudgeSize(0, -1);
		private void btnNudgeTaller_Click(object sender, EventArgs e) => NudgeSize(0, 1);

		private void NudgeSize(int X, int Y)
		{
			foreach (Item item in m_Items)
			{
				var bounds = item.Bounds;
				if (X > 0 || bounds.Width > 5)
					bounds.Width += X;
				if (Y > 0 || bounds.Height > 5)
					bounds.Height += Y;
				item.SetBounds(bounds, m_Transaction);
			}
			Updated();
		}

		#endregion

	}
}
