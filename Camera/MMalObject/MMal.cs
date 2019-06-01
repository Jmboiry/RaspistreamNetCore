/********
This work is an implementation in C# Net.Core by Jean-Marie Boiry <jean-marie.boiry@live.fr> 
of Python modules from pycamera (https://picamera.readthedocs.io/en/release-1.13/)
so the following copyright that apply to this software
**/
/**************************************
Copyright 2013-2017 Dave Jones<dave@waveform.org.uk>

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright
	  notice, this list of conditions and the following disclaimer.

	* Redistributions in binary form must reproduce the above copyright
	  notice, this list of conditions and the following disclaimer in the
	  documentation and/or other materials provided with the distribution.

	* Neither the name of the copyright holder nor the

	  names of its contributors may be used to endorse or promote products

	  derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.

******************************/
using System;
using System.Runtime.InteropServices;

namespace PiCamera.MMalObject
{
	public class MMal
	{
		public enum MMAL_STATUS_T
		{
			MMAL_SUCCESS = 0,                 /**< Success */
			MMAL_ENOMEM,                      /**< Out of memory */
			MMAL_ENOSPC,                      /**< Out of resources (other than memory) */
			MMAL_EINVAL,                      /**< Argument is invalid */
			MMAL_ENOSYS,                      /**< Function not implemented */
			MMAL_ENOENT,                      /**< No such file or directory */
			MMAL_ENXIO,                       /**< No such device or address */
			MMAL_EIO,                         /**< I/O error */
			MMAL_ESPIPE,                      /**< Illegal seek */
			MMAL_ECORRUPT,                    /**< Data is corrupt \attention FIXME: not POSIX */
			MMAL_ENOTREADY,                   /**< Component is not ready \attention FIXME: not POSIX */
			MMAL_ECONFIG,                     /**< Component is not configured \attention FIXME: not POSIX */
			MMAL_EISCONN,                     /**< Port is already connected */
			MMAL_ENOTCONN,                    /**< Port is disconnected */
			MMAL_EAGAIN,                      /**< Resource temporarily unavailable. Try again later*/
			MMAL_EFAULT,                      /**< Bad address */
			/* Do not add new codes here unless they match something from POSIX */
			MMAL_STATUS_MAX = 0x7FFFFFFF      /**< Force to 32 bit */
		}

		/** List of port types */
		public enum MMAL_PORT_TYPE_T
		{
			MMAL_PORT_TYPE_UNKNOWN = 0,          /**< Unknown port type */
			MMAL_PORT_TYPE_CONTROL,              /**< Control port */
			MMAL_PORT_TYPE_INPUT,                /**< Input port */
			MMAL_PORT_TYPE_OUTPUT,               /**< Output port */
			MMAL_PORT_TYPE_CLOCK,               /**< Clock port */
			MMAL_PORT_TYPE_INVALID = 0x7fffffff  /**< Dummy value to force 32bit enum */
		}


		public enum MMAL_ES_TYPE_T
		{
			MMAL_ES_TYPE_UNKNOWN,     /**< Unknown elementary stream type */
			MMAL_ES_TYPE_CONTROL,     /**< Elementary stream of control commands */
			MMAL_ES_TYPE_AUDIO,       /**< Audio elementary stream */
			MMAL_ES_TYPE_VIDEO,       /**< Video elementary stream */
			MMAL_ES_TYPE_SUBPICTURE   /**< Sub-picture elementary stream (e.g. subtitles, overlays) */
		}

		/** Common parameter ID group, used with many types of component. */
		const int MMAL_PARAMETER_GROUP_COMMON = (0 << 16);
		/** Camera-specific parameter ID group. */
		const int MMAL_PARAMETER_GROUP_CAMERA = (1 << 16);
		/** Video-specific parameter ID group. */
		const int MMAL_PARAMETER_GROUP_VIDEO = (2 << 16);
		/** Audio-specific parameter ID group. */
		const int MMAL_PARAMETER_GROUP_AUDIO = (3 << 16);
		/** Clock-specific parameter ID group. */
		const int MMAL_PARAMETER_GROUP_CLOCK = (4 << 16);
		/** Miracast-specific parameter ID group. */
		const int MMAL_PARAMETER_GROUP_MIRACAST = (5 << 16);
		/** Camera-specific MMAL parameter IDs.
		* @ingroup MMAL_PARAMETER_IDS
		*/
		public const int FULL_RES_PREVIEW_FRAME_RATE_NUM = 0;
		public const int FULL_RES_PREVIEW_FRAME_RATE_DEN = 1;
		// Stills format information
		// 0 implies variable
		public const int STILLS_FRAME_RATE_NUM = 0;
		public const int STILLS_FRAME_RATE_DEN = 1;

		public const int MMAL_PARAMETER_CAMERA_INFO_MAX_CAMERAS = 4;
		public const int MMAL_PARAMETER_CAMERA_INFO_MAX_FLASHES = 2;
		public const int MMAL_PARAMETER_CAMERA_INFO_MAX_STR_LEN = 16;
		public const int MMAL_CAMERA_ANNOTATE_MAX_TEXT_LEN_V2 = 256;

		public const string MMAL_COMPONENT_DEFAULT_CAMERA_INFO = "vc.camera_info";

		public enum MMAL_PARAMETER_DRC_STRENGTH_T
		{
			MMAL_PARAMETER_DRC_STRENGTH_OFF,
			MMAL_PARAMETER_DRC_STRENGTH_LOW,
			MMAL_PARAMETER_DRC_STRENGTH_MEDIUM,
			MMAL_PARAMETER_DRC_STRENGTH_HIGH,
			MMAL_PARAMETER_DRC_STRENGTH_MAX = 0x7fffffff
		}

		/** Exposure modes. */
		public enum MMAL_PARAM_EXPOSUREMODE_T
		{
			MMAL_PARAM_EXPOSUREMODE_OFF,
			MMAL_PARAM_EXPOSUREMODE_AUTO,
			MMAL_PARAM_EXPOSUREMODE_NIGHT,
			MMAL_PARAM_EXPOSUREMODE_NIGHTPREVIEW,
			MMAL_PARAM_EXPOSUREMODE_BACKLIGHT,
			MMAL_PARAM_EXPOSUREMODE_SPOTLIGHT,
			MMAL_PARAM_EXPOSUREMODE_SPORTS,
			MMAL_PARAM_EXPOSUREMODE_SNOW,
			MMAL_PARAM_EXPOSUREMODE_BEACH,
			MMAL_PARAM_EXPOSUREMODE_VERYLONG,
			MMAL_PARAM_EXPOSUREMODE_FIXEDFPS,
			MMAL_PARAM_EXPOSUREMODE_ANTISHAKE,
			MMAL_PARAM_EXPOSUREMODE_FIREWORKS,
			MMAL_PARAM_EXPOSUREMODE_MAX = 0x7fffffff
		}

		public enum MMAL_PARAM_EXPOSUREMETERINGMODE_T
		{
			MMAL_PARAM_EXPOSUREMETERINGMODE_AVERAGE,
			MMAL_PARAM_EXPOSUREMETERINGMODE_SPOT,
			MMAL_PARAM_EXPOSUREMETERINGMODE_BACKLIT,
			MMAL_PARAM_EXPOSUREMETERINGMODE_MATRIX,
			MMAL_PARAM_EXPOSUREMETERINGMODE_MAX = 0x7fffffff
		}

		/** AWB parameter modes. */
		public enum MMAL_PARAM_AWBMODE_T
		{
			MMAL_PARAM_AWBMODE_OFF,
			MMAL_PARAM_AWBMODE_AUTO,
			MMAL_PARAM_AWBMODE_SUNLIGHT,
			MMAL_PARAM_AWBMODE_CLOUDY,
			MMAL_PARAM_AWBMODE_SHADE,
			MMAL_PARAM_AWBMODE_TUNGSTEN,
			MMAL_PARAM_AWBMODE_FLUORESCENT,
			MMAL_PARAM_AWBMODE_INCANDESCENT,
			MMAL_PARAM_AWBMODE_FLASH,
			MMAL_PARAM_AWBMODE_HORIZON,
			MMAL_PARAM_AWBMODE_MAX = 0x7fffffff
		}

		/** Image effect */
		public enum MMAL_PARAM_IMAGEFX_T
		{
			MMAL_PARAM_IMAGEFX_NONE,
			MMAL_PARAM_IMAGEFX_NEGATIVE,
			MMAL_PARAM_IMAGEFX_SOLARIZE,
			MMAL_PARAM_IMAGEFX_POSTERIZE,
			MMAL_PARAM_IMAGEFX_WHITEBOARD,
			MMAL_PARAM_IMAGEFX_BLACKBOARD,
			MMAL_PARAM_IMAGEFX_SKETCH,
			MMAL_PARAM_IMAGEFX_DENOISE,
			MMAL_PARAM_IMAGEFX_EMBOSS,
			MMAL_PARAM_IMAGEFX_OILPAINT,
			MMAL_PARAM_IMAGEFX_HATCH,
			MMAL_PARAM_IMAGEFX_GPEN,
			MMAL_PARAM_IMAGEFX_PASTEL,
			MMAL_PARAM_IMAGEFX_WATERCOLOUR,
			MMAL_PARAM_IMAGEFX_FILM,
			MMAL_PARAM_IMAGEFX_BLUR,
			MMAL_PARAM_IMAGEFX_SATURATION,
			MMAL_PARAM_IMAGEFX_COLOURSWAP,
			MMAL_PARAM_IMAGEFX_WASHEDOUT,
			MMAL_PARAM_IMAGEFX_POSTERISE,
			MMAL_PARAM_IMAGEFX_COLOURPOINT,
			MMAL_PARAM_IMAGEFX_COLOURBALANCE,
			MMAL_PARAM_IMAGEFX_CARTOON,
			MMAL_PARAM_IMAGEFX_DEINTERLACE_DOUBLE,
			MMAL_PARAM_IMAGEFX_DEINTERLACE_ADV,
			MMAL_PARAM_IMAGEFX_DEINTERLACE_FAST,
			MMAL_PARAM_IMAGEFX_MAX = 0x7fffffff
		}

		public enum MMAL_PARAMETER_CAMERA_CONFIG_TIMESTAMP_MODE_T
		{
			MMAL_PARAM_TIMESTAMP_MODE_ZERO,           /**< Always timestamp frames as 0 */
			MMAL_PARAM_TIMESTAMP_MODE_RAW_STC,        /**< Use the raw STC value for the frame timestamp */
			MMAL_PARAM_TIMESTAMP_MODE_RESET_STC,      /**< Use the STC timestamp but subtract the timestamp
                                              * of the first frame sent to give a zero based timestamp.
                                              */
			MMAL_PARAM_TIMESTAMP_MODE_MAX = 0x7FFFFFFF
		}

		public enum MMAL_PARAMETER_IDS
		{
			/* 0 */
			MMAL_PARAMETER_THUMBNAIL_CONFIGURATION    /**< Takes a @ref MMAL_PARAMETER_THUMBNAIL_CONFIG_T */
				  = MMAL_PARAMETER_GROUP_CAMERA,
			MMAL_PARAMETER_CAPTURE_QUALITY,           /**< Unused? */
			MMAL_PARAMETER_ROTATION,                  /**< Takes a @ref MMAL_PARAMETER_INT32_T */
			MMAL_PARAMETER_EXIF_DISABLE,              /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_EXIF,                      /**< Takes a @ref MMAL_PARAMETER_EXIF_T */
			MMAL_PARAMETER_AWB_MODE,                  /**< Takes a @ref MMAL_PARAM_AWBMODE_T */
			MMAL_PARAMETER_IMAGE_EFFECT,              /**< Takes a @ref MMAL_PARAMETER_IMAGEFX_T */
			MMAL_PARAMETER_COLOUR_EFFECT,             /**< Takes a @ref MMAL_PARAMETER_COLOURFX_T */
			MMAL_PARAMETER_FLICKER_AVOID,             /**< Takes a @ref MMAL_PARAMETER_FLICKERAVOID_T */
			MMAL_PARAMETER_FLASH,                     /**< Takes a @ref MMAL_PARAMETER_FLASH_T */
			MMAL_PARAMETER_REDEYE,                    /**< Takes a @ref MMAL_PARAMETER_REDEYE_T */
			MMAL_PARAMETER_FOCUS,                     /**< Takes a @ref MMAL_PARAMETER_FOCUS_T */
			MMAL_PARAMETER_FOCAL_LENGTHS,             /**< Unused? */
			MMAL_PARAMETER_EXPOSURE_COMP,             /**< Takes a @ref MMAL_PARAMETER_INT32_T or MMAL_PARAMETER_RATIONAL_T */
			MMAL_PARAMETER_ZOOM,                      /**< Takes a @ref MMAL_PARAMETER_SCALEFACTOR_T */
			MMAL_PARAMETER_MIRROR,                    /**< Takes a @ref MMAL_PARAMETER_MIRROR_T */

			/* 0x10 */
			MMAL_PARAMETER_CAMERA_NUM,                /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_CAPTURE,                   /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_EXPOSURE_MODE,             /**< Takes a @ref MMAL_PARAMETER_EXPOSUREMODE_T */
			MMAL_PARAMETER_EXP_METERING_MODE,         /**< Takes a @ref MMAL_PARAMETER_EXPOSUREMETERINGMODE_T */
			MMAL_PARAMETER_FOCUS_STATUS,              /**< Takes a @ref MMAL_PARAMETER_FOCUS_STATUS_T */
			MMAL_PARAMETER_CAMERA_CONFIG,             /**< Takes a @ref MMAL_PARAMETER_CAMERA_CONFIG_T */
			MMAL_PARAMETER_CAPTURE_STATUS,            /**< Takes a @ref MMAL_PARAMETER_CAPTURE_STATUS_T */
			MMAL_PARAMETER_FACE_TRACK,                /**< Takes a @ref MMAL_PARAMETER_FACE_TRACK_T */
			MMAL_PARAMETER_DRAW_BOX_FACES_AND_FOCUS,  /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_JPEG_Q_FACTOR,             /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_FRAME_RATE,                /**< Takes a @ref MMAL_PARAMETER_FRAME_RATE_T */
			MMAL_PARAMETER_USE_STC,                   /**< Takes a @ref MMAL_PARAMETER_CAMERA_STC_MODE_T */
			MMAL_PARAMETER_CAMERA_INFO,               /**< Takes a @ref MMAL_PARAMETER_CAMERA_INFO_T */
			MMAL_PARAMETER_VIDEO_STABILISATION,       /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_FACE_TRACK_RESULTS,        /**< Takes a @ref MMAL_PARAMETER_FACE_TRACK_RESULTS_T */
			MMAL_PARAMETER_ENABLE_RAW_CAPTURE,        /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */

			/* 0x20 */
			MMAL_PARAMETER_DPF_FILE,                  /**< Takes a @ref MMAL_PARAMETER_URI_T */
			MMAL_PARAMETER_ENABLE_DPF_FILE,           /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_DPF_FAIL_IS_FATAL,         /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_CAPTURE_MODE,              /**< Takes a @ref MMAL_PARAMETER_CAPTUREMODE_T */
			MMAL_PARAMETER_FOCUS_REGIONS,             /**< Takes a @ref MMAL_PARAMETER_FOCUS_REGIONS_T */
			MMAL_PARAMETER_INPUT_CROP,                /**< Takes a @ref MMAL_PARAMETER_INPUT_CROP_T */
			MMAL_PARAMETER_SENSOR_INFORMATION,        /**< Takes a @ref MMAL_PARAMETER_SENSOR_INFORMATION_T */
			MMAL_PARAMETER_FLASH_SELECT,              /**< Takes a @ref MMAL_PARAMETER_FLASH_SELECT_T */
			MMAL_PARAMETER_FIELD_OF_VIEW,             /**< Takes a @ref MMAL_PARAMETER_FIELD_OF_VIEW_T */
			MMAL_PARAMETER_HIGH_DYNAMIC_RANGE,        /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_DYNAMIC_RANGE_COMPRESSION, /**< Takes a @ref MMAL_PARAMETER_DRC_T */
			MMAL_PARAMETER_ALGORITHM_CONTROL,         /**< Takes a @ref MMAL_PARAMETER_ALGORITHM_CONTROL_T */
			MMAL_PARAMETER_SHARPNESS,                 /**< Takes a @ref MMAL_PARAMETER_RATIONAL_T */
			MMAL_PARAMETER_CONTRAST,                  /**< Takes a @ref MMAL_PARAMETER_RATIONAL_T */
			MMAL_PARAMETER_BRIGHTNESS,                /**< Takes a @ref MMAL_PARAMETER_RATIONAL_T */
			MMAL_PARAMETER_SATURATION,                /**< Takes a @ref MMAL_PARAMETER_RATIONAL_T */

			/* 0x30 */
			MMAL_PARAMETER_ISO,                       /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_ANTISHAKE,                 /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_IMAGE_EFFECT_PARAMETERS,   /**< Takes a @ref MMAL_PARAMETER_IMAGEFX_PARAMETERS_T */
			MMAL_PARAMETER_CAMERA_BURST_CAPTURE,      /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_CAMERA_MIN_ISO,            /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_CAMERA_USE_CASE,           /**< Takes a @ref MMAL_PARAMETER_CAMERA_USE_CASE_T */
			MMAL_PARAMETER_CAPTURE_STATS_PASS,        /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_CAMERA_CUSTOM_SENSOR_CONFIG, /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_ENABLE_REGISTER_FILE,      /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_REGISTER_FAIL_IS_FATAL,    /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_CONFIGFILE_REGISTERS,      /**< Takes a @ref MMAL_PARAMETER_CONFIGFILE_T */
			MMAL_PARAMETER_CONFIGFILE_CHUNK_REGISTERS,/**< Takes a @ref MMAL_PARAMETER_CONFIGFILE_CHUNK_T */
			MMAL_PARAMETER_JPEG_ATTACH_LOG,           /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_ZERO_SHUTTER_LAG,          /**< Takes a @ref MMAL_PARAMETER_ZEROSHUTTERLAG_T */
			MMAL_PARAMETER_FPS_RANGE,                 /**< Takes a @ref MMAL_PARAMETER_FPS_RANGE_T */
			MMAL_PARAMETER_CAPTURE_EXPOSURE_COMP,     /**< Takes a @ref MMAL_PARAMETER_INT32_T */

			/* 0x40 */
			MMAL_PARAMETER_SW_SHARPEN_DISABLE,        /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_FLASH_REQUIRED,            /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_SW_SATURATION_DISABLE,     /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_SHUTTER_SPEED,             /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_CUSTOM_AWB_GAINS,          /**< Takes a @ref MMAL_PARAMETER_AWB_GAINS_T */
			MMAL_PARAMETER_CAMERA_SETTINGS,           /**< Takes a @ref MMAL_PARAMETER_CAMERA_SETTINGS_T */
			MMAL_PARAMETER_PRIVACY_INDICATOR,         /**< Takes a @ref MMAL_PARAMETER_PRIVACY_INDICATOR_T */
			MMAL_PARAMETER_VIDEO_DENOISE,             /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_STILLS_DENOISE,            /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_ANNOTATE,                  /**< Takes a @ref MMAL_PARAMETER_CAMERA_ANNOTATE_T */
			MMAL_PARAMETER_STEREOSCOPIC_MODE,         /**< Takes a @ref MMAL_PARAMETER_STEREOSCOPIC_MODE_T */
			MMAL_PARAMETER_CAMERA_INTERFACE,          /**< Takes a @ref MMAL_PARAMETER_CAMERA_INTERFACE_T */
			MMAL_PARAMETER_CAMERA_CLOCKING_MODE,      /**< Takes a @ref MMAL_PARAMETER_CAMERA_CLOCKING_MODE_T */
			MMAL_PARAMETER_CAMERA_RX_CONFIG,          /**< Takes a @ref MMAL_PARAMETER_CAMERA_RX_CONFIG_T */
			MMAL_PARAMETER_CAMERA_RX_TIMING,          /**< Takes a @ref MMAL_PARAMETER_CAMERA_RX_TIMING_T */
			MMAL_PARAMETER_DPF_CONFIG,                /**< Takes a @ref MMAL_PARAMETER_UINT32_T */

			/* 0x50 */
			MMAL_PARAMETER_JPEG_RESTART_INTERVAL,     /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_CAMERA_ISP_BLOCK_OVERRIDE, /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_LENS_SHADING_OVERRIDE,     /**< Takes a @ref MMAL_PARAMETER_LENS_SHADING_T */
			MMAL_PARAMETER_BLACK_LEVEL,               /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_RESIZE_PARAMS,             /**< Takes a @ref MMAL_PARAMETER_RESIZE_T */
			MMAL_PARAMETER_CROP,                      /**< Takes a @ref MMAL_PARAMETER_CROP_T */
			MMAL_PARAMETER_OUTPUT_SHIFT,              /**< Takes a @ref MMAL_PARAMETER_INT32_T */
			MMAL_PARAMETER_CCM_SHIFT,                 /**< Takes a @ref MMAL_PARAMETER_INT32_T */
			MMAL_PARAMETER_CUSTOM_CCM,                /**< Takes a @ref MMAL_PARAMETER_CUSTOM_CCM_T */
			MMAL_PARAMETER_ANALOG_GAIN,               /**< Takes a @ref MMAL_PARAMETER_RATIONAL_T */
			MMAL_PARAMETER_DIGITAL_GAIN,              /**< Takes a @ref MMAL_PARAMETER_RATIONAL_T */
			/** Video-specific MMAL parameter IDs.
			* @ingroup MMAL_PARAMETER_IDS
			*/
			MMAL_PARAMETER_DISPLAYREGION           /**< Takes a @ref MMAL_DISPLAYREGION_T */
				  = MMAL_PARAMETER_GROUP_VIDEO,
			MMAL_PARAMETER_SUPPORTED_PROFILES,     /**< Takes a @ref MMAL_PARAMETER_VIDEO_PROFILE_T */
			MMAL_PARAMETER_PROFILE,                /**< Takes a @ref MMAL_PARAMETER_VIDEO_PROFILE_T */
			MMAL_PARAMETER_INTRAPERIOD,            /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_RATECONTROL,            /**< Takes a @ref MMAL_PARAMETER_VIDEO_RATECONTROL_T */
			MMAL_PARAMETER_NALUNITFORMAT,          /**< Takes a @ref MMAL_PARAMETER_VIDEO_NALUNITFORMAT_T */
			MMAL_PARAMETER_MINIMISE_FRAGMENTATION, /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_MB_ROWS_PER_SLICE,      /**< Takes a @ref MMAL_PARAMETER_UINT32_T.
                                           * Setting the value to zero resets to the default (one slice per frame). */
			MMAL_PARAMETER_VIDEO_LEVEL_EXTENSION,  /**< Takes a @ref MMAL_PARAMETER_VIDEO_LEVEL_EXTENSION_T */
			MMAL_PARAMETER_VIDEO_EEDE_ENABLE,      /**< Takes a @ref MMAL_PARAMETER_VIDEO_EEDE_ENABLE_T */
			MMAL_PARAMETER_VIDEO_EEDE_LOSSRATE,    /**< Takes a @ref MMAL_PARAMETER_VIDEO_EEDE_LOSSRATE_T */
			MMAL_PARAMETER_VIDEO_REQUEST_I_FRAME,  /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T.
                                           * Request an I-frame. */
			MMAL_PARAMETER_VIDEO_INTRA_REFRESH,    /**< Takes a @ref MMAL_PARAMETER_VIDEO_INTRA_REFRESH_T */
			MMAL_PARAMETER_VIDEO_IMMUTABLE_INPUT,  /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_BIT_RATE,         /**< Takes a @ref MMAL_PARAMETER_UINT32_T.
                                           * Run-time bit rate control */
			MMAL_PARAMETER_VIDEO_FRAME_RATE,       /**< Takes a @ref MMAL_PARAMETER_FRAME_RATE_T */
			MMAL_PARAMETER_VIDEO_ENCODE_MIN_QUANT, /**< Takes a @ref MMAL_PARAMETER_UINT32_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_MAX_QUANT, /**< Takes a @ref MMAL_PARAMETER_UINT32_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_RC_MODEL,  /**< Takes a @ref MMAL_PARAMETER_VIDEO_ENCODE_RC_MODEL_T. */
			MMAL_PARAMETER_EXTRA_BUFFERS,          /**< Takes a @ref MMAL_PARAMETER_UINT32_T. */
			MMAL_PARAMETER_VIDEO_ALIGN_HORIZ,      /**< Takes a @ref MMAL_PARAMETER_UINT32_T.
                                               Changing this paramater from the default can reduce frame rate
                                               because image buffers need to be re-pitched.*/
			MMAL_PARAMETER_VIDEO_ALIGN_VERT,        /**< Takes a @ref MMAL_PARAMETER_UINT32_T.
                                               Changing this paramater from the default can reduce frame rate
                                               because image buffers need to be re-pitched.*/
			MMAL_PARAMETER_VIDEO_DROPPABLE_PFRAMES,      /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_INITIAL_QUANT,   /**< Takes a @ref MMAL_PARAMETER_UINT32_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_QP_P,            /**< Takes a @ref MMAL_PARAMETER_UINT32_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_RC_SLICE_DQUANT, /**< Takes a @ref MMAL_PARAMETER_UINT32_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_FRAME_LIMIT_BITS,    /**< Takes a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_VIDEO_ENCODE_PEAK_RATE,       /**< Takes a @ref MMAL_PARAMETER_UINT32_T. */

			/*H264 specific parameters*/
			MMAL_PARAMETER_VIDEO_ENCODE_H264_DISABLE_CABAC,      /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_H264_LOW_LATENCY,        /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_H264_AU_DELIMITERS,      /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_H264_DEBLOCK_IDC,        /**< Takes a @ref MMAL_PARAMETER_UINT32_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_H264_MB_INTRA_MODE,      /**< Takes a @ref MMAL_PARAMETER_VIDEO_ENCODER_H264_MB_INTRA_MODES_T. */

			MMAL_PARAMETER_VIDEO_ENCODE_HEADER_ON_OPEN,  /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_VIDEO_ENCODE_PRECODE_FOR_QP,  /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */

			MMAL_PARAMETER_VIDEO_DRM_INIT_INFO,          /**< Takes a @ref MMAL_PARAMETER_VIDEO_DRM_INIT_INFO_T. */
			MMAL_PARAMETER_VIDEO_TIMESTAMP_FIFO,         /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_VIDEO_DECODE_ERROR_CONCEALMENT,        /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_VIDEO_DRM_PROTECT_BUFFER,              /**< Takes a @ref MMAL_PARAMETER_VIDEO_DRM_PROTECT_BUFFER_T. */

			MMAL_PARAMETER_VIDEO_DECODE_CONFIG_VD3,       /**< Takes a @ref MMAL_PARAMETER_BYTES_T */
			MMAL_PARAMETER_VIDEO_ENCODE_H264_VCL_HRD_PARAMETERS, /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_H264_LOW_DELAY_HRD_FLAG, /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_INLINE_HEADER,            /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_SEI_ENABLE,               /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_ENCODE_INLINE_VECTORS,           /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T. */
			MMAL_PARAMETER_VIDEO_RENDER_STATS,           /**< Take a @ref MMAL_PARAMETER_VIDEO_RENDER_STATS_T. */
			MMAL_PARAMETER_VIDEO_INTERLACE_TYPE,           /**< Take a @ref MMAL_PARAMETER_VIDEO_INTERLACE_TYPE_T. */
			MMAL_PARAMETER_VIDEO_INTERPOLATE_TIMESTAMPS,         /**< Takes a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_VIDEO_ENCODE_SPS_TIMING,         /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_VIDEO_MAX_NUM_CALLBACKS,         /**< Take a @ref MMAL_PARAMETER_UINT32_T */
			MMAL_PARAMETER_VIDEO_SOURCE_PATTERN,         /**< Take a @ref MMAL_PARAMETER_SOURCE_PATTERN_T */
			MMAL_PARAMETER_VIDEO_ENCODE_SEPARATE_NAL_BUFS,  /**< Take a @ref MMAL_PARAMETER_BOOLEAN_T */
			MMAL_PARAMETER_VIDEO_DROPPABLE_PFRAME_LENGTH,   /**< Take a @ref MMAL_PARAMETER_UINT32_T */


		};

		public enum MMAL_DISPLAYSET_T
		{
			MMAL_DISPLAY_SET_NONE = 0,
			MMAL_DISPLAY_SET_NUM = 1,
			MMAL_DISPLAY_SET_FULLSCREEN = 2,
			MMAL_DISPLAY_SET_TRANSFORM = 4,
			MMAL_DISPLAY_SET_DEST_RECT = 8,
			MMAL_DISPLAY_SET_SRC_RECT = 0x10,
			MMAL_DISPLAY_SET_MODE = 0x20,
			MMAL_DISPLAY_SET_PIXEL = 0x40,
			MMAL_DISPLAY_SET_NOASPECT = 0x80,
			MMAL_DISPLAY_SET_LAYER = 0x100,
			MMAL_DISPLAY_SET_COPYPROTECT = 0x200,
			MMAL_DISPLAY_SET_ALPHA = 0x400,
			MMAL_DISPLAY_SET_DUMMY = 0x7FFFFFFF
		}


		/** Display transformations.
		 * Although an enumeration, the values correspond to combinations of:
		 * \li 1 Reflect in a vertical axis
		 * \li 2 180 degree rotation
		 * \li 4 Reflect in the leading diagonal
		 */
		public enum MMAL_DISPLAYTRANSFORM_T
		{
			MMAL_DISPLAY_ROT0 = 0,
			MMAL_DISPLAY_MIRROR_ROT0 = 1,
			MMAL_DISPLAY_MIRROR_ROT180 = 2,
			MMAL_DISPLAY_ROT180 = 3,
			MMAL_DISPLAY_MIRROR_ROT90 = 4,
			MMAL_DISPLAY_ROT270 = 5,
			MMAL_DISPLAY_ROT90 = 6,
			MMAL_DISPLAY_MIRROR_ROT270 = 7,
			MMAL_DISPLAY_DUMMY = 0x7FFFFFFF
		}

		public enum MMAL_PARAM_FLICKERAVOID_T
		{
			MMAL_PARAM_FLICKERAVOID_OFF,
			MMAL_PARAM_FLICKERAVOID_AUTO,
			MMAL_PARAM_FLICKERAVOID_50HZ,
			MMAL_PARAM_FLICKERAVOID_60HZ,
			MMAL_PARAM_FLICKERAVOID_MAX = 0x7FFFFFFF
		}


		/** Display modes. */
		public enum MMAL_DISPLAYMODE_T
		{
			MMAL_DISPLAY_MODE_FILL = 0,
			MMAL_DISPLAY_MODE_LETTERBOX = 1,
			// these allow a left eye source->dest to be specified and the right eye mapping will be inferred by symmetry
			MMAL_DISPLAY_MODE_STEREO_LEFT_TO_LEFT = 2,
			MMAL_DISPLAY_MODE_STEREO_TOP_TO_TOP = 3,
			MMAL_DISPLAY_MODE_STEREO_LEFT_TO_TOP = 4,
			MMAL_DISPLAY_MODE_STEREO_TOP_TO_LEFT = 5,
			MMAL_DISPLAY_MODE_DUMMY = 0x7FFFFFFF
		}


		/** \defgroup MmalDefaultComponents List of default components
		 * This provides a list of default components on a per platform basis.
		 * @{
		 */

		public const string MMAL_COMPONENT_DEFAULT_CAMERA = "vc.ril.camera";
		public const string MMAL_COMPONENT_DEFAULT_IMAGE_ENCODER = "vc.ril.image_encode";
		public const string MMAL_COMPONENT_DEFAULT_VIDEO_RENDERER = "vc.ril.video_render";
		public const string MMAL_COMPONENT_DEFAULT_VIDEO_DECODER = "vc.ril.video_decode";
		public const string MMAL_COMPONENT_DEFAULT_VIDEO_ENCODER = "vc.ril.video_encode";
		public const string MMAL_COMPONENT_DEFAULT_VIDEO_SPLITTER = "vc.ril.video_splitter";

		// The following two components aren't in the MMAL headers, but do exist
		public const string MMAL_COMPONENT_DEFAULT_NULL_SINK = "vc.null_sink";
		public const string MMAL_COMPONENT_DEFAULT_RESIZER = "vc.ril.resize";
		public const string MMAL_COMPONENT_DEFAULT_ISP = "vc.ril.isp";
		public const string MMAL_COMPONENT_RAW_CAMERA = "vc.ril.rawcam";

		//		public const int MMAL_PARAMETER_CAMERA_INFO_MAX_CAMERAS = 4;
		//		public const int MMAL_PARAMETER_CAMERA_INFO_MAX_FLASHES = 2;
		//#define MMAL_PARAMETER_CAMERA_INFO_MAX_STR_LEN 16

		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_PARAMETER_FPS_RANGE_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;

			public MMAL_RATIONAL_T fps_low;                /**< Low end of the permitted framerate range */
			public MMAL_RATIONAL_T fps_high;               /**< High end of the permitted framerate range */
		}

		/** Thumbnail configuration parameter type */
		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_PARAMETER_THUMBNAIL_CONFIG_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;

			public uint enable;                  /**< Enable generation of thumbnails during still capture */
			public uint width;                   /**< Desired width of the thumbnail */
			public uint height;                  /**< Desired height of the thumbnail */
			public uint quality;                 /**< Desired compression quality of the thumbnail */
		}



		/**@}*/
		/**
		This config sets the output display device, as well as the region used
		on the output display, any display transformation, and some flags to
		indicate how to scale the image.
		*/

		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_DISPLAYREGION_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;
			/** Bitfield that indicates which fields are set and should be used. All
			 * other fields will maintain their current value.
			 * \ref MMAL_DISPLAYSET_T defines the bits that can be combined.
			 */
			public uint set;
			/** Describes the display output device, with 0 typically being a directly
			 * connected LCD display.  The actual values will depend on the hardware.
			 * Code using hard-wired numbers (e.g. 2) is certain to fail.
			 */
			public uint display_num;
			/** Indicates that we are using the full device screen area, rather than
			 * a window of the display.  If zero, then dest_rect is used to specify a
			 * region of the display to use.
			 */
			public uint fullscreen;
			/** Indicates any rotation or flipping used to map frames onto the natural
			 * display orientation.
			 */
			public MMAL_DISPLAYTRANSFORM_T transform;
			/** Where to display the frame within the screen, if fullscreen is zero.
			 */
			public MMAL_RECT_T dest_rect;
			/** Indicates which area of the frame to display. If all values are zero,
			 * the whole frame will be used.
			 */
			public MMAL_RECT_T src_rect;
			/** If set to non-zero, indicates that any display scaling should disregard
			 * the aspect ratio of the frame region being displayed.
			 */
			public uint noaspect;
			/** Indicates how the image should be scaled to fit the display. \code
			 * MMAL_DISPLAY_MODE_FILL \endcode indicates that the image should fill the
			 * screen by potentially cropping the frames.  Setting \code mode \endcode
			 * to \code MMAL_DISPLAY_MODE_LETTERBOX \endcode indicates that all the source
			 * region should be displayed and black bars added if necessary.
			 */
			public MMAL_DISPLAYMODE_T mode;
			/** If non-zero, defines the width of a source pixel relative to \code pixel_y
			 * \endcode.  If zero, then pixels default to being square.
			 */
			public uint pixel_x;
			/** If non-zero, defines the height of a source pixel relative to \code pixel_x
			 * \endcode.  If zero, then pixels default to being square.
			 */
			public uint pixel_y;
			/** Sets the relative depth of the images, with greater values being in front
			 * of smaller values.
			 */
			public uint layer;
			/** Set to non-zero to ensure copy protection is used on output.
			 */
			public uint copyprotect_required;
			/** Level of opacity of the layer, where zero is fully transparent and
			 * 255 is fully opaque.
			 */
			public uint alpha;
		}


		/** Parameter header type. All parameter structures need to begin with this type.
		 * The \ref id field must be set to a parameter ID, such as one of those listed on
		 * the \ref MMAL_PARAMETER_IDS "Pre-defined MMAL parameter IDs" page.
		 */
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct MMAL_PARAMETER_HEADER_T
		{
			public uint id;      /**< Parameter ID. */
			public uint size;    /**< Size in bytes of the parameter (including the header) */
		}


		/** Generic signed 32-bit integer parameter type. */
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct MMAL_PARAMETER_INT32_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;

			public uint value; /**< Parameter value */
		}


		/** Describes a rectangle */
		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_RECT_T
		{
			public int x;      /**< x coordinate (from left) */
			public int y;      /**< y coordinate (from top) */
			public int width;  /**< width */
			public int height; /**< height */
		}


		/** Describes a rational number */
		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_RATIONAL_T
		{
			public int num;    /**< Numerator */
			public int den;    /**< Denominator */
		}


		/** \name Special Unknown Time Value
		 * Timestamps in MMAL are defined as signed 64 bits integer values representing microseconds.
		 * However a pre-defined special value is used to signal that a timestamp is not known. */
		/* @{ */
		public const Int64 MMAL_TIME_UNKNOWN = 1<<63;

		/**< Special value signalling that time is not known */
		/* @} */

		/** Definition of a video format.
		* This describes the properties specific to a video stream */
		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_VIDEO_FORMAT_T
		{
			public uint width;        /**< Width of frame in pixels */
			public uint height;       /**< Height of frame in rows of pixels */
			public MMAL_RECT_T crop;         /**< Visible region of the frame */
			public MMAL_RATIONAL_T frame_rate;   /**< Frame rate */
			public MMAL_RATIONAL_T par;          /**< Pixel aspect ratio */

			/*MMAL_FOURCC_T*/
			int color_space;  /**< FourCC specifying the color space of the
										* video stream. See the \ref MmalColorSpace
										* "pre-defined color spaces" for some examples.
										*/

		}


		[StructLayout(LayoutKind.Explicit)]
		public struct MMAL_ES_SPECIFIC_FORMAT_T // union
		{
			//MMAL_AUDIO_FORMAT_T audio;      /**< Audio specific information */
			[FieldOffset(0)]
			public MMAL_VIDEO_FORMAT_T video;      /**< Video specific information */
			//MMAL_SUBPICTURE_FORMAT_T subpicture; /**< Subpicture specific information */
		}



		/** Definition of an elementary stream format */
		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct MMAL_ES_FORMAT_T
		{
			public MMAL_ES_TYPE_T type;           /**< Type of the elementary stream */

			public UInt32 encoding;        /**< FourCC specifying the encoding of the elementary stream.
                                    * See the \ref MmalEncodings "pre-defined encodings" for some
                                    * examples.
                                    */
			public UInt32 encoding_variant;/**< FourCC specifying the specific encoding variant of
                                    * the elementary stream. See the \ref MmalEncodingVariants
                                    * "pre-defined encoding variants" for some examples.
                                    */

			public MMAL_ES_SPECIFIC_FORMAT_T* es; /**< Type specific information for the elementary stream */
			//public IntPtr es;

			public uint bitrate;              /**< Bitrate in bits per second */
			public uint flags;                /**< Flags describing properties of the elementary stream.
                                    * See \ref elementarystreamflags "Elementary stream flags".
                                    */

			public uint extradata_size;       /**< Size of the codec specific data */
			//uint8_t* extradata;           /**< Codec specific data */
			public IntPtr extradata;

		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct MMAL_PARAMETER_CAMERA_INFO_CAMERA_T
		{
			uint port_id;
			public uint max_width;
			public uint max_height;
			uint lens_present;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = MMAL_PARAMETER_CAMERA_INFO_MAX_STR_LEN)]
			public byte[] camera_name;
		}

		public enum MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_T
		{
			MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_XENON = 0, /* Make values explicit */
			MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_LED = 1, /* to ensure they match */
			MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_OTHER = 2, /* values in config ini */
			MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_MAX = 0x7FFFFFFF
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_PARAMETER_CAMERA_INFO_FLASH_T
		{
			MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_T flash_type;
		}

		public const int MMAL_MAX_IMAGEFX_PARAMETERS = 6;  /* Image effects library currently uses a maximum of 5 parameters per effect */

		public struct MMAL_PARAMETER_IMAGEFX_PARAMETERS_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;

			public MMAL_PARAM_IMAGEFX_T effect;   /**< Image effect mode */
			uint num_effect_params;     /**< Number of used elements in */
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = MMAL_MAX_IMAGEFX_PARAMETERS)]
			public uint[] effect_parameter; /**< Array of parameters */
		}
		

		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_PARAMETER_CAMERA_INFO_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;
			public uint num_cameras;
			public uint num_flashes;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = MMAL_PARAMETER_CAMERA_INFO_MAX_CAMERAS)]
			public MMAL_PARAMETER_CAMERA_INFO_CAMERA_T[] cameras;
			//public IntPtr cameras;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = MMAL_PARAMETER_CAMERA_INFO_MAX_FLASHES)]
			public MMAL_PARAMETER_CAMERA_INFO_FLASH_T[] flashes;
			//public IntPtr flashes;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct MMAL_PARAMETER_CAMERA_CONFIG_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;

			/* Parameters for setting up the image pools */
			public uint max_stills_w;        /**< Max size of stills capture */
			public uint max_stills_h;
			public uint stills_yuv422;       /**< Allow YUV422 stills capture */
			public uint one_shot_stills;     /**< Continuous or one shot stills captures. */

			public uint max_preview_video_w; /**< Max size of the preview or video capture frames */
			public uint max_preview_video_h;
			public uint num_preview_video_frames;

			public uint stills_capture_circular_buffer_height; /**< Sets the height of the circular buffer for stills capture. */

			public uint fast_preview_resume;    /**< Allows preview/encode to resume as fast as possible after the stills input frame
                                     * has been received, and then processes the still frame in the background
                                     * whilst preview/encode has resumed.
                                     * Actual mode is controlled by MMAL_PARAMETER_CAPTURE_MODE.
                                     */

			public MMAL_PARAMETER_CAMERA_CONFIG_TIMESTAMP_MODE_T use_stc_timestamp;
			/**< Selects algorithm for timestamping frames if there is no clock component connected.
			  */
		}

		public enum MMAL_STEREOSCOPIC_MODE_T
		{
			MMAL_STEREOSCOPIC_MODE_NONE = 0,
			MMAL_STEREOSCOPIC_MODE_SIDE_BY_SIDE = 1,
			MMAL_STEREOSCOPIC_MODE_TOP_BOTTOM = 2,
			MMAL_STEREOSCOPIC_MODE_MAX = 0x7FFFFFFF,
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_PARAMETER_STEREOSCOPIC_MODE_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;

			public MMAL_STEREOSCOPIC_MODE_T mode;
			public uint decimate;
			public uint swap_eyes;
		}
	


		/** Definition of a port.
		 * A port is the entity that is exposed by components to receive or transmit
		 * buffer headers (\ref MMAL_BUFFER_HEADER_T). A port is defined by its
		 * \ref MMAL_ES_FORMAT_T.
		 *
		 * It may be possible to override the buffer requirements of a port by using
		 * the MMAL_PARAMETER_BUFFER_REQUIREMENTS parameter.
		 */
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public unsafe struct MMAL_PORT_T
		{
			//struct MMAL_PORT_PRIVATE_T *priv; /**< Private member used by the framework */
			IntPtr priv;

			public IntPtr name;                 /**< Port name. Used for debugging purposes (Read Only) */

			//public MMAL_PORT_TYPE_T type;            /**< Type of the port (Read Only) */
			public MMAL_PORT_TYPE_T type;            /**< Type of the port (Read Only) */
			public short index;                   /**< Index of the port in its type list (Read Only) */
			public short index_all;               /**< Index of the port in the list of all ports (Read Only) */

			public uint is_enabled;              /**< Indicates whether the port is enabled or not (Read Only) */
			public MMAL_ES_FORMAT_T* format;         /**< Format of the elementary stream */
			//public IntPtr format;

			public uint buffer_num_min;          /**< Minimum number of buffers the port requires (Read Only).
                                          This is set by the component. */
			public uint buffer_size_min;         /**< Minimum size of buffers the port requires (Read Only).
                                          This is set by the component. */
			public uint buffer_alignment_min;    /**< Minimum alignment requirement for the buffers (Read Only).
                                          A value of zero means no special alignment requirements.
                                          This is set by the component. */
			public uint buffer_num_recommended;  /**< Number of buffers the port recommends for optimal performance (Read Only).
                                          A value of zero means no special recommendation.
                                          This is set by the component. */
			public uint buffer_size_recommended; /**< Size of buffers the port recommends for optimal performance (Read Only).
                                          A value of zero means no special recommendation.
                                          This is set by the component. */
			public uint buffer_num;              /**< Actual number of buffers the port will use.
                                          This is set by the client. */
			public uint buffer_size;             /**< Actual maximum size of the buffers that will be sent
                                          to the port. This is set by the client. */

			//struct MMAL_COMPONENT_T *component;    /**< Component this port belongs to (Read Only) */
			public MMAL_COMPONENT_T* component;
			// struct MMAL_PORT_USERDATA_T *userdata; /**< Field reserved for use by the client */
			public IntPtr userdata;

			public uint capabilities;            /**< Flags describing the capabilities of a port (Read Only).
                                       * Bitwise combination of \ref portcapabilities "Port capabilities"
                                       * values.
                                       */

		}


		/** Definition of a component. */
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public unsafe struct MMAL_COMPONENT_T
		{
			/** Pointer to the private data of the module in use */
			//struct MMAL_COMPONENT_PRIVATE_T *priv;
			IntPtr priv;

			/** Pointer to private data of the client */
			//struct MMAL_COMPONENT_USERDATA_T *userdata;
			IntPtr userdata;

			/** Component name */
			public IntPtr name;

			/** Specifies whether the component is enabled or not */
			uint is_enabled;

			/** All components expose a control port.
			 * The control port is used by clients to set / get parameters that are global to the
			 * component. It is also used to receive events, which again are global to the component.
			 * To be able to receive events, the client needs to enable and register a callback on the
			 * control port. */
			public MMAL_PORT_T* control;
			//public IntPtr control;

			public uint input_num;   /**< Number of input ports */
			public MMAL_PORT_T** input;     /**< Array of input ports */
			//public IntPtr input;

			public uint output_num;  /**< Number of output ports */
			public MMAL_PORT_T** output;    /**< Array of output ports */
			//public IntPtr output;

			public uint clock_num;   /**< Number of clock ports */
			//MMAL_PORT_T** clock;     /**< Array of clock ports */
			public IntPtr clock;

			public uint port_num;    /**< Total number of ports */
			public MMAL_PORT_T** port;      /**< Array of all the ports (control/input/output/clock) */
			//public IntPtr port;
			/** Uniquely identifies the component's instance within the MMAL
			 * context / process. For debugging. */
			public uint id;

		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public unsafe struct MMAL_QUEUE_T
		{

		}

		/** Specific data associated with video frames */
		[StructLayout(LayoutKind.Sequential)]
		unsafe struct MMAL_BUFFER_HEADER_VIDEO_SPECIFIC_T
		{
			uint planes;     /**< Number of planes composing the video frame */
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			uint[] offset;  /**< Offsets to the different planes. These must point within the
                             payload buffer */
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			uint[] pitch;   /**< Pitch (size in bytes of a line of a plane) of the different
                             planes */
			uint flags;      /**< Flags describing video specific properties of a buffer header
                             (see \ref videobufferheaderflags "Video buffer header flags") */
			/* TBD stereoscopic support */
		}
	;

		/** Type specific data that's associated with a payload buffer */
		[StructLayout(LayoutKind.Explicit)]
		unsafe struct MMAL_BUFFER_HEADER_TYPE_SPECIFIC_T
		{
			/** Specific data associated with video frames */
			[FieldOffset(0)]
			MMAL_BUFFER_HEADER_VIDEO_SPECIFIC_T video;

		}

		/** Video profiles.
		 * Only certain combinations of profile and level will be valid.
		 * @ref MMAL_VIDEO_LEVEL_T
		 */
		public enum MMAL_VIDEO_PROFILE_T
		{
			MMAL_VIDEO_PROFILE_H263_BASELINE,
			MMAL_VIDEO_PROFILE_H263_H320CODING,
			MMAL_VIDEO_PROFILE_H263_BACKWARDCOMPATIBLE,
			MMAL_VIDEO_PROFILE_H263_ISWV2,
			MMAL_VIDEO_PROFILE_H263_ISWV3,
			MMAL_VIDEO_PROFILE_H263_HIGHCOMPRESSION,
			MMAL_VIDEO_PROFILE_H263_INTERNET,
			MMAL_VIDEO_PROFILE_H263_INTERLACE,
			MMAL_VIDEO_PROFILE_H263_HIGHLATENCY,
			MMAL_VIDEO_PROFILE_MP4V_SIMPLE,
			MMAL_VIDEO_PROFILE_MP4V_SIMPLESCALABLE,
			MMAL_VIDEO_PROFILE_MP4V_CORE,
			MMAL_VIDEO_PROFILE_MP4V_MAIN,
			MMAL_VIDEO_PROFILE_MP4V_NBIT,
			MMAL_VIDEO_PROFILE_MP4V_SCALABLETEXTURE,
			MMAL_VIDEO_PROFILE_MP4V_SIMPLEFACE,
			MMAL_VIDEO_PROFILE_MP4V_SIMPLEFBA,
			MMAL_VIDEO_PROFILE_MP4V_BASICANIMATED,
			MMAL_VIDEO_PROFILE_MP4V_HYBRID,
			MMAL_VIDEO_PROFILE_MP4V_ADVANCEDREALTIME,
			MMAL_VIDEO_PROFILE_MP4V_CORESCALABLE,
			MMAL_VIDEO_PROFILE_MP4V_ADVANCEDCODING,
			MMAL_VIDEO_PROFILE_MP4V_ADVANCEDCORE,
			MMAL_VIDEO_PROFILE_MP4V_ADVANCEDSCALABLE,
			MMAL_VIDEO_PROFILE_MP4V_ADVANCEDSIMPLE,
			MMAL_VIDEO_PROFILE_H264_BASELINE,
			MMAL_VIDEO_PROFILE_H264_MAIN,
			MMAL_VIDEO_PROFILE_H264_EXTENDED,
			MMAL_VIDEO_PROFILE_H264_HIGH,
			MMAL_VIDEO_PROFILE_H264_HIGH10,
			MMAL_VIDEO_PROFILE_H264_HIGH422,
			MMAL_VIDEO_PROFILE_H264_HIGH444,
			MMAL_VIDEO_PROFILE_H264_CONSTRAINED_BASELINE,
			MMAL_VIDEO_PROFILE_DUMMY = 0x7FFFFFFF
		}


		/** Video levels.
		 * Only certain combinations of profile and level will be valid.
		 * @ref MMAL_VIDEO_PROFILE_T
		 */
		public enum MMAL_VIDEO_LEVEL_T
		{
			MMAL_VIDEO_LEVEL_H263_10,
			MMAL_VIDEO_LEVEL_H263_20,
			MMAL_VIDEO_LEVEL_H263_30,
			MMAL_VIDEO_LEVEL_H263_40,
			MMAL_VIDEO_LEVEL_H263_45,
			MMAL_VIDEO_LEVEL_H263_50,
			MMAL_VIDEO_LEVEL_H263_60,
			MMAL_VIDEO_LEVEL_H263_70,
			MMAL_VIDEO_LEVEL_MP4V_0,
			MMAL_VIDEO_LEVEL_MP4V_0b,
			MMAL_VIDEO_LEVEL_MP4V_1,
			MMAL_VIDEO_LEVEL_MP4V_2,
			MMAL_VIDEO_LEVEL_MP4V_3,
			MMAL_VIDEO_LEVEL_MP4V_4,
			MMAL_VIDEO_LEVEL_MP4V_4a,
			MMAL_VIDEO_LEVEL_MP4V_5,
			MMAL_VIDEO_LEVEL_MP4V_6,
			MMAL_VIDEO_LEVEL_H264_1,
			MMAL_VIDEO_LEVEL_H264_1b,
			MMAL_VIDEO_LEVEL_H264_11,
			MMAL_VIDEO_LEVEL_H264_12,
			MMAL_VIDEO_LEVEL_H264_13,
			MMAL_VIDEO_LEVEL_H264_2,
			MMAL_VIDEO_LEVEL_H264_21,
			MMAL_VIDEO_LEVEL_H264_22,
			MMAL_VIDEO_LEVEL_H264_3,
			MMAL_VIDEO_LEVEL_H264_31,
			MMAL_VIDEO_LEVEL_H264_32,
			MMAL_VIDEO_LEVEL_H264_4,
			MMAL_VIDEO_LEVEL_H264_41,
			MMAL_VIDEO_LEVEL_H264_42,
			MMAL_VIDEO_LEVEL_H264_5,
			MMAL_VIDEO_LEVEL_H264_51,
			MMAL_VIDEO_LEVEL_DUMMY = 0x7FFFFFFF
		}

		/** Video profile and level setting.
		 * This is a variable length structure when querying the supported profiles and
		 * levels. To get more than one, pass a structure with more profile/level pairs.
		 */
		[StructLayout(LayoutKind.Sequential)]
		public struct VIDEO_PROFILE
		{
			public MMAL_VIDEO_PROFILE_T profile;
			public MMAL_VIDEO_LEVEL_T level;
		}
	
	
		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_PARAMETER_VIDEO_PROFILE_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;
			//[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
			public VIDEO_PROFILE profile;
		}

		/** Intra refresh modes */
		public enum MMAL_VIDEO_INTRA_REFRESH_T
		{
			MMAL_VIDEO_INTRA_REFRESH_CYCLIC,
			MMAL_VIDEO_INTRA_REFRESH_ADAPTIVE,
			MMAL_VIDEO_INTRA_REFRESH_BOTH,
			MMAL_VIDEO_INTRA_REFRESH_KHRONOSEXTENSIONS = 0x6F000000,
			MMAL_VIDEO_INTRA_REFRESH_VENDORSTARTUNUSED = 0x7F000000,
			MMAL_VIDEO_INTRA_REFRESH_CYCLIC_MROWS,
			MMAL_VIDEO_INTRA_REFRESH_PSEUDO_RAND,
			MMAL_VIDEO_INTRA_REFRESH_MAX,
			MMAL_VIDEO_INTRA_REFRESH_DUMMY = 0x7FFFFFFF
		}
		
		/** H264 Only: Overrides for max macro-blocks per second, max framesize,
		 * and max bitrates. This overrides the default maximums for the configured level.
		 */
		[StructLayout(LayoutKind.Sequential)]
		public struct MMAL_PARAMETER_VIDEO_INTRA_REFRESH_T
		{
			public MMAL_PARAMETER_HEADER_T hdr;
			public MMAL_VIDEO_INTRA_REFRESH_T refresh_mode;
			public uint air_mbs;
			public uint air_ref;
			public uint cir_mbs;
			public uint pir_mbs;
		}
		
		/** Definition of the buffer header structure.
		 * A buffer header does not directly carry the data to be passed to a component but instead
		 * it references the actual data using a pointer (and an associated length).
		 * It also contains an internal area which can be used to store command to be associated
		 * with the external data.
		 */
		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct MMAL_BUFFER_HEADER_T
		{
			public MMAL_BUFFER_HEADER_T *next; /**< Used to link several buffer headers together */

			//public MMAL_BUFFER_HEADER_PRIVATE_T *priv; /**< Data private to the framework */
			public IntPtr priv;

			public uint cmd;              /**< Defines what the buffer header contains. This is a FourCC
                                   with 0 as a special value meaning stream data */

			public char* data;            /**< Pointer to the start of the payload buffer (should not be
                                   changed by component) */
			uint alloc_size;       /**< Allocated size in bytes of payload buffer */
			public uint length;           /**< Number of bytes currently used in the payload buffer (starting
                                   from offset) */
			uint offset;           /**< Offset in bytes to the start of valid data in the payload buffer */

			public uint flags;            /**< Flags describing properties of a buffer header (see
                                   \ref bufferheaderflags "Buffer header flags") */

			public Int64 pts;              /**< Presentation timestamp in microseconds. \ref MMAL_TIME_UNKNOWN
                                   is used when the pts is unknown. */
			Int64 dts;              /**< Decode timestamp in microseconds (dts = pts, except in the case
                                   of video streams with B frames). \ref MMAL_TIME_UNKNOWN
                                   is used when the dts is unknown. */

			/** Type specific data that's associated with a payload buffer */
			//MMAL_BUFFER_HEADER_TYPE_SPECIFIC_T* type;
			public IntPtr type;

			void* user_data;           /**< Field reserved for use by the client */

		}
		
		/** Definition of a pool */
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public unsafe struct MMAL_POOL_T
		{
			public MMAL_QUEUE_T* queue;             /**< Queue used by the pool */
			public uint headers_num;            /**< Number of buffer headers in the pool */
			public MMAL_BUFFER_HEADER_T** header;   /**< Array of buffer headers belonging to the pool */
		}

		
		/** Signals that the current payload is the end of the stream of data */
		public const int MMAL_BUFFER_HEADER_FLAG_EOS = (1 << 0);
		/** Signals that the start of the current payload starts a frame */
		public const int MMAL_BUFFER_HEADER_FLAG_FRAME_START = (1 << 1);
		/** Signals that the end of the current payload ends a frame */
		public const int MMAL_BUFFER_HEADER_FLAG_FRAME_END = (1 << 2);
		/** Signals that the current payload contains only complete frames (1 or more) */
		public const int MMAL_BUFFER_HEADER_FLAG_FRAME = (MMAL_BUFFER_HEADER_FLAG_FRAME_START | MMAL_BUFFER_HEADER_FLAG_FRAME_END);
		/** Signals that the current payload is a keyframe (i.e. self decodable) */
		public const int MMAL_BUFFER_HEADER_FLAG_KEYFRAME = (1 << 3);
		/** Signals a buffer containing some kind of config data for the component
		  * (e.g. codec config data) */
		/** Signals a discontinuity in the stream of data (e.g. after a seek).
		* Can be used for instance by a decoder to reset its state */
		public const int MMAL_BUFFER_HEADER_FLAG_DISCONTINUITY = (1 << 4);

		public const int MMAL_BUFFER_HEADER_FLAG_CONFIG = (1 << 5);
		/** Signals an encrypted payload */
		public const int MMAL_BUFFER_HEADER_FLAG_ENCRYPTED = (1 << 6);

		/** Signals a buffer containing side information */
		public const int MMAL_BUFFER_HEADER_FLAG_CODECSIDEINFO = (1 << 7);
		/** Signals that a buffer failed to be transmitted */
		/** Signals a buffer which is the snapshot/postview image from a stills capture */
		public const int MMAL_BUFFER_HEADER_FLAGS_SNAPSHOT = (1 << 8);
		/** Signals a buffer which contains data known to be corrupted */
		public const int MMAL_BUFFER_HEADER_FLAG_CORRUPTED = (1 << 9);
		/** Signals that a buffer failed to be transmitted */
		public const int MMAL_BUFFER_HEADER_FLAG_TRANSMISSION_FAILED = (1 << 10);
		/** Signals the output buffer won't be used, just update reference frames */
		public const int MMAL_BUFFER_HEADER_FLAG_DECODEONLY = (1 << 11);
		/** Signals that the end of the current payload ends a NAL */
		public const int MMAL_BUFFER_HEADER_FLAG_NAL_END = (1 << 12);

		/** The connection is tunnelled. Buffer headers do not transit via the client but
		* directly from the output port to the input port. */
		public const int MMAL_CONNECTION_FLAG_TUNNELLING = 0x1;
		/** Force the pool of buffer headers used by the connection to be allocated on the input port. */
		public const int MMAL_CONNECTION_FLAG_ALLOCATION_ON_INPUT = 0x2;
		
		//typedef void (* MMAL_CONNECTION_CALLBACK_T) (MMAL_CONNECTION_T* connection);

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct MMAL_CONNECTION_T
		{
			void* user_data;           /**< Field reserved for use by the client. */
			IntPtr /*MMAL_CONNECTION_CALLBACK_T*/ callback; /**< Callback set by the client. */

			uint is_enabled;       /**< Specifies whether the connection is enabled or not (Read Only). */

			uint flags;            /**< Flags passed during the create call (Read Only). A bitwise
                               * combination of \ref connectionflags "Connection flags" values.
                               */
			MMAL_PORT_T* input;           /**< Input port used for the connection (Read Only). */
			MMAL_PORT_T* output;          /**< Output port used for the connection (Read Only). */

			MMAL_POOL_T* pool;         /**< Pool of buffer headers used by the output port (Read Only). */
			MMAL_QUEUE_T* queue;       /**< Queue for the buffer headers produced by the output port (Read Only). */

			public /*char**/ IntPtr name;          /**< Connection name (Read Only). Used for debugging purposes. */

			/* Used for debug / statistics */
			Int64 time_setup;        /**< Time in microseconds taken to setup the connection. */
			Int64 time_enable;       /**< Time in microseconds taken to enable the connection. */
			Int64 time_disable;      /**< Time in microseconds taken to disable the connection. */
		}

		public static uint MMAL_FOURCC(char a, char b, char c, char d)
		{
			return (uint) ((a) | (b << 8) | (c << 16) | (d << 24));
		}

		/** VideoCore opaque image format, image handles are returned to
		 * the host but not the actual image data.
		 */
		public static uint MMAL_ENCODING_OPAQUE
		{
			get { return MMAL_FOURCC('O', 'P', 'Q', 'V'); }
		}

		public static uint MMAL_ENCODING_I420
		{
			get { return MMAL_FOURCC('I', '4', '2', '0'); }
		}

		public static uint MMAL_ENCODING_JPEG
		{
			get { return MMAL_FOURCC('J', 'P', 'E', 'G'); }
		}

		public static uint MMAL_ENCODING_RGB24
		{
			get { return MMAL_FOURCC('R', 'G', 'B', '3'); }
		}
		public static uint MMAL_ENCODING_BGR24
		{
			get { return MMAL_FOURCC('B', 'G', 'R', '3'); }
		}

		public static uint MMAL_ENCODING_RGBA
		{
			get { return MMAL_FOURCC('R', 'G', 'B', 'A'); }
		}

		public static uint MMAL_ENCODING_BGRA
		{
			get { return MMAL_FOURCC('B', 'G', 'R', 'A'); }
		}

		public static uint MMAL_ENCODING_GIF
		{
			get { return MMAL_FOURCC('G', 'I', 'F', ' '); }
		}

		public static uint MMAL_ENCODING_PNG
		{
			get { return MMAL_FOURCC('P', 'N', 'G', ' '); }
		}

		public static uint MMAL_ENCODING_PPM
		{
			get
			{
				return MMAL_FOURCC('P', 'P', 'M', ' ');
			}
		}

		
		public static uint MMAL_ENCODING_TGA
		{
			get
			{
				return MMAL_FOURCC('T', 'G', 'A', ' ');
			}
		}

		public static uint MMAL_ENCODING_BMP
		{
			get
			{
				return MMAL_FOURCC('B', 'M', 'P', ' ');
			}
		}

		public static uint MMAL_ENCODING_MJPEG
		{
			get { return MMAL_FOURCC('M', 'J', 'P', 'G'); }
		}

		public static uint MMAL_ENCODING_H264
		{
			get { return MMAL_FOURCC('H', '2', '6', '4'); }
		}

		

		/** \defgroup MmalEvents List of pre-defined event types
		 * This defines a list of standard event types. Components can still define proprietary
		 * event types by using their own FourCC and defining their own event structures. */
		/* @{ */

		/** \name Pre-defined event FourCCs */
		/* @{ */

		/** Error event. Data contains a \ref MMAL_STATUS_T */
		public static uint MMAL_EVENT_ERROR
		{
			get { return MMAL_FOURCC('E', 'R', 'R', 'O'); }
		}

		/** End-of-stream event. Data contains a \ref MMAL_EVENT_END_OF_STREAM_T */
		public static uint MMAL_EVENT_EOS
		{
			get { return MMAL_FOURCC('E', 'E', 'O', 'S'); }
		}

		/** Format changed event. Data contains a \ref MMAL_EVENT_FORMAT_CHANGED_T */
		public static uint MMAL_EVENT_FORMAT_CHANGED
		{
			get { return MMAL_FOURCC('E', 'F', 'C', 'H'); }
		}

		/** Parameter changed event. Data contains the new parameter value, see
		 * \ref MMAL_EVENT_PARAMETER_CHANGED_T
		 */
		public static uint MMAL_EVENT_PARAMETER_CHANGED
		{
			get { return MMAL_FOURCC('E', 'P', 'C', 'H'); }
		}

		/* @} */
		public static int VCOS_ALIGN_DOWN(int p, int n)
		{
			return (((p)) & ~((n) - 1));
		}

        public static int VCOS_ALIGN_UP(int p, int n)
		{
			return VCOS_ALIGN_DOWN((p) + (n) - 1, n);
		}

		/** Definition of the callback used by a port to send a \ref MMAL_BUFFER_HEADER_T
		 * back to the user.
		 *
		 * @param port The port sending the buffer header.
		 * @param buffer The buffer header being sent.
		*/
		// typedef void (* MMAL_PORT_BH_CB_T) (MMAL_PORT_T* port, MMAL_BUFFER_HEADER_T* buffer);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate void MMAL_PORT_BH_CB_T(MMAL_PORT_T* port, MMAL_BUFFER_HEADER_T* buffer);
		/** Create an instance of a component.
		 * The newly created component will expose ports to the client. All the exposed ports are
		 * disabled by default.
		 * Note that components are reference counted and creating a component automatically
		 * acquires a reference to it (released when \ref mmal_component_destroy is called).
		 *
		 * @param name name of the component to create, e.g. "video_decode"
		 * @param component returned component
		 * @return MMAL_SUCCESS on success
		 */
		//MMAL_STATUS_T mmal_component_create(const char* name,
		//									MMAL_COMPONENT_T **component);

			
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static extern unsafe MMAL_STATUS_T mmal_component_create(string name, MMal.MMAL_COMPONENT_T** camera);


		/** Destroy a previously created component
		 * Release an acquired reference on a component. Only actually destroys the component when
		 * the last reference is being released.
		 *
		 * @param component component to destroy
		 * @return MMAL_SUCCESS on success
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static extern unsafe MMAL_STATUS_T mmal_component_destroy(MMAL_COMPONENT_T* component);

		/** Get a parameter from a port.
		 * The size field must be set on input to the maximum size of the parameter
		 * (including the header) and will be set on output to the actual size of the
		 * parameter retrieved.
		 *
		 * \note If MMAL_ENOSPC is returned, the parameter is larger than the size
		 * given. The given parameter will have been filled up to its size and then
		 * the size field set to the full parameter's size. This can be used to
		 * resize the parameter buffer so that a second call should succeed.
		 *
		 * @param port The port to which the request is sent.
		 * @param param The pointer to the header of the parameter to get.
		 * @return MMAL_SUCCESS on success
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static extern unsafe MMAL_STATUS_T mmal_port_parameter_get(MMAL_PORT_T* port, MMAL_PARAMETER_HEADER_T* param);

		/** Set a parameter on a port.
		 *
		 * @param port The port to which the request is sent.
		 * @param param The pointer to the header of the parameter to set.
		 * @return MMAL_SUCCESS on success
		 */
		// MMAL_STATUS_T mmal_port_parameter_set(MMAL_PORT_T* port, const MMAL_PARAMETER_HEADER_T* param);

#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_STATUS_T mmal_port_parameter_set(MMal.MMAL_PORT_T* port, MMAL_PARAMETER_HEADER_T* param);

		/** Enable processing on a port
		 *
		 * If this port is connected to another, the given callback must be NULL, while for a
		 * disconnected port, the callback must be non-NULL.
		 *
		 * If this is a connected output port and is successfully enabled:
		 * <ul>
		 * <li>The port shall be populated with a pool of buffers, allocated as required, according
		 * to the buffer_num and buffer_size values.
		 * <li>The input port to which it is connected shall be set to the same buffer
		 * configuration and then be enabled. Should that fail, the original port shall be
		 * disabled.
		 * </ul>
		 *
		 * @param port port to enable
		 * @param cb callback use by the port to send a \ref MMAL_BUFFER_HEADER_T back
		 * @return MMAL_SUCCESS on success
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static extern unsafe MMAL_STATUS_T mmal_port_enable(MMAL_PORT_T* port, 
															[MarshalAs(UnmanagedType.FunctionPtr)] MMAL_PORT_BH_CB_T cb);


		/** Disable processing on a port
		 *
		 * Disabling a port will stop all processing on this port and return all (non-processed)
		 * buffer headers to the client.
		 *
		 * If this is a connected output port, the input port to which it is connected shall
		 * also be disabled. Any buffer pool shall be released.
		 *
		 * @param port port to disable
		 * @return MMAL_SUCCESS on success
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static extern unsafe MMAL_STATUS_T mmal_port_disable(MMAL_PORT_T* port);


		/** Enable processing on a component
		 * @param component component to enable
		 * @return MMAL_SUCCESS on success
		*/
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static extern unsafe MMAL_STATUS_T mmal_component_enable(MMAL_COMPONENT_T* component);

		/** Disable processing on a component
		 * @param component component to disable
		 * @return MMAL_SUCCESS on success
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static extern unsafe MMAL_STATUS_T mmal_component_disable(MMAL_COMPONENT_T* component);

		/** Helper function to set the value of a 32 bits unsigned integer parameter.
		 * @param port   port on which to set the parameter
		 * @param id     parameter id
		 * @param value  value to set the parameter to
		 *
		 * @return MMAL_SUCCESS or error
		 */
		//	MMAL_STATUS_T mmal_port_parameter_set_uint32(MMAL_PORT_T* port, uint32_t id, uint32_t value);
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static extern unsafe MMAL_STATUS_T mmal_port_parameter_set_uint32(MMAL_PORT_T* port, uint id, uint value);

		/** Commit format changes on a port.
		 *
		 * @param port The port for which format changes are to be committed.
		 * @return MMAL_SUCCESS on success
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static extern unsafe MMAL_STATUS_T mmal_port_format_commit(MMAL_PORT_T* port);

		/** Fully copy a format structure, including the extradata buffer.
		 *
		 * @param format_dest destination \ref MMAL_ES_FORMAT_T for the copy
		 * @param format_src source \ref MMAL_ES_FORMAT_T for the copy
		 * @return MMAL_SUCCESS on success
		*/
		//MMAL_STATUS_T mmal_format_full_copy(MMAL_ES_FORMAT_T* format_dest, MMAL_ES_FORMAT_T* format_src);
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_STATUS_T mmal_format_full_copy(MMAL_ES_FORMAT_T* format_dest, MMAL_ES_FORMAT_T* format_src);


		/** Shallow copy a format structure.
		 * It is worth noting that the extradata buffer will not be copied in the new format.
		 *
		 * @param format_dest destination \ref MMAL_ES_FORMAT_T for the copy
		 * @param format_src source \ref MMAL_ES_FORMAT_T for the copy
		 */
		// void mmal_format_copy(MMAL_ES_FORMAT_T* format_dest, MMAL_ES_FORMAT_T* format_src);
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern void mmal_format_copy(MMAL_ES_FORMAT_T* format_dest, MMAL_ES_FORMAT_T* format_src);


		/** Create a pool of MMAL_BUFFER_HEADER_T associated with a specific port.
		 * This allows a client to allocate memory for the payload buffers based on the preferences
		 * of a port. This for instance will allow the port to allocate memory which can be shared
		 * between the host processor and videocore.
		 * After allocation, all allocated buffer headers will have been added to the queue.
		 *
		 * It is valid to create a pool with no buffer headers, or with zero size payload buffers.
		 * The mmal_pool_resize() function can be used to increase or decrease the number of buffer
		 * headers, or the size of the payload buffers, after creation of the pool.
		 *
		 * @param port         Port responsible for creating the pool.
		 * @param headers      Number of buffers which will be allocated with the pool.
		 * @param payload_size Size of the payload buffer which will be allocated in
		 *                     each of the buffer headers.
		 * @return Pointer to the newly created pool or NULL on failure.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_POOL_T* mmal_port_pool_create(MMAL_PORT_T* port, uint headers, uint payload_size);
		/** Destroy a pool of MMAL_BUFFER_HEADER_T associated with a specific port.
		 * This will also deallocate all of the memory which was allocated when creating or
		 * resizing the pool.
		 *
		 * @param port  Pointer to the port responsible for creating the pool.
		 * @param pool  Pointer to the pool to be destroyed.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern	void mmal_port_pool_destroy(MMAL_PORT_T* port, MMAL_POOL_T* pool);

		/** Destroy a pool of MMAL_BUFFER_HEADER_T.
		 * This will also deallocate all of the memory which was allocated when creating or
		 * resizing the pool.
		 *
		 * If payload buffers have been allocated independently by the client, they should be
		 * released prior to calling this function.If the client provided allocator functions,
		 * the allocator_free function shall be called for each payload buffer.
		 *
		 * @param pool Pointer to a pool
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern void mmal_pool_destroy(MMAL_POOL_T* pool);

		/** Create a connection between two ports.
		 * The connection shall include a pool of buffer headers suitable for the current format of
		 * the output port. The format of the input port shall have been set to the same as that of
		 * the input port.
		 * Note that connections are reference counted and creating a connection automatically
		 * acquires a reference to it (released when \ref mmal_connection_destroy is called).
		 *
		 * @param connection The address of a connection pointer that will be set to point to the created
		 * connection.
		 * @param out        The output port to use for the connection.
		 * @param in         The input port to use for the connection.
		 * @param flags      The flags specifying which type of connection should be created.
		 *    A bitwise combination of \ref connectionflags "Connection flags" values.
		 * @return MMAL_SUCCESS on success.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_STATUS_T mmal_connection_create(MMAL_CONNECTION_T** connection, MMAL_PORT_T* output, MMAL_PORT_T* input, uint flags);

		/** Enable a connection.

 * 		* The format of the two ports must have been committed before calling this function,
		* although note that on creation, the connection automatically copies and commits the
		* output port's format to the input port.
		*
		* The MMAL_CONNECTION_T::callback field must have been set if the \ref MMAL_CONNECTION_FLAG_TUNNELLING
		* flag was not specified on creation. The client may also set the MMAL_CONNECTION_T::user_data
		* in order to get a pointer passed, via the connection, to the callback.
		*
		* @param connection The connection to be enabled.
		* @return MMAL_SUCCESS on success.
		*/
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_STATUS_T mmal_connection_enable(MMAL_CONNECTION_T* connection);

		/** Disable a connection.
		 *
		 * @param connection The connection to be disabled.
		 * @return MMAL_SUCCESS on success.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_STATUS_T mmal_connection_disable(MMAL_CONNECTION_T* connection);

		/** Destroy a connection.
		 * Release an acquired reference on a connection. Only actually destroys the connection when
		 * the last reference is being released.
		 * The actual destruction of the connection will start by disabling it, if necessary.
		 * Any pool, queue, and so on owned by the connection shall then be destroyed.
		 *
		 * @param connection The connection to be destroyed.
		 * @return MMAL_SUCCESS on success.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_STATUS_T mmal_connection_destroy(MMAL_CONNECTION_T* connection);
		/** Get the number of MMAL_BUFFER_HEADER_T currently in a queue.
		 *
		 * @param queue  Pointer to a queue
		 *
		 * @return length (in elements) of the queue.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern uint mmal_queue_length(MMAL_QUEUE_T* queue);


		/** Get a MMAL_BUFFER_HEADER_T from a queue
		 *
		 * @param queue  Pointer to a queue
		 *
		 * @return pointer to the next MMAL_BUFFER_HEADER_T or NULL if the queue is empty.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_BUFFER_HEADER_T* mmal_queue_get(MMAL_QUEUE_T* queue);

		/** Wait for a MMAL_BUFFER_HEADER_T from a queue.
		 * This is the same as a get except that this will block until a buffer header is
		 * available.
		 *
		 * @param queue  Pointer to a queue
		 *
		 * @return pointer to the next MMAL_BUFFER_HEADER_T.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_BUFFER_HEADER_T* mmal_queue_wait(MMAL_QUEUE_T* queue);


		/** Wait for a MMAL_BUFFER_HEADER_T from a queue, up to a given timeout.
		 * This is the same as a wait, except that it will abort in case of timeout.
		 *
		 * @param queue  Pointer to a queue
		 * @param timeout Number of milliseconds to wait before
		 *                returning if the semaphore can't be acquired.
		 *
		 * @return pointer to the next MMAL_BUFFER_HEADER_T.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_BUFFER_HEADER_T* mmal_queue_timedwait(MMAL_QUEUE_T* queue, uint timeout);
		/** Send a buffer header to a port.
		 *
		 * @param port The port to which the buffer header is to be sent.
		 * @param buffer The buffer header to send.
		 * @return MMAL_SUCCESS on success
		*/
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_STATUS_T mmal_port_send_buffer(MMAL_PORT_T* port, MMAL_BUFFER_HEADER_T* buffer);

		/** Helper function to set the value of a boolean parameter.
		 * @param port   port on which to set the parameter
		 * @param id     parameter id
		 * @param value  value to set the parameter to
		 *
		 * @return MMAL_SUCCESS or error
		*/
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_STATUS_T mmal_port_parameter_set_boolean(MMAL_PORT_T* port, uint id, int value);

		/** Lock the data buffer contained in the buffer header in memory.
		 * This call does nothing on all platforms except VideoCore where it is needed to pin a
		 * buffer in memory before any access to it.
		 *
		 * @param header buffer header to lock
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_STATUS_T mmal_buffer_header_mem_lock(MMAL_BUFFER_HEADER_T* header);

		/** Unlock the data buffer contained in the buffer header.
		 * This call does nothing on all platforms except VideoCore where it is needed to un-pin a
		 * buffer in memory after any access to it.
		 *
		 * @param header buffer header to unlock
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern void mmal_buffer_header_mem_unlock(MMAL_BUFFER_HEADER_T* header);

		/** Release a buffer header.
		 * Releasing a buffer header will decrease its reference counter and when no more references
		 * are left, the buffer header will be recycled by calling its 'release' callback function.
		 *
		 * If a pre-release callback is set (\ref MMAL_BH_PRE_RELEASE_CB_T), this will be invoked
		 * before calling the buffer's release callback and potentially postpone buffer recycling.
		 * Once pre-release is complete the buffer header is recycled with
		 * \ref mmal_buffer_header_release_continue.
		 *
		 * @param header buffer header to release
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern void mmal_buffer_header_release(MMAL_BUFFER_HEADER_T* header);

		/** Create a queue of MMAL_BUFFER_HEADER_T
		 *
		 * @return Pointer to the newly created queue or NULL on failure.
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern MMAL_QUEUE_T* mmal_queue_create();

		/** Destroy a queue of MMAL_BUFFER_HEADER_T.
		 *
		 * @param queue  Pointer to a queue
		 */
#if _WIN32_
		[DllImport("D:\\source\\repos\\NetCore\\RaspiCam\\Debug\\TestDDL.dll", CallingConvention = CallingConvention.Cdecl)]
#else
		[DllImport("libmmal.so", CallingConvention = CallingConvention.Cdecl)]
#endif
		public static unsafe extern void mmal_queue_destroy(MMAL_QUEUE_T* queue);
	}
}
