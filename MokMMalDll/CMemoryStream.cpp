/********
Copyrights  Jean-Marie Boiry <jean-marie.boiry@live.fr>


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

#include "stdafx.h"
#include "CMemoryStream.h"


CMemoryStream::CMemoryStream(int capacity)
{
	_buffer = NULL;
	_position = 0;
	_capacity = capacity;
	_expandable = true;
	_writable = true;
	_exposable = true;
	_origin = 0;      // Must be 0 for byte[]'s created by MemoryStream
	_isOpen = true;
}

CMemoryStream::CMemoryStream(unsigned char* buffer, int len)
{
	
	_buffer = new byte[len]; 
	//_buffer = new byte[17338402];
	memcpy(_buffer, buffer, len);
	_position = 0;
	_length = _capacity = len;
	_writable = true;
	_exposable = false;
	_origin = 0;
}

CMemoryStream::~CMemoryStream()
{
	delete[] _buffer;
}

int CMemoryStream::InternalReadInt32()
{
	
	int pos = (_position += 4); // use temp to avoid a race condition
	if (pos > _length)
	{
		_position = _length;
		throw "Error.GetEndOfFile()";
	}
	return (int)(_buffer[pos - 4] | _buffer[pos - 3] << 8 | _buffer[pos - 2] << 16 | _buffer[pos - 1] << 24);
}

long CMemoryStream::Position()
{
	return _position - _origin;
}


long CMemoryStream::Length()
{
	return _length - _origin;
	
}

long CMemoryStream::Seek(long offset)
{
	//EnsureNotClosed();

	/*if (offset > MemStreamMaxLength)
		throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_StreamLength);*/

	/*case SeekOrigin.Begin:
	{*/
		int tempPosition = _origin + (int)offset;
		if (offset < 0 || tempPosition < _origin)
			throw  "IOException(SR.IO_SeekBeforeBegin";
		_position = tempPosition;
	//	break;
	//}
	/*case SeekOrigin.Current:
	{
		int tempPosition = unchecked(_position + (int)offset);
		if (unchecked(_position + offset) < _origin || tempPosition < _origin)
			throw new IOException(SR.IO_SeekBeforeBegin);
		_position = tempPosition;
		break;
	}*/
	/*case SeekOrigin.End:
	{
		int tempPosition = unchecked(_length + (int)offset);
		if (unchecked(_length + offset) < _origin || tempPosition < _origin)
			throw new IOException(SR.IO_SeekBeforeBegin);
		_position = tempPosition;
		break;
	}
	default:
		throw new ArgumentException(SR.Argument_InvalidSeekOrigin);
	}*/

	_ASSERT(_position >= 0, "_position >= 0");
	return _position;
}

int CMemoryStream::Read(byte* buffer, int offset, int count)
{
	if (buffer == NULL)
		throw "new ArgumentNullException(nameof(buffer), SR.ArgumentNull_Buffer)";
	if (offset < 0)
		throw "new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_NeedNonNegNum)";
	if (count < 0)
		throw "new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_NeedNonNegNum)";
	/*if (buffer.Length - offset < count)
		"throw new ArgumentException(SR.Argument_InvalidOffLen)";*/

	//EnsureNotClosed();

	int n = _length - _position;
	if (n > count)
		n = count;
	if (n <= 0)
		return 0;

	_ASSERT(_position + n >= 0, "_position + n >= 0");  // len is less than 2^31 -1.

	if (n <= 8)
	{
		int byteCount = n;
		while (--byteCount >= 0)
			buffer[offset + byteCount] = _buffer[_position + byteCount];
	}
	else
		memcpy(buffer + offset, _buffer +_position, n);
	_position += n;

	return n;
}

int CMemoryStream::ReadByte()
{
	//EnsureNotClosed();

	if (_position >= _length)
		return -1;

	return _buffer[_position++];
}

