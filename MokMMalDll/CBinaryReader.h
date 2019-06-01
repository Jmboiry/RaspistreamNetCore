#pragma once
#include "CMemoryStream.h"

class CBinaryReader
{
	CMemoryStream* _ms;
	byte _buffer[250000];
	void FillBuffer(int numBytes);
public:
	CBinaryReader(CMemoryStream* stream);
	~CBinaryReader();
	int ReadInt32();
	__int64 ReadInt64();
};

