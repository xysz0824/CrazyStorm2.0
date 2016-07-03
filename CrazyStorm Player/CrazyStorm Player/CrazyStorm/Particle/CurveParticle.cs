/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2016
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    class CurveParticle : ParticleBase
    {
        public int Length { get; set; }
        public override void LoadPlayData(BinaryReader reader)
        {
            base.LoadPlayData(reader);
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
                case "Length":
                    VM.PushInt(Length);
                    return true;
            }
            return false;
        }
        public override bool SetProperty(string propertyName)
        {
            if (base.SetProperty(propertyName))
                return true;

            switch (propertyName)
            {
                case "Length":
                    Length = VM.PopInt();
                    return true;
            }
            return false;
        }
        public override void Update()
        {
            base.Update();
            //TODO
        }
        public override void CopyTo(ParticleBase particleBase)
        {
            base.CopyTo(particleBase);
            var curveParticle = particleBase as CurveParticle;
            curveParticle.Length = Length;
        }
    }
}
