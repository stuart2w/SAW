using System;
using System.Diagnostics;
using RepoB;
using System.Windows.Forms;

namespace Update
{
	partial class frmUpdate : Form
	{

		private RepoVersion m_Version;
		private bool m_Auto;

		public frmUpdate(RepoVersion version)
		{
			// the signature of the version has already been verified
			m_Version = version;
			InitializeComponent();
		}

		private void frmWelcome_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			CheckSecurity();
			Globals.Root.PermanentLog("Commandline: " + Environment.CommandLine);
			lblPrompt.Text = Strings.Item("Install_Explain").Replace("%0", m_Version.Version);
			m_Auto = true;
			btnBegin.Visible = false;
		}

		private void btnBegin_Click(object sender, EventArgs e)
		{
			Transfer();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void Transfer()
		{
			Globals.Root.PermanentLog("Begin file transfer version " + m_Version.Version);
			proMain.Maximum = m_Version.Files.Length;
			proMain.Value = 0;
			int failures = 0;
			foreach (RepoFile file in m_Version.Files)
			{
				string destination = m_Version.LocalFilePath(file);
				if (destination.StartsWith("d:\\data\\ACE"))
					throw (new Exception("Writing to development folder"));
				try
				{
					if (file.Delete)
					{
						Globals.Root.PermanentLog("Deleting: " + System.IO.Path.GetFileName(destination));
						if (System.IO.Directory.Exists(destination))
							System.IO.Directory.Delete(destination, true);
						else if (System.IO.File.Exists(destination))
							System.IO.File.Delete(destination);
					}
					else
					{
						EnsureAllFolders(destination);
						if (!file.IsFolder())
						{
							string source = Globals.Root.UpdateFolder + "\\" + file.StoredName();
							RepoVersion.FileState status = m_Version.CheckFile(file);
							if (status != RepoVersion.FileState.OK)
							{
								if (!System.IO.File.Exists(source))
									status = RepoVersion.FileState.OK;
							}
							if (status == RepoVersion.FileState.Wrong)
							{
								Globals.Root.PermanentLog("Removing old file: " + System.IO.Path.GetFileName(file.Destination));
								try
								{
									System.IO.File.Delete(destination);
								}
								catch { }
							}
							if (status != RepoVersion.FileState.OK)
							{
								Globals.Root.PermanentLog("Copying file: " + System.IO.Path.GetFileName(file.Destination));
								System.IO.File.Copy(source, destination);
							}
						}
					}
				}
				catch (Exception ex)
				{
					failures += 1;
					Globals.Root.PermanentLog("Failed to write: " + file.Destination + "; " + ex.Message);
				}
				proMain.Value += 1;
			}
			if (failures > 0)
			{
				Globals.Root.PermanentLog("File copy failed.");
				MessageBox.Show(Strings.Item("Install_Complete_Errors").Replace("%0", failures.ToString()));
				btnBegin.Visible = true; // in case this was in auto mode
				btnCancel.Visible = true;
			}
			else
			{
				// we do not reregister PCADObjects6.dll.  I think it is OK to add and remove methods as long as no Guids change and no classes are added or removed
				System.Text.StringBuilder output = new System.Text.StringBuilder();
				int errors = 0;
				VerifyVersion(ref output, ref errors);
				if (errors > 0)
				{
					Globals.Root.PermanentLog("Post installation verification failed: " + output.ToString());
					MessageBox.Show(Strings.Item("Install_Verify_Wrong").Replace("%0", Globals.Root.SharedFolder + "\\permanent.txt")); // filename not actually used in this text at the mo
				}
				else
					RestartSoftware();
			}
		}

		private static void EnsureAllFolders(string path)
		{
			string folder = System.IO.Path.GetDirectoryName(path);
			if (folder == "")
			{
				Debug.Fail("Destination filename contains no folder");
				return;
			}
			if (!System.IO.Directory.Exists(folder))
			{
				EnsureAllFolders(folder);
				System.IO.Directory.CreateDirectory(folder);
			}
		}

		private static void RestartSoftware()
		{
			Globals.Root.PermanentLog("Completed bootstrap, returning to Splash");
			// start the  executable
			System.Threading.Thread.Sleep(1000); // make sure everything really is closed
			Process proc = new Process
			{
				StartInfo = { FileName = Globals.Root.EXEFolder + "\\SAW.exe" }
			};
			proc.Start();
			Application.Exit();
		}

		private void CheckSecurity()
		{
			string file = Globals.Root.InternalFolder + "\\Test.exe";
			try
			{
				System.IO.File.Delete(file);
			}
			catch
			{
			}
			try
			{
				System.IO.StreamWriter writer = new System.IO.StreamWriter(file);
				writer.WriteLine("Test");
				writer.Close();
				try
				{
					System.IO.File.Delete(file);
				}
				catch
				{
				}
			}
			catch (Exception)
			{
				lblPermissions.Text = Strings.Item("Install_Permissions");
				lblPermissions.Visible = true;
			}
		}

		public void VerifyVersion(ref System.Text.StringBuilder sb, ref int wrong)
		{
			// mostly copied from Designer, but with some changes
			if (sb == null)
				sb = new System.Text.StringBuilder();
			wrong = 0;
			foreach (RepoFile file in m_Version.Files)
			{
				if (!file.Delete)
				{
					RepoVersion.FileState status = (m_Version.CheckFile(file));
					switch (status)
					{
						case RepoVersion.FileState.Missing:
							sb.Append(file.Destination).Append(": Missing").Append("\r\n");
							wrong += 1;
							break;
						case RepoVersion.FileState.OK:
							break;
						case RepoVersion.FileState.Wrong:
							sb.Append(file.Destination).Append(": Mismatch").Append("\r\n");
							wrong += 1;
							break;
						default:
							sb.Append(file.Destination).Append("!unknown status!").Append("\r\n");
							break;
					}
				}
				Application.DoEvents();
			}
			sb.Append("\r\n").Append(m_Version.Files.Length.ToString()).Append(" total files.").Append("\r\n");
			sb.Append(wrong.ToString()).Append(" wrong.").Append("\r\n");
		}

		private void tmrFront_Tick(object sender, EventArgs e)
		{
			// sometimes on XP it seems to get hidden behind other things
			tmrFront.Enabled = false;
			BringToFront();
			if (m_Auto)
				Transfer();
		}

	}
}