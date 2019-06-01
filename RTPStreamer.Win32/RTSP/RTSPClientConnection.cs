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


using RTPStreamer.Media;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RTPStreamer.Network;
using RTPStreamer.Tools;
using RTPStreamer.Core;
using System.Net;

namespace RTPStreamer.RTSP
{
	class Packet
	{
		public byte[] buffer;
		public int bytes;
	}

	public class RTSPClientConnection
	{
		public RTSPServer Server { get; private set; }
		static Logger _logger = LogManager.GetLogger("RTSPClientConnection");
		string[] _commands = new string[] { "DESCRIBE", "SETUP", "TEARDOWN", "SET_PARAMETER", "GET_PARAMETER", "PLAY", "PAUSE", "OPTIONS" };
		static string _supportedCommands = "OPTIONS, DESCRIBE, SET_PARAMETER, GET_PARAMETER, SETUP, TEARDOWN, PLAY, PAUSE";
		static string[] _parameters = new string[] { "WIDTH", "HEIGHT" };

		RTPStream _rtcpFilter;
		TcpClient _tcpClient;
		RTSPSession _session;

		public RTSPClientConnection(RTSPServer owner, TcpClient client) 
		{
			Server = owner;
			_tcpClient = client;
		}

		internal void RegisterFilter(RTPStream rtpStream)
		{
			_rtcpFilter = rtpStream;
		}

		public async Task ProcessingLoop()
		{
			try
			{
				// Get a stream object for reading and writing


				using (var stream = _tcpClient.GetStream())
				{

					byte[] buffer = new byte[1000];
					int bytesRead = 0;

					while (_tcpClient.Connected)
					{
						StringBuilder message = new StringBuilder();
						

						// Incoming message may be larger than the buffer size.
						do
						{
							bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
							// Translate data bytes to a ASCII string.
							message.AppendFormat("{0}", Encoding.ASCII.GetString(buffer, 0, bytesRead));
						}
						while (stream.DataAvailable);

						string request = message.ToString();

						if (String.IsNullOrEmpty(request))
						{
							_logger.Error("Empty packet");
							if (IsDisconnected(_tcpClient))
							{
								_logger.Error("Client disconnected ungracefully");
								goto close;
							}
							continue;
						}
							
						if (await ProcessIncomingRequest(request) == false)
							break;
					}
				}

			close:
				_tcpClient.Close();

			}
			
			catch (Exception ex)
			{
				if (_logger.IsDebugEnabled)
					_logger.Error(ex);
			}
			finally
			{
				_session?.Close();
				_session = null;
				_tcpClient.Close();
			}
			
			if (_logger.IsDebugEnabled)
				_logger.Debug("Network stream disconnected, thread exiting.");
		}


		// Checks if a client has disconnected ungracefully
		// Adapted from: http://stackoverflow.com/questions/722240/instantly-detect-client-disconnection-from-server-socket
		public bool IsDisconnected(TcpClient client)
		{
			try
			{

				Socket s = client.Client;
				return s.Poll(10 * 1000, SelectMode.SelectRead) && (s.Available == 0);
			}
			catch (SocketException)
			{
				// We got a socket error, assume it's disconnected
				return true;
			}
		}

		private async Task<Packet> ReadPacket(NetworkStream stream)
		{

			// We expect the following data over the TCP channel:
			//   optional RTSP command or response bytes (before the first '$' character)
			//   a '$' character
			//   a 1-byte channel id
			//   a 2-byte packet size (in network byte order)
			//   the packet data.
			// However, because the socket is being read asynchronously, this data might arrive in pieces.
			byte[] newBuffer = new byte[2000];
			byte[] buffer = new byte[1000];
			int bytesRead = 0;
			
			int bytes = await stream.ReadAsync(buffer, 0, buffer.Length);

			if (_rtcpFilter != null)
			{
				for (int i = 0; i < bytes; i++)
				{
					byte ch = buffer[i];

					if (ch == '$')
					{

						if (i + 4 < bytes)
							break;
						short one = BitConverter.ToInt16(buffer, i + 2);
						short two = IPAddress.NetworkToHostOrder(one);
						short size1 = buffer[i + 2];

						int size = (short)((size1 << 8) | buffer[i + 3]);

						byte[] rtcp = new byte[size];
						stream.Read(rtcp, 0, size);


						i += size;

						if (_rtcpFilter != null)
							_rtcpFilter.IncomingReportHandler(rtcp, (IPEndPoint)_tcpClient.Client.LocalEndPoint);
					}
				}
			}
			else
			{
				newBuffer = buffer;
				bytesRead = bytes;
			}
			
			return new Packet() { buffer = newBuffer, bytes = bytesRead };
		}

		public async Task<bool> ProcessIncomingRequest(string message)
		{
			bool retVal = false;
			if (_logger.IsDebugEnabled)
				_logger.Debug("C->S:\n{0}", message);

			if (_session != null)
				_session.LastKeepAlive = DateTime.Now;

			RTSPRequestParser parser = new RTSPRequestParser();

			try
			{
				parser.Parse(message.ToUpper()); // TODO : Error handling
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				HandleBadRequest();
				return false;
			}

			if (!_commands.Contains(parser.Method))
			{
				HandleCommandNotSupported(parser);
				return false;
			}

			if (!String.IsNullOrEmpty(parser.TransportType))
			{
				if (parser.TransportType != "RTP/AVP" && parser.TransportType != "RTP/AVP/TCP")
				{
					HandleUnsupportedTransport(parser);
					return false;
				}
			}

			// Handle session less verbs first
			if (parser.Method == "OPTIONS")
			{
				retVal = Options(parser);
				return retVal;
			}

			if (parser.Method == "DESCRIBE")
			{
				retVal = Describes(parser);
				return retVal;
			}


			if (parser.Method == "SET_PARAMETER")
			{
				retVal = SetParameter(parser);
				return retVal;
			}


			if (parser.Headers.ContainsKey("SESSION"))
			{
				_session = Server.GetSession(Convert.ToInt32(parser.Headers["SESSION"]));
				if (_session == null)
				{
					HandleSessionNotFound(parser);
					return false;
				}
			}
			else
			{
				_session = Server.GenerateSession(this);
				if (_logger.IsDebugEnabled)
					_logger.Debug("Generating new session {0}", _session.SessionId);
			}

			_session.LastKeepAlive = DateTime.Now;

			if (parser.Method == "SETUP")
				retVal = _session.Setup(_tcpClient, parser);

			else if (parser.Method == "PAUSE")
				retVal = _session.Pause(parser);

			else if (parser.Method == "GET_PARAMETER")
				retVal = _session.GetParameter(parser);
				
			else if (parser.Method == "PLAY")
				retVal = await _session.Play(parser);

			else if (parser.Method == "TEARDOWN")
			{
				_session.Teardown(parser);
				return false;
			}
			else
				HandleBadRequest();

			return retVal;
		}

	
		void HandleBadRequest()
		{
			// Don't do anything with "fCurrentCSeq", because it might be nonsense
			string reply = String.Format("RTSP/1.0 400 Bad Request\r\n{0}\r\nAllow: {1}\r\n\r\n",
										"Tue, 30 Aug 2016 01:33:17 GMT", _supportedCommands);
			SendResponse(reply);
		}

		void HandleCommandNotSupported(RTSPRequestParser parser)
		{
			string reply = String.Format("RTSP/1.0 405 Method Not Allowed\r\nCSeq: {0}\r\n{1}\r\nAllow: {2}\r\n\r\n",
										 parser.Headers["CSEQ"], "Tue, 30 Aug 2016 01:33:17 GMT", _supportedCommands);
			SendResponse(reply);
		}

		public void HandleNotFound(RTSPRequestParser parser)
		{
			string reply = String.Format("RTSP/1.0 404 Stream Not Found\r\nCSeq: {0}\r\n\r\n", parser.Headers["CSEQ"]);

			SendResponse(reply);
		}

		void HandleSessionNotFound(RTSPRequestParser parser)
		{
			string reply = String.Format("RTSP/1.0 454 Session Not Found\r\nCSeq: {0}\r\n{1}\r\n\r\n",
										  parser.Headers["CSEQ"], "Tue, 30 Aug 2016 01:33:17 GMT");
			SendResponse(reply);
		}

		public void HandleInvalidParameter(RTSPRequestParser parser, params string[] parameters)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("RTSP/1.0 451 Invalid Parameter\r\n");
			sb.AppendFormat("CSeq: {0}\r\n", parser.Headers["CSEQ"]);

			StringBuilder body = new StringBuilder();
			foreach(var str in parameters)
				body.AppendFormat("{0}\r\n", str);
			string tmp = body.ToString();
			sb.AppendFormat("Content-length: {0}\r\n", tmp.Length);
			sb.Append("Content-type: text/parameters\r\n\r\n");
			sb.AppendFormat("{0}", tmp);

			SendResponse(sb.ToString());
		}

		public void HandleUnsupportedTransport(RTSPRequestParser parser)
		{
			string reply = String.Format("RTSP/1.0 461 Unsupported Transport\r\nCSeq: {0}\r\n{1}\r\n\r\n",
										 parser.Headers["CSEQ"], "Tue, 30 Aug 2016 01:33:17 GMT");
			SendResponse(reply);
		}

		public void HandleInvalidState(RTSPRequestParser parser)
		{
			string reply = String.Format("RTSP/1.0 455 Method Not Valid in This State\r\nCSeq: {0}\r\n{1}\r\n\r\n",
										 parser.Headers["CSEQ"], "Tue, 30 Aug 2016 01:33:17 GMT");
			SendResponse(reply);
		}

		

		public bool Options(RTSPRequestParser parser)
		{
			// TODO : Take care of the Uri format
			string media = parser.Media;


			ServerMediaSession session = Server.lookupServerMediaSession(media);
			if (session == null)
			{
				HandleNotFound(parser);
				return false;
			}



			StringBuilder sb = new StringBuilder();
			sb.Append("RTSP/1.0 200 OK\r\n");
			sb.AppendFormat("Server: {0}\r\n", RTSPServer.ServerVersion);
			sb.AppendFormat("Cseq: {0}\r\n", parser.Headers["CSEQ"]);
			sb.AppendFormat("Public: {0}\r\n", _supportedCommands);
			sb.Append("\r\n");
			SendResponse(sb.ToString());
			return true;

		}

		private bool SetParameter(RTSPRequestParser parser)
		{
			// Begin by looking up the "ServerMediaSession" object for the specified "urlTotalSuffix":
			string media = parser.Media;


			ServerMediaSession session = Server.lookupServerMediaSession(media);
			if (session == null)
			{
				HandleNotFound(parser);
				return false;
			}

			// Look up information for the specified subsession (track):
			ServerMediaSubsession subsession;

			media = media.TrimEnd('/');// parser.Uri.Segments[2].TrimStart('/');
			subsession = session.LookupSubSession(media);

			string body = parser.Body;
			if (String.IsNullOrEmpty(body))
				HandleInvalidParameter(parser);

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			
			try
			{
				StringParser bodyParser = new StringParser(body.ToUpper());
				ParseParameters(parser, bodyParser, parameters);
				Server.NewParameters(parameters); 
			}
			catch
			{
				return true;
			}
			StringBuilder sb = new StringBuilder();
			sb.Append("RTSP/1.0 200 OK\r\n");
			sb.AppendFormat("Server: {0}\r\n", RTSPServer.ServerVersion);
			sb.AppendFormat("Cseq: {0}\r\n", parser.Headers["CSEQ"]);
			sb.Append("\r\n");
			SendResponse(sb.ToString());
			return true;
		}

		private void ParseParameters(RTSPRequestParser requestParser, StringParser parser, Dictionary<string, string> parameters)
		{
			while ((parser.Peek() != '\r') && (parser.Peek() != '\n'))
			{
				//First get the parameter identifier
				bool isOk;
				string keyword = parser.GetThru(':', out isOk);
				if (!isOk)
				{
					if (!parser.ParserEndOfString())
						throw new Exception(String.Format("Missing semicolumn in body {0}", parser.ToString()));
					else
						return;
				}

				keyword = keyword.Trim();

				
				//Look up the proper header enumeration based on the header string.
				//Use the enumeration to look up the dictionary ID of this header,
				//and set that dictionary attribute to be whatever is in the body of the header

				var headerVal = parser.ConsumeUntil(StringParser.sEOLMask);
				headerVal = headerVal.Trim();
				if (!RTSPServer._validParameters.Contains(keyword))
					HandleInvalidParameter(requestParser, keyword);
				else
					parameters.Add(keyword, headerVal);
				

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

				try
				{
					switch (keyword)
					{
						case "WIDTH":
							Convert.ToInt32(headerVal);
							break;
						case "HEIGHT":
							Convert.ToInt32(headerVal);
							break;
					}

				}
				catch
				{
					HandleInvalidParameter(requestParser, keyword, headerVal);
					throw;
				}
				if (!isOk)
					throw new Exception(String.Format("Error in header {0}", parser.ToString())); ;

			}
			if (!parser.ExpectEOL())
				throw new Exception(String.Format("Invalid request {0}", parser.ToString()));
		}

		public void SendResponse(string reply)
		{
			
			byte[] msg = System.Text.Encoding.ASCII.GetBytes(reply);
			if (_logger.IsDebugEnabled)
				_logger.Debug("S->C:\n{0}", reply);
			// Send back a response.
			_tcpClient.GetStream().Write(msg, 0, msg.Length);
		}

		private bool Describes(RTSPRequestParser parser)
		{

			// Begin by looking up the "ServerMediaSession" object for the specified "urlTotalSuffix":
			string media = parser.Media;


			ServerMediaSession session = Server.lookupServerMediaSession(media);
			if (session == null)
			{
				HandleNotFound(parser); 
				return false;
			}
			
			// Look up information for the specified subsession (track):
			ServerMediaSubsession subsession;
			
			media = media.TrimEnd('/');// parser.Uri.Segments[2].TrimStart('/');
			subsession = session.LookupSubSession(media);
			
			var sdp = subsession.GenerateSDPDescription();
			

			StringBuilder sb = new StringBuilder();
			sb.Append("RTSP/1.0 200 OK\r\n");
			sb.AppendFormat("Server: {0}\r\n", RTSPServer.ServerVersion);
			sb.AppendFormat("Cseq: {0}\r\n", parser.Headers["CSEQ"]);
			sb.Append("Last-Modified: Tue, 30 Aug 2016 01:33:17 GMT\r\n");
			sb.Append("Cache-Control: must-revalidate\r\n");

			sb.AppendFormat("Content-length: {0}\r\n", sdp.Length);

			sb.Append("Date: Sat, 22 Dec 2018 16:18:03 GMT\r\n");
			sb.Append("Expires: Sat, 22 Dec 2018 16:18:03 GMT\r\n");
			sb.Append("Content-Type: application/sdp\r\n").
			Append("x-Accept-Retransmit: our-retransmit\r\n").
			Append("x-Accept-Dynamic-Rate: 1\r\n").
			AppendFormat("Content-Base: rtsp://{0}:{1}/{2}\r\n\r\n", SocketExtensions.GetLocalIPV4Address(), Server.Port, subsession.Name).
			Append(sdp);

			var str = sb.ToString();
			SendResponse(str);
			return true;
		}
	}
}
