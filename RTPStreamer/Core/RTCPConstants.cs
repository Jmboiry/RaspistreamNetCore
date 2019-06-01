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

namespace RTPStreamer.Core
{
	public static class RTCPConstants
	{
		public const int EVENT_UNKNOWN = 0;
		public const int EVENT_REPORT = 1;
		public const int EVENT_BYE = 2;

		public const int PACKET_UNKNOWN_TYPE = 0;
		public const int PACKET_RTP = 1;
		public const int PACKET_RTCP_REPORT = 2;
		public const int PACKET_BYE = 3;
		public const int PACKET_RTCP_APP = 4;

		// RTCP packet types:
		public const byte RTCP_PT_SR = 200;
		public const byte RTCP_PT_RR = 201;
		public const byte RTCP_PT_SDES = 202;
		public const byte RTCP_PT_BYE = 203;
		public const byte RTCP_PT_APP = 204;
		public const byte RTCP_PT_RTPFB = 205; // Generic RTP Feedback [RFC4585]
		public const byte RTCP_PT_PSFB = 206; // Payload-specific [RFC4585]
		public const byte RTCP_PT_XR = 207; // extended report [RFC3611]
		public const byte RTCP_PT_AVB = 208; // AVB RTCP packet ["Standard for Layer 3 Transport Protocol for Time Sensitive Applications in Local Area Networks." Work in progress.]
		public const byte RTCP_PT_RSI = 209; // Receiver Summary Information [RFC5760]
		public const byte RTCP_PT_TOKEN = 210; // Port Mapping [RFC6284]
		public const byte RTCP_PT_IDMS = 211; // IDMS Settings [RFC7272]

		// SDES tags:
		public const byte RTCP_SDES_END = 0;
		public const byte RTCP_SDES_CNAME = 1;
		public const byte RTCP_SDES_NAME = 2;
		public const byte RTCP_SDES_EMAIL = 3;
		public const byte RTCP_SDES_PHONE = 4;
		public const byte RTCP_SDES_LOC = 5;
		public const byte RTCP_SDES_TOOL = 6;
		public const byte RTCP_SDES_NOTE = 7;
		public const byte RTCP_SDES_PRIV = 8;

		public const int IP_UDP_HDR_SIZE = 28;
		public const int maxRTCPPacketSize = 1456;



		// bytes (1500, minus some allowance for IP, UDP, UMTP headers)
		public const int preferredRTCPPacketSize = 1000; // bytes

	}
}
