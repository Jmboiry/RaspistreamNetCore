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
using NLog;
using System;

namespace PiCamera.MMalObject
{
	public class MMalBaseConnection
	{
		public MMalPort Source { get; private set; }
		public MMalPort Target { get; private set; }
		protected static Logger _logger = LogManager.GetLogger("Connection");
		static private string[] default_format = null;
		protected uint[] _formats;
		

		public MMalBaseConnection(MMalPort source, MMalPort target, uint[] formats)
	{

			Tuple<string, string>[] compatible_opaque_formats = 
				{	new Tuple<string, string>("OPQV-single", "OPQV-single") ,
					new Tuple<string, string>("OPQV-dual", "OPQV-dual"),
					new Tuple<string, string>("OPQV-strips", "OPQV-strips"),
					new Tuple<string, string>("OPQV-dual", "OPQV-single"),
					new Tuple<string, string>("OPQV-single", "OPQV-dual"), //# recent firmwares permit this

				};

			if (source.PortType != MMal.MMAL_PORT_TYPE_T.MMAL_PORT_TYPE_OUTPUT)
				throw new Exception("source is not an output port");
			if (target.PortType != MMal.MMAL_PORT_TYPE_T.MMAL_PORT_TYPE_INPUT)
				throw new Exception("target is not an input port");
			if (source.Connection != null)
				throw new Exception("source port is already connected");
			if (target.Connection != null)
				throw new Exception("target port is already connected");
			if (formats == null)
				_formats = new uint[] { };
			Source = source;
			Target = target;

			//try:

			//	iter(formats)
			//except TypeError:

			//	formats = (formats,)

			NegotiateFormat(formats);
			source.Connection = (MMalConnection) this;
			target.Connection = (MMalConnection) this;
			// # Descendents continue with connection implementation..)
		}

		private unsafe void NegotiateFormat(uint[] formats)
		{
			if (formats != null)
			{
				//if formats:
				//         # If there are any formats left to try, perform the negotiation
				//         # with the filtered list. Again, there's some special casing to
				//         # deal with the incompatible OPAQUE sub-formats
				//         for f in formats:

				//	if f == mmal.MMAL_ENCODING_OPAQUE:
				//                 if (self._source.opaque_subformat,
				//                         self._target.opaque_subformat) in self.compatible_opaque_formats:
				//                     self._source.format = mmal.MMAL_ENCODING_OPAQUE
				//                 else:
				//                     continue
				//             else:
				//                 self._source.format = f

				//	try:
				//                 copy_format()

				//	except PiCameraMMALError as e:
				//                 if e.status != mmal.MMAL_EINVAL:
				//                     raise

				//		continue
				//             else:
				//                 max_buffers()

				//		return
				//raise PiCameraMMALError(
				//	mmal.MMAL_EINVAL, 'failed to negotiate port format')
			}
			else
			{
				//         # If no formats are available to try (either from filtering or
				//         # because none were given), assume the source port is set up
				//         # properly. Just copy the format to the target and hope the caller
				//         # knows what they're doing
				try
				{
					if (_logger.IsDebugEnabled)
						_logger.Debug("Preparing to commit source {0}", Source.ToString());

					Source.Commit();
					Target.CopyFrom(Source);

					if (_logger.IsDebugEnabled)
						_logger.Debug("Preparing to commit target {0}", Target.ToString());

					Target.Commit();
				}
				catch (Exception ex)
				{
					throw new Exception("failed to copy source format to target port", ex);
				}

				Source.Pointer->buffer_num = Target.Pointer->buffer_num =
					Math.Max(Source.Pointer->buffer_num, Target.Pointer->buffer_num);
				Source.Pointer->buffer_size = Target.Pointer->buffer_size =
					 Math.Max(Source.Pointer->buffer_size, Target.Pointer->buffer_size);
			}
		}

		public virtual void Close()
		{
			if (Source != null)
				Source.Connection = null;
			Source = null;
			if (Target != null)
				Target.Connection = null;

			Target = null;
		}
	}
}
