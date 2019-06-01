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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using NLog;
using PiCamera.Encoders;
using PiCamera.MMalObject;
using PiCamera.Renderers;
using System.Diagnostics;

namespace PiCamera
{


	public unsafe class Camera : IDisposable
	{
		const int CAMERA_PREVIEW_PORT = 0;
		const int CAMERA_VIDEO_PORT = 1;
		const int CAMERA_CAPTURE_PORT = 2;
		const int VIDEO_FRAME_RATE_NUM = 30;
		const int VIDEO_FRAME_RATE_DEN = 1;

		MMalCamera _camera;
		MMalSplitter _splitter;
		private MMalConnection _splitterConnection;

		/// Video render needs at least 2 buffers.
		const int VIDEO_OUTPUT_BUFFERS_NUM = 3;
		(int width, int height) MAX_RESOLUTION = (1921, 1081); 

		ImageFormat[] RawImageFormats = { ImageFormat.yuv, ImageFormat.rgb, ImageFormat.rgba, ImageFormat.bgr, ImageFormat.bgra };
		ImageFormat[] VideoFormats = { ImageFormat.mpeg, ImageFormat.h264 };
		PiNullSink _preview;
		private int _preview_alpha;
		private int _preview_layer;
		private bool _preview_fullscreen;

		private string _raw_format;
		private int _sharpness;
		private int _contrast;
		private int _brightness;
		private int _saturation;
		private int _iso;
		private object _videoStabilization;
		private int _exposureCompensation;
		private string _exposureMode;
		private string _meterMode;
		private string _awbMode;
		private string imageEffect;
		private object _colorEffects;
		private int _rotation;
		private bool _hflip;
		private (double, double, double, double) _zoom;
		private (int width, int height) _resolution;
		public bool Recording { get; private set; }

		private bool _vflip;
		private int _sensorMode;
		private int? _framerate;
		private string _clockMode;

		private static Logger _logger = LogManager.GetLogger("PiCamera");


		private Dictionary<int, PiEncoder> _encoders = new Dictionary<int, PiEncoder>();


		#region Private

		private void InitDefault()
		{
			_sharpness = 0;
			_contrast = 0;
			_brightness = 50;
			_saturation = 0;
			_iso = 0;  //auto
			_videoStabilization = false;
			_exposureCompensation = 0;
			_exposureMode = "auto";
			_meterMode = "average";
			_awbMode = "auto";
			imageEffect = "none";
			_colorEffects = null;
			_rotation = 0;
			_hflip = _vflip = false;
			_zoom = (0.0, 0.0, 1.0, 1.0);
			Recording = false;
		}

		/// <summary>
		/// Returns the resolution padded up to the nearest multiple of *width * and * height * which default to 32 and 16 respectively(the camera's
		/// native block size for most operations).
		/// For example:
		/// (1920, 1080) => width = 1920, height = 1088
		/// (100, 100) => width = 128, height = 112)
		/// </summary>
		/// <param name="resolution"></param>
		/// <returns></returns>
		private (int width, int height) ToResolution((int width, int height) resolution)
		{
			int padwidth = 32;
			int padheight = 16;
			
			int width = ((resolution.width + (padwidth - 1)) / padwidth) * padwidth;
			int height = ((resolution.height + (padheight - 1)) / padheight) * padheight;

			return (width, height);
		}


		/// <summary>
		/// Ensures all splitter output ports have a sensible format(I420) and buffer sizes.
		/// This method is used to ensure the splitter configuration is sane,
		/// typically after CconfigureCamera is called.
		/// </summary>
		private void ConfigureSplitter()
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("ConfigureSplitter {0}", _splitter.Name);
			Debug.Assert(_splitter != null);
			_splitter.Inputs[0].CopyFrom(_camera.Outputs[CAMERA_VIDEO_PORT]);
			_splitter.Inputs[0].Commit();
		}


		/// <summary>
		/// Create a splitter component for the video port. This is to permit video recordings and captures where use_video_port=True 
		/// to occur simultaneously)
		/// </summary>
		private void InitSplitter()
		{

			_splitter = new MMalSplitter();
			_splitterConnection = _splitter.Inputs[0].Connect(_camera.Outputs[CAMERA_VIDEO_PORT]);
			_splitterConnection.Enable();
			if (_logger.IsDebugEnabled)
				_logger.Debug("InitSplitter {0}, Connection {0}", _splitter.Name, _splitterConnection.Name);

		}

		/// <summary>
		/// Init the preview port
		/// </summary>
		private unsafe void InitPreview()
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("InitPreview");

			_preview = new PiNullSink(_camera.Outputs[CAMERA_PREVIEW_PORT]);
		}

		
		/// <summary>
		///  Configure the Raspberry Camera to the required values
		/// </summary>
		/// <param name="sensorMode"></param>
		/// <param name="framerate"></param>
		/// <param name="resolution"></param>
		/// <param name="clockMode"></param>
		unsafe private void ConfigureCamera(int sensorMode, int? framerate, (int width, int height) resolution, string clockMode)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Sensor mode {0}, Framerate {1} Resolution {2}", sensorMode, framerate, resolution);

			
			(int width, int height) previewResolution;
			MMal.MMAL_STATUS_T status;

			if (sensorMode != 0)
				_camera.Control.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_CAMERA_CUSTOM_SENSOR_CONFIG, sensorMode);

		
			if (!_camera.Control.Enabled)
			{
				// Initial setup
				_camera.Control.Enable(null);
				previewResolution = resolution;
			}
			else if (_camera.Outputs[CAMERA_PREVIEW_PORT].Framesize.width == _camera.Outputs[CAMERA_VIDEO_PORT].Framesize.width &&
						_camera.Outputs[CAMERA_PREVIEW_PORT].Framesize.height == _camera.Outputs[CAMERA_VIDEO_PORT].Framesize.height)
			{
				previewResolution = resolution;
			}
			else
				previewResolution = _camera.Outputs[CAMERA_PREVIEW_PORT].Framesize;

			

			// force preview resolution to camera's resolution
			if (previewResolution.width != resolution.width || previewResolution.height != resolution.height)
				previewResolution = resolution;
			
			_sensorMode = sensorMode;
			_framerate = framerate.HasValue ? framerate : VIDEO_FRAME_RATE_NUM;

			
			MMal.MMAL_PARAMETER_CAMERA_CONFIG_T cam_config = new MMal.MMAL_PARAMETER_CAMERA_CONFIG_T();

			cam_config.hdr.id = (uint)MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_CAMERA_CONFIG;
			cam_config.hdr.size = (uint)Marshal.SizeOf(cam_config);
			cam_config.max_stills_w = (uint)resolution.width;
			cam_config.max_stills_h = (uint)resolution.height;
			cam_config.stills_yuv422 = 0;
			cam_config.one_shot_stills = 1;
			cam_config.max_preview_video_w = (uint)resolution.width; //_preview_parameters.previewWindow.width;
			cam_config.max_preview_video_h = (uint)resolution.height;//_preview_parameters.previewWindow.height;
			cam_config.num_preview_video_frames = 3;
			cam_config.stills_capture_circular_buffer_height = 0;
			cam_config.fast_preview_resume = 0;
			cam_config.use_stc_timestamp = MMal.MMAL_PARAMETER_CAMERA_CONFIG_TIMESTAMP_MODE_T.MMAL_PARAM_TIMESTAMP_MODE_RESET_STC;



			status = MMal.mmal_port_parameter_set(_camera.Control.Pointer, &cam_config.hdr);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Could not set parameter {0}, error {1}", cam_config.hdr.id, status));
			

			foreach (var port in _camera.Outputs)
			{
				MMal.MMAL_PARAMETER_FPS_RANGE_T fps_range = new MMal.MMAL_PARAMETER_FPS_RANGE_T();
				fps_range.hdr.id = (uint)MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_FPS_RANGE;
				fps_range.hdr.size = (uint)Marshal.SizeOf(fps_range);
				fps_range.fps_low.num = 30;
				fps_range.fps_low.den = 1;
				fps_range.fps_high.num = 30;
				fps_range.fps_high.den = 1;

				status = MMal.mmal_port_parameter_set(port.Pointer, &fps_range.hdr);
				if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
					throw new Exception(String.Format("Could not set parameter {0}, error {1}", fps_range.hdr.id, status));


				if (port.Index == CAMERA_PREVIEW_PORT)
					port.Framesize = previewResolution;
				else
					port.Framesize = resolution;

				port.Framerate = (30, 1);
				port.Commit();
			}

			if (_logger.IsDebugEnabled)
				_logger.Debug("{0}", _camera.ToString());
		}

		/// <summary>
		/// Create the MMal Camera object
		/// </summary>
		/// <param name="cameraNum"></param>
		/// <param name="stereoMode"></param>
		/// <param name="stereoDecimate"></param>
		private void InitializeCamera(int cameraNum, int stereoMode, bool stereoDecimate)
		{
			try
			{
				_camera = new MMalCamera();
			}
			catch (Exception ex)
			{
				throw new Exception("Camera is not enabled. Try running 'sudo raspi-config' and ensure that the camera has been enabled.", ex);
			}

			MMal.MMAL_PARAMETER_INT32_T camera_num = new MMal.MMAL_PARAMETER_INT32_T();
			camera_num.hdr.id = (uint)MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_CAMERA_NUM;
			camera_num.hdr.size = (uint)Marshal.SizeOf(camera_num);
			camera_num.value = (uint)cameraNum;

			MMal.MMAL_STATUS_T status = MMal.mmal_port_parameter_set(_camera.Control.Pointer, &camera_num.hdr);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Could not set parameter {0}, error {1}", camera_num.hdr.id, status));
		}


		/// <summary>
		/// Raise an exception if the camera is currently recording.
		/// </summary>
		private void CheckRecordingStopped()
		{
			if (Recording)
				throw new Exception("Recording is currently running");
		}

		/// <summary>
		/// Raise an exception if the camera is already closed, or if the camera
		/// has encountered a fatal error.
		/// </summary>
		private void CheckCameraOpen()
		{
			if (Closed)
				throw new Exception("Camera is closed");
		}
			   
		/// <summary>
		/// Disable the camera for reconfiguration
		/// </summary>
		private void DisableCamera()
		{
			_splitterConnection.Disable();
			_preview.Connection.Disable();
			_camera.Disable();
		}

		/// <summary>
		/// Re enable the camera after reconfiguration
		/// </summary>
		private void EnableCamera()
		{
			_camera.Enable();
			_preview.Connection.Enable();
			_splitterConnection.Enable();
		}

		/// <summary>
		/// Determine the camera and output ports for given capture options.
		///	See camera_hardware for more information on picamera's usage of
		/// camera, splitter, and encoder ports.The general idea here is that the capture(still) port operates on its own, while the video port is
		/// always connected to a splitter component, so requests for a video port
		/// also have to specify which splitter port they want to use.
		/// </summary>
		/// <param name="fromVideoPort"></param>
		/// <param name="splitterPort"></param>
		/// <returns></returns>
		private (MMalPort cameraPort, MMalPort outputPort) GetPorts(bool fromVideoPort, int splitterPort)
		{
			CheckCameraOpen();

			PiEncoder dummy;
			if (fromVideoPort &&  _encoders.TryGetValue(splitterPort, out dummy) == true)
				throw new Exception(String.Format("The camera is already using port {0}", splitterPort));

			var cameraport = fromVideoPort ? _camera.Outputs[CAMERA_VIDEO_PORT] : _camera.Outputs[CAMERA_CAPTURE_PORT];

			var outputport = fromVideoPort ? _splitter.Outputs[splitterPort] : cameraport;

			if (_logger.IsDebugEnabled)
				_logger.Debug("GetPorts: Cameraport {0} Outputport {1}", cameraport.ToString(), outputport.ToString());

			return (cameraport, outputport);
		}


		/// <summary>
		/// Construct a video encoder for the requested parameters.
		/// This method is called by StartRecording() to construct a video encoder. The cameraPort parameter gives the MMAL camera port that should be
		/// enabled for capture by the encoder.The outputPort parameter gives the MMAL port that the encoder should read output from
		/// (this may be the same as the camera port, but may be different if other component(s) like a splitter have been placed in the pipeline). 
		/// The format parameter indicates the video format and will be one of:
		/// h264
		/// mjpeg
		/// 
		/// The resize parameter indicates the size that the encoder should resize the output to(presumably by including a resizer in the pipeline). 
		/// This feature is not implemented yet.
		/// Finally, options includes extra keyword arguments that
		/// should be passed verbatim to the encoder.
		/// </summary>
		/// <param name="cameraPort"></param>
		/// <param name="outputPort"></param>
		/// <param name="format"></param>
		/// <param name="resize"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		private PiEncoder GetVideoEncoder(MMalPort cameraPort, MMalPort outputPort,
										ImageFormat format, int resize, params string[] options)
		{
			PiEncoder encoder = null;
			if (RawImageFormats.Contains(format))
				encoder = new PiRawVideoEncoder(this, cameraPort, outputPort);
			else
				encoder = new PiCookedVideoEncoder(this, cameraPort, outputPort);

			encoder.Initialize(format, resize, options);
			return encoder;
		}

		/// <summary>
		/// Construct an image encoder for the requested parameters.
		/// This method is called by Capture() to construct an image encoder. 
		/// </summary>
		/// <param name="cameraPort"></param>
		/// <param name="outputPort"></param>
		/// <param name="format"></param>
		/// <param name="resize"></param>
		/// <param name="options"></param>
		/// <returns></returns>

		private PiEncoder GetImageEncoder(MMalPort cameraPort, MMalPort outputPort,
											 ImageFormat format, int resize, params string[] options)
		{
			PiEncoder encoder = null;

			if (RawImageFormats.Contains(format))
				throw new NotImplementedException(String.Format("Not implemented Image format {0}", format));
			else if (!VideoFormats.Contains(format))
				encoder = new PiCookedOneImageEncoder(this, cameraPort, outputPort);
			
			encoder.Initialize(format, resize, options);
			return encoder;
		}

		#endregion


		#region Internals
		
		// Only enable capture if the port is the camera's still port, or if
		// there's a single active encoder on the video splitter
		// TODO: must implement this check.

		internal void StartCapture(MMalPort port)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("start capture on {0}", port.ToString());
			port.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_CAPTURE, true);
		}

		internal void StopCapture(MMalPort port)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("stop capture on {0}", port.ToString());
			port.SetParam(MMal.MMAL_PARAMETER_IDS.MMAL_PARAMETER_CAPTURE, false);

		}

		#endregion

		#region Public

		public Camera(int camera_num = 0, int stereoMode = 0, bool stereoDecimate = false, int width = 800, int height = 600
						/*resolution= None,*/ , int? framerate = null, int sensorMode = 0, /*led_pin= None,*/ string clockMode = "reset"/*, framerate_range= None*/)
		{
			if (_logger.IsDebugEnabled)
				_logger.Debug("Camera.Camera");
			BCMHost.bcm_host_init();

			try
			{
				_resolution = ToResolution((width, height)); 
				_framerate = 30;
				InitializeCamera(camera_num, stereoMode, stereoDecimate);
				ConfigureCamera(sensorMode, _framerate, _resolution/*Resolution*/, clockMode);
				InitPreview();
				InitSplitter();

				_camera.Enable();

				InitDefault();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Exception while initializing camera. Is it enabled using raspi-config and not already used by another program ?");
				Dispose();
				throw;
			}
		}

		public bool Closed
		{
			get { return _camera == null; }
		}

		public void Close()
		{
			Dispose();
		}

		public (int width, int height) Resolution
		{
			get => _resolution;
			set
			{
				
				CheckCameraOpen();
				CheckRecordingStopped();

				_resolution = ToResolution(value);
				if (_logger.IsDebugEnabled)
					_logger.Debug("Setting camera resolution to {0}", _resolution);

				if (!((0 < value.width || value.width <= MAX_RESOLUTION.width) &&
					(0 < value.height || value.height <= MAX_RESOLUTION.height)))
					throw new Exception(String.Format("Invalid resolution requested: {0}", value));
								
				DisableCamera();

				ConfigureCamera(_sensorMode, _framerate, _resolution, _clockMode);
				ConfigureSplitter();

				EnableCamera();
				_resolution = value;
			}

		}
			   	

		public void StartRecording(Func<MMalBuffer, bool> output, ImageFormat format = ImageFormat.h264, int resize = 1, int splitterPort = 1, params string[] options)
		{
			PiEncoder encoder = null;

			lock (_encoders)
			{

				var (cameraPort, outputPort) = GetPorts(true, splitterPort);

				encoder = GetVideoEncoder(cameraPort, outputPort, format, resize, options);
			
				_encoders[splitterPort] = encoder;
			}

			if (_logger.IsDebugEnabled)
				_logger.Debug("Encoder configuration done");
			try
			{
				encoder.Start(output/*, options.get('motion_output')*/);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
				encoder.Close();
				lock (_encoders)
				{
					_encoders.Remove(splitterPort);
				}
				throw ex;
			}
		}


		/// <summary>
		/// Stop recording video from the camera.
		/// After calling this method the video encoder will be shut down and output will stop being written to the Func object specified with
		/// StartRecording. 
		/// </summary>
		/// <param name="splitterPort"></param>
		public void StopRecording(int splitterPort = 1)
		{
			PiEncoder encoder = null;

			lock (_encoders)
			{

				try
				{

					encoder = _encoders[splitterPort];
				}
				catch (Exception ex)
				{
					throw new Exception(String.Format("There is no recording in progress on port {0}", splitterPort), ex);
				}
				try
				{
					WaitRecording(0, splitterPort);
				}
				finally
				{
					encoder.Close();
					lock (_encoders)
					{
						_encoders.Remove(splitterPort);
					}
				}
			}

			if (_logger.IsDebugEnabled)
				_logger.Debug("Stop recording done");
		}
		
		/// <summary>
		/// Signal the recording thrad to stop and waits until finished.
		/// a timeout of 0 means no wait, stop immediately. 
		/// </summary>
		/// <param name="timeout">Value in seconds</param>
		/// <param name="splitterPort"></param>
		public void WaitRecording(int timeout, int splitterPort = 1)
		{
			PiEncoder encoder = null;
			
			try
			{
				lock (_encoders)
				{
					encoder = _encoders[splitterPort];
				}
			}
			catch (Exception ex)
			{
				throw new Exception(String.Format("There is no recording in progress on port {0}", splitterPort), ex);
			}
			encoder.Wait(timeout * 1000);
		}

		
		/// <summary>
		/// Capture an image from the camera, storing it by calling action.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="useVideoPort"></param>
		/// <param name="format"></param>
		/// <param name="splitterPort"></param>
		/// <param name="resize"></param>
		/// <param name="options"></param>
		public void Capture(Func<MMalBuffer, bool> action, bool useVideoPort = false, ImageFormat format = ImageFormat.jpeg, int splitterPort = 0,
								int resize = 0, params string[] options)
		{
			var (camera_port, output_port) = GetPorts(false, splitterPort);
			

			var encoder = GetImageEncoder(camera_port, output_port, format, resize, options);

			try
			{
				encoder.Start(action);
				// Wait for the callback to set the event indicating the end of
				// image capture
				
				encoder.Wait();
				
			}
			catch (Exception ex)
			{
				throw new Exception("Timed out waiting for capture to end", ex);
			}
			finally
			{
				encoder.Close();
				if (_logger.IsDebugEnabled)
					_logger.Debug("Encoder {0} closed", encoder);
				if (useVideoPort)
					_encoders.Remove(splitterPort);
			}

		}

		#endregion

		#region IDisposable Support

		private bool _disposedValue = false; // Pour détecter les appels redondants

		protected virtual void Dispose(bool disposing)
		{
			
			if (!_disposedValue)
			{
				if (disposing)
				{
					// supprimer l'état managé (objets managés).

					_splitterConnection?.Close();
					_splitter?.Close();
					_preview?.Connection.Close();
					_preview?.Close();
					_camera?.Close();
					
					if (_encoders != null)
					{
						foreach (var item in _encoders.Values)
							item.Close();
						_encoders = null;
					}
					
					_camera = null;
					_splitter = null;
					_splitterConnection = null;
					_preview = null;
					_camera = null;
					_splitter = null;
					_splitterConnection = null;
				}

				// TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
				// TODO: définir les champs de grande taille avec la valeur Null.
			

				_disposedValue = true;
			}
		}

		// TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
		~Camera()
		{
			Dispose(false);
			
		}

		// Ce code est ajouté pour implémenter correctement le modèle supprimable.
		public void Dispose()
		{
			// Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion


	}
}
