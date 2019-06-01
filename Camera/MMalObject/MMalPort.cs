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
using System.Diagnostics;
using System.Text;
using NLog;

namespace PiCamera.MMalObject
{
	//Represents an MMAL port with properties to configure and update the port's
	//   format.This is the base class of :class:`MMALVideoPort`,
	//   :class:`MMALAudioPort`, and :class:`MMALSubPicturePort`.

	unsafe public class MMalPort : MMalControlPort
	{
		string[] _opaqueSubformats;
		public MMalPortPool Pool { get; private set; }
		public MMalConnection Connection { get; set; }
		public bool Stopped { get; private set; }
		UserCallback _callback;
		static MMal.MMAL_PORT_BH_CB_T _wrapper;
		public static Logger _logger = LogManager.GetLogger("MMalPort");

		public MMalPort(MMal.MMAL_PORT_T* port, string[] opaqueSubformat) :
			base(port)
		{
			_opaqueSubformats = opaqueSubformat;
			Connection = null;
			Pool = null;
			Stopped = true;
		}

		#region Properties

		uint BufferSize
		{
			get { return Pointer->buffer_size; }
			set
			{
				if (value < 0)
					throw new Exception("buffer size < 0");
				Pointer->buffer_size = value;
			}
		}

		public static string FormatToString(uint format)
		{
			if (format == MMal.MMAL_ENCODING_OPAQUE)
				return "MMal.MMAL_ENCODING_OPAQUE";
			else if (format == MMal.MMAL_ENCODING_JPEG)
				return "MMAL_ENCODING_JPEG";
			else if (format == MMal.MMAL_ENCODING_H264)
				return "MMAL_ENCODING_H264";
			else if (format == MMal.MMAL_ENCODING_I420)
				return "MMAL_ENCODING_I420";
			else 
				return String.Format("Encoding {0}", format);
		}
		
		//Retrieves or sets the encoding format of the port. Setting this
		//attribute implicitly sets the encoding variant to a sensible value
		//(I420 in the case of OPAQUE).
		//After setting this attribute, call: meth:`commit` to make the changes
		//effective.

		public uint Format
		{
			get { return _port->format->encoding; }
			set
			{
				_port->format->encoding = value;
				if (value == MMal.MMAL_ENCODING_OPAQUE)
					_port->format->encoding_variant = MMal.MMAL_ENCODING_I420;
			}
		}

		// Retrieves or sets the bitrate limit for the port's format.
		public uint BitRate
		{
			get { return _port->format->bitrate; }
			set { _port->format->bitrate = value; }
		}


		//public virtual (int width, int height) Framesize { get { return (0, 0); } set {; } }

		//Retrieves or sets the framerate of the port's video frames in fps.
		//After setting this attribute, call: meth:`~MMALPort.commit` to make the
		//changes effective.

		//public (int numerator, int denominator) Framerate { get { return (1, 1); } set {; } }

		public (int width, int height) Framesize
		{
			get
			{
				int width = Pointer->format->es->video.crop.width;
				int height = Pointer->format->es->video.crop.height;
				return (width, height);
			}
			set
			{
				//value = to_resolution(value)
				Pointer->format->es->video.width = (uint)MMal.VCOS_ALIGN_UP(value.width, 32);
				Pointer->format->es->video.height = (uint)MMal.VCOS_ALIGN_UP(value.height, 16);
				Pointer->format->es->video.crop.width = MMal.VCOS_ALIGN_UP(value.width, 32); ;
				Pointer->format->es->video.crop.height = MMal.VCOS_ALIGN_UP(value.height, 16);
			}
		}

		public virtual (int numerator, int denominator) Framerate
		{

			get
			{
				return (Pointer->format->es->video.frame_rate.num, Pointer->format->es->video.frame_rate.den);
			}
			set
			{
				Pointer->format->es->video.frame_rate.num = value.numerator;
				Pointer->format->es->video.frame_rate.den = value.denominator;
			}
		}


		#endregion

		//Copies the port's :attr:`format` from the *source*
		//:class:`MMALControlPort`.
		public void CopyFrom(MMalControlPort source)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("MMalPort.CopyFrom: {0}", Name, source.Name);

			MMal.mmal_format_copy(Pointer->format, source.Pointer->format);
		}

		internal void Commit()
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Committing port : {0}", Name);

			MMal.MMAL_STATUS_T status = MMal.mmal_port_format_commit(Pointer);

			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Unable to enable port {0} : {1}", Name, status));
			
		}

		private unsafe void Callback(MMal.MMAL_PORT_T* port, MMal.MMAL_BUFFER_HEADER_T* buffer)
		{
			Debug.Assert(_callback != null);

			MMalBuffer buf = new MMalBuffer(buffer);
			if (_logger.IsTraceEnabled)
				_logger.Trace("buf flags: {0} len: {1}", buf.Flags, buf.Length);

			try
			{
				buf.Lock();
				bool bStop = _callback(buf);
				if (_logger.IsTraceEnabled)
					_logger.Trace("Callback returned eof :{0}", bStop);

				if (bStop)
					Stopped = true;
			}
			finally
			{
				buf.Release();
				buf.Unlock();
			}

			try
			{
				if (port->is_enabled != 0)
				{
					if (_logger.IsTraceEnabled)
						_logger.Trace("Sending buffer back");

					Pool.SendBuffer(block: false);
				}
			}
			catch (Exception ex)
			{
				_logger.Error("Exception {0}", ex.ToString());
			}

		}

		//Enable the port with the specified callback function(this must be
		//``None`` for connected ports, and a callable for disconnected ports).
		//The callback function must accept two parameters which will be this
		//:class:`MMALControlPort` (or descendent) and an :class:`MMALBuffer`
		//instance.The callback should return ``True`` when processing is
		//complete and no further calls are expected (e.g.at frame-end for an
		//image encoder), and ``False`` otherwise.

		public override void Enable(UserCallback callback = null)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Enabling port : {0}", Name);

			if (callback != null)
			{
				Debug.Assert(Stopped);
				Debug.Assert(Pool == null);
				Stopped = false;
				Pool = new MMalPortPool(this);
				_callback = callback;
				try
				{
					_wrapper = new MMal.MMAL_PORT_BH_CB_T(Callback);
					MMal.MMAL_STATUS_T status = MMal.mmal_port_enable(_port, _wrapper);
					if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
						throw new Exception(String.Format("Unable to enable port {0} : {1}", Name, status));

					//# If this port is an output port, send it all the buffers
					//# in the pool. If it's an input port, don't bother: the user
					//# will presumably want to feed buffers to it manually
					
					if (PortType == MMal.MMAL_PORT_TYPE_T.MMAL_PORT_TYPE_OUTPUT)
						Pool.SendAllBuffers(block: false);
				}
				catch (Exception ex)
				{
					_logger.Error(ex);
					Pool.Close();
					Pool = null;
					_callback = null;
					Stopped = true;
					throw ex;
				}
			}
			else
				base.Enable();
		}

		//Disable the port.
		public override void Disable()
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Disabling port {0}", Name);
			Stopped = true;
			base.Disable();
			if (Pool != null)
				Pool.Close();
			Pool = null;
		}

		public MMalConnection Connect(MMalPort other)
		{

			//Connect this port to the* other* :class:`MMALPort` (or
			//:class:`MMALPythonPort`). The type and configuration of the connection

			//will be automatically selected.

			//Various connection*options* can be specified as keyword arguments.
			//These will be passed onto the :class:`MMALConnection` or
			//:class:`MMALPythonConnection` constructor that is called (see those

			//classes for an explanation of the available options).
			//"""
			//# Always construct connections from the output end
			if (PortType != MMal.MMAL_PORT_TYPE_T.MMAL_PORT_TYPE_OUTPUT)
				return other.Connect(this);

			if (other.PortType != MMal.MMAL_PORT_TYPE_T.MMAL_PORT_TYPE_INPUT)
				throw new Exception("A connection can only be established between an output and an input port");

			//if isinstance(other, MMALPythonPort) :
			//    return MMALPythonConnection(self, other, **options)
			//else:
			return new MMalConnection(this, other/*, **options*/, null);
		}

		public void Disconnect()
		{
			// Destroy the connection between this port and internal MMalBaseConnection 
			// another port.
			if (_logger.IsDebugEnabled)
				_logger.Debug("Port {0}", Name);
			if (Connection != null)
				Connection.Close();
		}

		// Returns a :class:`MMALBuffer` from the associated :attr:`pool`. *block*
		//and*timeout* act as they do in the corresponding
		// :meth:`MMALPool.get_buffer`.
		public MMalBuffer GetBuffer(bool block = true, int timeout = 0)
		{
			if (!Enabled)
				throw new Exception(String.Format("cannot get buffer from disabled port {0}", Name));
			return Pool.GetBuffer(block, timeout);
		}

		//Send :class:`MMALBuffer` *buf* to the port.
		public void SendBuffer(MMalBuffer buf)
		{
			MMal.MMAL_STATUS_T status = MMal.MMAL_STATUS_T.MMAL_SUCCESS;

			try
			{
				status = MMal.mmal_port_send_buffer(Pointer, buf._buf);
				if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
					throw new Exception(String.Format("cannot send buffer to port {0}", Name));
			}
			catch (Exception ex)
			{
				
				if (status == MMal.MMAL_STATUS_T.MMAL_EINVAL && !Enabled)
				{
					_logger.Error(ex);
					throw new Exception(String.Format("cannot send buffer to disabled port {0}", ex, Name));
				}
			}
		}

		public override string ToString()
		{
			return new StringBuilder().AppendFormat("MMalPort: {0}, type {1}", Name, PortType).ToString();
		}
	}
}
