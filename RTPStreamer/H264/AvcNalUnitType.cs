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


namespace RTPStreamer.H264
{
	/*!
	 * AVC NAL unit types
	 */
	enum AvcNalUnitType
	{
		/*! Non IDR AVC slice*/
		GF_AVC_NALU_NON_IDR_SLICE = 1,
		/*! DP_A AVC slice*/
		GF_AVC_NALU_DP_A_SLICE = 2,
		/*! DP_B AVC slice*/
		GF_AVC_NALU_DP_B_SLICE = 3,
		/*! DP_C AVC slice*/
		GF_AVC_NALU_DP_C_SLICE = 4,
		/*! IDR AVC slice*/
		GF_AVC_NALU_IDR_SLICE = 5,
		/*! SEI Message*/
		GF_AVC_NALU_SEI = 6,
		/*! Sequence Parameter Set */
		GF_AVC_NALU_SEQ_PARAM = 7,
		/*! Picture Parameter Set*/
		GF_AVC_NALU_PIC_PARAM = 8,
		/*! Access Unit delimiter*/
		GF_AVC_NALU_ACCESS_UNIT = 9,
		/*! End of Sequence*/
		GF_AVC_NALU_END_OF_SEQ = 10,
		/*! End of stream*/
		GF_AVC_NALU_END_OF_STREAM = 11,
		/*! Filler data*/
		GF_AVC_NALU_FILLER_DATA = 12,
		/*! Sequence Parameter Set Extension*/
		GF_AVC_NALU_SEQ_PARAM_EXT = 13,
		/*! SVC preffix*/
		GF_AVC_NALU_SVC_PREFIX_NALU = 14,
		/*! SVC subsequence parameter set*/
		GF_AVC_NALU_SVC_SUBSEQ_PARAM = 15,
		/*! Auxiliary slice*/
		GF_AVC_NALU_SLICE_AUX = 19,
		/*! SVC slice*/
		GF_AVC_NALU_SVC_SLICE = 20,
		/*! View and dependency representation delimiter */
		GF_AVC_NALU_VDRD = 24
	}
}
