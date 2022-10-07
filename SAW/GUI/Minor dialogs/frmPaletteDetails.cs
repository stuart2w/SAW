using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows.Forms;


namespace SAW
{
	internal partial class frmPaletteDetails
	{

		private frmPaletteDetails()
		{
			InitializeComponent();
			Strings.Translate(this); // Must be before the values are assigned

			List<Palette.Purpose> list = new List<Palette.Purpose>();
			// we filter out a few purposes which aren't appropriate for user defined palettes
			foreach (Palette.Purpose purpose in Palette.Purpose.PossiblePurposes)
			{
				switch (purpose.Special)
				{
					case Palette.Purpose.Specials.Sockets:
					default:
						list.Add(purpose);
						break;
				}
			}
			cmbPurpose.DataSource = list;
			cmbPurpose.ValueMember = "Parameter"; // or "Special" - they are interchangeable
			cmbPurpose.DisplayMember = "Name";
		}

		public void frmPaletteDetails_Load(object sender, EventArgs e)
		{
			GUIUtilities.ScaleDPI(this);
		}

		/// <summary>Updates the parameters on OK</summary>
		/// <param name="purpose"></param>
		/// <param name="createFlow">Should be true on entry if creating; histrionics it if the box was ticked</param>
		/// <param name="isDuplicate">Should be true if we are duplicating an existing palette; disallows changing of the flow option, and updates the UI</param>
		/// <param name="title"></param>
		/// <param name="description"></param>
		/// <param name="subtitle"></param>
		public static DialogResult Display(ref string title, ref string description, ref string subtitle, ref Palette.Purpose purpose, ref bool createFlow, bool isDuplicate)
		{
			frmPaletteDetails form = new frmPaletteDetails();
			form.txtTitle.Text = title;
			form.txtDescription.Text = description;
			form.txtSubTitle.Text = subtitle;
			form.cmbPurpose.SelectedValue = purpose.Parameter;
			Debug.Assert(!createFlow || !isDuplicate); // partly because the 2 controls are on top of each other.  But also it makes no sense for both of these to be true
			form.chkFlow.Visible = createFlow;
			form.lblDuplicate.Visible = isDuplicate;
			DialogResult result = form.ShowDialog();
			if (result == DialogResult.OK)
			{
				purpose = new Palette.Purpose((int)form.cmbPurpose.SelectedValue);
				title = form.txtTitle.Text;
				description = form.txtDescription.Text;
				subtitle = form.txtSubTitle.Text;
				createFlow = form.chkFlow.Checked;
			}
			return result;
		}

		public void txtName_TextChanged(object sender, EventArgs e)
		{
			btnOK.Enabled = !string.IsNullOrEmpty(txtTitle.Text ) && !string.IsNullOrEmpty(txtDescription.Text ) && cmbPurpose.SelectedIndex >= 0;
		}

	}
}
