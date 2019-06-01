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
#include "BitStream.h"


BitStream::BitStream(CMemoryStream* stream)
{
	_stream = stream;
	_nbBits = 8;
}


BitStream::~BitStream()
{
}

unsigned int BitStream::gf_bs_read_int(unsigned int nBits)
{
	unsigned int ret;

	ret = 0;
	while (nBits-- > 0)
	{
		ret <<= 1;
		ret |= gf_bs_read_bit();
	}
	return ret;
}

byte BitStream::gf_bs_read_bit()
{
	if (_nbBits == 8)
	{
		_current = BS_ReadByte();
		_nbBits = 0;
	}
	{
		int ret;
		_current <<= 1;
		_nbBits++;
		ret = (int)(_current & 0x100) >> 8;
		return (byte)ret;
	}
}

byte BitStream::BS_ReadByte()
{
	return _stream->ReadByte();
}
