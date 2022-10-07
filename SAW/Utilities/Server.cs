namespace SAW
{
	/// <summary>Stub for compatibility with Splash</summary>
	internal class Server
	{
		internal static string Language2 = System.Globalization.CultureInfo.CurrentUICulture.Name.Substring(0, 2);

		/// <summary>Software code used for requests to the update service</summary>
		public const string Software = "saw";
		internal const string PRODUCT = "SAW";

		internal static string TechURL
		{
			get
			{
				// Base URL of the Tech folder
				return "http://www.saw-at.co.uk/tech/";
			}
		}

	}

}


