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

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace RTPStreamer.Network
{
	public sealed class SocketAwaitable : INotifyCompletion
	{
		private readonly static Action SENTINEL = () => { };
		internal bool m_wasCompleted;
		internal Action m_continuation;
		internal SocketAsyncEventArgs m_eventArgs;
		public SocketAwaitable(SocketAsyncEventArgs eventArgs)
		{
			if (eventArgs == null)
				throw new ArgumentNullException("eventArgs");
			m_eventArgs = eventArgs;
			eventArgs.Completed += delegate
			{
				var prev = m_continuation ?? Interlocked.CompareExchange(
					ref m_continuation, SENTINEL, null);
				if (prev != null)
					prev();
			};
		}

		internal void Reset()
		{
			m_wasCompleted = false;
			m_continuation = null;
		}

		public SocketAwaitable GetAwaiter() { return this; }
		public bool IsCompleted { get { return m_wasCompleted; } }
		public void OnCompleted(Action continuation)
		{
			if (m_continuation == SENTINEL ||
				Interlocked.CompareExchange(
					ref m_continuation, continuation, null) == SENTINEL)
			{
				Task.Run(continuation);
			}
		}
		public int GetResult()
		{
			if (m_eventArgs.SocketError != SocketError.Success)
				throw new SocketException((int)m_eventArgs.SocketError);
			return m_eventArgs.BytesTransferred;
		}
	}


	public static class SocketExtensions
	{
		public static SocketAwaitable ReceiveAsync(this Socket socket, SocketAwaitable awaitable)
		{
			awaitable.Reset();
			if (!socket.ReceiveAsync(awaitable.m_eventArgs))
				awaitable.m_wasCompleted = true;
			return awaitable;
		}

		public static SocketAwaitable SendAsync(this Socket socket, SocketAwaitable awaitable)
		{
			awaitable.Reset();
			if (!socket.SendAsync(awaitable.m_eventArgs))
				awaitable.m_wasCompleted = true;
			return awaitable;
		}

		public static string GetLocalIPV4Address()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				//Console.WriteLine(ip);
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					return ip.ToString();
				}
			}
			throw new Exception("No network adapters with an IPv4 address in the system!");
		}
	}
}