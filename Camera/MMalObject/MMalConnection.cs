
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
using System.Runtime.InteropServices;

namespace PiCamera.MMalObject
{
	public unsafe class MMalConnection : MMalBaseConnection
	{
		MMal.MMAL_CONNECTION_T* _connection;
		static uint[] default_formats = new uint[]
				{   MMal.MMAL_ENCODING_OPAQUE,
					MMal.MMAL_ENCODING_I420,
					MMal.MMAL_ENCODING_RGB24,
					MMal.MMAL_ENCODING_BGR24,
					MMal.MMAL_ENCODING_RGBA,
					MMal.MMAL_ENCODING_BGRA
				};

		public string Name => Marshal.PtrToStringAnsi(_connection->name);

		public static uint[] DefaultFormats { get => default_formats; set => default_formats = value; }

		public MMalConnection(MMalPort source, MMalPort target, uint[] formats/*, delegate callback = null*/) :
			base(source, target, formats)
		{
			//_callback = callback
			if (_logger.IsDebugEnabled)
				_logger.Debug("Source {0} Target {1}", source.ToString(), target.ToString());
			int flags = MMal.MMAL_CONNECTION_FLAG_ALLOCATION_ON_INPUT;


			//if callback is None:
			flags |= MMal.MMAL_CONNECTION_FLAG_TUNNELLING;
		
			MMal.MMAL_CONNECTION_T* connection = null;
			MMal.MMAL_STATUS_T status = MMal.mmal_connection_create(&connection, source.Pointer, target.Pointer, (uint)flags);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Failed to create connection. status {0}", status));
			_connection = connection;
		}

		public override void Close()
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Connection Disconnect {0}", Name);

			if (_connection != null)
				MMal.mmal_connection_destroy(_connection);

			_connection = null;
			//self._wrapper = None

			base.Close();
		}

		//      
		//      Enable the connection.When a connection is enabled, data is
		//      continually transferred from the output port of the source to the input
		//		port of the target component.
		//      

		public void Enable()
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("MMalConnection : Enabling {0}", Name);

			MMal.MMAL_STATUS_T status = MMal.mmal_connection_enable(_connection);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Cannot enable connection {0} {1}", Name, status));

		}

		// Disables the connection.
		public void Disable()
		{
			MMal.MMAL_STATUS_T status = MMal.mmal_connection_disable(_connection);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Cannot disable connection {0} {1}", Name, status));
		}

		//      def wrapper(connection):

		//	buf = mmal.mmal_queue_get(connection[0].queue)
		//          if buf:
		//              buf = MMALBuffer(buf)
		//              try:
		//                  modified_buf = self._callback(self, buf)
		//		except:

		//			buf.release()
		//			raise
		//              else:
		//                  if modified_buf is not None:
		//                      try:

		//					self._target.send_buffer(modified_buf)
		//				except PiCameraPortDisabled:
		//                          # Target port disabled; ignore the error
		//                          pass
		//                  else:
		//                      buf.release()
		//                  return
		//          buf = mmal.mmal_queue_get(connection[0].pool[0].queue)
		//          if buf:
		//              buf = MMALBuffer(buf)
		//              try:
		//                  self._source.send_buffer(buf)
		//		except PiCameraPortDisabled:
		//                  # Source port has been disabled; ignore the error
		//                  pass

		//      if self._callback is not None:

		//	self._wrapper = mmal.MMAL_CONNECTION_CALLBACK_T(wrapper)
		//	self._connection[0].callback = self._wrapper

		//	self._source.params[mmal.MMAL_PARAMETER_ZERO_COPY] = True

		//	self._target.params[mmal.MMAL_PARAMETER_ZERO_COPY] = True

		//mmal_check(
		//	mmal.mmal_connection_enable(self._connection),
		//	prefix= "Failed to enable connection")
		//      if self._callback is not None:

		//	MMALPool(self._connection[0].pool).send_all_buffers(self._source)
	}
}
