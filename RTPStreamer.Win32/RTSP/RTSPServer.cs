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
using RTPStreamer.H264;
using RTPStreamer.Media;
using RTPStreamer.Network;
using RTPStreamer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;



namespace RTPStreamer.RTSP
{
	public class RTSPServer
	{
		public static PiCameraH264Broadcaster Broadcaster { get; private set; }
		static Logger _logger = LogManager.GetLogger("RTSPServer");

		public int Port { get; private set; }
		TcpListener _listener;
		
		public const string  ServerVersion = "RaspiCam V1.0";
		public static string[] _validParameters = new string[] { "WIDTH", "HEIGHT" };
		Dictionary<int, RTSPSession> _sessions = new Dictionary<int, RTSPSession>();
		Dictionary<string, ServerMediaSession> _mediaSessions = new Dictionary<string, ServerMediaSession>();
		Random random = new Random(1964);
		AutoResetEvent _event = new AutoResetEvent(false);

		public RTSPServer(int port, (int width, int height) resolution)
		{
			Broadcaster = new PiCameraH264Broadcaster(resolution);

			var hostName = Dns.GetHostName();
			IPAddress localAddr = IPAddress.Parse("0.0.0.0");
			_logger.Info("Server started on {0}, Rasberry camera resolution {1}", hostName, resolution);

			IPHostEntry entry = Dns.GetHostEntry(hostName);
			IPAddress address = entry.AddressList[0];
			foreach (var e in entry.AddressList)
			{
				_logger.Info("Entry : {0}", e.ToString());
				if (e.AddressFamily == AddressFamily.InterNetwork)
					address = e;
			}


			_logger.Info("Server Listening on {0}, port {1}", address, port);
			Port = port;
			_listener = new TcpListener(localAddr, Port);

			_logger.Info("Adjusting network presentation time");

			TimeVal tv = new TimeVal();
			RTPTime.GetTimestamp(ref tv);
		}

		

		public void Run()
		{
			_listener.Start();

			ListenToNewTcpConnection();
			AliveCheck();
		}

		public void Shutdown()
		{
			Console.WriteLine("ShutDown");


			_listener.Stop();


			Console.WriteLine("Closing camera");
			Broadcaster.Stop();

			Console.WriteLine("Exiting....");
		}

		private void ListenToNewTcpConnection()
		{

			Task.Run(async () =>
			{
				var tcpClient = await _listener.AcceptTcpClientAsync();
				if (_logger.IsDebugEnabled)
					_logger.Info("Accepted connection from {0}", tcpClient.Client.RemoteEndPoint);

				ListenToNewTcpConnection();


				RTSPClientConnection connection = new RTSPClientConnection(this, tcpClient);
				await connection.ProcessingLoop();

			});

		}

		private void AliveCheck()
		{
			while (true)
			{
				_event.WaitOne(1000 * 60 * 5); // wait 5 mn
				DateTime now = DateTime.Now;
				var sessions = _sessions.ToArray();
				foreach (var session in sessions)
				{
					TimeSpan timeSpan = now.Subtract(session.Value.LastKeepAlive);
					if (_logger.IsDebugEnabled)
						_logger.Debug("Session {0} active at {1}, elapsed {2}", session.Value.SessionId, session.Value.LastKeepAlive.ToString("F"), timeSpan);
					if (timeSpan.Minutes > 10)
					{
						if (_logger.IsDebugEnabled)
							_logger.Debug("Session idle for more than {0}. Closing it.", timeSpan);

						session.Value.Close();
					}
				}
			}
		}

		internal RTSPSession GenerateSession(RTSPClientConnection connection)
		{
			bool ok = false;
			int sessionId = 0;
			RTSPSession session = null;
			lock (_sessions)
			{
				do
				{
					sessionId = random.Next(10000000, 99999999);
					if (!_sessions.ContainsKey(sessionId))
					{
						ok = true;
						session = new RTSPSession(sessionId, connection);
						_sessions.Add(sessionId, session);
					}
				} while (!ok);
			}
			return session;
		}

		public void AddServerMediaSession(ServerMediaSession media)
		{
			_mediaSessions.Add(media.Name, media);
		}

		internal ServerMediaSession lookupServerMediaSession(string stream)
		{
			ServerMediaSession sms;
			stream = stream.ToLower();
			return _mediaSessions.TryGetValue(stream, out sms) ? sms : null;
		}


		internal RTSPSession GetSession(int session)
		{
			lock (_sessions)
			{
				if (_sessions.ContainsKey(session))
					return _sessions[session];
				else
					return null;
			}
		}

		internal void RemoveSession(int sessionId)
		{
			lock (_sessions)
			{
				if (!_sessions.ContainsKey(sessionId))
					_logger.Error(String.Format("Unknown session {0}.", sessionId));
				else
					_sessions.Remove(sessionId);

				if (_logger.IsDebugEnabled)
					_logger.Info("Num active session {0}", _sessions.Count);

			}
		}

		// Minimal implementation
		internal void NewParameters(Dictionary<string, string> parameters)
		{
			// Parameters have been checked
			foreach (var param in parameters)
			{
				if (!_validParameters.Contains(param.Key))
				{
					_logger.Debug("Invalid parameter: {0}", param.Key);
				}
			}

			if (!parameters.ContainsKey("WIDTH") && parameters.ContainsKey("HEIGHT"))
				_logger.Debug("Missing width and height. Only one found");
			int width = Convert.ToInt32(parameters["WIDTH"]);
			int height = Convert.ToInt32(parameters["HEIGHT"]);
			Broadcaster.PushResolution((width, height));
		}
	}
}
