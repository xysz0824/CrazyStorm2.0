/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.Text;
using CrazyStorm.Core;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;

namespace CrazyStorm.Core
{
    public class PlayDataHelper
    {
        static byte[] GetBytes(object obj)
        {
            if (obj is bool)
                return BitConverter.GetBytes((bool)obj);
            else if (obj is int || obj is Enum)
                return BitConverter.GetBytes((int)obj);
            else if (obj is float)
                return BitConverter.GetBytes((float)obj);
            else if (obj is string)
                return GetStringBytes((string)obj);
            else
                throw new PlayDataException();
        }
        public static byte[] GetStringBytes(string s)
        {
            List<byte> bytes = new List<byte>();
            byte[] unicodeBytes = Encoding.Unicode.GetBytes(s);
            unicodeBytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, unicodeBytes);
            bytes.AddRange(unicodeBytes);
            bytes.Add(0);
            return bytes.ToArray();
        }
        public unsafe static byte[] GetStructBytes<T>(T s) where T : unmanaged
        {
            int size = sizeof(T);
            Span<byte> bytes = stackalloc byte[size];
            MemoryMarshal.Write(bytes, ref s);
            return bytes.ToArray();
        }
        public static List<byte> CreateBlock(List<byte> content)
        {
            List<byte> block = new List<byte>();
            //The header is a integer representing block size.
            block.AddRange(BitConverter.GetBytes(content.Count));
            block.AddRange(content);
            return block;
        }
        public static List<byte> CreateBlock(byte[] content)
        {
            List<byte> block = new List<byte>();
            //The header is a integer representing block size.
            block.AddRange(BitConverter.GetBytes(content.Length));
            block.AddRange(content);
            return block;
        }
        public static void GeneratePlayDataFields(object source, List<byte> data)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var fieldInfos = source.GetType().GetFields(flags).OrderBy(f => f.MetadataToken);
            foreach (var info in fieldInfos)
            {
                object[] attributes = info.GetCustomAttributes(false);
                for (int i = 0; i < attributes.Length; ++i)
                {
                    if (attributes[i] is PlayDataAttribute)
                    {
                        data.AddRange(GetBytes(info.GetValue(source)));
                        break;
                    }
                }
            }
        }
        public unsafe static void GenerateStruct<T>(T source, List<byte> data) where T : unmanaged
        {
            data.AddRange(CreateBlock(GetStructBytes(source)));
        }
        public static void GenerateObjectList<T>(IList<T> source, List<byte> data)
            where T : IGeneratePlayData
        {
            var objectListBytes = new List<byte>();
            foreach (var obj in source)
                objectListBytes.AddRange((obj as IGeneratePlayData).GeneratePlayData());

            data.AddRange(CreateBlock(objectListBytes));
        }
        static object ReadBytes(Type type, BinaryReader reader)
        {
            if (type == typeof(bool))
                return reader.ReadBoolean();
            else if (type == typeof(int) || type.IsEnum)
                return reader.ReadInt32();
            else if (type == typeof(float))
                return reader.ReadSingle();
            else if (type == typeof(string))
                return ReadString(reader);
            else
                throw new PlayDataException();
        }
        public static void ReadPlayDataFields(object source, BinaryReader reader)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var fieldInfos = source.GetType().GetFields(flags).OrderBy(f => f.MetadataToken);
            foreach (var info in fieldInfos)
            {
                object[] attributes = info.GetCustomAttributes(false);
                for (int i = 0; i < attributes.Length; ++i)
                {
                    if (attributes[i] is PlayDataAttribute)
                    {
                        info.SetValue(source, ReadBytes(info.FieldType, reader));
                        break;
                    }
                }
            }
        }
        public static T ReadStructBytes<T>(byte[] bytes, int startIndex) where T : unmanaged
        {
            return MemoryMarshal.Read<T>(bytes.AsSpan(startIndex));
        }
        public static T ReadStruct<T>(BinaryReader reader) where T : unmanaged
        {
            var bytes = GetBlock(reader);
            return MemoryMarshal.Read<T>(bytes.AsSpan());
        }
        public static string ReadString(BinaryReader reader)
        {
            var bytes = new List<byte>();
            while (true)
            {
                byte stringByte = reader.ReadByte();
                if (stringByte != '\0') bytes.Add(stringByte);
                else break;
            }
            return Encoding.UTF8.GetString(bytes.ToArray());
        }
        public static string ReadString(byte[] bytes, int startIndex)
        {
            List<byte> stringBytes = new List<byte>();
            while (true)
            {
                byte stringByte = bytes[startIndex++];
                if (stringByte != '\0') stringBytes.Add(stringByte);
                else break;
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
        public static void ReadObjectList<T>(IList<T> source, BinaryReader reader, float version)
            where T : ILoadPlayData, new()
        {
            using (BinaryReader objectListReader = GetBlockReader(reader))
            {
                while (!EndOfReader(objectListReader))
                {
                    T obj = new T();
                    (obj as ILoadPlayData).LoadPlayData(objectListReader, version);
                    source.Add(obj);
                }
            }
        }
    }
}
