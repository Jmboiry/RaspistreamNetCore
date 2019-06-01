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
using System.Collections.Generic;
using System.Linq;

namespace RTPStreamer.Http
{
	public class HttpRequestParser
	{
		static string[] allowedMethod = new string[] { "GET", "HEAD", "POST", "PUT", "OPTIONS", "DELETE", "TRACE" };
		Dictionary<string, string> _headers = new Dictionary<string, string>();

		public string Method { get; private set; }
		public string Body { get; private set; }
		public Dictionary<string, string> Headers { get => _headers; set => _headers = value; }

		public void Parse(string request)
		{
			StringParser parser = new StringParser(request);

			//parse status line.
			ParseFirstLine(parser);

			ParseHeaders(parser);

			ParseBody(parser);
		}

		private void ParseBody(StringParser parser)
		{
			if (Headers.ContainsKey("Content-Length"))
			{
				parser.ConsumeWhitespace();

				Body = parser.ConsumeToEnd();
			}
				//if (!parser.ExpectEOL())
				//	throw new Exception(String.Format("Invalid request {0}", parser.ToString()));
			//}
		}

		void ParseFirstLine(StringParser parser)
		{
			//first get the method
			string method = parser.ConsumeWord();
			Method = method;


			//THIS WORKS UNDER THE ASSUMPTION THAT:
			//valid HTTP/1.1 headers are: 
			if (!allowedMethod.Contains(Method))
				throw new Exception(String.Format("Invalid HTTP verb {0}", Method)); 
			//no longer assume this is a space... instead, just consume whitespace
			parser.ConsumeWhitespace();

			//now parse the uri
			var uri = parser.ConsumeUntil(new char[] { ' ', '\r', 'n' });
			


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
					//case "TRANSPORT": ParseTransportHeader(headerVal); break;
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

	}
}
