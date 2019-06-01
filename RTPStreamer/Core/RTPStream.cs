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
using RTPStreamer.Network;
using RTPStreamer.RTSP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RTPStreamer.Core
{
	public abstract class RTPStream
	{
		static Logger _logger = LogManager.GetLogger("RTPStream");

		H264VideoRTPFramer _framer;
		Dictionary<int, RTPSessionState> _sessions = new Dictionary<int, RTPSessionState>();

		public string Name => _name;
		public string SubSession => _subSession;

		public Dictionary<int, RTPSessionState> Sessions { get => _sessions; set => _sessions = value; }
		public H264VideoRTPFramer Framer { get => _framer; set => _framer = value; }
		public DatagramTransport RtpUdpClient { get => _rtpUdpClient; set => _rtpUdpClient = value; }
		public DatagramTransport RtcpUdpClient { get => _rtcpUdpClient; set => _rtcpUdpClient = value; }
		public ManualResetEvent SenderEvent { get => _senderEvent; set => _senderEvent = value; }
		public RTPTransmissionStatsDB Transsmitions { get => _transmitions; private set => _transmitions = value; }

		string _name;
		string _subSession;
		DatagramTransport _rtpUdpClient;
		DatagramTransport _rtcpUdpClient;
				
		public SDESItem fCNAME;

		RTPTransmissionStatsDB _transmitions;
		public CancellationTokenSource tokenSender = new CancellationTokenSource();
		public CancellationTokenSource tokenReader = new CancellationTokenSource();

		
		protected int _refCount;
		Stopwatch _sw;
		ManualResetEvent _senderEvent;

		public RTPStream(string name, string subSession, DatagramTransport rtpTransport, DatagramTransport rtcpTransport)
		{
			_name = name;
			_subSession = subSession;
			_refCount = 0;
			_sw = new Stopwatch();
			SenderEvent = new ManualResetEvent(false);
			RtpUdpClient = rtpTransport;
			RtcpUdpClient = rtcpTransport;


			Framer = new H264VideoRTPFramer(this, 96);

			fCNAME = new SDESItem(RTCPConstants.RTCP_SDES_CNAME, Dns.GetHostName());

			Transsmitions = new RTPTransmissionStatsDB(this);
		}

		protected void StartStream()
		{
			
			RTSPServer.Broadcaster.AddListener(Name, this);
			SenderEvent.Set();
		}


		protected void StopStream()
		{
			RTSPServer.Broadcaster.RemoveListener(Name);
			Framer = new H264VideoRTPFramer(this, 96);
			SenderEvent.Reset();
		}

		public void ResetStream()
		{
			Framer.ResetBuffer();
		}

		public async Task AddDestination(RTSPSession session, Destination destinations)
		{
			RTPSessionState sessionState = null;

			lock (Sessions)
			{
				if (Sessions.ContainsKey(session.SessionId))
					throw new ArgumentException(String.Format("Session {0} has already registered destination", session.SessionId));

				if (destinations.isTCP)
				{
					sessionState = new RTPSessionState()
					{
						TCP = true,
						RTPTransport = new TCPTransport(destinations._tcpSocket, destinations.rtpChannelId),
						RTCPTransport = new TCPTransport(destinations._tcpSocket, destinations.rtcpChannelId),
						RTPIPEndPoint = null,
						RTCPIPEndPoint = null,
						OctetCount = Framer.OctetCount(),
						PacketsCount = Framer.PacketCount()

					};

					Sessions.Add(session.SessionId, sessionState);
				}
				else
				{
					sessionState = new RTPSessionState()
					{
						TCP = false,
						RTPTransport = RtpUdpClient,
						RTCPTransport = RtcpUdpClient,
						RTPIPEndPoint = new IPEndPoint(destinations._address, destinations.rtpPort),
						RTCPIPEndPoint = new IPEndPoint(destinations._address, destinations.rtcpPort),
						OctetCount = Framer.OctetCount(),
						PacketsCount = Framer.PacketCount()
					};
					Sessions.Add(session.SessionId, sessionState);
				}
			}
			var sr = new RTCPSRPacket(this, sessionState);

			await sr.sendReport();

			if (Interlocked.Increment(ref _refCount) == 1)
				StartStream();
		}


		public void RemoveSession(int sessionId)
		{
			lock (Sessions)
			{
				if (!Sessions.ContainsKey(sessionId))
					throw new ArgumentException(String.Format("Session {0} isn't playing", sessionId));
				Sessions.Remove(sessionId);
			}

			if (Interlocked.Decrement(ref _refCount) == 0)
			{
				if (_logger.IsDebugEnabled)
					_logger.Debug("Stopping Camera");

				StopStream();
			}
		}


		public async Task OnNewFragment(bool lastFragmentCompletedNalUnit, bool pictureEndMarker, byte[] fragment, TimeVal tv)
		{
			await Framer.OnNewFrame(lastFragmentCompletedNalUnit, pictureEndMarker, fragment, tv);
		}


		public abstract Task OnRTPPacket(byte[] packet);

		public async virtual Task SendSRReport(RTPStream stream, RTPSessionState[] sessions)
		{

			foreach (var session in sessions)
			{
				var sr = new RTCPSRPacket(stream, session);
				await sr.sendReport();
			}
		}

		protected async Task RTCPReader()
		{
			CancellationToken ct = tokenReader.Token;
			try
			{

				await Task.Run(async () =>
				{


					do
					{
						var result = await RtcpUdpClient.ReceivePacket();

						if (ct.IsCancellationRequested)
						{
							//	//If token wait handle was first to signal then exit
							if (_logger.IsDebugEnabled)
								_logger.Debug("Reader Exiting");


							ct.ThrowIfCancellationRequested();
						}
						else
						{

							IncomingReportHandler(result.Buffer, result.RemoteEndPoint);

						}

					} while (true);

				}, ct);

			}
			catch (Exception ex)
			{

				Console.WriteLine("Task Reader Exiting");
			}

			Console.WriteLine("RTCP Reader exiting");
		}


		public void IncomingReportHandler(byte[] received, IPEndPoint fromAddress)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Incoming report size {0} from {1}", received.Length, fromAddress);

			ProcessIncomingReport(received, fromAddress);
		}

		[Conditional("DEBUG")]
		void Dump(byte[] buff, int len)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("RTCP Packet {0} len {1}\n", buff.Length, len);
			for (int i = 0; i < buff.Length; ++i)
			{
				if ((i % 4) == 0)
					sb.Append(" ");
				sb.AppendFormat("{0:x2}", buff[i]);
				if (((i + 1) % 50) == 0)
					sb.Append("\n");
			}
			_logger.Trace(sb.ToString());
		}



		private void ProcessIncomingReport(byte[] received, IPEndPoint fromAddress)
		{
			//if (_logger.IsTraceEnabled)
			//	Dump(received, packetSize);
			BinaryReader br = new BinaryReader(new MemoryStream(received));

			bool packetOK = false;
			RTCPPacket packet = new RTCPPacket();
			packetOK = packet.ParsePacket(br);
			if (!packetOK)
				return;

			while (br.BaseStream.Position < received.Length && packetOK)
			{

				switch (packet.PacketType)
				{
					case RTCPConstants.RTCP_PT_RR:
						{
							RTCPReceiverPacket receiver = new RTCPReceiverPacket();
							packetOK = receiver.ParseReport(packet, this, fromAddress);
						}
						break;
					case RTCPConstants.RTCP_PT_SDES:
						{
							RTCPSDSEReport sdes = new RTCPSDSEReport();
							packetOK = sdes.ParseReport(packet);
						}
						break;
					default:
						break;

				}
				if (!packetOK)
					break;

				packetOK = packet.ParseSubPacket();

			}
		}


		public bool nextTimestampHasBeenPreset()
		{
			return Framer.NextTimestampHasBeenPreset();
		}

		public uint SSRC()
		{
			return Framer.SSRC();
		}

		public uint OctetCount()
		{
			return Framer.OctetCount();
		}

		public uint PacketCount()
		{
			return Framer.PacketCount();
		}

		
		public uint ConvertToRTPTimestamp(TimeVal timeNow)
		{
			return Framer.ConvertToRTPTimestamp(timeNow);
		}

		public ushort CurrentSeqNo()
		{
			return Framer.CurrentSeqNo();
		}

		public uint CurrentTimeStamp()
		{
			return Framer.CurrentTimeStamp();
		}
	}
}
