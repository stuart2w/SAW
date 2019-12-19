namespace SAW
{
	/// <summary>Stub for compatibility with Splash</summary>
	public class Server
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

	#region Partial classes for services, allowing 'using'
	// all from: http://omaralzabir.com/do-not-use-using-in-wcf-client/


	//namespace Repo2
	//{
	//	public partial class Repo2SoapClient : IDisposable
	//	{

	//		public void Dispose()
	//		{
	//			if (State == CommunicationState.Faulted)
	//				Abort();
	//			else if (State != CommunicationState.Closed)
	//				Close();
	//		}

	//	}
	//}

	#endregion

}


