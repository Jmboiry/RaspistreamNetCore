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
using PiCamera.Util;
using System;
using System.Runtime.InteropServices;

namespace PiCamera.Encoders
{

	/// Encoder for image capture.
	// method to configure the encoder for a variety of encoded image outputs
	//   (JPEG, PNG, etc.).


	/// <summary>
	/// Encoder for image capture.
	//	method to configure the encoder for a variety of encoded image outputs  (JPEG, PNG, etc.).
	/// </summary>
	abstract class PiImageEncoder : PiEncoder
	{
		(int width, int height, int quality)? _thumbnail;
		int _quality;
		int _restart;
		

		public PiImageEncoder(Camera parent, MMalPort cameraPort, MMalPort inputPort, int quality = 85, 
								int restart= 0, (int width, int height, int quality)? thumbnail = null) :
			base(parent, cameraPort, inputPort)
		{
			
			if (!thumbnail.HasValue)
				_thumbnail = (64, 48, 35);
			else
				_thumbnail = thumbnail;
			_quality = quality;
			_restart = restart;
		}

		internal override MMalEncoder GetEncoder()
		{
			return new MMalImageEncoder();
		}


		/// <summary>
		/// configure the image encoder for JPEG, PNG, etc.
		/// Only JPEG, GIF, PNG, BMP are supported.
		/// </summary>
		/// <param name="format"></param>
		/// <param name="resize"></param>
		/// <param name="options"></param>
		public unsafe override void CreateEncoder(ImageFormat format, int resize, params string[] options)
		{
			base.CreateEncoder(format, resize, options);

			uint imgFormat = MMal.MMAL_ENCODING_JPEG;
			switch (format)
			{
				case ImageFormat.jpeg:
					imgFormat = MMal.MMAL_ENCODING_JPEG;
					break;
				case ImageFormat.gif:
					imgFormat = MMal.MMAL_ENCODING_GIF;
					break;
				case ImageFormat.png:
					imgFormat = MMal.MMAL_ENCODING_PNG;
					break;
				case ImageFormat.bmp:
					imgFormat = MMal.MMAL_ENCODING_BMP;
					break;
				default:
					imgFormat = MMal.MMAL_ENCODING_JPEG;
					break;

			}

			// Specify output format
			_outputPort.Format = imgFormat;
			_outputPort.Pointer->buffer_size = _outputPort.Pointer->buffer_size_recommended;

			if (_outputPort.Pointer->buffer_size < _outputPort.Pointer->buffer_size_min)
				_outputPort.Pointer->buffer_size = _outputPort.Pointer->buffer_size_min;

			_outputPort.Pointer->buffer_num = _outputPort.Pointer->buffer_num_recommended;

			if (_outputPort.Pointer->buffer_num < _outputPort.Pointer->buffer_num_min)
				_outputPort.Pointer->buffer_num = _outputPort.Pointer->buffer_num_min;

			_outputPort.Commit();
			// Commit the port changes to the output port
			if (MMal.MMAL_ENCODING_JPEG == imgFormat)
			{
				 _quality = GetConfig.GetInt32("quality", 85, options);
				int quality = _quality;
				if (_quality <= 0 || _quality >= 100)
					_quality = 85;
				
				// Set the JPEG quality level
				_outputPort.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_JPEG_Q_FACTOR, quality);
			
				// Set the JPEG restart interval
				if (_restart != 0)
					_outputPort.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_JPEG_RESTART_INTERVAL, _restart);
			}

			//Set up any required thumbnail
			MMal.MMAL_PARAMETER_THUMBNAIL_CONFIG_T param_thumb = new MMal.MMAL_PARAMETER_THUMBNAIL_CONFIG_T();
			param_thumb.hdr.id = (uint)MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_THUMBNAIL_CONFIGURATION;
			param_thumb.hdr.size = (uint)Marshal.SizeOf(param_thumb);


			if (_thumbnail.HasValue)
			{
				if (_thumbnail.Value.width > 0 && _thumbnail.Value.height > 0)
				{
					// Have a valid thumbnail defined
					param_thumb.enable = 1;
					param_thumb.width = (uint)_thumbnail.Value.width;
					param_thumb.height = (uint)_thumbnail.Value.height;
					param_thumb.quality = (uint)_thumbnail.Value.quality;
				}
				else
				{
					param_thumb.enable = 0;
					param_thumb.width = 0;
					param_thumb.height = 0;
					param_thumb.quality = 0;
				}
			}

			MMal.MMAL_STATUS_T status = MMal.mmal_port_parameter_set(_outputPort.Pointer, &param_thumb.hdr);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Unable to set thumbnail config {0}, restart {1}", status, _thumbnail.Value));

			Encoder.Enable();
			
		}
	}
}
