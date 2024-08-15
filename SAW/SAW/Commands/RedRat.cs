using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedRat;
using RedRat.AvDeviceDb;
using RedRat.IR;
using RedRat.RedRat4.Usb.Rp2040;
using RedRat.Usb;
using RedRat.Util.Serialization;
using SAW.CommandEditors;

namespace SAW.Commands
{
	public class CmdRedRatSend : ParamBasedCommand
	{
		public CmdRedRatSend() : base(new[] { Param.ParamTypes.String, Param.ParamTypes.String })
		{ }

		/// <summary>Name of device.  Is Param 0 - convenience method for simpler access</summary>
		public string Device
		{
			get { return GetParamAsString(0); }
			set { ParamList[0] = new StringParam(value); }
		}

		/// <summary>Name of signal.  Is Param 1 - convenience method for simpler access</summary>
		public string Signal
		{
			get { return GetParamAsString(1); }
			set { ParamList[1] = new StringParam(value); }
		}

		internal override void Execute(ExecutionContext context)
		{
			RedRatSupport.Prepare();
			if (!RedRatSupport.HasDevice)
				throw new UserException("[Script_Error_RedRatNoDevice]");
			IRPacket signal = RedRatSupport.GetSignal(Device, Signal); //throws sensible error if not found
			RedRatSupport.Pico.OutputModulatedSignal(signal);
		}

		internal override ICommandEditor GetEditor() => new RedRatSendEditor();

	}

	public class CmdRedRatColour : ParamBasedCommand
	{
		// Param Integer can only store 16-bit so can't store a colour code.  So we use 3 params for R, G, B

		public CmdRedRatColour() : base(new[] { Param.ParamTypes.Integer, Param.ParamTypes.Integer, Param.ParamTypes.Integer })
		{ 
		}

		public Color Colour
		{
			get { return Color.FromArgb(GetParamAsInt(0), GetParamAsInt(1), GetParamAsInt(2)); }
			set
			{
				ParamList.Clear();
				ParamList.Add(new IntegerParam(value.R));
				ParamList.Add(new IntegerParam(value.G));
				ParamList.Add(new IntegerParam(value.B));
			}
		}

		internal override void InitialiseDefaultsForCreation()
		{
			Colour = Color.Red; // default colour to use for new command.  Otherwise it is black, which looks a bit odd!
		}

		internal override void Execute(ExecutionContext context)
		{
			RedRatSupport.Prepare();
			if (!RedRatSupport.HasDevice)
				throw new UserException("[Script_Error_RedRatNoDevice]");
			RedRatSupport.Pico.LedColor = Colour;
		}

		internal override ICommandEditor GetEditor() => new RedRatColourEditor();

	}

	/// <summary>sets the power on the 2 channels.  Both params should be 0-100</summary>
	public class CmdRedRatStrength : ParamBasedCommand
	{

		public CmdRedRatStrength() : base(new[] { Param.ParamTypes.Integer, Param.ParamTypes.Integer })
		{ 

		}

		internal override void Execute(ExecutionContext context)
		{
			RedRatSupport.Prepare();
			if (!RedRatSupport.HasDevice)
				throw new UserException("[Script_Error_RedRatNoDevice]");
			RedRatSupport.Pico.EnableIROutput(1, GetParamAsInt(0).LimitTo(0, 100));
			RedRatSupport.Pico.EnableIROutput(2, GetParamAsInt(1).LimitTo(0, 100));
		}

		internal override void InitialiseDefaultsForCreation()
		{
			EnsureParams(2);
			ParamList[0] = new IntegerParam(100);
		}

		internal override ICommandEditor GetEditor() => new RedRatStrengthEditor();

	}

	public static class RedRatSupport
	{
		/// <summary>The signal database.  Loaded by Prepare.  Kept in memory as a global once used</summary>
		public static AVDeviceDB SignalDatabase;
		/// <summary>Device - just first found. Defined by Prepare.  Will be null still if none found.  (Check if SignalDatabase == null to check if Prepare is done, dont check if this is null)</summary>
		internal static RedRatPico Pico;

		/// <summary>Loads the signal database if not already loaded.  Safe to call again - will just ignore unnecessary calls</summary>
		public static void Prepare()
		{
			if (SignalDatabase != null)
				return;
			SignalDatabase = Serializer.AvDeviceDbFromXmlFile(Path.Combine(Globals.Root.InternalFolder, "RedRatDeviceDB.xml")).Object;
			IEnumerable<UsbLocationInfo> devices = RedRatPico.FindDevices().OfType<UsbLocationInfo>();
			if (devices.Any())
			{
				Pico = RedRatPico.GetInstance(devices.First());
				Pico.Connect();
			}
		}

		public static IRPacket GetSignal(string deviceName, string signalName)
		{
			Prepare();
			IRPacket sig = SignalDatabase.GetAVDevice(deviceName).GetSignal(signalName);
			return sig ?? throw new UserException(Strings.Item("Script_Error_RedRatSignalNotFound", $"{deviceName}->{signalName}"));
		}

		public static bool HasDevice => Pico != null;

	}
}
