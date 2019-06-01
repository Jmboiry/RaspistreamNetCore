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
using RTPStreamer.Tools;

using System;

namespace RTPStreamer.Core
{
	public abstract class RTPFramer
	{
		protected byte _rtPayloadType;
		protected uint _packetCount, _octetCount, _totalOctetCount /*incl RTP hdr*/;
		protected long fTotalOctetCountStartTime;
	
		protected ushort _sequenceNumber;
		uint _SSRC, _timestampBase;
		protected uint _timestampFrequency;
		bool _nextTimestampHasBeenPreset;
		bool _enableRTCPReports; // whether RTCP "SR" reports should be sent for this sink (default: True)
		string fRTPPayloadFormatName;
		uint fNumChannels;
			
		public RTPFramer(byte rtpPayloadType,
						uint rtpTimestampFrequency,
						string rtpPayloadFormatName,
						uint numChannels)
		{
			_rtPayloadType = rtpPayloadType;
			
			_timestampFrequency = rtpTimestampFrequency;
			_enableRTCPReports = true;
			fNumChannels = numChannels;
			fRTPPayloadFormatName = rtpPayloadFormatName;
			
			Random rand = new Random(751);

			_sequenceNumber = (ushort)rand.Next();
			_SSRC = (uint)rand.Next();
			_timestampBase = (uint)rand.Next();
			_nextTimestampHasBeenPreset = true;
		}

		
		public uint ConvertToRTPTimestamp(TimeVal tv)
		{
		  // Begin by converting from TimeVal units to RTP timestamp units:
			uint timestampIncrement = (uint) (_timestampFrequency * tv.tv_sec);
			timestampIncrement += (uint) (_timestampFrequency*(tv.tv_usec/1000000.0) + 0.5); // note: rounding

			// Then add this to our 'timestamp base':
			if (_nextTimestampHasBeenPreset)
			{
				_nextTimestampHasBeenPreset = false;
				
				// Make the returned timestamp the same as the current "fTimestampBase",
				// so that timestamps begin with the value that was previously preset:
				_timestampBase -= timestampIncrement;
			 }

			uint rtpTimestamp = _timestampBase + timestampIncrement;
			
			return rtpTimestamp;
		}

		public uint CurrentTimeStamp()
		{
			TimeVal timeNow = new TimeVal();
			RTPTime.GetTimestamp(ref timeNow);
			return ConvertToRTPTimestamp(timeNow);
		}

		public uint PresetNextTimestamp()
		{
			TimeVal timeNow = new TimeVal();
			RTPTime.GetTimestamp(ref timeNow);
			
			uint tsNow = ConvertToRTPTimestamp(timeNow);
			if (_nextTimestampHasBeenPreset == false)
			{
				// Ideally, we shouldn't preset the timestamp if there 
				_timestampBase = tsNow;
				_nextTimestampHasBeenPreset = true;
			}
			return tsNow;
		}

		public ushort CurrentSeqNo()
		{
			return _sequenceNumber;
		}

		public uint SSRC()
		{
			return _SSRC;
		}

		public bool EnableRTCPReports()
		{
			return _enableRTCPReports; 
		}

		public bool NextTimestampHasBeenPreset()
		{
			return _nextTimestampHasBeenPreset; 
		}

		public uint PacketCount()
		{
			return _packetCount;
		}

		public uint OctetCount() 
		{
			return _octetCount;
		}
	}
}
