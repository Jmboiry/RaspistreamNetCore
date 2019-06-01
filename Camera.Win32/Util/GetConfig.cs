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

namespace PiCamera.Util
{
	class GetConfig
	{
		public static int GetInt32(string name, int defaultValue, params string[] options)
		{
			bool found = false;
			var optstrValue = ParseOptions(name, out found, options);
			int optValue = defaultValue;
			if (found && optstrValue != null)
			{
				try
				{
					optValue = Convert.ToInt32(optstrValue);
				}
				catch
				{
				}

			}
			return optValue;
		}

		public static string GetString(string name, string defaultValue, params string[] options)
		{
			bool found = false;
			var optstrValue = ParseOptions(name, out found, options);
			string optValue = defaultValue;
			if (found && optstrValue != null)
				optValue = optstrValue;
			return optValue;
		}

		private static string ParseOptions(string key, out bool found, params string[] options)
		{
			// looks for key=value items
			foreach (string opt in options)
			{
				var keyvalue = opt.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

				if (keyvalue.Length == 2)
				{
					var optName = keyvalue[0].ToLower();
					key = key.ToLower();
					if (key == optName)
					{
						var optValue = keyvalue[1].ToLower();
						found = true;
						return optValue;
					}
				}
			}
			found = false;
			return null;
		}
	}
}

