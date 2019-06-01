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

	//Represents the MMAL camera component.This component has 0 input ports and
	//3 output ports.The intended use of the output ports (which in turn
	//determines the behaviour of those ports) is as follows:


	//* Port 0 is intended for preview renderers


	//* Port 1 is intended for video recording


	//* Port 2 is intended for still image capture

	//Use the ``MMAL_PARAMETER_CAMERA_CONFIG`` parameter on the control port to
	//obtain and manipulate the camera's configuration.

	public class MMalCamera : MMalBaseComponent, IDisposable
	{

		
		public override string ComponentType => MMal.MMAL_COMPONENT_DEFAULT_CAMERA;

		protected override string[] OpaqueInputSubformats => new string[] { };

		protected override string[] OpaqueOutputSubformats => 
								new string[] { "OPQV-single", "OPQV-dual", "OPQV-strips" };

		public unsafe MMalCamera(int camera_num = 0, string stereo_mode = "none", bool stereo_decimate= false,
							int sensorMode = 0, string clockMode = "reset" /*framerate= None, */ 
							/*, framerate_range = null*/) :
			base()
		{
			MMal.MMAL_PORT_T* preview_port;
			MMal.MMAL_PORT_T* video_port;
			MMal.MMAL_PORT_T* still_port;

			preview_port = _component->output[0];
			video_port = _component->output[1];
			still_port = _component->output[2];


			// Now set up the port formats
			// Set the encode format on the Preview port
			// HW limitations mean we need the preview to be the same size as the required recorded output

			MMal.MMAL_ES_FORMAT_T* format;
			format = preview_port->format;
			format->encoding = MMal.MMAL_ENCODING_OPAQUE;
			format->encoding_variant = MMal.MMAL_ENCODING_I420;

		}






		#region IDisposable Support
		private bool disposedValue = false; // Pour détecter les appels redondants
		
		protected virtual void Dispose(bool disposing)
		{
			
			if (!disposedValue)
			{
				Close();
				disposedValue = true;
			}
		}

		// TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
		 ~MMalCamera()
		{
			Dispose(false);
		}

		// Ce code est ajouté pour implémenter correctement le modèle supprimable.
		public void Dispose()
		{
			// Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.

			Dispose(true);
			
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
