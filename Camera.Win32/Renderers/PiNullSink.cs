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

namespace PiCamera.Renderers
{
	//Implements an :class:`~mmalobj.MMALNullSink` which can be used in place of
	//a renderer.
	//The*parent* parameter specifies the :class:`PiCamera` instance which
	//constructed this :class:`~mmalobj.MMALNullSink`. The*source* parameter
	//specifies the :class:`~mmalobj.MMALPort` which the null-sink should connect
	//to its input.
	//The null-sink can act as a drop-in replacement for :class:`PiRenderer` in
	//most cases, but obviously doesn't implement attributes like ``alpha``,
	//``layer``, etc. as it simply dumps any incoming frames.This is also the
	//reason that this class doesn't derive from :class:`PiRenderer` like all
	//other classes in this module.
	public class PiNullSink
	{
		MMalPort _port;
		private MMalNullSink _renderer;
		private MMalConnection _connection;

		public PiNullSink(MMalPort source)
		{
			_renderer = new MMalNullSink();
			_renderer.Enable();
			_connection = _renderer.Inputs[0].Connect(source);
			_connection.Enable();
			_port = source;
		}

		public MMalConnection Connection { get => _connection; private set => _connection = value; }

		//Finalizes the null - sink and deallocates all structures.
		//This method is called by the camera prior to destroying the null - sink
		//(or more precisely, letting it go out of scope to permit the garbage
		//collector to destroy it at some future time).
		public void Close()
		{
			if (_renderer != null)
				_renderer.Close();
			_renderer = null;
		}

	}
}
