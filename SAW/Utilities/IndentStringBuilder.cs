using System.Text;

namespace SAW
{
	/// <summary>Works like StringBuilder, but implements automatic indenting of lines (this is needed for the export as text since an element won't know how far its parents have indented)</summary>
	public class IndentStringBuilder
	{
		private StringBuilder m_Builder = new StringBuilder();
		private bool m_StartLine;

		/// <summary>How many times to indent; one indent may be more than one space</summary>
		public int Indent { get; set; }

		/// <summary>Adds the spaces if at start of line, and sets StartLine = false</summary>
		private void DoIndent()
		{
			if (!m_StartLine)
				return;
			if (Indent > 0)
				m_Builder.Append(' ', Indent * 2);
			m_StartLine = false;
		}

		public IndentStringBuilder Append(string s)
		{
			DoIndent();
			m_Builder.Append(s);
			return this;
		}

		/// <summary>Encodes string to remove line feeds and tabs, etc. </summary>
		/// <returns></returns>
		public IndentStringBuilder AppendEncoded(string s)
		{
			return Append(s.Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "\\r")); // yes, really
		}

		public IndentStringBuilder Append(char c)
		{
			DoIndent();
			m_Builder.Append(c);
			m_StartLine = (c == '\n');
			return this;
		}

		public IndentStringBuilder AppendLine(string s)
		{
			DoIndent();
			m_Builder.AppendLine(s);
			m_StartLine = true;
			return this;
		}

		public IndentStringBuilder AppendLine()
		{
			DoIndent();
			m_Builder.AppendLine();
			m_StartLine = true;
			return this;
		}

		public IndentStringBuilder AppendLine(int n)
		{
			DoIndent();
			m_Builder.AppendLine(n.ToString());
			m_StartLine = true;
			return this;
		}

		public IndentStringBuilder Append(int n)
		{
			DoIndent();
			m_Builder.Append(n);
			return this;
		}

		public IndentStringBuilder Append(float number, string format = "0.##")
		{
			DoIndent();
			m_Builder.Append(number.ToString(format));
			return this;
		}

		public IndentStringBuilder AppendLine(float number, string format = "0.##")
		{
			DoIndent();
			m_Builder.AppendLine(number.ToString(format));
			m_StartLine = true;
			return this;
		}

		public override string ToString()
		{
			return m_Builder.ToString();
		}

		public int Length
		{ get { return m_Builder.Length; } }
	}
}
