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
using System.Collections.Generic;
using System.Net;

namespace RTPStreamer.Core
{
	public class RTPTransmissionStatsDB
	{
		Dictionary<uint, RTPTransmissionStat> _receivers = new Dictionary<uint, RTPTransmissionStat>();
		RTPStream _stream;

		public RTPTransmissionStatsDB(RTPStream stream)
		{
			_stream = stream;
		}

		public void noteIncomingRR(uint SSRC, IPEndPoint lastFromAddress,
                 uint lossStats, uint lastPacketNumReceived,
                 uint jitter, uint lastSRTime, uint diffSR_RRTime)
		{
			RTPTransmissionStat stats = null;
			bool exists = _receivers.TryGetValue(SSRC, out stats);

			if (!exists)
			{
				// This is the first time we've heard of this SSRC.
				// Create a new record for it:
				stats = new RTPTransmissionStat(_stream, SSRC);
				_receivers.Add(SSRC, stats);
			}

			stats.noteIncomingRR(lastFromAddress, lossStats, lastPacketNumReceived, jitter, lastSRTime, diffSR_RRTime);
		}

		public void ReapOldMembers()
		{
			TimeVal now = new TimeVal();
			RTPTime.GetTimestamp(ref now);
			List<uint> oldMembers = new List<uint>();
			foreach (var stat in _receivers.Values)
			{
				var timeReceived = stat.fTimeReceived;
				if (now.tv_sec - timeReceived.tv_sec > 60)
					oldMembers.Add(stat.SSRC);
			}

			foreach (var ssrc in oldMembers)
			{
				Console.WriteLine("Removing SSRC {0} from TransmissionStats", ssrc);
				_receivers.Remove(ssrc);
			}
		}
	}
}
