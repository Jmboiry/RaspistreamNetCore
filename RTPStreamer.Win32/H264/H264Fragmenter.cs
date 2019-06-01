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
using RTPStreamer.Tools;
using System;
using System.Diagnostics;
using System.IO;

namespace RTPStreamer.H264
{
	// Fragment a nal unit payload according to
	// https://tools.ietf.org/html/rfc6184
	// 
	public class H264Fragmenter 
	{
		int _inputBufferSize;
		int _maxOutputPacketSize;
		byte[] _inputBuffer;
		int _currentDataOffset;

		int _maxSize;
		static Logger _logger = LogManager.GetLogger("H264Fragmenter");
		PiCameraH264Broadcaster _broadcaster;
		int sequenceNumber;

		public H264Fragmenter(int inputBufferMax, int maxOutputPacketSize) 
		{
			_maxSize = 60000;
			_inputBufferSize = inputBufferMax + 1;
			_maxOutputPacketSize = maxOutputPacketSize;
			_inputBuffer = new byte[_inputBufferSize];
			reset();
		}

		public H264Fragmenter(PiCameraH264Broadcaster broadcaster, int inputBufferMax, int maxOutputPacketSize) :
			this(inputBufferMax, maxOutputPacketSize)
		{
			_broadcaster = broadcaster;
		}

		void reset()
		{
			_currentDataOffset = 1;
		}


		public void OnNewNalUnit(byte[] frame, bool completedNalUnit, bool pictureEndMarker)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Buffer {0}, {1}", frame.Length, _inputBuffer.Length);

			int frameSize; // out

			Array.Copy(frame, 0, _inputBuffer, 1, frame.Length);
			int numValidDataBytes = frame.Length + 1;
			int numDelivered = 0;
			byte[] fragment;

			//Dump(buffer, (uint)buffer.Length, nalCompletedNalUnit, fPictureEndMarker);


			// We have NAL unit data in the buffer.  There are two cases to consider:
			// 1. There is a new NAL unit in the buffer, and it's small enough to deliver
			//    to the RTP sink (as is).
			// 2. 1) There is a new NAL unit in the buffer, but it's too large to deliver to
			//    the RTP sink in its entirety.  Deliver the first fragment of this data,
			//    as a FU packet, with one extra preceding header byte (for the "FU header").
			//    then 
			//    2) Deliver the next fragment of this data,
			//    as a FU packet, with two (H.264) extra preceding header bytes
			//    (for the "NAL header" and the "FU header").
			if (_maxSize < _maxOutputPacketSize)
			{ 
				// shouldn't happen
				_logger.Error("MaxSize ({0}) is smaller than expected", _maxSize);
				throw new Exception(string.Format("MaxSize ({0}) is smaller than expected", _maxSize));
			}
			else
			{
				_maxSize = _maxOutputPacketSize;
			}

			TimeVal tv = new TimeVal();
			RTPTime.GetTimestamp(ref tv);

			bool lastFragmentCompletedNALUnit = true; // by default
			if (_currentDataOffset == 1)
			{ 
				// case 1 or 2
				if (numValidDataBytes - 1 <= _maxSize)
				{ // case 1
					fragment = new byte[numValidDataBytes - 1];
					for (int i = 0; i < numValidDataBytes - 1; i++)
						fragment[i] = _inputBuffer[1 + i];
					
					frameSize = numValidDataBytes - 1;
					_currentDataOffset = numValidDataBytes;
					numDelivered = fragment.Length;

					_broadcaster.OnNewFragment(lastFragmentCompletedNALUnit, pictureEndMarker, fragment, tv);
				}
				else
				{
					// case 2
					// We need to send the NAL unit data as FU packets.  Deliver the first
					// packet now.  Note that we add "NAL header" and "FU header" bytes to the front
					// of the packet (overwriting the existing "NAL header").
					fragment = new byte[_maxSize];

					_inputBuffer[0] = (byte)((_inputBuffer[1] & 0xE0) | 28); // FU indicator
					_inputBuffer[1] = (byte)(0x80 | (_inputBuffer[1] & 0x1F)); // FU header (with S bit)

					for (int i = 0; i < _maxSize; i++)
						fragment[i] = _inputBuffer[i];
					frameSize = _maxSize;
					_currentDataOffset += _maxSize - 1;
					
					lastFragmentCompletedNALUnit = false;
					
					numDelivered += fragment.Length;
					
					_broadcaster.OnNewFragment(lastFragmentCompletedNALUnit, pictureEndMarker, fragment, tv);
					bool last = false;
					do
					{

						// case 3
						// We are sending this NAL unit data as FU packets.  We've already sent the
						// first packet (fragment).  Now, send the next fragment.  Note that we add
						// "NAL header" and "FU header" bytes to the front.  (We reuse these bytes that
						// we already sent for the first fragment, but clear the S bit, and add the E
						// bit if this is the last fragment.)
						int numExtraHeaderBytes;

						_inputBuffer[_currentDataOffset - 2] = _inputBuffer[0]; // FU indicator
						_inputBuffer[_currentDataOffset - 1] = (byte)(_inputBuffer[1] & ~0x80); // FU header (no S bit)
						numExtraHeaderBytes = 2;

						int numBytesToSend = numExtraHeaderBytes + (numValidDataBytes - _currentDataOffset);
						if (numBytesToSend > _maxSize)
						{
							// We can't send all of the remaining data this time:
							numBytesToSend = _maxSize;
							lastFragmentCompletedNALUnit = false;
						}
						else
						{
							// This is the last fragment:

							_inputBuffer[_currentDataOffset - 1] |= 0x40; // set the E bit in the FU header
							lastFragmentCompletedNALUnit = true;
							last = true;
						}
						fragment = new byte[numBytesToSend];
						numDelivered += fragment.Length;
						Array.Copy(_inputBuffer, _currentDataOffset - numExtraHeaderBytes, fragment, 0, numBytesToSend);
					

						if (last)
							lastFragmentCompletedNALUnit = completedNalUnit;
						_broadcaster.OnNewFragment(lastFragmentCompletedNALUnit, pictureEndMarker, fragment, tv);
						
						frameSize = numBytesToSend;
						_currentDataOffset += numBytesToSend - numExtraHeaderBytes;
						

					} while (!last);
				}
				if (_currentDataOffset >= numValidDataBytes)
				{
					// We're done with this data.  Reset the pointers for receiving new data:
					if (_logger.IsDebugEnabled)
						_logger.Debug("Done with NalUnit {0} {1}. NumDelivered {2}", sequenceNumber++, frame.Length, numDelivered);
					numDelivered = 0;

					numValidDataBytes = _currentDataOffset = 1;
				}
			}
		}

#if DEBUG

		void Dump(byte[] buff, uint len, bool nalCompletedNalUnit, bool fPictureEndMarker)
		{
			Debug.Assert(buff.Length == len);
			using (FileStream s = new FileStream("d:\\NetCoreNalUnit.txt", FileMode.OpenOrCreate, FileAccess.Write))
			{
				s.Seek(0, SeekOrigin.End);
				using (StreamWriter sw = new StreamWriter(s))
				{
					sw.WriteLine("NalUnit size: {0}, endAccessUnit: {1}, pictureComplete: {2}", buff.Length, nalCompletedNalUnit ? 1 : 0, fPictureEndMarker ? 1 : 0);
					for (int i = 0; i < buff.Length; ++i)
					{
						if ((i % 4) == 0)
							sw.Write(" ");
						sw.Write("{0:x2}", buff[i]);
						if (((i + 1) % 50) == 0)
							sw.WriteLine();
					}
					sw.WriteLine("");
				}
			}
		}
#endif
		
	}
}
