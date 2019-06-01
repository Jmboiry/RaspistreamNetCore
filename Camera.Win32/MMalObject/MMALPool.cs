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
	//Represents an MMAL pool containing :class:`MMALBuffer` objects.All active
	//ports are associated with a pool of buffers, and a queue.Instances can be
	//treated as a sequence of :class:`MMALBuffer` objects but this is only
	//recommended for debugging purposes; otherwise, use the :meth:`get_buffer`,
	//:meth:`send_buffer`, and :meth:`send_all_buffers` methods which work with
	//the encapsulated :class:`MMALQueue`.
	public unsafe class MMalPool
	{
		protected MMal.MMAL_POOL_T* _pool;
		MMALQueue _queue;

		public MMalPool()
		{
		}

		protected void Initialize(MMal.MMAL_POOL_T* pool)
		{
			_pool = pool;
			_queue = new MMALQueue(_pool->queue);
		}

		public virtual void Close()
		{
			if (_pool != null)
				MMal.mmal_pool_destroy(_pool);
			_pool = null;
		}

		//Get the next buffer from the pool's queue. See :meth:`MMALQueue.get`
		//for the meaning of the parameters.
		public MMalBuffer GetBuffer(bool block = true, int timeout = 0)
		{
			return _queue.Get(block, timeout);
		}

		//Get a buffer from the pool's queue and send it to *port*. *block* and
		//*timeout* act as they do in :meth:`get_buffer`. If no buffer is
		//available(for the values of* block* and* timeout*,
		//:exc:`~picamera.PiCameraMMALError` is raised).
		public virtual void SendBuffer(MMalPort port, bool block, int timeout)
		{
			var buf = GetBuffer(block, timeout);

			if (buf == null)
				throw new Exception("no buffers available");
			port.SendBuffer(buf);
		}

		public virtual void SendAllBuffers(MMalPort port = null, bool block = true, int timeout = 0)
		{
			int num = (int)MMal.mmal_queue_length(_pool->queue);
			if (num == 0)
				throw new Exception("Pool queue is empty");
			for (int i = 0; i < num; i++)
				SendBuffer(port, block, timeout);
		}
	}
}
