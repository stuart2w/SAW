using System.Text;

namespace SAW.CCF
{
	internal abstract class Query
	{
		public int Reference; 
		// some fields (especially in derived classes) can be amended before calling RequestString
		protected static int g_NextReference = 1;
		public string LanguageOut = "bliss";

		public abstract string RequestString();

	}

	internal class QueryFromWord : Query
	{ // LanguageOut can be an image set (or sets, comma-separated) or a language name
		public string Word;
		public string LanguageIn = "en";

		public QueryFromWord(string word)
		{
			Reference = g_NextReference++;
			Word = word;
		}

		// I'm not using the JavaScriptSerializer class as I would like this to run under .net 2
		public override string RequestString()
		{
			StringBuilder strRequest = new StringBuilder();
			strRequest.Append("{\"Q\": \"lookup\"");
			strRequest.Append(", \"ref\": \"").Append(Reference).Append("\"");
			strRequest.Append(", \"word\": \"").Append(Word).Append("\"");
			strRequest.Append(", \"langIn\": \"").Append(LanguageIn).Append("\"");
			strRequest.Append(", \"langOut\": \"").Append(LanguageOut).Append("\"");
			strRequest.Append("}");
			return strRequest.ToString();
		}

	}

	internal class QueryReprFromConceptID : Query
	{// Gets a representation from the concept ID set LanguageOut to either image set name or text name

		public string ConceptID;

		public QueryReprFromConceptID(string conceptID)
		{
			Reference = g_NextReference++;
			ConceptID = conceptID;
		}

		// I'm not using the JavaScriptSerializer class as I would like this to run under .net 2
		public override string RequestString()
		{
			StringBuilder strRequest = new StringBuilder();
			strRequest.Append("{\"Q\": \"repr\"");
			strRequest.Append(", \"ref\": \"").Append(Reference).Append("\"");
			strRequest.Append(", \"ccd\": \"").Append(ConceptID).Append("\"");
			strRequest.Append(", \"lang\": \"").Append(LanguageOut).Append("\"");
			strRequest.Append("}");
			return strRequest.ToString();
		}
	}
}
