#pragma once
#include "CppHeader.h"

class CMemoryStream
{
	byte* _buffer;    // Either allocated internally or externally.
	int _origin;       // For user-provided arrays, start at this origin
	int _position;     // read/write head.
	int _length;       // Number of bytes within the memory stream
	int _capacity;     // length of usable portion of buffer for stream
	// Note that _capacity == _buffer.Length for non-user-provided byte[]'s

	bool _expandable;  // User-provided buffers aren't expandable.
	bool _writable;    // Can user write to this stream?
	bool _exposable;   // Whether the array can be returned to the user.
	bool _isOpen;      // Is this stream open or closed?

	
public:
	CMemoryStream(int capacity = 0);
	CMemoryStream(unsigned char* buffer, int len);
	~CMemoryStream();

	int InternalReadInt32();
	long Position();
	long Length();
	long Seek(long offset);
	int Read(byte* buffer, int offset, int count);
	int ReadByte();

};

