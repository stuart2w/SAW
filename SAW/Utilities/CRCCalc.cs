using System;
using System.IO;

namespace SAW
{
	internal static class CRCCalc
	{
		private static readonly uint[] table = new uint[256];

		private static uint Calc(uint start, Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);
			int n = stream.ReadByte();
			while (n != -1)
			{
				start = table[(start & 255) ^ n] ^ (start >> 8);
				n = stream.ReadByte();
			}
			return start;
		}

		public static int Calc(string filename)
		{
			using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				uint n = Calc(0, fs);
				// but it used to be simply cast from unsigned long to long in C++,
				// so now need to achieve this cast in a meddling, interfering environment, which won't actually do it...
				long huge = n;
				if (huge > int.MaxValue)
					huge -= (long)int.MaxValue * 2;
				return (int)huge;
			}

		}

		public static int Calc(Stream strm)
		{
			uint n = Calc(0, strm);
			long huge = n;
			if (huge > int.MaxValue)
				huge -= (long)int.MaxValue * 2;
			return (int)huge;
		}

		public static int Calc(byte[] buffer)
		{
			uint start = 0;
			for (int index = 0; index <= buffer.Length - 1; index++)
			{
				int n = buffer[index];
				start = table[(start & 255) ^ n] ^ (start >> 8);
			}
			// and convert back to a signed 32-bit integer
			long huge = start;
			if (huge > int.MaxValue)
				huge -= (long)int.MaxValue * 2;
			return (int)huge;
		}

		private static readonly uint X = Convert.ToUInt32(0xEDB88320);
		static CRCCalc()
		{
			for (int i = 0; i <= 255; i++)
			{
				var crc = Convert.ToUInt32(i);
				for (var n = 1; n <= 8; n++)
				{
					if ((crc & 1) == 1) // ie if odd
						crc = (crc >> 1) ^ X;
					else
						crc = crc >> 1;
					table[i] = crc;
				}
			}
		}


	}
}
