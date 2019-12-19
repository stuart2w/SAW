using System.Collections.Generic;
using System;
using System.Windows.Forms;
using SAW.Repo2;

namespace SAW
{
	public partial class frmErrorReport
	{
		public frmErrorReport()
		{
			InitializeComponent();
		}

		internal static List<string> SubErrors = new List<string>(); // all recent LogSubError calls.  Gets cleared at the start of the document open process
		private static int NumberErrors = 0; // discounts up as unhandled errors are reported.  If there are several a quit button is added
		private static bool ShowingReport;

		public static void RecordSubError(string error)
		{
			SubErrors.Add(error);
			if (SubErrors.Count > 20)
				SubErrors.RemoveRange(0, 5);
		}

		public static void ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			// the application object attaches this as the WinForms unhandled exception handler; this gets any exceptions in the UI thread
			// Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException) would force these into the handler below, but this function can only be used before any controls created
			// (I.e. before frmMenu is created, but we don't have a Sub Main and I'm not sure how to get one)
			// We don't have this in UnhandledException because we cannot prevent the application from exiting, so it doesn't make much sense#
			if (e.Exception is UserException)
			{
				MessageBox.Show(e.Exception.Message);
				return;
			}
			else if (e.Exception is InvalidDataException)
			{
				MessageBox.Show(Strings.Item("Error_DataException").Replace("%0", e.Exception.Message));
				return;
			}
			ReportException(e.Exception);
		}

		public static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			// the application object attaches this as the AppDomain unhandled exception handler; this gets any exceptions in background threads
			ReportException((Exception)e.ExceptionObject);
			Application.Exit(); // We cannot prevent the application from exiting.  Doing this does prevent the Windows error box appearing however
		}

		private static void ReportException(Exception e)
		{
			try
			{
				if (ShowingReport)
				{
					Utilities.LogSubError("Cannot show error, report already open");
					Utilities.LogSubError(e);
					return;
				}
				Globals.Root.Log.WriteLine(e.ToString());
			}
			catch
			{ }
			ShowingReport = true;
			try
			{
				// if there is an error dragging or similar, which end up with endless error reports as the mouse moves, try to cancel anything ongoing
				try
				{
					Globals.AbandonOngoing(true);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.ToString());
				}
				DoErrorReport(e);
			}
			catch (Exception ex)
			{ MessageBox.Show("Error doing error report: " + ex); }
			finally
			{ ShowingReport = false; }
		}

		// in some contexts we only report an error once no matter how often it occurs
		internal enum OnceErrorContexts
		{
			// errors which should only be reported once per run
			Startup,
			frmMain_ApplyConfig
		}
		/// <summary>errors which have already been reported.  Only applies to ReportErrorOnce </summary>
		private static readonly Dictionary<OnceErrorContexts, Exception> g_OnceErrors = new Dictionary<OnceErrorContexts, Exception>();
		internal static void ReportErrorOnce(OnceErrorContexts error, Exception ex)
		{
			if (g_OnceErrors.ContainsKey(error))
				return;
			g_OnceErrors.Add(error, ex);
			DoErrorReport(ex);
		}

		public static void DoErrorReport(Exception ex)
		{
			NumberErrors += 1;
			frmErrorReport frmNew = new frmErrorReport();
			frmNew.lblErrorMessage.Text = Strings.Item("Error_Message") + ex.Message;
			DialogResult eResult = frmNew.ShowDialog();
			ProcessErrorReport(ex.ToString(), frmNew.txtInfo.Text, eResult == DialogResult.OK);
		}

		public static void ProcessErrorReport(string content, string user, bool send)
		{
			try
			{
				using (System.IO.MemoryStream report = new System.IO.MemoryStream(10000))
				using (DataWriter writer = new DataWriter(report, FileMarkers.ErrorReport))
				{
					if (Globals.Root.CurrentDocument != null)
					{
						try
						{
							writer.Write(Globals.Root.CurrentDocument);
						}
						catch (Exception e2)
						{
							Utilities.LogSubError("Document could not be written to error report", true);
							Utilities.LogSubError(e2, true);
							// If there was an exception then usually nothing could have been written to the stream, because the output for each object is typically buffered
							// by the DataWriter
							writer.Write(new Document(true));
						}
					}
					else
						writer.Write(new Document(true));
					// And now use the standard binary writer methods to write the rest of the data
					writer.Write(content);
					writer.Write(user);
					writer.Write(SubErrors);
					writer.WriteByte((byte)Globals.Root.User);
					writer.Write(Activities.ActivityID);
					writer.Write(Config.UserUser);
					writer.Write((Datum)null); // in Splash this is the teacher user
					writer.Write("N/A");
					writer.Write(Globals.Root.Log.GetEntireContent());
					writer.Write(Globals.Root.LogPermanent.GetEntireContent());
					Globals.WriteDiagnosticEvents(writer);
					writer.Write(false); // for compatibility with splash

					if (!send)
					{
						// just writes it to disk
						DoSaveData(report.ToArray());
					}
					else if (DoSendData(report.ToArray()))
					{
						MessageBox.Show(Strings.Item("Error_Sent"));
					}
					else
					{ // calls here tried to send, but failed
						DoSaveData(report.ToArray());
						MessageBox.Show(Strings.Item("Error_Failed"));
					}
				}
			}
			catch (Exception exInternal) when (!Globals.Root.IsDebug)
			{
				if (send)
					MessageBox.Show(Strings.Item("Error_Failed") + "\r\n" + "(" + exInternal + ")");
				try
				{
					Globals.Root.LogPermanent.WriteLine("Failed to send error report: " + exInternal);
				}
				catch { }
			}
		}

		public void frmErrorReport_Load(object sender, EventArgs e)
		{
			Strings.Translate(this);
			GUIUtilities.ScaleDPI(this);
			btnQuit.Visible = NumberErrors >= 4;
		}

		private static bool DoSendData(byte[] bytData)
		{
			// pass in query string starting with parameters (exists.aspx?)
			Cursor.Current = Cursors.WaitCursor;
			try
			{
				using (Repo2SoapClient server = new Repo2SoapClient("Repo2Soap", Server.TechURL + "repo2.asmx"))
				{
					server.StoreError(Server.Software, bytData);
				}
			}
			catch (Exception ex) when (!Globals.Root.IsDebug)
			{
				Utilities.LogSubError(ex);
				return false;
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
			return true;
		}

		private static void DoSaveData(byte[] bytData)
		{
			try
			{
				string folder = System.IO.Path.Combine(Globals.Root.SharedFolder, "errors");
				MessageBox.Show("DoSaveError in " + folder);
				if (!System.IO.Directory.Exists(folder))
					System.IO.Directory.CreateDirectory(folder);
				System.IO.FileStream output = new System.IO.FileStream(folder + System.IO.Path.DirectorySeparatorChar + DateTime.Now.ToString().Replace("/", "-").Replace("\\", "-").Replace(":", "-") + ".error", System.IO.FileMode.Create, System.IO.FileAccess.Write);
				output.Write(bytData, 0, bytData.Length);
				output.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private static void StreamToFile(System.IO.Stream stream, string file)
		{
			// NB doesn't close stream
			System.IO.FileStream strOut = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);
			byte[] buffer = new byte[1001];
			int count = stream.Read(buffer, 0, 1000);
			while (count > 0)
			{
				strOut.Write(buffer, 0, count);
				count = stream.Read(buffer, 0, 1000);
			}
			strOut.Close();
		}

		public void btnQuit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
	}
}
