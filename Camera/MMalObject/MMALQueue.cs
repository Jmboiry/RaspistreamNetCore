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

namespace PiCamera.MMalObject
{
	//Represents an MMAL buffer queue.Buffers can be added to the queue with the
	//:meth:`put` method, and retrieved from the queue(with optional wait
	//timeout) with the :meth:`get` method.

	unsafe class MMALQueue
	{
		bool _created = false;
		MMal.MMAL_QUEUE_T* _queue;

		public MMALQueue(MMal.MMAL_QUEUE_T* queue)
		{
			_queue = queue;
		}

		public void Create()
		{
			MMal.mmal_queue_create();
			_created = true;
		}


		public void Close()
		{
			if (_created)
				MMal.mmal_queue_destroy(_queue);
			_queue = null;
		}

		//Get the next buffer from the queue.If* block* is ``True`` (the default)
		//and* timeout* is ``None`` (the default) then the method will block
		//until a buffer is available.Otherwise* timeout* is the maximum time to
		//wait(in seconds) for a buffer to become available.If a buffer is not
		//available before the timeout expires, the method returns ``None``.
		//Likewise, if *block* is ``False`` and no buffer is immediately
		//available then ``None`` is returned.
		public MMalBuffer Get(bool block = true, int timeout = 0)
		{
			MMal.MMAL_BUFFER_HEADER_T* buf = null;

			if (block && timeout == 0)
				buf = MMal.mmal_queue_wait(_queue);

			else if (block && timeout != 0)
				buf = MMal.mmal_queue_timedwait(_queue, (uint)(timeout * 1000));
			else
				buf = MMal.mmal_queue_get(_queue);

			if (buf != null)
				return new MMalBuffer(buf);
			else
				return null;
		}
	}
}
