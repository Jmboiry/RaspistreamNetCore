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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;

namespace RTPStreamer.Tools
{
	public unsafe class PacketBuffer
	{
		public static int maxSize = 100000; // by default

		int _packetStart, _curOffset, _preferred, _max, _limit;
		byte[] fBuf;
		
		int _overflowDataOffset, _overflowDataSize;

		public PacketBuffer(int preferredPacketSize, int maxPacketSize, int maxBufferSize = 0)
		{
			_preferred = preferredPacketSize;
			_max = maxPacketSize;
			_overflowDataSize = 0;
			if (maxBufferSize == 0)
				maxBufferSize = 60000;
			int maxNumPackets = (maxBufferSize + (maxPacketSize - 1)) / maxPacketSize;
			_limit = maxNumPackets * maxPacketSize;
			fBuf = new byte[_limit];
			ResetPacketStart();
			ResetOffset();
			
		}

		public int TotalBytesAvailable()
		{
			return _limit - (_packetStart + _curOffset);
		}

		public void ResetPacketStart()
		{
			if (_overflowDataSize > 0)
			{
				_overflowDataOffset += _packetStart;
			}
			_packetStart = 0;
		}

		public void ResetOffset()
		{
			_curOffset = 0;
		}

		
		public byte* CurPtr()
		{
			fixed (byte* ptr = &fBuf[_packetStart + _curOffset])
			{
				return ptr;
			}
		}
		

		public void WriteBytes(byte* from, int numBytes)
		{
			if (numBytes > TotalBytesAvailable())
				numBytes = TotalBytesAvailable();
			
			byte* ptr = CurPtr();
			{
				if (ptr != from)
					MemCpy(ptr, from, numBytes);
				Increment(numBytes);
			}
		}

		void Increment(int numBytes)
		{
			_curOffset += numBytes;
		}

		public int CurPacketSize()
		{
			return _curOffset;
		}

		public void SkipBytes(int numBytes)
		{
			if (numBytes > TotalBytesAvailable())
				numBytes = TotalBytesAvailable();

			Increment(numBytes);
		}

		void MemCpy(byte* dest, byte* src, int len)
		{
			for (int i = 0; i < len; i++)
				dest[i] = src[i];
		}

		public void WriteWord(uint word)
		{
			uint nWord = (uint)IPAddress.HostToNetworkOrder((int)word);
			WriteBytes((byte*)&nWord, 4);
		}

		
		public ReadOnlySpan<byte> Packet(int packetsize)
		{
			fixed (void* ptr = &fBuf[_packetStart])
			{
				return new ReadOnlySpan<byte>(ptr, (int)packetsize);
			}
		}

		public void AddBuffer(byte[] buffer)
		{
			fixed (byte* ptr = &fBuf[_curOffset])
			{
				for (int i = 0; i < buffer.Length; i++)
					ptr[i] = buffer[i];
			}
			Increment(buffer.Length);
		}

		
		void Insert(byte* from, int numBytes, int toPosition)
		{
			int realToPosition = _packetStart + toPosition;
			if (realToPosition + numBytes > _limit)
			{
				if (realToPosition > _limit) return; // we can't do this
				numBytes = _limit - realToPosition;
			}
			fixed (byte* ptr = &fBuf[realToPosition])
			{
				MemCpy(ptr, from, numBytes);
			}
			if (toPosition + numBytes > _curOffset)
			{
				_curOffset = toPosition + numBytes;
			}
		}

		public void InsertWord(uint word, int toPosition)
		{
			uint nWord = (uint)IPAddress.HostToNetworkOrder((unchecked((int)word)));
			Insert((byte*) &nWord, 4, toPosition);
		}

		public uint ExtractWord(int fromPosition)
		{
			uint nWord;
			Extract((byte*) &nWord, 4, fromPosition);
			return (uint)IPAddress.NetworkToHostOrder((unchecked((int)nWord)));//	ntohl(nWord);
		}

		void Extract(byte* to, int numBytes, int fromPosition)
		{
			int realFromPosition = _packetStart + fromPosition;
			if (realFromPosition + numBytes > _limit)
			{ // sanity check
				if (realFromPosition > _limit)
					return;
				numBytes = _limit - realFromPosition;
			}

			fixed (byte* ptr = &fBuf[realFromPosition])
			{
				MemCpy(to, ptr, numBytes);
			}
		}
	}
}
