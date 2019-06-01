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
using PiCamera.MMalObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using PiCamera.Util;

namespace PiCamera.Encoders
{
	public class PiVideoEncoder : PiEncoder
	{
		const int MAX_BITRATE_MJPEG = 25000000; // 25Mbits/s
		const int MAX_BITRATE_LEVEL4 = 25000000; // 25Mbits/s
		const int MAX_BITRATE_LEVEL42 = 62500000; // 62.5Mbits/s

		private static Logger _logger = LogManager.GetLogger("PiVideoEncoder");
		object _next_output = null;//[]
		PiVideoFrame _split_frame = null;
		PiVideoFrame _frame = null;
		

		public PiVideoEncoder(Camera parent, MMalPort cameraPort, MMalPort inputPort) :
			base(parent, cameraPort, inputPort)
		{
		}

		internal override MMalEncoder GetEncoder()
		{
			return new MMalVideoEncoder();
		}

		/// <summary>
		/// Default settings used: 
		///	bitrate=17000000, intra_period=None, profile='high',
		///	level='4', quantization=0, quality=0, inline_headers=True,
		/// sei=False, sps_timing=False, motion_output=None,
		///	intra_refresh=None
		//	Max bitrate we allow for recording
		/// </summary>

		public unsafe override void CreateEncoder(ImageFormat format, int resize, params string[] options)
		{

			if (_logger.IsDebugEnabled)
				_logger.Debug("CreateEncoder {0}", format);


			int bitrate = 17000000;
			bool inlineHeaders = true;
			bool sei = false;
			bool sps_timing = false;
			int quality = 23;


			uint mmalFormat;
			if (format == ImageFormat.h264)
				mmalFormat = MMal.MMAL_ENCODING_H264;
			else if (format == ImageFormat.mpeg)
				mmalFormat = MMal.MMAL_ENCODING_MJPEG;
			else
				throw new Exception(String.Format("Invalid video format {0}", format));

			_outputPort.Format = (uint)mmalFormat;

			if (format == ImageFormat.h264)
			{
				//MMal.MMAL_VIDEO_PROFILE_T _profile = MMal.MMAL_VIDEO_PROFILE_T.MMAL_VIDEO_PROFILE_H264_HIGH;
				//MMal.MMAL_VIDEO_LEVEL_T _level = MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_4;
				MMal.MMAL_VIDEO_PROFILE_T profile = GetProfile(options);
				MMal.MMAL_VIDEO_LEVEL_T level = GetLevel(options);

				MMal.MMAL_PARAMETER_VIDEO_PROFILE_T param = new MMal.MMAL_PARAMETER_VIDEO_PROFILE_T();
				param.hdr.id = (uint)MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_PROFILE;
				param.hdr.size = (uint)Marshal.SizeOf(param);
				param.profile = new MMal.VIDEO_PROFILE();
				param.profile.profile = profile;
				param.profile.level = level;

				MMal.MMAL_STATUS_T status = MMal.mmal_port_parameter_set(_outputPort.Pointer, &param.hdr);
				if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
					throw new Exception(String.Format("Cannot set parameter on port {0} {1}", _outputPort.Name, status));

				bitrate = GetBitrate(level, bitrate, options);
				// We need to set the frame rate on output to 0, to ensure it gets
				// updated correctly from the input framerate when port connected
				_outputPort.Framerate = (0, 1);

				_outputPort.BitRate = (uint)bitrate;
				_outputPort.Commit();
				if (_logger.IsDebugEnabled)
					_logger.Debug("Port Commited");

				if (inlineHeaders)
					_outputPort.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_VIDEO_ENCODE_INLINE_HEADER, true);
				if (sei)
					_outputPort.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_VIDEO_ENCODE_SEI_ENABLE, true);
				if (sps_timing)
					_outputPort.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_VIDEO_ENCODE_SPS_TIMING, true);

				// other H264 parameters must be set here
			}
			else
			{
				_outputPort.BitRate = 1;
				_outputPort.Commit();
			}
			if (quality != 0)
			{
				_outputPort.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_VIDEO_ENCODE_INITIAL_QUANT, quality);
				_outputPort.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_VIDEO_ENCODE_MIN_QUANT, quality);
				_outputPort.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_VIDEO_ENCODE_MAX_QUANT, quality);
			}

			Encoder.Inputs[0].SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_VIDEO_IMMUTABLE_INPUT, true);
			Encoder.Enable();

			if (_logger.IsDebugEnabled)
				_logger.Debug("PiVideoEncoder.CreateEncoder done Output port {0}", _outputPort.ToString());

			base.CreateEncoder(format, resize, options);
		}


		public override void Start(Func<MMalBuffer, bool> action)
		{
			base.Start(action);
		}


		public override void Stop()
		{
			base.Stop();
		}

		private static Tuple<string, MMal.MMAL_VIDEO_PROFILE_T>[] _h264Profiles = new Tuple<string, MMal.MMAL_VIDEO_PROFILE_T>[]
		{
				new Tuple<string, MMal.MMAL_VIDEO_PROFILE_T>("baseline",    MMal.MMAL_VIDEO_PROFILE_T.MMAL_VIDEO_PROFILE_H264_BASELINE),
				new Tuple<string, MMal.MMAL_VIDEO_PROFILE_T>("main",        MMal.MMAL_VIDEO_PROFILE_T.MMAL_VIDEO_PROFILE_H264_MAIN),
				new Tuple<string, MMal.MMAL_VIDEO_PROFILE_T>("extended",    MMal.MMAL_VIDEO_PROFILE_T.MMAL_VIDEO_PROFILE_H264_EXTENDED),
				new Tuple<string, MMal.MMAL_VIDEO_PROFILE_T>("high",        MMal.MMAL_VIDEO_PROFILE_T.MMAL_VIDEO_PROFILE_H264_HIGH),
				new Tuple<string, MMal.MMAL_VIDEO_PROFILE_T>("constrained", MMal.MMAL_VIDEO_PROFILE_T.MMAL_VIDEO_PROFILE_H264_CONSTRAINED_BASELINE)
		};

		private MMal.MMAL_VIDEO_PROFILE_T GetProfile(params string[] options)
		{
			var strProfile = GetConfig.GetString("profile", "main", options);

			var profile = _h264Profiles.FirstOrDefault(c => c.Item1 == strProfile);

			if (profile != null)
				return profile.Item2;

			return MMal.MMAL_VIDEO_PROFILE_T.MMAL_VIDEO_PROFILE_H264_HIGH;
		}

		static Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>[] _h264Levels = new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>[]
		{
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("1", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_1),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("1.0", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_1),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("1b",  MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_1b),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("1.1", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_11),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("1.2", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_12),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("1.3", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_13),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("2", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_2),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("2.0", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_2),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("2.1", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_21),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("2.2", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_22),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("3",   MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_3),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("3.0", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_3),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("3.1", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_31),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("3.2", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_32),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("4",   MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_4),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("4.0", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_4),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("4.1", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_41),
			new Tuple<string, MMal.MMAL_VIDEO_LEVEL_T>("4.2", MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_42),
		};

		private MMal.MMAL_VIDEO_LEVEL_T GetLevel(string[] options)
		{
			var strlevel = GetConfig.GetString("level", "4", options);

			var level = _h264Levels.FirstOrDefault(c => c.Item1 == strlevel);

			if (level != null)
				return level.Item2;

			return MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_4;
		}

		//# From https://en.wikipedia.org/wiki/H.264/MPEG-4_AVC#Levels
		static Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>[] _h264bitRates = {
			//              # level, high-profile:  bitrate
			
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_1, 80000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_1b, 160000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_11, 240000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_12, 480000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_13, 960000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_2, 2500000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_21, 5000000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_22, 5000000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_3, 12500000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_31, 17500000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_32, 25000000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_4, 25000000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_41, 62500000),
			new Tuple<MMal.MMAL_VIDEO_LEVEL_T, int>(MMal.MMAL_VIDEO_LEVEL_T.MMAL_VIDEO_LEVEL_H264_42, 62500000),
		};

		private int GetBitrate(MMal.MMAL_VIDEO_LEVEL_T level, int bitrate, string[] options)
		{
			var paramBitrate = GetConfig.GetInt32("bitrate", 17000000, options);

			var allowedBitRate = _h264bitRates.FirstOrDefault(c => c.Item1 == level);

			
			if (allowedBitRate != null)
				return  allowedBitRate.Item2 < paramBitrate ? allowedBitRate.Item2 : paramBitrate;

			return paramBitrate;
		}
	}
}
