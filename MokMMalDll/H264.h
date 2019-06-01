#pragma once
typedef unsigned char byte;
#include <iostream>
#include <memory>
#include "CMemoryStream.h"
using namespace std;

class H264
{
	byte* _buffer;
	int _size;
	
public:
	H264();
	~H264();

	auto_ptr<CMemoryStream> _ms;

	void SetBuffer(const char* filePath);
	int gf_media_nalu_locate_start_code_bs(bool locate_trailing);
	unsigned int gf_media_nalu_is_start_code();
	int gf_media_nalu_next_start_code_bs()
	{
		return gf_media_nalu_locate_start_code_bs(false);
	}
	
	int gf_media_nalu_payload_end_bs()
	{
		return gf_media_nalu_locate_start_code_bs(true);
	}
	
	long Position()
	{
		return _ms->Position();
	}
	
	long Length()
	{
		return _ms->Length();
	}

	byte* GetNextNalu(int* size);
};

