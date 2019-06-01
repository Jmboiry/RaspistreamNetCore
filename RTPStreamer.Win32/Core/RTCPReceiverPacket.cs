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

using System.Collections.Generic;
using System.Net;

namespace RTPStreamer.Core
{
	public class RTCPReceiverPacket : RTCPPacket
	{
		List<RTCPRRRecord> RRRecords = new List<RTCPRRRecord>();

		public bool ParseReport(RTCPPacket packet, RTPStream stream, IPEndPoint fromAddress)
		{
			uint reportBlocksSize = (uint)(packet.ReportCount * (6 * 4));
			int length = (int)packet.Reader.BaseStream.Length - (int) packet.Reader.BaseStream.Position;
			if (length < reportBlocksSize)
				return false;

			for (int i = 0; i < packet.ReportCount; ++i)
			{
				uint senderSSRC = unchecked((uint)IPAddress.NetworkToHostOrder(packet.Reader.ReadInt32()));
				if (_logger.IsTraceEnabled)
					_logger.Trace("\tSSRC/CSRC: {0:x8}", packet.SenderSSRC);

				// We care only about reports about our own transmission, not others'
				if (senderSSRC == stream.SSRC())
				{
					var RR = new RTCPRRRecord()
					{
						SenderSSRC = senderSSRC,
						LossStats = unchecked((uint)IPAddress.NetworkToHostOrder(packet.Reader.ReadInt32())),
						HighestReceived = unchecked((uint)IPAddress.NetworkToHostOrder(packet.Reader.ReadInt32())),
						Jitter = unchecked((uint)IPAddress.NetworkToHostOrder(packet.Reader.ReadInt32())),
						TimeLastSR = unchecked((uint)IPAddress.NetworkToHostOrder(packet.Reader.ReadInt32())),
						TimeSinceLastSR = unchecked((uint)IPAddress.NetworkToHostOrder(packet.Reader.ReadInt32()))
					};
					stream.Transsmitions.noteIncomingRR(RR.SenderSSRC, fromAddress,
														RR.LossStats,
														RR.HighestReceived, 
														RR.Jitter,
														RR.TimeLastSR,
														RR.TimeSinceLastSR);

					RRRecords.Add(RR);
				}

				
			}
			return true;
		}

		public double GetCumulativeFractionLostPackets()
		{
			double avgFractionLost = 0;
			int i = 0;
			foreach (var rr in RRRecords)
			{ 
				avgFractionLost += ((rr.LossStats & 0xFF000000UL) >> 24);
				avgFractionLost /= (i + 1);
				i++;
			}

			return avgFractionLost;
		}


		public double GetCumulativeJitter()
		{
			double avgJitter = 0;
			int i = 0;
			foreach(var rr in RRRecords)
			{
				avgJitter += rr.Jitter;
				avgJitter /= (i + 1);
				i++;
			}

			return avgJitter;
		}


		public uint GetCumulativeTotalLostPackets()
		{
			uint totalLostPackets = 0;
			
			foreach (var rr in RRRecords)
				totalLostPackets += (uint)((0x00FFFFFFUL & rr.LossStats));

			return totalLostPackets;
		}
	}
}
