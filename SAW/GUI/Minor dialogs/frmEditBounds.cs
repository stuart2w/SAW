using SAW.Functions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SAW
{
	public partial class frmEditBounds : Form
	{

		private Rectangle m_OriginalValue;
		private bool m_ARLocked;
		private float m_OriginalAR; // actually we should use the AR of the image to avoid cumulative rounding errors, but I don't think it will make a noticeable difference

		private frmEditBounds(Rectangle rct, bool lockAR = false)
		{
			InitializeComponent();
			Strings.Translate(this);
			Value = rct;
			m_OriginalValue = rct;
			m_ARLocked = lockAR;
			Owner = Globals.Root.Editor;
			chkAdjustContents.Enabled = !lockAR;
			txtHeight.Enabled = !lockAR;
			if (lockAR)
				chkAdjustContents.Checked = true;
			lblARlocked.Visible = lockAR;
			m_OriginalAR = (float)Math.Max(rct.Width, 1) / Math.Max(rct.Height, 1); // the .Max avoid any awkward zeroes or infinities which could throw exceptions
		}

		public Rectangle Value
		{
			get { return new Rectangle(txtX.Value, txtY.Value, txtWidth.Value, txtHeight.Value); }
			set
			{
				txtX.Value = value.X;
				txtY.Value = value.Y;
				txtWidth.Value = value.Width;
				txtHeight.Value = value.Height;
			}
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		public static DialogResult EditItemBounds(Item item)
		{
			Rectangle rct = item.Bounds.ToRectangle();
			Page page = Globals.Root.CurrentPage;
			rct.Y = (int)(rct.Y - page.Bounds.Top); // Y origin at top of page
			frmEditBounds frm = new frmEditBounds(rct);
			frm.Text = Strings.Item("Verb_EditItemBounds");
			frm.lblOuterHeader.Text = Strings.Item("SAW_ItemBounds_Set");
			frm.txtOuterX.Visible = false;
			frm.txtOuterY.Visible = false;
			frm.txtOuterWidth.Text = page.Bounds.Width.ToString("0");
			frm.txtOuterHeight.Text = page.Bounds.Height.ToString("0");
			frm.chkAdjustContents.Visible = item.Contents.Any();
			DialogResult result = frm.ShowDialog();
			if (result == DialogResult.OK)
			{
				rct = frm.Value;
				rct.Y = (int)(rct.Y + page.Bounds.Top);
				Transaction transaction = new Transaction();
				if (item.Parent is Scriptable scriptable)
					transaction.Edit(scriptable);
				transaction.Edit(item);
				item.SetBounds(rct.ToRectangleF(), transaction);
				if (frm.chkAdjustContents.Checked)
					TransformContents(item.Contents, transaction, false, frm.Value, frm.m_OriginalValue);
				Globals.Root.StoreNewTransaction(transaction, true);
				Globals.Root.CurrentPage.SelectOnly(item);
			}
			return result;
		}

		public static DialogResult EditWindowBounds()
		{
			Document document = Globals.Root.CurrentDocument;
			Rectangle rect = document.IsPaletteWithin ? document.Page(0).Bounds.ToRectangle() : document.SAWHeader.MainWindowBounds;
			Screen screen = Screen.FromPoint(rect.Centre());
			frmEditBounds frm = new frmEditBounds(rect, Globals.Root.CurrentPage.LockARToBackgroundImage);
			frm.Text = Strings.Item("Verb_EditWindowBounds");
			frm.lblOuterHeader.Text = Strings.Item("SAW_ItemBounds_Screen");
			frm.txtOuterX.Text = screen.Bounds.X.ToString();
			frm.txtOuterY.Text = screen.Bounds.Y.ToString();
			frm.txtOuterWidth.Text = screen.Bounds.Width.ToString();
			frm.txtOuterHeight.Text = screen.Bounds.Height.ToString();
			DialogResult result = frm.ShowDialog();
			if (result == DialogResult.OK)
			{
				Globals.Root.StoreNewTransaction(SetBoundsInData(frm.Value, frm.chkAdjustContents.Checked), true);
				Globals.Root.Editor.MatchWindowToSet();
			}
			return result;
		}

		/// <summary>Apply a change of set bounds to data, either due to this form or for some other reason.
		/// Returns the transaction with the changes, which is not yet stored</summary>
		public static Transaction SetBoundsInData(Rectangle newBounds, bool adjustContents)
		{
			Document document = Globals.Root.CurrentDocument;
			Rectangle old = document.SAWHeader.MainWindowBounds;
			Transaction transaction = new Transaction { IsPageSizeChange = true };
			transaction.Edit(document);
			document.SAWHeader.SetWindowBounds(newBounds);
			if (newBounds.Size != old.Size)
			{
				foreach (Page p in document.Pages)
				{
					transaction.Edit(p);
					old = p.Bounds.ToRectangle();
					Rectangle pageBounds = newBounds;
					if (p.LockARToBackgroundImage)
					{ // has image and background mode is set to match AR (to keep things aligned)
					  //Size imageSize = p.BackgroundImage.GetSize();
					  //float neededAR = (float)imageSize.Height/imageSize.Width;
					  //pageBounds.Width = (int)Math.Sqrt(pageBounds.Width * pageBounds.Height / neededAR); // formula will always give ratio X:Y = neededAR
					  //pageBounds.Height = (int)Math.Sqrt(pageBounds.Width * pageBounds.Height * neededAR);
						Size newSize = pageBounds.Size;
						GUIUtilities.CalcDestSize(p.BackgroundImage.GetSize(), ref newSize);
						pageBounds.Size = newSize;
					}
					p.SetSize(pageBounds.Size, p.Margin);
					if (adjustContents)
						TransformContents(p.Contents, transaction, true, pageBounds, old);
					else
						TranslateYContents(p.Contents, transaction, -pageBounds.Height + old.Height);
					p.NotifyIndirectChange(null, ChangeAffects.RepaintNeeded,
						new RectangleF(-10000, -10000, 20000, 20000)); // forces a complete repaint, including any non-page areas (current non-page might include bits which were previously page)
				}
			}
			return transaction;
		}

		private static void TransformContents(List<Shape> contents, Transaction transaction, bool changingDocument, Rectangle newRect, Rectangle oldRect)
		{
			System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
			if (!changingDocument)
			{
				matrix.Translate(-oldRect.X, -oldRect.Y, System.Drawing.Drawing2D.MatrixOrder.Append);
				matrix.Scale((float)newRect.Width / oldRect.Width, (float)newRect.Height / oldRect.Height, System.Drawing.Drawing2D.MatrixOrder.Append);
				matrix.Translate(newRect.X, newRect.Y, System.Drawing.Drawing2D.MatrixOrder.Append);
			}
			else
				matrix.Scale((float)newRect.Width / oldRect.Width, (float)newRect.Height / oldRect.Height, System.Drawing.Drawing2D.MatrixOrder.Append);
			Transformation transform = new TransformAffine(Transformation.Modes.Move, matrix);
			foreach (Shape shape in contents)
			{
				transaction.Edit(shape);
				shape.ApplyTransformation(transform);
			}
		}


		/// <summary>Just moves all contents in Y coordinate</summary>
		private static void TranslateYContents(List<Shape> contents, Transaction transaction, float Yoffset)
		{
			//Debug.WriteLine("Translate: " + Yoffset);
			System.Drawing.Drawing2D.Matrix matrix = new System.Drawing.Drawing2D.Matrix();
			matrix.Translate(0, Yoffset, System.Drawing.Drawing2D.MatrixOrder.Append);
			Transformation transform = new TransformAffine(Transformation.Modes.Move, matrix);
			foreach (Shape shape in contents)
			{
				transaction.Edit(shape);
				shape.ApplyTransformation(transform);
			}
		}

		private void txtWidth_TextChanged(object sender, EventArgs e)
		{
			if (!m_ARLocked)
				return;
			txtHeight.Value = (int)(txtWidth.Value / m_OriginalAR);
		}

	}
}
