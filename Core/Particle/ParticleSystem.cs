/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public struct ShakeScreenEvent
    {
        public float duration;
        public float level;
        public float frame;
    }
    public struct ScaleFrameEvent
    {
        public float duration;
        public float level;
        public float frame;
    }
    public enum OrderType
    {
        FirstAsTop,
        LastAsTop
    }
    public class ParticleSystem : PoolObject<ParticleSystem, NullData>, IXmlData, IGeneratePlayData, ILoadPlayData
    {
        public const float FRAME_RATE_BASE = 60;

        #region Private Members
        int instancedID;
        [StringData]
        [XmlAttribute]
        string name;
        [XmlAttribute]
        OrderType orderType;
        GenericContainer<MaskType> customMaskTypes;
        GenericContainer<ParticleType> customTypes;
        GenericContainer<Layer> layers;
        GenericContainer<Note> notes;
        GenericContainer<Component> componentTree;
        FileResource maskImage;
        int maskImageID = -1;
        [XmlAttribute]
        int customMaskTypeIndex;
        [XmlAttribute]
        int customTypeIndex;
        [XmlAttribute]
        int layerIndex;
        Dictionary<int, int> componentIndex;
        Dictionary<int, int> typeSoundMap;
        Vector2 screenOffset;
        ShakeScreenEvent shakeScreenEvent;
        ScaleFrameEvent scaleFrameEvent;
        #endregion

        #region Public Members
        public int InstancedID => instancedID;
        public string Name 
        { 
            get { return name; }
            set { name = value; }
        }
        public OrderType OrderType
        {
            get { return orderType; }
            set { orderType = value; }
        }
        public Vector2 LogicOffset { get; set; }
        public Vector2 ScreenOffset => LogicOffset + screenOffset;
        public float FrameFactor { get; private set; }
        public float CurrentFrame { get; private set; }
        public int FrameSkipCount { get; set; }
        public int TotalFrame
        {
            get
            {
                var totalFrame = 0;
                for (int i = 0; i < Layers.Count; ++i)
                {
                    //Set the biggest number as totalFrame 
                    totalFrame = Layers[i].TotalFrame > totalFrame ? Layers[i].TotalFrame : totalFrame;
                }
                return totalFrame;
            }
        }
        public GenericContainer<MaskType> CustomMaskTypes { get { return customMaskTypes; } }
        public GenericContainer<ParticleType> CustomTypes { get { return customTypes; } }
        public GenericContainer<Layer> Layers { get { return layers; } }
        public GenericContainer<Note> Notes { get { return notes; } }
        public GenericContainer<Component> ComponentTree { get { return componentTree; } }
        public FileResource MaskImage
        {
            get { return maskImage; }
            set
            {
                maskImage = value;
                maskImageID = value != null ? value.ID : -1;
            }
        }
        public int CustomMaskTypeIndex { get { return customMaskTypeIndex++; } }
        public int CustomTypeIndex { get { return customTypeIndex++; } }
        public int LayerIndex { get { return layerIndex++; } }
        public GenericContainer<FileResource> Sounds { get; set; }
        public Dictionary<int, int> TypeSoundMap { get { return typeSoundMap; } }
        public int Status { get; private set; }
        public float StatusFrame { get; private set; }
        public Vector2 BodyPosition 
        {
            set
            {
                for (int j = 0; j < Layers.Count; ++j)
                {
                    var layer = Layers[j];
                    for (int k = 0; k < layer.Components.Count; ++k)
                    {
                        var component = layer.Components[k];
                        component.BodyPosition = value;
                    }
                }
            }
        }
        public Vector2 CenterPosition { get; private set; }
        #endregion

        #region Constructor
        public ParticleSystem()
        {
            customMaskTypes = new GenericContainer<MaskType>();
            customTypes = new GenericContainer<ParticleType>();
            layers = new GenericContainer<Layer>();
            notes = new GenericContainer<Note>();
            componentTree = new GenericContainer<Component>();
        }
        public ParticleSystem(string name) : this()
        {
            this.name = name;
            componentIndex = new Dictionary<int, int>();
            typeSoundMap = new Dictionary<int, int>();
        }
        public ParticleSystem(string name, string defaultLayerName) : this(name)
        {
            layers.Add(new Layer(defaultLayerName));
        }
        #endregion

        #region Private Methods
        float ClampTargetFrame(float targetFrame)
        {
            int totalFrame = Math.Max(1, TotalFrame);
            int normalizedFrame = (int)targetFrame;
            if (normalizedFrame < 1) return 1;
            if (normalizedFrame > totalFrame) return totalFrame;
            return normalizedFrame;
        }
        bool StepUpdate(float frameRate, bool resetWhenFinished)
        {
            var frameScale = FRAME_RATE_BASE / frameRate;
            UpdateGlobalEvents(frameScale);
            StatusFrame += frameScale;
            CenterPosition = GetCenterPositionRuntime();
            for (int i = 0; i < ComponentTree.Count; ++i)
            {
                ComponentTree[i].Status = Status;
                ComponentTree[i].StatusFrame = StatusFrame;
                ComponentTree[i].CenterPosition = CenterPosition;
                UpdateComponent(ComponentTree[i], FrameFactor * frameScale, CurrentFrame);
            }
            CurrentFrame += frameScale;
            if (CurrentFrame > Math.Max(1, TotalFrame) && resetWhenFinished) Reset(false);
            return true;
        }
        #endregion

        #region Public Methods
        public int GetComponentIndex()
        {
            int index = 0;
            foreach (var pair in componentIndex) index += pair.Value;
            return index;
        }
        public int GetAndIncreaseComponentIndex(string componentType)
        {
            var typeHash = StringUtil.StableHash32_Fnv1a(componentType);
            if (!componentIndex.ContainsKey(typeHash))
                componentIndex[typeHash] = 0;

            return componentIndex[typeHash]++;
        }
        public void AddComponentToLayer(Layer layer, Component component)
        {
            if (component.Parent == null)
                componentTree.Add(component);
            else
                component.Parent.Children.Add(component);

            foreach (var item in component.GetPosterity())
                layer.Components.Add(item);

            layer.Components.Add(component);
        }
        public void DeleteComponentFromLayer(Layer layer, Component component)
        {
            if (component.Parent == null)
                componentTree.Remove(component);
            else
                component.Parent.Children.Remove(component);

            foreach (var item in component.GetPosterity())
                layer.Components.Remove(item);


            foreach (var layerItem in layers)
            {
                foreach (var componentItem in layerItem.Components)
                {
                    if (componentItem.BindingTarget == component)
                        componentItem.BindingTarget = null;
                }
            }
            layer.Components.Remove(component);
        }
        public void AddLayer(Layer layer)
        {
            foreach (var component in layer.Components)
            {
                if (component.Parent == null)
                    componentTree.Add(component);
            }
            layers.Add(layer);
        }
        public void InsertLayer(Layer layer)
        {
            foreach (var component in layer.Components)
            {
                if (component.Parent == null)
                    componentTree.Add(component);
            }
            layers.Insert(0, layer);
        }
        public void DeleteLayer(Layer layer)
        {
            foreach (var component in layer.Components)
            {
                if (component.Parent == null)
                    componentTree.Remove(component);
            }
            layers.Remove(layer);
        }
        public void RebuildComponentTree()
        {
            if (componentTree == null) componentTree = new GenericContainer<Component>();
            else componentTree.Clear();
            for (int i = 0; i < Layers.Count; ++i)
            {
                AddLayer(Layers[0]);
                Layers.RemoveAt(0);
            }
        }
        public object Clone()
        {
            var clone = MemberwiseClone() as ParticleSystem;
            clone.customMaskTypes = new GenericContainer<MaskType>();
            foreach (var type in customMaskTypes) clone.customMaskTypes.Add(type.Clone() as MaskType);
            clone.customTypes = new GenericContainer<ParticleType>();
            foreach (var type in customTypes) clone.customTypes.Add(type.Clone() as ParticleType);
            clone.layers = new GenericContainer<Layer>();
            foreach (var layer in layers) clone.layers.Add(layer.Clone() as Layer);
            clone.notes = new GenericContainer<Note>();
            foreach (var note in notes) clone.notes.Add(note.Clone() as Note);
            clone.componentTree = new GenericContainer<Component>();
            clone.componentIndex = new Dictionary<int, int>();
            foreach (var kv in componentIndex) clone.componentIndex[kv.Key] = kv.Value;
            clone.typeSoundMap = new Dictionary<int, int>();
            foreach (var kv in typeSoundMap) clone.typeSoundMap[kv.Key] = kv.Value;
            return clone;
        }
        public ParticleSystem Instantiate()
        {
            var instance = Rent(NullData.Empty);
            instance.instancedID = InstancedID + 1;
            instance.name = name;
            instance.orderType = orderType;
            instance.LogicOffset = default;
            instance.customMaskTypes = customMaskTypes;
            instance.MaskImage = maskImage;
            instance.customTypes = customTypes;
            instance.layers.Clear();
            for (int i = 0; i < layers.Count; ++i)
            {
                var layer = layers[i].Instantiate();
                instance.layers.Add(layer);
            }
            foreach (var layer in instance.layers)
            {
                foreach (var component in layer.Components)
                {
                    component.System = instance;
                    component.RebuildReferenceFromCollection();
                }
            }
            instance.RebuildMaskTypeReferences();
            instance.RebuildComponentTree();
            instance.Sounds = Sounds;
            instance.typeSoundMap = typeSoundMap;
            instance.Status = 0;
            instance.StatusFrame = 0;
            return instance;
        }
        public void Destroy()
        {
            foreach (var layer in layers) layer.Destroy();
            Return(this);
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            var nodeName = "ParticleSystem";
            var particleSystemNode = (XmlElement)node.SelectSingleNode(nodeName);
            if (node.Name == nodeName) particleSystemNode = node;
            XmlHelper.BuildFromFields(this, particleSystemNode);
            if (particleSystemNode.HasAttribute("maskImage"))
            {
                int parsedID;
                if (int.TryParse(particleSystemNode.GetAttribute("maskImage"), out parsedID)) maskImageID = parsedID;
                else throw new FileLoadException("FileDataError");
            }
            else maskImageID = -1;
            maskImage = null;
            XmlHelper.BuildFromObjectList(customMaskTypes, new MaskType(0), particleSystemNode, "CustomMaskTypes");
            //customTypes
            XmlHelper.BuildFromObjectList(customTypes, new ParticleType(0), particleSystemNode, "CustomTypes");
            //layers
            XmlHelper.BuildFromObjectList(layers, new Layer(""), particleSystemNode, "Layers");
            //notes
            XmlHelper.BuildFromObjectList(notes, new Note(), particleSystemNode, "Notes");
            //componentIndex
            XmlHelper.BuildFromDictionary(componentIndex, particleSystemNode, "ComponentIndex");
            //typeSoundMap
            XmlHelper.BuildFromDictionary(typeSoundMap, particleSystemNode, "TypeSoundMap");
            return particleSystemNode;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            var particleSystemNode = doc.CreateElement("ParticleSystem");
            XmlHelper.StoreFields(this, doc, particleSystemNode);
            if (maskImage != null)
            {
                var maskImageAttribute = doc.CreateAttribute("maskImage");
                maskImageAttribute.Value = maskImage.ID.ToString();
                particleSystemNode.Attributes.Append(maskImageAttribute);
            }
            XmlHelper.StoreObjectList(customMaskTypes, doc, particleSystemNode, "CustomMaskTypes");
            //customTypes
            XmlHelper.StoreObjectList(customTypes, doc, particleSystemNode, "CustomTypes");
            //layers
            XmlHelper.StoreObjectList(layers, doc, particleSystemNode, "Layers");
            //notes
            XmlHelper.StoreObjectList(notes, doc, particleSystemNode, "Notes");
            //componentIndex
            XmlHelper.StoreDictionary(componentIndex, doc, particleSystemNode, "ComponentIndex");
            //typeSoundMap
            XmlHelper.StoreDictionary(typeSoundMap, doc, particleSystemNode, "TypeSoundMap");
            node.AppendChild(particleSystemNode);
            return particleSystemNode;
        }
        public List<byte> GeneratePlayData(File file)
        {
            var particleSystemBytes = new List<byte>();
            //orderType
            PlayDataHelper.GenerateStruct(orderType, particleSystemBytes);
            //stringDataField
            PlayDataHelper.GenerateStringDataFields(this, particleSystemBytes);
            particleSystemBytes.AddRange(BitConverter.GetBytes(maskImage != null ? maskImage.ID : -1));
            PlayDataHelper.GenerateObjectList(file, customMaskTypes, particleSystemBytes);
            //customTypes
            PlayDataHelper.GenerateObjectList(file, customTypes, particleSystemBytes);
            //layers
            PlayDataHelper.GenerateObjectList(file, layers, particleSystemBytes);
            //typeSoundMap
            particleSystemBytes.AddRange(BitConverter.GetBytes(typeSoundMap.Count));
            foreach (var typeSound in typeSoundMap)
            {
                particleSystemBytes.AddRange(BitConverter.GetBytes(typeSound.Key));
                particleSystemBytes.AddRange(BitConverter.GetBytes(typeSound.Value));
            }
            return PlayDataHelper.CreateBlock(particleSystemBytes);
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            using (BinaryReader particleSystemReader = PlayDataHelper.GetBlockReader(reader))
            {
                //orderType
                orderType = PlayDataHelper.ReadStruct<OrderType>(particleSystemReader);
                //stringDataFields
                PlayDataHelper.ReadStringDataFields(this, particleSystemReader);
                maskImageID = particleSystemReader.ReadInt32();
                PlayDataHelper.ReadObjectList(CustomMaskTypes, particleSystemReader, version);
                //customTypes
                PlayDataHelper.ReadObjectList(CustomTypes, particleSystemReader, version);
                //layers
                PlayDataHelper.ReadObjectList(Layers, particleSystemReader, version);
                for (int i = 0; i < Layers.Count; ++i)
                {
                    //Set id of layer to components of layer
                    Layers[i].SetComponentsID(i);
                }
                //typeSoundMap
                typeSoundMap = new Dictionary<int, int>();
                var count = particleSystemReader.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    var key = particleSystemReader.ReadInt32();
                    var value = particleSystemReader.ReadInt32();
                    typeSoundMap.Add(key, value);
                }
            }
        }
        public void RebuildMaskImageReference(GenericContainer<FileResource> collection)
        {
            if (maskImageID == -1) return;
            foreach (var target in collection)
            {
                if (maskImageID == target.ID)
                {
                    MaskImage = target;
                    break;
                }
            }
            maskImageID = -1;
        }
        public void RebuildMaskTypeReferences()
        {
            foreach (var layer in Layers)
            {
                foreach (var component in layer.Components)
                {
                    var eventField = component as EventField;
                    if (eventField != null)
                    {
                        eventField.RebuildMaskTypeReference(CustomMaskTypes);
                    }
                }
            }
        }
        Vector2 GetCenterPositionRuntime()
        {
            for (int i = 0; i < Layers.Count; ++i)
            {
                var layer = Layers[i];
                for (int k = 0; k < layer.Components.Count; ++k)
                {
                    if (layer.Components[k] is Center && layer.Components[k].Name == File.DefaultCenterName)
                    {
                        return layer.Components[k].Position;
                    }
                }
            }
            return Vector2.Zero;
        }
        void UpdateGlobalEvents(float frameScale)
        {
            if (shakeScreenEvent.frame < shakeScreenEvent.duration)
            {
                shakeScreenEvent.frame = Math.Min(shakeScreenEvent.frame + frameScale, shakeScreenEvent.duration);
                if (shakeScreenEvent.frame == shakeScreenEvent.duration) screenOffset = new Vector2(0, 0);
                else
                {
                    float t = shakeScreenEvent.frame / shakeScreenEvent.duration;
                    if (MathHelper.FrameMod(shakeScreenEvent.frame, frameScale, 2))
                    {
                        screenOffset = new Vector2(0, ((1f - t) * shakeScreenEvent.level * (float)Math.Sin(shakeScreenEvent.frame)));
                    }
                }
            }
            if (scaleFrameEvent.frame < scaleFrameEvent.duration)
            {
                scaleFrameEvent.frame = Math.Min(scaleFrameEvent.frame + frameScale, scaleFrameEvent.duration);
                if (scaleFrameEvent.frame == scaleFrameEvent.duration) FrameFactor = 1;
                else if (scaleFrameEvent.level >= 1) FrameFactor = 0;
                else
                {
                    float t = scaleFrameEvent.frame / scaleFrameEvent.duration;
                    double percent = MathHelper.Lerp(0.5f, 1f, Math.Max(0, scaleFrameEvent.level));
                    double fadeTime = 0.8d;
                    FrameFactor = 1.0f - (float)(percent * (1.0d - Math.Pow(Math.Max(0, 1d / (1d - fadeTime) * (t - fadeTime)), 3)));
                }
            }
        }
        public bool Update(float frameRate) => StepUpdate(frameRate, true);
        public void SkipFrame(float targetFrame, bool replayFromStart, float frameRate = FRAME_RATE_BASE)
        {
            var normalizedFrame = ClampTargetFrame(targetFrame);
            if (!replayFromStart)
            {
                CurrentFrame = normalizedFrame;
                return;
            }

            Reset(true);
            EventManager.SkipFrame(this, true, frameRate);
            ParticleManager.SkipFrame(this, true, frameRate);
            while (CurrentFrame < normalizedFrame)
            {
                EventManager.SkipFrame(this, false, frameRate);
                StepUpdate(frameRate, false);
                ParticleManager.SkipFrame(this, false, frameRate);
            }
            CurrentFrame = normalizedFrame;
        }
        public void UpdateComponent(Component component, float frameScale, float currentFrame)
        {
            var layer = Layers[component.LayerID];
            if (layer.NeedUpdate(currentFrame) || component.BindingTarget != null)
            {
                component.Update(frameScale, currentFrame);
            }
            for (int i = 0; i < component.Children.Count; ++i)
            {
                component.Children[i].Status = Status;
                component.Children[i].StatusFrame = StatusFrame;
                component.Children[i].CenterPosition = CenterPosition;
                UpdateComponent(component.Children[i], frameScale, currentFrame);
            }
        }
        public void Reset(bool includeGlobalEvents)
        {
            CurrentFrame = 1;
            if (includeGlobalEvents)
            {
                StatusFrame = 0;
                FrameSkipCount = 0;
                screenOffset = new Vector2(0, 0);
                shakeScreenEvent = default;
                FrameFactor = 1;
                scaleFrameEvent = default;
            }
            for (int i = 0; i < Layers.Count; ++i) Layers[i].Reset();
        }
        public void SetStatus(int i)
        {
            if (Status == i) return;
            Status = i;
            StatusFrame = 0;
        }
        public void ShakeScreen(float duration, float level)
        {
            shakeScreenEvent = new ShakeScreenEvent { duration = duration, level = level, frame = 0 };
        }
        public void ScaleFrame(float duration, float level)
        {
            scaleFrameEvent = new ScaleFrameEvent { duration = duration, level = level, frame = 0 };
        }
        #endregion
    }
}
