#pragma once
#include "CMemoryStream.h"
class BitStream
{
	CMemoryStream* _stream;
	int _nbBits;
	unsigned int _current;
	byte gf_bs_read_bit();
	byte BS_ReadByte();
public:
	BitStream(CMemoryStream* stream);
	~BitStream();
	unsigned int gf_bs_read_int(unsigned int nBits);
};

