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
#include "CBinaryReader.h"


CBinaryReader::CBinaryReader(CMemoryStream* stream) :
	_ms(stream)
{
}


CBinaryReader::~CBinaryReader()
{
}

int CBinaryReader::ReadInt32()
{
	return _ms->InternalReadInt32();
}

__int64 CBinaryReader::ReadInt64()
{

	FillBuffer(8);
	unsigned lo = (unsigned)(_buffer[0] | _buffer[1] << 8 |
		_buffer[2] << 16 | _buffer[3] << 24);
	unsigned hi = (unsigned)(_buffer[4] | _buffer[5] << 8 |
		_buffer[6] << 16 | _buffer[7] << 24);
	return (__int64)((__int64)hi) << 32 | lo;
}

void CBinaryReader::FillBuffer(int numBytes)
{
	if (_buffer != NULL && (numBytes < 0 || numBytes > sizeof(_buffer)))
	{
		throw "ArgumentOutOfRangeException(nameof(numBytes), SR.ArgumentOutOfRange_BinaryReaderFillBuffer)";
	}

	int bytesRead = 0;
	int n = 0;

	if (_ms == NULL)
	{
		throw "Error.GetFileNotOpen()";
	}

	// Need to find a good threshold for calling ReadByte() repeatedly
	// vs. calling Read(byte[], int, int) for both buffered & unbuffered
	// streams.
	if (numBytes == 1)
	{
		n = _ms->ReadByte();
		if (n == -1)
		{
			throw "Error.GetEndOfFile()";
		}

		_buffer[0] = (byte)n;
		return;
	}

	do
	{
		n = _ms->Read(_buffer, bytesRead, numBytes - bytesRead);
		if (n == 0)
		{
			throw "Error.GetEndOfFile()";
		}
		bytesRead += n;
	} while (bytesRead < numBytes);
}
