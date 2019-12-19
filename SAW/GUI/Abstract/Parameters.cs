using System.Collections.Generic;
using System;
using System.Drawing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;


// "Parameters" actually refers to style parameters
namespace SAW
{
	/// <summary>Originally the set of values that could be adjusted in the UI, has been extended to include codes for other actions that can be performed.
	/// Mainly used in Action as the parameter of what the action primarily does - with associated values depending on the action.  For any regular parameter there will also be an integer value </summary>
	public enum Parameters // Assumed to be Int32 by Functions.Action persistence (And by Palette.Purpose and document palette purpose)
	{
		// the styles must be kept consecutive as we use for next loops to iterate through these
		// it is also best if they are near the beginning, because some arrays are dimension up to LastStyle (although there are only a few of these PER APPLICATION, so the memory is not exactly significant)
		// These also must not clash with Palette.Purpose.Special; although that uses numbers from 1000 upwards
		FirstStyle = FillColour,
		LastStyle = FontFace,

		// Note some of these names will appear in config data - should not be renamed
		Undefined = -1,
		None = 0,
		Tool,
		//ToolFolder,
		FillColour=3,
		LineColour,
		TextColour,
		LinePattern,
		LineWidth,
		FillPattern=8,
		TextAlignment=11,
		TextVerticalAlignment,
		FontSize, // this is used to get the TextStyle object for the font name and style itself, which don't have parameters
		ArrowheadEndType, // used to indicate ArrowHeads palette
		ArrowheadEndSize,
		ArrowheadStartType,
		ArrowheadStartSize,
		FontStyle, // bold and italic etc.  Uses system enum values
		FontFace, // never actually stored in data as integer - works as index into temporary list of available fonts

		// and a change FontSize is notified when the font in general changes
		// If adding any which are palettes update Palette.Purpose.PossiblePurposes

		Toggle_First = 100,
		Toggle_Last = 149, // this range reserved for toggled values

		// ** cannot use 1000+: see Palette.Purpose.Special

		// not genuine parameters, but used in Functions.Action.  Cannot be used with ParameterValue, ParameterApplicable etc
		Action_Verb = -2,
		/// <summary>value is itself a member of this</summary>
		Action_Palette = -3,
		/// <summary>button within a palette itself.  1 = refine; 2 = transparent</summary>
		Action_Palette_Button = -4,
		/// <summary>change snap mode</summary>
		Action_Snap = -6,
		Action_Key = -8,
		/// <summary>types the given character, without necessarily knowing a valid keycode (it might not be easily typeable, such as the division symbol)
		/// Key and Character are not distinguished to the user.  Character is used if we cannot find a suitable key for text provided by the user</summary>
		Action_Character = -9,
		/// <summary>Types a sequence of characters (used for equation templates)</summary>
		Action_Text = -12,
		Action_ShowGrid = -15
	}

	public interface IParameterConsumer
	{
		int ParameterValue(Parameters parameter);
		void SetParameterValue(int value, Parameters parameter);

	}

	internal class ParameterSupport
	{
		// the line widths offered to the user (*100 to make them integers)
		internal static int[] StandardLineWidths = { 40, 70, 110, 140, 180 };
		internal static int[] LineWidthsPlusVeryThin = { 24, 40, 70, 110, 140, 180 }; // Includes one thinner option for the button editor
		internal static DashStyle[] StandardLinePatterns = { DashStyle.Solid, DashStyle.Dash, DashStyle.Dot, DashStyle.DashDot, DashStyle.DashDotDot };
		internal static Shape.FillStyleC.Patterns[] StandardFillPatterns = { Shape.FillStyleC.Patterns.Solid, Shape.FillStyleC.Patterns.Empty, Shape.FillStyleC.Patterns.Vertical, Shape.FillStyleC.Patterns.Horizontal, Shape.FillStyleC.Patterns.ForwardDiagonal, Shape.FillStyleC.Patterns.BackwardDiagonal, Shape.FillStyleC.Patterns.Cross, Shape.FillStyleC.Patterns.DiagonalCross };

		internal static int[] GetStandardParameterValues(Parameters parameter)
		{
			// gets the values for the parameters which are offered to the user.  Mostly only the styles are implemented
			List<int> list = new List<int>();
			switch (parameter)
			{
				case Parameters.FillColour:
				case Parameters.LineColour:
				case Parameters.TextColour:
					// values are the same for all of these
					list.AddRange(from col in ColourPanel.GetStandard17Colours() select col.ToArgb());
					break;
				case Parameters.LineWidth:
					return StandardLineWidths;
				case Parameters.LinePattern:
					list.AddRange(StandardLinePatterns.Cast<int>()); //.Select(Function(eStyle) CType(eStyle, Integer)))
					break;
				case Parameters.FillPattern:
					list.AddRange(StandardFillPatterns.Cast<int>()); //.Select(Function(ePattern) CType(ePattern, Integer)))
					break;
				case Parameters.ArrowheadEndType:
				case Parameters.ArrowheadStartType:
					foreach (Lined.ArrowheadC.Styles eType in Enum.GetValues(typeof(Lined.ArrowheadC.Styles)))
					{
						list.Add((int)eType);
					}
					break;
				case Parameters.ArrowheadEndSize:
				case Parameters.ArrowheadStartSize:
					list.AddRange(new[] { 650, 1000, 1500, 2500 });
					break;
				default:
					throw new ArgumentException();
					// it is not intended to implement this for the tools etc (when doing configuration some of these are not really treated as parameters anyway)
			}
			return list.ToArray();
		}

		internal static string GetParameterTypeName(Parameters parameter)
		{
			// returns the name which can be displayed to the user for the parameter itself
			// not all are available
			switch (parameter)
			{
				case Parameters.FillColour: // cannot use the line below as it returns "[Palette_FirstStyle]"
					return Strings.Item("Palette_FillColour");
				case Parameters.LineColour:
				case Parameters.FillPattern:
					return Strings.Item("Palette_" + parameter);
				case Parameters.LineWidth:
					return Strings.Item("Line_Width");
				case Parameters.LinePattern:
					return Strings.Item("Line_Pattern");
				case Parameters.FontSize:
					return Strings.Item("Palette_TextStyle"); // note returns palette name not param name - we don't assign keys
															  // to font size param directly, but can attach them to activate the palette
				case Parameters.TextAlignment:
					return Strings.Item("Text_Alignment");
				case Parameters.TextVerticalAlignment:
					return Strings.Item("Text_VerticalAlignment");
				case Parameters.ArrowheadEndSize:
				case Parameters.ArrowheadStartSize:
					return Strings.Item("Arrowhead_Size");
				case Parameters.ArrowheadEndType:
				case Parameters.ArrowheadStartType:
					return Strings.Item("Arrowhead_Type");
				case Parameters.Tool:
					return Strings.Item("Palette_Tool");
				default:
					Debug.Fail("Unexpected parameter in GetParameterTypeName: " + parameter);
					return parameter.ToString();
			}
		}

		internal static string GetParameterValueName(Parameters parameter, int value)
		{
			switch (parameter)
			{
				case Parameters.FillColour:
				case Parameters.LineColour:
				case Parameters.TextColour:
					int index = ColourPanel.GetStandard17Colours().IndexOf(Color.FromArgb(value));
					if (index >= 0)
						return Strings.Item("Colour_" + index);
					return Strings.Item("Custom_Colour");
				case Parameters.LineWidth:
					float points = value / 100f / Geometry.POINTUNIT;
					return points.ToString("0.#") + " " + Strings.Item("pt");
				case Parameters.LinePattern:
					return Strings.Item("Line_" + (DashStyle)value);
				case Parameters.FillPattern:
					return Strings.Item("Hatch_" + (Shape.FillStyleC.Patterns)value);
				case Parameters.TextAlignment:
					switch ((StringAlignment)value)
					{
						case StringAlignment.Near:
							return Strings.Item("Align_Left");
						case StringAlignment.Center:
							return Strings.Item("Align_Centre");
						case StringAlignment.Far:
							return Strings.Item("Align_Right");
						default:
							return "?";
					}
				case Parameters.TextVerticalAlignment:
					switch ((StringAlignment)value)
					{
						case StringAlignment.Near:
							return Strings.Item("Align_Top");
						case StringAlignment.Center:
							return Strings.Item("Align_Middle");
						case StringAlignment.Far:
							return Strings.Item("Align_Bottom");
						default:
							return "?";
					}
				case Parameters.ArrowheadEndSize:
				case Parameters.ArrowheadStartSize:
					return (value / 1000).ToString("%");
				case Parameters.ArrowheadEndType:
				case Parameters.ArrowheadStartType:
					return Strings.Item("Arrowhead_" + (Lined.ArrowheadC.Styles)value);
				default:
					Debug.Fail("Unexpected eParameter in GetParameterValueName:" + parameter);
					return value.ToString();
			}
		}
	}
}
