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
using System.Runtime.InteropServices;

namespace PiCamera.MMalObject
{
	public unsafe class MMalControlPort
	{
		protected MMal.MMAL_PORT_T* _port;
		public delegate bool UserCallback(MMalBuffer buffer);

		public static Logger _logger = LogManager.GetLogger("MMalControlPort");

		public MMalControlPort(MMal.MMAL_PORT_T* port)
		{
			_port = port;
		}

		public string Name => Marshal.PtrToStringAnsi(_port->name);
		public MMal.MMAL_PORT_TYPE_T PortType => _port->type;
		public int Index => _port->index;
		public uint Capabilities => _port->capabilities;

		public bool Enabled
		{
			get { return _port->is_enabled != 0 ? true : false; }
		}

		public MMal.MMAL_PORT_T* Pointer
		{
			get { return _port; }
		}

		public void SetParam(MMal.MMAL_PARAMETER_IDS value, int param)
		{
			MMal.MMAL_STATUS_T status = MMal.mmal_port_parameter_set_uint32(_port, (uint)value, (uint)param);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Could not set parameter {0}, value {1}: error {2}", value, param, status));
		}

		public void SetParam(MMal.MMAL_PARAMETER_IDS value, bool param)
		{
			MMal.MMAL_STATUS_T status = MMal.mmal_port_parameter_set_boolean(_port, (uint)value, param ? 1 : 0);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Could not set parameter {0}, value {1}: error {2}", value, param, status));
		}

		
		public virtual void Enable(UserCallback callback = null)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Enabling port : {0}", Name);
			MMal.MMAL_STATUS_T status = MMal.mmal_port_enable(_port, null);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Unable to enable port {0} : {1}", Name, status));
		}

		private void ControlCallback(MMal.MMAL_PORT_T* port, MMal.MMAL_BUFFER_HEADER_T* buffer)
		{
			throw new NotImplementedException();
		}

		public virtual void Disable()
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Disabling port : {0}", Name);

			if (Enabled)
			{
				MMal.MMAL_STATUS_T status = MMal.mmal_port_disable(_port);
				if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
					throw new Exception(String.Format("Unable to disable port {0} : {1}", Name, status));
			}
			if (_logger.IsDebugEnabled)
				_logger.Debug("Port disabled : {0}", Name);

		}
	}
}
