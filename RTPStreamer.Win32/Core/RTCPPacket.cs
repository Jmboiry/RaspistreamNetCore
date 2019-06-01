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
using System.IO;
using System.Net;

namespace RTPStreamer.Core
{
	public class RTCPPacket
	{
		public const int MinPacketSize = 8;
		protected static Logger _logger = LogManager.GetLogger("RTCPPacket");

		public uint Header { get; private set; }
		public byte PacketType { get; private set; }
		public byte ReportCount { get; private set; }
		public uint PacketLen { get; private set; }
		public uint SenderSSRC { get; private set; }
		public BinaryReader Reader { get; private set; }

		//returns true if successful, false otherwise
		public bool ParsePacket(BinaryReader packet)
		{
			Reader = packet;
			if (packet.BaseStream.Length < MinPacketSize)
				return false;

			// Check the RTCP packet for validity:
			// It must at least contain a header (4 bytes), and this header
			// must be version=2, with no padding bit, and a payload type of
			// SR (200), RR (201), or APP (204):
			Header = (uint)IPAddress.NetworkToHostOrder((packet.ReadInt32()));
			if ((Header & 0xE0FE0000) != (0x80000000 | (RTCPConstants.RTCP_PT_SR << 16)) && (Header & 0xE0FF0000) != (0x80000000 | (RTCPConstants.RTCP_PT_APP << 16)))
			{
				if (_logger.IsTraceEnabled)
					_logger.Trace("rejected bad RTCP packet: header 0x%08x\n");
				return false;
			}
			ReportCount = (byte)((Header >> 24) & 0x1F);
			PacketType = (byte)((Header >> 16) & 0xFF);
			PacketLen = 4 * (Header & 0xFFFF); // doesn't count hdr
			if (PacketLen > Reader.BaseStream.Length - Reader.BaseStream.Position)
				return false;
			SenderSSRC = (uint)IPAddress.NetworkToHostOrder(Reader.ReadInt32());

			return true;
		}

		public bool ParseSubPacket()
		{
			if (Reader.BaseStream.Length - Reader.BaseStream.Position  < MinPacketSize)
				return false;

			uint rtcpHdr = unchecked((uint)IPAddress.NetworkToHostOrder(Reader.ReadInt32())); ;
			if ((rtcpHdr & 0xC0000000) != 0x80000000)
			{
				if (_logger.IsTraceEnabled)
					_logger.Trace("bad RTCP subpacket: header {0:x8}", rtcpHdr);
				return false;
			}
			Header = rtcpHdr;
			ReportCount = (byte)((Header >> 24) & 0x1F);
			PacketType = (byte)((Header >> 16) & 0xFF);
			PacketLen = 4 * (Header & 0xFFFF); // doesn't count hdr
			SenderSSRC = (uint)IPAddress.NetworkToHostOrder(Reader.ReadInt32());

			return true;
		}
	}
}
