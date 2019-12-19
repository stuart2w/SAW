using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace SAW.CCF
{
	internal class Response
	{
		private string m_Raw; // will be without header, if that was parsed correctly
		private string m_From;
		private string m_To;

		/// <summary>always set to something, even if successful</summary>
		public string ErrorMessage;
		/// <summary>whether this is validly parsed - might not contain any useful representations even if true</summary>
		public bool Valid; 
		public JObject JSON;
		public bool ContainsUsableResponse = true;
		/// <summary>returned representations, if any.  Null if invalid.  Empty list if none</summary>
		public List<Representation> Representations;
		/// <summary>"Ref" field from JSON</summary>
		public string Ref = "<missing>";
		/// <summary>"QR" field from JSON.  Contents determine structure of rest of JSON</summary>
		public string QR = "<missing>";

		/// <summary>we don't distinguish representations and concepts.  In CCF one concept can have multiple representations, but in our normal lookup
		///(image from word) it just returns an array of representations.  Also the 2-level structure in CCF isn't very relevent to what we're doing here </summary>
		public struct Representation
		{
			public readonly string Word;
			/// <summary>will always be set.  Other fields could be empty strings if not available.   Repr will be the image if looking up an image </summary>
			public readonly string Repr;
			/// <summary>the concept ID URI ("ccd" field) - is quite long</summary>
			public readonly string ConceptID;
			public readonly string Language;

			public Representation(string word, string repr, string conceptID, string language)
			{
				Word = word;
				Repr = repr;
				ConceptID = conceptID;
				Language = language;
			}

		}

		public Response(string text)
		{
			m_Raw = text;
			ErrorMessage = Parse(); // also sets Valid to true if succeeded
			if (Valid && ContainsUsableResponse)
			{
				Representations = new List<Representation>();
				HandleJSONInfo();
				if (ErrorMessage != "")
					System.Diagnostics.Debug.WriteLine("Failed parsing CCF response: " + ErrorMessage);
			}
		}

		// I'm not using the JavaScriptSerializer class as I would like this to run under .net 2
		// which does mean quite a bit of manual parsing...
		private string Parse()
		{ // returns error message, or "" if succeeded.  Fills in other member vars

			// split header and body around ';'
			int separator = m_Raw.IndexOf(';');
			if (separator < 1 || separator >= m_Raw.Length - 1) // note this rejects the end positions to ensure the string splitting below won't fail
				return "Invalid response: no ';' separator";
			string header = m_Raw.Substring(0, separator).Trim();
			string body = m_Raw.Substring(separator + 1).Trim();

			// parse addresses in header.  First one is from
			separator = header.IndexOf(' ');
			if (separator < 1 || separator >= header.Length - 1)
				return "Could not parse header: no spaces";
			m_From = header.Substring(0, separator);
			header = header.Substring(separator + 1);
			separator = header.IndexOf(' ');
			if (separator < 1 || separator >= header.Length - 1)
				m_To = header;// to is remainder of header
			else
				m_To = header.Substring(0, separator);
			// ignore the time stamp and 'plain' in header

			if (!body.StartsWith("{") || !body.EndsWith("}"))
				return "Body not enclosed by {}";

			m_Raw = body; // if header OK m_strRaw will reflect the body - which is what we are interested in
			JSON = JObject.Parse(body);
			if (!JSON.ContainsKey("ref"))
				return "JSON response does not contain a 'ref' item";
			if (!JSON.ContainsKey("QR"))
			{
				ContainsUsableResponse = false;
				if (!JSON.ContainsKey("Q")) return "JSON contains neither 'Q', nor 'QR' codes";
				// if it is Q we aren't interested in it - but it is valid
			}
			Valid = true;
			return "";

			//int position = 1; // position in string now
			//JSON = GetValues(ref position);
			//if (!JSON.ContainsKey("ref")) return "JSON response does not contain a 'ref' item";
			//if (!JSON.ContainsKey("QR"))
			//{
			//	ContainsValidResponse = false;
			//	if (!JSON.ContainsKey("Q")) return "JSON contains neither 'Q', nor 'QR' codes";
			//	// if it is Q we aren't interested in it - but it is valid
			//}
			//Valid = true;
			//return "";
		}

		private void HandleJSONInfo()
		{ // called after Parse (succeeded) to do something with fields that we are interested in in the JSON data
			if (JSON.ContainsKey("ref"))
				Ref = JSON["ref"].ToString();
			if (!JSON.ContainsKey("QR")) { ErrorMessage = "No content: no 'QR' in JSON."; return; }
			QR = JSON["QR"].ToString();
			if (QR == "lookup")
				HandleLookup();
			else if (QR == "repr")
				HandleRepr();
			else
				ErrorMessage = "Unexpected QR code: '" + QR + "'";
		}

		private void HandleLookup()
		{ // responses to requests with Q=lookup
			if (!JSON.ContainsKey("arr")) { ErrorMessage = "No content: no 'arr' in JSON."; return; }
			JToken arr = JSON["arr"];
			// we handle this is an array (expected) or a single object
			if (arr is JArray)
			{
				foreach (JToken sub in (arr as JArray))
				{
					if (sub is JObject)
						AddRepresentation((JObject)sub);
					else
						ErrorMessage = "One or more members of 'Arr' are not objects";
				}
			}
			else if (arr is JObject)
				AddRepresentation((JObject)arr);
		}

		private void HandleRepr()
		{ // responses to requests with Q=repr
			if (!JSON.ContainsKey("items")) { ErrorMessage = "No content: no 'items' in JSON."; return; }
			JToken items = JSON["items"];
			// must be an array; each member of which contains the ccd field and a reprArr which is an array of objects containing repr fields
			if (items is JArray)
			{
				if ((items as JArray).Count == 0) { ErrorMessage = "'items' contains 0 entries"; }
				foreach (JToken sub in (items as JArray))
				{
					if (sub is JObject)
					{ // note that this sets ErrorMessage if any subitems fails; it is clearly in at the end if we have found any valid representations
						JObject item = (JObject)sub;
						if (!item.ContainsKey("ccd")) { ErrorMessage = "No ccd field in one of 'items'."; break; }
						//string strCCD = item["ccd"].ToString();
						if (!item.ContainsKey("reprArr")) { ErrorMessage = "No reprArr field in one of 'reprArr'."; break; }
						if (!(item["reprArr"] is JArray)) { ErrorMessage = "'reprArr' is not an array"; break; }
						foreach (JToken representation in (item["reprArr"] as JArray))
						{
							if (!(representation is JObject)) { ErrorMessage = "item within 'reprArr' is not an object"; break; }
							JObject fields = (JObject)representation;
							//if (!objReprFields.ContainsKey("repr")) {ErrorMessage = "reprArr item does not contain 'repr' field"; break;}
							//if (!objReprFields.ContainsKey("lang")) {ErrorMessage = "reprArr item does not contain 'lang' field"; break;}
							AddRepresentation(fields);
						}
					}
					else
						ErrorMessage = "One or more members of 'items' are not objects";
				}
				if (Representations.Count > 0)
					ErrorMessage = ""; // ignore that one item failed as long as we did retrieve one valid response
			}
			else
				ErrorMessage = "'items' is not an array";
		}

		private void AddRepresentation(JObject representation)
		{// adds a single representation object from the JSON
		 // its the URL we are interested in ("repr") for the image
			if (!representation.ContainsKey("repr"))
				ErrorMessage = "One or more members of 'Arr' does not have a 'repr' field and is ignored.";
			else if ((representation["repr"] is JContainer))
				ErrorMessage = "One or more 'repr' members are arrays or objects - not understood";
			else
			{
				string repr = representation["repr"].ToString();
				string conceptID = ""; // we will continue and store it if either of these fields is not available
				string word = "";
				string language = "";
				if (representation.ContainsKey("ccd") && !(representation["ccd"] is JContainer))
					conceptID = representation["ccd"].ToString();
				if (representation.ContainsKey("word") && !(representation["word"] is JContainer))
					word = representation["word"].ToString();
				if (representation.ContainsKey("lang") && !(representation["lang"] is JContainer))
					language = representation["lang"].ToString();
				Representations.Add(new Representation(word, repr, conceptID, language));
			}
		}

	}
}
