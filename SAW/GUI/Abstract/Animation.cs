using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

// Animation and Colour blending controls - change colour smoothly etc
// a global list is kept and only one animation can be active on a given target at a time

namespace SAW
{
	#region Interfaces implemented by objects which are animated
	public interface IBlendableColourDisplay
	{
		// the colour might be either the foreground or the background - the control can decide which it uses
		Color VariableColour { get; set; }
		// the Set method should call Invalidate within a control.  When implementing this remove it from DesignerSerialisationVisibility
	}

	public interface ILinearAnimated
	{
		int VariableValue { get; set; }
	}

	public interface IToggleAnimated
	{
		bool ToggleState { get; set; }
		// "off" should be the resting state, if this needs to finish in a particular state after a particular number of iterations
	}

	public interface IAnimationNotifyComplete
	{
		void Complete();
	}
	#endregion

	internal abstract class AnimationController : IDisposable
	{

		protected int m_Step = 16;
		protected object m_Target; // probably, but NOT NECESSARILY a control, ditto IBlendableControl
		protected int m_OriginalRepetitions;
		protected int m_RepetitionsRemaining;
		protected bool m_Active;

		public const int INFINITEREPETITIONS = int.MaxValue; // xxxx -1 is so that it is an even number.  This will force it to return to the starting state at the end

		protected AnimationController(int step, int repetitions)
		{
			if (step != 0)
				m_Step = step;
			m_OriginalRepetitions = repetitions;
		}

		// each derived class should have its own Start method whose parameters will vary depending on what it does
		protected void SharedStart()
		{
			// the Start functions in the individual classes should call this at the END of the function
			Debug.Assert(m_Target != null && !m_Disposed);
			m_RepetitionsRemaining = m_OriginalRepetitions;
			if (!g_List.ContainsKey(m_Target))
			{
				//in case this has been terminated and then restarted
				g_List.Add(m_Target, this);
			}
			m_Active = true;
			g_LastTick = Environment.TickCount;
			GlobalTimer.Enabled = true;
		}

		public void Abandon()
		{
			// stops the animation, abandoning the current display in its current state
			if (m_Disposed)
				return;
			m_RepetitionsRemaining = 0;
			m_Active = false;
			if (g_List.ContainsKey(m_Target))
				g_List.Remove(m_Target);
		}

		public void Conclude()
		{
			// forces the animation to into its concluded state and then stops it
			if (m_Disposed)
				return;
			if (m_RepetitionsRemaining % 2 == 0)
			{
				// must make sure it iterates to the the initial position
				DoIterate(int.MaxValue / 4); // we ignore the bolNeedsRefresh and just assume it is the case for this (very unlikely we want to not refresh when returning it to its basic state)
				Reverse();
			}
			m_RepetitionsRemaining = 0;
			if (Active)
				DoIterate(int.MaxValue / 4); // move into the end state
			if (m_Target is IAnimationNotifyComplete)
				((IAnimationNotifyComplete)m_Target).Complete();
			m_Active = false;
			if (g_List.ContainsKey(m_Target))
				g_List.Remove(m_Target);
		}

		private bool Iterate(float relativeStep) // returns true if it wishes to continue
		{
			// must not modify g_List during this function, because this can be called while iterating g_List
			// relativeStep can be used to increase steps if its running slowly
			Debug.Assert(relativeStep >= 1);
			if (m_Step == 0)
				m_Step = 16; // just in case
			var returnValue = true;
			if (!DoIterate((int)(m_Step * relativeStep)))
			{
				// has finished doing one change
				if (m_RepetitionsRemaining > 0)
				{
					m_RepetitionsRemaining -= 1;
					Reverse();
				}
				else
				{
					m_Active = false;
					returnValue = false;
					(m_Target as IAnimationNotifyComplete)?.Complete();
					//bolNeedsRefresh = False	' container may not like refreshing an animation which it has already been told is complete
				}
			}
			return returnValue;
		}

		public bool Active => m_Active;

		protected abstract bool DoIterate(int step); // must return false if finished, true otherwise
		protected abstract void Reverse();

		#region Shared list and global timer
		// generally these are created using the Create function, and the list ensures that there is only one per target control.
		// the controllers delete themselves from the list when they complete.  However calling Start again will restart them and add them back to the list
		// if these are used to respond to mouse hover the control can probably most efficiently keep a reference to the AnimationController and keep reusing the same one
		private static Dictionary<object, AnimationController> g_List = new Dictionary<object, AnimationController>();
		protected static Timer GlobalTimer = new Timer();
		private static int g_LastTick = 0; // used to check time between Ticks - increases step size if it's running slowly
		private const int INTERVAL = 40; // intended ms between steps

		static AnimationController()
		{
			GlobalTimer.Interval = INTERVAL;
			GlobalTimer.Enabled = false;
			GlobalTimer.Tick += Tick;
			g_LastTick = Environment.TickCount;
		}

		internal static void Tick(object sender, EventArgs e)
		{
			bool active = false;
			List<AnimationController> current = new List<AnimationController>(); // iterate a separate list to protect against changes while iterating
			current.AddRange(g_List.Values);
			float step = 1;
			try
			{
				int interval = Environment.TickCount - g_LastTick;
				if (interval > INTERVAL * 2)
					step = interval / (INTERVAL * 1.25f); // running too slow - increase step size
			}
			catch // can (rarely) give an overflow exception if the machine was hibernated for a long time
			{
				step = 1;
			}
			//Debug.WriteLine("Animation relative step = " + sngStep.ToString + ".  Active count = " + colCurrent.Count.ToString)
			if (step > 4)
			{
				//Debug.WriteLine("Animation wanted relative step > 4 - TOO SLOW!? or interval mis-calculated")
				step = 4;
			}
			foreach (AnimationController animation in current)
			{
				if (animation.Active)
				{
					bool remove = animation.m_Target == null;
					if (g_List.ContainsKey(animation.m_Target)) // checks it hasn't been removed tweentimes
					{
						if (!remove && animation.Iterate(step)) // Iterate returns true to continue
																// block changes during
							active = true;
						else
							g_List.Remove(animation.m_Target);
					}
				}
			}
			GlobalTimer.Enabled = active;
			g_LastTick = Environment.TickCount;
		}

		internal static void EnsureNoAnimation(object target, bool abandon = false)
		{
			// ensures that there is no animation on the given target, and terminates it if there is
			// it is either abandoned or concluded depending on the second parameter
			if (g_List.ContainsKey(target))
			{
				AnimationController animation = g_List[target];
				g_List.Remove(target); // needs to be done first, because sometimes the Conclude function in the GUI might be shared between natural and forced conclusion
									   // and might call back to this function
				if (!abandon)
					animation.Conclude(); // will remove it from the list
				else
					animation.Abandon();
				animation.Dispose();
			}
		}

		internal static bool HasAnimation(object objTarget) => g_List.ContainsKey(objTarget);
		internal static bool Exists(object ctrTarget) => g_List.ContainsKey(ctrTarget);
		internal static AnimationController Item(object ctrTarget) => g_List[ctrTarget];

		internal static AnimationController GetAnimation(object objTarget)
		{
			if (!g_List.ContainsKey(objTarget))
				return null;
			return g_List[objTarget];
		}

		protected static void Add(AnimationController objController)
		{
			g_List.Add(objController.m_Target, objController);
		}

		// any derived class should implement a suitable Create function
		#endregion

		#region IDisposable Support
		private bool m_Disposed = false;
		public void Dispose()
		{
			// does not necessarily put the GUI into any particular state - this is mainly used if the GUI itself is being disposed
			if (!m_Disposed)
			{
				if (g_List.ContainsKey(m_Target))
				{
					g_List.Remove(m_Target);
				}
				m_Target = null;
			}
			m_Disposed = true;
			GC.SuppressFinalize(this);
		}
		#endregion

	}

	internal class AnimationColourChange : AnimationController
	{
		// these are used to change the colours in a IBlendableControl smoothly

		private Color m_TargetColour; // = System.Drawing.Color.Empty;
		private Color m_Original = Color.Empty;

		public const int STANDARDSTEP = 16;
		public const int SLIGHTCOLOURCHANGESTEP = 4; // number of iterations suitable for response to mouse hover which is only a small change
		public const int SLOWCOLOURCHANGESTEP = 2; // number of iterations suitable for big colour change, but slowly
		public const int INDEFINITEREPEATS = 999;
		public const int FLASHCONTROLSTEP = 8;

		private AnimationColourChange(Color newColour, int step, int repeats) : base(step, repeats)
		{
			m_TargetColour = newColour;
		}

		private static AnimationColourChange Create(IBlendableColourDisplay target, Color newColour, int step = 16, int repeats = 0)
		{
			if (Exists(target))
				return Item(target) as AnimationColourChange;
			Debug.Assert(repeats == 0 || repeats % 2 == 1);
			AnimationColourChange animation = new AnimationColourChange(newColour, step, repeats) { m_Target = target };
			Add(animation);
			return animation;
		}

		public void Start()
		{
			m_Original = Target.VariableColour;
			base.SharedStart();
		}

		protected override bool DoIterate(int step)
		{
			Color col = Target.VariableColour.StepColour(m_TargetColour, step);
			Target.VariableColour = col;
			return col.ToArgb() != m_TargetColour.ToArgb();
		}

		protected override void Reverse()
		{
			Color colOriginal = m_Original;
			m_Original = m_TargetColour;
			m_TargetColour = colOriginal;
		}

		/// <summary>Repeats should usually be an odd number: changing to the new colour is no repeat and back again is the first, so an odd number is required to finish on the colour we started on</summary>
		internal static void CreateStart(IBlendableColourDisplay target, Color newColour, int step = 16, int repeats = 0)
		{
			// only calls Start if there was no previous animation on this target
			if (Exists(target))
				return;
			AnimationColourChange animation = (AnimationColourChange)Create(target, newColour, step, repeats);
			animation?.Start();
		}

		/// <summary>Version of CreateStart which can accept any control and will iterate the back colour </summary>
		internal static void CreateStart(Control target, Color newColour, int step = 16, int repeats = 0)
		{
			CreateStart(new ControlBackProxy(target), newColour, step, repeats);
		}

		internal IBlendableColourDisplay Target => (IBlendableColourDisplay)m_Target;

		/// <summary>Proxy to implement IBlendableColourDisplay for any control, changing the back colour</summary>
		internal class ControlBackProxy : IBlendableColourDisplay
		{
			public readonly Control m_Control;

			public ControlBackProxy(Control ctr)
			{
				m_Control = ctr;
			}

			public Color VariableColour
			{
				get { return m_Control.BackColor; }
				set { m_Control.BackColor = value; }
			}
		}
	}

	internal class AnimationLinear : AnimationController
	{

		private int m_Maximum;
		private int m_Original;

		private AnimationLinear(int maximum, int step, int repeats) : base(step == 0 ? 1 : step, repeats)
		{
			m_Maximum = maximum;
		}

		public void Start()
		{
			m_Original = Target.VariableValue;
			base.SharedStart();
		}

		internal static AnimationController Create(ILinearAnimated target, int maximum, int step, int repeats = 0)
		{
			if (Exists(target))
				return Item(target);
			AnimationLinear animation = new AnimationLinear(maximum, step, repeats) { m_Target = target };
			Add(animation);
			return animation;
		}

		internal static void CreateStart(ILinearAnimated target, int maximum, int step, int repeats = 0)
		{
			if (Exists(target))
				return;
			AnimationLinear linear = new AnimationLinear(maximum, step, repeats) { m_Target = target };
			Add(linear);
			linear.Start();
		}

		protected override bool DoIterate(int step)
		{
			int newValue = Target.VariableValue + step;
			if (newValue == m_Maximum)
			{
				Target.VariableValue = newValue;
				return false;
			}
			if (Math.Sign(newValue - m_Maximum) == Math.Sign(step))
			{
				// we have gone past the target
				Target.VariableValue = m_Maximum;
				return false;
			}
			Target.VariableValue = newValue;
			return true;
		}

		protected override void Reverse()
		{
			int temp = m_Maximum;
			m_Maximum = m_Original;
			m_Original = temp;
		}

		internal ILinearAnimated Target => (ILinearAnimated)m_Target;

		public void ChangeMaximumValue(int maximum, int step)
		{
			m_Maximum = maximum;
			m_Step = step;
			if (!base.Active)
				Start();
		}
	}

	internal class AnimationToggle : AnimationController
	{

		// We don't just toggle on and off in response to the base class iteration, because often toggles will want to happen quite slowly
		// therefore we count up to a threshold value before toggling and changing direction
		private int m_Value = 0;
		private const int m_Threshold = 16; // i.e. toggle every 16 iterations

		private AnimationToggle(int interval, int repetitions = int.MaxValue) : base(1, repetitions)
		{ }

		public void Start()
		{
			base.SharedStart();
		}

		internal static AnimationController Create(IToggleAnimated target, int interval, int repetitions = int.MaxValue - 1)
		{
			if (Exists(target))
				return Item(target);
			AnimationToggle toggle = new AnimationToggle(interval, repetitions);
			toggle.m_Target = target;
			Add(toggle);
			return toggle;
		}

		protected override bool DoIterate(int step)
		{
			m_Value += step;
			if (m_Value >= m_Threshold)
			{
				Target.ToggleState = true;
				m_Value = m_Threshold;
				return false; // false indicates we have gone as far as possible in this direction
			}
			else if (m_Value <= 0)
			{
				Target.ToggleState = false;
				m_Value = 0;
				return false;
			}
			//		bolNeedsRefresh = False
			return true;
		}

		protected override void Reverse()
		{
			m_Step = -m_Step;
		}

		internal IToggleAnimated Target => (IToggleAnimated)m_Target;

	}

}
