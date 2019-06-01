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
using System.Text;

namespace RTPStreamer.Core
{
	public class SDESItem
	{
		byte[] _data = new byte[2 + 0xFF]; // first 2 bytes are tag and length

		public SDESItem(byte tag, string value)
		{
			var buffer = Encoding.ASCII.GetBytes(value);
			int length = buffer.Length;
			if (buffer.Length > 0xFF)
				length = 0xFF; // maximum data length for a SDES item

			_data[0] = tag;
			_data[1] = (byte)length;
			Array.Copy(buffer, 0, _data, 2, length);
		}

		public byte[] data()
		{
			return _data;
		}

		public int totalSize()
		{
			return 2 + _data[1];
		}

	
 	}
}
