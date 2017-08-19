/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016 
 */
using System;
using System.Collections.Generic;
using System.Text;
using CrazyStorm.Core;
using System.Reflection;

namespace CrazyStorm.Core
{
    public class PlayDataHelper
    {
        public static byte[] GetBytes(object obj)
        {
            if (obj is bool)
                return BitConverter.GetBytes((bool)obj);
            else if (obj is int || obj is Enum)
                return BitConverter.GetBytes((int)obj);
            else if (obj is float)
                return BitConverter.GetBytes((float)obj);
            else if (obj is Vector2)
                return GetVector2Bytes((Vector2)obj);
            else if (obj is RGB)
                return GetRGBBytes((RGB)obj);
            else if (obj is string)
                return GetStringBytes((string)obj);
            else
                throw new PlayDataException();
        }
        public static byte[] GetVector2Bytes(Vector2 v)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(v.x));
            bytes.AddRange(BitConverter.GetBytes(v.y));
            return bytes.ToArray();
        }
        public static byte[] GetRGBBytes(RGB rgb)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(rgb.r));
            bytes.AddRange(BitConverter.GetBytes(rgb.g));
            bytes.AddRange(BitConverter.GetBytes(rgb.b));
            return bytes.ToArray();
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
        public static List<byte> CreateBlock(List<byte> content)
        {
            List<byte> block = new List<byte>();
            //The header is a integer representing block size.
            block.AddRange(BitConverter.GetBytes(content.Count));
            block.AddRange(content);
            return block;
        }
        public static void GenerateFields(Type type, object source, List<byte> data)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            FieldInfo[] fieldInfos = type.GetFields(flags);
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
        public static void GenerateFields(object source, List<byte> data)
        {
            GenerateFields(source.GetType(), source, data);
        }
        public static void GenerateStruct<T>(T source, List<byte> data)
        {
            var structBytes = new List<byte>();
            FieldInfo[] fieldInfos = source.GetType().GetFields();
            foreach (var info in fieldInfos)
                structBytes.AddRange(GetBytes(info.GetValue(source)));
            
            data.AddRange(CreateBlock(structBytes));
        }
        public static void GenerateObjectList<T>(IList<T> source, List<byte> data)
            where T : IPlayData
        {
            var objectListBytes = new List<byte>();
            foreach (var obj in source)
                objectListBytes.AddRange((obj as IPlayData).GeneratePlayData());

            data.AddRange(CreateBlock(objectListBytes));
        }
    }
}
