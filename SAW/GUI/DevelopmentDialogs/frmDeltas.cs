using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Linq;

namespace SAW
{
	public partial class frmDeltas
	{

		private readonly Dictionary<string, Config.Delta> m_hashDeltas;
		private readonly List<Config.Delta> m_colDeltas; // list in the order that they are listed on the screen

#if DEBUG
		internal frmDeltas()
		{
			InitializeComponent();

			// remove folder stuff
			var colRemove = (from k in Config.SystemConfig.Values.Keys where k.StartsWith("Save_Folder_") || k.StartsWith("Open_Folder_") select k).ToList();
			foreach (var key in colRemove)
			{
				Config.SystemConfig.Values.Remove(key);
			}

			// Config.Delta constructor now removes value from configuration which shouldn't be included
			Dictionary<string, Config.Delta> deltas = new Dictionary<string, Config.Delta>();
			deltas.Add("System", new Config.Delta("System", Config.Levels.System));
			deltas.Add("User", new Config.Delta("User_Default", Config.Levels.User, "", null, Config.FromFile(Globals.Root.ConfigFolder + "\\user.sawcfg", Config.Levels.User)));
			//deltas.Add("Editor", new Config.Delta("Editor", Config.Levels.User));
			Activities.EnsureAllActivitiesLoaded();
			string folder = Globals.Root.ConfigFolder + "\\previous version configs"; // \\Activities";
			foreach (Document activity in Config.ActivityConfigs)
			{
				if (System.IO.File.Exists(folder + "\\" + activity.ID + ".sawcfg"))
				{
					Document before = Config.FromFile(folder + "\\" + activity.ID + ".sawcfg", Config.Levels.ActivityUser).Document;
					string name = Strings.Translate(before.Name);
					deltas.Add(activity.UserSettings.ID.ToString(), new Config.Delta(activity.UserSettings.ID.ToString(), Config.Levels.ActivityUser, name + "_User", before.UserSettings, activity.UserSettings));
					deltas.Add(activity.BothSettings.ID.ToString(), new Config.Delta(activity.BothSettings.ID.ToString(), Config.Levels.ActivityBoth, name + "_Both", before.BothSettings, activity.BothSettings));
				}
			}

			Config.SystemConfig.Write(Config.Delta_Applied, SoftwareVersion.Version);

			m_hashDeltas = deltas;
			m_colDeltas = new List<Config.Delta>();
			foreach (string ID in deltas.Keys)
			{
				m_colDeltas.Add(deltas[ID]);
			}
			foreach (Config.Delta delta in m_colDeltas)
			{
				string text = delta.Name;
				if (delta.IsEmpty)
					text += " (empty)";
				lstDeltas.Items.Add(text);
				// wipe any shape sequence:
				delta.After.ShapeSequence = null;
			}
			EnableSave();
			Config.UserUser.RecentFiles.Clear();
			Globals.Root.SaveAllActivityConfigs();
			Globals.Root.SaveUserConfigs();
			Globals.Root.SaveSystemConfig();
		}

		public static void Display()
		{
			frmDeltas frm = new frmDeltas();
			frm.ShowDialog();
		}

#else
		private frmDeltas()
		{
			InitializeComponent();
		}
#endif

		public void lstDeltas_SelectedIndexChanged(object sender, EventArgs e)
		{
			lstValues.Items.Clear();
			btnDelete.Enabled = false;
			if (lstDeltas.SelectedIndex < 0 || m_colDeltas == null)
				return;
			Config.Delta delta = m_colDeltas[lstDeltas.SelectedIndex];
			lblID.Text = "Cfg ID: " + delta.Before.ID + ";  Doc ID: " + delta.Before.Document.ID;
			Clipboard.SetText(delta.Before.Document.ID.ToString());
			List<string> keys = new List<string>(); // list of all the keys to display
			if (!chkDisplayUnchanged.Checked)
				keys.AddRange(delta.Changes.Values.Keys);
			else
			{
				// add up all of the keys which appear in both Before and After
				keys.AddRange(delta.After.Values.Keys);
				foreach (string strKey in delta.Before.Values.Keys)
				{
					if (!delta.After.Values.ContainsKey(strKey))
						keys.Add(strKey);
				}
			}
			keys.Sort();
			foreach (string key in keys)
			{
				ListViewItem create = lstValues.Items.Add(key);
				create.SubItems.Add(delta.Before.ReadString(key));
				create.SubItems.Add(delta.After.ReadString(key));
				create.SubItems.Add(cfgListValues.ConfigKeyDescription(key, delta.After));
				// items which have not changed should be fainter...
				if (!delta.Changes.Values.ContainsKey(key))
					create.ForeColor = System.Drawing.Color.Gray;
			}
		}


		public void lstValues_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (chkDisplayUnchanged.Checked)
			{
				btnDelete.Text = "Include";
				string key = lstValues.SelectedItems[0].Text;
				btnDelete.Enabled = lstValues.SelectedIndices.Count > 0 && !m_colDeltas[lstDeltas.SelectedIndex].Changes.Values.ContainsKey(key);
			}
			else
			{
				btnDelete.Text = "Delete";
				btnDelete.Enabled = lstValues.SelectedIndices.Count > 0;
			}
		}

		public void btnDelete_Click(object sender, EventArgs e)
		{
			// can be either Delete or Include depending on whether the list is displaying changes or all entries
			if (lstDeltas.SelectedIndex < 0)
				return;
			Config.Delta delta = m_colDeltas[lstDeltas.SelectedIndex];
			string key = lstValues.SelectedItems[0].Text;
			if (chkDisplayUnchanged.Checked)
			{
				// is Include
				if (delta.Changes.Values.ContainsKey(key))
					return;
				delta.Changes.Values.Add(key, delta.After.Values[key]);
				lstValues.SelectedItems[0].ForeColor = System.Drawing.Color.Black;
				lstDeltas.Items[lstDeltas.SelectedIndex] = lstDeltas.SelectedItem.ToString().Replace(" (empty)", "");
			}
			else
			{
				// Delete.  Deletes from list to save.  Doesn't affect configs
				delta.Changes.Values.Remove(key);
				lstValues.Items.Remove(lstValues.SelectedItems[0]);
				lstValues_SelectedIndexChanged(this, null);
				if (delta.IsEmpty)
					lstDeltas.Items[lstDeltas.SelectedIndex] = lstDeltas.SelectedItem + " (empty)";
			}
			EnableSave();
		}

		private void EnableSave()
		{
			foreach (Config.Delta delta in m_colDeltas)
			{
				if (!delta.IsEmpty)
				{
					btnSave.Enabled = true;
					return;
				}
			}
			btnSave.Enabled = false;
		}

		public void btnSave_Click(object sender, EventArgs e)
		{
#if DEBUG
			// This deliberately doesn't use FileDialog since it has a static default path
			if (dlgSave.ShowDialog() != DialogResult.OK)
				return;
			Config.Delta.SaveDeltas(dlgSave.FileName, m_hashDeltas);
			this.DialogResult = DialogResult.OK;
#endif
		}

	}
}
