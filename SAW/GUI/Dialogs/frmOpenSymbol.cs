using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace SAW
{
	public partial class frmOpenSymbol : Form
	{
		/// <summary>Any text to show in output instead of result images</summary>
		private string m_Message;
		/// <summary>the picture box control which is currently selected, if any.  See also DisplayedImage which returns the record</summary>
		private PictureBox m_Selected;
		/// <summary>Keys of images that we couldn't download.  Maintained until S/W closes </summary>
		private static HashSet<string> m_Failed = new HashSet<string>();
		private static string g_CacheFolder;
		private const int MINCHARS = 2;
		private const int MAXIMAGES = 50;
		private const int MAXCACHE = 20 * 1000 * 1000;// 20mb (ish)

		private frmOpenSymbol(string text)
		{
			InitializeComponent();
			Strings.Translate(this);
			txtText.Text = text;
			DoSearch();

			if (g_CacheFolder == null)
				g_CacheFolder = Path.Combine(Globals.Root.SharedFolder, "OpenSymbolCache");
			if (!Directory.Exists(g_CacheFolder))
				Directory.CreateDirectory(g_CacheFolder);
			EnsureCacheList();

			txtText.GotFocus += (s, e) => AcceptButton = btnSearch;
			Activated += (s, e) => txtText.Focus();
		}

		public static string DoSearch(string text)
		{
			var frm = new frmOpenSymbol(text);
			string result = null;
			if (frm.ShowDialog() == DialogResult.OK)
				result = frm.Selected?.Filename;
			CleanCache(MAXCACHE, result);
			return result;
		}

		#region Async search

		private async void DoSearch()
		{
			SetInfo(null, false);
			ClearImages();
			string text = txtText.Text;
			if (text.Length < MINCHARS)
			{
				m_Message = Strings.Item("SAW_Edit_OS_TooShort").Replace("%0", MINCHARS.ToString());
				return;
			}

			m_Message = Strings.Item("SAW_Edit_OS_Contacting");
			pnlImages.Invalidate();

			try
			{
				// sample URL: https://www.opensymbols.org/api/v1/symbols/search?q=house
				string searchResult = await GetStringAsync(@"https://www.opensymbols.org/api/v1/symbols/search?q=" + Uri.EscapeUriString(text));
				pnlImages.Invalidate();

				var json = JArray.Parse(searchResult);
				if (json.Count == 0)
					m_Message = Strings.Item("SAW_Edit_OS_NoneFound");
				else
				{
					if (json.Count > MAXIMAGES)
						SetInfo(Strings.Item("SAW_Edit_OS_TooMany").Replace("%0", json.Count.ToString()).Replace("%1", MAXIMAGES.ToString()), true);
					int failures = 0;
					foreach (JObject imageJson in json.Take(MAXIMAGES))
					{
						try
						{
							DisplayedImage image = await GetImage(imageJson);
							if (image != null)
							{
								if (m_Message != null) // remove message, if any, once first image is displayed
								{
									m_Message = null;
									pnlImages.Invalidate();
								}
								ShowPicture(image);
							}
						}
						catch (Exception ex)
						{
							Utilities.LogSubError("Image " + (imageJson["symbol_key"]?.ToString() ?? "?") + " failed: " + ex.Message);
							failures += 1;
						}
					}
					if (failures > 0)
						SetInfo(Strings.Item("SAW_Edit_OS_SomeFailed").Replace("%0", failures.ToString()), true);
				}
			}
			catch (Exception ex)
			{
				m_Message = Strings.Item("SAW_Edit_OS_Failed") + ex.Message;
				Utilities.LogSubError(ex);
			}
			pnlImages.Invalidate();
		}

		/// <summary>Adds the given file to the displayed list</summary>
		private void ShowPicture(DisplayedImage image)
		{
			PictureBox ctr = new PictureBox();
			MemoryImage memory = new MemoryImage(image.Filename); // this is used for its SVG handling.  We don't really need the memory buffer overhead!
			ctr.Image = memory.GetNetImage();
			ctr.Tag = image;
			ctr.Size = new Size(100, 100);
			ctr.SizeMode = PictureBoxSizeMode.Zoom;
			ctr.Margin = new Padding(10, 10, 10, 10);
			ctr.Padding = new Padding(10, 10, 10, 10);
			ctr.Click += Image_Click;
			ctr.DoubleClick += Image_DoubleClick;
			pnlImages.Controls.Add(ctr);
		}

		/// <summary>Downloads image - or gets from cache, and returns full file name</summary>
		private async Task<DisplayedImage> GetImage(JObject imageJson)
		{
			string key = imageJson["symbol_key"].ToString();
			if (m_Failed.Contains(key))
				return null;
			string extension = imageJson["extension"].ToString();
			string filename = Path.Combine(g_CacheFolder, key + "." + extension);
			if (!File.Exists(filename))
			{
				try
				{
					string uri = imageJson["image_url"].ToString();
					await GetFileAsync(uri, filename);
					g_CachedFiles.Add(new CachedFile(filename));
				}
				catch (Exception ex)
				{
					Globals.Root.Log.WriteLine("Image " + key + " (" + imageJson["image_url"] + ") failed: " + ex.Message);
					m_Failed.Add(key);
					return null;
				}
			}
			return new DisplayedImage(filename, imageJson["repo_key"]?.ToString(), imageJson["license"]?.ToString(), imageJson["license_url"]?.ToString());
		}

		/// <summary>Reads from HTTP into a string</summary>
		public async Task<string> GetStringAsync(string uri)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			request.Method = "GET";

			using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				return await reader.ReadToEndAsync();
			}
		}

		/// <summary>Reads from HTTP into a file</summary>
		public async Task GetFileAsync(string uri, string writeToFile)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			request.Method = "GET";

			using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
			using (Stream stream = response.GetResponseStream())
			using (FileStream output = new FileStream(writeToFile, FileMode.Create, FileAccess.Write))
			{
				await stream.CopyToAsync(output);
			}
		}

		private class DisplayedImage
		{
			public readonly string Filename;
			public readonly string SymbolSet;
			public readonly string LicenceName;
			public readonly string LicenceURL;

			public DisplayedImage(string filename, string symbolSet, string licenceName, string licenceUrl)
			{
				Filename = filename;
				SymbolSet = symbolSet ?? "";
				LicenceName = licenceName ?? "";
				LicenceURL = licenceUrl ?? "";
			}


		}

		#endregion

		#region Image list - much of this copied from CCF.frmLookup

		private void ClearImages()
		{ // we need to make sure that all images are correctly disposed.  Although the .net garbage collector will eventually tidy them up
		  // SAW failed to open them if they have not been disposed here (probably SAW is requesting higher permissions than needed)
			foreach (PictureBox pct in pnlImages.Controls)
			{
				pct.Image.Dispose();
				pct.Image = null;
			}
			pnlImages.Controls.Clear();
		}

		private void pnlImages_Paint(object sender, PaintEventArgs e)
		{
			StringFormat format = new StringFormat
			{
				LineAlignment = StringAlignment.Center,
				Alignment = StringAlignment.Center
			};
			e.Graphics.DrawString(m_Message, Font, Brushes.Black, new RectangleF(0, 0, pnlImages.Width, pnlImages.Height), format);
		}

		private void Image_Click(object sender, EventArgs e)
		{
			if (m_Selected == sender) return;
			if (m_Selected != null)
				m_Selected.BorderStyle = BorderStyle.None;
			m_Selected = (PictureBox)sender;
			m_Selected.BorderStyle = BorderStyle.Fixed3D;
			btnOK.Enabled = true;
			AcceptButton = btnOK;
			SetInfo(Strings.Item("SAW_Edit_OS_Library", Selected?.SymbolSet ?? "?")
				+ "\r\n"
				+ Strings.Item("SAW_Edit_OS_Licence", Selected?.LicenceName ?? "?")
				, false);
		}

		private void Image_DoubleClick(object sender, EventArgs e)
		{
			// this often isn't being called.  Changing the border style in Image_Click seems to lose the double-click (ie it will be lost unless
			// that item was already selected before the first click).  Can't think of a solution to this
			Image_Click(sender, e);
			DialogResult = DialogResult.OK;
		}

		private void btnSearch_Click(object sender, EventArgs e)
		{
			DoSearch();
		}

		/// <summary>Selected item as a DisplayedImage class, or null </summary>
		private DisplayedImage Selected
		{ get { return m_Selected?.Tag as DisplayedImage; } }

		#endregion

		#region Cache control

		/// <summary>All files in the cache folder.  A list is kept so we have the size and dates without needing to keep going back to windows </summary>
		private static List<CachedFile> g_CachedFiles;

		/// <summary>Filename with size and creation date. Sorts into ascending date order</summary>
		private class CachedFile : IComparable<CachedFile>
		{
			public readonly string Filename;
			public readonly int Size;
			public readonly DateTime CreatedDate;

			public CachedFile(string file)
			{
				Filename = file;
				FileInfo info = new FileInfo(file);
				Size = (int)info.Length;
				CreatedDate = info.CreationTime;
			}

			public int CompareTo(CachedFile other)
			{
				if (ReferenceEquals(this, other)) return 0;
				if (ReferenceEquals(null, other)) return 1;
				return CreatedDate.CompareTo(other.CreatedDate);
			}
		}

		/// <summary>Checks we have the list of cached files in memory, and if not scans the folder.
		/// Called when form opened, but is redundant after first time</summary>
		private static void EnsureCacheList()
		{
			if (g_CachedFiles != null)
				return;
			g_CachedFiles = new List<CachedFile>();
			foreach (string file in Directory.GetFiles(g_CacheFolder))
			{
				g_CachedFiles.Add(new CachedFile(file));
			}
		}

		/// <summary>Reduces cache to given size, deleting oldest first, but never deleting keepFile (which may be null)</summary>
		private static void CleanCache(int maxSize, string keepFile)
		{
			if (g_CachedFiles == null)
				return; // ??!!
			int totalSize = (from file in g_CachedFiles select file.Size).Sum();
			g_CachedFiles.Sort(); // will sort into ascending date
			int index = 0;
			while (totalSize > maxSize && index < g_CachedFiles.Count)
			{
				if (keepFile != null && g_CachedFiles[index].Filename == keepFile)
					index++;
				else
				{
					totalSize -= g_CachedFiles[index].Size;
					File.Delete(g_CachedFiles[index].Filename);
					g_CachedFiles.RemoveAt(index);
				}
			}
		}

		#endregion

		private void lnkOpenSymbols_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Globals.Root.OpenURL(@"https://www.opensymbols.org/", this);
		}

		/// <summary>Or null or empty hides the info box (which obscures the opensymbol link when displayed)</summary>
		private void SetInfo(string info, bool isWarning)
		{
			lblInfo.Text = info ?? "";
			lblInfo.Visible = !string.IsNullOrEmpty(info);
			lblInfo.ForeColor = isWarning ? Color.Red : Color.Black;
		}

	}
}
