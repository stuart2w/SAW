using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace SAW.CCF
{
	internal partial class frmLookup : Form
	{
		#region Fields
		/// <summary>only the most recent is remembered </summary>
		private QueryFromWord m_Query;
		/// <summary>the picture box control which is currently selected, if any </summary>
		private PictureBox m_Selected;
		private bool m_Failed;
		/// <summary>the text to display in the search results area, if any</summary>
		private string m_Message = "";

		#endregion

		/// <summary>shows the form and makes the request</summary>
		/// <param name="text">The DISPLAY text of the item</param>
		/// <param name="concept">The stored concept ID of the item, updated by this call</param>
		/// <param name="updateText">Returns a non-empty string if the text is also to be updated</param>
		/// <returns>The relative path of the image to use, or an empty string</returns>
		public static string DoLookup(string text, ref string concept, out string updateText)
		{
				updateText = "";
				Debug.WriteLine("Concept in = " + concept);
				frmLookup frm = new frmLookup(text);
				if (frm.ShowDialog() != DialogResult.OK)
					return "";
				concept = frm.SelectedConceptID;
				updateText = frm.SelectedUpdateWordForm;
				Debug.WriteLine("Concept out = " + concept);
				string source = frm.SelectedFile;
				Debug.WriteLine("File selected: " + source);
				return source.Replace("/", "\\"); // the / works in Windows, but confuses SAW which manipulates image paths
			{ }
		}

		#region Initialisation
		internal frmLookup(string text)
		{
			try
			{
				InitializeComponent();
				Strings.Translate(this);

				SelectedLanguage = Server.Language2;
				// connect...
				m_Message = Strings.Item("CCF_Registering"); // will be changed below by MakeRequest, if already registered
				Connection.EnsureConnection();
				Connection.SharedConnection.QueryReply += SharedConnection_QueryReply; // new EventHandler<CCFConnection.QueryReplyEvent>(m_objConnection_QueryReply);
				this.FormClosing += CCFLookup_FormClosing;
				// if the connection is already present and connected, we can send the request immediately, otherwise wait until the registration notification
				txtText.Text = text;
				if (!Connection.SharedConnection.Ready)
					Connection.SharedConnection.Registered += SharedConnection_Registered; // new EventHandler(m_objConnection_Registered);
				else
					MakeRequest();
			}
			catch (System.Net.Sockets.SocketException) // this seems to be the error that occurs if CCF isn't present
			{
				m_Failed = true;
				MessageBox.Show(Strings.Item("CCF_SocketError"));
			}
			catch (Exception ex)
			{
				m_Failed = true;
				MessageBox.Show(ex.ToString());
			}
		}

		private void CCFLookup_FormClosing(object sender, FormClosingEventArgs e)
		{
			// must remove handlers from global object - otherwise all forms will be kept in memory and will all be notified
			Connection.SharedConnection.QueryReply -= SharedConnection_QueryReply;
			Connection.SharedConnection.Registered -= SharedConnection_Registered; // doesn't matter if wasn't attached.  Removing non-attached handlers is ignored
			ClearImages();
		}


		private void MakeRequest()
		{
			m_Query = new QueryFromWord(txtText.Text);
#if DEBUG
			if (txtText.Text == "")
				throw new Exception("Sending empty request");
#endif
			m_Message = Strings.Item("CCF_Waiting");
			m_Query.LanguageIn = SelectedLanguage;
			m_Query.LanguageOut = "aras,bliss," + SelectedLanguage; // to get word forms
			Connection.SharedConnection.SendQuery(m_Query);
			m_Selected = null;
			ClearImages();
			pnlImages.Invalidate(); // in case it contains no controls, we still need to force it to redraw
			btnOK.Enabled = false;
		}

		private string SelectedLanguage
		{  // first 2 characters are language code.  List prefilled with the ones used in CCF, in form "code - name".  Only first 2 chars used
			get
			{
				if (cmbLanguage.Text.Length < 2) return "en";
				return cmbLanguage.Text.Substring(0, 2);
			}
			set
			{
				if (value.Length < 2) return;
				for (int i = 0; i < cmbLanguage.Items.Count - 1; i++)
					if (cmbLanguage.Items[i].ToString().Substring(0, 2) == value)
					{
						cmbLanguage.SelectedIndex = i;
						return;
					}
				cmbLanguage.Text = value;
			}
		}

		#endregion

		#region Connection events and receive timer

		private void tmrReceive_Tick(object sender, EventArgs e)
		{
			if (m_Failed)
			{ // true if the form failed to construct properly; an error message should already have been reported
				DialogResult = DialogResult.Cancel;
				return;
			}
			if (Connection.SharedConnection == null) return;
			try
			{
				Connection.SharedConnection.DoReceive();
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{ MessageBox.Show(ex.ToString()); }
		}

		void SharedConnection_Registered(object sender, EventArgs e)
		{
			Debug.WriteLine("SharedConnection_Registered");
			MakeRequest();
		}

		void SharedConnection_QueryReply(object sender, Connection.QueryReplyEvent e)
		{
			Debug.WriteLine("SharedConnection_QueryReply is expected reply: " + (e.Query == m_Query).ToString());
			if (e.Query == m_Query)
			{
				m_Message = "";
				pnlImages.Invalidate();
				Response response = e.Response;
				pnlImages.SuspendLayout();
				cmbUpdateWord.Items.Clear();
				cmbUpdateWord.Text = "";
				try
				{
					foreach (Response.Representation representation in response.Representations)
					{
						if (representation.Language == SelectedLanguage)
						{ // this gives us the word forms; not a picture.  Probably only one of these, but will work for more
							string[] words = representation.Repr.Split(',');
							foreach (string word in words)
								cmbUpdateWord.Items.Add(word.Trim());
						}
						else
						{
							PictureBox ctr = new PictureBox();
							string file = Connection.LocalPath + representation.Repr.Replace(Connection.URLReplace, ""); // NB doesn't always seem to be prefixed, so don't want to use SubString
							Debug.WriteLine(file);
							ctr.Image = Image.FromFile(file);
							ctr.Tag = representation;
							ctr.Size = new Size(100, 100);
							ctr.SizeMode = PictureBoxSizeMode.Zoom;
							ctr.Margin = new Padding(10, 10, 10, 10);
							ctr.Padding = new Padding(10, 10, 10, 10);
							ctr.Click += Image_Click;
							ctr.DoubleClick += Image_DoubleClick;
							pnlImages.Controls.Add(ctr);
						}
					}
					if (pnlImages.Controls.Count == 0)
						m_Message = Strings.Item("CCF_Failed"); // valid response, but no images available
				}
				catch (Exception ex) when (!Globals.Root.IsDebug)
				{ m_Message = ex.ToString(); }
				finally
				{ pnlImages.ResumeLayout(); }
			}
		}

		#endregion

		#region GUI event responses

		private void Image_Click(object sender, EventArgs e)
		{
			if (m_Selected == sender) return;
			if (m_Selected != null)
				m_Selected.BorderStyle = BorderStyle.None;
			m_Selected = (PictureBox)sender;
			m_Selected.BorderStyle = BorderStyle.Fixed3D;
			btnOK.Enabled = true;
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
			try
			{ MakeRequest(); }
			catch (Exception ex)
			{ MessageBox.Show(ex.ToString()); }
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			m_Selected = null;
			DialogResult = DialogResult.Cancel;
		}

		private void cmbUpdateWord_SelectedIndexChanged(object sender, EventArgs e)
		{
			btnOK.Enabled = (cmbUpdateWord.Text != "" || m_Selected != null);
		}

		#endregion

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


		#region Public result properties
		public string SelectedFile // returns relative path within the CCF folder
		{
			get
			{
				if (m_Selected == null) return "";
				return ((Response.Representation)m_Selected.Tag).Repr.Replace(Connection.URLReplace, "");
			}
		}

		public string SelectedConceptID
		{
			get
			{
				if (m_Selected == null) return "";
				return ((Response.Representation)m_Selected.Tag).ConceptID;
				// may be "" (actually shouldn't under CCF rules I believe, but we don't guard against it here)
			}
		}

		public string SelectedUpdateWordForm
		{ get { return cmbUpdateWord.Text; } }

		#endregion

	}
}