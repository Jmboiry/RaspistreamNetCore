/********
This work is an implementation in C# Net.Core by Jean-Marie Boiry <jean-marie.boiry@live.fr> 
of Python modules from pycamera (https://picamera.readthedocs.io/en/release-1.13/)
so the following copyright that apply to this software
**/
/**************************************
Copyright 2013-2017 Dave Jones<dave@waveform.org.uk>

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright
	  notice, this list of conditions and the following disclaimer.

	* Redistributions in binary form must reproduce the above copyright
	  notice, this list of conditions and the following disclaimer in the
	  documentation and/or other materials provided with the distribution.

	* Neither the name of the copyright holder nor the

	  names of its contributors may be used to endorse or promote products

	  derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.

******************************/

using System;
using System.Text;

namespace PiCamera.MMalObject
{
	public unsafe class MMalBuffer
	{
		public MMal.MMAL_BUFFER_HEADER_T* _buf;
		byte[] _buffer = null;

		public MMalBuffer(MMal.MMAL_BUFFER_HEADER_T* buf)
		{
			_buf = buf;
			Length = (int)_buf->length; 
			Timestamp = _buf->pts;
			Flags =_buf->flags; 

			_buffer = new byte[Length];
			byte* p = (byte*)_buf->data;
			for (int i = 0; i < Length; i++)
				_buffer[i] = p[i];
		}

		//def release(self):
		//Release a reference to the buffer.This is the opposing call to
		//:meth:`acquire`. Once all references have been released, the buffer
		//will be recycled.
		public void Release()
		{
			MMal.mmal_buffer_header_release(_buf);
		}

		public void Lock()
		{
			MMal.mmal_buffer_header_mem_lock(_buf);
		}

		public void Unlock()
		{
			MMal.mmal_buffer_header_mem_unlock(_buf);
		}

		public Int64 Timestamp { get; set; }
		
		public uint Flags { get; set; }
		
		public int Length { get; set; }
		
		public byte[] Data 
		{
			get
			{
				return _buffer;
			}
		}

		public static Tuple<int, string>[] BufferFlags = new Tuple<int, string>[]
		{
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_EOS, "MMAL_BUFFER_HEADER_FLAG_EOS"),
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_FRAME_START, "MMAL_BUFFER_HEADER_FLAG_FRAME_START"),
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_FRAME_END, "MMAL_BUFFER_HEADER_FLAG_FRAME_END"),
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_KEYFRAME, "MMAL_BUFFER_HEADER_FLAG_KEYFRAME"),
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_DISCONTINUITY, "MMAL_BUFFER_HEADER_FLAG_DISCONTINUITY"),
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_CONFIG, "MMAL_BUFFER_HEADER_FLAG_CONFIG"),
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_CODECSIDEINFO, "MMAL_BUFFER_HEADER_FLAG_CODECSIDEINFO"),
			/** Signals an encrypted payload */
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_ENCRYPTED, "MMAL_BUFFER_HEADER_FLAG_ENCRYPTED"),

			/** Signals a buffer containing side information */
			/** Signals that a buffer failed to be transmitted */
			/** Signals a buffer which is the snapshot/postview image from a stills capture */
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAGS_SNAPSHOT, "MMAL_BUFFER_HEADER_FLAGS_SNAPSHOT"),
			/** Signals a buffer which contains data known to be corrupted */
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_CORRUPTED, "MMAL_BUFFER_HEADER_FLAG_CORRUPTED"),
			/** Signals that a buffer failed to be transmitted */
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_TRANSMISSION_FAILED, "MMAL_BUFFER_HEADER_FLAG_TRANSMISSION_FAILED"),
			/** Signals the output buffer won't be used, just update reference frames */
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_DECODEONLY, "MMAL_BUFFER_HEADER_FLAG_DECODEONLY"),
			/** Signals that the end of the current payload ends a NAL */
			new Tuple<int, string>(MMal.MMAL_BUFFER_HEADER_FLAG_NAL_END, "MMAL_BUFFER_HEADER_FLAG_NAL_END")
		};


		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Payload: {0}, Timestamp {1} flags: ", Length, Timestamp);
			for(int i = 0; i < BufferFlags.Length; i++)
			{
				if ((Flags & BufferFlags[i].Item1) != 0)
				{
					//if (0 != BufferFlags.Length - 1)
					sb.Append("|");
					sb.AppendFormat("{0}", BufferFlags[i].Item2);
				}
				
			}

			return sb.ToString();
		}
	}
}
