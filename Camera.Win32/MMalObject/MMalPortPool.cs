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

namespace PiCamera.MMalObject
{
	//Construct an MMAL pool for the number and size of buffers required by
	//the :class:`MMALPort` *port*.
	public unsafe class MMalPortPool : MMalPool
	{
		MMalPort _port;

		public MMalPortPool(MMalPort port)
		{
			MMal.MMAL_POOL_T* pool = MMal.mmal_port_pool_create(port.Pointer, port.Pointer->buffer_num, port.Pointer->buffer_size);
			if (pool == null)
				throw new Exception(String.Format("failed to create buffer header pool for port {0}", port.Name));
			Port = port;
			base.Initialize(pool);
		}

		public MMalPort Port { get => _port; private set => _port = value; }

		//Get a buffer from the pool and send it to *port* (or the port the pool
		//is associated with by default). *block* and *timeout* act as they do in
		//:meth:`MMALPool.get_buffer`.
		public override void SendBuffer(MMalPort port = null, bool block = true, int timeout = 0)
		{
			if (port == null)
				port = _port;
			base.SendBuffer(port, block, timeout);
		}

		//Send all buffers from the pool to* port*(or the port the pool is
		//associated with by default).  *block * and * timeout * act as they do in
		//:meth:`MMALPool.get_buffer`.
		public override void SendAllBuffers(MMalPort port = null, bool block = true, int timeout = 0)
		{
			if (port == null)
				port = _port;

			base.SendAllBuffers(port, block, timeout);
		}

		public override void Close()
		{
			_port = null;
			base.Close();
		}
	}



	//def __init__(self, port):


	//	super(MMALPortPool, self).__init__(pool)

	//	self._port = port


	//def close(self):
	//    if self._pool is not None:

	//		mmal.mmal_port_pool_destroy(self._port._port, self._pool)

	//		self._port = None

	//		self._pool = None

	//	super(MMALPortPool, self).close()


	//@property

	//def port(self):
	//    return self._port


	//def send_buffer(self, port=None, block=True, timeout=None):
	//    """

	//	Get a buffer from the pool and send it to*port* (or the port the pool
	//    is associated with by default). *block* and*timeout* act as they do in
	//    :meth:`MMALPool.get_buffer`.
	//    """
	//    if port is None:

	//		port = self._port

	//	super(MMALPortPool, self).send_buffer(port, block, timeout)


	//def send_all_buffers(self, port=None, block=True, timeout=None):
	//    """

	//	Send all buffers from the pool to*port* (or the port the pool is

	//	associated with by default).  *block* and*timeout* act as they do in
	//    :meth:`MMALPool.get_buffer`.
	//    """
	//    if port is None:

	//		port = self._port

	//	super(MMALPortPool, self).send_all_buffers(port, block, timeout)

}

