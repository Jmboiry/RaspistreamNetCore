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

using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RTPStreamer.Network
{
	public class DatagramTransport : INetworkTransport
	{
		int _port;
		byte _ttl;
		UdpClient _udpClient;
		IPAddress _multicastAdress;

		public DatagramTransport(int port, byte ttl)
		{
			_port = port;
			_ttl = ttl;
			_udpClient = new UdpClient(_port);
		}

		public DatagramTransport(int port, byte ttl, IPAddress multicast)
		{
			_port = port;
			_ttl = ttl;
			_multicastAdress = multicast;
			_udpClient = new UdpClient(port);
			// Bind and listen. This constructor creates a socket 
			// and binds it to the port on which to receive data. The family 
			// parameter specifies that this connection uses an IPv6 address.
			
			// Join or create a multicast group. The multicast address ranges 
			// to use are specified in RFC#2375. You are free to use 
			// different addresses.

			MulticastOption option = new MulticastOption(_multicastAdress, IPAddress.Any);

			// Store the IPAdress multicast options.
			IPAddress group = option.Group;
			
									
			_udpClient.JoinMulticastGroup(group, _ttl);
					
		}


		public void Close()
		{
			if (_multicastAdress != null)
				_udpClient.DropMulticastGroup(_multicastAdress);
			_udpClient?.Close();
			
		}

		public async Task<UdpReceiveResult> ReceivePacket()
		{
			var task = await _udpClient.ReceiveAsync();

			return task;
		}


		public async Task<bool> SendPacket(byte[] packet, IPEndPoint endPoint)
		{
#if _WIN32
			Thread.Sleep(2);
#endif
			int bytesSent = await _udpClient.SendAsync(packet, packet.Length, endPoint);
			return (bytesSent == packet.Length);
		}
	}
}
