/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CrazyStorm.Core
{
    public partial class File
    {
        static readonly Regex RenderingOrderMatch = new Regex(@"RenderingOrder:(\d+)", RegexOptions.Compiled);
        static readonly Regex ExternalMatch = new Regex(@"(?<!\w)(?:[A-Za-z]:\\|\.{1,2}\\)?(?:[^\\/:*?""<>|\r\n]+\\)*[^\\/:*?""<>|\r\n]+\.(png|dat)", RegexOptions.Compiled);
        static readonly Regex TypeCountMatch = new Regex(@"(\d+) Types:", RegexOptions.Compiled);
        static readonly Regex TypeMatch = new Regex(
            @"^(?<name>[^_]+)_" + 
            @"(?<texid>[^_]+)_" +
            @"(?<rectx>[^_]+)_(?<recty>[^_]+)_(?<rectwidth>[^_]+)_(?<rectheight>[^_]+)_" +
            @"(?<ox>[^_]+)_(?<oy>[^_]+)_" +
            @"(?<pdr>[^_]+)_" +
            @"(?<color>[^_]+)" +
            @"(?:_(?<frames>[^_]+))?(?:_(?<interval>[^_]+))?" +
            @"(?:_(?<volumex>[^_]+))?(?:_(?<volumey>[^_]+))?(?:_(?<volumewidth>[^_]+))?(?:_(?<volumeheight>[^_]+))?(?:_(?<volumejudgearea>[^_]+))?$", RegexOptions.Compiled);
        static readonly Regex GlobalEventCountMatch = new Regex(@"(\d+) GlobalEvents:", RegexOptions.Compiled);
        static readonly Regex GlobalEventMatch = new Regex(
            @"^(?<frame>[^_]+)_" +
            @"(?<gotocondition>[^_]+)_(?<gotooperator>[^_]+)_(?<gotovalue>[^_]+)_(?<isgoto>[^_]+)_(?<gototime>[^_]+)_(?<gotowhere>[^_]+)_" +
            @"(?<quakecondition>[^_]+)_(?<quakeoperator>[^_]+)_(?<quakevalue>[^_]+)_(?<isquake>[^_]+)_(?<quaketime>[^_]+)_(?<quakelevel>[^_]+)_" +
            @"(?<stopcondition>[^_]+)_(?<stopoperator>[^_]+)_(?<stopvalue>[^_]+)_(?<isstop>[^_]+)_(?<stoptime>[^_]+)_(?<stoplevel>[^_]+)$", RegexOptions.Compiled);
        static readonly Regex SoundCountMatch = new Regex(@"(\d+) Sounds:", RegexOptions.Compiled);
        static readonly Regex SoundMatch = new Regex(
            @"^(?<typeid>[^_]+)_(?<path>[^_]+)_(?<volume>[^_]+)$", RegexOptions.Compiled);
        static readonly Regex CenterMatch = new Regex(@"^Center:(?:False,(?<events>.*)|" +
            @"(?<x>[^,]+),(?<y>[^,]+)" +
            @"(?:,(?<speed>[^,]+))?(?:,(?<speedd>[^,]+))?(?:,(?<aspeed>[^,]+))?(?:,(?<aspeedd>[^,]+))?,(?<events>.*))$", RegexOptions.Compiled);
        static readonly Vector2 OldCenter = new Vector2(480, 360);
        static readonly Dictionary<string, string> Words = new Dictionary<string, string>()
        {
            { "当前帧", "CurrentFrame" }, { "且", " & " }, { "或", " | "},
            { "额外发射", "EmitParticle" }, { "恢复", "Recover"},
            { "变化到", " ChangeTo " }, { "增加", " Increase " }, { "减少", " Decrease " },
            { "正比", "Linear" }, { "固定", "Instant"}, { "正弦", "Sin"}, { "无缝正弦", "Sin"},
            { "加速度方向", "AcspeedAngle" }, { "速度方向", "SpeedAngle" }, { "加速度", "Acspeed" }, { "速度", "Speed" },
            { "跟随自机X慢", "FollowBodyXSlow" }, { "跟随自机X", "FollowBodyX" }, { "跟随自机Y慢", "FollowBodyYSlow" }, { "跟随自机Y", "FollowBodyY" },
            { "跟随自机慢", "FollowBodySlow" }, { "跟随自机", "FollowBody" },
            { "范围移动慢", "RandomMoveSlow" }, { "范围移动", "RandomMove" }, { "范围飘动", "RandomWalk" },
        };
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
                if (center != null)
                {
                    center.Visibility = match.Groups["x"].Success;
                    center.ComponentEventGroups.Add(globalEvents);
                    var eventgroup = GetEventGroup(typeof(Center), match.Groups["events"].Value);
                    if (eventgroup.Events.Count > 0)
                    {
                        eventgroup.Name = "CenterEvents";
                        center.ComponentEventGroups.Add(eventgroup);
                    }
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
                ParticleSystems.Add(particleSystem);
            }
        }
        public static string ConvertEvent(string str)
        {
            foreach (var word in Words)
            {
                if (str.Contains(word.Key))
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
                    if (type == PropertyType.Single) return "0";
                    else if (type == PropertyType.Vector2)
                    {
                        expressionResult = true;
                        return "Position";
                    }
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
            return value;
        }
        public static EventGroup GetEventGroup(Type componentType, string str)
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
                var condition = ConvertEvent(split[0]);
                var content = ConvertEvent(split[1]);
                var eventInfo = new EventInfo();
                eventInfo.condition = condition;
                if (EventHelper.IsSpecialEvent(content))
                {
                    eventInfo.isSpecialEvent = true;
                    split = content.Split('(');
                    eventInfo.specialEvent = split[0];
                    eventInfo.arguments = content.Replace($"{eventInfo.specialEvent}(", "").Replace(")", "")
                        .Replace($"{eventInfo.specialEvent}，", "").Replace(eventInfo.specialEvent, "").Replace("，", ",");
                }
                else
                {
                    split = content.Split('，');
                    eventInfo.changeMode = split[1];
                    eventInfo.changeTime = split[2].Replace("帧", "");
                    split = split[0].Split(' ');
                    eventInfo.resultProperty = split[0];
                    eventInfo.resultType = PropertyTypeRule.GetValueType(componentType, eventInfo.resultProperty);
                    eventInfo.changeType = split[1];
                    eventInfo.resultValue = ConvertSpecialValue(eventInfo.resultType, split[2], out eventInfo.isExpressionResult);
                }
                group.Events.Add(EventHelper.BuildEvent(eventInfo, eventInfo.isSpecialEvent));
            }
            return group;
        }
        public static List<EventGroup> GetEventGroups(Type componentType, string str)
        {
            var split = str.Split('&');
            var groups = new List<EventGroup>();
            foreach (var groupStr in split)
            {
                if (string.IsNullOrEmpty(groupStr)) continue;
                groups.Add(GetEventGroup(componentType, groupStr));
            }
            return groups;
        }
    }
}
