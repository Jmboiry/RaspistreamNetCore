/********
This work is an RTSP/RTP server in C# Net.Core by Jean-Marie Boiry <jean-marie.boiry@live.fr> 
It was written porting a subset of the  http://www.live555.com/liveMedia/ library
so the following copyright that apply to this software
**/
/**********
This library is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the
Free Software Foundation; either version 3 of the License, or (at your
option) any later version. (See <http://www.gnu.org/copyleft/lesser.html>.)

This library is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for
more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
**********/
// "liveMedia"
// Copyright (c) 1996-2019 Live Networks, Inc.  All rights reserved.

using RTPStreamer.Tools;
using System;

namespace RTPStreamer.RTSP
{

	public class RTSPRequestParser
	{
		static string unicast = "UNICAST";
		static string multicast = "MULTICAST";



		string _method;
		Uri _uri;
		HeaderDictionary _headers = new HeaderDictionary();

		public HeaderDictionary Headers { get => _headers; set => _headers = value; }
		public string Method { get => _method; set => _method = value; }
	
		public int ClientPort1 { get; private set; }
		public int ClientPort2 { get; private set; }
		public int Channel1 { get; private set; }
		public int Channel2 { get; private set; }

		public string TransportType { get; private set; }
		public string TransportMode { get; private set; }
		public string Destination { get; private set; }
		public string Source { get; private set; }
		public string Body { get; private set; }

		public string Media
		{
			get
			{
				if (_uri != null)
				{
					if (_uri.Segments.Length > 1)
						return _uri.Segments[1].TrimEnd('/');
				}
				return "";
			}
		}

		public string Track
		{
			get
			{
				if (_uri != null)
				{
					if (_uri.Segments.Length > 2)
						return _uri.Segments[2].TrimEnd('/');
				}
				return "";
			}
		}

		public void Parse(string request)
		{
			StringParser parser = new StringParser(request);

			//parse status line.
			ParseFirstLine(parser);

			//handle any errors that come up    
			
			ParseHeaders(parser);

			ParseBody(parser);
		}



		void ParseFirstLine(StringParser parser)
		{
			//first get the method
			string method = parser.ConsumeWord();
			_method = method;
			
			//no longer assume this is a space... instead, just consume whitespace
			parser.ConsumeWhitespace();

			//now parse the uri
			var uri = parser.ConsumeUntil(new char[] { ' ', '\r', 'n' });
			_uri = new Uri(uri);
			
			
			//no longer assume this is a space... instead, just consume whitespace
			parser.ConsumeWhitespace();

			//if there is a version, consume the version string
			string version = parser.ConsumeUntil(StringParser.sEOLMask);

			//check the version
			//if (versionStr.Len > 0)
			//	fVersion = RTSPProtocol::GetVersion(versionStr);

			//go past the end of line
			if (!parser.ExpectEOL())
				throw new Exception(String.Format("Invalid request {0}", parser.ToString()));
		}

		private void ParseHeaders(StringParser parser)
		{
			while ((parser.Peek() != '\r') && (parser.Peek() != '\n'))
			{
				//First get the header identifier
				bool isOk;
				string keyword = parser.GetThru(':', out isOk);
				if (!isOk)
					throw new Exception(String.Format("Missing semicolumn in header {0}", parser.ToString()));

				keyword = keyword.Trim();

				//Look up the proper header enumeration based on the header string.
				//Use the enumeration to look up the dictionary ID of this header,
				//and set that dictionary attribute to be whatever is in the body of the header

				var headerVal = parser.ConsumeUntil(StringParser.sEOLMask);
				headerVal = headerVal.Trim();
				_headers.Add(keyword, headerVal);


				if ((parser.Peek() == '\r') || (parser.Peek() == '\n'))
				{
					isOk = true;
					parser.ConsumeEOL();
				}
				else
					isOk = false;

				while ((parser.Peek() == ' ') || (parser.Peek() == '\t'))
				{
					parser.ConsumeUntil(StringParser.sEOLMask);
					if ((parser.Peek() == '\r') || (parser.Peek() == '\n'))
					{
						isOk = true;
						parser.ConsumeEOL();
					}
					else
						isOk = false;
				}

				if (!isOk)
					throw new Exception(String.Format("Error in header {0}", parser.ToString())); ;
				
				//some headers require some special processing. If this code begins
				//to get out of control, we made need to come up with a function pointer table
				switch (keyword)
				{
					//case qtssSessionHeader: ParseSessionHeader(); break;
					case "TRANSPORT": ParseTransportHeader(headerVal); break;
					//case qtssRangeHeader: ParseRangeHeader(); break;
					//case qtssIfModifiedSinceHeader: ParseIfModSinceHeader(); break;
					//case qtssXRetransmitHeader: ParseRetransmitHeader(); break;
					//case qtssContentLengthHeader: ParseContentLengthHeader(); break;
					//case qtssSpeedHeader: ParseSpeedHeader(); break;
					//case qtssXTransportOptionsHeader: ParseTransportOptionsHeader(); break;
					//case qtssXPreBufferHeader: ParsePrebufferHeader(); break;
					//case qtssXDynamicRateHeader: ParseDynamicRateHeader(); break;
					//case qtssXRandomDataSizeHeader: ParseRandomDataSizeHeader(); break;
					//case qtss3GPPAdaptationHeader: fRequest3GPP.ParseAdpationHeader(&fHeaderDictionary); break;
					//case qtss3GPPLinkCharHeader: fRequest3GPP.ParseLinkCharHeader(&fHeaderDictionary); break;
					//case qtssBandwidthHeader: ParseBandwidthHeader(); break;
					default: break;
				}
			}
			if (!parser.ExpectEOL())
				throw new Exception(String.Format("Invalid request {0}", parser.ToString()));
		}


		private void ParseBody(StringParser parser)
		{
			if (Headers.ContainsKey("CONTENT-LENGTH"))
			{
				parser.ConsumeWhitespace();

				Body = parser.ConsumeToEnd();
			}
			else
				Body = null;
		}

		private void ParseTransportHeader(string headerVal)
		{
			//transport header from client: Transport: RTP/AVP;unicast;client_port=5000-5001\r\n
			//                              Transport: RTP/AVP;multicast;ttl=15;destination=229.41.244.93;client_port=5000-5002\r\n
			//                              Transport: RTP/AVP/TCP;unicast\r\n
			//								Transport: RTP/AVP/TCP;unicast;interleaved=0-1
			//
			// A client may send multiple transports to the server, comma separated.
			// In this case, the server should just pick one and use that. 
			TransportType = "RTP/AVP"; // Unless found something else
			StringParser theFirstTransportParser = new StringParser(headerVal);

			bool found;
			string theTransportSubHeader = theFirstTransportParser.GetThru(';', out found);
			
			while (!String.IsNullOrEmpty(theTransportSubHeader))
			{

				// Extract the relevent information from the relevent subheader.
				// So far we care about 3 sub-headers

				if (!ParseNetworkModeSubHeader(theTransportSubHeader))
				{
					theTransportSubHeader.Trim();

					switch (theTransportSubHeader[0])
					{
						case 'R':   // rtp/avp/??? Is this tcp or udp?
							{
								if (String.Compare("RTP/AVP/TCP", theTransportSubHeader, true) == 0)
									TransportType = "RTP/AVP/TCP";

								break;
							}
						case 'C':   //client_port sub-header
							{
								ParseClientPortSubHeader(theTransportSubHeader);
								break;
							}
						case 'P':
							{
								ParseClientPortSubHeader(theTransportSubHeader);
								break;
							}
						case 'D':   //destination sub-header
							{
								//Parse the header, extract the destination address
								Destination = ParseAddrSubHeader(theTransportSubHeader, "DESTINATION");
								break;
							}

						case 'S':   //source sub-header
							{
								//Same as above code
								
								Source = ParseAddrSubHeader(theTransportSubHeader, "SOURCE");
								break;
							}
						case 'I':
							ParseInterleavedSubHeader(theTransportSubHeader);
							break;

							//case 't':   //time-to-live sub-header
							//case 'T':   //time-to-live sub-header
							//	{
							//		this->ParseTimeToLiveSubHeader(&theTransportSubHeader);
							//		break;
							//	}
							//case 'm':   //mode sub-header
							//case 'M':   //mode sub-header
							//	{
							//		this->ParseModeSubHeader(&theTransportSubHeader);
							//		break;
							//	}
					}
				}

				// Move onto the next parameter
				theTransportSubHeader = theFirstTransportParser.GetThru(';', out found);
			}
		}

		private void ParseInterleavedSubHeader(string theTransportSubHeader)
		{
			StringParser theSubHeaderParser = new StringParser(theTransportSubHeader);

			// Skip over to the first port
			bool found = false;

			string theFirstBit = theSubHeaderParser.GetThru('=', out found);


			// Store the two client ports as integers
			theSubHeaderParser.ConsumeWhitespace();
			Channel1 = theSubHeaderParser.ConsumeInteger();
			theSubHeaderParser.GetThru('-', out found);
			theSubHeaderParser.ConsumeWhitespace();
			Channel2 = theSubHeaderParser.ConsumeInteger();
		}

		private string ParseAddrSubHeader(string inSubHeader, string comparand)
		{
			StringParser theSubHeaderParser = new StringParser(inSubHeader);

			// Skip over to the value
			bool found = true;
			var first = theSubHeaderParser.GetThru('=', out found);
			if (!found)
				throw new ArgumentException(String.Format("Invalid {0} header", comparand));
			first.Trim();

			// First make sure this is the proper subheader
			if (String.Compare(first, comparand, true) != 0)
				throw new ArgumentException(String.Format("Invalid {0} header", comparand));

			//Find the IP address
			theSubHeaderParser.ConsumeUntilDigit();

			//Set the addr string param.
			string theAddr = theSubHeaderParser.ConsumeToEnd();
			theAddr.Trim();
			//Convert the string to a UInt32 IP address

			return theAddr;
		}

		private void ParseClientPortSubHeader(string inClientPortSubHeader)
		{
			StringParser theSubHeaderParser = new StringParser(inClientPortSubHeader);

			// Skip over to the first port
			bool found = false;

			string theFirstBit = theSubHeaderParser.GetThru('=', out found);


			// Store the two client ports as integers
			theSubHeaderParser.ConsumeWhitespace();
			ClientPort1 = theSubHeaderParser.ConsumeInteger();
			theSubHeaderParser.GetThru('-', out found);
			theSubHeaderParser.ConsumeWhitespace();
			ClientPort2 = theSubHeaderParser.ConsumeInteger();
			if (ClientPort1 + 1 != ClientPort2)
				throw new ArgumentException(String.Format("client ports are not contiguous ({0}-{1}", ClientPort1, ClientPort2));
		}	

		bool ParseNetworkModeSubHeader(string subHeader)
		{
			bool result = false; // true means header was found

			if (!result && String.Compare(subHeader, unicast, true) == 0)
			{
				TransportMode = unicast;
				result = true;
			}

			if (!result && String.Compare(subHeader, multicast, true) == 0)
			{
				TransportMode = multicast;
				result = true;
			}

			return result;
		}
		
		
	}
}