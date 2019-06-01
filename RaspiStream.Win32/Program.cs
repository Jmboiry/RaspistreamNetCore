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

using RTPStreamer.H264;
using RTPStreamer.Media;
using RTPStreamer.RTSP;
using System;
using System.Configuration;

namespace RTPStreamer
{
	class Program
	{
		static RTSPServer Server;

		static void Main(string[] args)
		{
			
			var appSettings = ConfigurationManager.AppSettings;
			string rtspPort = appSettings["rtspPort"] ?? "8554";
			int rtsp = 8554;
			try
			{
				rtsp = Convert.ToInt32(rtspPort);
			}
			catch
			{ }
			string resolution = appSettings["cameraResolution1"] ?? "800;600";
			Console.WriteLine("Camera resolution: {0}", resolution);

			var xy = resolution.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			if (xy.Length != 2)
				xy = new string[] { "800", "600" };
			int width = 800;
			int height = 600;
			try
			{
				width = Convert.ToInt32(xy[0]);
			}
			catch
			{
				width = 800;
			}

			try
			{
				height = Convert.ToInt32(xy[1]);
			}
			catch
			{
				height = 600;
			}

			Console.WriteLine("Width {0} Height {1}", width, height);


			Server = new RTSPServer(rtsp, (width, height));

			var streamName = "picamera";

			ServerMediaSession unicast = new ServerMediaSession(streamName, streamName, String.Format("Session streamed by \"{0}\"", RTSPServer.ServerVersion));

			unicast.AddSubsession(new H264VideoCaptureMediaSubsession());
			Server.AddServerMediaSession(unicast);

			var multicast = new ServerMediaSession("pimulticast", "pimulticast", String.Format("Session streamed by \"{0}\"", RTSPServer.ServerVersion));
			multicast.AddSubsession(new H264MulticastVideoCaptureSubsession());

			Server.AddServerMediaSession(multicast);


			Server.Run();

			
			
			//Server.Shutdown();
		}

		
	}
}
