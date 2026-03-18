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
    public sealed class TextAtlasKey : IEquatable<TextAtlasKey>
    {
        public TextAtlasKey(string fontName, int charsetPixelSize)
        {
            FontName = fontName ?? string.Empty;
            CharsetPixelSize = charsetPixelSize;
        }

        public string FontName { get; }
        public int CharsetPixelSize { get; }

        public bool Equals(TextAtlasKey other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;
            return CharsetPixelSize == other.CharsetPixelSize &&
                string.Equals(FontName, other.FontName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TextAtlasKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.OrdinalIgnoreCase.GetHashCode(FontName) * 397) ^ CharsetPixelSize;
            }
        }
    }

    public sealed class TextCharacterKey : IEquatable<TextCharacterKey>
    {
        public TextCharacterKey(TextAtlasKey atlasKey, char character)
        {
            AtlasKey = atlasKey;
            Character = character;
        }

        public TextAtlasKey AtlasKey { get; }
        public char Character { get; }

        public bool Equals(TextCharacterKey other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;
            return Character == other.Character && Equals(AtlasKey, other.AtlasKey);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TextCharacterKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((AtlasKey?.GetHashCode() ?? 0) * 397) ^ Character.GetHashCode();
            }
        }
    }

    public sealed class TextBuildResult
    {
        public TextAtlasKey AtlasKey { get; set; }
        public int AtlasVersion { get; set; }
        public bool AtlasUpdated { get; set; }
        public byte[] AtlasPngBytes { get; set; }
        public IReadOnlyDictionary<uint, MSDFGlyph> GlyphMap { get; set; }
        public List<char> Characters { get; set; }
    }

    public static class FontHelper
    {
        const int PlaceholderGlyphSize = 1;
        static readonly object syncRoot = new object();
        static Dictionary<string, string> fontPathCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        static Dictionary<TextAtlasKey, TextAtlasCacheEntry> TextAtlasCache =
            new Dictionary<TextAtlasKey, TextAtlasCacheEntry>();
        static List<string> fontNames = new List<string>();
        static bool initialized;

        sealed class TextAtlasCacheEntry
        {
            public TextAtlasKey AtlasKey { get; set; }
            public string Charset { get; set; }
            public HashSet<char> CharsetSet { get; set; }
            public byte[] AtlasPngBytes { get; set; }
            public Dictionary<uint, MSDFGlyph> GlyphMap { get; set; }
            public int Version { get; set; }
        }

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

        public static TextBuildResult EnsureTextAtlas(string fontName, string text, int charsetPixelSize)
        {
            if (StringUtil.IsNullOrWhiteSpace(fontName) || string.IsNullOrEmpty(text)) return null;

            string fontPath;
            if (!TryResolveSystemFontPath(fontName, out fontPath)) return null;

            var requestedCharacters = text.ToList();
            var charset = BuildCharset(requestedCharacters);
            if (string.IsNullOrEmpty(charset)) return null;

            var atlasKey = new TextAtlasKey(fontName, charsetPixelSize);
            lock (syncRoot)
            {
                TextAtlasCacheEntry cacheEntry;
                bool atlasUpdated = false;
                if (!TextAtlasCache.TryGetValue(atlasKey, out cacheEntry))
                {
                    cacheEntry = BuildTextAtlasEntry(atlasKey, fontPath, charset, null);
                    if (cacheEntry == null) return null;
                    TextAtlasCache[atlasKey] = cacheEntry;
                    atlasUpdated = true;
                }
                else if (!ContainsAllCharacters(cacheEntry.CharsetSet, requestedCharacters))
                {
                    var mergedCharset = MergeCharset(cacheEntry.Charset, requestedCharacters);
                    var updatedEntry = BuildTextAtlasEntry(atlasKey, fontPath, mergedCharset, cacheEntry);
                    if (updatedEntry != null)
                    {
                        cacheEntry = updatedEntry;
                        TextAtlasCache[atlasKey] = cacheEntry;
                        atlasUpdated = true;
                    }
                }

                return new TextBuildResult
                {
                    AtlasKey = cacheEntry.AtlasKey,
                    AtlasVersion = cacheEntry.Version,
                    AtlasUpdated = atlasUpdated,
                    AtlasPngBytes = cacheEntry.AtlasPngBytes,
                    GlyphMap = cacheEntry.GlyphMap,
                    Characters = requestedCharacters
                };
            }
        }

        public static bool TryResolveSystemFontPath(string fontName, out string fontPath)
        {
            fontPath = null;
            if (StringUtil.IsNullOrWhiteSpace(fontName)) return false;
            EnsureInitialized();
            return fontPathCache.TryGetValue(fontName, out fontPath);
        }

        public static void UpdateCharacterType(ParticleType particleType, FileResource atlasResource, char character,
            IReadOnlyDictionary<uint, MSDFGlyph> glyphMap)
        {
            if (particleType == null) return;

            particleType.Name = character.ToString();
            particleType.Image = atlasResource;
            particleType.IsTextType = true;

            MSDFGlyph glyph;
            if (glyphMap == null || !glyphMap.TryGetValue(character, out glyph) ||
                glyph.AtlasBounds.Right <= glyph.AtlasBounds.Left ||
                glyph.AtlasBounds.Bottom <= glyph.AtlasBounds.Top)
            {
                particleType.IsTransparentPlaceholder = true;
                particleType.StartPoint = Vector2.Zero;
                particleType.Width = PlaceholderGlyphSize;
                particleType.Height = PlaceholderGlyphSize;
                particleType.CenterPoint = new Vector2(0.5f, 0.5f);
                particleType.Radius = 0;
                return;
            }

            particleType.IsTransparentPlaceholder = false;
            int width = Math.Max(1, (int)Math.Ceiling(glyph.AtlasBounds.Right - glyph.AtlasBounds.Left));
            int height = Math.Max(1, (int)Math.Ceiling(glyph.AtlasBounds.Bottom - glyph.AtlasBounds.Top));
            particleType.StartPoint = new Vector2((float)Math.Floor(glyph.AtlasBounds.Left), (float)Math.Floor(glyph.AtlasBounds.Top));
            particleType.Width = width;
            particleType.Height = height;
            particleType.CenterPoint = new Vector2(width / 2f, height / 2f);
            particleType.Radius = Math.Max(width, height) / 2;
        }

        static string BuildCharset(IEnumerable<char> characters)
        {
            var seen = new HashSet<char>();
            var chars = new List<char>();
            foreach (char item in characters)
            {
                if (seen.Add(item)) chars.Add(item);
            }
            return new string(chars.ToArray());
        }

        static bool ContainsAllCharacters(HashSet<char> existingCharacters, IEnumerable<char> requestedCharacters)
        {
            foreach (char character in requestedCharacters)
            {
                if (!existingCharacters.Contains(character)) return false;
            }
            return true;
        }

        static string MergeCharset(string existingCharset, IEnumerable<char> requestedCharacters)
        {
            var mergedCharacters = new List<char>(existingCharset?.Length ?? 0);
            var seen = new HashSet<char>();
            if (!string.IsNullOrEmpty(existingCharset))
            {
                foreach (char character in existingCharset)
                {
                    if (seen.Add(character)) mergedCharacters.Add(character);
                }
            }
            foreach (char character in requestedCharacters)
            {
                if (seen.Add(character)) mergedCharacters.Add(character);
            }
            return new string(mergedCharacters.ToArray());
        }

        static TextAtlasCacheEntry BuildTextAtlasEntry(TextAtlasKey atlasKey, string fontPath, string charset,
            TextAtlasCacheEntry previousEntry)
        {
            try
            {
                var options = new MSDFAtlasOptions
                {
                    FontPath = fontPath,
                    Charset = charset,
                    EmSize = atlasKey.CharsetPixelSize,
                    ImageType = MSDFImageType.Mtsdf,
                    YOrigin = MSDFYOrigin.Top
                };
                var atlasResult = MSDFAtlasNative.Generate(options);
                return new TextAtlasCacheEntry
                {
                    AtlasKey = atlasKey,
                    Charset = charset,
                    CharsetSet = new HashSet<char>(charset),
                    AtlasPngBytes = atlasResult.Pixels,
                    GlyphMap = BuildGlyphMap(atlasResult.Glyphs),
                    Version = previousEntry != null ? previousEntry.Version + 1 : 1
                };
            }
            catch
            {
                return null;
            }
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

        static Dictionary<uint, MSDFGlyph> BuildGlyphMap(MSDFGlyph[] glyphs)
        {
            var glyphMap = new Dictionary<uint, MSDFGlyph>();
            if (glyphs == null) return glyphMap;
            foreach (var glyph in glyphs)
            {
                glyphMap[glyph.Unicode] = glyph;
            }
            return glyphMap;
        }

    }
}
