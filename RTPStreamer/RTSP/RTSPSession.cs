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

using NLog;
using RTPStreamer.Core;
using RTPStreamer.Media;
using RTPStreamer.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RTPStreamer.RTSP
{

	public class RTSPSession : IDisposable
	{

		static Logger _logger = LogManager.GetLogger("RTSPClientSession");

		public int SessionId { get; private set; }
		public bool IsRTPOverTCP { get; private set; }
		public bool Closed { get; private set; }
		public bool Playing { get; private set; }
		public DateTime LastKeepAlive { get; set; }

		RTSPClientConnection _connection;
		RTPStream _rtpStream;
		Destination _destinations;
		

		public RTSPSession(int sessionId, RTSPClientConnection connection, short initialPortNum = 6970, bool multiplexRTCPWithRTP = false)
		{
			SessionId = sessionId;
			_connection = connection;
			Closed = false;
			IsRTPOverTCP = false;
			LastKeepAlive = DateTime.Now;
		}

		public bool GetParameter(RTSPRequestParser parser)
		{
			string media = parser.Media;


			ServerMediaSession session = _connection.Server.lookupServerMediaSession(media);
			if (session == null)
			{
				_connection.HandleNotFound(parser);
				return false;
			}
			// Look up information for the specified subsession (track):


			ServerMediaSubsession subsession = session.LookupSubSession(media);

			// Not yet implemented
			//ParseGetParameter(parser);

			StringBuilder sb = new StringBuilder();
			sb.Append("RTSP/1.0 200 OK\r\n");
			sb.AppendFormat("Server: {0}\r\n", RTSPServer.ServerVersion);
			sb.AppendFormat("Cseq: {0}\r\n", parser.Headers["CSEQ"]);
			sb.AppendFormat("Session: {0}\r\n", SessionId);
			sb.Append("Last-Modified: Tue, 30 Aug 2016 01:33:17 GMT\r\n");
			sb.Append("Date: Sat, 22 Dec 2018 16:18:03 GMT\r\n");
			sb.Append("Expires: Sun, 23 Dec 2018 12:01:28 GMT\r\n\r\n");

			string reply = sb.ToString();
			_connection.SendResponse(reply);
			return true;

		}

		private void ParseGetParameter(RTSPRequestParser parser)
		{
			if (String.IsNullOrEmpty(parser.Body))
				_connection.HandleInvalidParameter(parser);

		}

		public bool Setup(TcpClient tcpSocket, RTSPRequestParser parser)
		{
			string media = parser.Media;

			
			ServerMediaSession session = _connection.Server.lookupServerMediaSession(media);
			if (session == null)
			{
				_connection.HandleNotFound(parser);
				return false;
			}
			// Look up information for the specified subsession (track):
			
			
			ServerMediaSubsession subsession = session.LookupSubSession(media);
			
			
			Socket socket = null;
			if (String.Compare(parser.TransportType, "RTP/AVP/TCP") == 0)
			{
				socket = tcpSocket.Client;
				IsRTPOverTCP = true;
			}

			if ((subsession.IsMulticast && parser.TransportMode == "UNICAST") || (!subsession.IsMulticast && parser.TransportMode == "MULTICAST"))
			{
				_connection.HandleUnsupportedTransport(parser);
				return false;
			}

			int clientRTPPort = parser.ClientPort1;
			int clientRTCPPort = parser.ClientPort2;
			
			int serverRTPPort;
			int serverRTCPPort;
			string multiCastAddress;
			var clientAddress = tcpSocket.Client.RemoteEndPoint as IPEndPoint;
			subsession.GetStreamParameters(out serverRTPPort, out serverRTCPPort, out multiCastAddress);

			SetupDestinations(socket, clientAddress, null, clientRTPPort, clientRTCPPort, (byte)parser.Channel1, (byte)parser.Channel2);
			
			StringBuilder sb = new StringBuilder();
			sb.Append("RTSP/1.0 200 OK\r\n");
			sb.AppendFormat("Server: {0}\r\n", RTSPServer.ServerVersion);
			sb.AppendFormat("Cseq: {0}\r\n", parser.Headers["CSEQ"]);
			
			var localIP = SocketExtensions.GetLocalIPV4Address();
			if (parser.TransportMode == "UNICAST")
			{
				sb.AppendFormat("Session: {0}\r\n", SessionId);
				sb.Append("Last-Modified: Tue, 30 Aug 2016 01:33:17 GMT\r\n");
				sb.Append("Cache-Control: must-revalidate\r\n");
				sb.Append("Date: Sat, 22 Dec 2018 16:18:03 GMT\r\n");
				sb.Append("Expires: Sun, 23 Dec 2018 12:01:28 GMT\r\n");

				if (socket == null)
				{
					// UDP
					sb.AppendFormat("Transport: RTP/AVP;unicast;destination={0};source={1};client_port={2}-{3};server_port={4}-{5};\r\n\r\n",
									clientAddress.Address,
									localIP,
									parser.ClientPort1,
									parser.ClientPort2,
									serverRTPPort,
									serverRTCPPort);
				}
				else
				{
					// TCP
					sb.AppendFormat("Transport: RTP/AVP/TCP;unicast;destination={0};source={1};interleaved={2}-{3}\r\n\r\n",
									clientAddress.Address,
									localIP,
									parser.Channel1,
									parser.Channel2);
									


				}
			}
			// Multicast
			else
			{
				sb.Append("Date: Sat, 22 Dec 2018 16:18:03 GMT\r\n");
				sb.AppendFormat("Transport: RTP/AVP;multicast;destination={0};source={1};port={2}-{3};ttl=255\r\n",
									multiCastAddress,
									localIP,
									serverRTPPort,
									serverRTCPPort).
				AppendFormat("Session: {0};timeout=65\r\n\r\n", SessionId);
			}
		
			var reply = sb.ToString();
			_connection.SendResponse(reply);
			return true;
		}

	
		public void Teardown(RTSPRequestParser parser)
		{

			try
			{
				Close();
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}
		}


		public async Task<bool> Play(RTSPRequestParser parser)
		{
			string stream = parser.Media;


			ServerMediaSession session = _connection.Server.lookupServerMediaSession(stream);
			if (session == null)
			{
				_connection.HandleNotFound(parser);
				return false;
			}
			// Look up information for the specified subsession (track):
			ServerMediaSubsession subsession;
			
			subsession = session.LookupSubSession(stream);
			ushort rtpSeqNum = 0;
			uint rtpTimestamp = 0;

			_rtpStream = subsession.GetStreamInstance();

			rtpSeqNum = _rtpStream.CurrentSeqNo();
			rtpTimestamp = _rtpStream.CurrentTimeStamp();// presetNextTimestamp();
			Playing = true;

			await _rtpStream.AddDestination(this, _destinations);
			if (_destinations.isTCP)
				_connection.RegisterFilter(_rtpStream);
			
			StringBuilder sb = new StringBuilder();
			sb.Append("RTSP/1.0 200 OK\r\n");
			sb.AppendFormat("Server: {0}\r\n", RTSPServer.ServerVersion);
			sb.AppendFormat("Cseq: {0}\r\n", parser.Headers["CSEQ"]);
			sb.AppendFormat("Session: {0}\r\n", parser.Headers["SESSION"]);
			sb.Append("Last-Modified: Tue, 30 Aug 2016 01:33:17 GMT\r\n");
			sb.Append("Cache-Control: must-revalidate\r\n");
			sb.Append("Date: Sat, 22 Dec 2018 16:18:03 GMT\r\n");
			sb.Append("Expires: Sun, 23 Dec 2018 12:01:28 GMT\r\n");
			sb.Append("Range: npt=0.000-\r\n").
			AppendFormat("RTP-Info: url=rtsp://{0}:{1}/{2}/track1;seq={3};rtptime={4}\r\n\r\n", 
								SocketExtensions.GetLocalIPV4Address(), 
								_connection.Server.Port,
								subsession.Name,
								rtpSeqNum, rtpTimestamp);

			string reply = sb.ToString();
			_connection.SendResponse(reply);
			return true;
		}

		public bool Pause(RTSPRequestParser parser)
		{
			if (!Playing)
			{
				_connection.HandleInvalidState(parser);
				return true;
			}
						
			if (_rtpStream != null)
			{
				_rtpStream.RemoveSession(SessionId);
				Playing = false;
			}
			StringBuilder sb = new StringBuilder();
			sb.Append("RTSP/1.0 200 OK\r\n");
			sb.AppendFormat("Cseq: {0}\r\n", parser.Headers["CSEQ"]);
			sb.AppendFormat("Session: {0}\r\n", parser.Headers["SESSION"]);
			sb.Append("Date: Sat, 22 Dec 2018 16:18:03 GMT\r\n\r\n");
			_connection.SendResponse(sb.ToString());
			return true;
		}

		private void SetupDestinations(Socket tcpSocket,
								IPEndPoint clientAddress,
								IPEndPoint destinationAddress,
								int clientRTPPort,
								int clientRTCPPort,
								byte rtpChannelId,
								byte rtcpChannelId)
		{
			if (destinationAddress == null)
				destinationAddress = clientAddress;
			
			if (tcpSocket == null)
			{ // UDP

				_destinations = new Destination(destinationAddress.Address, clientRTPPort, clientRTCPPort);
			}
			else
			{ // TCP

				_destinations = new Destination(tcpSocket, rtpChannelId, rtcpChannelId);

			}
		}

		

		public void Close()
		{

			if (!Closed)
			{
				try
				{
					if (_rtpStream != null)
					{
						_rtpStream.RemoveSession(SessionId);
						_rtpStream = null;
					}
				}
				catch (Exception ex)
				{
					_logger.Error(ex);
				}
				finally
				{
					if (_logger.IsDebugEnabled)
						_logger.Debug("Removing session {0}", SessionId);

					_connection.Server.RemoveSession(SessionId);
					Closed = true;
				}
				if (_logger.IsDebugEnabled)
					_logger.Debug("Session {0} is closed", SessionId);

			}

		}
		
		#region IDisposable Support
		private bool disposedValue = false; // Pour détecter les appels redondants

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Close();
				}

				// TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
				// TODO: définir les champs de grande taille avec la valeur Null.
				
				disposedValue = true;
			}
		}

		// TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
		~RTSPSession()
		{
			
			Dispose(false);
		}

		// Ce code est ajouté pour implémenter correctement le modèle supprimable.
		public void Dispose()
		{
			// Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
			Dispose(true);
			// TODO: supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
			 GC.SuppressFinalize(this);
		}
		#endregion


	}
}
