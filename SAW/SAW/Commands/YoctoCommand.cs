using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SAW.CommandEditors;
using SAW.Commands;

namespace SAW.Commands
{
	/// <summary>The Yocto_Relay command which does all functions on YoctoPuce relays.
	/// Also contains some static util functions for Yocto</summary>
	/// <remarks>Unlike most commands this keeps all the relevant info in member fields as well as the params</remarks>
	public class CmdYocto : Command
	{
		/// <summary>ID of the relay to operate on; can be "All" or a numeric ID</summary>
		public string RelayID;
		public Commands Command;
		/// <summary>Time for pulse or status window</summary>
		public int Millisecond = 1000;

		public CmdYocto()
		{
			int i = 0;
		}

		public enum Commands
		{
			/// <summary>The script command just contains a numeric time in this case.  "Pulse" never appears in scripts</summary>
			Pulse,
			On,
			Off,
			Toggle,
			/// <summary>This is the "?" command which displays a status window</summary>
			Query
		}

		#region Script and editing

		public override void InitialiseDefaultsForCreation()
		{
			base.InitialiseDefaultsForCreation();
			Command = Commands.Pulse;
			Millisecond = 1000;
		}

		protected override void InitialiseFromParams(List<string> possibleParams, string commandUsed)
		{ // generally ID, command.  But ? command can also have an optional time
			base.InitialiseFromParams(possibleParams, commandUsed);
			if (possibleParams.Count < 2)
				throw new UserException(Strings.Item("Script_Error_TooFewParameters").Replace("%0", GetCommandName()));
			if (possibleParams.Count > 3)
				throw new UserException(Strings.Item("Script_Error_TooManyParameters").Replace("%0", GetCommandName()));
			m_ParamList.Add(new StringParam(possibleParams[0]));
			m_ParamList.Add(new StringParam(possibleParams[1], false));
			if (possibleParams.Count == 3 && int.TryParse(possibleParams[2], out _))
				m_ParamList.Add(new IntegerParam(int.Parse(possibleParams[2])));
			ParseParams();
		}

		/// <summary>Reads the member fields from the params</summary>
		private void ParseParams()
		{
			if (m_ParamList.Count > 1) // conditions shouldn't really be needed, but safer with (and it happened during dev)
				RelayID = m_ParamList[0].ValueAsString();
			if (m_ParamList.Count >= 2)
				switch (m_ParamList[1].ValueAsString().ToLowerInvariant())
				{
					case "on": Command = Commands.On; break;
					case "off": Command = Commands.Off; break;
					case "toggle": Command = Commands.Toggle; break;
					case "?":
						Command = Commands.Query;
						if (m_ParamList.Count == 3)
						{
							if (!int.TryParse(m_ParamList[2].ValueAsString(), out Millisecond) || Millisecond < 0) // note Millisecond set by first part of condition
								throw new UserException("[Script_Error_BadYoctoTime]");
						}
						else
							Millisecond = 5000;
						break;
					default:
						if (!int.TryParse(m_ParamList[1].ValueAsString(), out Millisecond))
							throw new UserException("[Script_Error_UnknownYocto]");
						if (Millisecond <= 0)
							throw new UserException("[Script_Error_BadYoctoTime]");
						Command = Commands.Pulse;
						break;
				}
		}

		public override ICommandEditor GetEditor() => new YoctoEditor();

		/// <summary>Resets the param list based on the properties of this object </summary>
		public void UpdateParams()
		{
			m_ParamList.Clear(); //simplest just to delete and recreate.  Efficiency is irrelevant as this will only be used in response to UI events for one command
			m_ParamList.Add(new StringParam(RelayID));
			switch (Command)
			{
				case Commands.Pulse:
					m_ParamList.Add(new IntegerParam(Millisecond));
					break;
				case Commands.On:
				case Commands.Off:
				case Commands.Toggle:
					m_ParamList.Add(new StringParam(Command.ToString().ToLowerInvariant(), false));
					break;
				case Commands.Query:
					m_ParamList.Add(new StringParam("?", false));
					m_ParamList.Add(new IntegerParam(Millisecond));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Data

		protected override void Read(DataReader reader)
		{ // no need to override ArchiveReader as these didn't exist in SAW6!
			base.Read(reader);
			ParseParams();
		}

		public override Command Clone()
		{
			var created = (CmdYocto)base.Clone();
			created.Command = Command;
			created.RelayID = RelayID;
			created.Millisecond = Millisecond;
			return created;
		}

		#endregion

		public override void Execute(ExecutionContext context)
		{
			RegisterYocto(false); // at run time this is the first place a relay would have been referenced, so this would not have been done yet
			IEnumerable<YRelay> relays = FindRelay(RelayID);
			if (!relays.Any())
			{
				context.View.ShowReportWindow(Strings.Item("Script_Error_YoctoNotFound", RelayID), true);
				return;
			}
			if (Command == Commands.Query) // if multiple relays this can't be called separately for each!
			{
				StringBuilder message = new StringBuilder();
				foreach (YRelay r in relays)
					message.Append(r.SAWName).Append(" : ").AppendLine(r.StateName);
				context.View.ShowReportWindow(message.ToString(), false, Millisecond);
				return;
			}
			foreach (YRelay relay in relays)
				switch (Command)
				{ // STATE_A is off, STATE_B is on
					case Commands.Pulse:
						relay.set_state(YRelay.STATE_A); // force it to off first, cancelling any pulse
						relay.pulse(Millisecond);
						break;
					case Commands.On:
						relay.StateActive = true;
						break;
					case Commands.Off:
						relay.StateActive = false;
						break;
					case Commands.Toggle:
						relay.StateActive = !relay.StateActive;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
		}

		public const string ALLID = "All";
		public bool IsAll => string.Equals(RelayID, ALLID, StringComparison.InvariantCultureIgnoreCase);

		#region Static utilities, inc for Yocto generally

		public static bool CommandUsesTime(Commands command) => command == Commands.Pulse || command == Commands.Query;

		private static bool g_RegisteredOK;
		/// <summary>Calls RegisterHub.  Doesn't call again once it has succeeded once (although Yocto doesn't seem to mind).  Returns true on success (or previous success)
		/// throwException is used if it fails - if false MessageBox.Show, if true throws exception.  There is no silent failure option</summary>
		public static bool RegisterYocto(bool throwException)
		{
			if (g_RegisteredOK)
				return true;
			string error = "";
			int result = YAPI.RegisterHub("usb", ref error);
			if (result == YAPI.SUCCESS)
				error = null; // leave field as null for OK (maybe Yocto returns an OK or something in the error)

			if (error != null)
			{
				if (throwException)
					throw new UserException(error);
				MessageBox.Show(error);
				return false;
			}
			g_RegisteredOK = true;
			return true;
		}

		/// <summary>Enumerates connected relays.  RegisterYocto must have been called first</summary>
		public static IEnumerable<YRelay> IterateRelays()
		{
			YRelay relay = YRelay.FirstRelay();
			while (relay != null)
			{
				yield return relay;
				relay = relay.nextRelay();
			}
		}

		/// <summary>Finds the relay from the ID as used in commands - but returns enumerable as All will return all relays.
		/// Text ID can either be the logical name or friendly name of the relay.  (Former is used in SAW editing, EXCEPT when empty - which is the default)
		/// Returns zero elements if not found.
		/// RegisterYocto must have been called before this</summary>
		public static IEnumerable<YRelay> FindRelay(string id)
		{
			if (id.Equals(ALLID, StringComparison.InvariantCultureIgnoreCase))
				return IterateRelays();
			YRelay[] relays = IterateRelays().ToArray(); // integer ID will need an array, and we may as well fetch it as text ID will need to search them
			int index;
			if (int.TryParse(id, out index))
			{  // if numeric it is a 1-based index into list - although no guarantee this will remain unchanged
				if (index <= 0 || index > relays.Length)
					return new YRelay[] { };
				return relays.Skip(index - 1).Take(1);
			}
			// must check serial and logical names
			// but also checks friendly - mainly intended for when logical hasn't been set; in which case friendly is displayed in editor
			return relays.Where(r => (!string.IsNullOrWhiteSpace(id) && r.get_logicalName().Equals(id, StringComparison.InvariantCultureIgnoreCase))
				|| r.FriendlyName.Equals(id, StringComparison.InvariantCultureIgnoreCase)
				|| r.get_serialNumber().Equals(id, StringComparison.InvariantCultureIgnoreCase));
		}

		#endregion

	}
}
