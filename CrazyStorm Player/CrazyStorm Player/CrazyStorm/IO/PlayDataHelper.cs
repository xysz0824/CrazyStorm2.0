/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using CrazyStorm.Core;

namespace CrazyStorm_Player.CrazyStorm
{
    class PlayDataHelper
    {
        public static Vector2 ReadVector2(BinaryReader reader)
        {
            var vector = new Vector2();
            vector.x = reader.ReadSingle();
            vector.y = reader.ReadSingle();
            return vector;
        }
        public static RGB ReadRGB(BinaryReader reader)
        {
            var rgb = new RGB();
            rgb.r = reader.ReadSingle();
            rgb.g = reader.ReadSingle();
            rgb.b = reader.ReadSingle();
            return rgb;
        }
        public static T ReadEnum<T>(BinaryReader reader)
        {
            return (T)(object)reader.ReadInt32();
        }
        public static string ReadString(BinaryReader reader)
        {
            var bytes = new List<byte>();
            while (true)
            {
                byte stringByte = reader.ReadByte();
                if (stringByte != '\0')
                    bytes.Add(stringByte);
                else
                    break;
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }
        public static string ReadString(byte[] bytes, int startIndex)
        {
            List<byte> stringBytes = new List<byte>();
            while (true)
            {
                byte stringByte = bytes[startIndex++];
                if (stringByte != '\0')
                    stringBytes.Add(stringByte);
                else
                    break;
            }
            return Encoding.UTF8.GetString(stringBytes.ToArray());
        }
        public static byte[] GetBlock(BinaryReader reader)
        {
            int size = reader.ReadInt32();
            return reader.ReadBytes(size);
        }
        public static BinaryReader GetBlockReader(BinaryReader reader)
        {
            MemoryStream stream = new MemoryStream(GetBlock(reader));
            return new BinaryReader(stream);
        }
        public static bool EndOfReader(BinaryReader reader)
        {
            return reader.BaseStream.Position == reader.BaseStream.Length;
        }
        public static void LoadObjectList<T>(IList<T> source, BinaryReader reader)
            where T : IPlayData , new()
        {
            using (BinaryReader objectListReader = GetBlockReader(reader))
            {
                while (!EndOfReader(objectListReader))
                {
                    T obj = new T();
                    (obj as IPlayData).LoadPlayData(objectListReader);
                    source.Add(obj);
                }
            }
        }
    }
}
