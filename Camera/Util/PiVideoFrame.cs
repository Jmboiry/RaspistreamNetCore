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

namespace PiCamera.Util
{

	public enum VideoFrameType
	{
		//This class simply defines constants used to represent the type of a frame
		//in :attr:`PiVideoFrame.frame_type`. Effectively it is a namespace for an
		//enum.

		//.. attribute::frame

		//Indicates a predicted frame(P-frame). This is the most common frame

		//type.


		//..attribute:: key_frame

		//Indicates an intra-frame(I-frame) also known as a key frame.


		//..attribute:: sps_header

		//Indicates an inline SPS/PPS header(rather than picture data) which is
		//typically used as a split point.


		//..attribute:: motion_data

		//Indicates the frame is inline motion vector data, rather than picture

		//data.


		//..versionadded:: 1.5
		//"""
		frame = 0,
		key_frame = 1,
		sps_header = 2,
		motion_data = 3,
		None = 4
	}

	//	class PiVideoFrame(namedtuple('PiVideoFrame', (
	//   'index',         # the frame number, where the first frame is 0
	//   'frame_type',    # a constant indicating the frame type (see PiVideoFrameType)
	//   'frame_size',    # the size (in bytes) of the frame's data
	//   'video_size',    # the size (in bytes) of the video so far
	//   'split_size',    # the size (in bytes) of the video since the last split
	//   'timestamp',     # the presentation timestamp (PTS) of the frame
	//   'complete',      # whether the frame is complete or not
	//   ))):
	public class PiVideoFrame
	{
		public int index { set; get; }
		public VideoFrameType frame_type { set; get; }
		public uint flags { set; get; }
		public int frame_size { get; set; }
		public int video_size { get; set; }
		public bool complete { get; set; }
		public Int64 timestamp { get; set; }
		public byte[] data { get; set; }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Frame index: {0} type: {1}, size: {2} complete: {3} Timestamp: {4} Flags {5} Data {6}", index, frame_type, frame_size, complete, timestamp, flags, data.Length);
			return sb.ToString();
		}
	}
}
