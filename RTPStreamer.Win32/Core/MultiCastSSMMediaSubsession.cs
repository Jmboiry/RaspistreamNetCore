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

using RTPStreamer.Network;
using RTPStreamer.RTSP;
using RTPStreamer.Tools;
using System;
using System.Net;
using System.Text;

namespace RTPStreamer.Core
{
	public abstract class MultiCastSSMMediaSubsession : ServerMediaSubsession
	{
		short fInitialPortNum;
		bool fMultiplexRTCPWithRTP;

		protected DatagramTransport _rtpGroupsock;
		protected DatagramTransport _rtcpGroupsock;
		public override int ServerRTPPort { get; }
		public override int ServerRTCPPort { get; }
		public override bool IsMulticast => true;
		public string MulticastAdress = "224.168.100.2";
		
			
		public MultiCastSSMMediaSubsession(bool reuseFirstSource, short initialPortNum = 10000, bool multiplexRTCPWithRTP = false)
		{
			// Make sure RTP ports are even-numbered:
			//multicastAdress = chooseRandomIPv4SSMAddress();
			int firstPort = 2000;
			fInitialPortNum = (short)((initialPortNum + 1) & (~1));
			fMultiplexRTCPWithRTP = multiplexRTCPWithRTP;

			int serverPortNum;

			for (serverPortNum = fInitialPortNum; ; ++serverPortNum)
			{
				// Normal case: We're streaming RTP (over UDP or TCP).  Create a pair of
				//	 groupsocks(RTP and RTCP), with adjacent port numbers(RTP port number even).
				//	 (If we're multiplexing RTCP and RTP over the same port number, it can be odd or even.)
				//	NoReuse dummy(envir()); // ensures that we skip over ports that are already in use

				ServerRTPPort = serverPortNum;

				try
				{
					_rtpGroupsock = new DatagramTransport(firstPort++, 255, IPAddress.Parse(MulticastAdress));
				}
				catch (Exception)
				{
					_rtpGroupsock = null;
				}
				if (_rtpGroupsock == null)
					continue;

				if (fMultiplexRTCPWithRTP)
				{
					//Use the RTP 'groupsock' object for RTCP as well:

					ServerRTCPPort = ServerRTPPort;
					_rtcpGroupsock = _rtpGroupsock;
				}
				else
				{
					// Create a separate 'groupsock' object(with the next(odd) port number) for RTCP:
					ServerRTCPPort = ++serverPortNum;
					try
					{
						_rtcpGroupsock = new DatagramTransport(ServerRTCPPort, 255);
					}
					catch (Exception)
					{
						_rtcpGroupsock = null;
					}
					if (_rtcpGroupsock == null)
					{
						continue; // try again
					}
				}

				break; // success
			}
		}

		private uint chooseRandomIPv4SSMAddress()
		{
			// First, a hack to ensure that our random number generator is seeded:
			Random rand = new Random(314156);

			// Choose a random address in the range [232.0.1.0, 232.255.255.255)
			// i.e., [0xE8000100, 0xE8FFFFFF)
			uint first = 0xE8000100;
			uint lastPlus1 = 0xE8FFFFFF;
			uint range = lastPlus1 - first;
			
			var ip = first + ((uint)rand.Next() % range);
			return ip;
		}

		public override string GenerateSDPDescription()
		{
			StringBuilder sb = new StringBuilder();

			TimeVal timeVal = new TimeVal();
			RTPTime.GetTimestamp(ref timeVal);

			sb.Append("v= 0\r\n").
			AppendFormat("o=- {0}{1:06} 1 IN IP4 {2}\r\n", timeVal.tv_sec, timeVal.tv_usec, SocketExtensions.GetLocalIPV4Address()).
			AppendFormat("s=Session streamed by \"{0}\"\r\n", RTSPServer.ServerVersion).
			AppendFormat("i={0}\r\n", Name).
			Append("t=0 0\r\n").
			AppendFormat("a=tool:{0}\r\n", RTSPServer.ServerVersion).
			Append("a=type:broadcast\r\n").
			Append("a=control:*\r\n").
			AppendFormat("a=source-filter: incl IN IP4 * {0}\r\n", SocketExtensions.GetLocalIPV4Address()).
			Append("a=rtcp-unicast: reflection\r\n").
			Append("a=range:npt=0-\r\n").
			AppendFormat("a=x-qt-text-nam:Session streamed by {0}\r\n", RTSPServer.ServerVersion).
			AppendFormat("a=x-qt-text-inf:{0}\r\n", Name).
			AppendFormat("m=video {0} RTP/AVP 96\r\n", ServerRTPPort).
			AppendFormat("c=IN IP4 {0}/255\r\n", MulticastAdress).
			Append("b=AS:500\r\n").
			Append("a=rtpmap:96 H264/90000\r\n").
			Append("a=fmtp:96 packetization-mode=1;profile-level-id=640028;sprop-parameter-sets=J2QAKKwrQCgC3QDxImo=,KO4Pyw==\r\n").
			Append("a=control:track1\r\n");

			return sb.ToString();
		}
	}
}
