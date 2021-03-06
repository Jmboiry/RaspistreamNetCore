﻿/********
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

using System.Net;
using System.Net.Sockets;

namespace RTPStreamer.Network
{
	public class Destination
	{
		public bool isTCP = false;
		public IPAddress _address;
		public int rtpPort;
		public int rtcpPort;
		public byte rtpChannelId;
		public byte rtcpChannelId;
		public Socket _tcpSocket;

		public Destination(IPAddress address, int clientRTPPort, int clientRTCPPort)
		{
			_address = address;
			rtpPort = clientRTPPort;
			rtcpPort = clientRTCPPort;
		}

		public Destination(Socket socket, byte rtpChnlId, byte rtcpChnlId)
		{
			isTCP = true;
			_tcpSocket = socket;
			rtpChannelId = rtpChnlId;
			rtcpChannelId = rtcpChnlId;
		}
	}
}
