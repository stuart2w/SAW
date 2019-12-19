using System;
using System.Drawing;
using System.Windows.Forms;
using SAW;

namespace Switches.GUI
{
	public class TestPanel : Panel
	{
		private bool[] m_Selected = new bool[10];
		private int m_Highlight;
		private Engine m_Engine;

		public TestPanel()
		{
			Paint += TestPanel_Paint;
			Resize += TestPanel_Resize;
		}

		public Engine Engine
		{
			get { return m_Engine; }
			set
			{
				if (m_Engine != null)
				{
					m_Engine.Iterate -= m_Engine_Iterate;
					m_Engine.Trigger -= m_Engine_Trigger;
				}
				m_Engine = value;
				if (m_Engine != null)
				{
					m_Engine.Iterate += m_Engine_Iterate;
					m_Engine.Trigger += m_Engine_Trigger;
				}
			}
		}

		public void TestPanel_Paint(object sender, PaintEventArgs e)
		{
			int size = Math.Min(Height, Width / 10);
			if (size < 10)
				return;
			int inset = size / 4;
			for (int index = 0; index <= 9; index++)
			{
				int X = index * size;
				Rectangle rct = new Rectangle(X + inset, inset, size - inset * 2, size - inset * 2);
				e.Graphics.DrawRectangle(Pens.LightGreen, rct);
				if (m_Selected[index])
					e.Graphics.FillRectangle(Brushes.LightGreen, rct);
				if (index == m_Highlight)
				{
					Pen pn = new Pen(Color.Red, 2);
					int @int = Math.Max(2, size / 8);
					rct.Inflate(@int, @int);
					e.Graphics.DrawRectangle(pn, rct);
					pn.Dispose();
				}
			}
		}

		private void m_Engine_Iterate(object sender, SingleFieldEventClass<int> direction)
		{
			m_Highlight = (m_Highlight + direction) % 10;
			Invalidate();
		}

		private void m_Engine_Trigger(bool isRepeat)
		{
			m_Selected[m_Highlight] = !m_Selected[m_Highlight];
			Invalidate();
		}

		public void TestPanel_Resize(object sender, EventArgs e)
		{
			Invalidate();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Engine = null; // will release events
			Paint -= TestPanel_Paint;
			Resize -= TestPanel_Resize;
		}

	}
}
