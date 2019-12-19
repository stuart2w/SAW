using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;

namespace SAW.CCF
{
	/// <summary>connects to a CCF server at a given IP (assumed that in the long run this will probably be localhost)
	/// throws an exception on construction if it cannot connect the socket at all
	/// .Ready indicates whether it has made the initial ID request to the server with a successful response</summary>
	internal class Connection : IDisposable
	{
		public bool Ready { get; private set; }

		#region Shared resources
		/// <summary>the part of the URL returned by the server which needs to be removed </summary>
		internal const string URLReplace = "urn:ccf:repr:";
		/// <summary>URLReplace is replaced with this </summary>
		internal static string LocalPath;
		#endregion

		#region Private fields
		private Socket m_Socket;
		private string m_ID;
		private static readonly Regex IDCheck = new Regex("[-A-Z0-9._]+@CCFCLIENT", RegexOptions.IgnoreCase);
		// this expression might be excluding some punctuation marks which would be allowed by the CCF server.  I don't have a specification of exactly what is permitted

		/// <summary>data received which does not yet contain an entire string</summary>
		private byte[] m_ReceiveBuffer = new byte[1024];
		/// <summary>number of bytes in the above buffer</summary>
		private int m_Received;
		#endregion

		#region Public connection info

		public const int ServerPort = 8899;
		//public static IPAddress FemtioProcent = new IPAddress(new byte[]  { 90, 229, 197, 89 });
		/// <summary>queries which have not yet had replies</summary>
		public Dictionary<string, Query> OutstandingQueries = new Dictionary<string, Query>();

		#endregion

		#region Event declarations

		public event EventHandler<SingleFieldEventClass<string>> MessageSent;
		public event EventHandler<SingleFieldEventClass<string>> MessageReceived; // replies will fire both this and QueryReply (latter only if reply makes some sort of sense)
		public class QueryReplyEvent : EventArgs
		{
			public Response Response;// always set if this event is raised
			public Query Query; // only defined if Response could be matched to an outstanding query

			public QueryReplyEvent(Response response)
			{ Response = response; }

			public QueryReplyEvent(Response response, Query query)
			{ Query = query; Response = response; }

		}
		public event EventHandler<QueryReplyEvent> QueryReply;
		public event EventHandler Registered;

		#endregion

		#region Constructors and static instance - start connection

		/// <summary>static constructor just initialises LocalPath</summary>
		static Connection()
		{
			LocalPath = Environment.GetEnvironmentVariable("CCF_HOME");
			if (LocalPath == "") // returns "" if variable not set
				LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ccf";
			LocalPath += "\\";
		}

		/// <summary>is maintained between requests.  Also used by SAWMultiUpdate </summary>
		internal static Connection SharedConnection;
		/// <summary>ID to use when registering; we keep this the same until this application is un-loaded </summary>
		/// <remarks>Actually this is redundant atm as the connection is kept open anyway</remarks>
		private static int g_ID;

		internal static void EnsureConnection()
		{
			if (SharedConnection != null && SharedConnection.Ready == false)
			{ // connection must have failed to be registered last time.  Dispose and try again
				SharedConnection.Dispose();
				SharedConnection = null;
			}
			if (SharedConnection == null)
			{
				if (g_ID == 0)
				{
					Random rnd = new Random(Environment.TickCount);
					g_ID = rnd.Next();
				}
				SharedConnection = new Connection("SAW-" + g_ID + "@CCFCLIENT");
			}
		}

		// parameter to constructors is the ID which should be used when communicating with the server.  Must be in the form my-id@group
		public Connection(string ID)
		{
			Initialise(ID, IPAddress.Loopback);
		}

		public Connection(string ID, IPAddress IP)
		{
			Initialise(ID, IP);
		}

		private void Initialise(string ID, IPAddress IP)
		{
			if (!IDCheck.IsMatch(ID)) throw new Exception("ID must be in the form 'my-id@group'");
			m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			m_Socket.Connect(IP, ServerPort);
			// since we are generally sending to local host, only a short time should be required to send
			m_Socket.SendTimeout = 5000;
			m_Socket.Blocking = false;
			m_ID = ID;
			// send registration message to the server.  Not sure what should be in the request-ID?
			SendRaw("_ @ register", "request-id " + ID);
		}

		#endregion

		public void SendQuery(Query query)
		{
			if (!Ready) throw new Exception("Cannot send requests until registration has been confirmed");
			SendRaw(m_ID + " ccfServerClient@CCFSERVER", query.RequestString());
			OutstandingQueries.Add(query.Reference.ToString(), query);
		}

		private void SendRaw(string envelope, string content)
		{
			if (envelope.Contains(";") || content.Contains(";")) throw new Exception("Message cannot contain ';'");
			if (envelope.Contains("\n") || content.Contains("\n")) throw new Exception("Message cannot contain linefeed");
			string message = envelope + "; " + content + '\n';
			byte[] buffer = Encoding.UTF8.GetBytes(message);
			// since the data to be sent is quite small, and is always going to local host, we just send everything synchronously
			// delays longer than the five second timeout set in the constructor probably imply things have gone horribly wrong
			int sent = m_Socket.Send(buffer);
			if (sent < buffer.Length) throw new Exception("Not all data was sent");
			MessageSent?.Invoke(this, message);
			Debug.WriteLine("Message sent: " + message);
		}

		/// <summary>this method should be polled intermittently.  It will receive whatever data is available in the socket
		/// they could potentially be a partial response in the socket, so the data is stored in a byte buffer
		/// and only converted to a string if it contains a linefeed
		/// NOTE: this is probably not robust; a linefeed byte could occur as part of a multibyte character</summary>
		public void DoReceive()
		{
			int available = m_Socket.Available;
			if (available <= 0) return;
			if (m_ReceiveBuffer.Length < available + m_Received)
				Array.Resize(ref m_ReceiveBuffer, available + m_Received);
			int received = m_Socket.Receive(m_ReceiveBuffer, m_Received, m_ReceiveBuffer.Length - m_Received, SocketFlags.None);
			if (received > 0)
			{
				m_Received += received;
				// check for a line feed character...
				while (m_Received > 0)
				{
					Debug.WriteLine("m_Received=" + m_Received);
					int EOL = Array.IndexOf(m_ReceiveBuffer, (byte)10, 0, m_Received);
					if (EOL < 0) break; // from the while loop
					string message = Encoding.UTF8.GetString(m_ReceiveBuffer, 0, EOL);
					Debug.WriteLine("Message received: " + message);
					if (MessageReceived != null) MessageReceived.Invoke(this, message);

					// remove the used bytes from the buffer
					if (EOL == m_Received - 1) // the entire buffer was converted to a string.  We can just reset
						m_Received = 0;
					else
					{
						// only part of the buffer was used.  We need to move the remaining buffer down to the beginning
						int remaining = m_Received - EOL - 1;
						byte[] temp = new byte[1024];
						Array.ConstrainedCopy(m_ReceiveBuffer, EOL + 1, temp, 0, remaining);
						m_ReceiveBuffer = temp;
						m_Received = remaining;
					}

					if (!Ready)
					{ // check that the message confirms registration
						Regex RegistrationReply = new Regex(@"^\s?@\s" + m_ID + @"\s\d+;\s?registered\s" + m_ID + @"\s?(more)?\r?\n?", RegexOptions.IgnoreCase | RegexOptions.Singleline);
						// eg "@ SAW-1@CCFCLIENT 1321395918375; registered SAW-1@CCFCLIENT more" // "more" is appended if this is a repeat registration
						if (RegistrationReply.IsMatch(message))
						{
							Ready = true;
							Registered?.Invoke(this, EventArgs.Empty);
							Debug.WriteLine("Registered");
						}
						else
							Debug.Fail("Unexpected reply to registration attempt: " + message);
					}
					else
					{ // already registered - this should be a response to a Query
						Response response = new Response(message);
						QueryReplyEvent replyEvent = new QueryReplyEvent(response);
						if (response.Valid && response.ContainsUsableResponse)
						{
							if (OutstandingQueries.ContainsKey(response.Ref))
							{
								replyEvent.Query = OutstandingQueries[response.Ref];
								OutstandingQueries.Remove(response.Ref);
								Debug.WriteLine("...is reply to outstanding query");
							}
						}
						QueryReply?.Invoke(this, replyEvent);
					}
				}
			}
		}

		#region IDisposable

		private bool m_Disposed;
		public void Dispose()
		{
			if (m_Socket != null && !m_Disposed)
			{
				try
				{
					SendRaw("_ @ unregister", "");
				}
				catch { }
				m_Socket.Close();
				m_Socket = null;
				GC.SuppressFinalize(this);
			}
			m_Disposed = true;
		}

		~Connection()
		{
			Dispose();
		}

		#endregion
	}
}
