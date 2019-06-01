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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RTPStreamer.Core
{
	// Package and send nal unit payload according to
	// https://tools.ietf.org/html/rfc6184
	// 
	public class MultiFramedRTPFramer : RTPFramer
	{
		public const int RTP_PAYLOAD_MAX_SIZE = 1456;
		const int rtpHeaderSize = 12;

		PacketBuffer _outputBuffer;
		static Logger _logger = LogManager.GetLogger("MultiFramedRTPFramer");
		
			
		
		protected int _timestampPosition;
		protected int _specialHeaderPosition;
		protected int _specialHeaderSize; // size in bytes of any special header used
		protected int _curFrameSpecificHeaderPosition;
		protected int _curFrameSpecificHeaderSize; // size in bytes of cur frame-specific header
		protected int _totalFrameSpecificHeaderSizes; // size of all frame-specific hdrs in pkt
		protected int _maxPacketSize;
		
		RTPStream _client;

		public MultiFramedRTPFramer(RTPStream client, byte rtpPayloadType,
									uint rtpTimestampFrequency,
									string rtpPayloadFormatName,
									uint numChannels = 1) :
			base(rtpPayloadType, rtpTimestampFrequency, rtpPayloadFormatName, numChannels)
		{
			_client = client;
			
			_outputBuffer = new PacketBuffer(1000, RTP_PAYLOAD_MAX_SIZE);
			_maxPacketSize = RTP_PAYLOAD_MAX_SIZE; // save value, in case subclasses need it	setPacketSizes((RTP_PAYLOAD_PREFERRED_SIZE), (RTP_PAYLOAD_MAX_SIZE));
		}

		
		public void BuildPacket(byte[] buffer)
		{
			
			Debug.Assert(_outputBuffer.CurPacketSize() == 0);
			
			// Set up the RTP header:
			uint rtpHdr = 0x80000000; // RTP version 2; marker ('M') bit not set (by default; it can be set later)
			rtpHdr |= (uint)(_rtPayloadType << 16);
			rtpHdr |= _sequenceNumber; // sequence number
			_outputBuffer.WriteWord(rtpHdr);

			// Note where the RTP timestamp will go.
			// (We can't fill this in until we start packing payload frames.)
			_timestampPosition = (int) _outputBuffer.CurPacketSize();
			_outputBuffer.SkipBytes(4); // leave a hole for the timestamp

			_outputBuffer.WriteWord(SSRC());

			// Allow for a special, payload-format-specific header following the
			// RTP header:
			_specialHeaderPosition = (int)_outputBuffer.CurPacketSize();
			_specialHeaderSize = specialHeaderSize();
			_outputBuffer.SkipBytes(_specialHeaderSize);

			// First, skip over the space we'll use for any frame-specific header:
			_curFrameSpecificHeaderPosition = (int)_outputBuffer.CurPacketSize();
			_curFrameSpecificHeaderSize = frameSpecificHeaderSize();
			_outputBuffer.SkipBytes(_curFrameSpecificHeaderSize);
			_totalFrameSpecificHeaderSizes += _curFrameSpecificHeaderSize;

			// Then add the payload
			_outputBuffer.AddBuffer(buffer);
		}

		
		public async Task OnNewFrame(bool lastFragmentCompletedNalUnit, bool pictureEndMarker, byte[] buffer, TimeVal presentationTime)
		{
			Debug.Assert(buffer.Length + rtpHeaderSize <= RTP_PAYLOAD_MAX_SIZE);
			
			BuildPacket(buffer);
			
			uint frameSize = (uint)buffer.Length;
			uint numFrameBytesToUse = (uint)buffer.Length;
										
			// Here's where any payload format specific processing gets done:
			// Set the RTP 'M' (marker) bit if
			// 1/ The most recently delivered fragment was the end of (or the only fragment of) an NAL unit, and
			// 2/ This NAL unit was the last NAL unit of an 'access unit' (i.e. video frame).
			if (lastFragmentCompletedNalUnit && pictureEndMarker)
			{
				if (_logger.IsDebugEnabled)
					_logger.Debug("Setting the M bit");
				setMarkerBit();
			}
	
			SetTimestamp(presentationTime);
				
			// The packet is ready to be sent now
			await SendPacket();
		}

		public void ResetBuffer()
		{
			_outputBuffer.ResetPacketStart();
			_outputBuffer.ResetOffset();
		}

		async Task SendPacket()
		{
			
			// Send the packet:
#if TEST_LOSS
			// simulate packet loss here by sending modulo a random number
#endif
			//Dump(fOutBuf.packet(fOutBuf.curPacketSize()).ToArray(), fOutBuf.curPacketSize());
			int size = (int)_outputBuffer.CurPacketSize();
			
			byte[] packet = _outputBuffer.Packet(_outputBuffer.CurPacketSize()).ToArray();
			Debug.Assert(size <= RTP_PAYLOAD_MAX_SIZE);
				
			await _client.OnRTPPacket(packet);
				
			++_packetCount;
			_totalOctetCount += (uint)_outputBuffer.CurPacketSize();
			_octetCount += (uint)(_outputBuffer.CurPacketSize() - rtpHeaderSize - _specialHeaderSize - _totalFrameSpecificHeaderSizes);

			++_sequenceNumber; // for next time

			// Reset the packet start pointer back to the start:
			ResetBuffer();
		}

	
		public virtual bool frameCanAppearAfterPacketStart()
		{
			return false; // by default
		}

		protected void setMarkerBit()
		{
			uint rtpHdr = _outputBuffer.ExtractWord(0);
			rtpHdr |= 0x00800000;
			_outputBuffer.InsertWord(rtpHdr, 0);
		}

		protected void SetTimestamp(TimeVal framePresentationTime)
		{
			// First, convert the presentation time to a 32-bit RTP timestamp:
			uint currentTimestamp = ConvertToRTPTimestamp(framePresentationTime);
			
			// Then, insert it into the RTP packet:
			_outputBuffer.InsertWord(currentTimestamp, _timestampPosition);
		}

		// whether this frame can appear in position >1 in a pkt (default: True)
		protected virtual int specialHeaderSize()
		{
			// default implementation: Assume no special header:
			return 0;
		}

		// returns the size of any special header used (following the RTP header) (default: 0)
		protected virtual int frameSpecificHeaderSize()
		{
			// default implementation: Assume no frame-specific header:
			return 0;
		}

		int numPacket = 0;
		[Conditional("DEBUG")]
		void Dump(byte[] buff, uint len)
		{
			Debug.Assert(buff.Length == len);
			using (FileStream s = new FileStream("d:\\NetPacketDump.txt", FileMode.OpenOrCreate, FileAccess.Write))
			{
				s.Seek(0, SeekOrigin.End);
				using (StreamWriter sw = new StreamWriter(s))
				{
					sw.WriteLine("packet {0} size: {1}", numPacket++, buff.Length);
					for (int i = 0; i < buff.Length; ++i)
					{
						if ((i % 4) == 0)
							sw.Write(" ");
						sw.Write("{0:x2}", buff[i]);
						if (((i + 1) % 50) == 0)
							sw.WriteLine();
					}
					sw.WriteLine("");
				}
			}
		}

	}
}
