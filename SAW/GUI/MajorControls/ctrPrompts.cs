using System.Collections.Generic;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using SAW.Functions;

namespace SAW
{
	public sealed class ctrPrompts : Control, IBlendableColourDisplay
	{
		private List<Shape.Prompt> m_Prompts;
		public List<Shape.Prompt> Prompts
		{
			get { return m_Prompts; }
			set
			{
				if (this.DesignMode)
					return;
				if (value != null)
				{
					int index = 0;
					while (index < value.Count)
					{
						if (value[index] == null)
							value.RemoveAt(index);
						else
							index += 1;
					}
				}
				if (Shape.Prompt.ListsEqual(value, m_Prompts))
					return;
				m_Prompts = value;
				Invalidate();
				if (m_Prompts != null && m_Prompts.Count == 1 && m_Prompts[0].Verbs == Shape.ShapeVerbs.Warning)
					// If there is a warning it is always a single prompt
					StartWarningAnimation();
				else
				{
					AnimationController.EnsureNoAnimation(this);
					this.BackColor = Color.White;
				}
			}
		}

		[System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		internal EditableView AttachedView;

		public ctrPrompts()
		{
			this.BackColor = Color.White;
			this.DoubleBuffered = true;
			Globals.CurrentPromptsChanged += ChangePrompts;
			this.MouseClick += ctrPrompts_MouseClick;
			this.MouseLeave += ctrPrompts_MouseLeave;
			this.MouseMove += ctrPrompts_MouseMove;
			this.Paint += ctrPrompts_Paint;
			this.Resize += ctrPrompts_Resize;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Globals.CurrentPromptsChanged -= ChangePrompts;
		}

		#region Graphics
		private static readonly StringFormat FormatTopLeft;
		private static readonly Font DescriptionFont;
		private static readonly Font DescriptionFontSmall;

		static ctrPrompts()
		{
			FormatTopLeft = new StringFormat {Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near};
			DescriptionFont = new Font(FontFamily.GenericSansSerif, 12);
			DescriptionFontSmall = new Font(FontFamily.GenericSansSerif, 10);
		}

		private void ctrPrompts_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.Clear(this.BackColor);
			e.Graphics.DrawLine(Pens.DarkGray, 0, 0, Width, 0);
			if (this.DesignMode)
				e.Graphics.DrawString("Prompts", new Font(FontFamily.GenericSansSerif, 16), Brushes.Black, new PointF(0, 0), FormatTopLeft);
			else
			{
				if (Globals.Root.CurrentConfig != null && !Globals.Root.CurrentConfig.Low_Graphics)
					e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				if (m_Prompts != null && m_Prompts.Count > 0)
				{
					Font font = m_Prompts.Count > 2 ? DescriptionFontSmall : DescriptionFont;
					int availableHeight = Height - Padding.Vertical;
					int widthEach = (Width - Padding.Horizontal) / m_Prompts.Count;
					if (widthEach < availableHeight * 3)
						widthEach = availableHeight * 3;
					for (int index = 0; index <= m_Prompts.Count - 1; index++)
					{
						Shape.Prompt prompt = m_Prompts[index];
						Image image = prompt.Image;
						int X = Padding.Left + index * widthEach;
						int right = Padding.Left + index * widthEach + widthEach - 2; // the right-hand edge of where we should draw
						if (image != null)
						{
							e.Graphics.DrawImage(image,
								new Rectangle(X, Padding.Top, availableHeight,
									availableHeight)); //, New Rectangle(0, 0, objImage.Width, objImage.Height), GraphicsUnit.Pixel) ' this deliberately lists height twice - the image is square
														  //objImage.Dispose()
							X += availableHeight + 2;
						}
						RectangleF rctText = new RectangleF(X, Padding.Top, right - X, availableHeight);
						e.Graphics.DrawString(GetPromptText(prompt), font, Brushes.Black, rctText, FormatTopLeft);
						if (prompt.HasSecondText)
						{
							// we need to measure how much space the main text used
							float height = e.Graphics.MeasureString(GetPromptText(prompt), font, (int)rctText.Width - 8, FormatTopLeft).Height;
							height += 2; // leave a bit of extra space between the lines
							rctText.Y += height;
							rctText.Height -= height;
							// secondary text always draws in the smaller font
							e.Graphics.DrawString(prompt.SecondText, DescriptionFontSmall, Brushes.Black, rctText, FormatTopLeft);
						}
						if (index < m_Prompts.Count - 1)
						{
							// this is not the last one, add a dividing line
							e.Graphics.DrawLine(Pens.Gray, right - 1, Padding.Top, right - 1, Height - Padding.Bottom);
						}
					}
				}
				Image closeImage = m_CloseHover ? Resources.AM.CloseIconHot_16 : Resources.AM.CloseIcon_16;
				e.Graphics.DrawImageUnscaled(closeImage, CloseRectangle);
			}
		}

		/// <summary>Location of the close icon</summary>
		private Rectangle CloseRectangle => new Rectangle(Width - 17, 1, 16, 16);

		#endregion

		private string GetPromptText(Shape.Prompt prompt)
		{
			// only applies to the main part of the text.  If the prompt has secondary text is not parameterised
			string text = prompt.FullText;
			text = text.Replace("%Cancel%", ShapeVerbText(Codes.Cancel));
			text = text.Replace("%Complete%", ShapeVerbText(Codes.Complete));
			text = text.Replace("%Choose%", ShapeVerbText(Codes.Choose));
			text = text.Replace("%Increment%", ShapeVerbText(Codes.Increment));
			text = text.Replace("%Decrement%", ShapeVerbText(Codes.Decrement));
			return text.Replace("&", "");
		}

		private string ShapeVerbText(Codes code)
		{
			// Return a key (if any) at the start of the shape if a keyboard user; and during a shape if the keyboard is being used for word
			string ID = "Prompts_" + code;
			if (code == Codes.Choose) // text can be changed by the current view (between release mouse and click)
			{
				if (AttachedView != null)
					ID = AttachedView.ChooseText();
			}
			// otherwise return the default text:
			return Strings.Item(ID + (GUIUtilities.MouseSwapped ? "_Swapped" : ""));
		}

		private void ctrPrompts_Resize(object sender, EventArgs e)
		{
			Invalidate();
		}

		private void ChangePrompts(List<Shape.Prompt> value) //Handles Globals.CurrentPromptsChanged
		{
			if (this.DesignMode)
				return;
			if (value != null)
			{
				int index = 0;
				while (index < value.Count)
				{
					if (value[index] == null)
						value.RemoveAt(index);
					else
						index += 1;
				}
			}
			if (Shape.Prompt.ListsEqual(value, m_Prompts))
				return;
			m_Prompts = value;
			Invalidate();
			if (m_Prompts != null && m_Prompts.Count == 1 && m_Prompts[0].Verbs == Shape.ShapeVerbs.Warning)
				// If there is a warning it is always a single prompt
				StartWarningAnimation();
			else
			{
				AnimationController.EnsureNoAnimation(this);
				this.BackColor = Color.White;
			}
		}

		#region Warning colour

		public Color VariableColour
		{
			get{return this.BackColor;}
			set
			{
				this.BackColor = value;
				Invalidate();
			}
		}

		private void StartWarningAnimation()
		{
			if (this.DesignMode)
				return;
			if (AnimationController.HasAnimation(this))
				return;
			this.BackColor = Color.White;
			// linen ' Color.Yellow, 6
			AnimationColourChange.CreateStart((IBlendableColourDisplay) this, Color.MistyRose, 2, int.MaxValue);
		}
		#endregion

		#region Mouse
		private bool m_CloseHover ; // true if mouse over hover area
		private static bool g_CloseMessageGiven ;
		private void ctrPrompts_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && m_CloseHover)
			{
				Globals.Root.PerformAction(Verb.Find(Codes.TogglePrompts), EditableView.ClickPosition.Sources.Mouse);
				m_CloseHover = false;
				if (!g_CloseMessageGiven)
				{
					var keys = Globals.Root.CurrentConfig.GetKeysForAction(Verb.Find(Codes.TogglePrompts));
					if (keys.Any())
					{
						string text = Strings.Item("Hide_Prompts_Click").Replace("%0", GUIUtilities.KeyDescription(keys.First()));
						MessageBox.Show(text, RootApplication.AppName, MessageBoxButtons.OK);
					}
					g_CloseMessageGiven = true;
				}
				return;
			}
		}

		//pressed state not actually used atm - just reacts immediately to mouse
		private void ctrPrompts_MouseLeave(object sender, EventArgs e)
		{
			if (m_CloseHover)
				Invalidate(CloseRectangle);
			m_CloseHover = false;
		}

		private void ctrPrompts_MouseMove(object sender, MouseEventArgs e)
		{
			bool hover = CloseRectangle.Contains(e.Location);
			if (hover != m_CloseHover)
				Invalidate(CloseRectangle);
			m_CloseHover = hover;
		}

		#endregion

	}

}
