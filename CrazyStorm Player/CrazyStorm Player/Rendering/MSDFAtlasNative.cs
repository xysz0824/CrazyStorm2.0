/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Runtime.InteropServices;

namespace CrazyStorm_Player
{
    public enum MSDFStatus
    {
        Success = 0,
        InvalidArgument = 1,
        FontLoadFailed = 2,
        GlyphLoadFailed = 3,
        AtlasPackFailed = 4,
        AllocationFailed = 5,
        PngEncodeFailed = 6,
        InternalError = 7
    }

    public enum MSDFImageType
    {
        HardMask = 1,
        SoftMask = 2,
        Sdf = 3,
        Psdf = 4,
        Msdf = 5,
        Mtsdf = 6
    }

    public enum MSDFYOrigin
    {
        Bottom = 0,
        Top = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSDFBounds
    {
        public double Left;
        public double Bottom;
        public double Right;
        public double Top;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSDFFontMetrics
    {
        public double EmSize;
        public double LineHeight;
        public double Ascender;
        public double Descender;
        public double UnderlineY;
        public double UnderlineThickness;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSDFGlyph
    {
        public uint Unicode;
        public int GlyphIndex;
        public double Advance;
        public MSDFBounds PlaneBounds;
        public MSDFBounds AtlasBounds;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct MSDFOptionsNative
    {
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string font_path;
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string charset_utf8;
        public double em_size;
        public double px_range;
        public double miter_limit;
        public double max_corner_angle;
        public int width;
        public int height;
        public int thread_count;
        public int preprocess_geometry;
        public int enable_kerning;
        public MSDFImageType image_type;
        public MSDFYOrigin y_origin;
        public int face_index;
        public int font_weight;
        public int font_stretch;
        public int font_style;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MSDFFontMetricsNative
    {
        public double em_size;
        public double line_height;
        public double ascender;
        public double descender;
        public double underline_y;
        public double underline_thickness;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MSDFBoundsNative
    {
        public double left;
        public double bottom;
        public double right;
        public double top;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MSDFGlyphNative
    {
        public uint unicode;
        public int glyph_index;
        public double advance;
        public MSDFBoundsNative plane_bounds;
        public MSDFBoundsNative atlas_bounds;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MSDFResultNative
    {
        public int width;
        public int height;
        public int channels;
        public UIntPtr pixel_count;
        public UIntPtr pixel_stride;
        public IntPtr pixels;
        public int glyph_count;
        public IntPtr glyphs;
        public MSDFFontMetricsNative metrics;
        public double atlas_em_size;
        public double distance_range;
        public MSDFImageType image_type;
        public MSDFYOrigin y_origin;
    }

    public sealed class MSDFAtlasOptions
    {
        public string FontPath { get; set; } = "";
        public string Charset { get; set; } = "";
        public double EmSize { get; set; } = 32.0;
        public double PxRange { get; set; } = 4.0;
        public double MiterLimit { get; set; } = 1.0;
        public double MaxCornerAngle { get; set; } = 3.0;
        public int Width { get; set; }
        public int Height { get; set; }
        public int ThreadCount { get; set; } = 1;
        public bool PreprocessGeometry { get; set; } = true;
        public bool EnableKerning { get; set; } = true;
        public MSDFImageType ImageType { get; set; } = MSDFImageType.Msdf;
        public MSDFYOrigin YOrigin { get; set; } = MSDFYOrigin.Bottom;
        public int FaceIndex { get; set; }
        public int FontWeight { get; set; }
        public int FontStretch { get; set; }
        public int FontStyle { get; set; } = -1;
    }

    public sealed class MSDFAtlasResult
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Channels { get; set; }
        public byte[] Pixels { get; set; } = Array.Empty<byte>();
        public MSDFGlyph[] Glyphs { get; set; } = Array.Empty<MSDFGlyph>();
        public MSDFFontMetrics Metrics { get; set; }
        public double AtlasEmSize { get; set; }
        public double DistanceRange { get; set; }
        public MSDFImageType ImageType { get; set; }
        public MSDFYOrigin YOrigin { get; set; }
    }

    public static class MSDFAtlasNative
    {
        private const string DllName = "msdf-atlas-gen-c.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void msdf_atlas_c_default_options(out MSDFOptionsNative options);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern MSDFStatus msdf_atlas_c_generate(ref MSDFOptionsNative options, out IntPtr result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern MSDFStatus msdf_atlas_c_encode_png(IntPtr result, out IntPtr pngBytes, out UIntPtr pngSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void msdf_atlas_c_result_destroy(IntPtr result);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void msdf_atlas_c_free(IntPtr ptr);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr msdf_atlas_c_last_error();

        public static MSDFAtlasResult Generate(MSDFAtlasOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.FontPath))
                throw new ArgumentException("FontPath is required.", nameof(options));

            msdf_atlas_c_default_options(out var nativeOptions);
            nativeOptions.font_path = options.FontPath;
            nativeOptions.charset_utf8 = string.IsNullOrEmpty(options.Charset) ? null : options.Charset;
            nativeOptions.em_size = options.EmSize;
            nativeOptions.px_range = options.PxRange;
            nativeOptions.miter_limit = options.MiterLimit;
            nativeOptions.max_corner_angle = options.MaxCornerAngle;
            nativeOptions.width = options.Width;
            nativeOptions.height = options.Height;
            nativeOptions.thread_count = options.ThreadCount;
            nativeOptions.preprocess_geometry = options.PreprocessGeometry ? 1 : 0;
            nativeOptions.enable_kerning = options.EnableKerning ? 1 : 0;
            nativeOptions.image_type = options.ImageType;
            nativeOptions.y_origin = options.YOrigin;
            nativeOptions.face_index = options.FaceIndex;
            nativeOptions.font_weight = options.FontWeight;
            nativeOptions.font_stretch = options.FontStretch;
            nativeOptions.font_style = options.FontStyle;

            var status = msdf_atlas_c_generate(ref nativeOptions, out var resultPtr);
            if (status != MSDFStatus.Success)
                throw new InvalidOperationException(GetLastError(status));

            IntPtr pngPtr = default;
            try
            {
                status = msdf_atlas_c_encode_png(resultPtr, out pngPtr, out var pngSize);
                if (status != MSDFStatus.Success)
                    throw new InvalidOperationException(GetLastError(status));

                var nativeResult = Marshal.PtrToStructure<MSDFResultNative>(resultPtr);
                var pixels = new byte[checked((int)pngSize)];
                try
                {
                    Marshal.Copy(pngPtr, pixels, 0, pixels.Length);
                }
                finally
                {
                    msdf_atlas_c_free(pngPtr);
                }
                var glyphs = new MSDFGlyph[nativeResult.glyph_count];
                var glyphSize = Marshal.SizeOf<MSDFGlyphNative>();
                for (var i = 0; i < glyphs.Length; i++)
                {
                    var glyphPtr = IntPtr.Add(nativeResult.glyphs, i * glyphSize);
                    var nativeGlyph = Marshal.PtrToStructure<MSDFGlyphNative>(glyphPtr);
                    glyphs[i] = new MSDFGlyph
                    {
                        Unicode = nativeGlyph.unicode,
                        GlyphIndex = nativeGlyph.glyph_index,
                        Advance = nativeGlyph.advance,
                        PlaneBounds = new MSDFBounds
                        {
                            Left = nativeGlyph.plane_bounds.left,
                            Bottom = nativeGlyph.plane_bounds.bottom,
                            Right = nativeGlyph.plane_bounds.right,
                            Top = nativeGlyph.plane_bounds.top
                        },
                        AtlasBounds = new MSDFBounds
                        {
                            Left = nativeGlyph.atlas_bounds.left,
                            Bottom = nativeGlyph.atlas_bounds.bottom,
                            Right = nativeGlyph.atlas_bounds.right,
                            Top = nativeGlyph.atlas_bounds.top
                        }
                    };
                }

                return new MSDFAtlasResult
                {
                    Width = nativeResult.width,
                    Height = nativeResult.height,
                    Channels = nativeResult.channels,
                    Pixels = pixels,
                    Glyphs = glyphs,
                    Metrics = new MSDFFontMetrics
                    {
                        EmSize = nativeResult.metrics.em_size,
                        LineHeight = nativeResult.metrics.line_height,
                        Ascender = nativeResult.metrics.ascender,
                        Descender = nativeResult.metrics.descender,
                        UnderlineY = nativeResult.metrics.underline_y,
                        UnderlineThickness = nativeResult.metrics.underline_thickness
                    },
                    AtlasEmSize = nativeResult.atlas_em_size,
                    DistanceRange = nativeResult.distance_range,
                    ImageType = nativeResult.image_type,
                    YOrigin = nativeResult.y_origin
                };
            }
            finally
            {
                msdf_atlas_c_result_destroy(resultPtr);
            }
        }

        private static string GetLastError(MSDFStatus status)
        {
            var error = Marshal.PtrToStringAnsi(msdf_atlas_c_last_error());
            return string.IsNullOrWhiteSpace(error) ? $"MSDF call failed: {status}" : $"MSDF call failed: {status} - {error}";
        }
    }
}