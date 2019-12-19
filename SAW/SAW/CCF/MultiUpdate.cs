using System;
using System.Diagnostics;

namespace SAW.CCF
{
	/// <summary>This is copied from WindowCatcher and was the COM entry point for a multi-update.  Now it contains the settings and GUI for the update</summary>
	public class MultiUpdate
	{
		public enum Functions
		{
			ChangeText,
			ChangeSymbols,
			RemoveInfo
		}

		public bool OnlySelected;
		public bool OnlyCoded = true;
		public string SourceLanguage;
		public string PreferredOutput;
		public string SecondarySymbols = "";
		public Functions PerformFunction = Functions.ChangeSymbols;

		private frmMultiUpdateProgress m_frmProgress;
		private int m_Done; // number of items completed, excluding current item

		private Query m_Query; // only defined during call to DoUpdate.  Kept here so can be used in response to connection completed
		private Response m_Response = null; // defined if response received in time to a request.  DoUpdate processes.  Null if none received

		public void Start()
		{
			m_frmProgress = new frmMultiUpdateProgress(true);
			m_frmProgress.Display(0, "");
			m_frmProgress.Show();
			m_frmProgress.Log(Strings.Item("CCF_Update_Starting") + ": " + Strings.Item("CCF_Action_" + PerformFunction));
			m_Done = 0;

			Connection.EnsureConnection();
			Connection.SharedConnection.QueryReply += SharedConnection_QueryReply;
			Connection.SharedConnection.Registered += SharedConnection_Registered;
		}

		public void Completed()
		{
			m_frmProgress.Log("Update completed");
			if (Connection.SharedConnection != null) // just in case
			{
				Connection.SharedConnection.QueryReply -= SharedConnection_QueryReply;
				Connection.SharedConnection.Registered -= SharedConnection_Registered;
			}
		}

		~MultiUpdate()
		{
			if (Connection.SharedConnection != null) // just in case
			{
				Connection.SharedConnection.QueryReply -= SharedConnection_QueryReply;
				Connection.SharedConnection.Registered -= SharedConnection_Registered;
			}
		}

		/// <summary>Looks up the resulting value for an item, given its text and concept ID</summary>
		public string DoLookup(string text, string conceptID)
		{
			m_frmProgress.Display(m_Done, text);
			string result = "";
			Debug.WriteLine("Function = " + PerformFunction);
			try
			{
				if (!string.IsNullOrEmpty(conceptID)) // this works for both the main lookups at the moment
				{
					m_Query = new QueryReprFromConceptID(conceptID);
					m_frmProgress.Log(Strings.Item("CCF_Update_QueryConcept") + ": " + conceptID);
				}
				else
				{
					m_Query = new QueryFromWord(text);
					((QueryFromWord)m_Query).LanguageIn = SourceLanguage;
					m_frmProgress.Log(Strings.Item("CCF_Update_QueryWord") + ": " + text);
				}
				m_Query.LanguageOut = PreferredOutput; // this field contains either a symbol set or language name depending on the function

				switch (PerformFunction)
				{
					case Functions.ChangeText:
						if (text == "")
							return ""; // without text cannot update
						MakeRequest();
						if (m_Response != null && m_Response.Representations.Count > 0)
						{ // for repr queries of text repr is the language text
							m_frmProgress.Log(Strings.Item("CCF_Update_TextResults").Replace("%0", m_Response.Representations.Count.ToString())
								.Replace("%1", m_Response.Representations[0].Repr));
							result = m_Response.Representations[0].Repr;
							// CCF seems to return with the word forms separated by commas
							if (result.Contains(","))
								result = result.Substring(0, result.IndexOf(","));
						}
						break;
					case Functions.ChangeSymbols:
						if (text == "")
							return ""; // without text cannot update
						if (SecondarySymbols != "")
							m_Query.LanguageOut += "," + SecondarySymbols;
						MakeRequest();
						if (m_Response != null && m_Response.Representations.Count > 0)
						{
							m_frmProgress.Log(Strings.Item("CCF_Update_ImageResults").Replace("%0", m_Response.Representations.Count.ToString()));
							string secondary = ""; // any image found in the secondary symbol set
							foreach (Response.Representation representation in m_Response.Representations)
							{
								if (representation.Language == PreferredOutput)
								{
									result = representation.Repr;
									break; // foreach
								}
								else if (representation.Language == SecondarySymbols && secondary == "")
									secondary = representation.Repr;
							}
							if (result == "" && secondary != "") // use any image in secondary set if none was found in the preferred set
								result = secondary;
							if (result != "") // some tidying up of the URL is needed...
								result = result.Replace(Connection.URLReplace, "").Replace("/", "\\"); // NB doesn't always seem to be prefixed, so don't want to use SubString
																										  // CCFConnection.LocalPath + 
						}
						// otherwise result left as ""
						break;
					case Functions.RemoveInfo:
						// no action required here for this one.  The call just updates the GUI.  Return ""
						break;
					default:
						Debug.Fail("Unexpected action in SAWMultiUpdate.DoUpdate");
						break;
				}
			}
			catch (Exception ex)
			{ // don't really want to throw exception (=return COM error) as this just crashes SAW
				Debug.WriteLine("DoUpdate FAILED: " + ex.ToString());
				return "";
			}
			finally
			{
				m_Query = null;
				m_Response = null;
				m_frmProgress?.Log("Query returned: " + result);
			}
			m_Done++;
			return result;
		}

		private void MakeRequest()
		{// CCF works async, but we really want a synchronous response, so some waiting needed...
			Debug.Assert(m_Query != null);
			// send request immediately IFF connected
			if (Connection.SharedConnection.Ready)
				Connection.SharedConnection.SendQuery(m_Query);
			// otherwise the SharedConnection_Registered event will send it (this event can only be raised by SharedConnection.DoReceive 
			// so there's no danger of it happening out of sequence

			DateTime startTime = DateTime.Now;
			while (m_Response == null && DateTime.Now.Subtract(startTime).TotalSeconds < 15)
			{
				Connection.SharedConnection.DoReceive();
				// process anything received on connection socket
				// will trigger SharedConnection_QueryReply when something received, which sets m_Response
				System.Threading.Thread.Sleep(10);
			}
			if (m_Response == null)
				m_frmProgress?.Log(Strings.Item("CCF_Update_TimedOut"));
		}

		private void SharedConnection_Registered(object sender, EventArgs e)
		{
			Debug.WriteLine("SharedConnection_Registered");
			if (m_Query != null) // must have been waiting connection to complete - now send request
				Connection.SharedConnection.SendQuery(m_Query);
		}

		private void SharedConnection_QueryReply(object sender, Connection.QueryReplyEvent e)
		{
			Debug.WriteLine("SharedConnection_QueryReply is expected reply: " + (e.Query == m_Query));
			if (e.Query == m_Query)
				m_Response = e.Response; // MakeRequest will stop looping now this is defined
		}

	}
}
