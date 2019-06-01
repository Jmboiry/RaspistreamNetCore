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

namespace PiCamera.Encoders
{
	class PiOneImageEncoder : PiImageEncoder
	{
		public PiOneImageEncoder(Camera parent, MMalPort cameraPort, MMalPort inputPort, int quality = 85,
								int restart = 0, (int width, int height, int quality)? thumbnail = null) :
			base(parent, cameraPort, inputPort, quality, restart, thumbnail)
		{

		}

		public override void CreateEncoder(ImageFormat format, int resize, params string[] options)
		{
			base.CreateEncoder(format, resize, options);
		
//			_create_encoder(
//			self, format, quality = 85, thumbnail = (64, 48, 35), restart = 0):
//        """

			//		Extends the base :meth:`~PiEncoder._create_encoder` implementation to

			//		configure the image encoder for JPEG, PNG, etc.

			//		"""

			//		super(PiImageEncoder, self)._create_encoder(format)


			//		try:
			//            self.output_port.format = {
			//					'jpeg': mmal.MMAL_ENCODING_JPEG,
			//                'png':  mmal.MMAL_ENCODING_PNG,
			//                'gif':  mmal.MMAL_ENCODING_GIF,
			//                'bmp':  mmal.MMAL_ENCODING_BMP,
			//                }[format]
			//		except KeyError:

			//			raise PiCameraValueError("Unsupported format %s" % format)

			//		self.output_port.commit()

			//        if format == 'jpeg':
			//            self.output_port.params[mmal.MMAL_PARAMETER_JPEG_Q_FACTOR] = quality
			//            if restart > 0:
			//                # Don't set if zero as old firmwares don't support this param
			//                self.output_port.params[mmal.MMAL_PARAMETER_JPEG_RESTART_INTERVAL] = restart
			//            if thumbnail is None:
			//                mp = mmal.MMAL_PARAMETER_THUMBNAIL_CONFIG_T(
			//					mmal.MMAL_PARAMETER_HEADER_T(
			//						mmal.MMAL_PARAMETER_THUMBNAIL_CONFIGURATION,
			//						ct.sizeof(mmal.MMAL_PARAMETER_THUMBNAIL_CONFIG_T)
			//                        ),
			//                    0, 0, 0, 0)
			//            else:
			//                mp = mmal.MMAL_PARAMETER_THUMBNAIL_CONFIG_T(
			//					mmal.MMAL_PARAMETER_HEADER_T(
			//						mmal.MMAL_PARAMETER_THUMBNAIL_CONFIGURATION,
			//						ct.sizeof(mmal.MMAL_PARAMETER_THUMBNAIL_CONFIG_T)
			//                        ),
			//                    1, *thumbnail)
			//            self.encoder.control.params[mmal.MMAL_PARAMETER_THUMBNAIL_CONFIGURATION] = mp

			//self.encoder.enable()
		}

		//protected override bool CallbackWrite(MMalBuffer buf)
		//{
		//	return (base.CallbackWrite(buf/*, key*/) ||
		//		(buf.Flags & (MMal.MMAL_BUFFER_HEADER_FLAG_FRAME_END | MMal.MMAL_BUFFER_HEADER_FLAG_TRANSMISSION_FAILED)) != 0);
		// }	
	}
}
