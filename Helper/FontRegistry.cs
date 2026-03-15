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

namespace CrazyStorm
{
    public static class FontRegistry
    {
        static readonly object syncRoot = new object();
        static Dictionary<string, string> fontMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
                fontMap = map;
                fontNames = map.Keys.OrderBy(item => item, StringComparer.CurrentCultureIgnoreCase).ToList();
                initialized = true;
            }
        }

        public static bool TryGetFontPath(string fontName, out string path)
        {
            EnsureInitialized();
            return fontMap.TryGetValue(fontName ?? string.Empty, out path);
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
    }
}
