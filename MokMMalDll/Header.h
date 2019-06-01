#pragma once

#ifdef __cplusplus
extern "C" {
#endif


	/* C99 64bits integers */
#ifndef INT64_C
# define INT64_C(value) value##LL
# define UINT64_C(value) value##ULL
#endif

#define MMAL_TSRING(s) #s
#define MMAL_TO_STRING(s) MMAL_TSRING(s)

#define MMAL_COUNTOF(x) (sizeof((x))/sizeof((x)[0]))
#define MMAL_MIN(a,b) ((a)<(b)?(a):(b))
#define MMAL_MAX(a,b) ((a)<(b)?(b):(a))

/* FIXME: should be different for big endian */
#define MMAL_FOURCC(a,b,c,d) ((a) | (b << 8) | (c << 16) | (d << 24))
#define MMAL_PARAM_UNUSED(a) (void)(a)
#define MMAL_MAGIC MMAL_FOURCC('m','m','a','l')

	typedef int32_t MMAL_BOOL_T;
#define MMAL_FALSE   0
#define MMAL_TRUE    1

	typedef struct MMAL_CORE_STATISTICS_T
	{
		uint32_t buffer_count;        /**< Total buffer count on this port */
		uint32_t first_buffer_time;   /**< Time (us) of first buffer seen on this port */
		uint32_t last_buffer_time;    /**< Time (us) of most recently buffer on this port */
		uint32_t max_delay;           /**< Max delay (us) between buffers, ignoring first few frames */
	} MMAL_CORE_STATISTICS_T;

	/** Statistics collected by the core on all ports, if enabled in the build.
	 */
	typedef struct MMAL_CORE_PORT_STATISTICS_T
	{
		MMAL_CORE_STATISTICS_T rx;
		MMAL_CORE_STATISTICS_T tx;
	} MMAL_CORE_PORT_STATISTICS_T;

	/** Unsigned 16.16 fixed point value, also known as Q16.16 */
	typedef uint32_t MMAL_FIXED_16_16_T;
/** Status return codes from the API.
 *
 * \internal Please try to keep this similar to the standard POSIX codes
 * rather than making up new ones!
 */
typedef enum
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
} MMAL_STATUS_T;

/** Describes a rectangle */
typedef struct
{
	int32_t x;      /**< x coordinate (from left) */
	int32_t y;      /**< y coordinate (from top) */
	int32_t width;  /**< width */
	int32_t height; /**< height */
} MMAL_RECT_T;

/** Describes a rational number */
typedef struct
{
	int32_t num;    /**< Numerator */
	int32_t den;    /**< Denominator */
} MMAL_RATIONAL_T;

/** \name Special Unknown Time Value
 * Timestamps in MMAL are defined as signed 64 bits integer values representing microseconds.
 * However a pre-defined special value is used to signal that a timestamp is not known. */
 /* @{ */
#define MMAL_TIME_UNKNOWN (INT64_C(1)<<63)  /**< Special value signalling that time is not known */
/* @} */

/** Four Character Code type */
typedef uint32_t MMAL_FOURCC_T;

/* @} */

#ifdef __cplusplus
}
#endif

#ifdef __cplusplus
extern "C" {
#endif

	/** \defgroup MmalFormat Elementary stream format
	 * Definition of an elementary stream format and its associated API */
	 /* @{ */

/** Enumeration of the different types of elementary streams.
 * This divides elementary streams into 4 big categories, plus an invalid type. */
	typedef enum {
		MMAL_ES_TYPE_UNKNOWN,     /**< Unknown elementary stream type */
		MMAL_ES_TYPE_CONTROL,     /**< Elementary stream of control commands */
		MMAL_ES_TYPE_AUDIO,       /**< Audio elementary stream */
		MMAL_ES_TYPE_VIDEO,       /**< Video elementary stream */
		MMAL_ES_TYPE_SUBPICTURE   /**< Sub-picture elementary stream (e.g. subtitles, overlays) */
	} MMAL_ES_TYPE_T;

	/** Definition of a video format.
	 * This describes the properties specific to a video stream */
	typedef struct
	{
		uint32_t        width;        /**< Width of frame in pixels */
		uint32_t        height;       /**< Height of frame in rows of pixels */
		MMAL_RECT_T     crop;         /**< Visible region of the frame */
		MMAL_RATIONAL_T frame_rate;   /**< Frame rate */
		MMAL_RATIONAL_T par;          /**< Pixel aspect ratio */

		MMAL_FOURCC_T   color_space;  /**< FourCC specifying the color space of the
										* video stream. See the \ref MmalColorSpace
										* "pre-defined color spaces" for some examples.
										*/

	} MMAL_VIDEO_FORMAT_T;

	/** Definition of an audio format.
	 * This describes the properties specific to an audio stream */
	typedef struct MMAL_AUDIO_FORMAT_T
	{
		uint32_t channels;           /**< Number of audio channels */
		uint32_t sample_rate;        /**< Sample rate */

		uint32_t bits_per_sample;    /**< Bits per sample */
		uint32_t block_align;        /**< Size of a block of data */

		/** \todo add channel mapping, gapless and replay-gain support */

	} MMAL_AUDIO_FORMAT_T;

	/** Definition of a subpicture format.
	 * This describes the properties specific to a subpicture stream */
	typedef struct
	{
		uint32_t x_offset;        /**< Width offset to the start of the subpicture */
		uint32_t y_offset;        /**< Height offset to the start of the subpicture */

		/** \todo surely more things are needed here */

	} MMAL_SUBPICTURE_FORMAT_T;

	/** Definition of the type specific format.
	 * This describes the type specific information of the elementary stream. */
	typedef union
	{
		MMAL_AUDIO_FORMAT_T      audio;      /**< Audio specific information */
		MMAL_VIDEO_FORMAT_T      video;      /**< Video specific information */
		MMAL_SUBPICTURE_FORMAT_T subpicture; /**< Subpicture specific information */
	} MMAL_ES_SPECIFIC_FORMAT_T;

	/** \name Elementary stream flags
	 * \anchor elementarystreamflags
	 * The following flags describe properties of an elementary stream */
	 /* @{ */
#define MMAL_ES_FORMAT_FLAG_FRAMED       0x1 /**< The elementary stream will already be framed */
/* @} */

/** \name Undefined encoding value.
 * This value indicates an unknown encoding
 */
 /* @{ */
#define MMAL_ENCODING_UNKNOWN            0
/* @} */

/** \name Default encoding variant value.
 * This value indicates the default encoding variant is used
 */
 /* @{ */
#define MMAL_ENCODING_VARIANT_DEFAULT    0
/* @} */

/** Definition of an elementary stream format */
	typedef struct MMAL_ES_FORMAT_T
	{
		MMAL_ES_TYPE_T type;           /**< Type of the elementary stream */

		MMAL_FOURCC_T encoding;        /**< FourCC specifying the encoding of the elementary stream.
										 * See the \ref MmalEncodings "pre-defined encodings" for some
										 * examples.
										 */
		MMAL_FOURCC_T encoding_variant;/**< FourCC specifying the specific encoding variant of
										 * the elementary stream. See the \ref MmalEncodingVariants
										 * "pre-defined encoding variants" for some examples.
										 */

		MMAL_ES_SPECIFIC_FORMAT_T *es; /**< Type specific information for the elementary stream */

		uint32_t bitrate;              /**< Bitrate in bits per second */
		uint32_t flags;                /**< Flags describing properties of the elementary stream.
										 * See \ref elementarystreamflags "Elementary stream flags".
										 */

		uint32_t extradata_size;       /**< Size of the codec specific data */
		uint8_t  *extradata;           /**< Codec specific data */

	} MMAL_ES_FORMAT_T;

	/** Allocate and initialise a \ref MMAL_ES_FORMAT_T structure.
	 *
	 * @return a \ref MMAL_ES_FORMAT_T structure
	 */
	MMAL_ES_FORMAT_T *mmal_format_alloc(void);

	/** Free a \ref MMAL_ES_FORMAT_T structure allocated by \ref mmal_format_alloc.
	 *
	 * @param format the \ref MMAL_ES_FORMAT_T structure to free
	 */
	void mmal_format_free(MMAL_ES_FORMAT_T *format);

	/** Allocate the extradata buffer in \ref MMAL_ES_FORMAT_T.
	 * This buffer will be freed automatically when the format is destroyed or
	 * another allocation is done.
	 *
	 * @param format format structure for which the extradata buffer will be allocated
	 * @param size size of the extradata buffer to allocate
	 * @return MMAL_SUCCESS on success
	 */
	MMAL_STATUS_T mmal_format_extradata_alloc(MMAL_ES_FORMAT_T *format, unsigned int size);

	/** Shallow copy a format structure.
	 * It is worth noting that the extradata buffer will not be copied in the new format.
	 *
	 * @param format_dest destination \ref MMAL_ES_FORMAT_T for the copy
	 * @param format_src source \ref MMAL_ES_FORMAT_T for the copy
	 */
	__declspec(dllexport) void mmal_format_copy(MMAL_ES_FORMAT_T *format_dest, MMAL_ES_FORMAT_T *format_src);

	/** Fully copy a format structure, including the extradata buffer.
	 *
	 * @param format_dest destination \ref MMAL_ES_FORMAT_T for the copy
	 * @param format_src source \ref MMAL_ES_FORMAT_T for the copy
	 * @return MMAL_SUCCESS on success
	 */
	MMAL_STATUS_T mmal_format_full_copy(MMAL_ES_FORMAT_T *format_dest, MMAL_ES_FORMAT_T *format_src);

	/** \name Comparison flags
	 * \anchor comparisonflags
	 * The following flags describe the differences between 2 format structures */
	 /* @{ */
#define MMAL_ES_FORMAT_COMPARE_FLAG_TYPE              0x01 /**< The type is different */
#define MMAL_ES_FORMAT_COMPARE_FLAG_ENCODING          0x02 /**< The encoding is different */
#define MMAL_ES_FORMAT_COMPARE_FLAG_BITRATE           0x04 /**< The bitrate is different */
#define MMAL_ES_FORMAT_COMPARE_FLAG_FLAGS             0x08 /**< The flags are different */
#define MMAL_ES_FORMAT_COMPARE_FLAG_EXTRADATA         0x10 /**< The extradata is different */

#define MMAL_ES_FORMAT_COMPARE_FLAG_VIDEO_RESOLUTION   0x0100 /**< The video resolution is different */
#define MMAL_ES_FORMAT_COMPARE_FLAG_VIDEO_CROPPING     0x0200 /**< The video cropping is different */
#define MMAL_ES_FORMAT_COMPARE_FLAG_VIDEO_FRAME_RATE   0x0400 /**< The video frame rate is different */
#define MMAL_ES_FORMAT_COMPARE_FLAG_VIDEO_ASPECT_RATIO 0x0800 /**< The video aspect ratio is different */
#define MMAL_ES_FORMAT_COMPARE_FLAG_VIDEO_COLOR_SPACE  0x1000 /**< The video color space is different */

#define MMAL_ES_FORMAT_COMPARE_FLAG_ES_OTHER  0x10000000 /**< Other ES specific parameters are different */
/* @} */

/** Compare 2 format structures and returns a set of flags describing the differences.
 * The result will be zero if the structures are the same, or a combination of
 * one or more of the \ref comparisonflags "Comparison flags" if different.
 *
 * @param format_1 first \ref MMAL_ES_FORMAT_T to compare
 * @param format_2 second \ref MMAL_ES_FORMAT_T to compare
 * @return set of flags describing the differences
 */
	uint32_t mmal_format_compare(MMAL_ES_FORMAT_T *format_1, MMAL_ES_FORMAT_T *format_2);

	/* @} */


/** @defgroup MMAL_PARAMETER_IDS Pre-defined MMAL parameter IDs
 * @ingroup MmalParameters
 * @{
 */

 /** @name Parameter groups
  * Parameters are divided into groups, and then allocated sequentially within
  * a group using an enum.
  * @{
  */

  /** Common parameter ID group, used with many types of component. */
#define MMAL_PARAMETER_GROUP_COMMON            (0<<16)
/** Camera-specific parameter ID group. */
#define MMAL_PARAMETER_GROUP_CAMERA            (1<<16)
/** Video-specific parameter ID group. */
#define MMAL_PARAMETER_GROUP_VIDEO             (2<<16)
/** Audio-specific parameter ID group. */
#define MMAL_PARAMETER_GROUP_AUDIO             (3<<16)
/** Clock-specific parameter ID group. */
#define MMAL_PARAMETER_GROUP_CLOCK             (4<<16)
/** Miracast-specific parameter ID group. */
#define MMAL_PARAMETER_GROUP_MIRACAST       (5<<16)


/**@}*/

/** Common MMAL parameter IDs.
 */
	enum {
		MMAL_PARAMETER_UNUSED                  /**< Never a valid parameter ID */
		= MMAL_PARAMETER_GROUP_COMMON,
		MMAL_PARAMETER_SUPPORTED_ENCODINGS,    /**< Takes a MMAL_PARAMETER_ENCODING_T */
		MMAL_PARAMETER_URI,                    /**< Takes a MMAL_PARAMETER_URI_T */
		MMAL_PARAMETER_CHANGE_EVENT_REQUEST,   /**< Takes a MMAL_PARAMETER_CHANGE_EVENT_REQUEST_T */
		MMAL_PARAMETER_ZERO_COPY,              /**< Takes a MMAL_PARAMETER_BOOLEAN_T */
		MMAL_PARAMETER_BUFFER_REQUIREMENTS,    /**< Takes a MMAL_PARAMETER_BUFFER_REQUIREMENTS_T */
		MMAL_PARAMETER_STATISTICS,             /**< Takes a MMAL_PARAMETER_STATISTICS_T */
		MMAL_PARAMETER_CORE_STATISTICS,        /**< Takes a MMAL_PARAMETER_CORE_STATISTICS_T */
		MMAL_PARAMETER_MEM_USAGE,              /**< Takes a MMAL_PARAMETER_MEM_USAGE_T */
		MMAL_PARAMETER_BUFFER_FLAG_FILTER,     /**< Takes a MMAL_PARAMETER_UINT32_T */
		MMAL_PARAMETER_SEEK,                   /**< Takes a MMAL_PARAMETER_SEEK_T */
		MMAL_PARAMETER_POWERMON_ENABLE,        /**< Takes a MMAL_PARAMETER_BOOLEAN_T */
		MMAL_PARAMETER_LOGGING,                /**< Takes a MMAL_PARAMETER_LOGGING_T */
		MMAL_PARAMETER_SYSTEM_TIME,            /**< Takes a MMAL_PARAMETER_UINT64_T */
		MMAL_PARAMETER_NO_IMAGE_PADDING,       /**< Takes a MMAL_PARAMETER_BOOLEAN_T */
		MMAL_PARAMETER_LOCKSTEP_ENABLE         /**< Takes a MMAL_PARAMETER_BOOLEAN_T */
	};


	/** Video-specific MMAL parameter IDs.
	 * @ingroup MMAL_PARAMETER_IDS
	 */
	enum {
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
	/**@}*/

	

	/** Video profiles.
	 * Only certain combinations of profile and level will be valid.
	 * @ref MMAL_VIDEO_LEVEL_T
	 */
	typedef enum MMAL_VIDEO_PROFILE_T {
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
	} MMAL_VIDEO_PROFILE_T;

	/** Video levels.
	 * Only certain combinations of profile and level will be valid.
	 * @ref MMAL_VIDEO_PROFILE_T
	 */
	typedef enum MMAL_VIDEO_LEVEL_T {
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
	} MMAL_VIDEO_LEVEL_T;

	/** Parameter header type. All parameter structures need to begin with this type.
	 * The \ref id field must be set to a parameter ID, such as one of those listed on
	 * the \ref MMAL_PARAMETER_IDS "Pre-defined MMAL parameter IDs" page.
	 */
	typedef struct MMAL_PARAMETER_HEADER_T
	{
		uint32_t id;      /**< Parameter ID. */
		uint32_t size;    /**< Size in bytes of the parameter (including the header) */
	} MMAL_PARAMETER_HEADER_T;


	/** Video profile and level setting.
	 * This is a variable length structure when querying the supported profiles and
	 * levels. To get more than one, pass a structure with more profile/level pairs.
	 */
	typedef struct MMAL_PARAMETER_VIDEO_PROFILE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		struct
		{
			MMAL_VIDEO_PROFILE_T profile;
			MMAL_VIDEO_LEVEL_T level;
		} profile[1];
	} MMAL_PARAMETER_VIDEO_PROFILE_T;


	/** Change event request parameter type.
	 * This is used to control whether a \ref MMAL_EVENT_PARAMETER_CHANGED_T event
	 * is issued should a given parameter change.
	 */
	typedef struct MMAL_PARAMETER_CHANGE_EVENT_REQUEST_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		uint32_t change_id;  /**< ID of parameter that may change, see \ref MMAL_PARAMETER_IDS */
		MMAL_BOOL_T enable;  /**< True if the event is enabled, false if disabled */
	} MMAL_PARAMETER_CHANGE_EVENT_REQUEST_T;

	/** Buffer requirements parameter.
	 * This is mainly used to increase the requirements of a component. */
	typedef struct MMAL_PARAMETER_BUFFER_REQUIREMENTS_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		uint32_t buffer_num_min;          /**< Minimum number of buffers the port requires */
		uint32_t buffer_size_min;         /**< Minimum size of buffers the port requires */
		uint32_t buffer_alignment_min;    /**< Minimum alignment requirement for the buffers.
											   A value of zero means no special alignment requirements. */
		uint32_t buffer_num_recommended;  /**< Number of buffers the port recommends for optimal performance.
											   A value of zero means no special recommendation. */
		uint32_t buffer_size_recommended; /**< Size of buffers the port recommends for optimal performance.
											   A value of zero means no special recommendation. */
	} MMAL_PARAMETER_BUFFER_REQUIREMENTS_T;

	/** Seek request parameter type.
	 * This is used to issue a seek request to a source component.
	 */
	typedef struct MMAL_PARAMETER_SEEK_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		int64_t offset;  /**< Offset (in microseconds) to seek to */
		uint32_t flags;  /**< Seeking flags */

#define MMAL_PARAM_SEEK_FLAG_PRECISE 0x1 /**< Choose precise seeking even if slower */
#define MMAL_PARAM_SEEK_FLAG_FORWARD 0x2 /**< Seek to the next keyframe following the specified offset */

	} MMAL_PARAMETER_SEEK_T;

	/** Port statistics for debugging/test purposes.
	 * Ports may support query of this parameter to return statistics for debugging or
	 * test purposes. Not all values may be relevant for a given port.
	 */
	typedef struct MMAL_PARAMETER_STATISTICS_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		uint32_t buffer_count;           /**< Total number of buffers processed */
		uint32_t frame_count;            /**< Total number of frames processed */
		uint32_t frames_skipped;         /**< Number of frames without expected PTS based on frame rate */
		uint32_t frames_discarded;       /**< Number of frames discarded */
		uint32_t eos_seen;               /**< Set if the end of stream has been reached */
		uint32_t maximum_frame_bytes;    /**< Maximum frame size in bytes */
		int64_t  total_bytes;            /**< Total number of bytes processed */
		uint32_t corrupt_macroblocks;    /**< Number of corrupt macroblocks in the stream */
	} MMAL_PARAMETER_STATISTICS_T;

	typedef enum
	{
		MMAL_CORE_STATS_RX,
		MMAL_CORE_STATS_TX,
		MMAL_CORE_STATS_MAX = 0x7fffffff /* Force 32 bit size for this enum */
	} MMAL_CORE_STATS_DIR;

	/** MMAL core statistics. These are collected by the core itself.
	 */
	//typedef struct MMAL_PARAMETER_CORE_STATISTICS_T
	//{
	//	MMAL_PARAMETER_HEADER_T hdr;
	//	MMAL_CORE_STATS_DIR dir;
	//	MMAL_BOOL_T reset;               /**< Reset to zero after reading */
	//	MMAL_CORE_STATISTICS_T stats;    /**< The statistics */
	//} MMAL_PARAMETER_CORE_STATISTICS_T;

	/**
	 * Component memory usage statistics.
	 */
	typedef struct MMAL_PARAMETER_MEM_USAGE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;
		/**< The amount of memory allocated in image pools by the component */
		uint32_t pool_mem_alloc_size;
	} MMAL_PARAMETER_MEM_USAGE_T;

	/**
	 * Logging control.
	 */
	typedef struct MMAL_PARAMETER_LOGGING_T
	{
		MMAL_PARAMETER_HEADER_T hdr;
		uint32_t set;     /**< Logging bits to set */
		uint32_t clear;   /**< Logging bits to clear */
	} MMAL_PARAMETER_LOGGING_T;

	/**/
/** \name API Version
 * The following define the version number of the API */
 /* @{ */
 /** Major version number.
  * This changes when the API breaks in a way which is not backward compatible. */
#define MMAL_VERSION_MAJOR 0
  /** Minor version number.
   * This changes each time the API is extended in a way which is still source and
   * binary compatible. */
#define MMAL_VERSION_MINOR 1

#define MMAL_VERSION (MMAL_VERSION_MAJOR << 16 | MMAL_VERSION_MINOR)
#define MMAL_VERSION_TO_MAJOR(a) (a >> 16)
#define MMAL_VERSION_TO_MINOR(a) (a & 0xFFFF)

   /** \defgroup MmalBufferHeader Buffer headers
	* Definition of a buffer header and its associated API.
	* Buffer headers are the basic element used to pass data and information between different
	* parts of the system. They are passed to components via ports and sent back to the client
	* using a callback mechanism.
	*/
	/* @{ */

	/** Specific data associated with video frames */
	typedef struct {
		uint32_t planes;     /**< Number of planes composing the video frame */
		uint32_t offset[4];  /**< Offsets to the different planes. These must point within the
								  payload buffer */
		uint32_t pitch[4];   /**< Pitch (size in bytes of a line of a plane) of the different
								  planes */
		uint32_t flags;      /**< Flags describing video specific properties of a buffer header
								  (see \ref videobufferheaderflags "Video buffer header flags") */
								  /* TBD stereoscopic support */
	} MMAL_BUFFER_HEADER_VIDEO_SPECIFIC_T;

	/** Type specific data that's associated with a payload buffer */
	typedef union
	{
		/** Specific data associated with video frames */
		MMAL_BUFFER_HEADER_VIDEO_SPECIFIC_T video;

	} MMAL_BUFFER_HEADER_TYPE_SPECIFIC_T;

	/** Definition of the buffer header structure.
	 * A buffer header does not directly carry the data to be passed to a component but instead
	 * it references the actual data using a pointer (and an associated length).
	 * It also contains an internal area which can be used to store command to be associated
	 * with the external data.
	 */
	typedef struct MMAL_BUFFER_HEADER_T
	{
		struct MMAL_BUFFER_HEADER_T *next; /**< Used to link several buffer headers together */

		struct MMAL_BUFFER_HEADER_PRIVATE_T *priv; /**< Data private to the framework */

		uint32_t cmd;              /**< Defines what the buffer header contains. This is a FourCC
										with 0 as a special value meaning stream data */

		uint8_t  *data;            /**< Pointer to the start of the payload buffer (should not be
										changed by component) */
		uint32_t alloc_size;       /**< Allocated size in bytes of payload buffer */
		uint32_t length;           /**< Number of bytes currently used in the payload buffer (starting
										from offset) */
		uint32_t offset;           /**< Offset in bytes to the start of valid data in the payload buffer */

		uint32_t flags;            /**< Flags describing properties of a buffer header (see
										\ref bufferheaderflags "Buffer header flags") */

		int64_t  pts;              /**< Presentation timestamp in microseconds. \ref MMAL_TIME_UNKNOWN
										is used when the pts is unknown. */
		int64_t  dts;              /**< Decode timestamp in microseconds (dts = pts, except in the case
										of video streams with B frames). \ref MMAL_TIME_UNKNOWN
										is used when the dts is unknown. */

										/** Type specific data that's associated with a payload buffer */
		MMAL_BUFFER_HEADER_TYPE_SPECIFIC_T *type;

		void *user_data;           /**< Field reserved for use by the client */

	} MMAL_BUFFER_HEADER_T;

	/** \name Buffer header flags
	 * \anchor bufferheaderflags
	 * The following flags describe properties of a buffer header */
	 /* @{ */
	 /** Signals that the current payload is the end of the stream of data */
#define MMAL_BUFFER_HEADER_FLAG_EOS                    (1<<0)
/** Signals that the start of the current payload starts a frame */
#define MMAL_BUFFER_HEADER_FLAG_FRAME_START            (1<<1)
/** Signals that the end of the current payload ends a frame */
#define MMAL_BUFFER_HEADER_FLAG_FRAME_END              (1<<2)
/** Signals that the current payload contains only complete frames (1 or more) */
#define MMAL_BUFFER_HEADER_FLAG_FRAME                  (MMAL_BUFFER_HEADER_FLAG_FRAME_START|MMAL_BUFFER_HEADER_FLAG_FRAME_END)
/** Signals that the current payload is a keyframe (i.e. self decodable) */
#define MMAL_BUFFER_HEADER_FLAG_KEYFRAME               (1<<3)
/** Signals a discontinuity in the stream of data (e.g. after a seek).
 * Can be used for instance by a decoder to reset its state */
#define MMAL_BUFFER_HEADER_FLAG_DISCONTINUITY          (1<<4)
 /** Signals a buffer containing some kind of config data for the component
  * (e.g. codec config data) */
#define MMAL_BUFFER_HEADER_FLAG_CONFIG                 (1<<5)
  /** Signals an encrypted payload */
#define MMAL_BUFFER_HEADER_FLAG_ENCRYPTED              (1<<6)
/** Signals a buffer containing side information */
#define MMAL_BUFFER_HEADER_FLAG_CODECSIDEINFO          (1<<7)
/** Signals a buffer which is the snapshot/postview image from a stills capture */
#define MMAL_BUFFER_HEADER_FLAGS_SNAPSHOT              (1<<8)
/** Signals a buffer which contains data known to be corrupted */
#define MMAL_BUFFER_HEADER_FLAG_CORRUPTED              (1<<9)
/** Signals that a buffer failed to be transmitted */
#define MMAL_BUFFER_HEADER_FLAG_TRANSMISSION_FAILED    (1<<10)
/** Signals the output buffer won't be used, just update reference frames */
#define MMAL_BUFFER_HEADER_FLAG_DECODEONLY             (1<<11)
/** Signals that the end of the current payload ends a NAL */
#define MMAL_BUFFER_HEADER_FLAG_NAL_END                (1<<12)
/** User flags - can be passed in and will get returned */
#define MMAL_BUFFER_HEADER_FLAG_USER0                  (1<<28)
#define MMAL_BUFFER_HEADER_FLAG_USER1                  (1<<29)
#define MMAL_BUFFER_HEADER_FLAG_USER2                  (1<<30)
#define MMAL_BUFFER_HEADER_FLAG_USER3                  (1<<31)
/* @} */

/** \name Video buffer header flags
 * \anchor videobufferheaderflags
 * The following flags describe properties of a video buffer header.
 * As there is no collision with the MMAL_BUFFER_HEADER_FLAGS_ defines, these
 * flags will also be present in the MMAL_BUFFER_HEADER_T flags field.
 */
#define MMAL_BUFFER_HEADER_FLAG_FORMAT_SPECIFIC_START_BIT 16
#define MMAL_BUFFER_HEADER_FLAG_FORMAT_SPECIFIC_START (1<<MMAL_BUFFER_HEADER_FLAG_FORMAT_SPECIFIC_START_BIT)
 /* @{ */
 /** 16: Signals an interlaced video frame */
#define MMAL_BUFFER_HEADER_VIDEO_FLAG_INTERLACED       (MMAL_BUFFER_HEADER_FLAG_FORMAT_SPECIFIC_START<<0)
/** 17: Signals that the top field of the current interlaced frame should be displayed first */
#define MMAL_BUFFER_HEADER_VIDEO_FLAG_TOP_FIELD_FIRST  (MMAL_BUFFER_HEADER_FLAG_FORMAT_SPECIFIC_START<<1)
/** 19: Signals that the buffer should be displayed on external display if attached. */
#define MMAL_BUFFER_HEADER_VIDEO_FLAG_DISPLAY_EXTERNAL (MMAL_BUFFER_HEADER_FLAG_FORMAT_SPECIFIC_START<<3)
/** 20: Signals that contents of the buffer requires copy protection. */
#define MMAL_BUFFER_HEADER_VIDEO_FLAG_PROTECTED        (MMAL_BUFFER_HEADER_FLAG_FORMAT_SPECIFIC_START<<4)
/** 27-24: If non-zero it signals the video frame is encoded in column mode,
 * with a column width equal to 2^<masked value>.
 * Zero is raster order. */
#define MMAL_BUFFER_HEADER_VIDEO_FLAG_COLUMN_LOG2_SHIFT (MMAL_BUFFER_HEADER_FLAG_FORMAT_SPECIFIC_START_BIT+8)
#define MMAL_BUFFER_HEADER_VIDEO_FLAG_COLUMN_LOG2_MASK (0xF<<MMAL_BUFFER_HEADER_VIDEO_FLAG_COLUMN_LOG2_SHIFT)
 /* @} */

#ifdef __cplusplus
}
#endif

#ifdef __cplusplus
extern "C" {
#endif
/** List of port types */
typedef enum
{
	MMAL_PORT_TYPE_UNKNOWN = 0,          /**< Unknown port type */
	MMAL_PORT_TYPE_CONTROL,              /**< Control port */
	MMAL_PORT_TYPE_INPUT,                /**< Input port */
	MMAL_PORT_TYPE_OUTPUT,               /**< Output port */
	MMAL_PORT_TYPE_CLOCK,                /**< Clock port */
	MMAL_PORT_TYPE_INVALID = 0xffffffff  /**< Dummy value to force 32bit enum */

} MMAL_PORT_TYPE_T;

/** \name Port capabilities
 * \anchor portcapabilities
 * The following flags describe the capabilities advertised by a port */
 /* @{ */
 /** The port is pass-through and doesn't need buffer headers allocated */
#define MMAL_PORT_CAPABILITY_PASSTHROUGH                       0x01
/** The port wants to allocate the buffer payloads. This signals a preference that
 * payload allocation should be done on this port for efficiency reasons. */
#define MMAL_PORT_CAPABILITY_ALLOCATION                        0x02
 /** The port supports format change events. This applies to input ports and is used
  * to let the client know whether the port supports being reconfigured via a format
  * change event (i.e. without having to disable the port). */
#define MMAL_PORT_CAPABILITY_SUPPORTS_EVENT_FORMAT_CHANGE      0x04
  /* @} */

  /** Definition of a port.
   * A port is the entity that is exposed by components to receive or transmit
   * buffer headers (\ref MMAL_BUFFER_HEADER_T). A port is defined by its
   * \ref MMAL_ES_FORMAT_T.
   *
   * It may be possible to override the buffer requirements of a port by using
   * the MMAL_PARAMETER_BUFFER_REQUIREMENTS parameter.
   */

typedef struct MMAL_PORT_PRIVATE_T
{
	MMAL_BUFFER_HEADER_T* buffer;
} MMAL_PORT_PRIVATE_T;

typedef struct MMAL_PORT_T
{
	struct MMAL_PORT_PRIVATE_T *priv; /**< Private member used by the framework */
	const char *name;                 /**< Port name. Used for debugging purposes (Read Only) */

	MMAL_PORT_TYPE_T type;            /**< Type of the port (Read Only) */
	uint16_t index;                   /**< Index of the port in its type list (Read Only) */
	uint16_t index_all;               /**< Index of the port in the list of all ports (Read Only) */

	uint32_t is_enabled;              /**< Indicates whether the port is enabled or not (Read Only) */
	MMAL_ES_FORMAT_T *format;         /**< Format of the elementary stream */

	uint32_t buffer_num_min;          /**< Minimum number of buffers the port requires (Read Only).
										   This is set by the component. */
	uint32_t buffer_size_min;         /**< Minimum size of buffers the port requires (Read Only).
										   This is set by the component. */
	uint32_t buffer_alignment_min;    /**< Minimum alignment requirement for the buffers (Read Only).
										   A value of zero means no special alignment requirements.
										   This is set by the component. */
	uint32_t buffer_num_recommended;  /**< Number of buffers the port recommends for optimal performance (Read Only).
										   A value of zero means no special recommendation.
										   This is set by the component. */
	uint32_t buffer_size_recommended; /**< Size of buffers the port recommends for optimal performance (Read Only).
										   A value of zero means no special recommendation.
										   This is set by the component. */
	uint32_t buffer_num;              /**< Actual number of buffers the port will use.
										   This is set by the client. */
	uint32_t buffer_size;             /**< Actual maximum size of the buffers that will be sent
										   to the port. This is set by the client. */

	struct MMAL_COMPONENT_T *component;    /**< Component this port belongs to (Read Only) */
	struct MMAL_PORT_USERDATA_T *userdata; /**< Field reserved for use by the client */

	uint32_t capabilities;            /**< Flags describing the capabilities of a port (Read Only).
										* Bitwise combination of \ref portcapabilities "Port capabilities"
										* values.
										*/

} MMAL_PORT_T;

/** Commit format changes on a port.
 *
 * @param port The port for which format changes are to be committed.
 * @return MMAL_SUCCESS on success
 */
__declspec(dllexport) MMAL_STATUS_T mmal_port_format_commit(MMAL_PORT_T *port);

/** Definition of the callback used by a port to send a \ref MMAL_BUFFER_HEADER_T
 * back to the user.
 *
 * @param port The port sending the buffer header.
 * @param buffer The buffer header being sent.
 */
typedef void   (*__stdcall MMAL_PORT_BH_CB_T)(MMAL_PORT_T *port, MMAL_BUFFER_HEADER_T *buffer);

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
__declspec(dllexport) MMAL_STATUS_T mmal_port_enable(MMAL_PORT_T *port, MMAL_PORT_BH_CB_T cb);

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
__declspec(dllexport) MMAL_STATUS_T mmal_port_disable(MMAL_PORT_T *port);

/** Ask a port to release all the buffer headers it currently has.
 *
 * Flushing a port will ask the port to send all the buffer headers it currently has
 * to the client. Flushing is an asynchronous request and the flush call will
 * return before all the buffer headers are returned to the client.
 * It is up to the client to keep a count on the buffer headers to know when the
 * flush operation has completed.
 * It is also important to note that flushing will also reset the state of the port
 * and any processing which was buffered by the port will be lost.
 *
 * \attention Flushing a connected port behaviour TBD.
 *
 * @param port The port to flush.
 * @return MMAL_SUCCESS on success
 */
MMAL_STATUS_T mmal_port_flush(MMAL_PORT_T *port);

/** Set a parameter on a port.
 *
 * @param port The port to which the request is sent.
 * @param param The pointer to the header of the parameter to set.
 * @return MMAL_SUCCESS on success
 */
__declspec(dllexport) MMAL_STATUS_T mmal_port_parameter_set(MMAL_PORT_T *port,
	const MMAL_PARAMETER_HEADER_T *param);

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
__declspec(dllexport) MMAL_STATUS_T mmal_port_parameter_get(MMAL_PORT_T *port,
	MMAL_PARAMETER_HEADER_T *param);

/** Send a buffer header to a port.
 *
 * @param port The port to which the buffer header is to be sent.
 * @param buffer The buffer header to send.
 * @return MMAL_SUCCESS on success
 */
__declspec(dllexport) MMAL_STATUS_T mmal_port_send_buffer(MMAL_PORT_T *port, MMAL_BUFFER_HEADER_T *buffer);

/** Connect an output port to an input port.
 *
 * When connected and enabled, buffers will automatically progress from the
 * output port to the input port when they become available, and released back
 * to the output port when no longer required by the input port.
 *
 * Ports can be given either way around, but one must be an output port and
 * the other must be an input port. Neither can be connected or enabled
 * already. The format of the output port will be applied to the input port
 * on connection.
 *
 * @param port One of the ports to connect.
 * @param other_port The other port to connect.
 * @return MMAL_SUCCESS on success.
 */
MMAL_STATUS_T mmal_port_connect(MMAL_PORT_T *port, MMAL_PORT_T *other_port);

/** Disconnect a connected port.
 *
 * If the port is not connected, an error will be returned. Otherwise, if the
 * ports are enabled, they will be disabled and any buffer pool created will be
 * freed.
 *
 * @param port The ports to disconnect.
 * @return MMAL_SUCCESS on success.
 */
MMAL_STATUS_T mmal_port_disconnect(MMAL_PORT_T *port);

/** Allocate a payload buffer.
 * This allows a client to allocate memory for a payload buffer based on the preferences
 * of a port. This for instance will allow the port to allocate memory which can be shared
 * between the host processor and videocore.
 *
 * See \ref mmal_pool_create_with_allocator().
 *
 * @param port         Port responsible for allocating the memory.
 * @param payload_size Size of the payload buffer which will be allocated.
 *
 * @return Pointer to the allocated memory.
 */
uint8_t *mmal_port_payload_alloc(MMAL_PORT_T *port, uint32_t payload_size);

/** Free a payload buffer.
 * This allows a client to free memory allocated by a previous call to \ref mmal_port_payload_alloc.
 *
 * See \ref mmal_pool_create_with_allocator().
 *
 * @param port         Port responsible for allocating the memory.
 * @param payload      Pointer to the memory to free.
 */
void mmal_port_payload_free(MMAL_PORT_T *port, uint8_t *payload);

/** Get an empty event buffer header from a port
 *
 * @param port The port from which to get the event buffer header.
 * @param buffer The address of a buffer header pointer, which will be set on return.
 * @param event The specific event FourCC required. See the \ref MmalEvents "pre-defined events".
 * @return MMAL_SUCCESS on success
 */
MMAL_STATUS_T mmal_port_event_get(MMAL_PORT_T *port, MMAL_BUFFER_HEADER_T **buffer, uint32_t event);

/* @} */


 /** Acquire a buffer header.
  * Acquiring a buffer header increases a reference counter on it and makes sure that the
  * buffer header won't be recycled until all the references to it are gone.
  * This is useful for instance if a component needs to return a buffer header but still needs
  * access to it for some internal processing (e.g. reference frames in video codecs).
  *
  * @param header buffer header to acquire
  */
void mmal_buffer_header_acquire(MMAL_BUFFER_HEADER_T *header);

/** Reset a buffer header.
 * Resets all header variables to default values.
 *
 * @param header buffer header to reset
 */
void mmal_buffer_header_reset(MMAL_BUFFER_HEADER_T *header);

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
__declspec(dllexport)void mmal_buffer_header_release(MMAL_BUFFER_HEADER_T *header);

/** Continue the buffer header release process.
 * This should be called to complete buffer header recycling once all pre-release activity
 * has been completed.
 *
 * @param header buffer header to release
 */
void mmal_buffer_header_release_continue(MMAL_BUFFER_HEADER_T *header);

/** Buffer header pre-release callback.
 * The callback is invoked just before a buffer is released back into a
 * pool. This is used by clients who need to trigger additional actions
 * before the buffer can finally be released (e.g. wait for a bulk transfer
 * to complete).
 *
 * The callback should return TRUE if the buffer release need to be post-poned.
 *
 * @param header   buffer header about to be released
 * @param userdata user-specific data
 *
 * @return TRUE if the buffer should not be released
 */
typedef MMAL_BOOL_T (*MMAL_BH_PRE_RELEASE_CB_T)(MMAL_BUFFER_HEADER_T *header, void *userdata);

/** Set a buffer header pre-release callback.
 * If the callback is NULL, the buffer will be released back into the pool
 * immediately as usual.
 *
 * @param header   buffer header to associate callback with
 * @param cb       pre-release callback to invoke
 * @param userdata user-specific data
 */
void mmal_buffer_header_pre_release_cb_set(MMAL_BUFFER_HEADER_T *header, MMAL_BH_PRE_RELEASE_CB_T cb, void *userdata);

/** Replicate a buffer header into another one.
 * Replicating a buffer header will not only do an exact copy of all the public fields of the
 * buffer header (including data and alloc_size), but it will also acquire a reference to the
 * source buffer header which will only be released once the replicate has been released.
 *
 * @param dest buffer header into which to replicate
 * @param src buffer header to use as the source for the replication
 * @return MMAL_SUCCESS on success
 */
MMAL_STATUS_T mmal_buffer_header_replicate(MMAL_BUFFER_HEADER_T *dest, MMAL_BUFFER_HEADER_T *src);

/** Lock the data buffer contained in the buffer header in memory.
 * This call does nothing on all platforms except VideoCore where it is needed to pin a
 * buffer in memory before any access to it.
 *
 * @param header buffer header to lock
 */
__declspec(dllexport)MMAL_STATUS_T mmal_buffer_header_mem_lock(MMAL_BUFFER_HEADER_T *header);

/** Unlock the data buffer contained in the buffer header.
 * This call does nothing on all platforms except VideoCore where it is needed to un-pin a
 * buffer in memory after any access to it.
 *
 * @param header buffer header to unlock
 */
__declspec(dllexport)void mmal_buffer_header_mem_unlock(MMAL_BUFFER_HEADER_T *header);

/* @} */

#ifdef __cplusplus
}
#endif

#ifdef __cplusplus
extern "C" {
#endif

	/** \defgroup MmalQueue Queues of buffer headers
	 * This provides a thread-safe implementation of a queue of buffer headers
	 * (\ref MMAL_BUFFER_HEADER_T). The queue works in a first-in, first-out basis
	 * so the buffer headers will be dequeued in the order they have been queued. */
	 /* @{ */



	typedef struct MMAL_QUEUE_T
	{
		int current; 
		MMAL_BUFFER_HEADER_T* buffers[1];
	} MMAL_QUEUE_T;

	/** Create a queue of MMAL_BUFFER_HEADER_T
	 *
	 * @return Pointer to the newly created queue or NULL on failure.
	 */
	MMAL_QUEUE_T *mmal_queue_create(void);

	/** Put a MMAL_BUFFER_HEADER_T into a queue
	 *
	 * @param queue  Pointer to a queue
	 * @param buffer Pointer to the MMAL_BUFFER_HEADER_T to add to the queue
	 */
	void mmal_queue_put(MMAL_QUEUE_T *queue, MMAL_BUFFER_HEADER_T *buffer);

	/** Put a MMAL_BUFFER_HEADER_T back at the start of a queue.
	 * This is used when a buffer header was removed from the queue but not
	 * fully processed and needs to be put back where it was originally taken.
	 *
	 * @param queue  Pointer to a queue
	 * @param buffer Pointer to the MMAL_BUFFER_HEADER_T to add to the queue
	 */
	void mmal_queue_put_back(MMAL_QUEUE_T *queue, MMAL_BUFFER_HEADER_T *buffer);

	/** Get a MMAL_BUFFER_HEADER_T from a queue
	 *
	 * @param queue  Pointer to a queue
	 *
	 * @return pointer to the next MMAL_BUFFER_HEADER_T or NULL if the queue is empty.
	 */
	__declspec(dllexport) MMAL_BUFFER_HEADER_T *mmal_queue_get(MMAL_QUEUE_T *queue);

	/** Wait for a MMAL_BUFFER_HEADER_T from a queue.
	 * This is the same as a get except that this will block until a buffer header is
	 * available.
	 *
	 * @param queue  Pointer to a queue
	 *
	 * @return pointer to the next MMAL_BUFFER_HEADER_T.
	 */
	MMAL_BUFFER_HEADER_T *mmal_queue_wait(MMAL_QUEUE_T *queue);

	/** Wait for a MMAL_BUFFER_HEADER_T from a queue, up to a given timeout.
	 * This is the same as a wait, except that it will abort in case of timeout.
	 *
	 * @param queue  Pointer to a queue
	 * @param timeout Number of milliseconds to wait before
	 *                returning if the semaphore can't be acquired.
	 *
	 * @return pointer to the next MMAL_BUFFER_HEADER_T.
	 */
	 //MMAL_BUFFER_HEADER_T *mmal_queue_timedwait(MMAL_QUEUE_T *queue, VCOS_UNSIGNED timeout);

	 /** Get the number of MMAL_BUFFER_HEADER_T currently in a queue.
	  *
	  * @param queue  Pointer to a queue
	  *
	  * @return length (in elements) of the queue.
	  */
	__declspec(dllexport) unsigned int mmal_queue_length(MMAL_QUEUE_T *queue);

	/** Destroy a queue of MMAL_BUFFER_HEADER_T.
	 *
	 * @param queue  Pointer to a queue
	 */
	void mmal_queue_destroy(MMAL_QUEUE_T *queue);

	/* @} */

#ifdef __cplusplus
}
#endif

#ifdef __cplusplus
extern "C" {
#endif

	/** \defgroup MmalPool Pools of buffer headers
	 * A pool of buffer headers is composed of a queue (\ref MMAL_QUEUE_T) and a user
	 * specified number of buffer headers (\ref MMAL_BUFFER_HEADER_T). */
	 /* @{ */



/** Definition of a pool */
	typedef struct MMAL_POOL_T
	{
		MMAL_QUEUE_T *queue;             /**< Queue used by the pool */
		uint32_t headers_num;            /**< Number of buffer headers in the pool */
		MMAL_BUFFER_HEADER_T **header;   /**< Array of buffer headers belonging to the pool */
	} MMAL_POOL_T;

	/** Allocator alloc prototype
	 *
	 * @param context The context pointer passed in on pool creation.
	 * @param size    The size of the allocation required, in bytes.
	 * @return The pointer to the newly allocated memory, or NULL on failure.
	 */
	typedef void *(*mmal_pool_allocator_alloc_t)(void *context, uint32_t size);
	/** Allocator free prototype
	 *
	 * @param context The context pointer passed in on pool creation.
	 * @param mem     The pointer to the memory to be released.
	 */
	typedef void(*mmal_pool_allocator_free_t)(void *context, void *mem);

	/** Create a pool of MMAL_BUFFER_HEADER_T.
	 * After allocation, all allocated buffer headers will have been added to the queue.
	 *
	 * It is valid to create a pool with no buffer headers, or with zero size payload buffers.
	 * The mmal_pool_resize() function can be used to increase or decrease the number of buffer
	 * headers, or the size of the payload buffers, after creation of the pool.
	 *
	 * The payload buffers may also be allocated independently by the client, and assigned
	 * to the buffer headers, but it will be the responsibility of the client to deal with
	 * resizing and releasing the memory. It is recommended that mmal_pool_create_with_allocator()
	 * is used in this case, supplying allocator function pointers that will be used as
	 * necessary by MMAL.
	 *
	 * @param headers      Number of buffer headers to be allocated with the pool.
	 * @param payload_size Size of the payload buffer that will be allocated in
	 *                     each of the buffer headers.
	 * @return Pointer to the newly created pool or NULL on failure.
	 */
	MMAL_POOL_T *mmal_pool_create(unsigned int headers, uint32_t payload_size);

	/** Create a pool of MMAL_BUFFER_HEADER_T.
	 * After allocation, all allocated buffer headers will have been added to the queue.
	 *
	 * It is valid to create a pool with no buffer headers, or with zero size payload buffers.
	 * The mmal_pool_resize() function can be used to increase or decrease the number of buffer
	 * headers, or the size of the payload buffers, after creation of the pool. The allocators
	 * passed during creation shall be used when resizing the payload buffers.
	 *
	 * @param headers      Number of buffer headers to be allocated with the pool.
	 * @param payload_size Size of the payload buffer that will be allocated in
	 *                     each of the buffer headers.
	 * @param allocator_context Pointer to the context of the allocator.
	 * @param allocator_alloc   Function pointer for the alloc call of the allocator.
	 * @param allocator_free    Function pointer for the free call of the allocator.
	 *
	 * @return Pointer to the newly created pool or NULL on failure.
	 */
	MMAL_POOL_T *mmal_pool_create_with_allocator(unsigned int headers, uint32_t payload_size,
		void *allocator_context, mmal_pool_allocator_alloc_t allocator_alloc,
		mmal_pool_allocator_free_t allocator_free);

	/** Destroy a pool of MMAL_BUFFER_HEADER_T.
	 * This will also deallocate all of the memory which was allocated when creating or
	 * resizing the pool.
	 *
	 * If payload buffers have been allocated independently by the client, they should be
	 * released prior to calling this function. If the client provided allocator functions,
	 * the allocator_free function shall be called for each payload buffer.
	 *
	 * @param pool  Pointer to a pool
	 */
	__declspec(dllexport) void mmal_pool_destroy(MMAL_POOL_T *pool);

	/** Resize a pool of MMAL_BUFFER_HEADER_T.
	 * This allows modifying either the number of allocated buffers, the payload size or both at the
	 * same time.
	 *
	 * @param pool         Pointer to the pool
	 * @param headers      New number of buffer headers to be allocated in the pool.
	 *                     It is not valid to pass zero for the number of buffers.
	 * @param payload_size Size of the payload buffer that will be allocated in
	 *                     each of the buffer headers.
	 *                     If this is set to 0, all payload buffers shall be released.
	 * @return MMAL_SUCCESS or an error on failure.
	 */
	MMAL_STATUS_T mmal_pool_resize(MMAL_POOL_T *pool, unsigned int headers, uint32_t payload_size);

	/** Definition of the callback used by a pool to signal back to the user that a buffer header
	 * has been released back to the pool.
	 *
	 * @param pool       Pointer to the pool
	 * @param buffer     Buffer header just released
	 * @param userdata   User specific data passed in when setting the callback
	 * @return True to have the buffer header put back in the pool's queue, false if the buffer
	 *          header has been taken within the callback.
	 */
	typedef MMAL_BOOL_T(*MMAL_POOL_BH_CB_T)(MMAL_POOL_T *pool, MMAL_BUFFER_HEADER_T *buffer, void *userdata);

	/** Set a buffer header release callback to the pool.
	 * Each time a buffer header is released to the pool, the callback will be triggered.
	 *
	 * @param pool     Pointer to a pool
	 * @param cb       Callback function
	 * @param userdata User specific data which will be passed with each callback
	 */
	void mmal_pool_callback_set(MMAL_POOL_T *pool, MMAL_POOL_BH_CB_T cb, void *userdata);

	/** Set a pre-release callback for all buffer headers in the pool.
	 * Each time a buffer header is about to be released to the pool, the callback
	 * will be triggered.
	 *
	 * @param pool     Pointer to the pool
	 * @param cb       Pre-release callback function
	 * @param userdata User-specific data passed back with each callback
	 */
	void mmal_pool_pre_release_callback_set(MMAL_POOL_T *pool, MMAL_BH_PRE_RELEASE_CB_T cb, void *userdata);

	/* @} */

#ifdef __cplusplus
}
#endif
/** Offset in bytes of FIELD in TYPE. */
#define MMAL_OFFSET(TYPE, FIELD) ((size_t)((uint8_t *)&((TYPE*)0)->FIELD - (uint8_t *)0))

#ifdef __cplusplus
extern "C" {
#endif

	/** Convert a status to a statically-allocated string.
	 *
	 * @param status The MMAL status code.
	 * @return A C string describing the status code.
	 */
	const char *mmal_status_to_string(MMAL_STATUS_T status);

	/** Convert stride to pixel width for a given pixel encoding.
	 *
	 * @param encoding The pixel encoding (such as one of the \ref MmalEncodings "pre-defined encodings")
	 * @param stride The stride in bytes.
	 * @return The width in pixels.
	 */
	uint32_t mmal_encoding_stride_to_width(uint32_t encoding, uint32_t stride);

	/** Convert pixel width to stride for a given pixel encoding
	 *
	 * @param encoding The pixel encoding (such as one of the \ref MmalEncodings "pre-defined encodings")
	 * @param width The width in pixels.
	 * @return The stride in bytes.
	 */
	uint32_t mmal_encoding_width_to_stride(uint32_t encoding, uint32_t width);

	/** Return the 16 line high sliced version of a given pixel encoding
	 *
	 * @param encoding The pixel encoding (such as one of the \ref MmalEncodings "pre-defined encodings")
	 * @return The sliced equivalent, or MMAL_ENCODING_UNKNOWN if not supported.
	 */
	uint32_t mmal_encoding_get_slice_variant(uint32_t encoding);

	/** Convert a port type to a string.
	 *
	 * @param type The MMAL port type.
	 * @return A NULL-terminated string describing the port type.
	 */
	const char* mmal_port_type_to_string(MMAL_PORT_TYPE_T type);

	/** Get a parameter from a port allocating the required amount of memory
	 * for the parameter (i.e. for variable length parameters like URI or arrays).
	 * The size field will be set on output to the actual size of the
	 * parameter allocated and retrieved.
	 *
	 * The pointer returned must be released by a call to \ref mmal_port_parameter_free().
	 *
	 * @param port port to send request to
	 * @param id parameter id
	 * @param size initial size hint for allocation (can be 0)
	 * @param status status of the parameter get operation (can be 0)
	 * @return pointer to the header of the parameter or NULL on failure.
	 */
	MMAL_PARAMETER_HEADER_T *mmal_port_parameter_alloc_get(MMAL_PORT_T *port,
		uint32_t id, uint32_t size, MMAL_STATUS_T *status);

	/** Free a parameter structure previously allocated via
	 * \ref mmal_port_parameter_alloc_get().
	 *
	 * @param param pointer to header of the parameter
	 */
	void mmal_port_parameter_free(MMAL_PARAMETER_HEADER_T *param);

	/** Copy buffer header metadata from source to destination.
	 *
	 * @param dest The destination buffer header.
	 * @param src  The source buffer header.
	 */
	void mmal_buffer_header_copy_header(MMAL_BUFFER_HEADER_T *dest, const MMAL_BUFFER_HEADER_T *src);

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
	__declspec(dllexport) MMAL_POOL_T *mmal_port_pool_create(MMAL_PORT_T *port,
		unsigned int headers, uint32_t payload_size);

	/** Destroy a pool of MMAL_BUFFER_HEADER_T associated with a specific port.
	 * This will also deallocate all of the memory which was allocated when creating or
	 * resizing the pool.
	 *
	 * @param port  Pointer to the port responsible for creating the pool.
	 * @param pool  Pointer to the pool to be destroyed.
	 */
	__declspec(dllexport) void mmal_port_pool_destroy(MMAL_PORT_T *port, MMAL_POOL_T *pool);

	/** Log the content of a \ref MMAL_PORT_T structure.
	 *
	 * @param port  Pointer to the port to dump.
	 */
	void mmal_log_dump_port(MMAL_PORT_T *port);

	/** Log the content of a \ref MMAL_ES_FORMAT_T structure.
	 *
	 * @param format  Pointer to the format to dump.
	 */
	void mmal_log_dump_format(MMAL_ES_FORMAT_T *format);

	/** Return the nth port.
	 *
	 * @param comp   component to query
	 * @param index  port index
	 * @param type   port type
	 *
	 * @return port or NULL if not found
	 */
	//MMAL_PORT_T *mmal_util_get_port(MMAL_COMPONENT_T *comp, MMAL_PORT_TYPE_T type, unsigned index);

	/** Convert a 4cc into a string.
	 *
	 * @param buf    Destination for result
	 * @param len    Size of result buffer
	 * @param fourcc 4cc to be converted
	 * @return converted string (buf)
	 *
	 */
	char *mmal_4cc_to_string(char *buf, size_t len, uint32_t fourcc);


	/** On FW prior to June 2016, camera and video_splitter
	 *  had BGR24 and RGB24 support reversed.
	 *  This is now fixed, and this function will return whether the
	 *  FW has the fix or not.
	 *
	 * @param port   MMAL port to check (on camera or video_splitter)
	 * @return 0 if old firmware, 1 if new.
	 *
	 */
	int mmal_util_rgb_order_fixed(MMAL_PORT_T *port);

#ifdef __cplusplus
}
#endif

#ifdef __cplusplus
extern "C" {
#endif

	/**
	 * @file
	 * Utility functions to set some common parameters.
	 */

	 /** Helper function to set the value of a boolean parameter.
	  * @param port   port on which to set the parameter
	  * @param id     parameter id
	  * @param value  value to set the parameter to
	  *
	  * @return MMAL_SUCCESS or error
	  */
	__declspec(dllexport) MMAL_STATUS_T mmal_port_parameter_set_boolean(MMAL_PORT_T *port, uint32_t id, MMAL_BOOL_T value);

	/** Helper function to get the value of a boolean parameter.
	 * @param port   port on which to get the parameter
	 * @param id     parameter id
	 * @param value  pointer to where the value will be returned
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_get_boolean(MMAL_PORT_T *port, uint32_t id, MMAL_BOOL_T *value);

	/** Helper function to set the value of a 64 bits unsigned integer parameter.
	 * @param port   port on which to set the parameter
	 * @param id     parameter id
	 * @param value  value to set the parameter to
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_set_uint64(MMAL_PORT_T *port, uint32_t id, uint64_t value);

	/** Helper function to get the value of a 64 bits unsigned integer parameter.
	 * @param port   port on which to get the parameter
	 * @param id     parameter id
	 * @param value  pointer to where the value will be returned
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_get_uint64(MMAL_PORT_T *port, uint32_t id, uint64_t *value);

	/** Helper function to set the value of a 64 bits signed integer parameter.
	 * @param port   port on which to set the parameter
	 * @param id     parameter id
	 * @param value  value to set the parameter to
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_set_int64(MMAL_PORT_T *port, uint32_t id, int64_t value);

	/** Helper function to get the value of a 64 bits signed integer parameter.
	 * @param port   port on which to get the parameter
	 * @param id     parameter id
	 * @param value  pointer to where the value will be returned
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_get_int64(MMAL_PORT_T *port, uint32_t id, int64_t *value);

	/** Helper function to set the value of a 32 bits unsigned integer parameter.
	 * @param port   port on which to set the parameter
	 * @param id     parameter id
	 * @param value  value to set the parameter to
	 *
	 * @return MMAL_SUCCESS or error
	 */
	__declspec(dllexport) MMAL_STATUS_T mmal_port_parameter_set_uint32(MMAL_PORT_T *port, uint32_t id, uint32_t value);

	/** Helper function to get the value of a 32 bits unsigned integer parameter.
	 * @param port   port on which to get the parameter
	 * @param id     parameter id
	 * @param value  pointer to where the value will be returned
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_get_uint32(MMAL_PORT_T *port, uint32_t id, uint32_t *value);

	/** Helper function to set the value of a 32 bits signed integer parameter.
	 * @param port   port on which to set the parameter
	 * @param id     parameter id
	 * @param value  value to set the parameter to
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_set_int32(MMAL_PORT_T *port, uint32_t id, int32_t value);

	/** Helper function to get the value of a 32 bits signed integer parameter.
	 * @param port   port on which to get the parameter
	 * @param id     parameter id
	 * @param value  pointer to where the value will be returned
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_get_int32(MMAL_PORT_T *port, uint32_t id, int32_t *value);

	/** Helper function to set the value of a rational parameter.
	 * @param port   port on which to set the parameter
	 * @param id     parameter id
	 * @param value  value to set the parameter to
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_set_rational(MMAL_PORT_T *port, uint32_t id, MMAL_RATIONAL_T value);

	/** Helper function to get the value of a rational parameter.
	 * @param port   port on which to get the parameter
	 * @param id     parameter id
	 * @param value  pointer to where the value will be returned
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_get_rational(MMAL_PORT_T *port, uint32_t id, MMAL_RATIONAL_T *value);

	/** Helper function to set the value of a string parameter.
	 * @param port   port on which to set the parameter
	 * @param id     parameter id
	 * @param value  null-terminated string value
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_set_string(MMAL_PORT_T *port, uint32_t id, const char *value);

	/** Helper function to set the value of an array of bytes parameter.
	 * @param port   port on which to set the parameter
	 * @param id     parameter id
	 * @param data   pointer to the array of bytes
	 * @param size   size of the array of bytes
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_port_parameter_set_bytes(MMAL_PORT_T *port, uint32_t id,
		const uint8_t *data, unsigned int size);

	/** Helper function to set a MMAL_PARAMETER_URI_T parameter on a port.
	 * @param port   port on which to set the parameter
	 * @param uri    URI string
	 *
	 * @return MMAL_SUCCESS or error
	 */
	MMAL_STATUS_T mmal_util_port_set_uri(MMAL_PORT_T *port, const char *uri);

	/** Set the display region.
	 * @param port   port to configure
	 * @param region region
	 *
	 * @return MMAL_SUCCESS or error
	 */
	/*MMAL_STATUS_T mmal_util_set_display_region(MMAL_PORT_T *port,
		MMAL_DISPLAYREGION_T *region);*/

	/** Tell the camera to use the STC for timestamps rather than the clock.
	 *
	 * @param port   port to configure
	 * @param mode   STC mode to use
	 * @return MMAL_SUCCESS or error
	 */
	//MMAL_STATUS_T mmal_util_camera_use_stc_timestamp(MMAL_PORT_T *port, MMAL_CAMERA_STC_MODE_T mode);

	/** Get the MMAL core statistics for a given port.
	 *
	 * @param port  port to query
	 * @param dir   port direction
	 * @param reset reset the stats as well
	 * @param stats filled in with results
	 * @return MMAL_SUCCESS or error
	 */
	/*MMAL_STATUS_T mmal_util_get_core_port_stats(MMAL_PORT_T *port, MMAL_CORE_STATS_DIR dir, MMAL_BOOL_T reset,
		MMAL_CORE_STATISTICS_T *stats);*/

#ifdef __cplusplus
}
#endif


#define MMAL_COMPONENT_DEFAULT_CONTAINER_READER "container_reader"
#define MMAL_COMPONENT_DEFAULT_CONTAINER_WRITER "container_writer"

#if defined(ENABLE_MMAL_STANDALONE)
# define MMAL_COMPONENT_DEFAULT_VIDEO_DECODER    "avcodec.video_decode"
# define MMAL_COMPONENT_DEFAULT_VIDEO_ENCODER    "avcodec.video_encode"
# define MMAL_COMPONENT_DEFAULT_VIDEO_RENDERER   "sdl.video_render"
# define MMAL_COMPONENT_DEFAULT_IMAGE_DECODER    "avcodec.video_decode"
# define MMAL_COMPONENT_DEFAULT_IMAGE_ENCODER    "avcodec.video_encode"
# define MMAL_COMPONENT_DEFAULT_CAMERA           "artificial_camera"
# define MMAL_COMPONENT_DEFAULT_VIDEO_CONVERTER  "avcodec.video_convert"
# define MMAL_COMPONENT_DEFAULT_SPLITTER         "splitter"
# define MMAL_COMPONENT_DEFAULT_SCHEDULER        "scheduler"
# define MMAL_COMPONENT_DEFAULT_VIDEO_INJECTER   "video_inject"
# define MMAL_COMPONENT_DEFAULT_AUDIO_DECODER    "avcodec.audio_decode"
# define MMAL_COMPONENT_DEFAULT_AUDIO_RENDERER   "sdl.audio_render"
# define MMAL_COMPONENT_DEFAULT_MIRACAST         "miracast"
# define MMAL_COMPONENT_DEFAULT_CLOCK            "clock"
#elif defined(__VIDEOCORE__)
# define MMAL_COMPONENT_DEFAULT_VIDEO_DECODER    "ril.video_decode"
# define MMAL_COMPONENT_DEFAULT_VIDEO_ENCODER    "ril.video_encode"
# define MMAL_COMPONENT_DEFAULT_VIDEO_RENDERER   "ril.video_render"
# define MMAL_COMPONENT_DEFAULT_IMAGE_DECODER    "ril.image_decode"
# define MMAL_COMPONENT_DEFAULT_IMAGE_ENCODER    "ril.image_encode"
# define MMAL_COMPONENT_DEFAULT_CAMERA           "ril.camera"
# define MMAL_COMPONENT_DEFAULT_VIDEO_CONVERTER  "video_convert"
# define MMAL_COMPONENT_DEFAULT_SPLITTER         "splitter"
# define MMAL_COMPONENT_DEFAULT_SCHEDULER        "scheduler"
# define MMAL_COMPONENT_DEFAULT_VIDEO_INJECTER   "video_inject"
# define MMAL_COMPONENT_DEFAULT_VIDEO_SPLITTER   "ril.video_splitter"
# define MMAL_COMPONENT_DEFAULT_AUDIO_DECODER    "none"
# define MMAL_COMPONENT_DEFAULT_AUDIO_RENDERER   "ril.audio_render"
# define MMAL_COMPONENT_DEFAULT_MIRACAST         "miracast"
# define MMAL_COMPONENT_DEFAULT_CLOCK            "clock"
# define MMAL_COMPONENT_DEFAULT_CAMERA_INFO      "camera_info"
#else
# define MMAL_COMPONENT_DEFAULT_VIDEO_DECODER    "vc.ril.video_decode"
# define MMAL_COMPONENT_DEFAULT_VIDEO_ENCODER    "vc.ril.video_encode"
# define MMAL_COMPONENT_DEFAULT_VIDEO_RENDERER   "vc.ril.video_render"
# define MMAL_COMPONENT_DEFAULT_IMAGE_DECODER    "vc.ril.image_decode"
# define MMAL_COMPONENT_DEFAULT_IMAGE_ENCODER    "vc.ril.image_encode"
# define MMAL_COMPONENT_DEFAULT_CAMERA           "vc.ril.camera"
# define MMAL_COMPONENT_DEFAULT_VIDEO_CONVERTER  "vc.video_convert"
# define MMAL_COMPONENT_DEFAULT_SPLITTER         "vc.splitter"
# define MMAL_COMPONENT_DEFAULT_SCHEDULER        "vc.scheduler"
# define MMAL_COMPONENT_DEFAULT_VIDEO_INJECTER   "vc.video_inject"
# define MMAL_COMPONENT_DEFAULT_VIDEO_SPLITTER   "vc.ril.video_splitter"
# define MMAL_COMPONENT_DEFAULT_AUDIO_DECODER    "none"
# define MMAL_COMPONENT_DEFAULT_AUDIO_RENDERER   "vc.ril.audio_render"
# define MMAL_COMPONENT_DEFAULT_MIRACAST         "vc.miracast"
# define MMAL_COMPONENT_DEFAULT_CLOCK            "vc.clock"
# define MMAL_COMPONENT_DEFAULT_CAMERA_INFO      "vc.camera_info"
#endif


#ifdef __cplusplus
extern "C" {
#endif

	/** \name Connection flags
	 * \anchor connectionflags
	 * The following flags describe the properties of the connection. */
	 /* @{ */
	 /** The connection is tunnelled. Buffer headers do not transit via the client but
	  * directly from the output port to the input port. */
#define MMAL_CONNECTION_FLAG_TUNNELLING 0x1
	  /** Force the pool of buffer headers used by the connection to be allocated on the input port. */
#define MMAL_CONNECTION_FLAG_ALLOCATION_ON_INPUT 0x2
/** Force the pool of buffer headers used by the connection to be allocated on the output port. */
#define MMAL_CONNECTION_FLAG_ALLOCATION_ON_OUTPUT 0x4
/** Specify that the connection should not modify the buffer requirements. */
#define MMAL_CONNECTION_FLAG_KEEP_BUFFER_REQUIREMENTS 0x8
/** The connection is flagged as direct. This doesn't change the behaviour of
 * the connection itself but is used by the the graph utility to specify that
 * the buffer should be sent to the input port from with the port callback. */
#define MMAL_CONNECTION_FLAG_DIRECT 0x10
 /** Specify that the connection should not modify the port formats. */
#define MMAL_CONNECTION_FLAG_KEEP_PORT_FORMATS 0x20
/* @} */

/** Forward type definition for a connection */
	typedef struct MMAL_CONNECTION_T MMAL_CONNECTION_T;

	/** Definition of the callback used by a connection to signal back to the client
	 * that a buffer header is available either in the pool or in the output queue.
	 *
	 * @param connection Pointer to the connection
	 */
	typedef void(*MMAL_CONNECTION_CALLBACK_T)(MMAL_CONNECTION_T *connection);

	/** Structure describing a connection between 2 ports (1 output and 1 input port) */
	struct MMAL_CONNECTION_T {

		void *user_data;           /**< Field reserved for use by the client. */
		MMAL_CONNECTION_CALLBACK_T callback; /**< Callback set by the client. */

		uint32_t is_enabled;       /**< Specifies whether the connection is enabled or not (Read Only). */

		uint32_t flags;            /**< Flags passed during the create call (Read Only). A bitwise
									* combination of \ref connectionflags "Connection flags" values.
									*/
		MMAL_PORT_T *in;           /**< Input port used for the connection (Read Only). */
		MMAL_PORT_T *out;          /**< Output port used for the connection (Read Only). */

		MMAL_POOL_T *pool;         /**< Pool of buffer headers used by the output port (Read Only). */
		MMAL_QUEUE_T *queue;       /**< Queue for the buffer headers produced by the output port (Read Only). */

		const char *name;          /**< Connection name (Read Only). Used for debugging purposes. */

		/* Used for debug / statistics */
		int64_t time_setup;        /**< Time in microseconds taken to setup the connection. */
		int64_t time_enable;       /**< Time in microseconds taken to enable the connection. */
		int64_t time_disable;      /**< Time in microseconds taken to disable the connection. */
	};

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
	__declspec(dllexport) MMAL_STATUS_T mmal_connection_create(MMAL_CONNECTION_T **connection,
		MMAL_PORT_T *out, MMAL_PORT_T *in, uint32_t flags);

	/** Acquire a reference on a connection.
	 * Acquiring a reference on a connection will prevent a connection from being destroyed until
	 * the acquired reference is released (by a call to \ref mmal_connection_destroy).
	 * References are internally counted so all acquired references need a matching call to
	 * release them.
	 *
	 * @param connection connection to acquire
	 */
	void mmal_connection_acquire(MMAL_CONNECTION_T *connection);

	/** Release a reference on a connection
	 * Release an acquired reference on a connection. Triggers the destruction of the connection when
	 * the last reference is being released.
	 * \note This is in fact an alias of \ref mmal_connection_destroy which is added to make client
	 * code clearer.
	 *
	 * @param connection connection to release
	 * @return MMAL_SUCCESS on success
	 */
	MMAL_STATUS_T mmal_connection_release(MMAL_CONNECTION_T *connection);

	/** Destroy a connection.
	 * Release an acquired reference on a connection. Only actually destroys the connection when
	 * the last reference is being released.
	 * The actual destruction of the connection will start by disabling it, if necessary.
	 * Any pool, queue, and so on owned by the connection shall then be destroyed.
	 *
	 * @param connection The connection to be destroyed.
	 * @return MMAL_SUCCESS on success.
	 */
	__declspec(dllexport) MMAL_STATUS_T mmal_connection_destroy(MMAL_CONNECTION_T *connection);

	/** Enable a connection.
	 * The format of the two ports must have been committed before calling this function,
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
	__declspec(dllexport) MMAL_STATUS_T mmal_connection_enable(MMAL_CONNECTION_T *connection);

	/** Disable a connection.
	 *
	 * @param connection The connection to be disabled.
	 * @return MMAL_SUCCESS on success.
	 */
	__declspec(dllexport) MMAL_STATUS_T mmal_connection_disable(MMAL_CONNECTION_T *connection);

	/** Apply a format changed event to the connection.
	 * This function can be used when the client is processing buffer headers and receives
	 * a format changed event (\ref MMAL_EVENT_FORMAT_CHANGED). The connection is
	 * reconfigured, changing the format of the ports, the number of buffer headers and
	 * the size of the payload buffers as necessary.
	 *
	 * @param connection The connection to which the event shall be applied.
	 * @param buffer The buffer containing a format changed event.
	 * @return MMAL_SUCCESS on success.
	 */
	MMAL_STATUS_T mmal_connection_event_format_changed(MMAL_CONNECTION_T *connection,
		MMAL_BUFFER_HEADER_T *buffer);

#ifdef __cplusplus
}
#endif


#ifdef __cplusplus
extern "C" {
#endif

	/** \defgroup MmalComponent Components
	 * Definition of a MMAL component and its associated API. A component will always expose ports
	 * which it uses to send and receive data in the form of buffer headers
	 * (\ref MMAL_BUFFER_HEADER_T) */
	 /* @{ */

	struct MMAL_COMPONENT_PRIVATE_T;
	typedef struct MMAL_COMPONENT_PRIVATE_T MMAL_COMPONENT_PRIVATE_T;

	/** Definition of a component. */
	typedef struct MMAL_COMPONENT_T
	{
		/** Pointer to the private data of the module in use */
		struct MMAL_COMPONENT_PRIVATE_T *priv;

		/** Pointer to private data of the client */
		struct MMAL_COMPONENT_USERDATA_T *userdata;

		/** Component name */
		const char *name;

		/** Specifies whether the component is enabled or not */
		uint32_t is_enabled;

		/** All components expose a control port.
		 * The control port is used by clients to set / get parameters that are global to the
		 * component. It is also used to receive events, which again are global to the component.
		 * To be able to receive events, the client needs to enable and register a callback on the
		 * control port. */
		MMAL_PORT_T *control;

		uint32_t    input_num;   /**< Number of input ports */
		MMAL_PORT_T **input;     /**< Array of input ports */

		uint32_t    output_num;  /**< Number of output ports */
		MMAL_PORT_T **output;    /**< Array of output ports */

		uint32_t    clock_num;   /**< Number of clock ports */
		MMAL_PORT_T **clock;     /**< Array of clock ports */

		uint32_t    port_num;    /**< Total number of ports */
		MMAL_PORT_T **port;      /**< Array of all the ports (control/input/output/clock) */

		/** Uniquely identifies the component's instance within the MMAL
		 * context / process. For debugging. */
		uint32_t id;

	} MMAL_COMPONENT_T;

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
	__declspec(dllexport) MMAL_STATUS_T mmal_component_create(const char *name,
		MMAL_COMPONENT_T **component);

	/** Acquire a reference on a component.
	 * Acquiring a reference on a component will prevent a component from being destroyed until
	 * the acquired reference is released (by a call to \ref mmal_component_destroy).
	 * References are internally counted so all acquired references need a matching call to
	 * release them.
	 *
	 * @param component component to acquire
	 */
	void mmal_component_acquire(MMAL_COMPONENT_T *component);

	/** Release a reference on a component
	 * Release an acquired reference on a component. Triggers the destruction of the component when
	 * the last reference is being released.
	 * \note This is in fact an alias of \ref mmal_component_destroy which is added to make client
	 * code clearer.
	 *
	 * @param component component to release
	 * @return MMAL_SUCCESS on success
	 */
	MMAL_STATUS_T mmal_component_release(MMAL_COMPONENT_T *component);

	/** Destroy a previously created component
	 * Release an acquired reference on a component. Only actually destroys the component when
	 * the last reference is being released.
	 *
	 * @param component component to destroy
	 * @return MMAL_SUCCESS on success
	 */
	__declspec(dllexport) MMAL_STATUS_T mmal_component_destroy(MMAL_COMPONENT_T *component);

	/** Enable processing on a component
	 * @param component component to enable
	 * @return MMAL_SUCCESS on success
	 */
	__declspec(dllexport) MMAL_STATUS_T mmal_component_enable(MMAL_COMPONENT_T *component);

	/** Disable processing on a component
	 * @param component component to disable
	 * @return MMAL_SUCCESS on success
	 */
	__declspec(dllexport) MMAL_STATUS_T mmal_component_disable(MMAL_COMPONENT_T *component);

	/* @} */


/** Camera-specific MMAL parameter IDs.
 * @ingroup MMAL_PARAMETER_IDS
 */
	enum {
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
	};

	/** Thumbnail configuration parameter type */
	typedef struct MMAL_PARAMETER_THUMBNAIL_CONFIG_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		uint32_t enable;                  /**< Enable generation of thumbnails during still capture */
		uint32_t width;                   /**< Desired width of the thumbnail */
		uint32_t height;                  /**< Desired height of the thumbnail */
		uint32_t quality;                 /**< Desired compression quality of the thumbnail */
	} MMAL_PARAMETER_THUMBNAIL_CONFIG_T;

	/** EXIF parameter type. */
	typedef struct MMAL_PARAMETER_EXIF_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		uint32_t keylen;                            /**< If 0, assume key is terminated by '=', otherwise length of key and treat data as binary */
		uint32_t value_offset;                      /**< Offset within data buffer of the start of the value. If 0, look for a "key=value" string */
		uint32_t valuelen;                          /**< If 0, assume value is null-terminated, otherwise length of value and treat data as binary */
		uint8_t data[1];                            /**< EXIF key/value string. Variable length */
	} MMAL_PARAMETER_EXIF_T;

	/** Exposure modes. */
	typedef enum
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
	} MMAL_PARAM_EXPOSUREMODE_T;

	typedef struct MMAL_PARAMETER_EXPOSUREMODE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_EXPOSUREMODE_T value;   /**< exposure mode */
	} MMAL_PARAMETER_EXPOSUREMODE_T;

	typedef enum
	{
		MMAL_PARAM_EXPOSUREMETERINGMODE_AVERAGE,
		MMAL_PARAM_EXPOSUREMETERINGMODE_SPOT,
		MMAL_PARAM_EXPOSUREMETERINGMODE_BACKLIT,
		MMAL_PARAM_EXPOSUREMETERINGMODE_MATRIX,
		MMAL_PARAM_EXPOSUREMETERINGMODE_MAX = 0x7fffffff
	} MMAL_PARAM_EXPOSUREMETERINGMODE_T;

	typedef struct MMAL_PARAMETER_EXPOSUREMETERINGMODE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_EXPOSUREMETERINGMODE_T value;   /**< metering mode */
	} MMAL_PARAMETER_EXPOSUREMETERINGMODE_T;

	/** AWB parameter modes. */
	typedef enum MMAL_PARAM_AWBMODE_T
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
	} MMAL_PARAM_AWBMODE_T;

	/** AWB parameter type. */
	typedef struct MMAL_PARAMETER_AWBMODE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_AWBMODE_T value;   /**< AWB mode */
	} MMAL_PARAMETER_AWBMODE_T;

	/** Image effect */
	typedef enum MMAL_PARAM_IMAGEFX_T
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
	} MMAL_PARAM_IMAGEFX_T;

	typedef struct MMAL_PARAMETER_IMAGEFX_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_IMAGEFX_T value;   /**< Image effect mode */
	} MMAL_PARAMETER_IMAGEFX_T;

#define MMAL_MAX_IMAGEFX_PARAMETERS 6  /* Image effects library currently uses a maximum of 5 parameters per effect */

	typedef struct MMAL_PARAMETER_IMAGEFX_PARAMETERS_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_IMAGEFX_T effect;   /**< Image effect mode */
		uint32_t num_effect_params;     /**< Number of used elements in */
		uint32_t effect_parameter[MMAL_MAX_IMAGEFX_PARAMETERS]; /**< Array of parameters */
	} MMAL_PARAMETER_IMAGEFX_PARAMETERS_T;

	/** Colour effect parameter type*/
	typedef struct MMAL_PARAMETER_COLOURFX_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		int32_t enable;
		uint32_t u;
		uint32_t v;
	} MMAL_PARAMETER_COLOURFX_T;

	typedef enum MMAL_CAMERA_STC_MODE_T
	{
		MMAL_PARAM_STC_MODE_OFF,         /**< Frames do not have STCs, as needed in OpenMAX/IL */
		MMAL_PARAM_STC_MODE_RAW,         /**< Use raw clock STC, needed for true pause/resume support */
		MMAL_PARAM_STC_MODE_COOKED,      /**< Start the STC from the start of capture, only for quick demo code */
		MMAL_PARAM_STC_MODE_MAX = 0x7fffffff
	} MMAL_CAMERA_STC_MODE_T;

	typedef struct MMAL_PARAMETER_CAMERA_STC_MODE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;
		MMAL_CAMERA_STC_MODE_T value;
	} MMAL_PARAMETER_CAMERA_STC_MODE_T;

	typedef enum MMAL_PARAM_FLICKERAVOID_T
	{
		MMAL_PARAM_FLICKERAVOID_OFF,
		MMAL_PARAM_FLICKERAVOID_AUTO,
		MMAL_PARAM_FLICKERAVOID_50HZ,
		MMAL_PARAM_FLICKERAVOID_60HZ,
		MMAL_PARAM_FLICKERAVOID_MAX = 0x7FFFFFFF
	} MMAL_PARAM_FLICKERAVOID_T;

	typedef struct MMAL_PARAMETER_FLICKERAVOID_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_FLICKERAVOID_T value;   /**< Flicker avoidance mode */
	} MMAL_PARAMETER_FLICKERAVOID_T;

	typedef enum MMAL_PARAM_FLASH_T
	{
		MMAL_PARAM_FLASH_OFF,
		MMAL_PARAM_FLASH_AUTO,
		MMAL_PARAM_FLASH_ON,
		MMAL_PARAM_FLASH_REDEYE,
		MMAL_PARAM_FLASH_FILLIN,
		MMAL_PARAM_FLASH_TORCH,
		MMAL_PARAM_FLASH_MAX = 0x7FFFFFFF
	} MMAL_PARAM_FLASH_T;

	typedef struct MMAL_PARAMETER_FLASH_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_FLASH_T value;   /**< Flash mode */
	} MMAL_PARAMETER_FLASH_T;

	typedef enum MMAL_PARAM_REDEYE_T
	{
		MMAL_PARAM_REDEYE_OFF,
		MMAL_PARAM_REDEYE_ON,
		MMAL_PARAM_REDEYE_SIMPLE,
		MMAL_PARAM_REDEYE_MAX = 0x7FFFFFFF
	} MMAL_PARAM_REDEYE_T;

	typedef struct MMAL_PARAMETER_REDEYE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_REDEYE_T value;   /**< Red eye reduction mode */
	} MMAL_PARAMETER_REDEYE_T;

	typedef enum MMAL_PARAM_FOCUS_T
	{
		MMAL_PARAM_FOCUS_AUTO,
		MMAL_PARAM_FOCUS_AUTO_NEAR,
		MMAL_PARAM_FOCUS_AUTO_MACRO,
		MMAL_PARAM_FOCUS_CAF,
		MMAL_PARAM_FOCUS_CAF_NEAR,
		MMAL_PARAM_FOCUS_FIXED_INFINITY,
		MMAL_PARAM_FOCUS_FIXED_HYPERFOCAL,
		MMAL_PARAM_FOCUS_FIXED_NEAR,
		MMAL_PARAM_FOCUS_FIXED_MACRO,
		MMAL_PARAM_FOCUS_EDOF,
		MMAL_PARAM_FOCUS_CAF_MACRO,
		MMAL_PARAM_FOCUS_CAF_FAST,
		MMAL_PARAM_FOCUS_CAF_NEAR_FAST,
		MMAL_PARAM_FOCUS_CAF_MACRO_FAST,
		MMAL_PARAM_FOCUS_FIXED_CURRENT,
		MMAL_PARAM_FOCUS_MAX = 0x7FFFFFFF
	} MMAL_PARAM_FOCUS_T;

	typedef struct MMAL_PARAMETER_FOCUS_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_FOCUS_T value;   /**< Focus mode */
	} MMAL_PARAMETER_FOCUS_T;

	typedef enum MMAL_PARAM_CAPTURE_STATUS_T
	{
		MMAL_PARAM_CAPTURE_STATUS_NOT_CAPTURING,
		MMAL_PARAM_CAPTURE_STATUS_CAPTURE_STARTED,
		MMAL_PARAM_CAPTURE_STATUS_CAPTURE_ENDED,

		MMAL_PARAM_CAPTURE_STATUS_MAX = 0x7FFFFFFF
	} MMAL_PARAM_CAPTURE_STATUS_T;

	typedef struct MMAL_PARAMETER_CAPTURE_STATUS_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_CAPTURE_STATUS_T status;   /**< Capture status */
	} MMAL_PARAMETER_CAPTURE_STATUS_T;

	typedef enum MMAL_PARAM_FOCUS_STATUS_T
	{
		MMAL_PARAM_FOCUS_STATUS_OFF,
		MMAL_PARAM_FOCUS_STATUS_REQUEST,
		MMAL_PARAM_FOCUS_STATUS_REACHED,
		MMAL_PARAM_FOCUS_STATUS_UNABLE_TO_REACH,
		MMAL_PARAM_FOCUS_STATUS_LOST,
		MMAL_PARAM_FOCUS_STATUS_CAF_MOVING,
		MMAL_PARAM_FOCUS_STATUS_CAF_SUCCESS,
		MMAL_PARAM_FOCUS_STATUS_CAF_FAILED,
		MMAL_PARAM_FOCUS_STATUS_MANUAL_MOVING,
		MMAL_PARAM_FOCUS_STATUS_MANUAL_REACHED,
		MMAL_PARAM_FOCUS_STATUS_CAF_WATCHING,
		MMAL_PARAM_FOCUS_STATUS_CAF_SCENE_CHANGED,

		MMAL_PARAM_FOCUS_STATUS_MAX = 0x7FFFFFFF
	} MMAL_PARAM_FOCUS_STATUS_T;

	typedef struct MMAL_PARAMETER_FOCUS_STATUS_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_FOCUS_STATUS_T status;   /**< Focus status */
	} MMAL_PARAMETER_FOCUS_STATUS_T;

	typedef enum MMAL_PARAM_FACE_TRACK_MODE_T
	{
		MMAL_PARAM_FACE_DETECT_NONE,                           /**< Disables face detection */
		MMAL_PARAM_FACE_DETECT_ON,                             /**< Enables face detection */
		MMAL_PARAM_FACE_DETECT_MAX = 0x7FFFFFFF
	} MMAL_PARAM_FACE_TRACK_MODE_T;

	typedef struct MMAL_PARAMETER_FACE_TRACK_T /* face tracking control */
	{
		MMAL_PARAMETER_HEADER_T hdr;
		MMAL_PARAM_FACE_TRACK_MODE_T mode;
		uint32_t maxRegions;
		uint32_t frames;
		uint32_t quality;
	} MMAL_PARAMETER_FACE_TRACK_T;

	typedef struct MMAL_PARAMETER_FACE_TRACK_FACE_T /* face tracking face information */
	{
		int32_t     face_id;             /**< Face ID. Should remain the same whilst the face is detected to remain in the scene */
		int32_t     score;               /**< Confidence of the face detection. Range 1-100 (1=unsure, 100=positive). */
		MMAL_RECT_T face_rect;           /**< Rectangle around the whole face */

		MMAL_RECT_T eye_rect[2];         /**< Rectangle around the eyes ([0] = left eye, [1] = right eye) */
		MMAL_RECT_T mouth_rect;          /**< Rectangle around the mouth */
	} MMAL_PARAMETER_FACE_TRACK_FACE_T;

	typedef struct MMAL_PARAMETER_FACE_TRACK_RESULTS_T /* face tracking results */
	{
		MMAL_PARAMETER_HEADER_T hdr;

		uint32_t num_faces;        /**< Number of faces detected */
		uint32_t frame_width;      /**< Width of the frame on which the faces were detected (allows scaling) */
		uint32_t frame_height;     /**< Height of the frame on which the faces were detected (allows scaling) */

		MMAL_PARAMETER_FACE_TRACK_FACE_T faces[1];   /**< Face information (variable length array */
	} MMAL_PARAMETER_FACE_TRACK_RESULTS_T;

	typedef enum MMAL_PARAMETER_CAMERA_CONFIG_TIMESTAMP_MODE_T
	{
		MMAL_PARAM_TIMESTAMP_MODE_ZERO,           /**< Always timestamp frames as 0 */
		MMAL_PARAM_TIMESTAMP_MODE_RAW_STC,        /**< Use the raw STC value for the frame timestamp */
		MMAL_PARAM_TIMESTAMP_MODE_RESET_STC,      /**< Use the STC timestamp but subtract the timestamp
												   * of the first frame sent to give a zero based timestamp.
												   */
		MMAL_PARAM_TIMESTAMP_MODE_MAX = 0x7FFFFFFF
	} MMAL_PARAMETER_CAMERA_CONFIG_TIMESTAMP_MODE_T;

	typedef struct MMAL_PARAMETER_CAMERA_CONFIG_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		/* Parameters for setting up the image pools */
		uint32_t max_stills_w;        /**< Max size of stills capture */
		uint32_t max_stills_h;
		uint32_t stills_yuv422;       /**< Allow YUV422 stills capture */
		uint32_t one_shot_stills;     /**< Continuous or one shot stills captures. */

		uint32_t max_preview_video_w; /**< Max size of the preview or video capture frames */
		uint32_t max_preview_video_h;
		uint32_t num_preview_video_frames;

		uint32_t stills_capture_circular_buffer_height; /**< Sets the height of the circular buffer for stills capture. */

		uint32_t fast_preview_resume;    /**< Allows preview/encode to resume as fast as possible after the stills input frame
										  * has been received, and then processes the still frame in the background
										  * whilst preview/encode has resumed.
										  * Actual mode is controlled by MMAL_PARAMETER_CAPTURE_MODE.
										  */

		MMAL_PARAMETER_CAMERA_CONFIG_TIMESTAMP_MODE_T use_stc_timestamp;
		/**< Selects algorithm for timestamping frames if there is no clock component connected.
		  */


	} MMAL_PARAMETER_CAMERA_CONFIG_T;

#define MMAL_PARAMETER_CAMERA_INFO_MAX_CAMERAS 4
#define MMAL_PARAMETER_CAMERA_INFO_MAX_FLASHES 2
#define MMAL_PARAMETER_CAMERA_INFO_MAX_STR_LEN 16

	typedef struct MMAL_PARAMETER_CAMERA_INFO_CAMERA_T
	{
		uint32_t    port_id;
		uint32_t    max_width;
		uint32_t    max_height;
		MMAL_BOOL_T lens_present;
		char        camera_name[MMAL_PARAMETER_CAMERA_INFO_MAX_STR_LEN];
	} MMAL_PARAMETER_CAMERA_INFO_CAMERA_T;

	typedef enum MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_T
	{
		MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_XENON = 0, /* Make values explicit */
		MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_LED = 1, /* to ensure they match */
		MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_OTHER = 2, /* values in config ini */
		MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_MAX = 0x7FFFFFFF
	} MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_T;

	typedef struct MMAL_PARAMETER_CAMERA_INFO_FLASH_T
	{
		MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_T flash_type;
	} MMAL_PARAMETER_CAMERA_INFO_FLASH_T;

	typedef struct MMAL_PARAMETER_CAMERA_INFO_T
	{
		MMAL_PARAMETER_HEADER_T             hdr;
		uint32_t                            num_cameras;
		uint32_t                            num_flashes;
		MMAL_PARAMETER_CAMERA_INFO_CAMERA_T cameras[MMAL_PARAMETER_CAMERA_INFO_MAX_CAMERAS];
		MMAL_PARAMETER_CAMERA_INFO_FLASH_T  flashes[MMAL_PARAMETER_CAMERA_INFO_MAX_FLASHES];
	} MMAL_PARAMETER_CAMERA_INFO_T;

	typedef enum MMAL_PARAMETER_CAPTUREMODE_MODE_T
	{
		MMAL_PARAM_CAPTUREMODE_WAIT_FOR_END,            /**< Resumes preview once capture is completed. */
		MMAL_PARAM_CAPTUREMODE_WAIT_FOR_END_AND_HOLD,   /**< Resumes preview once capture is completed, and hold the image for subsequent reprocessing. */
		MMAL_PARAM_CAPTUREMODE_RESUME_VF_IMMEDIATELY,   /**< Resumes preview as soon as possible once capture frame is received from the sensor.
														 *   Requires fast_preview_resume to be set via MMAL_PARAMETER_CAMERA_CONFIG.
														 */
	} MMAL_PARAMETER_CAPTUREMODE_MODE_T;

	/** Stills capture mode control. */
	typedef struct MMAL_PARAMETER_CAPTUREMODE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;
		MMAL_PARAMETER_CAPTUREMODE_MODE_T mode;
	} MMAL_PARAMETER_CAPTUREMODE_T;

	typedef enum MMAL_PARAMETER_FOCUS_REGION_TYPE_T
	{
		MMAL_PARAMETER_FOCUS_REGION_TYPE_NORMAL,     /**< Region defines a generic region */
		MMAL_PARAMETER_FOCUS_REGION_TYPE_FACE,       /**< Region defines a face */
		MMAL_PARAMETER_FOCUS_REGION_TYPE_MAX
	} MMAL_PARAMETER_FOCUS_REGION_TYPE_T;

	typedef struct MMAL_PARAMETER_FOCUS_REGION_T
	{
		MMAL_RECT_T rect;    /**< Focus rectangle as 0P16 fixed point values. */
		uint32_t weight;     /**< Region weighting. */
		uint32_t mask;       /**< Mask for multi-stage regions */
		MMAL_PARAMETER_FOCUS_REGION_TYPE_T type;  /**< Region type */
	} MMAL_PARAMETER_FOCUS_REGION_T;

	typedef struct MMAL_PARAMETER_FOCUS_REGIONS_T
	{
		MMAL_PARAMETER_HEADER_T          hdr;
		uint32_t                         num_regions;      /**< Number of regions defined */
		MMAL_BOOL_T                      lock_to_faces;    /**< If region is within tolerance of a face, adopt face rect instead of defined region */
		MMAL_PARAMETER_FOCUS_REGION_T    regions[1];       /**< Variable number of regions */
	} MMAL_PARAMETER_FOCUS_REGIONS_T;

	typedef struct MMAL_PARAMETER_INPUT_CROP_T
	{
		MMAL_PARAMETER_HEADER_T hdr;
		MMAL_RECT_T             rect;    /**< Crop rectangle as 16P16 fixed point values */
	} MMAL_PARAMETER_INPUT_CROP_T;

	typedef struct MMAL_PARAMETER_SENSOR_INFORMATION_T
	{
		MMAL_PARAMETER_HEADER_T          hdr;
		MMAL_RATIONAL_T                  f_number;         /**< Lens f-number */
		MMAL_RATIONAL_T                  focal_length;     /**< Lens focal length */
		uint32_t                         model_id;         /**< Sensor reported model id */
		uint32_t                         manufacturer_id;  /**< Sensor reported manufacturer id */
		uint32_t                         revision;         /**< Sensor reported revision */
	} MMAL_PARAMETER_SENSOR_INFORMATION_T;

	typedef struct MMAL_PARAMETER_FLASH_SELECT_T
	{
		MMAL_PARAMETER_HEADER_T          hdr;
		MMAL_PARAMETER_CAMERA_INFO_FLASH_TYPE_T flash_type;   /**< Flash type to use */
	} MMAL_PARAMETER_FLASH_SELECT_T;

	typedef struct MMAL_PARAMETER_FIELD_OF_VIEW_T
	{
		MMAL_PARAMETER_HEADER_T          hdr;
		MMAL_RATIONAL_T                  fov_h;         /**< Horizontal field of view */
		MMAL_RATIONAL_T                  fov_v;         /**< Vertical field of view */
	} MMAL_PARAMETER_FIELD_OF_VIEW_T;

	typedef enum MMAL_PARAMETER_DRC_STRENGTH_T
	{
		MMAL_PARAMETER_DRC_STRENGTH_OFF,
		MMAL_PARAMETER_DRC_STRENGTH_LOW,
		MMAL_PARAMETER_DRC_STRENGTH_MEDIUM,
		MMAL_PARAMETER_DRC_STRENGTH_HIGH,
		MMAL_PARAMETER_DRC_STRENGTH_MAX = 0x7fffffff
	} MMAL_PARAMETER_DRC_STRENGTH_T;

	typedef struct MMAL_PARAMETER_DRC_T
	{
		MMAL_PARAMETER_HEADER_T          hdr;
		MMAL_PARAMETER_DRC_STRENGTH_T    strength;      /**< DRC strength */
	} MMAL_PARAMETER_DRC_T;

	typedef enum MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_T
	{
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_FACETRACKING,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_REDEYE_REDUCTION,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_VIDEO_STABILISATION,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_WRITE_RAW,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_VIDEO_DENOISE,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_STILLS_DENOISE,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_TEMPORAL_DENOISE,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_ANTISHAKE,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_IMAGE_EFFECTS,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_DYNAMIC_RANGE_COMPRESSION,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_FACE_RECOGNITION,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_FACE_BEAUTIFICATION,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_SCENE_DETECTION,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_HIGH_DYNAMIC_RANGE,
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_MAX = 0x7fffffff
	} MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_T;

	typedef struct MMAL_PARAMETER_ALGORITHM_CONTROL_T
	{
		MMAL_PARAMETER_HEADER_T          hdr;
		MMAL_PARAMETER_ALGORITHM_CONTROL_ALGORITHMS_T algorithm;
		MMAL_BOOL_T                      enabled;
	} MMAL_PARAMETER_ALGORITHM_CONTROL_T;


	typedef enum MMAL_PARAM_CAMERA_USE_CASE_T
	{
		MMAL_PARAM_CAMERA_USE_CASE_UNKNOWN,             /**< Compromise on behaviour as use case totally unknown */
		MMAL_PARAM_CAMERA_USE_CASE_STILLS_CAPTURE,      /**< Stills capture use case */
		MMAL_PARAM_CAMERA_USE_CASE_VIDEO_CAPTURE,       /**< Video encode (camcorder) use case */

		MMAL_PARAM_CAMERA_USE_CASE_MAX = 0x7fffffff
	} MMAL_PARAM_CAMERA_USE_CASE_T;

	typedef struct MMAL_PARAMETER_CAMERA_USE_CASE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_CAMERA_USE_CASE_T use_case;   /**< Use case */
	} MMAL_PARAMETER_CAMERA_USE_CASE_T;

	typedef struct MMAL_PARAMETER_FPS_RANGE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_RATIONAL_T   fps_low;                /**< Low end of the permitted framerate range */
		MMAL_RATIONAL_T   fps_high;               /**< High end of the permitted framerate range */
	} MMAL_PARAMETER_FPS_RANGE_T;

	typedef struct MMAL_PARAMETER_ZEROSHUTTERLAG_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_BOOL_T zero_shutter_lag_mode;        /**< Select zero shutter lag mode from sensor */
		MMAL_BOOL_T concurrent_capture;           /**< Activate full zero shutter lag mode and
												   *  use the last preview raw image for the stills capture
												   */
	} MMAL_PARAMETER_ZEROSHUTTERLAG_T;

	typedef struct MMAL_PARAMETER_AWB_GAINS_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_RATIONAL_T r_gain;                   /**< Red gain */
		MMAL_RATIONAL_T b_gain;                   /**< Blue gain */
	} MMAL_PARAMETER_AWB_GAINS_T;

	typedef struct MMAL_PARAMETER_CAMERA_SETTINGS_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		uint32_t exposure;
		MMAL_RATIONAL_T analog_gain;
		MMAL_RATIONAL_T digital_gain;
		MMAL_RATIONAL_T awb_red_gain;
		MMAL_RATIONAL_T awb_blue_gain;
		uint32_t focus_position;
	} MMAL_PARAMETER_CAMERA_SETTINGS_T;

	typedef enum MMAL_PARAM_PRIVACY_INDICATOR_T
	{
		MMAL_PARAMETER_PRIVACY_INDICATOR_OFF,        /**< Indicator will be off. */
		MMAL_PARAMETER_PRIVACY_INDICATOR_ON,         /**< Indicator will come on just after a stills capture and
													  *   and remain on for 2seconds, or will be on whilst output[1]
													  *   is actively producing images.
													  */
		MMAL_PARAMETER_PRIVACY_INDICATOR_FORCE_ON,   /**< Turns indicator of for 2s independent of capture status.
													  *   Set this mode repeatedly to keep the indicator on for a
													  *   longer period.
													  */
		MMAL_PARAMETER_PRIVACY_INDICATOR_MAX = 0x7fffffff
	} MMAL_PARAM_PRIVACY_INDICATOR_T;

	typedef struct MMAL_PARAMETER_PRIVACY_INDICATOR_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_PARAM_PRIVACY_INDICATOR_T mode;
	} MMAL_PARAMETER_PRIVACY_INDICATOR_T;

#define MMAL_CAMERA_ANNOTATE_MAX_TEXT_LEN 32
	typedef struct MMAL_PARAMETER_CAMERA_ANNOTATE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_BOOL_T enable;
		char text[MMAL_CAMERA_ANNOTATE_MAX_TEXT_LEN];
		MMAL_BOOL_T show_shutter;
		MMAL_BOOL_T show_analog_gain;
		MMAL_BOOL_T show_lens;
		MMAL_BOOL_T show_caf;
		MMAL_BOOL_T show_motion;
	} MMAL_PARAMETER_CAMERA_ANNOTATE_T;

#define MMAL_CAMERA_ANNOTATE_MAX_TEXT_LEN_V2 256
	typedef struct MMAL_PARAMETER_CAMERA_ANNOTATE_V2_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_BOOL_T enable;
		MMAL_BOOL_T show_shutter;
		MMAL_BOOL_T show_analog_gain;
		MMAL_BOOL_T show_lens;
		MMAL_BOOL_T show_caf;
		MMAL_BOOL_T show_motion;
		MMAL_BOOL_T show_frame_num;
		MMAL_BOOL_T black_text_background;
		char text[MMAL_CAMERA_ANNOTATE_MAX_TEXT_LEN_V2];
	} MMAL_PARAMETER_CAMERA_ANNOTATE_V2_T;

#define MMAL_CAMERA_ANNOTATE_MAX_TEXT_LEN_V3 256
	typedef struct MMAL_PARAMETER_CAMERA_ANNOTATE_V3_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_BOOL_T enable;
		MMAL_BOOL_T show_shutter;
		MMAL_BOOL_T show_analog_gain;
		MMAL_BOOL_T show_lens;
		MMAL_BOOL_T show_caf;
		MMAL_BOOL_T show_motion;
		MMAL_BOOL_T show_frame_num;
		MMAL_BOOL_T enable_text_background;
		MMAL_BOOL_T custom_background_colour;
		uint8_t     custom_background_Y;
		uint8_t     custom_background_U;
		uint8_t     custom_background_V;
		uint8_t     dummy1;
		MMAL_BOOL_T custom_text_colour;
		uint8_t     custom_text_Y;
		uint8_t     custom_text_U;
		uint8_t     custom_text_V;
		uint8_t     text_size;
		char text[MMAL_CAMERA_ANNOTATE_MAX_TEXT_LEN_V3];
	} MMAL_PARAMETER_CAMERA_ANNOTATE_V3_T;

#define MMAL_CAMERA_ANNOTATE_MAX_TEXT_LEN_V4 256
	typedef struct MMAL_PARAMETER_CAMERA_ANNOTATE_V4_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_BOOL_T enable;
		MMAL_BOOL_T show_shutter;
		MMAL_BOOL_T show_analog_gain;
		MMAL_BOOL_T show_lens;
		MMAL_BOOL_T show_caf;
		MMAL_BOOL_T show_motion;
		MMAL_BOOL_T show_frame_num;
		MMAL_BOOL_T enable_text_background;
		MMAL_BOOL_T custom_background_colour;
		uint8_t     custom_background_Y;
		uint8_t     custom_background_U;
		uint8_t     custom_background_V;
		uint8_t     dummy1;
		MMAL_BOOL_T custom_text_colour;
		uint8_t     custom_text_Y;
		uint8_t     custom_text_U;
		uint8_t     custom_text_V;
		uint8_t     text_size;
		char text[MMAL_CAMERA_ANNOTATE_MAX_TEXT_LEN_V3];
		uint32_t    justify; //0=centre, 1=left, 2=right
		uint32_t    x_offset; //Offset from the justification edge
		uint32_t    y_offset;
	} MMAL_PARAMETER_CAMERA_ANNOTATE_V4_T;

	typedef enum MMAL_STEREOSCOPIC_MODE_T {
		MMAL_STEREOSCOPIC_MODE_NONE = 0,
		MMAL_STEREOSCOPIC_MODE_SIDE_BY_SIDE = 1,
		MMAL_STEREOSCOPIC_MODE_TOP_BOTTOM = 2,
		MMAL_STEREOSCOPIC_MODE_MAX = 0x7FFFFFFF,
	} MMAL_STEREOSCOPIC_MODE_T;

	typedef struct MMAL_PARAMETER_STEREOSCOPIC_MODE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_STEREOSCOPIC_MODE_T mode;
		MMAL_BOOL_T decimate;
		MMAL_BOOL_T swap_eyes;
	} MMAL_PARAMETER_STEREOSCOPIC_MODE_T;

	typedef enum MMAL_CAMERA_INTERFACE_T {
		MMAL_CAMERA_INTERFACE_CSI2 = 0,
		MMAL_CAMERA_INTERFACE_CCP2 = 1,
		MMAL_CAMERA_INTERFACE_CPI = 2,
		MMAL_CAMERA_INTERFACE_MAX = 0x7FFFFFFF,
	} MMAL_CAMERA_INTERFACE_T;

	typedef struct MMAL_PARAMETER_CAMERA_INTERFACE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_CAMERA_INTERFACE_T mode;
	} MMAL_PARAMETER_CAMERA_INTERFACE_T;

	typedef enum MMAL_CAMERA_CLOCKING_MODE_T {
		MMAL_CAMERA_CLOCKING_MODE_STROBE = 0,
		MMAL_CAMERA_CLOCKING_MODE_CLOCK = 1,
		MMAL_CAMERA_CLOCKING_MODE_MAX = 0x7FFFFFFF,
	} MMAL_CAMERA_CLOCKING_MODE_T;

	typedef struct MMAL_PARAMETER_CAMERA_CLOCKING_MODE_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_CAMERA_CLOCKING_MODE_T mode;
	} MMAL_PARAMETER_CAMERA_CLOCKING_MODE_T;

	typedef enum MMAL_CAMERA_RX_CONFIG_DECODE {
		MMAL_CAMERA_RX_CONFIG_DECODE_NONE = 0,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM8TO10 = 1,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM7TO10 = 2,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM6TO10 = 3,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM8TO12 = 4,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM7TO12 = 5,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM6TO12 = 6,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM10TO14 = 7,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM8TO14 = 8,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM12TO16 = 9,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM10TO16 = 10,
		MMAL_CAMERA_RX_CONFIG_DECODE_DPCM8TO16 = 11,
		MMAL_CAMERA_RX_CONFIG_DECODE_MAX = 0x7FFFFFFF
	} MMAL_CAMERA_RX_CONFIG_DECODE;

	typedef enum MMAL_CAMERA_RX_CONFIG_ENCODE {
		MMAL_CAMERA_RX_CONFIG_ENCODE_NONE = 0,
		MMAL_CAMERA_RX_CONFIG_ENCODE_DPCM10TO8 = 1,
		MMAL_CAMERA_RX_CONFIG_ENCODE_DPCM12TO8 = 2,
		MMAL_CAMERA_RX_CONFIG_ENCODE_DPCM14TO8 = 3,
		MMAL_CAMERA_RX_CONFIG_ENCODE_MAX = 0x7FFFFFFF
	} MMAL_CAMERA_RX_CONFIG_ENCODE;

	typedef enum MMAL_CAMERA_RX_CONFIG_UNPACK {
		MMAL_CAMERA_RX_CONFIG_UNPACK_NONE = 0,
		MMAL_CAMERA_RX_CONFIG_UNPACK_6 = 1,
		MMAL_CAMERA_RX_CONFIG_UNPACK_7 = 2,
		MMAL_CAMERA_RX_CONFIG_UNPACK_8 = 3,
		MMAL_CAMERA_RX_CONFIG_UNPACK_10 = 4,
		MMAL_CAMERA_RX_CONFIG_UNPACK_12 = 5,
		MMAL_CAMERA_RX_CONFIG_UNPACK_14 = 6,
		MMAL_CAMERA_RX_CONFIG_UNPACK_16 = 7,
		MMAL_CAMERA_RX_CONFIG_UNPACK_MAX = 0x7FFFFFFF
	} MMAL_CAMERA_RX_CONFIG_UNPACK;

	typedef enum MMAL_CAMERA_RX_CONFIG_PACK {
		MMAL_CAMERA_RX_CONFIG_PACK_NONE = 0,
		MMAL_CAMERA_RX_CONFIG_PACK_8 = 1,
		MMAL_CAMERA_RX_CONFIG_PACK_10 = 2,
		MMAL_CAMERA_RX_CONFIG_PACK_12 = 3,
		MMAL_CAMERA_RX_CONFIG_PACK_14 = 4,
		MMAL_CAMERA_RX_CONFIG_PACK_16 = 5,
		MMAL_CAMERA_RX_CONFIG_PACK_RAW10 = 6,
		MMAL_CAMERA_RX_CONFIG_PACK_RAW12 = 7,
		MMAL_CAMERA_RX_CONFIG_PACK_MAX = 0x7FFFFFFF
	} MMAL_CAMERA_RX_CONFIG_PACK;

	typedef struct MMAL_PARAMETER_CAMERA_RX_CONFIG_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_CAMERA_RX_CONFIG_DECODE decode;
		MMAL_CAMERA_RX_CONFIG_ENCODE encode;
		MMAL_CAMERA_RX_CONFIG_UNPACK unpack;
		MMAL_CAMERA_RX_CONFIG_PACK pack;
		uint32_t data_lanes;
		uint32_t encode_block_length;
		uint32_t embedded_data_lines;
		uint32_t image_id;
	} MMAL_PARAMETER_CAMERA_RX_CONFIG_T;

	typedef struct MMAL_PARAMETER_CAMERA_RX_TIMING_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		uint32_t timing1;
		uint32_t timing2;
		uint32_t timing3;
		uint32_t timing4;
		uint32_t timing5;
		uint32_t term1;
		uint32_t term2;
		uint32_t cpi_timing1;
		uint32_t cpi_timing2;
	} MMAL_PARAMETER_CAMERA_RX_TIMING_T;

	typedef struct MMAL_PARAMETER_LENS_SHADING_T
	{
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_BOOL_T enabled;
		uint32_t grid_cell_size;
		uint32_t grid_width;
		uint32_t grid_stride;
		uint32_t grid_height;
		uint32_t mem_handle_table;
		uint32_t ref_transform;
	} MMAL_PARAMETER_LENS_SHADING_T;

	/*
	The mode determines the kind of resize.
	MMAL_RESIZE_BOX allow the max_width and max_height to set a bounding box into
	which the output must fit.
	MMAL_RESIZE_BYTES allows max_bytes to set the maximum number of bytes into which the
	full output frame must fit.  Two flags aid the setting of the output
	size. preserve_aspect_ratio sets whether the resize should
	preserve the aspect ratio of the incoming
	image. allow_upscaling sets whether the resize is allowed to
	increase the size of the output image compared to the size of the
	input image.
	*/
	typedef enum MMAL_RESIZEMODE_T {
		MMAL_RESIZE_NONE,
		MMAL_RESIZE_CROP,
		MMAL_RESIZE_BOX,
		MMAL_RESIZE_BYTES,
		MMAL_RESIZE_DUMMY = 0x7FFFFFFF
	} MMAL_RESIZEMODE_T;

	typedef struct MMAL_PARAMETER_RESIZE_T {
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_RESIZEMODE_T mode;
		uint32_t max_width;
		uint32_t max_height;
		uint32_t max_bytes;
		MMAL_BOOL_T preserve_aspect_ratio;
		MMAL_BOOL_T allow_upscaling;
	} MMAL_PARAMETER_RESIZE_T;

	typedef struct MMAL_PARAMETER_CROP_T {
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_RECT_T rect;
	} MMAL_PARAMETER_CROP_T;

	typedef struct MMAL_PARAMETER_CCM_T {
		MMAL_RATIONAL_T ccm[3][3];
		int32_t offsets[3];
	} MMAL_PARAMETER_CCM_T;

	typedef struct MMAL_PARAMETER_CUSTOM_CCM_T {
		MMAL_PARAMETER_HEADER_T hdr;

		MMAL_BOOL_T enable;           /**< Enable the custom CCM. */
		MMAL_PARAMETER_CCM_T ccm;     /**< CCM to be used. */
	} MMAL_PARAMETER_CUSTOM_CCM_T;

#ifdef __cplusplus
}
#endif
//#include "interface/mmal/mmal.h"
//#include "interface/mmal/mmal_logging.h"
//#include "interface/mmal/mmal_buffer.h"
//#include "interface/mmal/util/mmal_util.h"
//#include "interface/mmal/util/mmal_util_params.h"
//#include "interface/mmal/util/mmal_default_components.h"
//#include "interface/mmal/util/mmal_connection.h"
//#include "interface/mmal/mmal_parameters_camera.h"

typedef unsigned char byte;
#include <memory>
#include <vector>
#include <algorithm>
using namespace std;