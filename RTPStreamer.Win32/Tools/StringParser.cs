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


using System;

namespace RTPStreamer.Tools
{
	public class StringParser
	{
		string _str;
		int _currentPos = 0;
		//int _endPos;
		static char[] _endOfWord = new char[] { '\n', '\r' };

		public static byte[] sNonWordMask =
		{
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //0-9 
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //10-19 
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //20-29
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //30-39 
			1, 1, 1, 1, 1, 0, 1, 1, 1, 1, //40-49 - is a word
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //50-59
			1, 1, 1, 1, 1, 0, 0, 0, 0, 0, //60-69 //stop on every character except a letter
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //70-79
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //80-89
			0, 1, 1, 1, 1, 0, 1, 0, 0, 0, //90-99 _ is a word
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //100-109
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //110-119
			0, 0, 0, 1, 1, 1, 1, 1, 1, 1, //120-129
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //130-139
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //140-149
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //150-159
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //160-169
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //170-179
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //180-189
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //190-199
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //200-209
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //210-219
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //220-229
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //230-239
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //240-249
			1, 1, 1, 1, 1, 1             //250-255
		};

		public static byte[] sWordMask =
		{
			// Inverse of the above
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0-9 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //10-19 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //20-29
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //30-39 
			0, 0, 0, 0, 0, 1, 0, 0, 0, 0, //40-49 - is a word
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //50-59
			0, 0, 0, 0, 0, 1, 1, 1, 1, 1, //60-69 //stop on every character except a letter
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //70-79
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //80-89
			1, 0, 0, 0, 0, 1, 0, 1, 1, 1, //90-99 _ is a word
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //100-109
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //110-119
			1, 1, 1, 0, 0, 0, 0, 0, 0, 0, //120-129
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //130-139
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //140-149
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //150-159
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //160-169
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //170-179
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //180-189
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //190-199
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //200-209
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //210-219
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //220-229
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //230-239
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //240-249
			0, 0, 0, 0, 0, 0             //250-255
		};

		public static byte[] sEOLMask =
		{
					0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0-9   
			1, 0, 0, 1, 0, 0, 0, 0, 0, 0, //10-19    //'\r' & '\n' are stop conditions
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //20-29
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //30-39 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //40-49
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //50-59
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //60-69  
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //70-79
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //80-89
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //90-99
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //100-109
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //110-119
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //120-129
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //130-139
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //140-149
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //150-159
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //160-169
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //170-179
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //180-189
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //190-199
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //200-209
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //210-219
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //220-229
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //230-239
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //240-249
			0, 0, 0, 0, 0, 0             //250-255
		};

		public static byte[] sWhitespaceMask =
		{
			1, 1, 1, 1, 1, 1, 1, 1, 1, 0, //0-9      // stop on '\t'
			0, 0, 0, 0, 1, 1, 1, 1, 1, 1, //10-19    // '\r', \v', '\f' & '\n'
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //20-29
			1, 1, 0, 1, 1, 1, 1, 1, 1, 1, //30-39   //  ' '
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //40-49
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //50-59
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //60-69
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //70-79
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //80-89
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //90-99
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //100-109
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //110-119
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //120-129
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //130-139
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //140-149
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //150-159
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //160-169
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //170-179
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //180-189
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //190-199
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //200-209
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //210-219
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //220-229
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //230-239
			1, 1, 1, 1, 1, 1, 1, 1, 1, 1, //240-249
			1, 1, 1, 1, 1, 1             //250-255
		};

		public static byte[] sDigitMask =
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //0-9
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //10-19 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //20-29
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //30-39
			0, 0, 0, 0, 0, 0, 0, 0, 1, 1, //40-49 //stop on every character except a number
			1, 1, 1, 1, 1, 1, 1, 1, 0, 0, //50-59
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //60-69 
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //70-79
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //80-89
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //90-99
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //100-109
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //110-119
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //120-129
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //130-139
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //140-149
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //150-159
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //160-169
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //170-179
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //180-189
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //190-199
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //200-209
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //210-219
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //220-229
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //230-239
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, //240-249
			0, 0, 0, 0, 0, 0             //250-255
		};

		public StringParser(string str)
		{
			_str = str;
			_currentPos = 0;
			//_endPos = _str.Length;
		}

		public string ConsumeToEnd()
		{
			if (ParserEndOfString())
				return "";

			return _str.Substring(_currentPos);
		}

		//Returns the next word
		public string ConsumeWord()
		{
			return  ConsumeUntil(sNonWordMask);
		}

		public void ConsumeUntilDigit()
		{
			ConsumeUntil(sDigitMask);
		}

		public string ConsumeUntil(byte[] mask)
		{
			if (ParserEndOfString())
				return "";

			int originalStartGet = _currentPos;

			while ((_currentPos < _str.Length) && (mask[_str[_currentPos]] == 0))//make sure inMask is indexed with an unsigned char
				AdvanceMark();

			return _str.Substring(originalStartGet, _currentPos - originalStartGet);
				
		}

		public string ConsumeUntil(char[] ch)
		{
			if (ParserEndOfString())
				return "";

			int originalStartGet = _currentPos;
			bool bAdvance = true;
			bool found = false;
			while (_currentPos < _str.Length  && bAdvance)
			{
				for (int i = 0; i < ch.Length; i++)//make sure inMask is indexed with an unsigned char
				{

					if (_str[_currentPos] == ch[i])
					{
						bAdvance = false;
						found = true;
						break;
					}
				}
				if (bAdvance)
				{
					if (!AdvanceMark())
						break;
				}
			}
			string str;

			str = _str.Substring(originalStartGet, _currentPos - originalStartGet);
			return str;

		}

		private bool AdvanceMark()
		{
			if (ParserEndOfString())
				return false;

			if ((_str[_currentPos] == '\n') || ((_str[_currentPos] == '\r') && (_str[_currentPos]!= '\n')))
			{
				// we are progressing beyond a line boundary (don't count \r\n twice)
				_currentPos++;
			}
			_currentPos++;
			return true;
		}

		public bool ParserEndOfString()
		{
			return _currentPos == _str.Length;
		}

		//public string ConsumeWord()
		//{

		//	int last = _str.IndexOfAny( new char[] {' ', '\t', '=', '-' }, _currentPos);
		//	if (last == -1)
		//		return "";
		//	var tmp = _str.Substring(_currentPos, last -_currentPos);
		//	_currentPos =  last + 1;
		//	return tmp;
		//}

		
		//public string ConsumeUntil(string value)
		//{
		//	int last = _str.IndexOf(value, _currentPos);
		//	if (last == -1)
		//		return "";// _rows[_currentLine][_currentStr++];
		//	else
		//	{
		//		var tmp = _str.Substring(_currentPos, last -_currentPos);
		//		_currentPos = last + 1;
		//		return tmp;
		//	}
		//}

		public bool ExpectEOL()
		{
			if (_currentPos == _str.Length - 1)
				return false;
			//This function processes all legal forms of HTTP / RTSP eols.
			//They are: \r (alone), \n (alone), \r\n
			bool retVal = false;
			if (_str[_currentPos] == '\r' || _str[_currentPos] == '\n')
			{
				retVal = true;
				_currentPos++;
				//check for a \r\n, which is the most common EOL sequence.
				if (_currentPos == _str.Length - 1)
					return retVal;
				else if (_str[_currentPos - 1] == '\r' && _str[_currentPos] == '\n')
					_currentPos++;
			}
			return retVal;
		}

		public string GetThru(char ch, out bool found)
		{
			string str = ConsumeUntil(new char[] { ch });

			found = Expect(ch);
			return str;
		}


		public bool Expect(char stopChar)
		{
			if (ParserEndOfString())
				return false;

			if (_str[_currentPos] != stopChar)
				return false;
			else
			{
				AdvanceMark();
				return true;
			}
		}

		public string ConsumeEOL()
		{
			//This function processes all legal forms of HTTP / RTSP eols.
			//They are: \r (alone), \n (alone), \r\n
			int original = _currentPos;

			if ((_currentPos < _str.Length) && (_str[_currentPos] == '\r') || (_str[_currentPos] == '\n'))
			{
				_currentPos++;
				//check for a \r\n, which is the most common EOL sequence.
				if ((_currentPos < _str.Length) && (_str[_currentPos - 1] == '\r') || (_str[_currentPos] == '\n'))
					_currentPos++;
			}

			var str = _str.Substring(original, _currentPos - original);
			return str;
		}

		public char Peek()
		{
			if (_currentPos == _str.Length)
				return '\0';
			else
				return _str[_currentPos];
		}

		public void ConsumeWhitespace()
		{
			ConsumeUntil(sWhitespaceMask);
		}

		public int ConsumeInteger()
		{
			if (ParserEndOfString())
				return 0;

			int theValue = 0;
			string tmp = "";
			char fStartGet = _str[_currentPos];


			while (_currentPos < _str.Length && (_str[_currentPos] >= '0') && (_str[_currentPos] <= '9'))
			{
				tmp += _str.Substring(_currentPos, 1);
				if (!AdvanceMark())
					break;
			}

			return Convert.ToInt32(tmp);
		}

		public override string ToString()
		{
			return _str; 
		}
	}
}
