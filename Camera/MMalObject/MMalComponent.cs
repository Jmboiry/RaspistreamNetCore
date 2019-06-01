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
using System.Runtime.InteropServices;
using System.Text;

namespace PiCamera.MMalObject
{
	unsafe abstract public class MMalComponent : MMalBaseComponent
	{
		
		public MMalConnection Connection { get { return Inputs[0].Connection;  } }

		public MMalComponent() :
			base()
		{
		}

	
		
		//   Close the component and release all its resources.After this is

		//called, most methods will raise exceptions if called.
		//      """
		public override void Close()
		{
			Disconnect();
			base.Close();
		}

		public void Disconnect()
		{

			//Destroy the connection between this component's input port and the
			//upstream component.

			if (Inputs?.Length != 0)
			{
				Inputs[0]?.Disconnect();
			}
		}

		
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("Component name {0}, output port {1}, intput port {2}\n", Marshal.PtrToStringAnsi(_component->name), _component->output_num, _component->input_num);

			foreach (var port in Outputs)
				sb.AppendFormat("Output port: {0}\n", port.ToString());
			foreach (var port in Inputs)
				sb.AppendFormat("Input port: {0}\n", port.ToString());
						
			return sb.ToString();
		}
	}
}
