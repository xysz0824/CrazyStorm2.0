/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Expression;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public class File : IXmlData, IGeneratePlayData, ILoadPlayData
    {
        #region Private Members
        IList<ParticleSystem> particleSystems;
        IList<FileResource> images;
        IList<FileResource> sounds;
        IList<VariableResource> globals;
        [XmlAttribute]
        int fileResourceIndex;
        [XmlAttribute]
        int particleIndex;
        #endregion

        #region Public Members
        public static string CurrentDirectory = string.Empty;
        public IList<ParticleSystem> ParticleSystems { get { return particleSystems; } }
        public IList<FileResource> Images { get { return images; } }
        public IList<FileResource> Sounds { get { return sounds; } }
        public IList<VariableResource> Globals { get { return globals; } }
        public Vector2 BodyPosition
        {
            set
            {
                foreach (var particleSystem in ParticleSystems)
                {
                    foreach (var layer in particleSystem.Layers)
                    {
                        foreach (var component in layer.Components)
                            component.BodyPosition = value;
                    }
                }
            }
        }
        public int ParticleIndex { get { return particleIndex++; } }
        public int FileResourceIndex { get { return fileResourceIndex++; } }
        #endregion

        #region Constructor
        public File()
        {
            particleSystems = new List<ParticleSystem>();
            images = new GenericContainer<FileResource>();
            sounds = new GenericContainer<FileResource>();
            globals = new GenericContainer<VariableResource>();
        }
        public File(string defaultParticleSystemName, string defaultLayerName, string defaultBodyPositionName) : this()
        {
            particleSystems.Add(new ParticleSystem(defaultParticleSystemName, defaultLayerName));
        }
        #endregion

        #region Public Methods
        public void UpdateResource()
        {
            foreach (var item in images)
                item.CheckValid();

            foreach (var item in sounds)
                item.CheckValid();

            foreach (var item in globals)
                item.CheckValid();
        }
        public object Clone()
        {
            throw new NotImplementedException();
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            XmlHelper.BuildFromFields(this, node);
            //particleSystems
            XmlHelper.BuildFromObjectList(particleSystems, new ParticleSystem(""), node, "ParticleSystems");
            //images
            XmlHelper.BuildFromObjectList(images, new FileResource(0, "", ""), node, "Images");
            //sounds
            XmlHelper.BuildFromObjectList(sounds, new FileResource(0, "", ""), node, "Sounds");
            //globals
            XmlHelper.BuildFromObjectList(globals, new VariableResource(""), node, "Globals");
            return node;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            XmlHelper.StoreFields(this, doc, node);
            //particleSystems
            XmlHelper.StoreObjectList(particleSystems, doc, node, "ParticleSystems");
            //images
            XmlHelper.StoreObjectList(images, doc, node, "Images");
            //sounds
            XmlHelper.StoreObjectList(sounds, doc, node, "Sounds");
            //globals
            XmlHelper.StoreObjectList(globals, doc, node, "Globals");            
            return node;
        }
        public static bool CheckVersion(string filePath)
        {
            var doc = new XmlDocument();
            doc.Load(filePath);
            var root = (XmlElement)doc.SelectSingleNode(VersionInfo.AppName.Replace(" ", ""));
            if (root == null) throw new XmlException();
            else
            {
                if (!root.HasAttribute("version"))
                {
                    throw new System.IO.FileLoadException("FileDataError");
                }
                string version = root.GetAttribute("version");
                return VersionInfo.PlayVersion == version;
            }
        }
        public void RebuildComponentTree(ParticleSystem particleSystem)
        {
            for (int i = 0; i < particleSystem.Layers.Count; ++i)
            {
                particleSystem.AddLayer(particleSystem.Layers[0]);
                particleSystem.Layers.RemoveAt(0);
            }
        }
        public void RebuildComponentTree()
        {
            foreach (var particleSystem in ParticleSystems)
                RebuildComponentTree(particleSystem);
        }
        public void RebuildObjectReference()
        {
            foreach (var particleSystem in ParticleSystems)
            {
                //Rebuild all custom types
                foreach (var customType in particleSystem.CustomTypes)
                {
                    customType.RebuildImageReferenceFromCollection(Images);
                }
                //Collect all particle types
                var particleTypes = new List<ParticleType>();
                particleTypes.AddRange(ParticleType.DefaultTypes);
                particleTypes.AddRange(particleSystem.CustomTypes);
                //Collect all components
                var components = new List<Core.Component>();
                foreach (var layer in particleSystem.Layers)
                    components.AddRange(layer.Components);
                //Rebuild components reference
                foreach (var component in components)
                {
                    component.RebuildReferenceFromCollection(components);
                    //Rebuild particles reference
                    if (component is Emitter)
                        (component as Emitter).Particle.RebuildReferenceFromCollection(particleTypes);
                }
            }
        }
        public void Load(string filePath)
            {
                var doc = new XmlDocument();
                doc.Load(filePath);
                var root = (XmlElement)doc.SelectSingleNode(VersionInfo.AppName.Replace(" ", ""));
                if (root == null) throw new XmlException();
                else
                {
                    BuildFromXml(root);
                    RebuildObjectReference();
                    RebuildComponentTree();
                }
            }
        public void Save(string filePath)
        {
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declaration);
            var root = doc.CreateElement(VersionInfo.AppName.Replace(" ", ""));
            var version = doc.CreateAttribute("version");
            version.Value = VersionInfo.PlayVersion;
            root.Attributes.Append(version);
            StoreAsXml(doc, root);
            doc.AppendChild(root);
            doc.Save(filePath);
        }
        void CompilePropertyExpressions(PropertyContainer container)
        {
            if (container is Emitter) CompilePropertyExpressions((container as Emitter).Particle);
            Type containerType = container.GetType();
            foreach (var property in container.Properties)
            {
                if (property.Value.Expression)
                {
                    var lexer = new Expression.Lexer();
                    lexer.Load(property.Value.Value);
                    var syntaxTree = new Expression.Parser(lexer).Expression();
                    if (!SyntaxTree.CanEval(syntaxTree))
                    {
                        var compiledBytes = new List<byte>();
                        syntaxTree.Compile(compiledBytes);
                        property.Value.CompiledExpression = compiledBytes.ToArray();
                    }
                    else
                    {
                        var value = syntaxTree.Eval(null);
                        var propertyInfo = containerType.GetProperty(property.Key);
                        //Casting
                        if (propertyInfo.PropertyType == typeof(int) && value is float)
                        {
                            value = (int)(float)value;
                        }
                        propertyInfo.GetSetMethod().Invoke(container, new object[] { value });
                        property.Value.Expression = false;
                    }
                }
            }
        }
        void CompileEventGroups(Component component)
        {
            CompileEvents(component.ComponentEventGroups);
            if (component is Emitter)
                CompileEvents((component as Emitter).ParticleEventGroups);
            else if (component is EventField)
                CompileEvents((component as EventField).EventFieldEventGroups);
            else if (component is Rebounder)
                CompileEvents((component as Rebounder).RebounderEventGroups);
        }
        void CompileEvents(IList<EventGroup> eventGroups)
        {
            foreach (EventGroup eventGroup in eventGroups)
            {
                eventGroup.CompiledCondition = null;
                if (!string.IsNullOrEmpty(eventGroup.Condition))
                {
                    var lexer = new Expression.Lexer();
                    lexer.Load(eventGroup.Condition);
                    var syntaxTree = new Expression.Parser(lexer).Expression();
                    var compiledBytes = new List<byte>();
                    syntaxTree.Compile(compiledBytes);
                    eventGroup.CompiledCondition = compiledBytes.ToArray();
                }
                eventGroup.CompiledEvents.Clear();
                foreach (string originalEvent in eventGroup.Events)
                {
                    eventGroup.CompiledEvents.Add(EventHelper.GenerateEventData(originalEvent));
                }
            }
        }
        void Compile()
        {
            foreach (var particleSystem in ParticleSystems)
            {
                foreach (var layer in particleSystem.Layers)
                {
                    foreach (var component in layer.Components)
                    {
                        CompilePropertyExpressions(component);
                        CompileEventGroups(component);
                    }
                }
            }
        }
        public List<byte> GeneratePlayData()
        {
            var fileBytes = new List<byte>();
            //particleSystems
            PlayDataHelper.GenerateObjectList(particleSystems, fileBytes);
            //images
            PlayDataHelper.GenerateObjectList(images, fileBytes);
            //sounds
            PlayDataHelper.GenerateObjectList(sounds, fileBytes);
            //globals
            PlayDataHelper.GenerateObjectList(globals, fileBytes);
            return fileBytes;
        }
        public byte[] GeneratePlayFile()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            //Play file use UTF-8 encoding
            //Write play file header
            writer.Write(PlayDataHelper.GetStringBytes("BG"));
            //Write play file version
            writer.Write(PlayDataHelper.GetStringBytes(VersionInfo.PlayVersion));
            //Write play file data
            Compile();
            writer.Write(GeneratePlayData().ToArray());
            var bytes = stream.ToArray();
            stream.Close();
            return bytes;
        }
        public void GeneratePlayFile(string filePath, string fileName)
        {
            string genPath = Path.GetDirectoryName(filePath) + "\\" + fileName + ".bg";
            using (FileStream stream = new FileStream(genPath, FileMode.Create))
            {
                var writer = new BinaryWriter(stream);
                writer.Write(GeneratePlayFile());
            }
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            //ParticleSystems
            PlayDataHelper.ReadObjectList(ParticleSystems, reader, version);
            //Images
            PlayDataHelper.ReadObjectList(Images, reader, version);
            //Sounds
            PlayDataHelper.ReadObjectList(Sounds, reader, version);
            //Globals
            globals = new List<VariableResource>();
            PlayDataHelper.ReadObjectList(globals, reader, version);
            foreach (var particleSystem in ParticleSystems)
            {
                foreach (var layer in particleSystem.Layers)
                {
                    foreach (var component in layer.Components)
                        component.Globals = globals;
                }
            }
        }
        void RebuildObjectReference(File file)
        {
            foreach (var particleSystem in file.ParticleSystems)
            {
                //Rebuild all custom types
                foreach (var customType in particleSystem.CustomTypes)
                {
                    customType.RebuildImageReferenceFromCollection(file.Images);
                }
                //Collect all particle types
                var particleTypes = new List<ParticleType>();
                ParticleType.LoadDefaultTypes();
                particleTypes.AddRange(ParticleType.DefaultTypes);
                particleTypes.AddRange(particleSystem.CustomTypes);
                //Collect all components
                var components = new List<Component>();
                foreach (var layer in particleSystem.Layers)
                    components.AddRange(layer.Components);
                //Rebuild components reference
                foreach (var component in components)
                {
                    component.RebuildReferenceFromCollection(components);
                    //Rebuild particles reference
                    if (component is Emitter)
                        (component as Emitter).InitialTemplate.RebuildReferenceFromCollection(particleTypes);
                }
            }
        }
        public bool LoadPlayFile(byte[] bytes, float baseVersion)
        {
            var stream = new MemoryStream(bytes);
            var reader = new BinaryReader(stream);
            //Play file use UTF-8 encoding
            string header = PlayDataHelper.ReadString(reader);
            if (header == "BG")
            {
                float version = float.Parse(PlayDataHelper.ReadString(reader));
                if (version >= baseVersion)
                {
                    LoadPlayData(reader, version);
                    RebuildObjectReference(this);
                    RebuildComponentTree();
                    return true;
                }
            }
            return false;
        }
        public bool LoadPlayFile(string filePath, float baseVersion)
        {
            return LoadPlayFile(System.IO.File.ReadAllBytes(filePath), baseVersion);
        }
        #endregion
    }
}
