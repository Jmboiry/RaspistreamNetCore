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
#include "H264.h"
#include "CMemoryStream.h"
#include "BitStream.h"
#include <stdio.h>

H264::H264()
{
	_buffer = NULL;
	_size = 0;
}


H264::~H264()
{
}

void H264::SetBuffer(const char* filePath)
{
	FILE* fp = fopen(filePath, "rb");
	fseek(fp, 0, SEEK_END);
	_size = ftell(fp);
	_buffer = new byte[_size];


	fseek(fp, 0, SEEK_SET);
	int read = fread(_buffer, _size, 1, fp);
	if (read != 1)
		throw "Invalid read";

	fclose(fp);
	_ms.reset(new CMemoryStream(_buffer, _size));
}

byte* H264::GetNextNalu(int* size)
{
	size = 0;
	return NULL;
}

#define AVC_CACHE_SIZE	4096

int H264::gf_media_nalu_locate_start_code_bs(bool locate_trailing)
{
	int v, bpos, nb_cons_zeros = 0;
	long end, cache_start, load_size;
	long start = _ms->Position();

	byte avc_cache[AVC_CACHE_SIZE];
	if (start < 3)
		return 0;

	load_size = 0;
	bpos = 0;
	cache_start = 0;
	end = 0;
	v = 0xffffffff;
	while (end == 0)
	{
		/*refill cache*/
		if (bpos == load_size)
		{
			if (_ms->Position() == _ms->Length())
				break;
			load_size = _ms->Length() - _ms->Position();
			if (load_size > AVC_CACHE_SIZE)
				load_size = AVC_CACHE_SIZE;
			bpos = 0;
			cache_start = _ms->Position();
			_ms->Read(avc_cache, 0, (int)load_size);
		}
		v = ((v << 8) & 0xFFFFFF00) | ((unsigned int)avc_cache[bpos]);
		//v = ((v << 8) & 0xFFFFFF00);
		//uint tmp = (uint)avc_cache[bpos];
		//v = v | tmp;

		bpos++;
		if (locate_trailing)
		{
			if ((v & 0x000000FF) == 0) nb_cons_zeros++;
			else nb_cons_zeros = 0;
		}

		if (v == 0x00000001)
			end = cache_start + bpos - 4;
		else if ((v & 0x00FFFFFF) == 0x00000001)
			end = cache_start + bpos - 3;
	}
	_ms->Seek(start/*, SeekOrigin.Begin*/);
	if (end == 0)
		end = _ms->Length();// gf_bs_get_size(bs);
	if (locate_trailing)
	{
		if (nb_cons_zeros >= 3)
			return (int)(end - start - nb_cons_zeros);
	}
	return (int)(end - start);
}

unsigned int H264::gf_media_nalu_is_start_code()
{
	byte s1, s2, s3, s4;
	unsigned int is_sc = 0;
	long pos = _ms->Position();
	BitStream bs(_ms.get() );

	s1 = (byte)bs.gf_bs_read_int(8);
	s2 = (byte)bs.gf_bs_read_int(8);
	if (s1 == 0 && s2 == 0)
	{
		s3 = (byte)bs.gf_bs_read_int(8);
		if (s3 == 0x01)
			is_sc = 3;
		else if (s3 == 0)
		{
			s4 = (byte)bs.gf_bs_read_int(8);
			if (s4 == 0x01)
				is_sc = 4;
		}
	}
	_ms->Seek(pos + is_sc);
	return is_sc;
}