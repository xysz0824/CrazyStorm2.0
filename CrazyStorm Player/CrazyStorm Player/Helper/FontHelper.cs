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
using System.Runtime.InteropServices;
using Win32;
using Win32.Graphics.DirectWrite;
using static Win32.Apis;
using static Win32.Graphics.DirectWrite.Apis;

namespace CrazyStorm_Player
{
    public sealed class FontFace
    {
        public string FamilyLocaleName { get; set; }
        public uint FaceIndex { get; set; }
        public string FaceName { get; set; }
        public string FaceLocaleName { get; set; }
        public string FontPath { get; set; }
        public string Weight { get; set; }
        public string Style { get; set; }
        public string stretch { get; set; }
    }
    public sealed class TextAtlasKey : IEquatable<TextAtlasKey>
    {
        public string FontFamily { get; }
        public string FontFace { get; }
        public int CharsetPixelSize { get; }
        public TextAtlasKey(string fontFamily, string fontFace, int charsetPixelSize)
        {
            FontFamily = fontFamily;
            FontFace = fontFace;
            CharsetPixelSize = charsetPixelSize;
        }
        public bool Equals(TextAtlasKey other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(other, null)) return false;
            return CharsetPixelSize == other.CharsetPixelSize &&
                string.Equals(FontFamily, other.FontFamily) &&
                string.Equals(FontFace, other.FontFace);
        }
        public override bool Equals(object obj) => Equals(obj as TextAtlasKey);
        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.OrdinalIgnoreCase.GetHashCode(FontFamily) * 
                    StringComparer.OrdinalIgnoreCase.GetHashCode(FontFace) * 397) ^ CharsetPixelSize;
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
        public override bool Equals(object obj) => Equals(obj as TextCharacterKey);
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
        static Dictionary<string, List<FontFace>> fontPathDict = new Dictionary<string, List<FontFace>>();
        static Dictionary<TextAtlasKey, TextAtlasCache> TextAtlasCaches =
            new Dictionary<TextAtlasKey, TextAtlasCache>();
        static List<string> fontFamilyNames = new List<string>();

        sealed class TextAtlasCache
        {
            public TextAtlasKey AtlasKey { get; set; }
            public string Charset { get; set; }
            public HashSet<char> CharsetSet { get; set; }
            public byte[] AtlasPngBytes { get; set; }
            public Dictionary<uint, MSDFGlyph> GlyphMap { get; set; }
            public int Version { get; set; }
        }
        public static List<string> GetFontFamilysLocale()
        {
            var fontFamilys = new List<string>();
            foreach (var fontFamily in fontPathDict)
            {
                var face = fontFamily.Value.FirstOrDefault();
                fontFamilys.Add(face.FamilyLocaleName);
            }
            return fontFamilys;
        }
        public static string GetFontFamilyLocale(string fontFamily)
        {
            if (fontPathDict.ContainsKey(fontFamily))
            {
                var face = fontPathDict[fontFamily].FirstOrDefault();
                return face.FamilyLocaleName;
            }
            return null;
        }
        public static string GetFontFamilyByLocale(string familyLocale)
        {
            foreach (var fontFamily in fontPathDict)
            {
                var face = fontFamily.Value.FirstOrDefault(item => string.Equals(item.FamilyLocaleName, familyLocale));
                if (face != null) return fontFamily.Key;
            }
            return null;
        }
        public static List<string> GetFontFaces(string fontFamily)
        {
            var fontFaces = new List<string>();
            if (fontPathDict.ContainsKey(fontFamily))
            {
                foreach (var face in fontPathDict[fontFamily])
                {
                    fontFaces.Add(face.FaceName);
                }
            }
            return fontFaces;
        }
        public static List<string> GetFontFacesLocale(string fontFamily)
        {
            var fontFaces = new List<string>();
            if (fontPathDict.ContainsKey(fontFamily))
            {
                foreach (var face in fontPathDict[fontFamily])
                {
                    fontFaces.Add(face.FaceLocaleName);
                }
            }
            return fontFaces;
        }
        public static string GetFontFaceLocale(string fontFamily, string fontFace)
        {
            if (fontPathDict.ContainsKey(fontFamily))
            {
                var face = fontPathDict[fontFamily].FirstOrDefault(item => string.Equals(item.FaceName, fontFace));
                return face?.FaceLocaleName;
            }
            return null;
        }
        public static string GetFontFaceByLocale(string fontFamily, string faceLocale)
        {
            if (fontPathDict.ContainsKey(fontFamily))
            {
                var face = fontPathDict[fontFamily].FirstOrDefault(item => string.Equals(item.FaceLocaleName, faceLocale));
                return face?.FaceName;
            }
            return null;
        }
        private static unsafe int FindLocale(IDWriteLocalizedStrings* strings, string localeName)
        {
            if (string.IsNullOrEmpty(localeName)) return -1;
            fixed (char* pLocale = localeName)
            {
                uint index = 0;
                Bool32 exists = 0;
                var result = strings->FindLocaleName((ushort*)pLocale, &index, &exists);
                if (result.Failure) return -1;
                return exists != 0 ? (int)index : -1;
            }
        }
        private unsafe static string ReadStringAt(IDWriteLocalizedStrings* list, uint index)
        {
            uint length = 0;
            var result = list->GetStringLength(index, &length);
            if (result.Failure) return null;

            char* buffer = stackalloc char[(int)length + 1];
            result = list->GetString(index, (ushort*)buffer, length + 1);
            if (result.Failure) return null;

            return new string(buffer);
        }
        private unsafe static string GetFirstPropertyString(IDWriteFontSet* fontSet, uint listIndex, FontPropertyId propertyId, bool local)
        {
            using (ComPtr<IDWriteLocalizedStrings> values = default)
            {
                Bool32 exists = default;
                {
                    var hr = fontSet->GetPropertyValues(listIndex, propertyId, &exists, values.GetAddressOf());
                    if (hr.Failure || values.Get() == null) return string.Empty;
                    uint count = values.Get()->GetCount();
                    if (count == 0) return string.Empty;
                    if (!local) return ReadStringAt(values.Get(), 0);
                    string locale = LocaleHelper.GetSystemCulture().Name;
                    int index = FindLocale(values.Get(), locale);
                    if (index < 0) index = 0;
                    return ReadStringAt(values.Get(), (uint)index);
                }
            }
        }
        private unsafe static string TryGetLocalFilePath(IDWriteFontFaceReference* faceRef)
        {
            using (ComPtr<IDWriteFontFile> fontFile = default)
            {
                var result = faceRef->GetFontFile(fontFile.GetAddressOf());
                if (result.Failure) return null;
                void* referenceKey = null;
                uint referenceKeySize = 0;
                fontFile.Get()->GetReferenceKey(&referenceKey, &referenceKeySize);
                if (referenceKey == null || referenceKeySize == 0) return null;
                using (ComPtr<IDWriteFontFileLoader> loader = default)
                {
                    result = fontFile.Get()->GetLoader(loader.GetAddressOf());
                    if (result.Failure) return null;
                    using (ComPtr<IDWriteLocalFontFileLoader> localLoader = default)
                    {
                        result = loader.Get()->QueryInterface(__uuidof<IDWriteLocalFontFileLoader>(), localLoader.GetVoidAddressOf());
                        if (result.Failure) return null;
                        uint pathLength = 0;
                        localLoader.Get()->GetFilePathLengthFromKey(referenceKey, referenceKeySize, &pathLength);
                        char* buffer = stackalloc char[(int)pathLength + 1];
                        localLoader.Get()->GetFilePathFromKey(referenceKey, referenceKeySize, (ushort*)buffer, pathLength + 1);
                        return new string(buffer);
                    }
                }
            }
        }
        public unsafe static void EnsureInitialized()
        {
            if (fontPathDict.Count > 0) return;
            using (ComPtr<IDWriteFactory3> factory = default)
            {
                var result = DWriteCreateFactory(FactoryType.Shared, __uuidof<IDWriteFactory3>(), factory.GetVoidAddressOf());
                if (result.Failure || factory.Get() == null) return;
                using (ComPtr<IDWriteFontSet> fontSet = default)
                {
                    result = factory.Get()->GetSystemFontSet(fontSet.GetAddressOf());
                    if (result.Failure) return;
                    uint fontCount = fontSet.Get()->GetFontCount();
                    for (uint i = 0; i < fontCount; ++i)
                    {
                        using (ComPtr<IDWriteFontFaceReference> faceRef = default)
                        {
                            result = fontSet.Get()->GetFontFaceReference(i, faceRef.GetAddressOf());
                            if (result.Failure) continue;
                            uint faceIndex = faceRef.Get()->GetFontFaceIndex();
                            string familyName = GetFirstPropertyString(fontSet.Get(), i, FontPropertyId.FamilyName, false);
                            string familyLocaleName = GetFirstPropertyString(fontSet.Get(), i, FontPropertyId.FamilyName, true);
                            string faceName = GetFirstPropertyString(fontSet.Get(), i, FontPropertyId.FaceName, false);
                            string faceLocaleName = GetFirstPropertyString(fontSet.Get(), i, FontPropertyId.FaceName, true);
                            string fullName = GetFirstPropertyString(fontSet.Get(), i, FontPropertyId.FullName, true);
                            string weight = GetFirstPropertyString(fontSet.Get(), i, FontPropertyId.Weight, false);
                            string style = GetFirstPropertyString(fontSet.Get(), i, FontPropertyId.Style, false);
                            string stretch = GetFirstPropertyString(fontSet.Get(), i, FontPropertyId.Stretch, false);
                            string filePath = TryGetLocalFilePath(faceRef.Get());
                            if (string.IsNullOrEmpty(filePath)) continue;
                            if (!fontPathDict.ContainsKey(familyName)) fontPathDict[familyName] = new List<FontFace>();
                            fontPathDict[familyName].Insert(MathHelper.Clamp((int)faceIndex, 0, fontPathDict[familyName].Count), new FontFace
                            {
                                FamilyLocaleName = familyLocaleName,
                                FaceIndex = faceIndex,
                                FaceName = faceName,
                                FaceLocaleName = faceLocaleName,
                                FontPath = filePath,
                                Weight = weight,
                                Style = style,
                                stretch = stretch,
                            });
                        }

                    }
                }
            }
            fontFamilyNames = fontPathDict.Keys.OrderBy(item => item, StringComparer.CurrentCultureIgnoreCase).ToList();
        }

        public static TextBuildResult EnsureTextAtlas(string fontFamily, string fontFace, string text, int charsetPixelSize)
        {
            if (string.IsNullOrEmpty(text)) return null;

            string fontPath;
            if (!TryResolveSystemFontPath(fontFamily, fontFace, out fontPath)) return null;

            var requestedCharacters = text.ToList();
            var charset = BuildCharset(requestedCharacters);
            if (string.IsNullOrEmpty(charset)) return null;

            var atlasKey = new TextAtlasKey(fontFamily, fontFace, charsetPixelSize);
            TextAtlasCache cache;
            bool atlasUpdated = false;
            if (!TextAtlasCaches.TryGetValue(atlasKey, out cache))
            {
                cache = BuildTextAtlas(atlasKey, fontPath, charset, null);
                if (cache == null) return null;
                TextAtlasCaches[atlasKey] = cache;
                atlasUpdated = true;
            }
            else if (!ContainsAllCharacters(cache.CharsetSet, requestedCharacters))
            {
                var mergedCharset = MergeCharset(cache.Charset, requestedCharacters);
                var updated = BuildTextAtlas(atlasKey, fontPath, mergedCharset, cache);
                if (updated != null)
                {
                    cache = updated;
                    TextAtlasCaches[atlasKey] = cache;
                    atlasUpdated = true;
                }
            }

            return new TextBuildResult
            {
                AtlasKey = cache.AtlasKey,
                AtlasVersion = cache.Version,
                AtlasUpdated = atlasUpdated,
                AtlasPngBytes = cache.AtlasPngBytes,
                GlyphMap = cache.GlyphMap,
                Characters = requestedCharacters
            };
        }

        public static bool TryResolveSystemFontPath(string fontFamily, string fontFace, out string fontPath)
        {
            fontPath = null;
            EnsureInitialized();
            if (fontPathDict.ContainsKey(fontFamily))
            {
                var faces = fontPathDict[fontFamily];
                var face = faces.FirstOrDefault(item => string.Equals(item.FaceName, fontFace));
                if (face != null)
                {
                    fontPath = face.FontPath;
                    return true;
                }
            }
            return false;
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

        static TextAtlasCache BuildTextAtlas(TextAtlasKey atlasKey, string fontPath, string charset,
            TextAtlasCache previous)
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
                return new TextAtlasCache
                {
                    AtlasKey = atlasKey,
                    Charset = charset,
                    CharsetSet = new HashSet<char>(charset),
                    AtlasPngBytes = atlasResult.Pixels,
                    GlyphMap = BuildGlyphMap(atlasResult.Glyphs),
                    Version = previous != null ? previous.Version + 1 : 1
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
