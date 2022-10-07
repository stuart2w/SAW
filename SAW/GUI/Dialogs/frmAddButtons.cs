using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using SAW.Functions;
using SAW.Shapes;

namespace SAW
{
	/// <summary>Adds one or more buttons to custom palette.  Contians an action selector and add button.  Adding does not close the form</summary>
	internal partial class frmAddButtons
	{

		private readonly Page m_Page;
		/// <summary>the key combination pressed if adding a key action </summary>
		private Keys m_eKey;
		/// <summary>the matching character from the KeyPress </summary>
		private char m_Key;

		private frmAddButtons(Page page)
		{
			m_Page = page;
			InitializeComponent();
			ctrActions.Fill(new AppliedConfig(Config.SystemConfig));
		}

		public void frmAddButtons_Load(object sender, EventArgs e)
		{
			GUIUtilities.ScaleDPI(this);
			Strings.Translate(this);
		}

		public static void Display(Page page)
		{
			frmAddButtons frm = new frmAddButtons(page);
			frm.ShowDialog();
		}

		public void btnAdd_Click(object sender, EventArgs e)
		{
			Transaction transaction = new Transaction();
			List<ButtonShape> buttons = m_Page.OfType<ButtonShape>().ToList();
			PointF pt;
			SizeF sz;
			// Button is positioned to the bottom right of the existing ones; position will actually be ignored if there is a flow
			buttons.Sort((X, Y) =>
			{
				// Sorts into an effectively reading order, but backwards.  First entry will be bottom right
				float result = -(X.Bounds.Bottom - Y.Bounds.Bottom);
				if (Math.Abs(result) < Geometry.NEGLIGIBLE)
					result = -(X.Bounds.Right - Y.Bounds.Right);
				return Math.Sign(result);
			});
			if (buttons.Any())
			{
				sz = new SizeF(buttons.Average(x => x.Bounds.Width), buttons.Average(x => x.Bounds.Height));
				pt = buttons.First().Bounds.TopRight() + new SizeF(1, 0);
				if (pt.X + sz.Width > m_Page.Bounds.Width) // Has gone off the end of the line, start a new line
					pt = new PointF(0, pt.Y + sz.Height + 1);
				if (pt.Y + sz.Height * 0.6 > m_Page.Size.Height) // off the bottom - Just resets to the top left in this case
					pt = new PointF(0, -m_Page.Size.Height);
			}
			else
			{
				sz = new SizeF(9, 9);
				pt = new PointF(0, -m_Page.Size.Height);
			}
			ButtonShape create = new ButtonShape();
			transaction.Create(create);
			ClickPosition dummyPosition = new ClickPosition(pt, m_Page, 1, Shape.SnapModes.Off, Shape.SnapModes.Off, null, ClickPosition.Sources.Irrelevant);
			create.Start(dummyPosition);
			create.SetStyleObject(frmButton.GetStyleForNewButton(m_Page));
			dummyPosition.Exact = dummyPosition.Exact + sz;
			dummyPosition.Snapped = dummyPosition.Exact;
			create.Complete(dummyPosition);
			create.Action = ctrActions.SelectedAction;
			switch (create.Action.Change)
			{
				case Parameters.Action_Character:
					create.Action = new CharAction(m_Key.ToString());
					break;
				case Parameters.Action_Key:
					create.Action = new KeyAction(m_eKey);
					break;
				case Parameters.Action_Text:
					if (string.IsNullOrEmpty((create.Action as TextAction)?.Text)) // might already be specified by list
						create.Action = new TextAction(txtText.Text);
					break;
			}
			SharedImage image;
			string temp;
			ButtonShape.GetDisplayFromAction(create.Action, out temp, out image, transaction);
			create.LabelText = temp;
			if (image != null)
				create.SetImage(image);
			if (m_Page.IsSingleAutoSize)
			{
				IShapeContainer container = (IShapeContainer)m_Page.Shapes.First();
				transaction.Edit((Datum)container);
				container.Contents.Add(create);
				create.Parent = (IShapeParent)container;
				container.FinishedModifyingContents(transaction, null);
				if (create.Bounds.Bottom > m_Page.Bounds.Bottom) // Is off the bottom
				{
					transaction.Edit(m_Page);
					m_Page.SetSizeExcludingMargin(new SizeF(m_Page.Size.Width, pt.Y + sz.Height - m_Page.Bounds.Top));
					((IAutoSize)container).SetBounds(m_Page.Bounds, null);
					Globals.OnCurrentPageSizeChanged();
				}
			}
			else
			{
				m_Page.AddNew(create, transaction);
				if (pt.Y > m_Page.Bounds.Bottom - 1) // Is off the bottom
				{
					transaction.Edit(m_Page);
					float height = pt.Y + sz.Height + m_Page.Margin * 2 - m_Page.Bounds.Top;
					TransformMove delta = new TransformMove(0, -(height - m_Page.PhysicalSize.Height)); // All existing shapes will need to be moved up by this much because the top coordinate of the page will change
					m_Page.SetSize(new SizeF(m_Page.PhysicalSize.Width, height), m_Page.Margin);
					foreach (Shape shp in m_Page)
					{
						transaction.Edit(shp);
						shp.ApplyTransformation(delta);
					}
					Globals.OnCurrentPageSizeChanged();
				}
			}
			Globals.Root.StoreNewTransaction(transaction, true);
		}

		public void ctrActions_AfterSelect(object sender, EventArgs e)
		{
			btnAdd.Enabled = !ctrActions.SelectedAction.IsEmpty;
			if (ctrActions.SelectedAction.Change == Parameters.Action_Key || ctrActions.SelectedAction.Change == Parameters.Action_Character)
			{
				pnlKey.Visible = true;
				lblKey.Text = Strings.Item("Button_AddButtons_Press");
				if (m_eKey == Keys.None)
					btnAdd.Enabled = false;
				txtKey.Visible = true;
				txtText.Visible = false;
			}
			else if (ctrActions.SelectedAction.Change == Parameters.Action_Text && string.IsNullOrEmpty((ctrActions.SelectedAction as TextAction).Text))
			{
				// only need to type the text if it doesn't specify one (some equation entries specify text already)
				pnlKey.Visible = true;
				lblKey.Text = Strings.Item("Button_AddButtons_Text");
				if (string.IsNullOrEmpty(txtText.Text))
					btnAdd.Enabled = false;
				txtText.Visible = true;
				txtKey.Visible = false;
			}
			else
				pnlKey.Visible = false;
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
					m_eKey = e.KeyData;
					CombinedKeyEvent eCombined = new CombinedKeyEvent(e); // on Mac this will modify the key as needed
					txtKey.Text = GUIUtilities.KeyDescription(eCombined.KeyData);
					break;
			}
			e.Handled = true;
			e.SuppressKeyPress = false;
			if (ctrActions.SelectedAction.Change == Parameters.Action_Key || ctrActions.SelectedAction.Change == Parameters.Action_Character)
				btnAdd.Enabled = true;
		}

		public void txtText_TextChanged(object sender, EventArgs e)
		{
			if (ctrActions.SelectedAction.Change == Parameters.Action_Text)
				btnAdd.Enabled = !string.IsNullOrEmpty(txtText.Text);
		}

		public void txtKey_KeyPress(object sender, KeyPressEventArgs e)
		{
			m_Key = e.KeyChar;
			e.Handled = true;
			if (ctrActions.SelectedAction.Change == Parameters.Action_Character)
				// was already set to key description in KeyDown - but for type character should be the text instead
				txtKey.Text = m_Key.ToString();
		}

		public void ctrActions_DoubleClick(object sender, EventArgs e)
		{
			if (btnAdd.Enabled)
				btnAdd_Click(sender, e);
		}

		public void frmAddButtons_KeyDown(object sender, KeyEventArgs e)
		{
			// otherwise enter does OK
			if (e.KeyCode == Keys.Enter && txtKey.Focused)
				txtKey_KeyDown(sender, e);
			else if (btnAdd.Enabled)
				btnAdd_Click(sender, EventArgs.Empty);
		}

	}
}
