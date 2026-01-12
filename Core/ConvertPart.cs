/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CrazyStorm.Core
{
    public partial class File
    {
        static readonly LayerColor[] LayerColorMap = new LayerColor[]
        {
            LayerColor.Blue, LayerColor.Yellow, LayerColor.Pink, LayerColor.Green, LayerColor.Purple, LayerColor.Red, LayerColor.Orange, 
        };
        static readonly Regex RenderingOrderMatch = new Regex(@"RenderingOrder:(\d+)", RegexOptions.Compiled);
        static readonly Regex ExternalMatch = new Regex(@"(?<!\w)(?:[A-Za-z]:\\|\.{1,2}\\)?(?:[^\\/:*?""<>|\r\n]+\\)*[^\\/:*?""<>|\r\n]+\.(png|dat)", RegexOptions.Compiled);
        static readonly Regex TypeCountMatch = new Regex(@"(\d+) Types:", RegexOptions.Compiled);
        static readonly Regex TypeMatch = new Regex(@"^(?<name>[^_]+)_" + 
            @"(?<texid>[^_]+)_" +
            @"(?<rectx>[^_]+)_(?<recty>[^_]+)_(?<rectwidth>[^_]+)_(?<rectheight>[^_]+)_" +
            @"(?<ox>[^_]+)_(?<oy>[^_]+)_" +
            @"(?<pdr>[^_]+)_" +
            @"(?<color>[^_]+)" +
            @"(?:_(?<frames>[^_]+))?(?:_(?<interval>[^_]+))?" +
            @"(?:_(?<volumex>[^_]+))?(?:_(?<volumey>[^_]+))?(?:_(?<volumewidth>[^_]+))?(?:_(?<volumeheight>[^_]+))?(?:_(?<volumejudgearea>[^_]+))?$", RegexOptions.Compiled);
        static readonly Regex GlobalEventCountMatch = new Regex(@"(\d+) GlobalEvents:", RegexOptions.Compiled);
        static readonly Regex GlobalEventMatch = new Regex(@"^(?<frame>[^_]+)_" +
            @"(?<gotocondition>[^_]+)_(?<gotooperator>[^_]+)_(?<gotovalue>[^_]+)_(?<isgoto>[^_]+)_(?<gototime>[^_]+)_(?<gotowhere>[^_]+)_" +
            @"(?<quakecondition>[^_]+)_(?<quakeoperator>[^_]+)_(?<quakevalue>[^_]+)_(?<isquake>[^_]+)_(?<quaketime>[^_]+)_(?<quakelevel>[^_]+)_" +
            @"(?<stopcondition>[^_]+)_(?<stopoperator>[^_]+)_(?<stopvalue>[^_]+)_(?<isstop>[^_]+)_(?<stoptime>[^_]+)_(?<stoplevel>[^_]+)$", RegexOptions.Compiled);
        static readonly Regex SoundCountMatch = new Regex(@"(\d+) Sounds:", RegexOptions.Compiled);
        static readonly Regex SoundMatch = new Regex(@"^(?<typeid>[^_]+)_(?<path>[^_]+)_(?<volume>[^_]+)$", RegexOptions.Compiled);
        static readonly Regex CenterMatch = new Regex(@"^Center:(?:False,(?<events>.*)|" +
            @"(?<x>[^,]+),(?<y>[^,]+)" +
            @"(?:,(?<speed>[^,]+))?(?:,(?<speedd>[^,]+))?(?:,(?<aspeed>[^,]+))?(?:,(?<aspeedd>[^,]+))?,(?<events>.*))$", RegexOptions.Compiled);
        static readonly Vector2 OldCenter = new Vector2(480, 360);
        static readonly List<string> ConditionKeyWords = new List<string>
        {
            "且", "或", "=", ">", "<",
        };
        static readonly List<string> NormalEventKeyWords = new List<string>()
        {
            "变化到", "增加", "减少",
        };
        static readonly Dictionary<string, string> KeywordMap = new Dictionary<string, string>()
        {
            { "当前帧", "CurrentFrame" }, { "且", "&" }, { "或", "|"},
            { "额外发射", "EmitParticle" }, { "恢复", "Recover"},
            { "变化到", "ChangeTo" }, { "增加", "Increase" }, { "减少", "Decrease" },
            { "正比", "Linear" }, { "固定", "Instant"}, { "正弦", "Sin"}, { "无缝正弦", "Sin"},
            { "跟随自机X慢", "FollowBodyXSlow" }, { "跟随自机X", "FollowBodyX" }, { "跟随自机Y慢", "FollowBodyYSlow" }, { "跟随自机Y", "FollowBodyY" },
            { "跟随自机慢", "FollowBodySlow" }, { "跟随自机", "FollowBody" },
            { "范围移动慢", "RandomMoveSlow" }, { "范围移动", "RandomMove" }, { "范围飘动", "RandomWalk" },
            { "子弹X坐标", "PPosition.x" }, { "子弹Y坐标", "PPosition.y" },
            { "X坐标", "Position.x" }, { "Y坐标", "Position.y" }, { "半径方向", "EmitRoundAngle" }, { "半径", "EmitRadius" },
            { "条数", "EmitCount" }, { "周期", "EmitCycle" }, { "角度", "EmitAngle" }, { "范围", "EmitRange" },
            { "子弹加速度方向", "PAcspeedAngle" }, { "子弹加速度", "PAcspeed" }, { "子弹速度方向", "PSpeedAngle" }, { "子弹速度", "PSpeed" },
            { "加速度方向", "AcspeedAngle" }, { "加速度", "Acspeed" }, { "速度方向", "SpeedAngle" }, { "速度", "Speed" },
            { "生命", "MaxLife" }, { "宽比", "WidthScale" }, { "高比", "HeightScale" }, 
            { "R", "RGB.r" }, { "G", "RGB.g" }, { "B", "RGB.b" },
            { "不透明度", "Opacity" }, { "朝向", "PRotation" },
            { "横比", "PSpeedHScale" }, { "纵比", "PSpeedVScale" },
            { "消除效果", "FadeEffect" }, { "拖影效果", "AfterimageEffect" }, { "出屏即消", "KillOutside" }, { "无敌状态", "Collision" },
        };
        static readonly Dictionary<string, string> BoolValueMap = new Dictionary<string, string>()
        {
            { "1", "True" }, { "0", "False" },
        };
        static readonly string TypeKeyword = "类型";
        static readonly string BlendKeyword = "高光效果";
        static readonly Dictionary<string, string> BlendValueMap = new Dictionary<string, string>()
        {
            { "1", "Additive" }, { "0", "AlphaBlend" },
        };
        static readonly Dictionary<string, string> ParticleKeywordMap = new Dictionary<string, string>()
        {
            { "X坐标", "子弹X坐标" }, { "Y坐标", "子弹Y坐标" },
        };
        static readonly Regex LayerMatch = new Regex(@"^Layer(?<num>[^:]+):" +
            @"(?<name>[^,]+),(?<begin>[^,]+),(?<end>[^,]+)," +
            @"(?<batchcount>[^,]+),(?<lasecount>[^,]+),(?<covercount>[^,]+),(?<reboundcount>[^,]+),(?<forcecount>[^,]+)", RegexOptions.Compiled);
        static readonly Regex BatchMatch = new Regex(@"^(?<id>[^,]+),(?<layerid>[^,]+)," +
            @"(?<binding>[^,]+),(?<bindid>[^,]+),(?<bindwithspeedd>[^,]+),," +
            @"(?<x>[^,]+),(?<y>[^,]+),(?<begin>[^,]+),(?<life>[^,]+)," +
            @"(?<fx>[^,]+),(?<fy>[^,]+),(?<r>[^,]+),(?<rdirection>[^,]+),(?<rdirections>[^,]+)," +
            @"(?<tiao>[^,]+),(?<t>[^,]+),(?<fdirection>[^,]+),(?<fdirections>[^,]+),(?<range>[^,]+)," +
            @"(?<speed>[^,]+),(?<speedd>[^,]+),(?<speedds>[^,]+)," +
            @"(?<aspeed>[^,]+),(?<aspeedd>[^,]+),(?<aspeedds>[^,]+)," +
            @"(?<sonlife>[^,]+),(?<typeid>[^,]+),(?<wscale>[^,]+),(?<hscale>[^,]+)," +
            @"(?<colorR>[^,]+),(?<colorG>[^,]+),(?<colorB>[^,]+),(?<alpha>[^,]+)," +
            @"(?<head>[^,]+),(?<heads>[^,]+),(?<withspeedd>[^,]+)," +
            @"(?<sonspeed>[^,]+),(?<sonspeedd>[^,]+),(?<sondspeedds>[^,]+)," +
            @"(?<sonaspeed>[^,]+),(?<sonaspeedd>[^,]+),(?<sondaspeedds>[^,]+)," +
            @"(?<xscale>[^,]+),(?<yscale>[^,]+)," +
            @"(?<mist>[^,]+),(?<dispel>[^,]+),(?<blend>[^,]+),(?<afterimage>[^,]+),(?<outdispel>[^,]+),(?<invincible>[^,]+)," +
            @"(?:(?<events>[^,]+))?,(?:(?<sonevents>[^,]+))?," +
            @"(?<randfx>[^,]+),(?<randfy>[^,]+),(?<randr>[^,]+),(?<randrdirection>[^,]+)," +
            @"(?<randtiao>[^,]+),(?<randt>[^,]+),(?<randfdirection>[^,]+),(?<randrange>[^,]+)," +
            @"(?<randspeed>[^,]+),(?<randspeedd>[^,]+),(?<randaspeed>[^,]+),(?<randaspeedd>[^,]+),(?<randhead>[^,]+)," +
            @"(?<randsonspeed>[^,]+),(?<randsonspeedd>[^,]+),(?<randsonaspeed>[^,]+),(?<randsonaspeedd>[^,]+)" +
            @"(?:,(?<affectedByCover>[^,]+))?(?:,(?<affectedByRebound>[^,]+))?(?:,(?<affectedByForce>[^,]+))?" +
            @"(?:,(?<deepbind>[^,]+))?" +
            @"(?:,(?<randwscale>[^,]+))?(?:,(?<randhscale>[^,]+))?(?:,(?<syncScale>[^,]+))?" +
            @"(?:,(?<instantMovement>[^,]+))?$", RegexOptions.Compiled);
        public static bool IsCS1(string filePath)
        {
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                var head = reader.ReadLine();
                if (head == null) return false;
                return head.Trim() == "Crazy Storm Data 1.01";
            }
        }
        public void ConvertFromCS1(string filePath)
        {
            string line = default;
            Match match = default;
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                //header
                reader.ReadLine();
                match = RenderingOrderMatch.Match(reader.ReadLine());
                //particleSystem
                var particleSystem = new ParticleSystem(Path.GetFileName(filePath));
                if (match.Success)
                {
                    particleSystem.OrderType = (OrderType)int.Parse(match.Groups[1].Value);
                    line = reader.ReadLine().Trim();
                }
                if (line == "External:")
                {
                    //images
                    line = reader.ReadLine().Trim();
                    match = ExternalMatch.Match(line);
                    while (match.Success)
                    {
                        var relativePath = match.Groups[0].Value;
                        var image = new FileResource
                        {
                            ID = FileResourceIndex,
                            Label = Path.GetFileName(relativePath),
                            RelatviePath = relativePath,
                        };
                        image.CheckValid();
                        Images.Add(image);
                        line = reader.ReadLine().Trim();
                        match = ExternalMatch.Match(line);
                    }
                }
                //types
                match = TypeCountMatch.Match(line);
                var typeMap = new ParticleColor[] { 
                    ParticleColor.Red, ParticleColor.Orange, ParticleColor.Yellow, ParticleColor.Green, 
                    ParticleColor.Cyan, ParticleColor.Blue, ParticleColor.Purple, ParticleColor.Gray };
                if (match.Success)
                {
                    var count = int.Parse(match.Groups[1].Value);
                    for (int i = 0; i < count; ++i)
                    {
                        line = reader.ReadLine().Trim();
                        match = TypeMatch.Match(line);
                        if (match.Success)
                        {
                            var particleType = new ParticleType(particleSystem.CustomTypeIndex,
                                match.Groups["name"].Value);
                            var imageId = int.Parse(match.Groups["texid"].Value);
                            particleType.Image = imageId >= 0 ? Images[imageId] : null;
                            particleType.StartPointX = float.Parse(match.Groups["rectx"].Value);
                            particleType.StartPointY = float.Parse(match.Groups["recty"].Value);
                            particleType.Width = int.Parse(match.Groups["rectwidth"].Value);
                            particleType.Height = int.Parse(match.Groups["rectheight"].Value);
                            particleType.CenterPointX = float.Parse(match.Groups["ox"].Value);
                            particleType.CenterPointY = float.Parse(match.Groups["oy"].Value);
                            particleType.Radius = int.Parse(match.Groups["pdr"].Value);
                            particleType.Color = typeMap[int.Parse(match.Groups["color"].Value)];
                            var frames = match.Groups["frames"];
                            particleType.Frames = !frames.Success ? 1 : int.Parse(frames.Value);
                            var delay = match.Groups["interval"];
                            particleType.Delay = !delay.Success ? 0 : int.Parse(delay.Value);
                            var volumex = match.Groups["volumex"];
                            particleType.VolumeStartX = !volumex.Success ? 0 : float.Parse(volumex.Value);
                            var volumey = match.Groups["volumey"];
                            particleType.VolumeStartY = !volumey.Success ? 0 : float.Parse(volumey.Value);
                            var volumewidth = match.Groups["volumewidth"];
                            particleType.VolumeWidth = !volumewidth.Success ? 0 : int.Parse(volumewidth.Value);
                            var volumeheight = match.Groups["volumeheight"];
                            particleType.VolumeHeight = !volumeheight.Success ? 0 : int.Parse(volumeheight.Value);
                            var volumeJudgeArea = match.Groups["volumejudgearea"];
                            particleType.VolumeJudgeArea = !volumeJudgeArea.Success ? 0 : int.Parse(volumeJudgeArea.Value);
                            particleSystem.CustomTypes.Add(particleType);
                        }
                    }
                    if (count > 0) line = reader.ReadLine().Trim();
                }
                //globalevents
                EventGroup globalEvents = null;
                match = GlobalEventCountMatch.Match(line);
                if (match.Success)
                {
                    var count = int.Parse(match.Groups[1].Value);
                    if (count > 0) globalEvents = new EventGroup() { Name = "GlobalEvents" };
                    for (int i = 0; i < count; ++i)
                    {
                        line = reader.ReadLine().Trim();
                        match = GlobalEventMatch.Match(line);
                        if (match.Success)
                        {
                            var currentFrame = int.Parse(match.Groups["frame"].Value);
                            var condition = $"CurrentFrame={currentFrame}";
                            var isgoto = bool.Parse(match.Groups["isgoto"].Value);
                            var isquake = bool.Parse(match.Groups["isquake"].Value);
                            var isstop = bool.Parse(match.Groups["isstop"].Value);
                            if (isgoto)
                            {
                                var gotowhere = int.Parse(match.Groups["gotowhere"].Value);
                                var gototime = int.Parse(match.Groups["gototime"].Value);
                                var eventInfo = new EventInfo();
                                eventInfo.condition = condition;
                                eventInfo.isSpecialEvent = true;
                                eventInfo.specialEvent = "GotoFrame";
                                eventInfo.arguments = $"{gotowhere},{gototime}";
                                globalEvents.Events.Add(EventHelper.BuildEvent(eventInfo, false));
                            }
                            else if (isquake)
                            {
                                var quakelevel = int.Parse(match.Groups["quakelevel"].Value);
                                var quaketime = int.Parse(match.Groups["quaketime"].Value);
                                var eventInfo = new EventInfo();
                                eventInfo.condition = condition;
                                eventInfo.isSpecialEvent = true;
                                eventInfo.specialEvent = "QuakeScreen";
                                eventInfo.arguments = $"{quakelevel},{quaketime}";
                                globalEvents.Events.Add(EventHelper.BuildEvent(eventInfo, false));
                            }
                            else if (isstop)
                            {
                                var stoplevel = int.Parse(match.Groups["stoplevel"].Value);
                                var stoptime = int.Parse(match.Groups["stoptime"].Value);
                                var eventInfo = new EventInfo();
                                eventInfo.condition = condition;
                                eventInfo.isSpecialEvent = true;
                                eventInfo.specialEvent = "StopScreen";
                                eventInfo.arguments = $"{stoplevel},{stoptime}";
                                globalEvents.Events.Add(EventHelper.BuildEvent(eventInfo, false));
                            }
                        }
                    }
                    if (globalEvents != null) line = reader.ReadLine().Trim();
                }
                //sounds
                match = SoundCountMatch.Match(line);
                if (match.Success)
                {
                    var count = int.Parse(match.Groups[1].Value);
                    for (int i = 0; i < count; ++i)
                    {
                        line = reader.ReadLine().Trim();
                        match = SoundMatch.Match(line);
                        if (match.Success)
                        {
                            var typeid = int.Parse(match.Groups["typeid"].Value) - 1;
                            var relativePath = match.Groups["path"].Value;
                            var sound = new FileResource
                            {
                                ID = FileResourceIndex,
                                Label = Path.GetFileName(relativePath),
                                RelatviePath = relativePath,
                            };
                            sound.CheckValid();
                            Sounds.Add(sound);
                            particleSystem.TypeSoundMap.Add(typeid, sound.ID);
                        }
                    }
                    if (count > 0) line = reader.ReadLine().Trim();
                }
                //center
                match = CenterMatch.Match(line);
                var center = match.Success ? new Center() : null;
                center.Name = "0";
                center.ID = particleSystem.GetComponentIndex();
                particleSystem.GetAndIncreaseComponentIndex(center.GetType().ToString());
                if (center != null)
                {
                    center.Visibility = match.Groups["x"].Success;
                    center.ComponentEventGroups.Add(globalEvents);
                    var eventgroup = GetEventGroup(typeof(Center), match.Groups["events"].Value, false);
                    if (eventgroup.Events.Count > 0)
                    {
                        eventgroup.Name = "CenterEvents";
                        center.ComponentEventGroups.Add(eventgroup);
                    }
                    line = reader.ReadLine().Trim();
                }
                if (center.Visibility)
                {
                    center.X = float.Parse(match.Groups["x"].Value) - OldCenter.x;
                    center.Y = float.Parse(match.Groups["y"].Value) - OldCenter.y;
                    center.Speed = float.Parse(match.Groups["speed"].Value);
                    center.SpeedAngle = float.Parse(match.Groups["speedd"].Value);
                    center.Acspeed = float.Parse(match.Groups["aspeed"].Value);
                    center.AcspeedAngle = float.Parse(match.Groups["aspeedd"].Value);
                }
                //totalframe
                if (line.StartsWith("Totalframe:"))
                {
                    line = reader.ReadLine().Trim();
                }
                //layers
                do
                {
                    match = LayerMatch.Match(line);
                    if (match.Success)
                    {
                        var layer = new Layer(match.Groups["name"].Value);
                        layer.Color = LayerColorMap[int.Parse(match.Groups["num"].Value) - 1];
                        layer.BeginFrame = int.Parse(match.Groups["begin"].Value) - 1;
                        layer.TotalFrame = int.Parse(match.Groups["end"].Value) - layer.BeginFrame;
                        var batchCount = int.Parse(match.Groups["batchcount"].Value);
                        for (int i = 0; i < batchCount; ++i)
                        {
                            //batchs
                            line = reader.ReadLine().Trim();
                            match = BatchMatch.Match(line);
                            if (!match.Success) continue;
                            var batch = new MultiEmitter();
                            batch.Name = match.Groups["id"].Value;
                            batch.ID = particleSystem.GetComponentIndex();
                            particleSystem.GetAndIncreaseComponentIndex(batch.GetType().ToString());
                            batch.ParentID = center.ID;
                            batch.Position = ConvertVector2(float.Parse(match.Groups["x"].Value), float.Parse(match.Groups["y"].Value),
                                0, 0, batch, "Position") - OldCenter;
                            batch.BeginFrame = int.Parse(match.Groups["begin"].Value);
                            batch.TotalFrame = int.Parse(match.Groups["life"].Value);
                            batch.EmitPosition = ConvertVector2(float.Parse(match.Groups["fx"].Value), float.Parse(match.Groups["fy"].Value),
                                float.Parse(match.Groups["randfx"].Value), float.Parse(match.Groups["randfy"].Value), batch, "EmitPosition") - OldCenter;
                            batch.EmitRadius = ConvertFloat(float.Parse(match.Groups["r"].Value), float.Parse(match.Groups["randr"].Value),
                                batch, "EmitRadius");
                            batch.EmitRoundAngle = ConvertAngle(float.Parse(match.Groups["rdirection"].Value), float.Parse(match.Groups["randrdirection"].Value), 
                                batch, "EmitRoundAngle");
                            batch.EmitCount = ConvertInt(int.Parse(match.Groups["tiao"].Value), int.Parse(match.Groups["randtiao"].Value),
                                batch, "EmitCount");
                            batch.EmitCycle = ConvertInt(int.Parse(match.Groups["t"].Value), int.Parse(match.Groups["randt"].Value),
                                batch, "EmitCycle");
                            batch.EmitAngle = ConvertAngle(float.Parse(match.Groups["fdirection"].Value), float.Parse(match.Groups["randfdirection"].Value), 
                                batch, "EmitAngle");
                            batch.EmitRange = ConvertInt(int.Parse(match.Groups["range"].Value), int.Parse(match.Groups["randrange"].Value),
                                batch, "EmitRange");
                            batch.Speed = ConvertFloat(float.Parse(match.Groups["speed"].Value), float.Parse(match.Groups["randspeed"].Value),
                                batch, "Speed");
                            batch.SpeedAngle = ConvertAngle(float.Parse(match.Groups["speedd"].Value), float.Parse(match.Groups["randspeedd"].Value),
                                batch, "SpeedAngle");
                            batch.Acspeed = ConvertFloat(float.Parse(match.Groups["aspeed"].Value), float.Parse(match.Groups["randaspeed"].Value),
                                batch, "Acspeed");
                            batch.AcspeedAngle = ConvertAngle(float.Parse(match.Groups["aspeedd"].Value), float.Parse(match.Groups["randaspeedd"].Value),
                                batch, "AcspeedAngle");
                            batch.InstantMovement = match.Groups["instantmovement"].Success ? bool.Parse(match.Groups["instantmovement"].Value) : false;
                            var particle = batch.Particle as Particle;
                            particle.MaxLife = int.Parse(match.Groups["sonlife"].Value);
                            var typeId = int.Parse(match.Groups["typeid"].Value);
                            if (typeId < ParticleType.DefaultTypes.Count)
                            {
                                particle.Type = ParticleType.DefaultTypes[typeId];
                            }
                            else if (typeId < particleSystem.CustomTypes.Count)
                            {
                                particle.Type = particleSystem.CustomTypes[typeId];
                            }
                            particle.WidthScale = ConvertFloat(float.Parse(match.Groups["wscale"].Value), match.Groups["randwscale"].Success ?
                                float.Parse(match.Groups["randwscale"].Value) : 0f, particle, "WidthScale");
                            particle.HeightScale = ConvertFloat(float.Parse(match.Groups["hscale"].Value), match.Groups["randhscale"].Success ?
                                float.Parse(match.Groups["randhscale"].Value) : 0f, particle, "HeightScale");
                            particle.RetainScale = match.Groups["syncScale"].Success ? bool.Parse(match.Groups["syncScale"].Value) : true;
                            particle.RGB = new RGB(float.Parse(match.Groups["colorR"].Value),
                                float.Parse(match.Groups["colorG"].Value), float.Parse(match.Groups["colorB"].Value));
                            particle.Opacity = float.Parse(match.Groups["alpha"].Value);
                            particle.PRotation = ConvertAngle(float.Parse(match.Groups["head"].Value), float.Parse(match.Groups["randhead"].Value),
                                particle, "PRotation");
                            particle.StickToSpeedAngle = bool.Parse(match.Groups["withspeedd"].Value);
                            particle.PSpeed = ConvertFloat(float.Parse(match.Groups["sonspeed"].Value), float.Parse(match.Groups["randsonspeed"].Value),
                                particle, "PSpeed");
                            particle.PSpeedAngle = ConvertAngle(float.Parse(match.Groups["sonspeedd"].Value), float.Parse(match.Groups["randsonspeedd"].Value),
                                particle, "PSpeedAngle");
                            particle.PAcspeed = ConvertFloat(float.Parse(match.Groups["sonaspeed"].Value), float.Parse(match.Groups["randsonaspeed"].Value),
                                particle, "PAcspeed");
                            particle.PAcspeedAngle = ConvertAngle(float.Parse(match.Groups["sonaspeedd"].Value), float.Parse(match.Groups["randsonaspeedd"].Value),
                                particle, "PAcspeedAngle");
                            particle.PSpeedHScale = float.Parse(match.Groups["xscale"].Value);
                            particle.PSpeedVScale = float.Parse(match.Groups["yscale"].Value);
                            particle.FadeEffect = bool.Parse(match.Groups["dispel"].Value);
                            particle.BlendType = bool.Parse(match.Groups["blend"].Value) ? 
                                BlendType.Additive : BlendType.AlphaBlend;
                            particle.AfterimageEffect = bool.Parse(match.Groups["afterimage"].Value);
                            particle.KillOutside = bool.Parse(match.Groups["outdispel"].Value);
                            particle.Collision = bool.Parse(match.Groups["invincible"].Value);
                            particle.IgnoreMask = match.Groups["affectedByCover"].Success ? !bool.Parse(match.Groups["affectedByCover"].Value) : false;
                            particle.IgnoreRebound = match.Groups["affectedByRebound"].Success ? !bool.Parse(match.Groups["affectedByRebound"].Value) : false;
                            particle.IgnoreForce = match.Groups["affectedByForce"].Success ? !bool.Parse(match.Groups["affectedByForce"].Value) : false;
                            //events
                            var eventGroups = GetEventGroups(typeof(MultiEmitter), match.Groups["events"].Value, false);
                            foreach (var eventGroup in eventGroups) batch.ComponentEventGroups.Add(eventGroup);
                            //sonevents
                            eventGroups = GetEventGroups(typeof(Particle), match.Groups["sonevents"].Value, true);
                            foreach (var eventGroup in eventGroups) batch.ParticleEventGroups.Add(eventGroup);
                            layer.Components.Add(batch);
                        }
                        //binding
                        //TODO : Binding
                        if (particleSystem.Layers.Count == 0)
                        {
                            layer.Components.Add(center);
                        }
                        particleSystem.Layers.Add(layer);
                    }
                    line = reader.ReadLine().Trim();
                }
                while (!reader.EndOfStream);
                ParticleSystems.Add(particleSystem);
            }
        }
        public static int ConvertInt(int v, int rand, PropertyContainer propertyContainer, string name)
        {
            if (rand != 0)
            {
                propertyContainer.Properties[name] = new PropertyValue()
                {
                    Value = $"{v}+{{{rand}}}",
                    Expression = true,
                };
                return 0;
            }
            else return v;
        }
        public static float ConvertFloat(float v, float rand, PropertyContainer propertyContainer, string name)
        {
            if (rand != 0f)
            {
                propertyContainer.Properties[name] = new PropertyValue()
                {
                    Value = $"{v}+{{{rand}}}",
                    Expression = true,
                };
                return 0f;
            }
            else return v;
        }
        public static Vector2 ConvertVector2(float x, float y, float randx, float randy, PropertyContainer propertyContainer, string name)
        {
            string randXStr = randx.ToString();
            if (randx != 0f) randXStr = $"{{{randx}}}";
            string randYStr = randy.ToString();
            if (randy != 0f) randYStr = $"{{{randy}}}";
            if (x == -99998 && y == -99998)
            {
                string randStr = (randx != 0f || randy != 0f) ? $"+[{randXStr},{randYStr}]" : "";
                propertyContainer.Properties[name] = new PropertyValue()
                {
                    Value = $"Position{randStr}",
                    Expression = true,
                };
                return Vector2.Zero;
            }
            else if (x == -99999 && y == -99999)
            {
                string randStr = (randx != 0f || randy != 0f) ? $"+[{randXStr},{randYStr}]" : "";
                propertyContainer.Properties[name] = new PropertyValue()
                {
                    Value = $"BodyPosition{randStr}",
                    Expression = true,
                };
                return Vector2.Zero;
            }
            else if (x <= -99998 || y <= -99998)
            {
                randXStr = randx != 0f ? $"+{randXStr}" : "";
                randYStr = randy != 0f ? $"+{randYStr}" : "";
                propertyContainer.Properties[name] = new PropertyValue()
                {
                    Value = $"[{(x == -99998 ? "Position.x" : x == -99999 ? "BodyPosition.x" : x.ToString())}{randXStr}," +
                    $"{(y == -99998 ? "Position.y" : y == -99999 ? "BodyPosition.y" : y.ToString())}{randYStr}]",
                    Expression = true,
                };
                return Vector2.Zero;
            }
            else if (randx != 0f || randy != 0f)
            {
                string randStr = $"+[{randXStr},{randYStr}]";
                propertyContainer.Properties[name] = new PropertyValue()
                {
                    Value = $"[{x},{y}]{randStr}",
                    Expression = true,
                };
                return Vector2.Zero;
            }
            else return new Vector2(x, y);
        }
        public static float ConvertAngle(float deg, float rand, PropertyContainer propertyContainer, string name)
        {
            string randStr = rand != 0f ? $"+{{{rand}}}" : "";
            if (deg == -100000)
            {
                //This expression is not implemented, just ignore it
                return 0;
            }
            else if (deg == -99999)
            {
                propertyContainer.Properties[name] = new PropertyValue
                {
                    Value = $"BodyAngle{randStr}",
                    Expression = true,
                };
                return 0;
            }
            else if (deg == -99998)
            {
                propertyContainer.Properties[name] = new PropertyValue
                {
                    Value = $"SelfAngle{randStr}",
                    Expression = true,
                };
                return 0;
            }
            else if (rand != 0f)
            {
                propertyContainer.Properties[name] = new PropertyValue
                {
                    Value = $"{deg}{randStr}",
                    Expression = true,
                };
                return 0;
            }
            else return deg;
        }
        public static string ConvertKeyword(string str)
        {
            foreach (var word in KeywordMap)
            {
                if (str == word.Key)
                {
                    str = str.Replace(word.Key, word.Value);
                }
            }
            return str;
        }
        public static string ConvertCondition(string str)
        {
            foreach (var keyword in ConditionKeyWords)
            {
                if (str.Contains(keyword)) str = str.Replace(keyword, $" {keyword} ");
            }
            var split = str.Split(' ');
            str = "";
            for (int i = 0; i < split.Length; ++i)
            {
                str += ConvertKeyword(split[i]);
            }
            return str;
        }
        public static string ConvertParticleEventProperty(string str)
        {
            foreach (var word in ParticleKeywordMap)
            {
                if (str == word.Key)
                {
                    str = str.Replace(word.Key, word.Value);
                }
            }
            return str;
        }
        public static string ConvertSpecialValue(PropertyType type, string value, out bool expressionResult)
        {
            expressionResult = false;
            switch (value)
            {
                case "自身":
                    expressionResult = true;
                    if (type == PropertyType.Single) return "SelfAngle";
                    else if (type == PropertyType.Vector2) return "Position";
                    break;
                case "自机":
                    expressionResult = true;
                    if (type == PropertyType.Single) return "BodyAngle";
                    else if (type == PropertyType.Vector2) return "BodyPosition";
                    break;
                case "中心":
                    expressionResult = true;
                    if (type == PropertyType.Single) return "CenterAngle";
                    else if (type == PropertyType.Vector2) return "CenterPosition";
                    break;
            }
            if (type == PropertyType.Boolean && BoolValueMap.ContainsKey(value)) return BoolValueMap[value];
            if (value.Contains("+"))
            {
                var split = value.Split('+');
                return $"{split[0]}+{{{split[1]}}}";
            }
            else return value;
        }
        public static bool IsSpecialEvent(string str)
        {
            foreach (var keyword in NormalEventKeyWords)
            {
                if (str.Contains(keyword)) return false;
            }
            return true;
        }
        public static string AdjustEventText(string str)
        {
            foreach (var keyword in NormalEventKeyWords)
            {
                if (str.Contains(keyword)) return str.Replace(keyword, $" {keyword} ");
            }
            return str;
        }
        public static EventGroup GetEventGroup(Type componentType, string str, bool particleEvents)
        {
            EventGroup group = new EventGroup();
            var split = str.Split('|');
            if (split.Length >= 4)
            {
                group.Name = split[0];
                var t = int.Parse(split[1]);
                var addtime = int.Parse(split[2]);
            }
            var events = split.Length >= 4 ? split[3].Split(';') : str.Split(';');
            foreach (var e in events)
            {
                if (string.IsNullOrEmpty(e)) continue;
                split = e.Split('：');
                //TODO : Process t and addtime
                var condition = split[0];
                var content = split[1];
                var eventInfo = new EventInfo();
                eventInfo.condition = ConvertCondition(condition);
                if (IsSpecialEvent(content))
                {
                    eventInfo.isSpecialEvent = true;
                    if (!content.Contains('(')) split = content.Split('，');
                    else split = content.Split('(');
                    if (particleEvents) split[0] = ConvertParticleEventProperty(split[0]);
                    eventInfo.specialEvent = ConvertKeyword(split[0]);
                    eventInfo.arguments = ConvertKeyword(content.Replace($"{split[0]}(", "").Replace(")", "")
                        .Replace($"{split[0]}，", "").Replace(split[0], "").Replace("，", ","));
                }
                else
                {
                    split = content.Split('，');
                    eventInfo.changeMode = ConvertKeyword(split[1]);
                    eventInfo.changeTime = split[2].Replace("帧", "");
                    if (eventInfo.changeTime.Contains("(") && eventInfo.changeTime.EndsWith(")"))
                    {
                        //Event execution time is not supported
                        eventInfo.changeTime = eventInfo.changeTime.Split('(')[0];
                    }
                    split = AdjustEventText(split[0]).Split(' ');
                    if (particleEvents) split[0] = ConvertParticleEventProperty(split[0]);
                    eventInfo.resultProperty = ConvertKeyword(split[0]);
                    if (eventInfo.resultProperty == TypeKeyword)
                    {
                        //TODO : Increase/Decrease Type
                        eventInfo.isSpecialEvent = true;
                        eventInfo.specialEvent = "ChangeType";
                        eventInfo.arguments = "0,0";
                    }
                    else
                    {
                        if (eventInfo.resultProperty == BlendKeyword)
                        {
                            eventInfo.resultValue = BlendValueMap[eventInfo.resultValue];
                        }
                        eventInfo.resultType = PropertyTypeRule.GetValueType(componentType, eventInfo.resultProperty);
                        eventInfo.changeType = ConvertKeyword(split[1]);
                        eventInfo.resultValue = ConvertSpecialValue(eventInfo.resultType, ConvertKeyword(split[2]), out eventInfo.isExpressionResult);
                    }
                }
                var eventText = EventHelper.BuildEvent(eventInfo, !eventInfo.isSpecialEvent);
                group.Events.Add(eventText);
            }
            return group;
        }
        public static List<EventGroup> GetEventGroups(Type componentType, string str, bool particleEvents)
        {
            var split = str.Split('&');
            var groups = new List<EventGroup>();
            foreach (var groupStr in split)
            {
                if (string.IsNullOrEmpty(groupStr)) continue;
                groups.Add(GetEventGroup(componentType, groupStr, particleEvents));
            }
            return groups;
        }
    }
}
