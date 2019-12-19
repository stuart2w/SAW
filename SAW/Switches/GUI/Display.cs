using System.Collections.Generic;
using System;
using System.Drawing;
using SAW;
using Switches.Switching;

namespace Switches.GUI
{
	/// <summary>Represents items on a Diagram control.  Not actually used in SAW - see DevelopTest</summary>
	public abstract class Display
	{
		
		public Rectangle Bounds;
		
		protected static Font TitleFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular, GraphicsUnit.Point);
		public virtual void Draw(Graphics gr)
		{
			// does not draw connectors due to clipping.  See below for these
			gr.DrawRectangle(Pens.Black, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width - 1, Bounds.Height - 1));
			gr.DrawString(Name, TitleFont, Brushes.Black, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height / 2));
		}
		
		protected abstract string Name {get;}
		public abstract int Column {get;} // in which column this should appear
		public virtual void RefreshAsNeeded(Diagram.ScaledInvalidate fnInvalidate)
		{
			foreach (Connector input in m_Inputs)
			{
				input.RefreshAsNeeded(fnInvalidate);
			}
		}
		
		protected float m_LastFractionDrawn = -1;
		protected void DrawTimer(Graphics gr, float fraction)
		{
			// Draws a timer in the bottom right section
			m_LastFractionDrawn = fraction;
			if (fraction <= 0)
				return;
			gr.FillPie(Brushes.Black, TimerRectangle(), 270, Convert.ToSingle(360 * fraction));
		}
		
		protected Rectangle TimerRectangle()
		{
			Rectangle rct = new Rectangle(Bounds.Right - Bounds.Height / 2, Bounds.Y + Bounds.Height / 2, Bounds.Height / 2, Bounds.Height / 2);
			// Note that it is deliberate that the height of the bounding box is used to generate the width;
			// assuming that the width is greater than height for this display this gives us a box filling the right-hand end of the bottom half
			rct.Inflate(-1, -1);
			return rct;
		}
		
		public abstract object Displays {get;} // the data item displayed here
		
#region Connectors
		protected List<Connector> m_Inputs = new List<Connector>();
		
		public virtual void DrawConnectors(Graphics gr)
		{
			foreach (Connector objConnector in m_Inputs)
			{
				objConnector.Draw(gr);
			}
		}
		
		protected class Connector
		{
			public Display From;
			public Display To;
			private int EventTime = 0;
			private bool EventState = false; // whether last was up or down
			private int LastDrawnAlpha = 0; // alpha of the event hilite last time drawn
			private bool LastDrawnState = false; // state drawn last time
			
			private int PULSETIME = 500; // ms
			private int FLASHWIDTH = 6;
			
			public Connector(Display fromDisplay, Display toDisplay)
			{
				From = fromDisplay;
				To = toDisplay;
				((ISwitch) From.Displays).StateChanged += InputChanged;
			}
			
			public void Draw(Graphics gr)
			{
				if (From == null || To == null)
					return;
				Point pt1 = new Point(From.Bounds.Right, From.Bounds.Y + From.Bounds.Height / 2);
				Point pt2 = new Point(To.Bounds.X, To.Bounds.Y + To.Bounds.Height / 2);
				gr.DrawLine(Pens.Black, pt1, pt2);
				int alpha = EventAlpha();
				if (alpha > 0)
				{
					using (Pen pn = new Pen(Color.FromArgb(alpha, EventState ? Color.Green : Color.Red), FLASHWIDTH))
					{
						gr.DrawLine(pn, pt1, pt2);
					}
					
				}
				LastDrawnState = EventState;
				LastDrawnAlpha = alpha;
			}
			
			private void InputChanged(bool down)
			{
				EventTime = Environment.TickCount;
				EventState = down;
			}
			
			private int EventAlpha()
			{
				int elapsed = Environment.TickCount - EventTime;
				if (elapsed > PULSETIME)
					return 0;
				return 255 * (PULSETIME - elapsed) / PULSETIME;
			}
			
			public void RefreshAsNeeded(Diagram.ScaledInvalidate fnInvalidate)
			{
				if (LastDrawnState != EventState && LastDrawnAlpha > 0 || LastDrawnAlpha != EventAlpha())
				{
					Point pt1 = new Point(From.Bounds.Right, From.Bounds.Y + From.Bounds.Height / 2);
					Point pt2 = new Point(To.Bounds.X, To.Bounds.Y + To.Bounds.Height / 2);
					int half = FLASHWIDTH / 2;
					Rectangle rct = new Rectangle(pt1.X - half, pt1.Y - half, FLASHWIDTH, FLASHWIDTH); // slightly clunky way of doing this, but don't think efficieny matters
					rct = Rectangle.Union(rct, new Rectangle(pt2.X - half, pt2.Y - half, FLASHWIDTH, FLASHWIDTH));
					fnInvalidate.Invoke(rct);
				}
			}
		}
#endregion
		
	}
	
	public abstract class SwitchDisplay : Display
	{
		
		private ISwitch m_Switch;
		
		protected SwitchDisplay(ISwitch toDisplay)
		{
			m_Switch = toDisplay;
		}
		
		private bool m_LastStateDrawn;
		public override void Draw(Graphics gr)
		{
			base.Draw(gr);
			bool state = m_Switch.State;
			m_LastStateDrawn = state;
			// draws an arrow in the bottom left-hand quadrant
			Rectangle stateRectangle = StateArrowRectangle();
			Point[] points = new Point[3];
			points[0] = new Point(stateRectangle.X, stateRectangle.Y + stateRectangle.Height / 2);
			points[1] = new Point(stateRectangle.Right, stateRectangle.Y + stateRectangle.Height / 2);
			if (state)
			{
				points[2] = new Point(stateRectangle.X + stateRectangle.Width / 2, stateRectangle.Bottom);
				gr.FillPolygon(Brushes.Green, points);
			}
			else
			{
				points[2] = new Point(stateRectangle.X + stateRectangle.Width / 2, stateRectangle.Y);
				gr.FillPolygon(Brushes.Red, points);
			}
		}
		
		protected Rectangle StateArrowRectangle()
		{
			Rectangle state = new Rectangle(Bounds.X, Bounds.Y + Bounds.Height / 2, Bounds.Width / 2, Bounds.Height / 2);
			state.Inflate(-3, -1); // nominal rectangle in which we will draw
			if (state.Width > state.Height * 1.2)
			{
				// Is rather wide; centre it somewhat
				int excess = state.Width - state.Height;
				state.Inflate(Convert.ToInt32(- excess / 2), 0);
			}
			return state;
		}
		
		public override void RefreshAsNeeded(Diagram.ScaledInvalidate fnInvalidate)
		{
			base.RefreshAsNeeded(fnInvalidate);
			if (m_LastStateDrawn != m_Switch.State)
				fnInvalidate.Invoke(StateArrowRectangle());
		}
	}
	
	public class LogicalDisplay : SwitchDisplay
	{
		
		private Logical m_Switch;
		
		public LogicalDisplay(Logical toDisplay, Diagram ctr) : base(toDisplay)
		{
			m_Switch = toDisplay;
			SwitchDisplay sourceDisplay = (SwitchDisplay) ctr.FindDisplay(m_Switch.Source);
			if (sourceDisplay != null)
			{
				m_Inputs.Add(new Connector(sourceDisplay, this));
			}
		}
		
		protected override string Name
		{get{return m_Switch.GetName();}}
		
		public override int Column
		{get{return 1;}}
		
		public override void Draw(Graphics gr)
		{
			base.Draw(gr);
			base.DrawTimer(gr, m_Switch.ElapsedTimerFraction);
		}
		
		public override void RefreshAsNeeded(Diagram.ScaledInvalidate fnInvalidate)
		{
			base.RefreshAsNeeded(fnInvalidate);
			if (m_LastFractionDrawn != m_Switch.ElapsedTimerFraction)
				fnInvalidate.Invoke(TimerRectangle());
		}
		
		public override object Displays
		{get{return m_Switch;}}
	}
	
	public class PhysicalDisplay : SwitchDisplay
	{
		
		private PhysicalSwitch m_Switch;
		
		public PhysicalDisplay(int index) : base(PhysicalSwitch.Switch(index))
		{
			m_Switch = PhysicalSwitch.Switch(index);
		}
		
		protected override string Name
		{get{return m_Switch.Type + ": " + m_Switch.ParamDescription;}}
		
		public override int Column
		{get{return 0;}}
		
		// Base class Draw is sufficient
		public override object Displays
		{get{return m_Switch;}}
		
	}
	
	public class EngineDisplay : Display
	{
		
		private Engine m_Engine;
		
		public EngineDisplay(Engine engine, Diagram ctr)
		{
			m_Engine = engine;
			m_Engine.Iterate += m_Engine_Iterate;
			m_Engine.Trigger += m_Engine_Trigger;
			foreach (Logical input in m_Engine.GetInputs())
			{
				m_Inputs.Add(new Connector(ctr.FindDisplay(input), this));
			}
		}
		
		protected override string Name
		{get{return m_Engine.DisplayName;}}
		
		public override int Column
		{get{return 2;}}
		
#region Graphics and pulse
		private string EventText = "";
		private int EventTime = 0;
		private string m_DrawnEventText = "";
		private int m_DrawnEventAlpha = 0;
		private const int PULSETIME = 500;
		private static readonly Font EventFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold, GraphicsUnit.Point);
		public override void Draw(Graphics gr)
		{
			base.Draw(gr);
			base.DrawTimer(gr, m_Engine.ElapsedTimerFraction);
			int alpha = EventAlpha();
			if (alpha > 0 && EventText != "")
			{
				using (SolidBrush br = new SolidBrush(Color.FromArgb(alpha, Color.Black)))
				{
					gr.DrawString(EventText, EventFont, br, EventRectangle());
				}
				
			}
			m_DrawnEventText = EventText;
			m_DrawnEventAlpha = alpha;
		}
		
		private int EventAlpha()
		{
			int elapsed = Environment.TickCount - EventTime;
			if (elapsed >= PULSETIME)
				return 0;
			return ((PULSETIME - elapsed) * 255) / PULSETIME;
		}
		
		private Rectangle EventRectangle()
		{
			return new Rectangle(Bounds.X, Bounds.Y + Bounds.Height / 2, Bounds.X - Bounds.Y / 2, Bounds.Height / 2);
		}
		
		public override void RefreshAsNeeded(Diagram.ScaledInvalidate fnInvalidate)
		{
			base.RefreshAsNeeded(fnInvalidate);
			if (m_LastFractionDrawn != m_Engine.ElapsedTimerFraction)
				fnInvalidate.Invoke(TimerRectangle());
			if (EventText != m_DrawnEventText || m_DrawnEventAlpha != EventAlpha())
				fnInvalidate.Invoke(EventRectangle());
		}

#endregion
		
		public override object Displays
		{get{return m_Engine;}}
		
#region Event monitoring
		private void m_Engine_Iterate(object sender, SingleFieldEventClass<int> direction)
		{
			EventTime = Environment.TickCount;
			EventText = direction < 0 ? "Back" : "Iterate";
		}
		
		private void m_Engine_Trigger(bool isRepeat)
		{
			EventTime = Environment.TickCount;
			EventText = "Trigger";
		}

#endregion
		
	}
}
