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
using System;
using System.Net;
using System.Threading.Tasks;

namespace RTPStreamer.Core
{
	public class RTCPSRPacket
	{
		static Logger _logger = LogManager.GetLogger("RTCPPacket");
		RTPSessionState _session;
		RTPStream _stream;
		PacketBuffer _fOutBuf;

		public RTCPSRPacket(RTPStream stream, RTPSessionState session)
		{
			_stream = stream;
			_session = session;
			_fOutBuf = new PacketBuffer(RTCPConstants.preferredRTCPPacketSize, RTCPConstants.maxRTCPPacketSize, RTCPConstants.maxRTCPPacketSize);
		}


		public async Task sendReport()
		{
			// Begin including a SR
			if (!addReport())
				return;

			// Then, include a SDES:
			addSDES();

			// Send the report:
			bool result = await sendBuiltPacket();
						
		}

		async Task<bool> sendBuiltPacket()
		{
			int reportSize = _fOutBuf.CurPacketSize();

			bool result = false;
			try
			{
				
				INetworkTransport transport = _session.RTCPTransport;
				IPEndPoint endPoint = _session.RTCPIPEndPoint;
				if (_logger.IsDebugEnabled)
					_logger.Debug("Sending SR report to : {0}", endPoint);

				result = await transport.SendPacket((_fOutBuf.Packet(reportSize).ToArray()), endPoint);
			}
			catch (Exception ex) when (ex is ObjectDisposedException)
			{
				// TCP connection has been closed and will be removed from the destinations list
			}
			
			_fOutBuf.ResetOffset();

			return result;
		}

		bool addReport()
		{
			// Include a SR, we're a server
			
			// Hack: Don't send a SR during those (brief) times when the timestamp of the
			// next outgoing RTP packet has been preset, to ensure that that timestamp gets
			// used for that outgoing packet.
			if (_stream.nextTimestampHasBeenPreset())
				return false;

			addSR();
			return true;
		}

		public void addSR()
		{
			enqueueCommonReportPrefix(RTCPConstants.RTCP_PT_SR, _stream.SSRC(),
						  5 /* extra words in a SR */);

			// Now, add the 'sender info' for our sink

			// Insert the NTP and RTP timestamps for the 'wallclock time':
			TimeVal timeNow = new TimeVal();
			RTPTime.GetTimestamp(ref timeNow);
			_fOutBuf.WriteWord((uint)(timeNow.tv_sec + 0x83AA7E80));
			// NTP timestamp most-significant word (1970 epoch -> 1900 epoch)
			double fractionalPart = (timeNow.tv_usec / 15625.0) * 0x04000000; // 2^32/10^6
			_fOutBuf.WriteWord((uint)(fractionalPart + 0.5));
			// NTP timestamp least-significant word
			uint rtpTimestamp = _stream.ConvertToRTPTimestamp(timeNow);
			_fOutBuf.WriteWord(rtpTimestamp); // RTP ts

			// Insert the packet and byte counts:
			uint packetCount = _stream.PacketCount() - _session.PacketsCount;
			uint octetCount = _stream.OctetCount() - _session.OctetCount;
 			_fOutBuf.WriteWord(packetCount);
			_fOutBuf.WriteWord(octetCount);
			if (_logger.IsDebugEnabled)
				_logger.Debug("Sending report : {0}.{1} packet {2} octet {3} Sequence {4}", timeNow.tv_sec + 0x83AA7E80,
								  (uint)(fractionalPart + 0.5), packetCount, octetCount, _stream.CurrentSeqNo());

		}

		unsafe void addSDES()
		{
			// For now we support only the CNAME item; later support more #####

			// Begin by figuring out the size of the entire SDES report:
			int numBytes = 4;
			// counts the SSRC, but not the header; it'll get subtracted out
			numBytes += _stream.fCNAME.totalSize(); // includes id and length
			numBytes += 1; // the special END item

			uint num4ByteWords = (uint)((numBytes + 3) / 4);

			uint rtcpHdr = 0x81000000; // version 2, no padding, 1 SSRC chunk
			rtcpHdr |= (RTCPConstants.RTCP_PT_SDES << 16);
			rtcpHdr |= num4ByteWords;
			_fOutBuf.WriteWord(rtcpHdr);


			_fOutBuf.WriteWord(_stream.SSRC());

			// Add the CNAME:
			fixed (byte* ptr = _stream.fCNAME.data())
			{
				_fOutBuf.WriteBytes(ptr, _stream.fCNAME.totalSize());
			}
			// Add the 'END' item (i.e., a zero byte), plus any more needed to pad:
			int numPaddingBytesNeeded = 4 - ((int)_fOutBuf.CurPacketSize() % 4);
			byte zero = 0;
			while (numPaddingBytesNeeded-- > 0)
				_fOutBuf.WriteBytes(&zero, 1);
		}

		void enqueueCommonReportPrefix(byte packetType,
								 uint SSRC,
								 uint numExtraWords = 0)
		{
			uint numReportingSources = 0;
						
			uint rtcpHdr = 0x80000000; // version 2, no padding
			rtcpHdr |= (numReportingSources << 24);
			rtcpHdr |= (uint)(packetType << 16);
			rtcpHdr |= (1 + numExtraWords + 6 * numReportingSources);
			// each report block is 6 32-bit words long
			_fOutBuf.WriteWord(rtcpHdr);

			_fOutBuf.WriteWord(SSRC);
		}
	}
}
