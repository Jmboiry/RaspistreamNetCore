
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

using NLog;
using PiCamera.MMalObject;
using RTPStreamer.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace RTPStreamer.H264
{
	public class H264Parser
	{
		static Logger _logger = LogManager.GetLogger("H264Parser");
		H264Fragmenter _fragmenter;
				
		public H264Parser(H264Fragmenter fragmenter, bool includeStartCodeInOutput) 
		{
			_fragmenter = fragmenter;
		}

		int numNalunit;
	
		public void Parse(MMalBuffer buffer, MMalBuffer next)
		{
			
			if (_logger.IsDebugEnabled)
				_logger.Debug("buffer.Len {0}, presentation time {1}", buffer.Length, buffer.Timestamp);
			if (buffer.Data.Length < 4)
				return;

			

			bool pictureEndMarker = false;
			MemoryStream ms = new MemoryStream(buffer.Data);
					   			
			if (NaluIsStartCode(ms) == 0)
			{
				Dump(true, buffer, 10);
				throw new Exception("No Start Code");
			}
			
			long nalStart = ms.Position;

			while (ms.Position < ms.Length)
			{
				int nalAndTrailingSize;
				int nalSize;
				int nalEnd;
				
				pictureEndMarker = false;
				nalAndTrailingSize = nalSize = NaluNextStartCode(ms);
				if (true)
					nalSize = NaluPayloadEnd(ms);


				byte[] nalunit = new byte[nalSize];
				ms.Read(nalunit, 0, (int)nalSize);

				

				ms.Seek(nalStart, SeekOrigin.Begin);
											
				byte nalunitType = (byte)(nalunit[0] & 0x1F);

				// Now that we have found (& copied) a NAL unit, process it if it's of special interest to us:
				if (IsVPS(nalunitType))
				{ 
					// Video parameter set
					
					
					// We haven't yet parsed a frame rate from the stream.
					// So parse this NAL unit to check whether frame rate information is present:
					// uint num_units_in_tick = 0, time_scale = 0;
					// AnalyzeVPS(ref num_units_in_tick, ref time_scale);
				}
				else if (IsSPS(nalunitType))
				{ 
					// Sequence parameter set
					// We haven't yet parsed a frame rate from the stream.
					// So parse this NAL unit to check whether frame rate information is present:
					// uint num_units_in_tick = 0, time_scale = 0;
					//AnalyzeSPS(nalunit, ref num_units_in_tick, ref time_scale);
					//if (time_scale > 0 && num_units_in_tick > 0)
					//{
					//	fFrameRate = fParsedFrameRate = time_scale / (DeltaTfiDivisor * num_units_in_tick);
					//}
					
				}
				else if (IsPPS(nalunitType))
				{ 
				}
				//else if (isSEI(nal_unit_type))
				//{ // Supplemental enhancement information (SEI)
				//	analyze_sei_data(nal_unit_type);
				//	// Later, perhaps adjust "fPresentationTime" if we saw a "pic_timing" SEI payload??? #####
				//}


				// Now, check whether this NAL unit ends an 'access unit'.
				// (RTP streamers need to know this in order to figure out whether or not to set the "M" bit.)
				bool thisNALUnitEndsAccessUnit = false;
				if (IsEOF(nalunitType))
				{
					// There is no next NAL unit, so we assume that this one ends the current 'access unit':
					thisNALUnitEndsAccessUnit = true;
				}
				else if (UsuallyBeginsAccessUnit(nalunitType))
				{
					// These NAL units usually *begin* an access unit, so assume that they don't end one here:
					thisNALUnitEndsAccessUnit = false;
				}
				else
				{
					// We need to check the *next* NAL unit to figure out whether
					// the current NAL unit ends an 'access unit':
					byte nextNalunitType = 0;
					nalEnd = (int)ms.Position;// gf_bs_get_position(bs);
					Debug.Assert(nalStart <= nalEnd);
					Debug.Assert(nalEnd <= nalStart + nalAndTrailingSize);
					if (nalEnd != nalStart + nalAndTrailingSize)
						ms.Seek(nalStart + nalAndTrailingSize, SeekOrigin.Begin);



					if (ms.Position == ms.Length)
					{
						if (next.Data.Length > 5)
							nextNalunitType = (byte)(next.Data[4] & 0x1F);
					}
					else
					{
						ms.Seek(nalStart + nalAndTrailingSize, SeekOrigin.Begin);

						/*consume next start code*/
						nalStart = NaluNextStartCode(ms);
						if (nalStart != 0)
						{
							Console.WriteLine("[avc-h264] invalid nal_size ({0})? Skipping {1} bytes to reach next start code\n", nalSize, nalStart);
							//gf_bs_skip_bytes(bs, nal_start);
						}
						nalStart = NaluIsStartCode(ms);
						if (nalStart == 0)
						{
							_logger.Error("avc-h264 error: no start code found");
							break;
						}
						nalStart = ms.Position;// gf_bs_get_position(bs);
						nalAndTrailingSize = nalSize = NaluNextStartCode(ms);
						if (true)
							nalSize = NaluPayloadEnd(ms);


						byte[] nextNalunit = new byte[nalSize];
						ms.Read(nextNalunit, 0, (int)nalSize);
						nextNalunitType = (byte)(next.Data[4] & 0x1F);

						ms.Seek(nalStart, SeekOrigin.Begin);

					}

					if (IsVCL(nextNalunitType))
					{
						// The high-order bit of the byte after the "nal_unit_header" tells us whether it's
						// the start of a new 'access unit' (and thus the current NAL unit ends an 'access unit'):
						byte byteAfter_nal_unit_header = 0x80;
						if (next.Data.Length >= 5)
							byteAfter_nal_unit_header = next.Data[5];
						thisNALUnitEndsAccessUnit = (byteAfter_nal_unit_header & 0x80) != 0;

						if (_logger.IsDebugEnabled)
							_logger.Debug("End of NalUnit {0}", thisNALUnitEndsAccessUnit);
					}
					else if (UsuallyBeginsAccessUnit(nextNalunitType))
					{
						// The next NAL unit's type is one that usually appears at the start of an 'access unit',
						// so we assume that the current NAL unit ends an 'access unit':
						thisNALUnitEndsAccessUnit = true;
					}
					else
					{
						// The next NAL unit definitely doesn't start a new 'access unit',
						// which means that the current NAL unit doesn't end one:
						thisNALUnitEndsAccessUnit = false;
					}

				}


				if (thisNALUnitEndsAccessUnit)
				{

					if (_logger.IsDebugEnabled)
						_logger.Debug(" * ****This NAL unit ends the current access unit * ****\n");
					pictureEndMarker = true;
				}
				
				
				if (_logger.IsDebugEnabled)
					_logger.Debug("EndNalAccessUnit {0}, presentation {1}", thisNALUnitEndsAccessUnit, buffer.Timestamp);

				// TODO
				//byte[] tmp = new byte[nalSize + 4];
				//tmp[3] = 1;
				//Array.Copy(nalunit, 0, tmp, 4, nalunit.Length);
				//Dump(tmp, numNalunit);
				//numNalunit++;

				_fragmenter.OnNewNalUnit(nalunit, thisNALUnitEndsAccessUnit, pictureEndMarker);

				nalEnd = (int)ms.Position;
				Debug.Assert(nalStart <= nalEnd);
				Debug.Assert(nalEnd <= nalStart + nalAndTrailingSize);
				if (nalEnd != nalStart + nalAndTrailingSize)
					ms.Seek(nalStart + nalAndTrailingSize, SeekOrigin.Begin);

				if (nalSize == 0)
					break;

				if (ms.Position == ms.Length)
					break;
				
				/*consume next start code*/
				nalStart = NaluNextStartCode(ms);
				if (nalStart != 0)
				{
					_logger.Error("[avc-h264] invalid nal_size ({0})? Skipping {1} bytes to reach next start code", nalSize, nalStart);
				}
				nalStart = NaluIsStartCode(ms);
				if (nalStart == 0)
				{
					_logger.Error("[avc-h264] error: no start code found");
					break;
				}
				nalStart = ms.Position;

			} // while

		}

		private void Dump(byte[] buff, int numNalunit)
		{
			
			string stream = String.Format(@"d:\shared\nal\nalunit_{0}.txt", numNalunit);
			if (File.Exists(stream))
				File.Delete(stream);
			using (FileStream s = new FileStream(stream, FileMode.OpenOrCreate, FileAccess.Write))
			{
				s.Seek(0, SeekOrigin.End);
				using (StreamWriter sw = new StreamWriter(s))
				{
					sw.WriteLine("iteration: {0} packet size: {1}", numNalunit, buff.Length);
					sw.Write("Offset {0,6}: ", 0);


					for (int i = 0; i < buff.Length; ++i)
					{
						if ((i % 4) == 0)
							sw.Write(" ");
						sw.Write("{0:x2}", buff[i]);
						if (((i + 1) % 20) == 0)
						{
							sw.WriteLine();
							sw.Write("Offset {0,6}: ", i);
						}
					}
				}
			}
		}

		void Dump(bool force, MMalBuffer buffer, int len)
		{
			if (force)
			{
				
				_logger.Error("buffer {0}", buffer.ToString());
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < buffer.Data.Length && i < len; ++i)
				{
					if ((i % 4) == 0)
						sb.Append(" ");
					sb.AppendFormat("{0:x2}", buffer.Data[i]);
					if (((i + 1) % 30) == 0)
						sb.Append("\r\n");
				}
				_logger.Error(sb.ToString());
			}
		}

		uint NaluIsStartCode(MemoryStream ms)
		{
			byte s1, s2, s3, s4;
			uint is_sc = 0;
			long pos = ms.Position;
			BitStream bs = new BitStream(ms, BitStreamMode.GF_BITSTREAM_READ);

			s1 = (byte)bs.ReadBits(8);
			s2 = (byte)bs.ReadBits(8);
			if (s1 == 0 && s2 == 0)
			{
				s3 = (byte)bs.ReadBits(8);
				if (s3 == 0x01)
					is_sc = 3;
				else if (s3 == 0)
				{
					s4 = (byte)bs.ReadBits(8);
					if (s4 == 0x01)
						is_sc = 4;
				}
			}
			ms.Seek(pos + is_sc, SeekOrigin.Begin);
			return is_sc;
		}

		int NaluPayloadEnd(MemoryStream ms)
		{
			return LocateStartCode(ms, true);
		}

		int NaluNextStartCode(MemoryStream ms)
		{
			return LocateStartCode(ms, false);
		}

		const int AVC_CACHE_SIZE = 4096;

		int LocateStartCode(MemoryStream ms, bool locateTrailing)
		{
			uint v, bpos, nb_cons_zeros = 0;
			long end, cache_start, load_size;
			long start = ms.Position;

			byte[] avc_cache = null;
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
					if (ms.Position == ms.Length)
						break;
					load_size = ms.Length - ms.Position;
					if (load_size > AVC_CACHE_SIZE)
						load_size = AVC_CACHE_SIZE;
					bpos = 0;
					avc_cache = new byte[AVC_CACHE_SIZE];
					cache_start = ms.Position;
					ms.Read(avc_cache, 0, (int)load_size);
				}
				v = ((v << 8) & 0xFFFFFF00) | ((uint)avc_cache[bpos]);
				//v = ((v << 8) & 0xFFFFFF00);
				//uint tmp = (uint)avc_cache[bpos];
				//v = v | tmp;

				bpos++;
				if (locateTrailing)
				{
					if ((v & 0x000000FF) == 0) nb_cons_zeros++;
					else nb_cons_zeros = 0;
				}

				if (v == 0x00000001)
					end = cache_start + bpos - 4;
				else if ((v & 0x00FFFFFF) == 0x00000001)
					end = cache_start + bpos - 3;
			}
			ms.Seek(start, SeekOrigin.Begin);
			if (end == 0)
				end = ms.Length;// gf_bs_get_size(bs);
			if (locateTrailing)
			{
				if (nb_cons_zeros >= 3)
					return (int)(end - start - nb_cons_zeros);
			}
			return (int)(end - start);
		}

		const int VPS_MAX_SIZE = 1000; // larger than the largest possible VPS (Video Parameter Set) NAL unit
		const int SPS_MAX_SIZE = 1000; // larger than the largest possible SPS (Sequence Parameter Set) NAL unit

		void AnalyzeSPS(byte[] nalUnit, ref uint numUnitsInTick, ref uint timescale)
		{
			numUnitsInTick = timescale = 0; // default values

			// Begin by making a copy of the NAL unit data, removing any 'emulation prevention' bytes:
			int spsSize = 0;
			byte[] spsDataWithoutEmulationBytes = new byte[nalUnit.Length];
			int spsDataWithoutEmulationBytesSize;
			spsDataWithoutEmulationBytesSize = spsSize = NaluRemoveEMulationBytes(nalUnit, ref spsDataWithoutEmulationBytes, nalUnit.Length);
			byte[] sps = new byte[spsDataWithoutEmulationBytesSize];
			Array.Copy(spsDataWithoutEmulationBytes, sps, spsDataWithoutEmulationBytesSize);
						
			BitStream bs = new BitStream(new MemoryStream(sps), BitStreamMode.GF_BITSTREAM_READ);

			bs.ReadBits(8); // forbidden_zero_bit; nal_ref_idc; nal_unit_type
			uint profile_idc = bs.ReadBits(8);

			uint constraint_setN_flag = bs.ReadBits(8); // also "reserved_zero_2bits" at end

			uint level_idc = bs.ReadBits(8);

			uint seq_parameter_set_id = bs.bs_get_ue();

			if (profile_idc == 100 || profile_idc == 110 || profile_idc == 122 || profile_idc == 244 || profile_idc == 44 || profile_idc == 83 || profile_idc == 86 || profile_idc == 118 || profile_idc == 128)
			{
				uint chroma_format_idc = bs.bs_get_ue();
				if (chroma_format_idc == 3)
				{
					bool separate_colour_plane_flag = bs.ReadBits(1) != 0;
				}
				bs.bs_get_ue(); // bit_depth_luma_minus8
				bs.bs_get_ue(); // bit_depth_chroma_minus8
				bs.ReadBits(1); // qpprime_y_zero_transform_bypass_flag
				bool seq_scaling_matrix_present_flag = bs.ReadBits(1) != 0;
				if (seq_scaling_matrix_present_flag)
				{
					for (int i = 0; i < ((chroma_format_idc != 3) ? 8 : 12); ++i)
					{
						bool seq_scaling_list_present_flag = bs.ReadBits(1) != 0;
						if (seq_scaling_list_present_flag)
						{
							uint sizeOfScalingList = (uint)(i < 6 ? 16 : 64);
							uint lastScale = 8;
							uint nextScale = 8;
							for (uint j = 0; j < sizeOfScalingList; ++j)
							{
								if (nextScale != 0)
								{
									int delta_scale = (int)bs.bs_get_se();

									nextScale = (uint)(lastScale + delta_scale + 256) % 256;
								}
								lastScale = (nextScale == 0) ? lastScale : nextScale;
							}
						}
					}
				}
			}
			uint log2_max_frame_num_minus4 = bs.bs_get_ue();//bv.get_expGolomb();
			uint pic_order_cnt_type = bs.bs_get_ue();
			if (pic_order_cnt_type == 0)
			{
				uint log2_max_pic_order_cnt_lsb_minus4 = bs.bs_get_ue();
			}
			else if (pic_order_cnt_type == 1)
			{
				bs.ReadBits(1); // delta_pic_order_always_zero_flag
				bs.bs_get_ue(); // offset_for_non_ref_pic
				bs.bs_get_ue(); // offset_for_top_to_bottom_field
				uint num_ref_frames_in_pic_order_cnt_cycle = bs.bs_get_ue();
				for (uint i = 0; i < num_ref_frames_in_pic_order_cnt_cycle; ++i)
				{
					bs.bs_get_se(); // offset_for_ref_frame[i]
				}
			}
			uint max_num_ref_frames = bs.bs_get_ue();
			bool gaps_in_frame_num_value_allowed_flag = bs.ReadBits(1) != 0;
			uint pic_width_in_mbs_minus1 = bs.bs_get_ue();
			uint pic_height_in_map_units_minus1 = bs.bs_get_ue();
			bool frame_mbs_only_flag = bs.ReadBits(1) != 0;
			if (!frame_mbs_only_flag)
			{
				bs.ReadBits(1); // mb_adaptive_frame_field_flag
			}
			bs.ReadBits(1); // direct_8x8_inference_flag
			bool frame_cropping_flag = bs.ReadBits(1) != 0;
			if (frame_cropping_flag)
			{
				bs.bs_get_ue(); // frame_crop_left_offset
				bs.bs_get_ue(); // frame_crop_right_offset
				bs.bs_get_ue(); // frame_crop_top_offset
				bs.bs_get_ue(); // frame_crop_bottom_offset
			}
			bool vui_parameters_present_flag = bs.ReadBits(1) != 0;
			if (vui_parameters_present_flag)
			{
				AnalyzeVui(bs, numUnitsInTick, timescale);
			}
		}

		private void AnalyzeVui(BitStream bv, uint num_units_in_tick, uint time_scale)
		{

		}

		bool IsEOF(byte nal_unit_type)
		{
			// "end of sequence" or "end of (bit)stream"
			return (nal_unit_type == 10 || nal_unit_type == 11);
		}

		bool UsuallyBeginsAccessUnit(byte nal_unit_type)
		{
			return (nal_unit_type >= 6 && nal_unit_type <= 9) || (nal_unit_type >= 14 && nal_unit_type <= 18);
		}

		int NaluRemoveEMulationBytes(byte[] src, ref byte[] dest, int nalSize)
		{
			Debug.Assert(src.Length == nalSize);

			int i = 0, emulationBytesCount = 0;
			byte numZero = 0;

			while (i < nalSize)
			{
				/*ISO 14496-10: "Within the NAL unit, any four-byte sequence that starts with 0x000003
				  other than the following sequences shall not occur at any byte-aligned position:
				  0x00000300
				  0x00000301
				  0x00000302
				  0x00000303"
				*/
				if (numZero == 2
						&& src[i] == 0x03
						&& i + 1 < nalSize /*next byte is readable*/
						&& src[i + 1] < 0x04)
				{
					/*emulation code found*/
					numZero = 0;
					emulationBytesCount++;
					i++;
				}

				dest[i - emulationBytesCount] = src[i];

				if (src[i] == 0)
					numZero++;
				else
					numZero = 0;

				i++;
			}
			return nalSize - emulationBytesCount;
		}

		

		void AnalyzeVPS(ref uint num_units_in_tick, ref uint time_scale)
		{
			num_units_in_tick = time_scale = 0; // default values

			// Begin by making a copy of the NAL unit data, removing any 'emulation prevention' bytes:
			byte[] vps = new byte[VPS_MAX_SIZE];
			

			//removeEmulationBytes(vps, vpsSize);

			//BitVector bv(vps, 0, 8 * vpsSize);
			Debug.Assert(false);
			//// Assert: fHNumber == 265 (because this function is called only when parsing H.265)
			//unsigned i;

			//bv.skipBits(28); // nal_unit_header, vps_video_parameter_set_id, vps_reserved_three_2bits, vps_max_layers_minus1
			//unsigned vps_max_sub_layers_minus1 = bv.getBits(3);
			//DEBUG_PRINT(vps_max_sub_layers_minus1);
			//bv.skipBits(17); // vps_temporal_id_nesting_flag, vps_reserved_0xffff_16bits
			//profile_tier_level(bv, vps_max_sub_layers_minus1);
			//Boolean vps_sub_layer_ordering_info_present_flag = bv.get1BitBoolean();
			//DEBUG_PRINT(vps_sub_layer_ordering_info_present_flag);
			//for (i = vps_sub_layer_ordering_info_present_flag ? 0 : vps_max_sub_layers_minus1;
			//	 i <= vps_max_sub_layers_minus1; ++i)
			//{
			//	(void)bv.get_expGolomb(); // vps_max_dec_pic_buffering_minus1[i]
			//	(void)bv.get_expGolomb(); // vps_max_num_reorder_pics[i]
			//	(void)bv.get_expGolomb(); // vps_max_latency_increase_plus1[i]
			//}
			//unsigned vps_max_layer_id = bv.getBits(6);
			//DEBUG_PRINT(vps_max_layer_id);
			//unsigned vps_num_layer_sets_minus1 = bv.get_expGolomb();
			//DEBUG_PRINT(vps_num_layer_sets_minus1);
			//for (i = 1; i <= vps_num_layer_sets_minus1; ++i)
			//{
			//	bv.skipBits(vps_max_layer_id + 1); // layer_id_included_flag[i][0..vps_max_layer_id]
			//}
			//Boolean vps_timing_info_present_flag = bv.get1BitBoolean();
			//DEBUG_PRINT(vps_timing_info_present_flag);
			//if (vps_timing_info_present_flag)
			//{
			//	DEBUG_TAB;
			//	num_units_in_tick = bv.getBits(32);
			//	DEBUG_PRINT(num_units_in_tick);
			//	time_scale = bv.getBits(32);
			//	DEBUG_PRINT(time_scale);
			//	Boolean vps_poc_proportional_to_timing_flag = bv.get1BitBoolean();
			//	DEBUG_PRINT(vps_poc_proportional_to_timing_flag);
			//	if (vps_poc_proportional_to_timing_flag)
			//	{
			//		unsigned vps_num_ticks_poc_diff_one_minus1 = bv.get_expGolomb();
			//		DEBUG_PRINT(vps_num_ticks_poc_diff_one_minus1);
			//	}
			//}
			//Boolean vps_extension_flag = bv.get1BitBoolean();
			//DEBUG_PRINT(vps_extension_flag);
		}

		bool IsVPS(byte nal_unit_type)
		{
			// VPS NAL units occur in H.265 only:
			return false/*fHNumber == 265 && nal_unit_type == 32*/;
		}

		bool IsSPS(byte nal_unit_type)
		{
			return nal_unit_type == 7;
		}

		bool IsPPS(byte nal_unit_type)
		{
			return nal_unit_type == 8;
		}

		bool IsVCL(byte nal_unit_type)
		{
			return (nal_unit_type <= 5 && nal_unit_type > 0);
		}

	}
}
