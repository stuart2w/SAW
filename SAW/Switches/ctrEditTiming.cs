using System;
using System.Drawing;
using System.Windows.Forms;

namespace Switches
{
	[System.ComponentModel.DefaultEvent("UserChangedValue")]
	public partial class ctrEditTiming
	{

		public event EventHandler UserChangedValue;
		// not raised if user enters an invalid number

		/// <summary>not used internally; GUI may like to store this </summary>
		public Engine.Timings Meaning = Engine.Timings.AcceptanceTime; 

		private int m_Value;
		private bool m_Filling;

		public ctrEditTiming()
		{
			InitializeComponent();
		}

		/// <summary>Value is in ms, although displayed in seconds </summary>
		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public int Value
		{
			get { return m_Value; }
			set
			{
				m_Value = value;
				m_Filling = true;
				txtTime.Text = (m_Value / 1000f).ToString("0.###");
				m_Filling = false;
			}
		}

		/// <summary>tick count when button went down - 0 if not in use</summary>
		private int m_ButtonStart;
		public void btnSet_MouseDown(object sender, MouseEventArgs e)
		{
			m_ButtonStart = Environment.TickCount;
		}

		public void btnSet_MouseLeave(object sender, EventArgs e)
		{
			if (m_ButtonStart > 0)
			{
				// if the mouse leaves during press-and-hold we abort
				m_ButtonStart = 0;
				Console.Beep();
			}
		}

		public void btnSet_MouseUp(object sender, MouseEventArgs e)
		{
			if (m_ButtonStart == 0)
				return;
			int time = Environment.TickCount - m_ButtonStart;
			m_ButtonStart = 0;
			if (time <= 0 || time > 10000)
				Console.Beep();
			else
			{
				Value = time;
				UserChangedValue?.Invoke(this, EventArgs.Empty);
			}
			m_ButtonStart = 0; // otherwise it beeps on mouse leave
		}

		public void txtTime_TextChanged(object sender, EventArgs e)
		{
			if (m_Filling)
				return;
			float time;
			//exits on success
			if (float.TryParse(txtTime.Text, out time))
			{
				if (time >= 0 && time < 10)
				{
					txtTime.BackColor = Color.White;
					m_Value = (int)(time * 1000);
					UserChangedValue?.Invoke(this, EventArgs.Empty);
					return;
				}
			}
			// failed
			txtTime.BackColor = Color.Tomato;
		}


	}

}
