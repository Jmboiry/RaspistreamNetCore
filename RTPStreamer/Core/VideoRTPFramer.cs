
using RTPStreamer.Network;
using RTPStreamer.RTSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTPStreamer.Core
{
	public class VideoRTPFramer : MultiFramedRTPFramer
	{
		public VideoRTPFramer(RTPStream client,
							byte rtpPayloadType,
							uint rtpTimestampFrequency,
							string rtpPayloadFormatName) :
			base(client, rtpPayloadType, rtpTimestampFrequency, rtpPayloadFormatName)
		{
		} 
	}
}
