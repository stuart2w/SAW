using System.Collections.Generic;
using System;
using System.Diagnostics;
using SAW;
using Switches.Engines;
using Switches.Switching;


namespace Switches
{
	/// <summary>Base class for an instance of a switching engine;  also static methods for overall control</summary>
	public abstract class Engine : IComparable<Engine>, IDisposable
	{

		#region Scan methods, creating instances
		public enum Methods
		{
			AutoSingle,
			UserSingle,
			/// <summary>With switches, manual single.  Mouse version is DwellPointer</summary>
			DwellSelect,
			/// <summary>second switch cancels</summary>
			DwellSelectWithCancel, 
			ManualTwo,
			ManualTwoSingleStep,
			AutoTwo,
			CriticalOverScan,
			DirectPointer,
			DwellPointer,
			DwellAveragePointer
			// if adding methods update Create
		}

		public abstract Methods Method { get; }

		public static Engine Create(Methods method)
		{
			switch (method)
			{
				case Methods.AutoSingle: return new AutoSingle();
				case Methods.UserSingle: return new UserSingle();
				case Methods.DwellSelect: return new DwellSelectEngine();
				case Methods.DwellSelectWithCancel: return new DwellSelectCancelEngine();
				case Methods.ManualTwo: return new ManualEngine(false);
				case Methods.ManualTwoSingleStep: return new ManualEngine(true);
				case Methods.CriticalOverScan: return new CriticalOverScan();
				case Methods.AutoTwo: return new Auto2();
				case Methods.DirectPointer:
				case Methods.DwellPointer:
				case Methods.DwellAveragePointer:
					return new PointerEngine(method);
				default:
					throw new ArgumentException("Unexpected Engine.Methods in Create: " + method);
			}
		}

		// new should not connect to any switches - these can be created for info only.  Connect to switches on Initialise

		protected bool m_Initialised;
		public virtual void Initialise()
		{
			Debug.Assert(!m_Initialised);
			m_Initialised = true;
			Switch1 = Logical.Switch(0);
			Switch1.StateChanged += Switch1Changed;
		}

		public bool Initialised
		{ get { return m_Initialised; } }

		private static Engine[] g_PossibleEngines;
		public static Engine[] PossibleEngines(int numberSwitches)
		{
			if (g_PossibleEngines == null)
			{
				List<Engine> col = new List<Engine>();
				foreach (Methods method in Enum.GetValues(typeof(Methods)))
				{
					col.Add(Create(method));
				}
				col.Sort(); // into alphabetical DisplayName
				g_PossibleEngines = col.ToArray();
			}
			List<Engine> matching = new List<Engine>();
			foreach (Engine engine in g_PossibleEngines)
			{
				if (engine.NumberSwitchInputs == numberSwitches || (engine.NumberSwitchInputs == numberSwitches + 1 && engine.AcceptShortLongSwitches && numberSwitches>0))
					matching.Add(engine);
			}
			return matching.ToArray();
		}
		#endregion

		#region Overall start/stop

		/// <summary>Creates the necessary switching objects from the config settings, and returns the engine object</summary>
		public static Engine PrepareSwitchingFromConfig()
		{
			int numberSwitches = Globals.Root.CurrentConfig.ReadInteger(Config.Number_Switches, 1); // only needed indirectly;  the engine often is enough info, but a 2 switch engine can use 1 with long presses
			PhysicalSwitch.Initialise(); // will be ignored if already done
			var engine = Create((Methods)Globals.Root.CurrentConfig.ReadInteger(Config.Switch_Engine, (int)DefaultMethodForSwitches(numberSwitches)));

			PhysicalSwitch.ClearSwitches();
			if (engine.NumberSwitchInputs >= 1)
				PhysicalSwitch.AssignSwitchFromConfig(0);
			if (engine.NumberSwitchInputs >= 2)
				PhysicalSwitch.AssignSwitchFromConfig(1);

			if (numberSwitches > 0) // for 0, mouse switch will do what is needed on initialise
			{
				if (numberSwitches == 2)
					Logical.CreateSwitches(Logical.Number.Two, engine);
				else if (engine.NumberSwitchInputs == 2)
					Logical.CreateSwitches(Logical.Number.ShortLong, engine);
				else
					Logical.CreateSwitches(Logical.Number.One, engine);
			}

			engine.Initialise();
			return engine;
		}

		public static Methods DefaultMethodForSwitches(int switches)
		{
			switch (switches)
			{
				case 1: return Methods.AutoSingle;
				case 2: return Methods.ManualTwo;
				case 0: return Methods.DirectPointer;
				default:
					Utilities.LogSubError("Unknown switch count in DefaultMethodForSwitches");
					return Methods.AutoSingle;
			}
		}

		#endregion

		#region Onward event declarations and triggers
		/// <summary>Fired when the UI should iterate to a different element</summary>
		public event EventHandler<SingleFieldEventClass<int>> Iterate;

		public delegate void TriggerEventHandler(bool isRepeat);
		/// <summary>Fired when the user has selected an item.  Will fire repeatedly, for some scan types, if the scanning allows repetition by holding, in which case paramater = true.
		/// Always followed, either immediately or delayed by an EndSelect event</summary>
		public event TriggerEventHandler Trigger;

		/// <summary>Invoked when the user has released the switch which triggered and there can be no more repetitions.  For some scan types this just follows the Trigger immediately</summary>
		public event NullEventHandler EndSelect;

		protected int m_Direction = 1;

		/// <summary>External UI can set this to true when at the start of the scan.  If true the ResetExtraTime is used in the ScanTime.  
		/// Automatically resets to false when Iterate is invoked</summary>
		public bool IsAtScanStart { get; set; } = true;

		protected virtual void OnIterate()
		{
			// it is not usually necessary to override this; generally alternative functionality can be put in a second function which calls this
			Debug.WriteLine("Iterate at " + Environment.TickCount);
			IsAtScanStart = false;
			Iterate?.Invoke(this, m_Direction);
			if (IterateCanRepeat && ScanTime > 0)
				m_Timer.Start(ScanTime, OnIterate, Timings.ScanTime);
		}

		protected virtual bool IterateCanRepeat
		{ get { return true; } }

		/// <summary>Should be set to true by any derived engine ONLY DURING TIMES when OnTrigger should start repeating.  Eg set it in response to switch movements</summary>
		protected bool CanStartRepeat;

		/// <summary>Fires trigger.  And if CanRepeat field = true will start the repetitions.  Note CanRepeat is read after the trigger, so it can usefully be changed by external events DURING this function</summary>
		protected void OnTrigger()
		{ 
			Trigger?.Invoke(false);
			if (CanStartRepeat && ConfiguredTiming(Timings.FirstRepeat) > 0)
				m_Timer?.Start(ConfiguredTiming(Timings.FirstRepeat), RepeatTrigger, Timings.FirstRepeat); // must be m_Timer? in case commands close SAW
		}

		protected void OnEndSelect()
		{
			EndSelect?.Invoke();
		}

		protected virtual void RepeatTrigger()
		{
			CanStartRepeat = false; // just in case!
			Trigger?.Invoke(true);
			if (ConfiguredTiming(Timings.SubsequentRepeat) > 0)
				m_Timer.Start(ConfiguredTiming(Timings.SubsequentRepeat), RepeatTrigger, Timings.SubsequentRepeat);
		}

		#endregion

		#region Info
		/// <summary>number of actual inputs required </summary>
		public virtual int NumberSwitchInputs
		{ get { return 1; } }

		/// <summary>only applicable if NumberSwitchInputs >=2;  if true can accept short and long presses to give 2 switches</summary>
		public virtual bool AcceptShortLongSwitches
		{get { return true; }}

		/// <summary>True if this engine can use this number of switches</summary>
		public bool CanAcceptNSwitches(int switches)
		{
			if (switches == NumberSwitchInputs)
				return true;
			if (switches == NumberSwitchInputs - 1 && AcceptShortLongSwitches)
				return true;
			return false;
		}

		public string DisplayName
		{ get { return Strings.Item("Switch_" + Method); } }

		public int CompareTo(Engine other)
		{
			return DisplayName.CompareTo(other.DisplayName);
		}

		public override string ToString()
		{
			return DisplayName;
		}

		#endregion

		#region Switches, timers and other state inputs
		protected Logical Switch1;

		protected abstract void Switch1Changed(bool down);

		protected StateTimer m_Timer = new StateTimer();

		public float ElapsedTimerFraction
		{
			get // for the Display; the fraction of the timer elapsed
			{
				if (m_Timer.Running == false)
					return 0;
				return m_Timer.ElapsedFraction;
			}
		}

		public virtual Logical[] GetInputs()
		{
			return new[] { Switch1 };
		}

		public virtual void StartScanning()
		{
			IsAtScanStart = true;
			PhysicalSwitch.MasterSwitch = PhysicalSwitch.MasterModes.On;
			// some auto scans will start here
		}

		#endregion

		#region Timings will be configurable
		// When stored in configurations, timings should not be in an array; they should be stored in a dictionary

		/// <summary>Note that numbers may be stored in data, and should not be changed; names may be used in translation look up, so should not be changed
		/// first to not really used by the engines; these are used by the actual switches </summary>
		[Flags]
		public enum Timings
		{
			/// <summary>time the switch must be held for to activate </summary>
			AcceptanceTime = 1,
			/// <summary>time after a switch is released during which cannot be pressed again.  Some engines might use this </summary>
			PostActivation = 2,
			LongPress = 4,
			ScanTime = 8,
			FirstRepeat = 16,
			SubsequentRepeat = 32,
			/// <summary>the trigger time for dwell select </summary>
			DwellSelect = 64,
			CriticalReverse = 128,
			/// <summary>As a percentage of the ScanTime</summary>
			RestartExtraTime = 256
		}

		public virtual int DefaultTiming(Timings timing)
		{
			// default for the given timing for this engine.  Can be overridden by individual engines; they should call through to this for any values they are not interested in
			// Note that this will return a default value for the timing is not used by this engine
			switch (timing)
			{
				case Timings.AcceptanceTime:
					return 0; // Logical.StandardAcceptanceTime; // note these get updated by GUI - bit tangled at the mo
				case Timings.PostActivation:
					return 100; // Logical.PostActivationTime;
				case Timings.LongPress:
					return 1000;
				case Timings.ScanTime:
					return 800;
				case Timings.FirstRepeat:
					return 3600;
				case Timings.SubsequentRepeat:
					return 1800;
				case Timings.DwellSelect:
					return 1500;
				case Timings.CriticalReverse:
					return 1200;
				default:
					Debug.Fail("Unexpected timing in DefaultTiming: " + timing);
					return 1000;
			}
		}

		/// <summary>Returns the config field for the given timing</summary>
		public static string TimingConfigField(Timings timing)
		{
			return "Timing_" + timing;
		}

		/// <summary>Returns the current value of the given timing from the current settings</summary>
		public int ConfiguredTiming(Timings timing)
		{
			return Globals.Root.CurrentConfig.ReadInteger(TimingConfigField(timing), DefaultTiming(timing));
		}

		/// <summary>individual engines must override this to specify which values they are interested in.  Add on AlwaysRelevantTimings to get complete set</summary>
		public abstract Timings RelevantTimings { get; }

		// These look up the timings used by base class functions, where it might be necessary to use a different one of the above values
		protected virtual int ScanTime
		{ get { return ConfiguredTiming(Timings.ScanTime); } }

		#endregion

		#region Disposable
		private bool m_Disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (m_Disposed) return;
			if (disposing)
			{
				if (m_Timer != null)
				{
					m_Timer.Dispose();
					m_Timer = null;
				}
				if (Switch1 != null)
				{
					Switch1.StateChanged -= Switch1Changed;
					Switch1 = null; // note we don't dispose the switch - it isn't owned by this
									// but we do dereference it to make sure nothing is held in memory
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
			m_Disposed = true;
			GC.SuppressFinalize(this);
		}

		~Engine()
		{
			Dispose(false);
		}

		#endregion

	}

}
