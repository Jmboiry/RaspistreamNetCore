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
using System.Runtime.InteropServices;

namespace PiCamera.MMalObject
{
	public abstract unsafe class MMalBaseComponent
	{
		public MMal.MMAL_COMPONENT_T* _component;
		public abstract string ComponentType { get; }

		public MMalControlPort Control { get; private set; }
		public MMalPort[] Outputs { get; private set; }
		public MMalPort[] Inputs { get; private set; }

		protected abstract string[] OpaqueInputSubformats { get; }
		protected abstract string[] OpaqueOutputSubformats { get; }

		public MMalBaseComponent()
		{
		//	mmal_check(
		//	mmal.mmal_component_create(self.component_type, self._component),
		//	prefix = "Failed to create MMAL component %s" % self.component_type)

		//if self._component[0].input_num != len(self.opaque_input_subformats):
  //          raise PiCameraRuntimeError(

		//		'Expected %d inputs but found %d on component %s' % (
		//			len(self.opaque_input_subformats),
		//			self._component[0].input_num,
		//			self.component_type))
  //      if self._component[0].output_num != len(self.opaque_output_subformats):
  //          raise PiCameraRuntimeError(

		//		'Expected %d outputs but found %d on component %s' % (
		//			len(self.opaque_output_subformats),
		//			self._component[0].output_num,
		//			self.component_type))
			MMal.MMAL_STATUS_T status;
			MMal.MMAL_COMPONENT_T* component = null;

			status = MMal.mmal_component_create(ComponentType, &component);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Unable to create component {0}, status {1}. Is it enabled ?", ComponentType, status));
			_component = component;
#if !_WIN32_
			if (_component->input_num != OpaqueInputSubformats.Length)
				throw new Exception(String.Format("Expected {0} inputs but found {1} on component {2}",
									OpaqueInputSubformats.Length,
									_component->input_num,
									ComponentType));
			if (_component->output_num != OpaqueOutputSubformats.Length)
				throw new Exception(String.Format("Expected {0} outputs but found {1} on component {2}",
									OpaqueOutputSubformats.Length,
									_component->output_num,
									ComponentType));
#endif
			Control = new MMalControlPort(_component->control);

			MMal.MMAL_PORT_T** ports = _component->output;
			Outputs = CreatePorts(ports, _component->output_num, OpaqueOutputSubformats);
			ports = _component->input;
			Inputs = CreatePorts(ports, _component->input_num, OpaqueInputSubformats);

			//port_class = {
			//	mmal.MMAL_ES_TYPE_UNKNOWN:    MMALPort,
			//         mmal.MMAL_ES_TYPE_CONTROL:    MMALControlPort,
			//         mmal.MMAL_ES_TYPE_VIDEO:      MMALVideoPort,
			//         mmal.MMAL_ES_TYPE_AUDIO:      MMALAudioPort,
			//         mmal.MMAL_ES_TYPE_SUBPICTURE: MMALSubPicturePort,
			//         }
			//self._inputs = tuple(
			//	port_class[self._component[0].input[n][0].format[0].type](
			//		self._component[0].input[n], opaque_subformat)

			//for n, opaque_subformat in enumerate(self.opaque_input_subformats))
			//	self._outputs = tuple(
			//		port_class[self._component[0].output[n][0].format[0].type](
			//			self._component[0].output[n], opaque_subformat)

			//for n, opaque_subformat in enumerate(self.opaque_output_subformats))


			//Outputs = new MMalPort[_component->output_num];
			//MMal.MMAL_PORT_T** ports = _component->output;
			//for (int i = 0; i < _component->output_num; i++)
			//	Outputs[i] = new MMalPort(ports[i]);

			//Inputs = new MMalPort[_component->input_num];
			//ports = _component->input;
			//for (int i = 0; i < _component->input_num; i++)
			//	Inputs[i] = new MMalPort(ports[i]);

		}

		private MMalPort[] CreatePorts(MMal.MMAL_PORT_T** ports, uint numPort, string[] subFormat)
		{
			MMalPort[] array = new MMalPort[numPort];
			for (int i = 0; i < numPort; i++)
			{
				switch (ports[i]->format->type)
				{
					case MMal.MMAL_ES_TYPE_T.MMAL_ES_TYPE_UNKNOWN:
						array[i] = new MMalPort(ports[i], subFormat);
						break;
					case MMal.MMAL_ES_TYPE_T.MMAL_ES_TYPE_CONTROL:
						array[i] =  new MMalPort(ports[i], new string[] { });
						break;
					case MMal.MMAL_ES_TYPE_T.MMAL_ES_TYPE_VIDEO:
						array[i] = new MMalVideoPort(ports[i], subFormat);
						break;
					case MMal.MMAL_ES_TYPE_T.MMAL_ES_TYPE_AUDIO:
						array[i] = new MMalAudioPort(ports[i], subFormat);
						break;
					// TODO case MMal.MMAL_ES_TYPE_T.MMAL_ES_TYPE_SUBPICTURE:
					default:
						throw new Exception(String.Format("Invalid port format type {0}", ports[i]->format->type));
				}
			}

			return array;
		}


		public string Name
		{
			get { return Marshal.PtrToStringAnsi(_component->name); }
		}

		//Enable the component.When a component is enabled it will process data
		//sent to its input port(s), sending the results to buffers on its output
		//port(s). Components may be implicitly enabled by connections.
		public void Enable()
		{
			MMal.MMAL_STATUS_T status = MMal.mmal_component_enable(_component);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Unable to disable component {0}, status {1}. Is it enabled ?", ComponentType, status));
		}

		// Disables the component.
		public void Disable()
		{
			MMal.MMAL_STATUS_T status = MMal.mmal_component_disable(_component);
			if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
				throw new Exception(String.Format("Unable to disable component {0}, status {1}. Is it enabled ?", ComponentType, status));
		}

		virtual public void Close()
		{

			if (_component != null)
			{
				//# ensure we free any pools associated with input/output ports
				foreach (var output in Outputs)
					output.Disable();
				foreach (var input in Inputs)
					input.Disable();

				MMal.MMAL_STATUS_T status = MMal.mmal_component_destroy(_component);
				if (status != MMal.MMAL_STATUS_T.MMAL_SUCCESS)
					throw new Exception(String.Format("Unable to destroy component {0}, status {1}. Is it enabled ?", ComponentType, status));

				_component = null;

				Inputs = null;
				Outputs = null;
				Control = null;
			}
		}
	}
}