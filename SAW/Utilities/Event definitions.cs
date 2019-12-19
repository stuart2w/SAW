using System;

namespace SAW
{
	/// <summary>Used for non-compliant events that have no parameters at all</summary>
	public delegate void NullEventHandler();
	public delegate void ShapeEventHandler(Shape shp);

	/// <summary>Can be used within the standard EventHandler if a single value is all that is needed.<br />
	/// Has implicit constructor, so the value alone can be used when raising the event: eventname?.Invoke(this, value).<br />
	/// Event declared: public event EventHandler&lt;SingleFieldEventClass&lt;fieldtype&gt;&gt; eventname;</summary>
	public class SingleFieldEventClass<T>: EventArgs
	{
		public T Value;

		public SingleFieldEventClass(T value)
		{
			Value = value;
		}

		public static implicit operator SingleFieldEventClass<T>(T value)
		{
			return new SingleFieldEventClass<T>(value);
		}

		public static implicit operator T(SingleFieldEventClass<T> args)
		{
			return args.Value;
		}

	}
}