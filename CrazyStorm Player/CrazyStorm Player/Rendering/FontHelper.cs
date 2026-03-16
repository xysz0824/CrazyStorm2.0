/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.Linq;

namespace CrazyStorm_Player
{
    public sealed class RuntimeTextBuildResult
    {
        public FileResource AtlasResource { get; set; }
        public byte[] AtlasPngBytes { get; set; }
        public List<ParticleType> CharacterTypes { get; set; }
    }

    public static class FontHelper
    {
        const int PlaceholderGlyphSize = 1;
        static readonly object syncRoot = new object();
        static Dictionary<string, string> fontPathCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        static List<string> fontNames = new List<string>();
        static bool initialized;

        public static IReadOnlyList<string> FontNames
        {
            get
            {
                EnsureInitialized();
                return fontNames;
            }
        }

        public static void EnsureInitialized()
        {
            if (initialized) return;
            lock (syncRoot)
            {
                if (initialized) return;
                var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var path in EnumerateFontFiles())
                {
                    foreach (var fontName in LoadFontNames(path))
                    {
                        string existingPath;
                        if (!map.TryGetValue(fontName, out existingPath) || CompareFontPath(path, existingPath) < 0)
                        {
                            map[fontName] = path;
                        }
                    }
                }
                fontPathCache = map;
                fontNames = map.Keys.OrderBy(item => item, StringComparer.CurrentCultureIgnoreCase).ToList();
                initialized = true;
            }
        }

        public static RuntimeTextBuildResult BuildRuntimeText(CrazyStorm.Core.File file, ParticleSystem particleSystem, string fontName, string text,
            int charsetPixelSize)
        {
            if (file == null || particleSystem == null || StringUtil.IsNullOrWhiteSpace(fontName) || string.IsNullOrEmpty(text))
            {
                return null;
            }

            string fontPath;
            if (!TryResolveSystemFontPath(fontName, out fontPath)) return null;

            var charset = BuildCharset(text);
            if (string.IsNullOrEmpty(charset)) return null;

            var options = new MSDFAtlasOptions
            {
                FontPath = fontPath,
                Charset = charset,
                EmSize = charsetPixelSize,
                ImageType = MSDFImageType.Mtsdf,
                YOrigin = MSDFYOrigin.Top
            };
            var atlasResult = MSDFAtlasNative.Generate(options);
            var atlasPngBytes = MSDFAtlasNative.GeneratePng(options);
            var atlasResource = new FileResource(file, file.FileResourceIndex, $"{fontName}_{particleSystem.Name}_TextAtlas", "__runtime_text_atlas__");
            var glyphMap = BuildGlyphMap(atlasResult);
            var characterTypes = new List<ParticleType>(text.Length);
            foreach (char character in text)
            {
                characterTypes.Add(BuildCharacterType(particleSystem, atlasResource, character, glyphMap));
            }

            return new RuntimeTextBuildResult
            {
                AtlasResource = atlasResource,
                AtlasPngBytes = atlasPngBytes,
                CharacterTypes = characterTypes
            };
        }

        public static bool TryResolveSystemFontPath(string fontName, out string fontPath)
        {
            fontPath = null;
            if (StringUtil.IsNullOrWhiteSpace(fontName)) return false;
            EnsureInitialized();
            return fontPathCache.TryGetValue(fontName, out fontPath);
        }

        static string BuildCharset(string text)
        {
            var seen = new HashSet<char>();
            var chars = new List<char>();
            foreach (char item in text)
            {
                if (seen.Add(item)) chars.Add(item);
            }
            return new string(chars.ToArray());
        }

        static IEnumerable<string> EnumerateFontFiles()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var directory in new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts")
            })
            {
                if (StringUtil.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) continue;
                foreach (var pattern in new[] { "*.ttf", "*.otf" })
                {
                    foreach (var path in Directory.EnumerateFiles(directory, pattern, SearchOption.TopDirectoryOnly))
                    {
                        paths.Add(path);
                    }
                }
            }
            return paths;
        }

        static IEnumerable<string> LoadFontNames(string path)
        {
            var collection = new PrivateFontCollection();
            try
            {
                collection.AddFontFile(path);
                return collection.Families.Select(item => item.Name).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
            finally
            {
                collection.Dispose();
            }
        }

        static int CompareFontPath(string left, string right)
        {
            var leftExt = Path.GetExtension(left);
            var rightExt = Path.GetExtension(right);
            int extensionRank = GetExtensionRank(leftExt).CompareTo(GetExtensionRank(rightExt));
            if (extensionRank != 0) return extensionRank;

            int lengthRank = left.Length.CompareTo(right.Length);
            if (lengthRank != 0) return lengthRank;

            return StringComparer.OrdinalIgnoreCase.Compare(left, right);
        }

        static int GetExtensionRank(string extension)
        {
            if (string.Equals(extension, ".ttf", StringComparison.OrdinalIgnoreCase)) return 0;
            if (string.Equals(extension, ".otf", StringComparison.OrdinalIgnoreCase)) return 1;
            return 2;
        }

        static Dictionary<uint, MSDFGlyph> BuildGlyphMap(MSDFAtlasResult result)
        {
            var glyphMap = new Dictionary<uint, MSDFGlyph>();
            if (result?.Glyphs == null) return glyphMap;
            foreach (var glyph in result.Glyphs)
            {
                glyphMap[glyph.Unicode] = glyph;
            }
            return glyphMap;
        }

        static ParticleType BuildCharacterType(ParticleSystem particleSystem, FileResource atlasResource, char character,
            IDictionary<uint, MSDFGlyph> glyphMap)
        {
            var particleType = new ParticleType(particleSystem.CustomTypeIndex, character.ToString());
            particleType.Image = atlasResource;
            particleType.IsTextType = true;

            MSDFGlyph glyph;
            if (!glyphMap.TryGetValue(character, out glyph) ||
                glyph.AtlasBounds.Right <= glyph.AtlasBounds.Left ||
                glyph.AtlasBounds.Bottom <= glyph.AtlasBounds.Top)
            {
                particleType.IsTransparentPlaceholder = true;
                particleType.StartPoint = Vector2.Zero;
                particleType.Width = PlaceholderGlyphSize;
                particleType.Height = PlaceholderGlyphSize;
                particleType.CenterPoint = new Vector2(0.5f, 0.5f);
                particleType.Radius = 0;
                return particleType;
            }

            int width = Math.Max(1, (int)Math.Ceiling(glyph.AtlasBounds.Right - glyph.AtlasBounds.Left));
            int height = Math.Max(1, (int)Math.Ceiling(glyph.AtlasBounds.Bottom - glyph.AtlasBounds.Top));
            particleType.StartPoint = new Vector2((float)Math.Floor(glyph.AtlasBounds.Left), (float)Math.Floor(glyph.AtlasBounds.Top));
            particleType.Width = width;
            particleType.Height = height;
            particleType.CenterPoint = new Vector2(width / 2f, height / 2f);
            particleType.Radius = Math.Max(width, height) / 2;
            return particleType;
        }
    }
}
