using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SAW;
using System.Windows.Forms;
using SAW.Shapes;

namespace Switches.Switching
{
	/// <summary>Behaves partly as a physical switch, but is using the pointer to directly click in the work area, not intercepting the mouse generally.  Not selectable by user.</summary>
	public abstract class PointerSwitch : PhysicalSwitch
	{
		public Engine Engine;

		#region Switch lifecycle, view connection

		/// <summary>The view to work on.  Set as a static property for convenience.  Must be set BEFORE switches initialised (via engine initialisation)
		/// Changes to this will have no effect unless switches destroyed and recreated at the moment, although that could be changed</summary>
		internal static RunView View { get; set; }

		public static PhysicalSwitch Create(int param)
		{
			if (View == null)
				throw new InvalidOperationException("PointerSwitch.View not assigned");
			Engine.Methods method = (Engine.Methods)param;
			switch (method)
			{
				case Engine.Methods.DirectPointer:
					return new PointerClickSwitch();
				case Engine.Methods.DwellPointer:
					return new PointerDwellSwitch();
				case Engine.Methods.DwellAveragePointer:
					return new PointerDwellAverageSwitch();
				default: throw new ArgumentException(nameof(param));
			}
		}

		protected PointerSwitch()
		{
			View.MouseMove += View_MouseMove;
			View.MouseLeave += View_MouseLeave;
		}

		public override void Destroy()
		{
			View.MouseMove -= View_MouseMove;
			View.MouseLeave -= View_MouseLeave;
		}

		#endregion

		#region Events and hit tracking

		protected Scriptable m_Current;
		private void View_MouseMove(object sender, MouseEventArgs e)
		{
			if (View == null) // can happen if this event was queued from before window closed
				return;
			Scriptable hit = View.HitTest(e.Location);
			if (hit != m_Current)
			{
				m_Current = hit;
				OnCurrentChanged();
			}
		}

		private void View_MouseLeave(object sender, EventArgs e)
		{
			if (m_Current == null)
				return;
			m_Current = null;
			OnCurrentChanged();
		}


		/// <summary>Called when m_Current changes</summary>
		protected virtual void OnCurrentChanged()
		{
			View?.SelectItem(m_Current, true);
		}

		#endregion

		#region Info
		/// <summary>Param not applicable here</summary>

		public override string ParamDescription => "";
		public override Types Type => Types.Pointer;

		#endregion

	}

	public class PointerClickSwitch : PointerSwitch
	{
		public PointerClickSwitch()
		{
			View.MouseDown += View_MouseDown;
			View.MouseUp += View_MouseUp;
		}

		public override void Destroy()
		{
			base.Destroy();
			View.MouseDown -= View_MouseDown;
			View.MouseUp -= View_MouseUp;
		}

		private void View_MouseUp(object sender, MouseEventArgs e)
		{
			OnStateChange(false);
		}

		private void View_MouseDown(object sender, MouseEventArgs e)
		{
			if (m_Current != null)
				OnStateChange(true);
		}

		public override int Param => (int)Engine.Methods.DirectPointer;
	}

	public class PointerDwellSwitch : PointerSwitch
	{// will be attached to a LogicalDwell which does the timings.  Must just trigger on entry to an item
		public PointerDwellSwitch()
		{ }

		protected override void OnCurrentChanged()
		{
			base.OnCurrentChanged();
			OnStateChange(false);  // always send an UP to cancel any previous dwell
			if (m_Current != null)
				OnStateChange(true);
		}

		public override int Param => (int)Engine.Methods.DwellPointer;

	}

	/// <summary>Switch for dwell averaging.  Requires an outgoing logical switch with no delay - dwell time is done internally
	/// Requires a reference to the engine to be supplied after construction</summary>
	public class PointerDwellAverageSwitch : PhysicalSwitch
	{ // doesn't derive from PointerSwitch as the base class stuff isn't especially useful

		public Engine Engine;

		#region Switch life-cycle

		public PointerDwellAverageSwitch()
		{
			MasterSwitchChanged += PhysicalSwitch_MasterSwitchChanged;
			PointerSwitch.View.MouseLeave += View_MouseLeave;
			PointerSwitch.View.MouseMove += View_MouseMove;
			m_Timer = new Timer { Interval = 1 };
			m_Timer.Tick += m_Timer_Tick;
		}

		public override void Destroy()
		{
			MasterSwitchChanged -= PhysicalSwitch_MasterSwitchChanged;
			PointerSwitch.View.MouseLeave -= View_MouseLeave;
			PointerSwitch.View.MouseMove -= View_MouseMove;
			m_Timer.Tick -= m_Timer_Tick;
		}

		#endregion

		#region Position and averaging

		private Timer m_Timer;
		/// <summary>Item that mouse is literally directly over </summary>
		private Scriptable m_MouseOver;
		/// <summary>Accumulated time for each item</summary>
		private Dictionary<Scriptable, int> m_Timings = new Dictionary<Scriptable, int>();
		/// <summary>Last tick time that was processed</summary>
		private int m_LastTick;
		/// <summary>Set when switch is clicked, allows repeats? If this is defined then the switch state must be down, and vice versa</summary>
		private Scriptable m_Clicked;

		private Scriptable m_Selected;

		private void View_MouseMove(object sender, MouseEventArgs e)
		{
			m_MouseOver = PointerSwitch.View.HitTest(e.Location);
			if (m_Clicked != null && m_Clicked != m_MouseOver)
			{
				OnStateChange(false);
				m_Clicked = null;
				m_Selected = null; // because UI will probably deselect?
			}
		}

		private void View_MouseLeave(object sender, EventArgs e)
		{
			m_MouseOver = null;
			if (m_Clicked != null)
				OnStateChange(false);
			m_Clicked = null;
		}

		private void m_Timer_Tick(object sender, EventArgs e)
		{ // fired rapidly.  Works independently of MouseMove which will update m_MouseOver (when mouse actually moves!)
			if (Engine == null)
				throw new InvalidOperationException("PointerDwellAverageSwitch.Engine was not assigned after construction");
			int elapsed = Environment.TickCount - m_LastTick;
			m_LastTick = Environment.TickCount;
			if (elapsed < 10) elapsed = 10;
			if (elapsed > 500) elapsed = 500; // if Windows was very slow responding, don't suddenly apply a huge amount of time to one button
			if (m_MouseOver != null)
			{
				if (!m_Timings.ContainsKey(m_MouseOver))
					m_Timings.Add(m_MouseOver, elapsed);
				else
				{
					m_Timings[m_MouseOver] += elapsed;
					if (m_Timings[m_MouseOver] > Engine.ConfiguredTiming(Engine.Timings.DwellSelect))
					{
						m_Clicked = m_MouseOver;
						OnStateChange(true);
						m_Timings.Clear();
					}
				}
			}
			Scriptable longest = m_Timings.GetMaxKey();
			Debug.WriteLine("Mouseover = " + (m_MouseOver?.Description ?? "null"));
			Debug.WriteLine("Longest = " + (longest?.Description ?? "null") + " with " + (longest == null ? "" : m_Timings[longest].ToString()));
			if (longest != m_Selected && longest != null)
			{
				PointerSwitch.View.SelectItem(longest, true);
				m_Selected = longest;
			}
			foreach (Scriptable item in m_Timings.Keys.ToList()) // tolist so we can amend the original
			{
				if (item == m_MouseOver)
					continue;
				int time = m_Timings[item];
				time -= elapsed * 3;
				if (time < 0)
				{
					m_Timings.Remove(item);
					if (item == m_Selected) // presumably mouse outside screen.  No selection at all any longer
					{
						PointerSwitch.View.SelectItem(null, true);
						m_Selected = null;
					}
				}
				else
					m_Timings[item] = time;
			}
		}

		private void PhysicalSwitch_MasterSwitchChanged()
		{
			m_Timer.Enabled = (MasterSwitch == MasterModes.On);
		}

		#endregion

		#region Info 
		public override int Param => (int)Engine.Methods.DwellAveragePointer;
		public override string ParamDescription => Strings.Item("Switch_DwellAveragePointer");
		public override Types Type => Types.Pointer;

		#endregion

	}

}
