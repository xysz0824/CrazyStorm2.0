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
    class CurveParticle : ParticleBase
    {
        public int Length { get; set; }
        public override void LoadPlayData(BinaryReader reader, float version)
        {
            base.LoadPlayData(reader, version);
            using (BinaryReader curveParticleReader = PlayDataHelper.GetBlockReader(reader))
            {
                using (BinaryReader dataReader = PlayDataHelper.GetBlockReader(curveParticleReader))
                {
                    Length = dataReader.ReadInt32();
                }
            }
        }
        public override bool PushProperty(string propertyName)
        {
            if (base.PushProperty(propertyName))
                return true;

            switch (propertyName)
            {
#if GENERATE_SNIPPET
                case "Length":
                    VM.PushInt(Length);
                    return true;
#endif
            }
            return false;
        }
        public override bool SetProperty(string propertyName)
        {
            if (base.SetProperty(propertyName))
                return true;

            switch (propertyName)
            {
#if GENERATE_SNIPPET
                case "Length":
                    Length = VM.PopInt();
                    return true;
#endif
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
            //TODO
        }
    }
}
