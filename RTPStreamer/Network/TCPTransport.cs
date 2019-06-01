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

using RTPStreamer.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RTPStreamer.Network
{
	public class TCPTransport : INetworkTransport
	{
		Socket _socket;
		byte _channelId;
		byte[] _framingHeader = new byte[4];

		public TCPTransport(Socket socket, byte channelId)
		{
			_socket = socket;
			_channelId = channelId;
		}

		public void Close()
		{
		}

		public Task<UdpReceiveResult> ReceivePacket()
		{
			return null;
		}

		public async Task<bool> SendPacket(byte[] packet, IPEndPoint endPoint)
		{

			// Obviously, we are TCP...
			Debug.Assert(packet.Length <= MultiFramedRTPFramer.RTP_PAYLOAD_MAX_SIZE);
			_framingHeader[0] = (byte)'$';
			_framingHeader[1] = _channelId;
			_framingHeader[2] = (byte)(((packet.Length) & 0xFF00) >> 8);
			_framingHeader[3] = (byte)((packet.Length) & 0xFF);

			
			List<ArraySegment<byte>> tmp = new List<ArraySegment<byte>>();
			tmp.Add(new ArraySegment<byte>(_framingHeader));
			tmp.Add(new ArraySegment<byte>(packet));

			int bytesSent = await SendPacketInternal(tmp);
			if (bytesSent != packet.Length + 4)
				return false;

			return true;
		}

		private async Task<int> SendPacketInternal(List<ArraySegment<byte>> buffers)
		{
#if _WIN32
			Thread.Sleep(5);
#endif

			try
			{
				while (_socket.Poll(1000, SelectMode.SelectWrite))
				{
					int bytesSent = await _socket.SendAsync(buffers, SocketFlags.None);
					return bytesSent;
				}
				return 0;
			}
			catch (Exception ex) when (ex is ObjectDisposedException)
			{
				return 0;
			}
			catch (SocketException ex)
			{
				throw ex;
			}
		}
	}
}
