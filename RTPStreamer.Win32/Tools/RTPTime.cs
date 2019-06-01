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
using System;
using System.Threading;

namespace RTPStreamer.Tools
{
	static class RTPTime
	{
		static bool _isInitialized = false;
		static long _initializeLock = 0;
		static DateTime _origin;
		static long _originTicks; 

		
		public static  int GetTimestamp(ref TimeVal tp)
		{
			
			if (!_isInitialized)
			{
				if (1 == Interlocked.Increment(ref _initializeLock))
				{
					//	// La valeur de cette propriété représente le nombre d’intervalles de 100 nanosecondes qui se sont écoulées depuis 12:00:00 minuit, 
					//	// le 1er janvier 0001(0 : 00:00 UTC 1er janvier 0001, dans le calendrier grégorien), qui représente DateTime.MinValue.
					//	// Il n’inclut pas le nombre de graduations qui sont attribuables aux secondes intercalaires.
					_origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
					_originTicks = _origin.Ticks;
					DateTimeGenerator myDTG;
					DateTime TheNetworkTime;
					DateTime TheLocalTime;
					TimeSpan elapsedSpan;

					myDTG = DateTimeGenerator.Instance;

					TheLocalTime = DateTime.UtcNow;
					TheNetworkTime = myDTG.GetNTPTime();

					elapsedSpan = TheNetworkTime.Subtract(TheLocalTime);

					Console.WriteLine("LocalTime {0}", TheLocalTime.ToString());
					Console.WriteLine("NetworkTime {0}", TheNetworkTime.ToString());

					Console.WriteLine("Origin Time {0} ", _origin.ToString());

					_originTicks = _origin.Ticks - elapsedSpan.Ticks;
					Console.WriteLine("   {0:N0} nanoseconds (10^9s)", elapsedSpan.Ticks * 100);

					Console.WriteLine("   {0:N0} origin ticks", _originTicks);
					Console.WriteLine("   {0:N0} delta ticks", elapsedSpan.Ticks);
					Console.WriteLine("   {0:N2} seconds", elapsedSpan.TotalSeconds);
					Console.WriteLine("   {0:N2} minutes", elapsedSpan.TotalMinutes);
					Console.WriteLine("   {0:N0} days, {1} hours, {2} minutes, {3} seconds, millisecond {4}",
									  elapsedSpan.Days, elapsedSpan.Hours,
									  elapsedSpan.Minutes, elapsedSpan.Seconds, elapsedSpan.TotalMilliseconds);

					Console.WriteLine("Corrected Time {0} ", _origin.ToString());

					_originTicks += elapsedSpan.Ticks;
					Console.WriteLine(" Corrected ticks  {0:N0} ticks", _originTicks);

					// next caller can use ticks for time calculation
					_isInitialized = true;

				}
				else
				{
					Interlocked.Decrement(ref _initializeLock);
					// wait until first caller has initialized static values
					while (!_isInitialized)
					{
						Thread.Sleep(1);
					}
				}
			}
			//DateTime now = DateTime.Now;
			DateTime now = DateTime.UtcNow;

			TimeSpan span = new TimeSpan(now.Ticks - _originTicks);

			tp.tv_sec = (uint)(span.TotalSeconds);
			tp.tv_usec = (uint)((span.TotalMilliseconds % 1000) * 1000);

			return 0;
			
			
		}
	}
}
