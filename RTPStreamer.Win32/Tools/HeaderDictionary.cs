
/********
This work is an RTSP/RTP server in C# Net.Core by Jean-Marie Boiry <jean-marie.boiry@live.fr> 
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

using System.Collections.Generic;

namespace RTPStreamer.Tools
{
	public class HeaderDictionary
	{
		Dictionary<string, string> _dictionary = new Dictionary<string, string>();

		public string this[string key]
		{
			get
			{
				var tmp = key?.ToUpper();
				return _dictionary.ContainsKey(tmp) ? _dictionary[tmp] : "";
			}
			set
			{
				var tmp = key?.ToUpper();
				_dictionary[tmp] = value;
			}

		}

		public void Add(string key, string value)
		{
			key = key.ToUpper();
			_dictionary.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			key = key.ToUpper();
			return _dictionary.ContainsKey(key);
		}
	}
}
