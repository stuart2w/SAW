using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Switches.Switching;

namespace Switches.GUI
{
	/// <summary>Not used within SAW.  Only used within DevelopTest at the moment</summary>
	public sealed class Diagram : Control
	{
		
		private List<Display> m_Items = new List<Display>();
		private int[] m_ColumnPositions = new int[3]; // the bottom of the last item placed so far in each column
		public Timer m_Timer = new Timer();
		
		// always draws in a nominal viewport 150*3+80*2+40 wide = 650 wide; Paint uses scaling as necessary to make it fit
		private const int VERTICALSPACE = 30; // Space above and below each item
		private const int ITEMHEIGHT = 100;
		private const int ITEMWIDTH = 150;
		private const int HORIZONTALSPACE = 80;
		private const int NOMINALWIDTH = 650;
		private int m_HeightUsed = 0;
		private float m_Scale = 1;
		
		public Diagram()
		{
			DoubleBuffered = true;
			m_Timer.Interval = 10;
			m_Timer.Enabled = true;
			PhysicalSwitch.MasterSwitchChanged += MasterStateChange;
		}
		
		public void AddItem(Display display)
		{
			int column = display.Column;
			int Y = m_ColumnPositions[column] + VERTICALSPACE;
			int X = (HORIZONTALSPACE + ITEMWIDTH) * column + 20;
			display.Bounds = new Rectangle(X, Y, ITEMWIDTH, ITEMHEIGHT);
			m_Items.Add(display);
			m_ColumnPositions[column] = Y + ITEMHEIGHT + VERTICALSPACE;
			Invalidate(display.Bounds);
			m_HeightUsed = Math.Max(m_HeightUsed, m_ColumnPositions[column]);
			Diagram_Resize(this, null);
		}
		
		public void Clear()
		{
			m_Items.Clear();
			Array.Clear(m_ColumnPositions, 0, 3);
			m_HeightUsed = 0;
			Invalidate(); // no great point calling Diagram_Resize as the height is 0
		}
		
		public void Construct(Engine engine)
		{
			// Builds the diagram based on the global lists in the switch classes
			Debug.Assert(engine.Initialised);
			Clear();
			for (int index = 0; index <= 8; index++) // upper limit is arbitrary
			{
				PhysicalSwitch objSwitch = PhysicalSwitch.Switch(index);
				if (objSwitch != null)
					AddItem(new PhysicalDisplay(index));
			}
			for (int index = 0; index <= 8; index++) // upper limit is arbitrary
			{
				Logical objSwitch = Logical.Switch(index);
				if (objSwitch != null)
					AddItem(new LogicalDisplay(objSwitch, this));
			}
			AddItem(new EngineDisplay(engine, this));
			Invalidate();
		}
		
		public void Diagram_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(Color.White);
			if (DesignMode)
				return;
			e.Graphics.ScaleTransform(m_Scale, m_Scale);
			foreach (Display item in m_Items)
			{
				// it appears that the clipping rectangle doesn't follow the transformation...
				//e.Graphics.SetClip(New Rectangle(objItem.Bounds.X * m_sngScale, objItem.Bounds.Y * m_sngScale, objItem.Bounds.Width * m_sngScale, objItem.Bounds.Height * m_sngScale))
				Rectangle rct = item.Bounds;
				rct.Inflate(1, 1); // the scaling seems to cause some rounding errors, which can leave the border missing
				e.Graphics.SetClip(rct);
				item.Draw(e.Graphics);
			}
			e.Graphics.ResetClip();
			foreach (Display item in m_Items)
			{
				item.DrawConnectors(e.Graphics);
			}
		}
		
		public void m_Timer_Tick(object sender, EventArgs e)
		{
			if (DesignMode)
				return;
			foreach (Display item in m_Items)
			{
				item.RefreshAsNeeded( DoInvalidate);
			}
		}
		
		private void MasterStateChange()
		{
			//	Invalidate()
		}
		
		public Display FindDisplay(object forItem)
		{
			foreach (Display item in m_Items)
			{
				if (item.Displays == forItem)
					return item;
			}
			return null;
		}
		
		public void Diagram_Resize(object sender, EventArgs e)
		{
			m_Scale = Math.Min(Width / NOMINALWIDTH, Height / Math.Max(m_HeightUsed, 20)); // Math.Max just in case not initialised yet!
			// that is the scale which will make everything fit; but if very small, I think it's best just to leave some off the edge
			m_Scale = (float) (Math.Max(m_Scale, 0.55));
			Invalidate();
		}
		
		public delegate void ScaledInvalidate(Rectangle invalidate);
		
		private void DoInvalidate(Rectangle invalidate)
		{
			// used by the display elements to invalidate an area; the coordinates given in the nominal coordinates
			base.Invalidate(new Rectangle((int)(invalidate.X * m_Scale),
				(int)(invalidate.Y * m_Scale),
				(int)(invalidate.Width * m_Scale),
				(int)(invalidate.Height * m_Scale)));
		}
		
	}
}