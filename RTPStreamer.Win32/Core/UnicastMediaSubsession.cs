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
using System;

namespace RTPStreamer.Core
{
	public abstract class UnicastMediaSubsession : ServerMediaSubsession
	{

		short fInitialPortNum;
		bool fMultiplexRTCPWithRTP;
		
		protected DatagramTransport _rtpGroupsock;
		protected DatagramTransport _rtcpGroupsock;
		public override int ServerRTPPort { get; }
		public override int ServerRTCPPort { get; }
		public override bool IsMulticast => false;

		public UnicastMediaSubsession(bool reuseFirstSource, short initialPortNum = 6970, bool multiplexRTCPWithRTP = false)
		{
			// Make sure RTP ports are even-numbered:

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
					_rtpGroupsock = new DatagramTransport(ServerRTPPort, 255);
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

		public override void GetStreamParameters(out int rtpServerPort, out int rtcpServerPort, out string multicastAddress)
		{
			rtpServerPort = ServerRTPPort;
			rtcpServerPort = ServerRTCPPort;
			multicastAddress = "";
		}
	}
}
