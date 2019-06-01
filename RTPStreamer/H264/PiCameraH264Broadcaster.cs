/********
This work is an RTSP/RTP server in C# Net.Core by Jean-Marie Boiry <jean-marie.boiry@live.fr> 
It was written porting a subset of the  http://www.live555.com/liveMedia/ library
so the following copyright that apply to this software
**/
/**********
This library is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the
Free Software Foundation; either version 3 of the License, or (at your
option) any later version. (See <http://www.gnu.org/copyleft/lesser.html>.)

This library is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public License for
more details.

You should have received a copy of the GNU Lesser General Public License
along with this library; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301  USA
**********/
// "liveMedia"
// Copyright (c) 1996-2019 Live Networks, Inc.  All rights reserved.

using NLog;
using PiCamera;
using PiCamera.MMalObject;
using RTPStreamer.Core;
using RTPStreamer.Network;
using RTPStreamer.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RTPStreamer.H264
{
	public class PiCameraH264Broadcaster
	{
		static Logger _logger = LogManager.GetLogger("VideoBroadcaster");

		Dictionary<string, RTPStream> _observers = new Dictionary<string, RTPStream>();

		(int width, int height) _resolution;
		int _disposed;
		private object _sync = new object();
		H264Parser _parser;
		Camera _camera;
		MMalBuffer _buffer;
		MMalBuffer _next;
		
		//-------------------------------------------------------------------------------------------------
		//--- Singleton, make sure that only 1 Instance of the class exists
		//-------------------------------------------------------------------------------------------------

		public PiCameraH264Broadcaster((int width, int height) resolution)
		{
			_resolution = resolution;
			Setup();
		}
		
		// Change the camera resolution at the next camera start.
		public void PushResolution((int width, int height) resolution)
		{
			_resolution = resolution;
		}

		private void ResetListener()
		{
			lock (_observers)
			{
				foreach(var stream in _observers.Values)
					stream.ResetStream();
			}
		}

		public void Setup()
		{
			H264Fragmenter fragmenter = new H264Fragmenter(this, PacketBuffer.maxSize, MultiFramedRTPFramer.RTP_PAYLOAD_MAX_SIZE - 12/*RTP hdr size*/);
			_parser = new H264Parser(fragmenter, false);
		}

		public void Start()
		{
			Interlocked.Exchange(ref _disposed, 0);
			lock (_sync)
			{
				_buffer = _next = null;
			}

			_camera = new Camera();
			_camera.Resolution = _resolution;
			_camera.StartRecording(Callback, ImageFormat.h264);
		}

		public void Stop()
		{
			Interlocked.Exchange(ref _disposed, 1);
			lock (_sync)
			{
				_buffer = _next = null;
			}
			_camera?.StopRecording();
			_camera?.Close();
			_logger.Info("Camera disposed");
			_camera = null;
		}

		// The parser needs to know what the "net frame will be. So we buffer the current the first time.
		bool Callback(MMalBuffer frame)
		{
			bool stop = ((frame.Flags & MMal.MMAL_BUFFER_HEADER_FLAG_EOS) != 0);
			if (stop)
			{
				_logger.Info("RTP Producer exiting");
			}
			else
			{
				try
				{
					lock (_sync)
					{
						if (_disposed == 1)
							return false;
					}
						
					if (_buffer == null)
					{
						_buffer = frame;
						return true;
					}
					else
						_next = frame;

					_parser.Parse(_buffer, _next);

				}
				catch (Exception ex)
				{
					_logger.Error(ex);
				}
				_buffer = frame;

			}
			return stop;
		}

		public void OnNewFragment(bool lastFragmentCompletedNalUnit, bool pictureEndMarker, byte[] fragment, TimeVal tv)
		{
			List<RTPStream> observers;
			Task task = null;
			IEnumerable<Task> allTasks = null;

			lock (_observers)
			{
				
				observers = _observers.Values.ToList(); 
				
			}
			try
			{
				allTasks = observers.Select(c => c.OnNewFragment(lastFragmentCompletedNalUnit, pictureEndMarker, fragment, tv));
				/*IEnumerable<Task> allResults = */
				task = Task.WhenAll(allTasks);
				task.Wait();

			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}
			finally
			{
				//Parallel.ForEach(allTasks, (t) => t.Dispose());
				task.Dispose();
			}
			//foreach (var kv in _observers)
			//{
			//	kv.Value.afterGettingFrame3(fLastFragmentCompletedNALUnit, fPictureEndMarker, outputBuffer, tv, timestamp);
			//}
		}

	
		public void AddListener(string streamName, RTPStream stream)
		{
			lock(_observers)
			{
				if (_observers.ContainsKey(streamName))
					throw new ArgumentException(String.Format("Stream {0} is already playing.", streamName));
				_observers.Add(streamName, stream);
				if (_logger.IsDebugEnabled)
					_logger.Debug("Adding client {0}.", streamName);

				if (_observers.Count == 1)
				{
					if (_logger.IsDebugEnabled)
						_logger.Debug("First client connected. Starting Camera.");

					Start();
				}
			}
		}

		public void RemoveListener(string stream)
		{
			lock (_observers)
			{
				if (!_observers.ContainsKey(stream))
					throw new ArgumentException(String.Format("Stream {0} isn't playing.", stream));
				_observers.Remove(stream);
				if (_logger.IsDebugEnabled)
					_logger.Debug("Removing client {0}. Actual client left {1}", stream, _observers.Count);
				
					
			}
			if (_observers.Count == 0)
			{
				if (_logger.IsDebugEnabled)
					_logger.Debug("Last client disconnected. Stopping Camera.");
				Stop();
				if (_logger.IsDebugEnabled)
					_logger.Debug("Last client disconnected. Camera stopped.");
			}
		}
	}
}



