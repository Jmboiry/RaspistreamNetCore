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
using System.Diagnostics;
using System.IO;

namespace RTPStreamer.Tools
{

	public enum BitStreamMode
	{
		GF_BITSTREAM_READ = 0,
		GF_BITSTREAM_WRITE
	};

	public class BitStream 
	{
		BinaryReader _reader;
		BinaryWriter _writer;
		int _nbBits = 8;
		uint _current;
		Stream _stream;
		BitStreamMode _fileAccess;


		public BitStream(Stream stream, BitStreamMode fileAccess) 
		{
			_stream = stream;
			_fileAccess = fileAccess;

			switch (fileAccess)
			{
				case BitStreamMode.GF_BITSTREAM_READ:
					_nbBits = 8;
					_current = 0;
					_reader = new BinaryReader(_stream);
					break;
				case BitStreamMode.GF_BITSTREAM_WRITE:
					_writer = new BinaryWriter(_stream);
					_nbBits = 0;
					break;
			}
		}

		public void WriteUint32(uint value)
		{
			Debug.Assert(_nbBits == 0);
			BS_WriteByte((byte)((value >> 24) & 0xff));
			BS_WriteByte((byte)((value >> 16) & 0xff));
			BS_WriteByte((byte)((value >> 8) & 0xff));
			BS_WriteByte((byte)((value) & 0xff));
			
		}

		public byte[] ReadBytes(uint size)
		{
			byte[] buffer = new byte[size];
			_stream.Read(buffer, 0, (int)size);
			return buffer;

		}

		public void Seek(ulong offset)
		{
			_stream.Seek((long)offset, SeekOrigin.Begin);
		}

		private void BS_WriteByte(byte value)
		{
			if ((_fileAccess == BitStreamMode.GF_BITSTREAM_READ) /*|| (bs->bsmode == GF_BITSTREAM_FILE_READ)*/)
				throw new Exception("[BS] Attempt to write on read bitstream\n");

			_writer.Write(value);
		}

		public void WriteChars(char[] data, int count)
		{
			_writer.Write(data, 0, count);
		}


		public void WriteBytes(byte[] data)
		{
			_writer.Write(data);
		}

		public void WriteBits(int value, int nBits)
		{
			uint val; 
			int nb_shift;

			if (nBits == 0)
				return;
			//move to unsigned to avoid sanitizer warnings when we pass a value not codable on the given number of bits
			//we do this when setting bit fileds to all 1's
			val = (uint)value;
			nb_shift = sizeof(int) * 8 - nBits;
			if (nb_shift != 0)
				val <<= nb_shift;

			while (--nBits >= 0)
			{
				//but check value as signed
				byte temp = (byte)(((int)val) < 0 ? 1 : 0);
				BS_WriteBit(temp);
				val <<= 1;
			}
		}

		private void BS_WriteBit(uint bit)
		{
			_current <<= 1;
			_current |= bit;
			if (++_nbBits == 8)
			{
				_nbBits = 0;
				_writer.Write((byte)_current);
				_current = 0;
			}
		}

		public void WriteByte(uint value)
		{
			Debug.Assert(_nbBits == 0);
			BS_WriteByte((byte)value);
		}

		public void WriteUInt16(uint value)
		{
			Debug.Assert(_nbBits == 0);
			BS_WriteByte((byte)((value >> 8) & 0xff));
			BS_WriteByte((byte)((value) & 0xff));
		}

		public void WriteUInt24(uint value)
		{
			Debug.Assert(_nbBits == 0);
			BS_WriteByte((byte)((value >> 16) & 0xff));
			BS_WriteByte((byte)((value >> 8) & 0xff));
			BS_WriteByte((byte)((value) & 0xff));
		}

		internal void WriteUInt64(ulong value)
		{
			Debug.Assert(_nbBits == 0);
			WriteUint32((uint)((value >> 32) & 0xffffffff));
			WriteUint32((uint)(value & 0xffffffff));
		}

		public void gf_odf_write_base_descriptor(byte tag, uint size)
		{
			uint length;
			byte[] vals = new byte[4];


			length = size;
			vals[3] = (byte)(length & 0x7f);
			length >>= 7;
			vals[2] = (byte)((length & 0x7f) | 0x80);
			length >>= 7;
			vals[1] = (byte)((length & 0x7f) | 0x80);
			length >>= 7;
			vals[0] = (byte)((length & 0x7f) | 0x80);

			WriteBits(tag, 8);
			if (size < 0x00000080)
			{
				WriteBits(vals[3], 8);
			}
			else if (size < 0x00004000)
			{
				WriteBits(vals[2], 8);
				WriteBits(vals[3], 8);
			}
			else if (size < 0x00200000)
			{
				WriteBits(vals[1], 8);
				WriteBits(vals[2], 8);
				WriteBits(vals[3], 8);
			}
			else if (size < 0x10000000)
			{
				WriteBits(vals[0], 8);
				WriteBits(vals[1], 8);
				WriteBits(vals[2], 8);
				WriteBits(vals[3], 8);
			}
		}

		public byte[] GetStream()
		{
			byte[] buffer = new byte[_stream.Length];
			long pos = _stream.Position;
			_stream.Seek(0, SeekOrigin.Begin);
			_stream.Read(buffer, 0, (int) _stream.Length);
			_stream.Seek(pos, SeekOrigin.Begin);
			return buffer;
		}

		public uint ReadByte()
		{
			Debug.Assert(_nbBits == 8);
			return (uint)_reader.ReadByte();
		}

		public uint ReadBits(uint nBits)
		{
			uint ret;

			ret = 0;
			while (nBits-- > 0)
			{
				ret <<= 1;
				ret |= ReadBit();
			}
			return ret;
		}

		public byte ReadBit()
		{
			if (_nbBits == 8)
			{
				_current = BS_ReadByte();
				_nbBits = 0;
			}
			{
				int ret;
				_current <<= 1;
				_nbBits++;
				ret = (int)(_current & 0x100) >> 8;
				return (byte)ret;
			}
		}

		private byte BS_ReadByte()
		{
			return _reader.ReadByte();
		}
		
		public uint bs_get_se()
		{
			uint v = bs_get_ue();
			if ((v & 0x1) == 0)
				return (0 - (v >> 1));
			return (v + 1) >> 1;
		}

		static byte[] avc_golomb_bits = {
						8, 7, 6, 6, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 4, 3,
						3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2,
						2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2,
						2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
						1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
						0 };


		public uint bs_get_ue()
		{
			byte coded;
			int bits = 0; 
			uint read = 0;

			//while (true)
			{
				read = gf_bs_peek_bits(8, 0);
				//if (read != 0)
				//	break;
				//if (_stream.Position >= _stream.Length)
				//	throw new Exception("EOS");
				//bits += 8;
			}
			coded = avc_golomb_bits[read];
			ReadBits(coded);
			bits += coded;
			return ReadBits((uint)bits + 1) - 1;
		}

		private uint gf_bs_peek_bits( uint numBits, long byte_offset)
		{
			long curPos;
			int curBits;
			uint ret;
			uint current;

			if (numBits == 0 || (_stream.Length < _stream.Position + byte_offset))
				return 0;

			/*store our state*/
			curPos = _stream.Position;
			curBits = _nbBits;
			current = _current;

			if (byte_offset != 0)
				_stream.Seek(_stream.Position + byte_offset, SeekOrigin.Begin);
			ret = ReadBits(numBits);

			/*restore our cache - position*/
			_stream.Seek(curPos, SeekOrigin.Begin);
			/*to avoid re-reading our bits ...*/
			_nbBits = curBits;
			_current = current;
			return ret;
		}

		private bool gf_bs_is_align()
		{
			switch (_fileAccess)
			{
				case BitStreamMode.GF_BITSTREAM_READ:
					return ((8 == _nbBits) ? true: false);
				default:
					return _nbBits == 0;
			}
		}

		public ulong Position()
		{
			return (ulong)_stream.Position;
		}

		public ulong Size()
		{
			return (ulong) _stream.Length;
		}
	}
}
