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

using System.Text;

namespace RTPStreamer.Core
{
	public class RTCPSDSEReport : RTCPPacket
	{
		public bool ParseReport(RTCPPacket packet)
		{
			int length = (int)packet.Reader.BaseStream.Length - (int)packet.Reader.BaseStream.Position;
			if (_logger.IsTraceEnabled)
				_logger.Trace("\tSSRC/CSRC: {0:x8}", packet.SenderSSRC);


			// 'Handle' SDES packets only in debugging code, by printing out the 'SDES items':
			// Process each 'chunk':
			bool chunkOK = false;
			
			while (length >= 8)
			{
				// A valid chunk must be at least 8 bytes long
				chunkOK = false; // until we learn otherwise

				// Process each 'SDES item' in the chunk:
				byte itemType = packet.Reader.ReadByte();
				//ADVANCE(1); 
				--length;
				while (itemType != 0)
				{
					int itemLen = packet.Reader.ReadByte();
					
					--length;
					
					// Make sure "itemLen" allows for at least 1 zero byte at the end of the chunk:
					if (itemLen + 1 > length /*|| received[br.BaseStream.Position + itemLen] != 0*/)
						return false;
					var str = Encoding.ASCII.GetString(packet.Reader.ReadBytes(itemLen));
					if (_logger.IsTraceEnabled)
						_logger.Trace("\t\t{0}:{1}", itemType == 1 ? "CNAME" :
							itemType == 2 ? "NAME" :
							itemType == 3 ? "EMAIL" :
							itemType == 4 ? "PHONE" :
							itemType == 5 ? "LOC" :
							itemType == 6 ? "TOOL" :
							itemType == 7 ? "NOTE" :
							itemType == 8 ? "PRIV" :
							"(unknown)",
							itemType < 8 ? str : "???"); // hack, because we know it's '\0'-terminated
																									 //: "???"/* don't try to print out PRIV or unknown items */);
																									 /*ADVANCE(itemLen); */
					length -= itemLen;

					itemType = packet.Reader.ReadByte();
					--length;
				}
				if (itemType != 0)
					return false; // bad 'SDES item'

				// Thus, itemType == 0.  This zero 'type' marks the end of the list of SDES items.
				// Skip over remaining zero padding bytes, so that this chunk ends on a 4-byte boundary:
				while (length % 4 > 0 && packet.Reader.ReadByte() == 0)
				{
					//ADVANCE(1);
					--length;
				}
				if (length % 4 > 0)
					return false; // Bad (non-zero) padding byte

				chunkOK = true;
			}
			if (!chunkOK || length > 0)
				return false; // bad chunk, or not enough bytes for the last chunk

			return true;
		}
	}
}