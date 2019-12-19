using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

namespace SAW
{
	public partial class frmTexture
	{

		private SharedImage m_Chosen;
		private readonly Document m_Document; // document being edited

		public frmTexture(DatumList systemTextures, Document document)
		{
			InitializeComponent();
			m_Document = document;
			Strings.Translate(this);
			FillList(lstSystem, systemTextures);
			// can't just use objDocument.SharedBitmapsList because that is all sorts of imported images
			DatumList colUsed = document.TexturesUsed();
			FillList(lstDocument, colUsed);
			if (colUsed.Count == 0)
				tpDocument.Dispose();
		}

		private void FillList(ListView lst, DatumList dataList)
		{
			try
			{
				foreach (SharedImage texture in dataList.Values)
				{
					lst.Items.Add(new ListViewItem("") { Tag = texture });
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private static DatumList g_SystemTextures;
		public static SharedImage PickTexture(Document document)
		{
			// returns nothing if cancelled
			if (g_SystemTextures == null)
			{
				try
				{
					g_SystemTextures = new DatumList();
					if (System.IO.File.Exists(Globals.Root.InternalFolder + System.IO.Path.DirectorySeparatorChar + "textures"))
					{
						using (DataReader reader = new DataReader(Globals.Root.InternalFolder + System.IO.Path.DirectorySeparatorChar + "textures", FileMarkers.SharedBitmap))
						{
							g_SystemTextures.Add(reader.ReadDataList(FileMarkers.SharedBitmap));
						}

					}
				}
				catch (Exception ex)
				{
					Utilities.LogSubError("Failed to load textures: " + ex.Message);
					g_SystemTextures = new DatumList();
				}
			}

			frmTexture frm = new frmTexture(g_SystemTextures, document);
			if (frm.ShowDialog() != DialogResult.OK)
				return null;
			return frm.m_Chosen;
		}

		public void lstTextures_Click(object sender, EventArgs e)
		{
			ListView listView = (ListView)sender;
			if (listView.SelectedItems.Count == 1)
			{
				m_Chosen = (SharedImage)listView.SelectedItems[0].Tag;
				this.DialogResult = DialogResult.OK;
			}
		}

		public void btnLoad_Click(object sender, EventArgs e)
		{
			string filename = FileDialog.ShowOpen(FileDialog.Context.Image, "[Filter_Image1]");
			if (string.IsNullOrEmpty(filename))
				return;
			Transaction transaction = new Transaction();
			SharedImage texture = (SharedImage)m_Document.AddSharedResourceFromFile(filename, transaction);
			Globals.Root.StoreNewTransaction(transaction);
			if (texture != null)
			{
				m_Chosen = texture;
				this.DialogResult = DialogResult.OK;
			}
		}

		public void lstDocument_GotFocus(object sender, EventArgs e)
		{
			if (lstDocument.SelectedIndices.Count == 0 && lstDocument.Items.Count > 0)
				lstDocument.SelectedIndices.Add(0);
		}

		public void lstSystem_GotFocus(object sender, EventArgs e)
		{
			if (lstSystem.SelectedIndices.Count == 0 && lstSystem.Items.Count > 0)
				lstSystem.SelectedIndices.Add(0);
		}

		public void lstDocument_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
				lstTextures_Click(sender, null);
		}

		public void lstSystem_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
				lstTextures_Click(sender, null);
		}

		public void lstDocument_DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			SharedImage texture = (SharedImage)e.Item.Tag;
			var rct = e.Bounds;
			rct.Inflate(-6, -6); // area to draw image in
			GUIUtilities.CalcDestRect(texture.GetSize(rct.Width), ref rct);
			texture.MemoryImage.Draw(e.Graphics, rct);
			Debug.WriteLine(e.State.ToString());
			if ((e.State & ListViewItemStates.Focused) > 0)
			{
				rct = e.Bounds;
				rct.Inflate(-3, -3);
				using (Pen pn = new Pen(Color.Orange, 3))
				{
					e.Graphics.DrawRectangle(pn, rct);
				}
			}
		}

	}
}
