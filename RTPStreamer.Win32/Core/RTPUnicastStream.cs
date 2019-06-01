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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RTPStreamer.Core
{
	public class RTPUnicastStream : RTPStream
	{
		static Logger _logger = LogManager.GetLogger("RTPUnicastStream");

		List<Task> _rtcpTasks = new List<Task>();

		public RTPUnicastStream(string name, string subSession, DatagramTransport rtpTransport, DatagramTransport rtcpTransport) :
			base(name, subSession, rtpTransport, rtcpTransport)
		{
			_rtcpTasks.Add(RTCPSender());
			_rtcpTasks.Add(RTCPReader());
		}

		public override async Task OnRTPPacket(byte[] packet)
		{
			RTPSessionState[] transports = null;
			lock (Sessions)
			{
				transports = Sessions.Values.ToArray();
			}


			foreach (var transport in transports)
			{
				INetworkTransport network = transport.RTPTransport;
				IPEndPoint endPoint = transport.RTPIPEndPoint;
				try
				{
					bool success = await network.SendPacket(packet, endPoint);
				}
				
				catch (Exception ex)
				{
					_logger.Error(ex);
				}
			}
		}

		public async Task RTCPSender()
		{
			CancellationToken ct = tokenSender.Token;
			try
			{

				await Task.Run(async () =>
				{
					while (true)
					{
						var handleArray = new WaitHandle[] { ct.WaitHandle };
						//Waiting on wait handle to signal first
						var finishedId = WaitHandle.WaitAny(handleArray, 10000);
						if (finishedId == 0)
						{
							
							// Poll on this property if you have to do
							// other cleanup before throwing.
							if (ct.IsCancellationRequested)
							{
								// Clean up here, then...
								//sendBYE();
								if (_logger.IsDebugEnabled)
									_logger.Debug("RTCP Sender exiting");
								
								ct.ThrowIfCancellationRequested();
							}
						}
						//Transmitions.ReapOldMembers();

						if (SenderEvent.WaitOne(0) == true)
						{


							RTPSessionState[] sessions = null;
							lock (Sessions)
							{
								sessions = Sessions.Values.ToArray();
							}


							await SendSRReport(this, sessions);
						}
					}
				}, ct);
			}
			catch (Exception ex)
			{
				Console.WriteLine("RTCP Sender exiting");
			}
		}

	}
}
