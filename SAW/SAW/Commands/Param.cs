using System;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace SAW.Commands
{
	abstract public class Param : IArchivable
	{
		public enum ParamTypes
		{
			Integer,
			Float,
			String,
			Bool
		}

		#region Data
		public abstract void Read(ArchiveReader ar);
		public abstract void Write(ArchiveWriter ar);

		public abstract void Write(DataWriter writer);

		public static Param FromReader(DataReader reader)
		{
			switch (reader.ReadByte()) // param type
			{
				case 0: return new IntegerParam(reader.ReadInt16());
				case 1: return new FloatParam(reader.ReadSingle());
				case 2: return new StringParam(reader.ReadString());
				case 3: return new BoolParam(reader.ReadBoolean());
				default: throw new InvalidDataException("Unexpected param type");
			}
		}
		#endregion

		public abstract Param Clone();

		#region Reading value as specific types
		/// <summary>The text that should be used when building the text representation of a command</summary>
		public abstract string ValueAsString();

		public virtual short ValueAsInt()
		{
			throw new UserException(Strings.Item("Script_Error_ParameterNotInt").Replace("%0", ToString()));
		}

		public virtual float ValueAsFloat()
		{
			throw new UserException(Strings.Item("Script_Error_ParameterNotFloat").Replace("%0", ToString()));
		}

		public virtual bool ValueAsBool()
		{
			throw new UserException(Strings.Item("Script_Error_ParameterNotBool").Replace("%0", ToString()));
		}

		#endregion

		public override string ToString()
		{
			return ValueAsString();
		}
	}

	public class IntegerParam : Param
	{
		public IntegerParam() { }
		public IntegerParam(Int16 i) { Value = i; }

		/// <summary>NOTE: can only actually store 16-bit.  32-bit is included for convenience</summary>
		/// <param name="i"></param>
		public IntegerParam(int i)
		{
			if (i > short.MaxValue || i < short.MinValue)
				throw new ArgumentException("IntegerParam value exceeds 16 bits");
			Value = (Int16)i;
		}

		/// <summary>Construct from text in a script line</summary>
		public IntegerParam(string source)
		{
			if (!Int16.TryParse(source, out Value))
				throw new UserException(Strings.Item("Script_Error_ParameterNotInt").Replace("%0", source));
		}

		public Int16 Value;

		#region Data methods
		public override void Read(ArchiveReader ar)
		{
			Value = ar.ReadInt16(); // NB 16 is correct
		}
		public override void Write(ArchiveWriter ar)
		{
			ar.Write(Value);
		}

		public override void Write(DataWriter writer)
		{
			writer.WriteByte(0);
			writer.Write(Value);
		}

		public override Param Clone()
		{
			return new IntegerParam(Value);
		}

		public override bool Equals(object obj)
		{
			return Value == (obj as IntegerParam)?.Value;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
		#endregion

		#region Value reading
		public override string ValueAsString()
		{
			return Value.ToString();
		}

		public override short ValueAsInt()
		{
			return Value;
		}

		public override float ValueAsFloat()
		{
			return Value;
		}
		#endregion
	}

	public class FloatParam : Param
	{
		public float Value;

		public FloatParam()
		{
		}

		public FloatParam(float value)
		{
			Value = value;
		}

		public FloatParam(string source)
		{
			if (!float.TryParse(source, out Value))
				throw new UserException(Strings.Item("Script_Error_ParameterNotFloat").Replace("%0", source));
		}

		#region Data methods
		public override void Write(DataWriter writer)
		{
			writer.WriteByte(1);
			writer.Write(Value);
		}
		public override void Read(ArchiveReader ar)
		{
			Value = ar.ReadSingle();
		}
		public override void Write(ArchiveWriter ar)
		{
			ar.Write(Value);
		}

		public override Param Clone()
		{
			return new FloatParam(Value);
		}

		public override bool Equals(object obj)
		{
			return Value == (obj as FloatParam)?.Value;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		#endregion

		#region Value reading
		public override string ValueAsString()
		{
			return Value.ToString();
		}

		public override float ValueAsFloat()
		{
			return Value;
		}

		public override short ValueAsInt()
		{
			return (short)Value;
		}

		#endregion

	}

	public class StringParam : Param
	{
		public StringParam() { }
		public StringParam(string s) { Value = s; }

		public string Value;

		#region Data methods
		public override void Read(ArchiveReader ar)
		{
			Value = ar.ReadStringL();
		}
		public override void Write(ArchiveWriter ar)
		{
			ar.WriteStringL(Value);
		}

		public override void Write(DataWriter writer)
		{
			writer.WriteByte(2);
			writer.Write(Value);
		}

		public override Param Clone()
		{
			return new StringParam(Value);
		}

		public override bool Equals(object obj)
		{
			return Value == (obj as StringParam)?.Value;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		#endregion

		#region Value reading
		public override string ValueAsString()
		{
			return Value;
		}

		public override short ValueAsInt()
		{
			short result;
			if (short.TryParse(Value, out result))
				return result;
			return base.ValueAsInt();
		}

		public override float ValueAsFloat()
		{
			float result;
			if (float.TryParse(Value, out result))
				return result;
			return base.ValueAsFloat();
		}
		#endregion
	}

	public class BoolParam : Param
	{
		public bool Value;

		public BoolParam()
		{
		}

		public BoolParam(bool b)
		{
			Value = b;
		}

		public BoolParam(string source)
		{ // should be on/off
			if (source.Equals(TrueText, StringComparison.CurrentCultureIgnoreCase))
				Value = true;
			else if (source.Equals(FalseText, StringComparison.CurrentCultureIgnoreCase))
				Value = false;
			else if (bool.TryParse(source.ToLower(), out Value))
			{ } // do nothing, condition set value
			else if (source.Equals("on", StringComparison.CurrentCultureIgnoreCase)) // accept on/off even if the local language is different
				Value = true;
			else if (source.Equals("off", StringComparison.CurrentCultureIgnoreCase))
				Value = false;
			else
				throw new UserException(Strings.Item("Script_Error_ParameterNotBool", source));
		}

		// could cache these, but there are few commands using them so I doubt it makes the blind bit of difference
		public static string TrueText
		{ get { return Strings.Item("Script_CommandParam_On"); } }
		public static string FalseText
		{ get { return Strings.Item("Script_CommandParam_Off"); } }

		#region Data methods
		public override void Read(ArchiveReader ar)
		{
			Value = (ar.ReadInt16() != 0); // yes really is 16
		}
		public override void Write(ArchiveWriter ar)
		{
			if (Value)
				ar.Write(Convert.ToInt16(1));
			else
				ar.Write(Convert.ToInt16(0));
		}

		public override void Write(DataWriter writer)
		{
			writer.WriteByte(3);
			writer.Write(Value);
		}

		public override Param Clone()
		{
			return new BoolParam(Value);
		}

		public override bool Equals(object obj)
		{
			return Value == (obj as BoolParam)?.Value;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		#endregion

		#region Value reading

		public override string ValueAsString()
		{
			return Value ? TrueText : FalseText;
		}

		public override bool ValueAsBool()
		{
			return Value;
		}

		#endregion

	}
}