/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using File = CrazyStorm.Core.File;

namespace CrazyStorm_Player
{
    sealed class FontTextManager
    {
        sealed class TextFileBinding
        {
            public FileResource AtlasResource { get; set; }
            public int AtlasVersion { get; set; }
        }

        static readonly Dictionary<File, Dictionary<TextAtlasKey, TextFileBinding>> TextBindings =
            new Dictionary<File, Dictionary<TextAtlasKey, TextFileBinding>>();
        static readonly Dictionary<ParticleSystem, Dictionary<TextCharacterKey, ParticleType>> TextCharacterTypes =
            new Dictionary<ParticleSystem, Dictionary<TextCharacterKey, ParticleType>>();
        static readonly Dictionary<TextAtlasKey, List<ParticleSystem>> TextAtlasSystems =
            new Dictionary<TextAtlasKey, List<ParticleSystem>>();

        public static void UpdateTextResources(File file, ParticleSystem instance, Text text, 
            Dictionary<ParticleSystem, File> instanceMap, Action<int, byte[]> onTextureUpdate)
        {
            try
            {
                var result = FontHelper.EnsureTextAtlas(text.FontFamily, text.FontFace, text.TextValue, text.CharsetPixelSize);
                if (result == null || result.AtlasKey == null || result.AtlasPngBytes == null ||
                    result.GlyphMap == null || result.Characters == null)
                {
                    throw new InvalidDataException();
                }
                RegisterTextAtlasUsage(result.AtlasKey, instance);
                EnsureTextFileBinding(file, instance, result, onTextureUpdate);
                if (result.AtlasUpdated) UpdateTextAtlas(result, instanceMap, onTextureUpdate);
                text.ApplyRuntimeResources(ResolveTextCharacterTypes(file, instance, result));
            }
            catch
            {
                text.ClearRuntimeResources();
            }
        }

        public static void Clear()
        {
            TextBindings.Clear();
            TextCharacterTypes.Clear();
            TextAtlasSystems.Clear();
        }
        public static void Clear(ParticleSystem instance)
        {
            //TODO
        }

        static void RegisterTextAtlasUsage(TextAtlasKey atlasKey, ParticleSystem instance)
        {
            List<ParticleSystem> systems;
            if (!TextAtlasSystems.TryGetValue(atlasKey, out systems))
            {
                systems = new List<ParticleSystem>();
                TextAtlasSystems[atlasKey] = systems;
            }
            if (!systems.Contains(instance)) systems.Add(instance);
        }

        static void UpdateTextAtlas(TextBuildResult result, Dictionary<ParticleSystem, File> instanceMap, 
            Action<int, byte[]> onTextureUpdate)
        {
            List<ParticleSystem> systems;
            if (!TextAtlasSystems.TryGetValue(result.AtlasKey, out systems)) return;
            foreach (var system in systems)
            {
                File boundFile;
                if (!instanceMap.TryGetValue(system, out boundFile)) continue;

                var binding = EnsureTextFileBinding(boundFile, system, result, onTextureUpdate);
                if (binding == null || binding.AtlasResource == null) continue;
                UpdateTextCharacterTypes(system, result, binding.AtlasResource);
            }
        }

        static TextFileBinding EnsureTextFileBinding(File file, ParticleSystem system, TextBuildResult result,
             Action<int, byte[]> onTextureUpdate)
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
            onTextureUpdate?.Invoke(binding.AtlasResource.ID, result.AtlasPngBytes);
            binding.AtlasVersion = result.AtlasVersion;
            return binding;
        }

        static void UpdateTextCharacterTypes(ParticleSystem instance, TextBuildResult result, FileResource atlasResource)
        {
            Dictionary<TextCharacterKey, ParticleType> characterTypes;
            if (!TextCharacterTypes.TryGetValue(instance, out characterTypes)) return;
            foreach (var item in characterTypes)
            {
                if (!Equals(item.Key.AtlasKey, result.AtlasKey)) continue;
                FontHelper.UpdateCharacterType(item.Value, atlasResource, item.Key.Character, result.GlyphMap);
            }
        }

        static List<ParticleType> ResolveTextCharacterTypes(File file, ParticleSystem instance, TextBuildResult result)
        {
            Dictionary<TextCharacterKey, ParticleType> characterTypes;
            if (!TextCharacterTypes.TryGetValue(instance, out characterTypes))
            {
                characterTypes = new Dictionary<TextCharacterKey, ParticleType>();
                TextCharacterTypes[instance] = characterTypes;
            }

            var atlasResource = TextBindings[file][result.AtlasKey].AtlasResource;
            var resolvedTypes = new List<ParticleType>(result.Characters.Count);
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
                FontHelper.UpdateCharacterType(particleType, atlasResource, character, result.GlyphMap);
                resolvedTypes.Add(particleType);
            }
            return resolvedTypes;
        }
    }
}
