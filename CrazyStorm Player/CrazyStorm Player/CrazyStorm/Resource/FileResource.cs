/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    class FileResource : Resource
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public override void LoadPlayData(BinaryReader reader)
        {
            base.LoadPlayData(reader);
            using (BinaryReader fileResourceReader = PlayDataHelper.GetBlockReader(reader))
            {
                Id = fileResourceReader.ReadInt32();
                Path = PlayDataHelper.ReadString(fileResourceReader);
            }
        }
    }
}
