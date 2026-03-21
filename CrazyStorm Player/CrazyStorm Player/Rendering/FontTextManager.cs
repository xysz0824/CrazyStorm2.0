/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using File = CrazyStorm.Core.File;

namespace CrazyStorm_Player
{
    sealed class FontTextManager
    {
        sealed class TextAtlasKey : IEquatable<TextAtlasKey>
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
        sealed class TextCharacterKey : IEquatable<TextCharacterKey>
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
        sealed class TextBuildResult
        {
            public TextAtlasKey AtlasKey { get; set; }
            public int AtlasVersion { get; set; }
            public bool AtlasUpdated { get; set; }
            public byte[] AtlasPngBytes { get; set; }
            public IReadOnlyDictionary<uint, MSDFGlyph> GlyphMap { get; set; }
            public List<char> Characters { get; set; }
            public double DistanceRange { get; set; }
        }

        sealed class TextAtlasCache
        {
            public TextAtlasKey AtlasKey { get; set; }
            public string Charset { get; set; }
            public HashSet<char> CharsetSet { get; set; }
            public byte[] AtlasPngBytes { get; set; }
            public Dictionary<uint, MSDFGlyph> GlyphMap { get; set; }
            public double DistanceRange { get; set; }
            public int Version { get; set; }
        }
        sealed class TextFileBinding
        {
            public FileResource AtlasResource { get; set; }
            public int AtlasVersion { get; set; }
        }

        static Dictionary<TextAtlasKey, TextAtlasCache> TextAtlasCaches = 
            new Dictionary<TextAtlasKey, TextAtlasCache>();
        static readonly Dictionary<File, Dictionary<TextAtlasKey, TextFileBinding>> TextBindings =
            new Dictionary<File, Dictionary<TextAtlasKey, TextFileBinding>>();
        static readonly Dictionary<ParticleSystem, Dictionary<TextCharacterKey, ParticleType>> TextCharacterTypes =
            new Dictionary<ParticleSystem, Dictionary<TextCharacterKey, ParticleType>>();
        static readonly Dictionary<TextAtlasKey, List<ParticleSystem>> TextAtlasSystems =
            new Dictionary<TextAtlasKey, List<ParticleSystem>>();

        static void RegisterTextAtlasSystem(TextAtlasKey atlasKey, ParticleSystem instance)
        {
            List<ParticleSystem> systems;
            if (!TextAtlasSystems.TryGetValue(atlasKey, out systems))
            {
                systems = new List<ParticleSystem>();
                TextAtlasSystems[atlasKey] = systems;
            }
            if (!systems.Contains(instance)) systems.Add(instance);
        }
        static void UpdateTextAtlas(TextBuildResult result, List<ParticleSystem> instances, 
            Action<FileResource, byte[]> onTextureUpdate)
        {
            List<ParticleSystem> systems;
            if (!TextAtlasSystems.TryGetValue(result.AtlasKey, out systems)) return;
            foreach (var system in systems)
            {
                var binding = EnsureTextFileBinding(system.File, system, result, onTextureUpdate);
                if (binding == null || binding.AtlasResource == null) continue;
                UpdateTextCharacterTypes(system, result, binding.AtlasResource);
            }
        }
        static TextFileBinding EnsureTextFileBinding(File file, ParticleSystem system, TextBuildResult result,
             Action<FileResource, byte[]> onTextureUpdate)
        {
            Dictionary<TextAtlasKey, TextFileBinding> bindings;
            if (!TextBindings.TryGetValue(file, out bindings))
            {
                bindings = new Dictionary<TextAtlasKey, TextFileBinding>();
                TextBindings[file] = bindings;
            }

            TextFileBinding binding;
            if (!bindings.TryGetValue(result.AtlasKey, out binding))
            {
                binding = new TextFileBinding
                {
                    AtlasResource = new FileResource(file, file.FileResourceIndex,
                        $"{result.AtlasKey.FontFamily}_{result.AtlasKey.FontFace}_{system.Name}_TextAtlas", "")
                };
                bindings[result.AtlasKey] = binding;
            }
            if (binding.AtlasVersion == result.AtlasVersion) return binding;
            onTextureUpdate?.Invoke(binding.AtlasResource, result.AtlasPngBytes);
            binding.AtlasVersion = result.AtlasVersion;
            return binding;
        }
        static void UpdateParticleType(ParticleType particleType, FileResource atlasResource, char character,
            IReadOnlyDictionary<uint, MSDFGlyph> glyphMap, double distanceRange)
        {
            if (particleType == null) return;
            particleType.Name = character.ToString();
            particleType.Image = atlasResource;
            particleType.IsTextType = true;
            particleType.TextPxRange = distanceRange;
            MSDFGlyph glyph;
            if (glyphMap == null || !glyphMap.TryGetValue(character, out glyph) ||
                glyph.AtlasBounds.Right <= glyph.AtlasBounds.Left ||
                glyph.AtlasBounds.Bottom <= glyph.AtlasBounds.Top)
            {
                particleType.IsTransparentPlaceholder = true;
                particleType.StartPoint = Vector2.Zero;
                particleType.Width = 1;
                particleType.Height = 1;
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
        static void UpdateTextCharacterTypes(ParticleSystem instance, TextBuildResult result, FileResource atlasResource)
        {
            Dictionary<TextCharacterKey, ParticleType> characterTypes;
            if (!TextCharacterTypes.TryGetValue(instance, out characterTypes)) return;
            foreach (var item in characterTypes)
            {
                if (!Equals(item.Key.AtlasKey, result.AtlasKey)) continue;
                UpdateParticleType(item.Value, atlasResource, item.Key.Character, result.GlyphMap, result.DistanceRange);
            }
        }
        static List<ParticleType> EnsureTextCharacterTypes(File file, ParticleSystem instance, TextBuildResult result)
        {
            Dictionary<TextCharacterKey, ParticleType> characterTypes;
            if (!TextCharacterTypes.TryGetValue(instance, out characterTypes))
            {
                characterTypes = new Dictionary<TextCharacterKey, ParticleType>();
                TextCharacterTypes[instance] = characterTypes;
            }
            var repeatable = new List<ParticleType>();
            var atlasResource = TextBindings[file][result.AtlasKey].AtlasResource;
            foreach (var character in result.Characters)
            {
                var characterKey = new TextCharacterKey(result.AtlasKey, character);
                ParticleType particleType;
                if (!characterTypes.TryGetValue(characterKey, out particleType))
                {
                    particleType = new ParticleType(instance.CustomTypeIndex, character.ToString());
                    instance.CustomTypes.Add(particleType);
                    characterTypes[characterKey] = particleType;
                }
                UpdateParticleType(particleType, atlasResource, character, result.GlyphMap, result.DistanceRange);
                repeatable.Add(particleType);
            }
            return repeatable;
        }
        static TextBuildResult EnsureTextAtlas(string fontFamily, string fontFace, string text, int charsetPixelSize)
        {
            if (string.IsNullOrEmpty(text)) return null;

            string fontPath;
            if (!FontHelper.TryResolveSystemFontPath(fontFamily, fontFace, out fontPath)) return null;

            var requestedCharacters = text.ToList();
            var charset = BuildCharset(requestedCharacters);
            if (string.IsNullOrEmpty(charset)) return null;

            var atlasKey = new TextAtlasKey(fontFamily, fontFace, charsetPixelSize);
            TextAtlasCache cache;
            bool atlasUpdated = false;
            if (!TextAtlasCaches.TryGetValue(atlasKey, out cache))
            {
                cache = BuildTextAtlas(atlasKey, fontPath, fontFamily, fontFace, charset, null);
                if (cache == null) return null;
                TextAtlasCaches[atlasKey] = cache;
                atlasUpdated = true;
            }
            else if (!ContainsAllCharacters(cache.CharsetSet, requestedCharacters))
            {
                var mergedCharset = MergeCharset(cache.Charset, requestedCharacters);
                var updated = BuildTextAtlas(atlasKey, fontPath, fontFamily, fontFace, mergedCharset, cache);
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
                Characters = requestedCharacters,
                DistanceRange = cache.DistanceRange
            };
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
        static TextAtlasCache BuildTextAtlas(TextAtlasKey atlasKey, string fontPath, string fontFamily, string fontFace,
            string charset, TextAtlasCache previous)
        {
            try
            {
                var face = FontHelper.GetFontFace(fontFamily, fontFace);
                if (face == null) return null;
                var options = new MSDFAtlasOptions
                {
                    FontPath = fontPath,
                    FaceIndex = (int)face.FaceIndex,
                    FontWeight = int.Parse(face.Weight),
                    FontStretch = int.Parse(face.stretch),
                    FontStyle = int.Parse(face.Style),
                    Charset = charset,
                    EmSize = atlasKey.CharsetPixelSize,
                    ImageType = MSDFImageType.Sdf,
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
                    DistanceRange = atlasResult.DistanceRange,
                    Version = previous != null ? previous.Version + 1 : 1
                };
            }
            catch
            {
                return null;
            }
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

        public static List<ParticleType> UpdateTextResources(File file, ParticleSystem instance, Text text,
            List<ParticleSystem> instances, Action<FileResource, byte[]> onTextureUpdate)
        {
            var result = EnsureTextAtlas(text.FontFamily, text.FontFace, text.TextValue, text.CharsetPixelSize);
            if (result == null) throw new InvalidDataException();
            RegisterTextAtlasSystem(result.AtlasKey, instance);
            EnsureTextFileBinding(file, instance, result, onTextureUpdate);
            if (result.AtlasUpdated) UpdateTextAtlas(result, instances, onTextureUpdate);
            return EnsureTextCharacterTypes(file, instance, result);
        }

        public static void Clear()
        {
            TextBindings.Clear();
            TextCharacterTypes.Clear();
            TextAtlasSystems.Clear();
        }
        public static void Clear(ParticleSystem instance, Action<FileResource> onTextureDestroy)
        {
            if (instance == null) return;
            Dictionary<TextCharacterKey, ParticleType> characterTypes;
            if (TextCharacterTypes.TryGetValue(instance, out characterTypes))
            {
                foreach (var type in characterTypes.Values)
                {
                    instance.CustomTypes.Remove(type);
                }
                TextCharacterTypes.Remove(instance);
            }
            var removedAtlasKeys = new List<TextAtlasKey>();
            foreach (var atlasEntry in TextAtlasSystems)
            {
                var systems = atlasEntry.Value;
                if (systems == null) continue;
                systems.RemoveAll(system => ReferenceEquals(system, instance));
                if (systems.Count == 0) removedAtlasKeys.Add(atlasEntry.Key);
            }
            foreach (var atlasKey in removedAtlasKeys)
            {
                TextAtlasSystems.Remove(atlasKey);
            }
            if (removedAtlasKeys.Count == 0) return;
            var emptyFiles = new List<File>();
            foreach (var fileEntry in TextBindings)
            {
                var bindings = fileEntry.Value;
                foreach (var atlasKey in removedAtlasKeys)
                {
                    TextFileBinding binding;
                    if (!bindings.TryGetValue(atlasKey, out binding)) continue;
                    bindings.Remove(atlasKey);
                    if (binding?.AtlasResource != null) onTextureDestroy?.Invoke(binding.AtlasResource);
                }
                if (bindings.Count == 0) emptyFiles.Add(fileEntry.Key);
            }
            foreach (var file in emptyFiles)
            {
                TextBindings.Remove(file);
            }
        }
    }
}
