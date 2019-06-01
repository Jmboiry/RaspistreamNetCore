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
using RTPStreamer.Network;
using RTPStreamer.RTSP;
using RTPStreamer.Tools;
using System.Text;

namespace RTPStreamer.H264
{
	public class H264VideoCaptureMediaSubsession : UnicastMediaSubsession
	{
		public override string Name => "picamera";
		RTPStream _stream;

		public H264VideoCaptureMediaSubsession() :
			base(true)
		{

			_stream = new RTPUnicastStream(Name, "track1", _rtpGroupsock, _rtcpGroupsock);
		}

		public override RTPStream GetStreamInstance()
		{
			return _stream;
		}

		public override string GenerateSDPDescription()
		{
			StringBuilder body = new StringBuilder();

			TimeVal timeVal = new TimeVal();
			RTPTime.GetTimestamp(ref timeVal);
			body.Append("v=0\r\n").
				 AppendFormat("o=- {0}{1:06} 1 IN IP4 {2}\r\n", timeVal.tv_sec, timeVal.tv_usec, SocketExtensions.GetLocalIPV4Address()).
				 AppendFormat("s=Session streamed by \"{0}\"\r\n", RTSPServer.ServerVersion).
				 AppendFormat("i={0}\r\n", Name).
				 Append("t=0 0\r\n").
				 AppendFormat("a=tool:{0}\r\n", RTSPServer.ServerVersion).
				 Append("a=type:broadcast\r\n").
				 Append("a=control:*\r\n").
				 Append("a=range:npt=0-\r\n").
				 AppendFormat("a=x-qt-text-nam:Session streamed by \"{0}\"\r\n", RTSPServer.ServerVersion).
				 AppendFormat("a=x-qt-text-inf:{0}\r\n", Name).
				 Append("m=video 0 RTP/AVP 96\r\n").
				 Append("c=IN IP4 0.0.0.0\r\n").
				 Append("b=AS:500\r\n").
				 Append("a=rtpmap:96 H264/90000\r\n").
				 Append("a=fmtp:96 packetization-mode=1;profile-level-id=640028;sprop-parameter-sets=J2QAKKwrQCgC3QDxImo=,KO4Pyw==\r\n").
				 Append("a=control:track1\r\n");

			return body.ToString();
		}

	}
}
