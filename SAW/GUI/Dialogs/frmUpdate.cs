using System;
using System.Windows.Forms;
using SAW.Repo2;

namespace SAW
{
	internal partial class frmUpdate
	{
		public frmUpdate()
		{
			InitializeComponent();
		}

		private float m_LatestNumber = 0;
		private string Folder; // Load sets to AM.SharedFolder + IO.Path.DirectorySeparatorChar + "Update" + IO.Path.DirectorySeparatorChar
		private RepoB.RepoVersion m_Latest;
		/// <summary>is true if the latest version can only be downloaded from the website, and this auto-update can't cope</summary>
		private bool m_RequiresWeb = false; // 
		private Repo2SoapClient m_Server;

		public void frmUpdate_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
			txtCurrent.Text = SoftwareVersion.VersionString;
			Folder = Globals.Root.SharedFolder + System.IO.Path.DirectorySeparatorChar + "Update";
			if (!System.IO.Directory.Exists(Folder))
				System.IO.Directory.CreateDirectory(Folder);
			Folder += System.IO.Path.DirectorySeparatorChar;
		}

		public void tmrStart_Tick(object sender, EventArgs e)
		{
			// finds which is the latest version available, and gets the index file for it
			tmrStart.Enabled = false;
			this.Cursor = Cursors.WaitCursor;
			string errorAction = "Update_Contact_Failed"; // first part of error message - changes throughout function
			try
			{
				txtLatest.Text = "?"; // will be left in place if fn fails early
				m_Server = new Repo2SoapClient("Repo2Soap", Server.TechURL + "repo2.asmx");
				string latest = m_Server.LastVersionB(Server.Software, Server.PRODUCT, SoftwareVersion.VersionString, "", false, Server.Language2, RootApplication.GetNetVersion(), RootApplication.GetMachineID());
				// if it is prefixed with "w", then it needs to be downloaded from the website
				m_RequiresWeb = false;
				if (latest.StartsWith("w", StringComparison.InvariantCultureIgnoreCase))
				{
					m_RequiresWeb = true;
					latest = latest.Substring(1);
				}
				latest = latest.Split('#')[0];
				// other info can be appended after #.  Not used here
				txtLatest.Text = latest;
				m_LatestNumber = SoftwareVersion.VersionNumberFromString(latest);
				errorAction = "Update_Version_Fetch_Failed";
				m_Latest = null;
				if (!m_RequiresWeb)
					m_Latest = GetVersionIndex(latest);
				errorAction = "Update_Check_Failed";
				//MessageBox.Show("m_LatestNumber=" + m_LatestNumber + ", SoftwareVersion.Version=" + SoftwareVersion.Version);
				if (m_LatestNumber <= SoftwareVersion.Version + 0.001) // the +0.001 added as bizarrely this condition was failing on 32 bit machines when both were "700.7"
				{
					lblStatus.Text = Strings.Item("Update_Latest_Installed");
					btnDownload.Visible = false;
					int errors = 0;
					System.Text.StringBuilder sb = new System.Text.StringBuilder();
					VerifyVersion(m_Latest, false, ref sb, ref errors);
					//Clipboard.SetText(sb.ToString());
					if (errors > 0)
					{
						lblStatus.Text = Strings.Item("Update_Installation_Damaged");
						btnDownload.Text = Strings.Item("Update_Reinstall");
						btnDownload.Visible = true;
					}
				}
				else if (m_RequiresWeb)
				{
					lblStatus.Text = Strings.Item("Update_Newer_Manual");
					btnDownload.Text = Strings.Item("Update_Show_Website");
					btnDownload.Visible = true;
				}
				else
				{
					lblStatus.Text = Strings.Item("Update_Newer_Available");
					btnDownload.Text = Strings.Item("Update_Download_Install").Replace("%0", latest);
					btnDownload.Visible = true;
				}
			}
			catch (Exception ex)
			{
				lblStatus.Text = Strings.Item(errorAction).Trim() + " " + ex.Message;
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		public void btnDownload_Click(object sender, EventArgs e)
		{
			// Download and install latest version; theoretically this could be the current version if it is damaged
			if (m_RequiresWeb)
			{
				MessageBox.Show(Strings.Item("Update_Restart"));
				Windows.ShellExecute(this.Handle, 0, Server.TechURL + "redirect.aspx?to=download&version=" + txtLatest.Text + "&lang=" + Server.Language2, "", "", 0);
				// "&age=" + Globals.Root.NominalAgeString
			}
			else
				FetchVersionFiles(m_Latest);
		}

		#region Whole version processing
		public void VerifyVersion(RepoB.RepoVersion version, bool includeUpdateFolder, ref System.Text.StringBuilder sb, ref int wrong)
		{
			// checks our currently installed files against the version, and returns the number of errors and a report in the ByRef parameters
			// if includeUpdateFolder then files available in the update folder are counted
			// sb can be nothing if the report is not required
			if (sb == null)
				sb = new System.Text.StringBuilder();
			wrong = 0;
			foreach (RepoB.RepoFile file in version.Files)
			{
				if (!file.Delete)
				{
					var statusInt = version.CheckFile(file);
					if (statusInt != 0 && includeUpdateFolder && FileAvailableInUpdate(file))
						statusInt = 0;
					switch (statusInt)
					{
						case RepoB.RepoVersion.FileState.Missing:
							sb.Append(file.Destination).Append(": Missing").Append("\r\n");
							wrong += 1;
							break;
						case RepoB.RepoVersion.FileState.OK:
							break;
						case RepoB.RepoVersion.FileState.Wrong:
							sb.Append(file.Destination).Append(": Mismatch - current is ");
							sb.Append(RepoB.FileHash.GetFileHash(version.LocalFilePath(file)));
							sb.Append(", required ").Append(file.MD5).Append("\r\n");
							wrong += 1;
							break;
						default:
							sb.Append(file.Destination).Append("!unknown status!").Append("\r\n");
							break;
					}
				}
				Application.DoEvents();
			}
			sb.Append("\r\n").Append(version.Files.Length.ToString()).Append(" total files.").Append("\r\n");
			sb.Append(wrong.ToString()).Append(" wrong.").Append("\r\n");
		}

		/// <summary>download all the files in the version which are neither already in the update folder, nor match the currently installed files
		/// Returns true if all of the files were downloaded successfully</summary>
		private bool FetchVersionFiles(RepoB.RepoVersion version)
		{
			try
			{
				// generate a list of required files...
				Globals.Root.LogPermanent.WriteLine("Starting fetch version " + version.Version);
				StartAsync(Strings.Item("Update_Checking_Files"));
				RepoB.XferList updateList = new RepoB.XferList();
				foreach (RepoB.RepoFile file in version.Files)
				{
					if (!file.IsFolder() && !FileAvailableInUpdate(file) && version.CheckFile(file) != RepoB.RepoVersion.FileState.OK)
						updateList.Add(file);
					Application.DoEvents();
				}
				if (m_Cancel)
					return false;

				// Download the actual files...
				StartAsync(Strings.Item("Update_Downloading"), updateList.Count(), updateList.TotalSize());
				foreach (RepoB.RepoFile file in updateList.Files)
				{
					string localPath = Folder + file.StoredName();
					if (file.Delete == false && (!System.IO.File.Exists(localPath) || RepoB.FileHash.GetFileHash(localPath).ToLower() != file.MD5.ToLower()))
					{
						if (System.IO.File.Exists(localPath))
						{
							// needs to ensure that the file is writable
							System.IO.File.SetAttributes(localPath, System.IO.FileAttributes.Normal);
						}
						FetchFile("Files\\" + file.StoredName(), localPath, file.Size);
						if (m_Cancel)
							return false;
					}
					else
						AsyncProgress(1, file.Size);
				}
				if (m_Cancel)
				{
					return false;
				}

				// Verify the integrity of the files
				StartAsync(Strings.Item("Update_Verifying"), updateList.Count(), 0);
				int fails = 0;
				foreach (RepoB.RepoFile file in updateList.Files)
				{
					if (!file.Delete)
					{
						string localPath = Folder + file.StoredName();
						if (!System.IO.File.Exists(localPath) || RepoB.FileHash.GetFileHash(localPath).ToLower() != file.MD5.ToLower())
						{
							fails += 1;
							Globals.Root.Log.WriteLine("Failed, file missing or corrupt: " + file.Destination + " (stored at " + localPath + ")");
						}
					}
					AsyncProgress(1, 0);
					if (m_Cancel)
						return false;
				}

				// Report the result to the user
				if (fails == 0)
					StartInstall(version.Version);
				else
					MessageBox.Show(Strings.Item("Update_Verify_Failed").Replace("%0", fails.ToString()));
			}
			catch (Exception ex)
			{
				Utilities.LogSubError(ex);
				MessageBox.Show(ex.Message);
			}
			finally
			{
				EndAsync();
			}
			return true;
		}

		private void StartInstall(string version)
		{
			MessageBox.Show(Strings.Item("Update_Install_Starting"));
			Globals.Root.LogPermanent.WriteLine("About to start bootstrap of version " + version);
			//' the update executable may try to write to the same log files - so make sure they are closed immediately

			((IDisposable)Globals.Root.Log).Dispose();
			((IDisposable)Globals.Root.LogPermanent).Dispose();
			Globals.Root.Log = new NullLogger();
			Globals.Root.LogPermanent = new NullLogger();
			System.Threading.Thread.Sleep(500);
			// start the update executable, and close Designer
			string executable = System.IO.Path.Combine(Globals.Root.EXEFolder, "Update.exe");
			// for some reason ShellExecute is causing the app to not appear - it is running but window never appears.  Same with or without elevation.  .net version works tho...

			System.Diagnostics.Process proc = new System.Diagnostics.Process
			{
				StartInfo =
				{
					FileName = executable,
					Verb = "",
					Arguments = version + " /auto"
				}
			};
			proc.Start();

			DialogResult = DialogResult.OK;
		}

		#endregion

		#region Asynchronous control and feedback
		private bool m_Cancel;
		private bool m_Processing;
		private int m_FilesDone;
		private int m_FilesTotal;
		private int m_SizeDone;
		private int m_SizeTotal;
		private int m_Retries;
		private string m_ProgressMessage;
		private bool m_ProgressNeedsRefresh; // Async thread cannot directly update this form  (didn't create it).  This flags that label needs update

		private void StartAsync(string message, int files = 0, int size = 0)
		{
			// this can be called repeatedly to start other sections
			// if progress updates are required, the total size should be provided
			btnDownload.Enabled = false;
			btnClose.Text = Strings.Item("Cancel");
			m_Cancel = false;
			m_Processing = true;
			m_FilesTotal = files;
			m_SizeTotal = size;
			m_FilesDone = 0;
			m_SizeDone = 0;
			m_Retries = 0;
			m_ProgressMessage = message;
			lblStatus.Text = message;
			lblStatus.Refresh();
			tmrRefresh.Enabled = true;
			proDownload.Value = 0;
			proDownload.Visible = true;
		}

		private void EndAsync()
		{
			btnDownload.Enabled = true; // might not be visible of course!
			btnClose.Text = Strings.Item("Close");
			m_Processing = false;
			tmrRefresh.Enabled = false;
			proDownload.Visible = false;
		}

		public void btnClose_Click(object sender, EventArgs e)
		{
			// Just closes the form is nothing is happening, however if we are in the middle something asynchronous, this must cancel it
			if (m_Processing)
				m_Cancel = true;
			else
			{
				this.DialogResult = DialogResult.Cancel;
				lblStatus.Text = Strings.Item("Update_Cancelled");
			}
		}

		private void Progress(int files, int size)
		{
			m_FilesDone += files;
			m_SizeDone += size;
			RefreshProgress();
		}

		private void AsyncProgress(int files, int size)
		{
			m_FilesDone += files;
			m_SizeDone += size;
			m_ProgressNeedsRefresh = true;
		}

		private void RefreshProgress()
		{
			// the basic message needs to be given again, followed by the number of files and size which has now been processed
			m_ProgressNeedsRefresh = false;
			lblStatus.Text = m_ProgressMessage + "\r\n";
			if (m_FilesTotal > 0)
				lblStatus.Text += Strings.Item("Update_Files").Replace("%0", m_FilesDone.ToString()).Replace("%1", m_FilesTotal.ToString()) + ". ";
			if (m_SizeTotal > 0)
			{
				lblStatus.Text += " " + Strings.Item("Update_Size").Replace("%0", Utilities.MemoryText(m_SizeDone)).Replace("%1", Utilities.MemoryText(m_SizeTotal));
				try
				{
					proDownload.Maximum = m_SizeTotal;
					proDownload.Value = m_SizeDone;
				}
				catch
				{
				}
			}
		}

		public void tmrRefresh_Tick(object sender, EventArgs e)
		{
			if (m_ProgressNeedsRefresh)
				RefreshProgress();
		}

		#endregion

		#region Transfer processing

		private RepoB.RepoVersion GetVersionIndex(string version)
		{
			Globals.Root.Log.WriteLine("Fetching version index: " + version);
			string file = Folder + version + ".repo";
			if (!System.IO.File.Exists(file))
			{
				byte[] data = m_Server.Fetch(Server.Software, RepoB.RepoVersion.StoredAtRel(Server.PRODUCT, version));
				System.IO.File.WriteAllBytes(file, data);
			}
			RepoB.RepoVersion create = RepoB.RepoList.LoadVersionFile(file);
			VerifySignature(create);
			create.SetLocalPath(Globals.Root.InternalFolder + System.IO.Path.DirectorySeparatorChar);
			create.SetLocalPath("%shared%", Globals.Root.SharedFolder + System.IO.Path.DirectorySeparatorChar);
			return create;
		}

		private void VerifySignature(RepoB.RepoVersion version)
		{
			if (string.IsNullOrEmpty(version.Signature))
				return;
			if (version.Signature.ToLower() != version.CalculateSignature().ToLower())
			{
				string file = Folder + version.Version + ".repo";
				if (System.IO.File.Exists(file))
					System.IO.File.Delete(file);
				throw new Exception("UpdateWrongSignature");
			}
		}

#if DEBUG
		private const int MAXSYNCHRONOUSCHUNK = 1000;
#else
		private const int MAXSYNCHRONOUSCHUNK = 50000;
#endif

		private bool FileAvailableInUpdate(RepoB.RepoFile file)
		{
			// returns true if this file is available in the update folder (ignores whether or not the file matches the one currently installed)
			if (file.IsFolder())
				return false;
			string filename = Folder + file.StoredName();
			if (!System.IO.File.Exists(filename))
				return false;
			if (RepoB.FileHash.GetFileHash(filename) == file.MD5)
				return true;
			else
			{
				try
				{
					System.IO.File.Delete(filename);
				}
				catch (Exception)
				{
				}
				Utilities.LogSubError("Mismatched file in update folder: " + filename);
				return false;
			}
		}

		private void FetchFile(string serverName, string localPath, int size)
		{
			// will throw an exception if the file could not be downloaded
			EnsureAllFolders(localPath);
			byte[] complete = new byte[size - 1 + 1];
			int start = 0; // on a failure we can restart at the current position
			do
			{
				Globals.Root.Log.WriteLine("Start fetch: " + serverName + " -> " + localPath);
				try
				{
					while (start < size)
					{
						int length = Math.Min(MAXSYNCHRONOUSCHUNK, size - start);
						byte[] data = m_Server.FetchChunk(Server.Software, serverName, start, length);
						Array.Copy(data, 0, complete, start, length);
						start += length;
						AsyncProgress(0, length);
						Application.DoEvents();
					}
					System.IO.File.WriteAllBytes(localPath, complete);
					AsyncProgress(1, 0);
					return;
				}
				catch (System.Net.WebException exMissing)
				{
					if (exMissing.Status == System.Net.WebExceptionStatus.ProtocolError)
					{
						// indicates that a complete response was returned, but it did not have a success status code
						if ((int)((System.Net.HttpWebResponse)exMissing.Response).StatusCode == 599)
						{
							// indicates that the file is not available on the update server
							throw new Exception("UpdateFileMissing");
						}
						if (m_Retries >= 10)
							throw;
						WaitRetry(Strings.Item("Update_Download_Failed"), 30);
					}
					else
					{
						if (m_Retries >= 10)
							throw;
						WaitRetry(Strings.Item("Update_Download_Failed"), 30);
					}
				}
			} while (true);
		}

		private static void EnsureAllFolders(string path)
		{
			string folder = System.IO.Path.GetDirectoryName(path);
			if (!System.IO.Directory.Exists(folder))
			{
				EnsureAllFolders(folder);
				System.IO.Directory.CreateDirectory(folder);
			}
		}

		public void WaitRetry(string message, int seconds)
		{
			DateTime startTime = DateTime.Now;
			m_Retries += 1;
			try
			{
				while (DateTime.Now.Subtract(startTime).TotalSeconds < seconds)
				{
					lblStatus.Text = Strings.Item("Update_Retries") + m_Retries + ". " + message + " " + (seconds - (int)DateTime.Now.Subtract(startTime).TotalSeconds) + "s";
					lblStatus.Refresh();
					Application.DoEvents();
					System.Threading.Thread.Sleep(100);
					Application.DoEvents();
					if (m_Cancel)
						return;
				}
			}
			finally
			{
				lblStatus.Text = Strings.Item("Update_Retrying");
			}
		}

		#endregion

	}
}
