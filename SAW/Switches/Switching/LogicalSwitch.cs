using System.Collections.Generic;
using System;
using SAW;

namespace Switches.Switching
{
	public delegate void StateEventHandler(bool down);

	/// <summary>Represents a usual switch, has acceptance time and post activation delay.  Triggered on the falling edge, after acceptance time
	/// also base class for the other types, which override bits as needed</summary>
	public class Logical : ISwitch
	{

		public ISwitch Source;

		protected readonly Engine m_Engine;

		public event StateEventHandler StateChanged;

		public Logical(ISwitch source, Engine engine)
		{
			Source = source;
			Source.StateChanged += OnInputStateChanged;
			m_Engine = engine;
		}

		#region Timings

		protected virtual int AcceptanceTime => m_Engine.ConfiguredTiming(Engine.Timings.AcceptanceTime);

		public virtual Engine.Timings RequiredTimings => Engine.Timings.AcceptanceTime | Engine.Timings.PostActivation;

		#endregion

		#region Protected state model
		protected StateTimer m_Timer = new StateTimer();
		protected bool m_State = false;
		protected bool m_Cooldown = false; // true if in postactivation, which just ignores the switch

		protected virtual void Activate()
		{
			if (m_State)
				return;
			OnStateChanged(true);
			// any repeating done in Engine
		}

		protected virtual void CooledDown()
		{
			// PostActivation has elapsed; switch is now active again.  No change to public state
			m_Cooldown = false;
		}

		protected void OnStateChanged(bool down)
		{
			m_State = down;
			StateChanged?.Invoke(m_State);
		}

		protected virtual void OnInputStateChanged(bool down)
		{
			if (down)
			{
				if (!m_Cooldown && m_State == false)
					m_Timer.Start(AcceptanceTime, Activate);
			}
			else
			{
				if (m_State)
				{
					OnStateChanged(false);
					m_Cooldown = true;
					m_Timer.Start(m_Engine.ConfiguredTiming(Engine.Timings.PostActivation), CooledDown);
				}
				else if (!m_Cooldown)
					m_Timer.Cancel(); // stop the acceptance timer
			}
		}

		#endregion

		#region Public state model

		public bool State => m_State;

		public virtual void Abandon()
		{
			// If the switch is currently down, or waiting for Acceptance time, this aborts
			// the cooldown/post-activation is not affected
			if (m_Cooldown)
				return;
			m_Timer.Cancel();
			if (m_State)
				OnStateChanged(false);
		}

		public float ElapsedTimerFraction => m_Timer.Running == false ? 0 : m_Timer.ElapsedFraction;  // for the Display; the fraction of the timer elapsed

		public virtual int RecommendWaitTrigger => 0;
		// if >0 recommends that the engine wait this long before Triggering - used by ShortLong to sometimes
		// wait to detect if a current press is actually a Long press which is so far incomplete

		#endregion

		#region Global list
		public enum Number
		{
			One,
			Two,
			/// <summary>single physical switch acting as two</summary>
			ShortLong,
			/// <summary>Only used for DirectPointer.  Attaches just a LogicalDwell to physical[0] - ie uses dwell time not acceptance time</summary>
			DwellOnly,
			/// <summary>One switch which passes thru directly without timers </summary>
			PassThrough
		}
		private static List<Logical> Switches;

		public static Logical Switch(int index)
		{
			if (Switches == null)
				return null;
			if (index >= Switches.Count)
				return null;
			return Switches[index];
		}

		public static int Count => Switches.Count;

		public static void CreateSwitches(Number number, Engine engine)
		{
			Switches = new List<Logical>();
			var switch0 = PhysicalSwitch.Switch(0);
			switch (number)
			{
				case Number.One:
					Switches.Add(new Logical(switch0, engine));
					break;
				case Number.Two:
					var switch1 = PhysicalSwitch.Switch(1);
					if (Globals.Root.CurrentConfig.ReadBoolean(Config.Reverse_Switches) || Globals.Root.CurrentConfig.ReadBoolean(Config.Use_Swap_Switch))
					{ // feed switches through the swap logic:
						Switches.Add(new Logical(new LogicalSwapper(switch0, switch1, engine), engine));
						Switches.Add(new Logical(new LogicalSwapper(switch1, switch0, engine), engine));
					}
					else
					{
						Switches.Add(new Logical(switch0, engine));
						Switches.Add(new Logical(switch1, engine));
					}
					break;
				case Number.ShortLong:
					var rising = new LogicalRising(switch0, engine);
					Switches.Add(rising);
					Switches.Add(new LogicalLong(switch0, rising, engine));
					break;
				case Number.DwellOnly:
					Switches.Add(new LogicalDwell(switch0, engine));
					break;
				case Number.PassThrough:
					Switches.Add(new LogicalPassThrough(switch0, engine));
					break;
				default:
					throw (new ArgumentException("Logical.SetSwitches: invalid number"));
			}
		}

		public string GetName()
		{
			int index = Switches.IndexOf(this);
			return TypeName + " #" + (index + 1);
		}

		protected virtual string TypeName => "Standard filter";

		#endregion

	}
}
