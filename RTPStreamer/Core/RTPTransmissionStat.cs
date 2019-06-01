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
using RTPStreamer.Network;
using RTPStreamer.Tools;
using System.Net;

namespace RTPStreamer.Core
{
	public class RTPTransmissionStat
	{
		RTPStream _stream;
		public uint SSRC;
		IPEndPoint fLastFromAddress;
		uint fLastPacketNumReceived;
		byte fPacketLossRatio;
		uint fTotNumPacketsLost;
		uint fJitter;
		uint fLastSRTime;
		uint fDiffSR_RRTime;
		TimeVal fTimeCreated = new TimeVal();
		public TimeVal fTimeReceived = new TimeVal();
		bool fAtLeastTwoRRsHaveBeenReceived;
		uint fOldLastPacketNumReceived;
		uint fOldTotNumPacketsLost;
		bool fFirstPacket;
		uint fFirstPacketNumReported;
		uint fLastOctetCount, fTotalOctetCount_hi, fTotalOctetCount_lo;
		uint fLastPacketCount, fTotalPacketCount_hi, fTotalPacketCount_lo;
		Logger _logger = LogManager.GetLogger("RTPTransmissionStat");

		public RTPTransmissionStat(RTPStream stream, uint ssrc)
		{
			_stream = stream;
			SSRC = ssrc;
			fLastPacketNumReceived = 0;
			fPacketLossRatio = 0;
			fTotNumPacketsLost = 0;
			fJitter = 0;
			fLastSRTime = 0;
			fDiffSR_RRTime = 0;
			fAtLeastTwoRRsHaveBeenReceived = false;
			fFirstPacket = true;
			fTotalOctetCount_hi = 0;
			fTotalOctetCount_lo = 0;
			fTotalPacketCount_hi = 0;
			fTotalPacketCount_lo = 0;
			fTimeCreated = new TimeVal();

			RTPTime.GetTimestamp(ref fTimeCreated);

			fLastOctetCount = _stream.OctetCount();
			fLastPacketCount = _stream.PacketCount();
		}

		public void noteIncomingRR(IPEndPoint lastFromAddress, uint lossStats, uint lastPacketNumReceived, uint jitter, uint lastSRTime, uint diffSR_RRTime)
		{
			if (fFirstPacket)
			{
				fFirstPacket = false;
				fFirstPacketNumReported = lastPacketNumReceived;
			}
			else
			{
				fAtLeastTwoRRsHaveBeenReceived = true;
				fOldLastPacketNumReceived = fLastPacketNumReceived;
				fOldTotNumPacketsLost = fTotNumPacketsLost;
			}

			RTPTime.GetTimestamp(ref fTimeReceived);

			fLastFromAddress = lastFromAddress;
			fPacketLossRatio = (byte)(lossStats >> 24);
			fTotNumPacketsLost = lossStats & 0xFFFFFF;
			fLastPacketNumReceived = lastPacketNumReceived;
			fJitter = jitter;
			fLastSRTime = lastSRTime;
			fDiffSR_RRTime = diffSR_RRTime;

			if (_logger.IsDebugEnabled)
				_logger.Debug("RTCP RR data (received at {0}.{1}): lossStats {2:x8}, TotNumLost {3} lastPacketNumReceived {4}, jitter {5}, lastSRTime {6}, diffSR_RRTime {7} from SSRC {0}",
													fTimeReceived.tv_sec, fTimeReceived.tv_usec, lossStats, fTotNumPacketsLost,  lastPacketNumReceived, jitter, lastSRTime, diffSR_RRTime, SSRC);

			if (_logger.IsDebugEnabled)
			{
				uint rtd = roundTripDelay();
				_logger.Debug("=> round-trip delay: {0:x4} (== {1} seconds)", rtd, rtd / 65536.0);
			}

			// Update our counts of the total number of octets and packets sent towards
			// this receiver:
			uint newOctetCount = _stream.OctetCount();
			uint octetCountDiff = newOctetCount - fLastOctetCount;
			fLastOctetCount = newOctetCount;
			uint prevTotalOctetCount_lo = fTotalOctetCount_lo;
			fTotalOctetCount_lo += octetCountDiff;
			if (fTotalOctetCount_lo < prevTotalOctetCount_lo)
			{ // wrap around
				++fTotalOctetCount_hi;
			}

			uint newPacketCount = _stream.PacketCount();
			uint packetCountDiff = newPacketCount - fLastPacketCount;
			fLastPacketCount = newPacketCount;
			uint prevTotalPacketCount_lo = fTotalPacketCount_lo;
			fTotalPacketCount_lo += packetCountDiff;
			if (fTotalPacketCount_lo < prevTotalPacketCount_lo)
			{ // wrap around
				++fTotalPacketCount_hi;
			}
		}



		uint roundTripDelay()
		{
			// Compute the round-trip delay that was indicated by the most recently-received
			// RTCP RR packet.  Use the method noted in the RTP/RTCP specification (RFC 3350).

			if (fLastSRTime == 0)
			{
				// Either no RTCP RR packet has been received yet, or else the
				// reporting receiver has not yet received any RTCP SR packets from us:
				return 0;
			}

			// First, convert the time that we received the last RTCP RR packet to NTP format,
			// in units of 1/65536 (2^-16) seconds:
			uint lastReceivedTimeNTP_high = fTimeReceived.tv_sec + 0x83AA7E80; // 1970 epoch -> 1900 epoch
			double fractionalPart = (fTimeReceived.tv_usec * 0x0400) / 15625.0; // 2^16/10^6
			uint lastReceivedTimeNTP = (uint)((lastReceivedTimeNTP_high << 16) + fractionalPart + 0.5);

			int rawResult = unchecked((int)(lastReceivedTimeNTP - fLastSRTime - fDiffSR_RRTime));
			if (rawResult < 0)
			{
				// This can happen if there's clock drift between the sender and receiver,
				// and if the round-trip time was very small.
				rawResult = 0;
			}
			return (uint)rawResult;
		}
	}
}
