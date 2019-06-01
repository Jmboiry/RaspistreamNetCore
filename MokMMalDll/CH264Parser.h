#pragma once
#include "CppHeader.h"
#include "CMemoryStream.h"
#include "CBinaryReader.h"

class CH264Parser
{
	auto_ptr<CMemoryStream> _stream;
	auto_ptr<CBinaryReader> _br;

public:
	CH264Parser(byte* buffer, int len);

	~CH264Parser();

	void ParseNalUnits();

};

