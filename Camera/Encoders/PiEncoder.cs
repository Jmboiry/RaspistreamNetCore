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
using PiCamera.MMalObject;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PiCamera.Encoders
{
	//Base implementation of an MMAL encoder for use by PiCamera.
	//  The* parent* parameter specifies the :class:`PiCamera` instance that has
	//  constructed the encoder.The*camera_port* parameter provides the MMAL
	//  camera port that the encoder should enable for capture (this will be the
	//  still or video port of the camera component). The*input_port* parameter
	//  specifies the MMAL port that the encoder should connect to its input.
	//  Sometimes this will be the same as the camera port, but if other components
	//  are present in the pipeline (e.g.a splitter), it may be different.
	//  The*format* parameter specifies the format that the encoder should
	//  produce in its output.This is specified as a string and will be one of

	//  the following for image encoders:

	//   * ``'jpeg'``
	//   * ``'png'``
	//   * ``'gif'``
	//   * ``'bmp'``
	//   * ``'yuv'``
	//   * ``'rgb'``
	//   * ``'rgba'``
	//   * ``'bgr'``
	//   * ``'bgra'``


	//  And one of the following for video encoders:

	//   * ``'h264'``
	//   * ``'mjpeg'``


	//  The*resize* parameter is either ``None`` (indicating no resizing
	//  should take place), or a ``(width, height)`` tuple specifying the
	//  resolution that the output of the encoder should be resized to.
	//  Finally, the*options* parameter specifies additional keyword arguments
	//  that can be used to configure the encoder (e.g.bitrate for videos, or
	//  quality for images).

	public delegate bool OutputDelegate(MMalBuffer buffer);

	public unsafe abstract class PiEncoder
	{
		private static Logger _logger = LogManager.GetLogger("PiEncoder");

		private Camera _camera;
		
		private object _resizer; // not used currently
		private MMalPort _cameraPort;
		public MMalPort _inputPort;
		public MMalPort _outputPort;
		private Exception _exception;
		private Func<MMalBuffer, bool> _output;
		
		internal MMalEncoder Encoder { get; set; }
		public AutoResetEvent Event { get; private set; }
		public static Logger Log { get => _logger;  }

		public PiEncoder(Camera parent, MMalPort cameraPort, MMalPort inputPort)
		{
			_camera = parent;
			_resizer = null;
			Encoder = null;
			_cameraPort = cameraPort;
			_inputPort = inputPort;
			_outputPort = null;
			
			Event = new AutoResetEvent(false);
						
			Encoder = GetEncoder();
			_outputPort = Encoder.Outputs[0];

			if (Log.IsDebugEnabled)
				Log.Debug("Encoder {0}", Encoder.ToString());
			
			try
			{
				if (_camera != null && _camera.Closed)
					throw new Exception("Camera is closed");
			}
			catch (Exception ex)
			{
				Close();
				throw new Exception("Exception in PiEncoder", ex);
			}
		}

		//Returns ``True`` if the MMAL encoder exists and is enabled.
		public bool Active
		{
			get
			{
				try
				{
					return _outputPort.Enabled;
				}
				catch
				{
					//# output_port can be None; avoid a (demonstrated) race condition
					//# by catching AttributeError
					return false;
				}
			}
		}

		internal abstract MMalEncoder GetEncoder();


		public void Initialize(ImageFormat format, int resize, params string[] options)
		{
			if (Log.IsDebugEnabled)
				Log.Debug("cameraPort : {0}, inputPort {1} ", _cameraPort.ToString(), _inputPort.ToString());

			if (_camera == null && _camera.Closed)
				throw new Exception("Camera is closed");
			
			CreateEncoder(format, resize, options);
			
			if (Encoder != null)
				Encoder.Connection.Enable();

			if (Log.IsDebugEnabled)
				Log.Debug("Encoder connection done");

		}

		//This method only constructs the encoder; it does not connect it to the
		//input port.The method sets the :attr:`encoder` attribute to the
		//constructed encoder component, and the :attr:`output_port` attribute to
		//the encoder's output port (or the previously constructed resizer's
		//output port if one has been requested). Descendent classes extend this
		// method to finalize encoder configuration.

		public virtual void CreateEncoder(ImageFormat format, int resize, params string[] options)
		{
			if (Log.IsDebugEnabled)
				Log.Debug("connecting Encoder intput {0} with port {1}", Encoder.Inputs[0].ToString(), _inputPort.ToString());

			Encoder.Inputs[0].Connect(_inputPort);
		}

		//Stops the encoder, regardless of whether it's finished.
		//This method is called by the camera to terminate the execution of the
		//encoder.Typically, this is used with video to stop the recording, but
		//can potentially be called in the middle of image capture to terminate
		// the capture.
		public virtual void Stop()
		{
			//# NOTE: The active test below is necessary to prevent attempting to
			//# re-enter the parent lock in the case the encoder is being torn down
			//# by an error in the constructor
			if (Active)
			{
				_camera.StopCapture(_cameraPort);
				_outputPort.Disable();
				
				CloseOutput();
			}
		}


		//Finalizes the encoder and deallocates all structures.
		//This method is called by the camera prior to destroying the encoder(or
		//more precisely, letting it go out of scope to permit the garbage
		//collector to destroy it at some future time). The method destroys all
		//components that the various create methods constructed and resets their attributes.

		public void Close()
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("About to stop encoder {0}", ToString());
			Stop();
			
			if (Encoder != null)
				Encoder.Disconnect();
			//if self.resizer:
			//	self.resizer.disconnect()
			if (Encoder != null)
				Encoder.Close();
			Encoder = null;
			_outputPort = null;
		}


		//The encoder's main callback function.
		//When the encoder is active, this method is periodically called in a
		//background thread.The* port* parameter specifies the :class:`MMALPort`
		//providing the output (typically this is the encoder's output port, but
		//in the case of unencoded captures may simply be a camera port), while
		//the *buf* parameter is an :class:`~mmalobj.MMALBuffer` which can be
		//used to obtain the data to write, along with meta-data about the
		//current frame.
		//This method must set :attr:`event` when the encoder has finished (and
		//should set :attr:`exception` if an exception occurred during encoding).
		//Developers wishing to write a custom encoder class may find it simpler
		//to override the :meth:`_callback_write` method, rather than deal with
		// these complexities.
		protected virtual bool Callback(MMalBuffer buf)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug(buf.ToString());

			bool stop = false;
			try
			{
				stop = Write(buf);
				
			}
			catch (Exception ex)
			{
				stop = true;
				_exception = ex;
			}
			if (stop)
			{
				if (_logger.IsDebugEnabled)
					_logger.Debug("Callback done");
				Event.Set();
			}
			return stop;
		}

		//Writes output on behalf of the encoder callback function.
		//This method is called by :meth:`_callback` to handle writing to an
		//object in :attr:`outputs` identified by *key*. The* buf* parameter is
		//an :class:`~mmalobj.MMALBuffer` which can be used to obtain the data.
		//The method is expected to return a boolean to indicate whether output
		//is complete (``True``) or whether more data is expected(``False``).
		//The default implementation simply writes the contents of the buffer to
		//the output identified by* key*, and returns ``True`` if the buffer
		//flags indicate end of stream.Image encoders will typically override
		//the return value to indicate ``True`` on end of frame(as they only
		//wish to output a single image). Video encoders will typically override
		//this method to determine where key-frames and SPS headers occur.

		protected virtual bool Write(MMalBuffer buf/*, key= PiVideoFrameType.frame*/)
		{
			if (buf.Length > 0)
			{
				//with self.outputs_lock:
				if (_logger.IsTraceEnabled)
					_logger.Trace("{0} {1}", buf.Length, buf.Flags);

				try
				{
					//output = self.outputs[key][0]
#if _WIN32_
					_output(buf);
					//if (_output is Stream)
					//	((Stream)_output).Write(buf.Data, 0, buf.Length);
					//else if (_output is OutputDelegate)
					//	((OutputDelegate)_output)(buf);
					//else
					//	throw new NotImplementedException("");
#else
					_output(buf);

#endif
				}
				catch (Exception ex)
				{
					//# No output associated with the key type; discard the
					//# data
					//pass
					// else:
					//# Ignore None return value; most Python 2 streams have
					//# no return value for write()
					throw new Exception("Exception", ex);
				}
			}
			return ((buf.Flags & MMal.MMAL_BUFFER_HEADER_FLAG_EOS) != 0);
		}

		protected virtual void CloseOutput()
		{
		}


		//Starts the encoder object writing to the specified output.
		//This method is called by the camera to start the encoder capturing
		//data from the camera to the specified output.The* output* parameter
		//is either a filename, or a file-like object (for image and video
		//encoders), or an iterable of filenames or file-like objects(for
		//multi-image encoders).

		public virtual void Start(Func<MMalBuffer, bool> action)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Starting capture on {0}", action);

			_output = action;
			Event.Reset();
			
			_outputPort.Enable(Callback);

			_camera.StartCapture(_cameraPort);
		}
		

		//Waits for the encoder to finish(successfully or otherwise).
		//This method is called by the owning camera object to block execution
		//until the encoder has completed its task.If the *timeout* parameter
		//is None, the method will block indefinitely.Otherwise, the* timeout*
		//parameter specifies the(potentially fractional) number of seconds
		//to block for. If the encoder finishes successfully within the timeout,
		//the method returns ``True``. Otherwise, it returns ``False``.
		public bool Wait(int timeout = -1)
		{
			bool result = Event.WaitOne(timeout);
			
			Stop();
			if (_exception != null)
				throw _exception;
			
			return result;
		}
	}
}
	
