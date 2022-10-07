using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Linq;

namespace SAW
{
	internal partial class cfgListValues
	{
		public cfgListValues()
		{
			InitializeComponent();
		}

#if DEBUG
		public override void OnDisplay()
		{
			base.OnDisplay();
			m_Filling=true;
			lblTitle.Text = "Values (" + (m_Config.SourceFile ?? "?") + ")";
			lstValues.Items.Clear();
			List<string> keys = m_Config.Values.Keys.ToList();
			keys.Sort();
			foreach (string key in keys)
			{
				ListViewItem create = lstValues.Items.Add(key);
				create.SubItems.Add(m_Config.ReadString(key));
				create.SubItems.Add(ConfigKeyDescription(key, m_Config));
			}
			m_Filling=false;
		}
#endif

		public void btnListRemove_Click(object sender, EventArgs e)
		{
#if DEBUG
			if (lstValues.SelectedIndices.Count == 0)
				return;
			string key = lstValues.SelectedItems[0].Text;
			m_Config.Values.Remove(key);
			WrittenToCurrent();
#endif
		}

		public static string ConfigKeyDescription(string key, Config config)
		{
			if (key.StartsWith("Key_"))
			{
				return GUIUtilities.KeyDescription((Keys)Convert.ToInt32(key.Substring(4), 16));
				// add ElseIf as necessary
			}
			else if (key.StartsWith("Show_Palette_"))
			{
				string paletteKey = key.Replace("Show_Palette_", "");
				if (Palette.List.ContainsKey(paletteKey))
					return Strings.Translate(Palette.List[paletteKey].EditDescription);
				else
					return "Palette not found";
			}
			return "";
		}

		private void btnSet_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(txtAddKey.Text) || string.IsNullOrEmpty(txtAddValue.Text))
				return;
			m_Config.Write(txtAddKey.Text, txtAddValue.Text);
			WrittenToCurrent();
			OnDisplay();
			txtAddKey.Clear();
			txtAddValue.Clear();
		}
	}
}
