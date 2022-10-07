using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAW
{
	internal partial class frmAddCommand : Form
	{

		#region Initialise

		public frmAddCommand()
		{
			InitializeComponent();
			Strings.Translate(this);
			FillCommands();
		}

		private class Folder
		{
			/// <summary>Child folders indexed by code</summary>
			public Dictionary<string, Folder> Children = new Dictionary<string, Folder>();
			public readonly string Code;
			/// <summary>Actual text in UI</summary>
			public readonly string Text;

			public List<Command> Commands = new List<Command>();

			public Folder(string code)
			{
				Code = code;
				Text = Strings.Item("Script_Group_" + code);
			}

			public Folder GetChild(string code)
			{
				if (!Children.ContainsKey(code))
				{
					var create = new Folder(code);
					Children.Add(code, create);
				}
				return Children[code];
			}

		}

		private void FillCommands()
		{
			var rootFolder = new Folder("scripts");
			foreach (var item in CommandList.Entries.Values)
			{
				if (item.AddGUIPath == null)
					continue;
				var entry = CommandList.Entries[item.Code];
				// each possible command can be in a different folder, in principle (used by CmdKeysOut a lot; perhaps no others)
				for (int commandIndex = 0; commandIndex < entry.PossibleCommandsLower.Length; commandIndex++)
				{
					var folder = rootFolder;
					// can't look up directing in AddGuiPath as it is allowed to have just a single value which is to be used for all, so this is actual index to use:
					int guiIndex = entry.AddGUIPath.Length > 1 ? commandIndex : 0;
					if (item.AddGUIPath[guiIndex] == null)
						continue;
					foreach (string section in item.AddGUIPath[guiIndex].Split('/'))
					{
						folder = folder.GetChild(section);
					}
					string script = entry.PossibleCommandsLower[commandIndex];
					Command command = (Command)Activator.CreateInstance(entry.Class);
					command.InitialiseFromCommandUsed(script);
					//Command command = Command.FromScript(script, true); // just initialises the initial part of script
					folder.Commands.Add(command);
				}
			}
			PopulateTree(tvCommands.Nodes, rootFolder);
		}

		private void PopulateTree(TreeNodeCollection parent, Folder source)
		{
			// add sub-folders, sorted alphabetically:
			foreach (Folder sub in (from f in source.Children.Values orderby f.Text select f))
			{
				TreeNode node = new TreeNode(sub.Text);
				parent.Add(node);
				PopulateTree(node.Nodes, sub);
			}
			foreach (Command c in source.Commands)
			{
				parent.Add(new TreeNode(c.GetCommandName()) { Tag = c });
			}
		}

		#endregion

		public static Command CreateCommand(Form ownerForm = null)
		{
			using (frmAddCommand frm = new frmAddCommand())
			{
				frm.Owner = ownerForm;
				if (frm.ShowDialog() == DialogResult.OK)
					return frm.m_Current;
			}
			return null;
		}

		#region Current command

		private Command m_Current;
		private void tvCommands_AfterSelect(object sender, TreeViewEventArgs e)
		{
			m_Current = (e.Node?.Tag as Command)?.Clone();
			m_Current?.InitialiseDefaultsForCreation(); // will fill in any params needed to make the rest work
			lblDescription.Text = m_Current?.GetDescription()?.Trim('"') ?? Strings.Item("SAW_AddCommand_Prompt");
			propertyEditor.EditCommand(m_Current, null);
			btnOK.Enabled = (m_Current != null);
		}

		#endregion

		private void btnOK_Click(object sender, EventArgs e)
		{
			string error = propertyEditor.GetValidationError();
			if (error != null)
				MessageBox.Show(error);
			else
				DialogResult = DialogResult.OK;
		}

	}
}
