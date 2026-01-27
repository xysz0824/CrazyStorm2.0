/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Expression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CrazyStorm.Core
{
    public enum ConvertEventType
    {
        Component,
        Particle,
        CoverParticle,
    }
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
        static readonly List<string> LogicOperatorKeywords = new List<string> { "且", "或" };
        static readonly List<string> CompareOperatorKeywords = new List<string> { "=", ">", "<" };
        static readonly List<string> ChangeTypeKeywords = new List<string>() { "变化到", "增加", "减少" };
        static readonly Regex StatusMatch = new Regex(@"^状态(?<status>[0-9]+)$", RegexOptions.Compiled);
        static readonly Dictionary<string, string> KeywordMap = new Dictionary<string, string>()
        {
            { "进入遮罩瞬间", "PMasked" }, { "状态帧", "StatusFrame" }, { "状态", "Status"},
            { "子弹图层帧", "PLayerFrame" }, { "子弹当前帧", "PCurrentFrame" }, { "当前帧", "CurrentFrame" }, { "且", "&" }, { "或", "|"},
            { "额外发射", "EmitParticle" }, { "恢复", "Recover"},
            { "变化到", "ChangeTo" }, { "增加", "Increase" }, { "减少", "Decrease" },
            { "正比", "Linear" }, { "固定", "Instant"}, { "正弦", "Sin"}, { "无缝正弦", "Sin"},
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
        static readonly KeyValuePair<string, string> BlendKeyMap = new KeyValuePair<string, string>("高光效果", "BlendType");
        static readonly Dictionary<string, string> BlendValueMap = new Dictionary<string, string>()
        {
            { "1", "Additive" }, { "0", "AlphaBlend" },
        };
        static readonly Dictionary<string, string> ParticleKeywordMap = new Dictionary<string, string>()
        {
            { "当前帧", "子弹当前帧" }, { "X坐标", "子弹X坐标" }, { "Y坐标", "子弹Y坐标" },
        };
        static readonly Dictionary<string, string> CoverParticleKeywordMap = new Dictionary<string, string>()
        {
            { "当前帧", "子弹图层帧" }, { "X坐标", "子弹X坐标" }, { "Y坐标", "子弹Y坐标" },
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
        static readonly Regex LaseMatch = new Regex(@"^(?<id>[^,]+),(?<layerid>[^,]+)," +
            @"(?<binding>[^,]+),(?<bindid>[^,]+),(?<bindwithspeedd>[^,]+),," +
            @"(?<x>[^,]+),(?<y>[^,]+),(?<begin>[^,]+),(?<life>[^,]+)," +
            @"(?<r>[^,]+),(?<rdirection>[^,]+),(?<rdirections>[^,]+)," +
            @"(?<tiao>[^,]+),(?<t>[^,]+),(?<fdirection>[^,]+),(?<fdirections>[^,]+),(?<range>[^,]+)," +
            @"(?<speed>[^,]+),(?<speedd>[^,]+),(?<speedds>[^,]+)," +
            @"(?<aspeed>[^,]+),(?<aspeedd>[^,]+),(?<aspeedds>[^,]+)," +
            @"(?<sonlife>[^,]+),(?<typeid>[^,]+),(?<wscale>[^,]+),(?<longs>[^,]+)," +
            @"(?<alpha>[^,]+),(?<shape>[^,]+)," +
            @"(?<sonspeed>[^,]+),(?<sonspeedd>[^,]+),(?<sondspeedds>[^,]+)," +
            @"(?<sonaspeed>[^,]+),(?<sonaspeedd>[^,]+),(?<sondaspeedds>[^,]+)," +
            @"(?<xscale>[^,]+),(?<yscale>[^,]+)," +
            @"(?<blend>[^,]+),(?<outdispel>[^,]+),(?<invincible>[^,]+)," +
            @"(?<segment>[^,]+)," +
            @"(?:(?<events>[^,]+))?,(?:(?<sonevents>[^,]+))?," +
            @"(?<randr>[^,]+),(?<randrdirection>[^,]+)," +
            @"(?<randtiao>[^,]+),(?<randt>[^,]+),(?<randfdirection>[^,]+),(?<randrange>[^,]+)," +
            @"(?<randspeed>[^,]+),(?<randspeedd>[^,]+),(?<randaspeed>[^,]+),(?<randaspeedd>[^,]+)," +
            @"(?<randsonspeed>[^,]+),(?<randsonspeedd>[^,]+),(?<randsonaspeed>[^,]+),(?<randsonaspeedd>[^,]+)" +
            @"(?:,(?<deepbind>[^,]+))?" +
            @"(?:,(?<colorR>[^,]+))?(?:,(?<colorG>[^,]+))?(?:,(?<colorB>[^,]+))?" +
            @"(?:,(?<vspeed>[^,]+))?$", RegexOptions.Compiled);
        static readonly Regex CoverMatch = new Regex(@"^(?<id>[^,]+),(?<layerid>[^,]+)," +
            @"(?<x>[^,]+),(?<y>[^,]+),(?<begin>[^,]+),(?<life>[^,]+)," +
            @"(?<halfw>[^,]+),(?<halfh>[^,]+),(?<circle>[^,]+),(?<type>[^,]+),(?<controlid>[^,]+)," +
            @"(?<speed>[^,]+),(?<speedd>[^,]+),(?<speedds>[^,]+)," +
            @"(?<aspeed>[^,]+),(?<aspeedd>[^,]+),(?<aspeedds>[^,]+)," +
            @"(?:(?<events>[^,]+))?,(?:(?<sonevents>[^,]+))?," +
            @"(?<randspeed>[^,]+),(?<randspeedd>[^,]+),(?<randaspeed>[^,]+),(?<randaspeedd>[^,]+)" +
            @"(?:,(?<bindid>[^,]+))?(?:,(?<deepbind>[^,]+))?" +
            @"(?:,(?<maskon>[^,]+))?(?:,(?<masktype>[^,]+))?(?:,(?<maskmutex>[^,]+))?" +
            @"(?:,(?<degree>[^,]+))?$", RegexOptions.Compiled);
        static readonly Regex ReboundMatch = new Regex(@"^(?<id>[^,]+),(?<layerid>[^,]+)," +
            @"(?<x>[^,]+),(?<y>[^,]+),(?<begin>[^,]+),(?<life>[^,]+)," +
            @"(?<longs>[^,]+),(?<angle>[^,]+),(?<time>[^,]+)," +
            @"(?<speed>[^,]+),(?<speedd>[^,]+)," +
            @"(?<aspeed>[^,]+),(?<aspeedd>[^,]+)," +
            @"(?:(?<events>[^,]+))?," +
            @"(?<randspeed>[^,]+),(?<randspeedd>[^,]+),(?<randaspeed>[^,]+),(?<randaspeedd>[^,]+)" +
            @"(?:,(?<oneside>[^,]+))?$", RegexOptions.Compiled);
        static readonly Regex ForceMatch = new Regex(@"^(?<id>[^,]+),(?<layerid>[^,]+)," +
            @"(?<x>[^,]+),(?<y>[^,]+),(?<begin>[^,]+),(?<life>[^,]+)," +
            @"(?<halfw>[^,]+),(?<halfh>[^,]+),(?<circle>[^,]+),(?<type>[^,]+),(?<controlid>[^,]+)," +
            @"(?<speed>[^,]+),(?<speedd>[^,]+)," +
            @"(?<aspeed>[^,]+),(?<aspeedd>[^,]+)," +
            @"(?<addaspeed>[^,]+),(?<addaspeedd>[^,]+)," +
            @"(?<suction>[^,]+),(?<replusion>[^,]+),(?<addspeed>[^,]+)," +
            @"(?<randspeed>[^,]+),(?<randspeedd>[^,]+),(?<randaspeed>[^,]+),(?<randaspeedd>[^,]+)" +
            @"(?:,(?<bindid>[^,]+))?(?:,(?<deepbind>[^,]+))?$", RegexOptions.Compiled);
        public static readonly string DefaultCenterName = "Center";
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
                var particleSystem = new ParticleSystem(Path.GetFileNameWithoutExtension(filePath));
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
                            var typeid = int.Parse(match.Groups["typeid"].Value);
                            if (typeid < ParticleType.DefaultTypes.Count) typeid += ParticleType.DefaultTypeIndex;
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
                center.Name = DefaultCenterName;
                center.ID = particleSystem.GetComponentIndex();
                particleSystem.GetAndIncreaseComponentIndex(center.GetType().ToString());
                if (center != null)
                {
                    center.Visibility = match.Groups["x"].Success;
                    if (globalEvents != null) center.ComponentEventGroups.Add(globalEvents);
                    var eventgroup = GetEventGroup(typeof(Center), null, match.Groups["events"].Value, ConvertEventType.Component, "CenterEvents");
                    if (eventgroup.Events.Count > 0) center.ComponentEventGroups.Add(eventgroup);
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
                        layer.BeginFrame = int.Parse(match.Groups["begin"].Value);
                        layer.TotalFrame = int.Parse(match.Groups["end"].Value) - layer.BeginFrame + 1;
                        var batchCount = int.Parse(match.Groups["batchcount"].Value);
                        var batchs = new List<MultiEmitter>();
                        for (int i = 0; i < batchCount; ++i)
                        {
                            //batchs
                            line = reader.ReadLine().Trim();
                            var submatch = BatchMatch.Match(line);
                            if (!submatch.Success) continue;
                            var batch = new MultiEmitter();
                            batch.Name = (float.Parse(submatch.Groups["id"].Value) + 1).ToString();
                            batch.BindingTargetID = int.Parse(submatch.Groups["bindid"].Value);
                            batch.ID = particleSystem.GetComponentIndex();
                            particleSystem.GetAndIncreaseComponentIndex(batch.GetType().ToString());
                            batch.ParentID = center.ID;
                            batch.Position = ConvertVector2(float.Parse(submatch.Groups["x"].Value), float.Parse(submatch.Groups["y"].Value), 
                                0, 0, batch, "Position", OldCenter + center.Position);
                            batch.BeginFrame = int.Parse(submatch.Groups["begin"].Value);
                            batch.TotalFrame = int.Parse(submatch.Groups["life"].Value);
                            batch.EmitPosition = ConvertVector2(float.Parse(submatch.Groups["fx"].Value), float.Parse(submatch.Groups["fy"].Value),
                                float.Parse(submatch.Groups["randfx"].Value), float.Parse(submatch.Groups["randfy"].Value), batch, "EmitPosition", OldCenter);
                            batch.EmitRadius = ConvertFloat(float.Parse(submatch.Groups["r"].Value), float.Parse(submatch.Groups["randr"].Value),
                                batch, "EmitRadius");
                            batch.EmitRoundAngle = ConvertAngle(float.Parse(submatch.Groups["rdirection"].Value), float.Parse(submatch.Groups["randrdirection"].Value), 
                                batch, "EmitRoundAngle");
                            batch.EmitCount = ConvertInt(int.Parse(submatch.Groups["tiao"].Value), int.Parse(submatch.Groups["randtiao"].Value),
                                batch, "EmitCount");
                            batch.EmitCycle = ConvertInt(int.Parse(submatch.Groups["t"].Value), int.Parse(submatch.Groups["randt"].Value),
                                batch, "EmitCycle");
                            batch.EmitAngle = ConvertAngle(float.Parse(submatch.Groups["fdirection"].Value), float.Parse(submatch.Groups["randfdirection"].Value), 
                                batch, "EmitAngle");
                            batch.EmitRange = ConvertInt(int.Parse(submatch.Groups["range"].Value), int.Parse(submatch.Groups["randrange"].Value),
                                batch, "EmitRange");
                            batch.Speed = ConvertFloat(float.Parse(submatch.Groups["speed"].Value), float.Parse(submatch.Groups["randspeed"].Value),
                                batch, "Speed");
                            batch.SpeedAngle = ConvertAngle(float.Parse(submatch.Groups["speedd"].Value), float.Parse(submatch.Groups["randspeedd"].Value),
                                batch, "SpeedAngle");
                            batch.Acspeed = ConvertFloat(float.Parse(submatch.Groups["aspeed"].Value), float.Parse(submatch.Groups["randaspeed"].Value),
                                batch, "Acspeed");
                            batch.AcspeedAngle = ConvertAngle(float.Parse(submatch.Groups["aspeedd"].Value), float.Parse(submatch.Groups["randaspeedd"].Value),
                                batch, "AcspeedAngle");
                            batch.InstantMovement = submatch.Groups["instantmovement"].Success ? bool.Parse(submatch.Groups["instantmovement"].Value) : false;
                            var particle = batch.Particle as Particle;
                            particle.MaxLife = int.Parse(submatch.Groups["sonlife"].Value);
                            var typeId = int.Parse(submatch.Groups["typeid"].Value);
                            if (typeId < ParticleType.DefaultTypes.Count) particle.Type = ParticleType.DefaultTypes[typeId];
                            else if (typeId - ParticleType.DefaultTypes.Count < particleSystem.CustomTypes.Count)
                            {
                                particle.Type = particleSystem.CustomTypes[typeId - ParticleType.DefaultTypes.Count];
                            }
                            particle.WidthScale = ConvertFloat(float.Parse(submatch.Groups["wscale"].Value), submatch.Groups["randwscale"].Success ?
                                float.Parse(submatch.Groups["randwscale"].Value) : 0f, particle, "WidthScale");
                            particle.HeightScale = ConvertFloat(float.Parse(submatch.Groups["hscale"].Value), submatch.Groups["randhscale"].Success ?
                                float.Parse(submatch.Groups["randhscale"].Value) : 0f, particle, "HeightScale");
                            particle.RetainScale = submatch.Groups["syncScale"].Success ? bool.Parse(submatch.Groups["syncScale"].Value) : false;
                            particle.RGB = new RGB(float.Parse(submatch.Groups["colorR"].Value),
                                float.Parse(submatch.Groups["colorG"].Value), float.Parse(submatch.Groups["colorB"].Value));
                            particle.Opacity = float.Parse(submatch.Groups["alpha"].Value);
                            particle.PRotation = ConvertAngle(float.Parse(submatch.Groups["head"].Value), float.Parse(submatch.Groups["randhead"].Value),
                                particle, "PRotation");
                            particle.StickToSpeedAngle = bool.Parse(submatch.Groups["withspeedd"].Value);
                            particle.PSpeed = ConvertFloat(float.Parse(submatch.Groups["sonspeed"].Value), float.Parse(submatch.Groups["randsonspeed"].Value),
                                particle, "PSpeed");
                            particle.PSpeedAngle = ConvertAngle(float.Parse(submatch.Groups["sonspeedd"].Value), float.Parse(submatch.Groups["randsonspeedd"].Value),
                                particle, "PSpeedAngle");
                            particle.PAcspeed = ConvertFloat(float.Parse(submatch.Groups["sonaspeed"].Value), float.Parse(submatch.Groups["randsonaspeed"].Value),
                                particle, "PAcspeed");
                            particle.PAcspeedAngle = ConvertAngle(float.Parse(submatch.Groups["sonaspeedd"].Value), float.Parse(submatch.Groups["randsonaspeedd"].Value),
                                particle, "PAcspeedAngle");
                            particle.PSpeedHScale = float.Parse(submatch.Groups["xscale"].Value);
                            particle.PSpeedVScale = float.Parse(submatch.Groups["yscale"].Value);
                            particle.FogEffect = bool.Parse(submatch.Groups["mist"].Value);
                            particle.FadeEffect = bool.Parse(submatch.Groups["dispel"].Value);
                            particle.BlendType = bool.Parse(submatch.Groups["blend"].Value) ? 
                                BlendType.Additive : BlendType.AlphaBlend;
                            particle.AfterimageEffect = bool.Parse(submatch.Groups["afterimage"].Value);
                            particle.KillOutside = bool.Parse(submatch.Groups["outdispel"].Value);
                            particle.Collision = !bool.Parse(submatch.Groups["invincible"].Value);
                            particle.IgnoreMask = submatch.Groups["affectedByCover"].Success ? !bool.Parse(submatch.Groups["affectedByCover"].Value) : false;
                            particle.IgnoreRebound = submatch.Groups["affectedByRebound"].Success ? !bool.Parse(submatch.Groups["affectedByRebound"].Value) : false;
                            particle.IgnoreForce = submatch.Groups["affectedByForce"].Success ? !bool.Parse(submatch.Groups["affectedByForce"].Value) : false;
                            //events
                            var eventGroups = GetEventGroups(typeof(MultiEmitter), typeof(Particle), submatch.Groups["events"].Value, ConvertEventType.Component, "");
                            foreach (var eventGroup in eventGroups) batch.ComponentEventGroups.Add(eventGroup);
                            //sonevents
                            eventGroups = GetEventGroups(typeof(Particle), null, submatch.Groups["sonevents"].Value, ConvertEventType.Particle, "");
                            foreach (var eventGroup in eventGroups) batch.ParticleEventGroups.Add(eventGroup);
                            layer.Components.Add(batch);
                            batchs.Add(batch);
                        }
                        var laseCount = int.Parse(match.Groups["lasecount"].Value);
                        for (int i = 0; i < laseCount; ++i)
                        {
                            //lases
                            line = reader.ReadLine().Trim();
                            var submatch = LaseMatch.Match(line);
                            if (!submatch.Success) continue;
                            var lase = new CurveEmitter();
                            lase.Name = (float.Parse(submatch.Groups["id"].Value) + 1).ToString();
                            lase.BindingTargetID = int.Parse(submatch.Groups["bindid"].Value);
                            lase.ID = particleSystem.GetComponentIndex();
                            particleSystem.GetAndIncreaseComponentIndex(lase.GetType().ToString());
                            lase.ParentID = center.ID;
                            lase.Position = ConvertVector2(float.Parse(submatch.Groups["x"].Value), float.Parse(submatch.Groups["y"].Value), 
                                0, 0, lase, "Position", OldCenter + center.Position);
                            lase.BeginFrame = int.Parse(submatch.Groups["begin"].Value);
                            lase.TotalFrame = int.Parse(submatch.Groups["life"].Value);
                            lase.EmitRadius = ConvertFloat(float.Parse(submatch.Groups["r"].Value), float.Parse(submatch.Groups["randr"].Value),
                                lase, "EmitRadius");
                            lase.EmitRoundAngle = ConvertAngle(float.Parse(submatch.Groups["rdirection"].Value), float.Parse(submatch.Groups["randrdirection"].Value),
                                lase, "EmitRoundAngle");
                            lase.EmitCount = ConvertInt(int.Parse(submatch.Groups["tiao"].Value), int.Parse(submatch.Groups["randtiao"].Value),
                                lase, "EmitCount");
                            lase.EmitCycle = ConvertInt(int.Parse(submatch.Groups["t"].Value), int.Parse(submatch.Groups["randt"].Value),
                                lase, "EmitCycle");
                            lase.EmitAngle = ConvertAngle(float.Parse(submatch.Groups["fdirection"].Value), float.Parse(submatch.Groups["randfdirection"].Value),
                                lase, "EmitAngle");
                            lase.EmitRange = ConvertInt(int.Parse(submatch.Groups["range"].Value), int.Parse(submatch.Groups["randrange"].Value),
                                lase, "EmitRange");
                            lase.Speed = ConvertFloat(float.Parse(submatch.Groups["speed"].Value), float.Parse(submatch.Groups["randspeed"].Value),
                                lase, "Speed");
                            lase.SpeedAngle = ConvertAngle(float.Parse(submatch.Groups["speedd"].Value), float.Parse(submatch.Groups["randspeedd"].Value),
                                lase, "SpeedAngle");
                            lase.Acspeed = ConvertFloat(float.Parse(submatch.Groups["aspeed"].Value), float.Parse(submatch.Groups["randaspeed"].Value),
                                lase, "Acspeed");
                            lase.AcspeedAngle = ConvertAngle(float.Parse(submatch.Groups["aspeedd"].Value), float.Parse(submatch.Groups["randaspeedd"].Value),
                                lase, "AcspeedAngle");
                            var particle = lase.Particle as CurveParticle;
                            particle.MaxLife = int.Parse(submatch.Groups["sonlife"].Value);
                            var typeId = int.Parse(submatch.Groups["typeid"].Value);
                            if (typeId < ParticleType.DefaultTypes.Count) particle.Type = ParticleType.DefaultTypes[typeId];
                            else if (typeId - ParticleType.DefaultTypes.Count < particleSystem.CustomTypes.Count)
                            {
                                particle.Type = particleSystem.CustomTypes[typeId - ParticleType.DefaultTypes.Count];
                            }
                            particle.WidthScale = float.Parse(submatch.Groups["wscale"].Value);
                            particle.Length = int.Parse(submatch.Groups["longs"].Value);
                            particle.Opacity = float.Parse(submatch.Groups["alpha"].Value);
                            particle.CurveType = (CurveType)Enum.Parse(typeof(CurveType), submatch.Groups["shape"].Value);
                            if (particle.CurveType == CurveType.Ray)
                            {
                                particle.Length = 792;
                                particle.Properties.Remove("Length");
                            }
                            particle.VSpeed = submatch.Groups["vspeed"].Success ? float.Parse(submatch.Groups["vspeed"].Value) : 0f;
                            //events
                            var eventGroups = GetEventGroups(typeof(CurveEmitter), typeof(CurveParticle), submatch.Groups["events"].Value, ConvertEventType.Component, "");
                            foreach (var eventGroup in eventGroups) lase.ComponentEventGroups.Add(eventGroup);
                            //sonevents
                            eventGroups = GetEventGroups(typeof(CurveParticle), null,submatch.Groups["sonevents"].Value, ConvertEventType.Particle, "");
                            foreach (var eventGroup in eventGroups) lase.ParticleEventGroups.Add(eventGroup);
                            layer.Components.Add(lase);
                        }
                        var coverCount = int.Parse(match.Groups["covercount"].Value);
                        for (int i = 0; i < coverCount; ++i)
                        {
                            //cover
                            line = reader.ReadLine().Trim();
                            var submatch = CoverMatch.Match(line);
                            if (!submatch.Success) continue;
                            var cover = new EventField();
                            cover.Name = (float.Parse(submatch.Groups["id"].Value) + 1).ToString();
                            cover.BindingTargetID = int.Parse(submatch.Groups["bindid"].Value);
                            cover.ID = particleSystem.GetComponentIndex();
                            particleSystem.GetAndIncreaseComponentIndex(cover.GetType().ToString());
                            cover.ParentID = center.ID;
                            cover.Position = ConvertVector2(float.Parse(submatch.Groups["x"].Value), float.Parse(submatch.Groups["y"].Value),
                                0, 0, cover, "Position", OldCenter + center.Position);
                            cover.BeginFrame = int.Parse(submatch.Groups["begin"].Value);
                            cover.TotalFrame = int.Parse(submatch.Groups["life"].Value);
                            cover.HalfWidth = int.Parse(submatch.Groups["halfw"].Value);
                            cover.HalfHeight = int.Parse(submatch.Groups["halfh"].Value);
                            cover.FieldShape = bool.Parse(submatch.Groups["circle"].Value) ? FieldShape.Circle : FieldShape.Rectangle;
                            cover.Reach = int.Parse(submatch.Groups["type"].Value) == 0 ? Reach.All : Reach.Name;
                            cover.TargetName = cover.Reach == Reach.Name ? submatch.Groups["controlid"].Value : "";
                            cover.Speed = ConvertFloat(float.Parse(submatch.Groups["speed"].Value), float.Parse(submatch.Groups["randspeed"].Value),
                                cover, "Speed");
                            cover.SpeedAngle = ConvertAngle(float.Parse(submatch.Groups["speedd"].Value), float.Parse(submatch.Groups["randspeedd"].Value),
                                cover, "SpeedAngle");
                            cover.Acspeed = ConvertFloat(float.Parse(submatch.Groups["aspeed"].Value), float.Parse(submatch.Groups["randaspeed"].Value),
                                cover, "Acspeed");
                            cover.AcspeedAngle = ConvertAngle(float.Parse(submatch.Groups["aspeedd"].Value), float.Parse(submatch.Groups["randaspeedd"].Value),
                                cover, "AcspeedAngle");
                            cover.LayerMask = submatch.Groups["maskon"].Success ? bool.Parse(submatch.Groups["maskon"].Value) : false;
                            cover.LayerMaskType = submatch.Groups["masktype"].Success ? (LayerMaskType)int.Parse(submatch.Groups["masktype"].Value) : default;
                            cover.LayerMaskMutex = submatch.Groups["maskmutex"].Success ? bool.Parse(submatch.Groups["maskmutex"].Value) : false;
                            cover.Rotation = submatch.Groups["degree"].Success ? float.Parse(submatch.Groups["degree"].Value) : 0f;
                            //events
                            var eventGroups = GetEventGroups(typeof(EventField), typeof(ParticleBase), submatch.Groups["events"].Value, ConvertEventType.Component, "");
                            foreach (var eventGroup in eventGroups) cover.ComponentEventGroups.Add(eventGroup);
                            //sonevents
                            eventGroups = GetEventGroups(typeof(ParticleBase), null,submatch.Groups["sonevents"].Value, ConvertEventType.CoverParticle, "");
                            foreach (var eventGroup in eventGroups) cover.EventFieldEventGroups.Add(eventGroup);
                            GetFrameConditionRange(eventGroups, out int minFrame, out int maxFrame);
                            cover.BeginFrame = Math.Min(cover.BeginFrame, minFrame);
                            cover.TotalFrame = Math.Max(cover.TotalFrame, maxFrame - cover.BeginFrame + 1);
                            layer.BeginFrame = Math.Min(layer.BeginFrame, cover.BeginFrame);
                            layer.TotalFrame = Math.Max(layer.TotalFrame, cover.BeginFrame + cover.TotalFrame - layer.BeginFrame);
                            layer.Components.Add(cover);
                        }
                        var reboundCount = int.Parse(match.Groups["reboundcount"].Value);
                        for (int i = 0; i < reboundCount; ++i)
                        {
                            //rebound
                            line = reader.ReadLine().Trim();
                            var submatch = ReboundMatch.Match(line);
                            if (!submatch.Success) continue;
                            var rebound = new Rebounder();
                            rebound.Name = (float.Parse(submatch.Groups["id"].Value) + 1).ToString();
                            rebound.ID = particleSystem.GetComponentIndex();
                            particleSystem.GetAndIncreaseComponentIndex(rebound.GetType().ToString());
                            rebound.ParentID = center.ID;
                            rebound.Position = ConvertVector2(float.Parse(submatch.Groups["x"].Value), float.Parse(submatch.Groups["y"].Value), 
                                0, 0, rebound, "Position", OldCenter + center.Position);
                            rebound.BeginFrame = int.Parse(submatch.Groups["begin"].Value);
                            rebound.TotalFrame = int.Parse(submatch.Groups["life"].Value);
                            rebound.Size = int.Parse(submatch.Groups["longs"].Value);
                            rebound.Rotation = int.Parse(submatch.Groups["angle"].Value);
                            rebound.ReboundLimit = int.Parse(submatch.Groups["time"].Value);
                            rebound.Speed = ConvertFloat(float.Parse(submatch.Groups["speed"].Value), float.Parse(submatch.Groups["randspeed"].Value),
                                rebound, "Speed");
                            rebound.SpeedAngle = ConvertAngle(float.Parse(submatch.Groups["speedd"].Value), float.Parse(submatch.Groups["randspeedd"].Value),
                                rebound, "SpeedAngle");
                            rebound.Acspeed = ConvertFloat(float.Parse(submatch.Groups["aspeed"].Value), float.Parse(submatch.Groups["randaspeed"].Value),
                                rebound, "Acspeed");
                            rebound.AcspeedAngle = ConvertAngle(float.Parse(submatch.Groups["aspeedd"].Value), float.Parse(submatch.Groups["randaspeedd"].Value),
                                rebound, "AcspeedAngle");
                            rebound.ReboundOneSide = submatch.Groups["oneside"].Success ? bool.Parse(submatch.Groups["oneside"].Value) : false;
                            //events
                            var eventGroups = GetEventGroups(typeof(ParticleBase), null, submatch.Groups["events"].Value, ConvertEventType.Particle, "ReboundGroups");
                            foreach (var eventGroup in eventGroups) rebound.RebounderEventGroups.Add(eventGroup);
                            layer.Components.Add(rebound);
                        }
                        var forcecount = int.Parse(match.Groups["forcecount"].Value);
                        for (int i = 0; i < forcecount; ++i)
                        {
                            //force
                            line = reader.ReadLine().Trim();
                            var submatch = ForceMatch.Match(line);
                            if (!submatch.Success) continue;
                            var force = new ForceField();
                            force.Name = (float.Parse(submatch.Groups["id"].Value) + 1).ToString();
                            force.BindingTargetID = int.Parse(submatch.Groups["bindid"].Value);
                            force.ID = particleSystem.GetComponentIndex();
                            particleSystem.GetAndIncreaseComponentIndex(force.GetType().ToString());
                            force.ParentID = center.ID;
                            force.Position = ConvertVector2(float.Parse(submatch.Groups["x"].Value), float.Parse(submatch.Groups["y"].Value), 
                                0, 0, force, "Position", OldCenter + center.Position);
                            force.BeginFrame = int.Parse(submatch.Groups["begin"].Value);
                            force.TotalFrame = int.Parse(submatch.Groups["life"].Value);
                            force.HalfWidth = int.Parse(submatch.Groups["halfw"].Value);
                            force.HalfHeight = int.Parse(submatch.Groups["halfh"].Value);
                            force.FieldShape = bool.Parse(submatch.Groups["circle"].Value) ? FieldShape.Circle : FieldShape.Rectangle;
                            force.Reach = int.Parse(submatch.Groups["type"].Value) == 0 ? Reach.All : Reach.Name;
                            force.TargetName = force.Reach == Reach.Name ? submatch.Groups["controlid"].Value : "";
                            force.Speed = ConvertFloat(float.Parse(submatch.Groups["speed"].Value), float.Parse(submatch.Groups["randspeed"].Value),
                                force, "Speed");
                            force.SpeedAngle = ConvertAngle(float.Parse(submatch.Groups["speedd"].Value), float.Parse(submatch.Groups["randspeedd"].Value),
                                force, "SpeedAngle");
                            force.Acspeed = ConvertFloat(float.Parse(submatch.Groups["aspeed"].Value), float.Parse(submatch.Groups["randaspeed"].Value),
                                force, "Acspeed");
                            force.AcspeedAngle = ConvertAngle(float.Parse(submatch.Groups["aspeedd"].Value), float.Parse(submatch.Groups["randaspeedd"].Value),
                                force, "AcspeedAngle");
                            force.Force = float.Parse(submatch.Groups["addaspeed"].Value);
                            force.Direction = float.Parse(submatch.Groups["addaspeedd"].Value);
                            force.ForceType = bool.Parse(submatch.Groups["suction"].Value) ? ForceType.InnerForce :
                                bool.Parse(submatch.Groups["replusion"].Value) ? ForceType.OuterForce : ForceType.OneDirection;
                            force.ForceImpactSpeed = float.Parse(submatch.Groups["addspeed"].Value);
                            layer.Components.Add(force);
                        }
                        //binding
                        foreach (var component in layer.Components)
                        {
                            if (component.BindingTargetID == -1) continue;
                            component.BindingTarget = batchs.Find((emitter) => emitter.Name == (component.BindingTargetID + 1).ToString());
                            component.BindingTargetID = -1;
                        }
                        if (particleSystem.Layers.Count == 0)
                        {
                            layer.Components.Add(center);
                        }
                        particleSystem.Layers.Add(layer);
                    }
                    line = reader.ReadLine()?.Trim();
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
        public static Vector2 ConvertVector2(float x, float y, float randx, float randy, PropertyContainer propertyContainer, string name,
            Vector2 offset)
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
                    Value = $"[{(x == -99998 ? "Position.x" : x == -99999 ? "BodyPosition.x" : (x - offset.x).ToString())}{randXStr}," +
                    $"{(y == -99998 ? "Position.y" : y == -99999 ? "BodyPosition.y" : (y - offset.y).ToString())}{randYStr}]",
                    Expression = true,
                };
                return Vector2.Zero;
            }
            else if (randx != 0f || randy != 0f)
            {
                string randStr = $"+[{randXStr},{randYStr}]";
                propertyContainer.Properties[name] = new PropertyValue()
                {
                    Value = $"[{x - offset.x},{y - offset.y}]{randStr}",
                    Expression = true,
                };
                return Vector2.Zero;
            }
            else return new Vector2(x, y) - offset;
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
        public static string ConvertCondition(string str, int t, int addtime, ConvertEventType eventType)
        {
            foreach (var keyword in LogicOperatorKeywords)
            {
                if (str.Contains(keyword)) str = str.Replace(keyword, $" {keyword} ");
            }
            foreach (var keyword in CompareOperatorKeywords)
            {
                if (str.Contains(keyword)) str = str.Replace(keyword, $" {keyword} ");
            }
            var split = str.Split(' ');
            str = "";
            var match = StatusMatch.Match(split[0]);
            if (match.Success)
            {
                var status = match.Groups["status"].Value;
                str = $"Status={status}&StatusFrame{split[1]}{split[2]}";
            }
            else
            {
                for (int i = 0; i < split.Length; ++i)
                {
                    if (eventType == ConvertEventType.Particle) split[i] = ConvertEventProperty(ParticleKeywordMap, split[i]);
                    else if (eventType == ConvertEventType.CoverParticle) split[i] = ConvertEventProperty(CoverParticleKeywordMap, split[i]);
                    str += ConvertKeyword(split[i]);
                    if (t > 0 && addtime > 1 && i >= 1 && CompareOperatorKeywords.Exists((op) => op == split[i - 1]))
                    {
                        if (eventType == ConvertEventType.Particle) str += $"+PCurrentFrame/{t}*{addtime}";
                        else if (eventType == ConvertEventType.CoverParticle) str += $"+PLayerFrame/{t}*{addtime}";
                        else str += $"+CurrentFrame/{t}*{addtime}";
                    }
                }
            }
            return str;
        }
        public static string ConvertEventProperty(Dictionary<string, string> map, string str)
        {
            foreach (var word in map)
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
                expressionResult = true;
                var split = value.Split('+');
                return $"{split[0]}+{{{split[1]}}}";
            }
            else return value;
        }
        public static bool IsSpecialEvent(string str)
        {
            foreach (var keyword in ChangeTypeKeywords)
            {
                if (str.Contains(keyword)) return false;
            }
            return true;
        }
        public static string AdjustEventText(string str)
        {
            foreach (var keyword in ChangeTypeKeywords)
            {
                if (str.Contains(keyword)) return str.Replace(keyword, $" {keyword} ");
            }
            return str;
        }
        public static void GetFrameConditionRange(List<EventGroup> groups, out int min, out int max)
        {
            min = int.MaxValue;
            max = int.MinValue;
            foreach (var group in groups)
            {
                foreach (var e in group.Events)
                {
                    var info = EventHelper.SplitEvent(e);
                    var lexer = new Lexer();
                    lexer.Load(info.condition);
                    var syntaxTree = new Parser(lexer).Expression() as BinaryExpression;
                    if (syntaxTree == null) continue;
                    if (syntaxTree.GetLeftChild() is BinaryExpression && syntaxTree.GetRightChild() is BinaryExpression)
                    {
                        var left = syntaxTree.GetLeftChild() as BinaryExpression;
                        var right = syntaxTree.GetRightChild() as BinaryExpression;
                        if (left.GetLeftChild() is Name && right.GetLeftChild() is Name)
                        {
                            var leftName = left.GetLeftChild() as Name;
                            if (leftName.ToString().EndsWith("LayerFrame"))
                            {
                               var value = (float)left.GetRightChild().Eval(null);
                                if (value < min) min = (int)value;
                                if (value > max) max = (int)value;
                            }
                            var rightName = right.GetLeftChild() as Name;
                            if (rightName.ToString().EndsWith("LayerFrame"))
                            {
                                var value = (float)right.GetRightChild().Eval(null);
                                if (value < min) min = (int)value;
                                if (value > max) max = (int)value;
                            }
                        }
                    }
                    else if (syntaxTree.GetLeftChild() is Name)
                    {
                        var name = syntaxTree.GetLeftChild() as Name;
                        if (name.ToString().EndsWith("LayerFrame"))
                        {
                            var value = (float)syntaxTree.GetRightChild().Eval(null);
                            if (value < min) min = (int)value;
                            if (value > max) max = (int)value;
                        }
                    }
                }
            }
        }
        public static EventGroup GetEventGroup(Type type, Type subType, string str, ConvertEventType eventType, string defaultName)
        {
            EventGroup group = new EventGroup();
            var split = str.Split('|');
            int t = 1, addtime = 0;
            group.Name = defaultName;
            if (split.Length >= 4)
            {
                group.Name = split[0];
                t = int.Parse(split[1]);
                addtime = int.Parse(split[2]);
            }
            var events = split.Length >= 4 ? split[3].Split(';') : str.Split(';');
            foreach (var e in events)
            {
                if (string.IsNullOrEmpty(e)) continue;
                split = e.Split('：');
                var condition = split.Length >= 2 ? split[0] : "";
                var content = split.Length >= 2 ? split[1] : split[0];
                var eventInfo = new EventInfo();
                eventInfo.condition = ConvertCondition(condition, t, addtime, eventType);
                if (IsSpecialEvent(content))
                {
                    eventInfo.isSpecialEvent = true;
                    if (!content.Contains('(')) split = content.Split('，');
                    else split = content.Split('(');
                    if (eventType == ConvertEventType.Particle) split[0] = ConvertEventProperty(ParticleKeywordMap, split[0]);
                    else if (eventType == ConvertEventType.CoverParticle) split[0] = ConvertEventProperty(CoverParticleKeywordMap, split[0]);
                    eventInfo.specialEvent = ConvertKeyword(split[0]);
                    eventInfo.arguments = ConvertKeyword(content.Replace($"{split[0]}(", "").Replace(")", "")
                        .Replace($"{split[0]}，", "").Replace(split[0], "").Replace("，", ","));
                }
                else
                {
                    split = content.Split('，');
                    eventInfo.changeMode = split.Length >= 2 ? ConvertKeyword(split[1]) : "Instant";
                    eventInfo.changeTime = split.Length >= 3 ? split[2].Replace("帧", "") : "1";
                    if (eventInfo.changeTime.Contains("(") && eventInfo.changeTime.EndsWith(")"))
                    {
                        //Event execution time is not supported
                        eventInfo.changeTime = eventInfo.changeTime.Split('(')[0];
                    }
                    split = AdjustEventText(split[0]).Split(' ');
                    if (eventType == ConvertEventType.Particle) split[0] = ConvertEventProperty(ParticleKeywordMap, split[0]);
                    else if (eventType == ConvertEventType.CoverParticle) split[0] = ConvertEventProperty(CoverParticleKeywordMap, split[0]);
                    eventInfo.resultProperty = ConvertKeyword(split[0]);
                    if (eventInfo.resultProperty == TypeKeyword)
                    {
                        var rand = split[2].Split('+');
                        if (rand.Length >= 2) split[2] = $"{rand[0]}+{{{rand[1]}}}";
                        eventInfo.isSpecialEvent = true;
                        switch (ConvertKeyword(split[1]))
                        {
                            case "ChangeTo":
                                eventInfo.specialEvent = "ChangeType";
                                eventInfo.arguments = $"{split[2]}";
                                break;
                            case "Increase":
                                eventInfo.specialEvent = "IncreaseType";
                                eventInfo.arguments = split[2];
                                break;
                            case "Decrease":
                                eventInfo.specialEvent = "DecreaseType";
                                eventInfo.arguments = split[2];
                                break;
                        }
                    }
                    else
                    {
                        eventInfo.changeType = ConvertKeyword(split[1]);
                        if (eventInfo.resultProperty == BlendKeyMap.Key)
                        {
                            eventInfo.resultProperty = BlendKeyMap.Value;
                            eventInfo.resultType = PropertyType.Enum;
                            eventInfo.resultValue = BlendValueMap[split[2]];
                        }
                        else
                        {
                            eventInfo.resultType = PropertyTypeRule.GetValueType(type, subType, eventInfo.resultProperty);
                            eventInfo.resultValue = ConvertSpecialValue(eventInfo.resultType, ConvertKeyword(split[2]), out eventInfo.isExpressionResult);
                        }
                    }
                }
                var eventText = EventHelper.BuildEvent(eventInfo, !eventInfo.isSpecialEvent);
                group.Events.Add(eventText);
            }
            return group;
        }
        public static List<EventGroup> GetEventGroups(Type type, Type subType, string str, ConvertEventType eventType, string defaultName)
        {
            var split = str.Split('&');
            var groups = new List<EventGroup>();
            foreach (var groupStr in split)
            {
                if (string.IsNullOrEmpty(groupStr)) continue;
                groups.Add(GetEventGroup(type, subType, groupStr, eventType, defaultName));
            }
            return groups;
        }
    }
}
