using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using SAW.Shapes;

namespace SAW.Functions
{
	class ParameterAction : ValueSelectableAction
	{
		public readonly int Value;

		/// <summary>If true this is a placeholder in the selector, and the Change is set, but the TextValue must be requested from the user.  This action cannot be executed, and should only be selected with the action filled in</summary>
		public readonly bool CustomRequiresParameter;

		public ParameterAction(Parameters parameter, int value, bool customPlaceHolder = false) : base(parameter)
		{
			Value = value;
			CustomRequiresParameter = customPlaceHolder;
		}

		public override Action Clone()
		{
			return new ParameterAction(Change, Value, CustomRequiresParameter);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ParameterAction)) return false;
			ParameterAction other = (ParameterAction) obj;
			return Change == other.Change && Value == other.Value && CustomRequiresParameter == other.CustomRequiresParameter;
		}

		public override int GetHashCode() => Change.GetHashCode() ^ Value.GetHashCode() ^ CustomRequiresParameter.GetHashCode();

		public override void Trigger(ClickPosition.Sources source, EditableView pnlView, Transaction transaction)
		{
			Editor.SetParameterValue(Value, Change);
		}

		public override string PersistValueText => Value.ToString();
		public override int ValueAsInteger => Value;
		public override bool IsApplicable(EditableView pnlView) => Globals.ParameterApplicable(Change);

		public override string DescriptionWithAccelerator(bool forSpeech = false)
		{
			return ParameterSupport.GetParameterTypeName(Change) + "=" + ParameterSupport.GetParameterValueName(Change, Value);
		}

		#region Creating sample images
		static ParameterAction()
		{
			g_objArrowheadLine = new Line();
			g_objArrowheadLine.InitialiseArrowheadGUISample();
		}

		protected override Bitmap CreateSampleImage2(int size)
		{
			if (CustomRequiresParameter)
				return Resources.AM.GreyQuestion_32;

			Bitmap bitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics gr = Graphics.FromImage(bitmap))
			{
				using (NetCanvas canvas = new NetCanvas(gr))
				{
					gr.Clear(Color.Transparent);
					gr.SmoothingMode = SmoothingMode.AntiAlias;
					switch (Change)
					{
						case Parameters.FillColour:
							gr.Clear(Color.FromArgb(Value));
							break;
						case Parameters.FillPattern:
							if ((Shape.FillStyleC.Patterns)Value == Shape.FillStyleC.Patterns.Solid)
							{
								gr.Clear(Color.Black);
							} // leave it white
							else if ((Shape.FillStyleC.Patterns)Value == Shape.FillStyleC.Patterns.Empty)
							{
							}
							else if ((Shape.FillStyleC.Patterns)Value == Shape.FillStyleC.Patterns.Texture)
							{
								gr.Clear(Color.Gray);
								Debug.Fail("Cannot create sample image for Texture pattern");
							}
							else
							{
								HatchBrush br = new HatchBrush((HatchStyle)Value, Color.Black, Color.Empty);
								gr.FillRectangle(br, 0, 0, 32, 32);
								br.Dispose();
							}
							break;
						case Parameters.LineColour:
							using (Pen pn = new Pen(Color.FromArgb(Value), 2))
								gr.DrawLine(pn, 1, 1, 30, 30);
							break;
						case Parameters.TextColour:
							using (Font objFont = new Font(FontFamily.GenericSerif, 10))
							using (SolidBrush br = new SolidBrush(Color.FromArgb(Value)))
								gr.DrawString("Ab", objFont, br, 2, 10);
							break;
						case Parameters.LineWidth:
							using (Pen pn = new Pen(Color.Black, Value / 100f ))
								gr.DrawLine(pn, 1, 1, 30, 30);
							break;
						case Parameters.LinePattern:
							using (Pen pn = new Pen(Color.Black, 2))
							{
								pn.DashStyle = (DashStyle)Value;
								gr.DrawLine(pn, 1, 1, 30, 30);
							}
							break;
						case Parameters.TextAlignment:
							switch ((StringAlignment)Value)
							{
								case StringAlignment.Near: return (Bitmap)GUIUtilities.RM.GetObject("TextLeft");
								case StringAlignment.Center: return (Bitmap)GUIUtilities.RM.GetObject("TextCentre");
								case StringAlignment.Far: return (Bitmap)GUIUtilities.RM.GetObject("TextRight");
								default:
									return null;
							}
						case Parameters.TextVerticalAlignment:
							switch ((StringAlignment)Value)
							{
								case StringAlignment.Near: return (Bitmap)GUIUtilities.RM.GetObject("TextTop");
								case StringAlignment.Center: return (Bitmap)GUIUtilities.RM.GetObject("TextMiddle");
								case StringAlignment.Far: return (Bitmap)GUIUtilities.RM.GetObject("TextBottom");
							}
							break;
						case Parameters.ArrowheadEndSize:
						case Parameters.ArrowheadStartSize:
							DrawArrowhead(canvas, Lined.ArrowheadC.Styles.SimpleSolid, Value);
							break;
						case Parameters.ArrowheadEndType:
						case Parameters.ArrowheadStartType:
							DrawArrowhead(canvas, (Lined.ArrowheadC.Styles)Value, Lined.ArrowheadC.DEFAULTSIZE);
							break;
						default:
							gr.Clear(Color.Gray);
							Debug.Fail("Unexpected parameter in CreateParameterSampleImage: " + Change);
							break;
					}
				}
			}

			return bitmap;
		}

		private static readonly Line g_objArrowheadLine;
		private static void DrawArrowhead(Canvas gr, Lined.ArrowheadC.Styles type, int size)
		{
			g_objArrowheadLine.EndArrowhead.Size = size;
			g_objArrowheadLine.EndArrowhead.Style = type;
			SmoothingMode eOld = gr.SmoothingMode;
			gr.SmoothingMode = SmoothingMode.AntiAlias;
			g_objArrowheadLine.Draw(gr, 1, 1, null, StaticView.InvalidationBuffer.Base, 255, 255);
			gr.SmoothingMode = eOld;
		}

		#endregion


	}
}
