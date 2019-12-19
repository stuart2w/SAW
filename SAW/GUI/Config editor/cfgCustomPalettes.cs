using System.Collections.Generic;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;


namespace SAW
{
	public partial class cfgCustomPalettes
	{

		// This is the custom palettes section; see Features for the section which shows and hides palettes

		private List<Document> m_DisplayedPalettes = new List<Document>();

		public cfgCustomPalettes()
		{
			InitializeComponent();
		}

		public override void OnDisplay()
		{
			base.OnDisplay();
			m_Filling = true;
			bool hasMultiDocument = Config.UserEditor.ReadBooleanEx(Config.Multiple_Documents);
			// not necessary to check as an applied config since it can only be changed in this one place
			bool userOnly = Level == Config.Levels.ActivityUser || Level == Config.Levels.DocumentUser;

			lblPalettesHeading.Visible = !userOnly;
			lstPalettes.Visible = !userOnly;
			btnCreate.Visible = !userOnly;
			btnDelete.Visible = !userOnly;
			btnEdit.Visible = !userOnly;
			btnRename.Visible = !userOnly;
			btnDuplicate.Visible = !userOnly;
			lblPalettesTabsRequired.Visible = !hasMultiDocument;
			lnkPalettesTabsChange.Visible = !hasMultiDocument;
			btnCreate.Enabled = hasMultiDocument && !userOnly;
			chkShowSystem.Visible = Level != Config.Levels.System;

			lstPalettes.Items.Clear();
			m_DisplayedPalettes.Clear();
			if (!chkShowSystem.Checked || Level == Config.Levels.System)
				m_DisplayedPalettes.AddRange(m_Config.CustomPalettes.Values.Cast<Document>());
			else
				m_DisplayedPalettes.AddRange(m_Applied.CustomPalettes().Values);
			m_DisplayedPalettes.Sort(new Document.PaletteNameSort());
			foreach (Document palette in m_DisplayedPalettes)
			{
				lstPalettes.Items.Add("\"" + palette.PaletteEditingFullDescription + (Strings.IsTranslationMode ? "=" + palette.ID : "") + "\" -- " + palette.PalettePurpose.Name);
			}
			lstPalettes_SelectedIndexChanged(this, null);
			m_Filling = false;
		}

		public void lnkPalettesTabsChange_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_Transaction.Edit(Config.UserEditor);
			Config.UserEditor.Write(Config.Multiple_Documents, true.ToString());
			OnDisplay();
		}

		public void lstPalettes_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool hasMultiDocument = Config.UserEditor.ReadBooleanEx(Config.Multiple_Documents);
			if (!(Globals.Root.CurrentMainScreen() is frmMain))
				hasMultiDocument = false;
			// Check if the selected palette is actually from this configuration
			var external = lstPalettes.SelectedIndex >= 0 && !m_Config.CustomPalettes.ContainsValue(m_DisplayedPalettes[lstPalettes.SelectedIndex]);
			btnCreate.Enabled = true;
			btnEdit.Enabled = lstPalettes.SelectedIndex >= 0 && hasMultiDocument && !external;
			btnDuplicate.Enabled = lstPalettes.SelectedIndex >= 0 && hasMultiDocument;
			btnDelete.Enabled = lstPalettes.SelectedIndex >= 0 && !external;
			btnRename.Enabled = lstPalettes.SelectedIndex >= 0 && !external;
#if !DEBUG
			// Users cannot delete the standard ones stored in the system; if they cannot delete here, we need to stop them creating here as well
			if (Level == Config.Levels.System)
			{
				btnDelete.Enabled = false;
				btnCreate.Enabled = false;
			}
#endif
		}

		public void btnPaletteCreate_Click(object sender, EventArgs e)
		{
			string name = "";
			string description = "";
			string subTitle = "";
			Palette.Purpose purpose = Palette.Purpose.Custom;
			bool isFlow = true;
			if (frmPaletteDetails.Display(ref name, ref description, ref subTitle, ref purpose, ref isFlow, false) != System.Windows.Forms.DialogResult.OK)
				return;
			string display = Strings.Translate(description) + (!string.IsNullOrEmpty(subTitle) ? " (" + Strings.Translate(subTitle) + ")" : "");
			if (m_DisplayedPalettes.Any(d => d.PaletteEditingFullDescription == display))
			{
				MessageBox.Show(Strings.Item("Palette_DescriptionDuplicate"));
				return;
			}
			Document document = new Document(false)
			{
				ActivityID = Activities.PaletteID,
				PaletteWithin = m_Config,
				PaletteTitle = name,
				PaletteDescription = description,
				PalettePurpose = purpose,
				SnapMode = Shape.SnapModes.Grid,
				SubTitle = subTitle
			};
			document.PaletteDesignSize = new SizeF(60, 30);
			document.Page(0).SetSize(document.PaletteDesignSize, 0);
			document.Page(0).Paper = Paper.CreateNew(Paper.Papers.Graph, document.Page(0).Paper);
			document.Page(0).Paper.SetIntervals(1, 1, 10);
			if (isFlow)
			{
				Flow flow = new Flow() { Direction = Flow.Directions.RightThenDown, ShapeSeparation = 1 };
				flow.LineStyle.SetDefaults();
				flow.FillStyle.SetDefaults();
				flow.LineStyle.Colour = Color.Empty;
				flow.FillStyle.Colour = System.Drawing.Color.Empty;
				flow.SetBounds(document.Page(0).Bounds);
				document.Page(0).AddNew(flow, null);
			}

			m_Config.CustomPalettes.Add(document);
			Palette objRegistration = new Palette(document);
			if (document.PalettePurpose.IsCustom) // ensure new custom one shown
				m_Config.Write(Config.ShowPaletteKey(objRegistration), true.ToString());
			WrittenToCurrent();
			Palette.Register(objRegistration, null);
			Form.CloseAndEditPalette(document);
		}

		public void btnEdit_Click(object sender, EventArgs e)
		{
			if (lstPalettes.SelectedIndex < 0)
				return;
			Form.CloseAndEditPalette(m_DisplayedPalettes[lstPalettes.SelectedIndex]);
		}

		public void btnDuplicate_Click(object sender, System.EventArgs e)
		{
			if (lstPalettes.SelectedIndex < 0)
				return;
			Document original = m_DisplayedPalettes[lstPalettes.SelectedIndex];
			Mapping mapping = new Mapping();
			Document newDocument = (Document)original.Clone(mapping); // There is no real need to transact the new palette; if the reference to it is removed it will be discarded from memory
			newDocument.UpdateReferencesIDsChanged(mapping, newDocument);
			newDocument.PaletteWithin = m_Config;

			string title = newDocument.PaletteTitle;
			string description = newDocument.PaletteDescription;
			var purpose = newDocument.PalettePurpose;
			bool autosize = newDocument.Page(0).IsSingleAutoSize;
			if (frmPaletteDetails.Display(ref title, ref description, ref newDocument.SubTitle, ref purpose, ref autosize, true) != System.Windows.Forms.DialogResult.OK)
				return;
			newDocument.PaletteTitle = title;
			newDocument.PaletteDescription = description;
			newDocument.PalettePurpose = purpose;

			m_Config.CustomPalettes.Add(newDocument);
			// We also need to make sure that this palette becomes visible
			if (newDocument.PalettePurpose.IsCustom)
				m_Config.Write("Show_Palette_" + newDocument.ID, true.ToString());
			else
			{
				m_Config.Write("Show_Palette_" + Config.PaletteKeyFragment(newDocument.PalettePurpose), true.ToString());
				m_Config.Write(Config.SelectPaletteKey(newDocument.PalettePurpose), newDocument.ID.ToString());
			}
			Form.CloseAndEditPalette(newDocument);
		}

		public void btnPaletteRename_Click(object sender, EventArgs e)
		{
			Document palette = m_DisplayedPalettes[lstPalettes.SelectedIndex];
			Palette.Purpose oldPurpose = palette.PalettePurpose;
			string title = palette.PaletteTitle;
			string description = palette.PaletteDescription;
			bool temp = false;
			var purpose = palette.PalettePurpose;
			if (frmPaletteDetails.Display(ref title, ref description, ref palette.SubTitle, ref purpose, ref temp, false) == DialogResult.OK)
			{
				palette.PaletteTitle = title;
				palette.PaletteDescription = description;
				if (!palette.PalettePurpose.Equals(oldPurpose))
					Form.m_PalettePurposeChanged = true;
				OnDisplay();
			}
		}

		public void btnPaletteDelete_Click(object sender, EventArgs e)
		{
			Document selected = m_DisplayedPalettes[lstPalettes.SelectedIndex];
			if (Globals.Root.DocumentIsOpen(selected))
			{
				MessageBox.Show(Strings.Item("Config_PalettesDeleteEditingDocument"));
				return;
			}
			if (MessageBox.Show(Strings.Item("Config_PalettesDeleteConfirm"), RootApplication.AppName, MessageBoxButtons.YesNo) != DialogResult.Yes)
				return;
			// The user is warned that the palette won't be restored to the definition list
			m_Config.CustomPalettes.Remove(selected);
			m_Transaction.Delete(selected);
			WrittenToCurrent();
			OnDisplay();
			Palette.Deregister(selected.ID.ToString());
		}

		public void chkShowSystem_CheckedChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			OnDisplay();
		}
	}

}
